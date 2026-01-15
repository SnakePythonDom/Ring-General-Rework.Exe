using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Import;
using RingGeneral.Specs.Models.Import;

namespace RingGeneral.Tools.BakiImporter;

public static class BakiAttributeReportGenerator
{
    public static BakiAttributeReport Generate(
        SqliteConnection connection,
        BakiAttributeMappingSpec spec,
        BakiAttributeConverter converter)
    {
        var requiredColumns = CollectRequiredColumns(spec);
        var availableColumns = ChargerColonnes(connection, spec.SourceTable);
        var select = BuildSelect(spec.SourceTable, requiredColumns, availableColumns);
        var reportSpec = spec.Reporting ?? new BakiAttributeReportSpec("id", new[] { "name" }, 10);

        var quantileHistograms = BuildQuantileHistograms(spec);
        var sourceStats = BuildStats(
            spec,
            reportSpec.HistogramBins,
            attributeId => ResolveSourceScale(spec, attributeId));

        using (var command = connection.CreateCommand())
        {
            command.CommandText = select;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var fields = ReadFields(reader, requiredColumns);
                var context = new BakiWorkerContext(fields);
                UpdateSourceStats(sourceStats, spec, fields);
                UpdateQuantiles(quantileHistograms, spec, context);
            }
        }

        var quantileMaps = quantileHistograms.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.BuildMap(),
            StringComparer.OrdinalIgnoreCase);

        var convertedStats = BuildStats(
            spec,
            reportSpec.HistogramBins,
            _ => spec.TargetScale);
        var workerScores = new List<BakiWorkerScore>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = select;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var fields = ReadFields(reader, requiredColumns);
                var context = new BakiWorkerContext(fields);
                var converted = converter.ConvertWorker(context, quantileMaps);

                UpdateConvertedStats(convertedStats, converted);
                workerScores.Add(new BakiWorkerScore(
                    GetWorkerId(fields, reportSpec.WorkerIdField),
                    GetWorkerName(fields, reportSpec.DisplayNameFields),
                    converted.Values.Average()));
            }
        }

        var overboosted = workerScores
            .OrderByDescending(score => score.Average)
            .Take(20)
            .ToList();
        var tooWeak = workerScores
            .OrderBy(score => score.Average)
            .Take(20)
            .ToList();

        return new BakiAttributeReport(
            spec.SourceTable,
            new BakiAttributeReportSection(sourceStats),
            new BakiAttributeReportSection(convertedStats),
            overboosted,
            tooWeak);
    }

    private static Dictionary<string, AttributeStats> BuildStats(
        BakiAttributeMappingSpec spec,
        int bins,
        Func<string, BakiAttributeScale> scaleProvider)
    {
        var stats = new Dictionary<string, AttributeStats>(StringComparer.OrdinalIgnoreCase);
        foreach (var attribute in spec.Mappings.Keys)
        {
            var scale = scaleProvider(attribute);
            stats[attribute] = new AttributeStats(bins, scale.Min, scale.Max);
        }

        return stats;
    }

    private static BakiAttributeScale ResolveSourceScale(BakiAttributeMappingSpec spec, string attributeId)
    {
        if (spec.SourceScales.ByAttribute is not null
            && spec.SourceScales.ByAttribute.TryGetValue(attributeId, out var scale))
        {
            return scale;
        }

        return spec.SourceScales.Default;
    }

    private static Dictionary<string, BakiQuantileHistogram> BuildQuantileHistograms(BakiAttributeMappingSpec spec)
    {
        var histograms = new Dictionary<string, BakiQuantileHistogram>(StringComparer.OrdinalIgnoreCase);
        foreach (var (attributeId, mapping) in spec.Mappings)
        {
            if (!string.Equals(mapping.MappingMethod, "Quantile", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var scale = spec.SourceScales.Default;
            if (spec.SourceScales.ByAttribute is not null
                && spec.SourceScales.ByAttribute.TryGetValue(attributeId, out var overrideScale))
            {
                scale = overrideScale;
            }

            histograms[attributeId] = new BakiQuantileHistogram(200, scale.Min, scale.Max);
        }

        return histograms;
    }

    private static void UpdateSourceStats(
        Dictionary<string, AttributeStats> stats,
        BakiAttributeMappingSpec spec,
        IReadOnlyDictionary<string, object?> fields)
    {
        foreach (var (attributeId, mapping) in spec.Mappings)
        {
            var sourceField = string.IsNullOrWhiteSpace(mapping.SourceField)
                ? mapping.GroupSource
                : mapping.SourceField;

            if (string.IsNullOrWhiteSpace(sourceField))
            {
                continue;
            }

            if (!fields.TryGetValue(sourceField!, out var value) || value is null)
            {
                continue;
            }

            if (double.TryParse(value.ToString(), out var parsed))
            {
                stats[attributeId].Add(parsed);
            }
        }
    }

    private static void UpdateQuantiles(
        Dictionary<string, BakiQuantileHistogram> histograms,
        BakiAttributeMappingSpec spec,
        BakiWorkerContext context)
    {
        foreach (var (attributeId, mapping) in spec.Mappings)
        {
            if (!string.Equals(mapping.MappingMethod, "Quantile", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var sourceField = string.IsNullOrWhiteSpace(mapping.SourceField)
                ? mapping.GroupSource
                : mapping.SourceField;

            if (string.IsNullOrWhiteSpace(sourceField)
                || !context.TryGetNumber(sourceField, out var value))
            {
                continue;
            }

            if (histograms.TryGetValue(attributeId, out var histogram))
            {
                histogram.Add(value);
            }
        }
    }

    private static void UpdateConvertedStats(
        Dictionary<string, AttributeStats> stats,
        IReadOnlyDictionary<string, int> converted)
    {
        foreach (var (attribute, value) in converted)
        {
            if (stats.TryGetValue(attribute, out var stat))
            {
                stat.Add(value);
            }
        }
    }

    private static string GetWorkerId(IReadOnlyDictionary<string, object?> fields, string idField)
        => fields.TryGetValue(idField, out var value) ? value?.ToString() ?? string.Empty : string.Empty;

    private static string GetWorkerName(IReadOnlyDictionary<string, object?> fields, IReadOnlyList<string> nameFields)
    {
        var parts = new List<string>();
        foreach (var field in nameFields)
        {
            if (fields.TryGetValue(field, out var value) && value is not null)
            {
                var text = value.ToString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    parts.Add(text!);
                }
            }
        }

        return parts.Count > 0 ? string.Join(" ", parts) : string.Empty;
    }

    private static HashSet<string> ChargerColonnes(SqliteConnection connection, string table)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static IReadOnlyList<string> CollectRequiredColumns(BakiAttributeMappingSpec spec)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in spec.Mappings.Values)
        {
            if (!string.IsNullOrWhiteSpace(mapping.SourceField))
            {
                columns.Add(mapping.SourceField!);
            }

            if (!string.IsNullOrWhiteSpace(mapping.GroupSource))
            {
                columns.Add(mapping.GroupSource!);
            }
        }

        if (spec.ExperienceFactor is not null)
        {
            columns.Add(spec.ExperienceFactor.AgeField);
            columns.Add(spec.ExperienceFactor.ExperienceYearsField);
        }

        if (spec.Reporting is not null)
        {
            columns.Add(spec.Reporting.WorkerIdField);
            foreach (var field in spec.Reporting.DisplayNameFields)
            {
                columns.Add(field);
            }
        }

        if (spec.RoleCoherence is not null)
        {
            columns.Add(spec.RoleCoherence.RoleField);
        }

        return columns.ToList();
    }

    private static string BuildSelect(string table, IReadOnlyList<string> columns, HashSet<string> availableColumns)
    {
        var selectParts = columns.Select(column =>
            availableColumns.Contains(column)
                ? $"\"{column}\""
                : $"NULL AS \"{column}\"");

        return $"SELECT {string.Join(", ", selectParts)} FROM \"{table}\"";
    }

    private static Dictionary<string, object?> ReadFields(SqliteDataReader reader, IReadOnlyList<string> columns)
    {
        var fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < columns.Count; i++)
        {
            fields[columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }

        return fields;
    }

    public sealed record BakiAttributeReport(
        string SourceTable,
        BakiAttributeReportSection Source,
        BakiAttributeReportSection Converted,
        IReadOnlyList<BakiWorkerScore> Overboosted,
        IReadOnlyList<BakiWorkerScore> TooWeak);

    public sealed record BakiAttributeReportSection(
        IReadOnlyDictionary<string, AttributeStats> Attributes);

    public sealed record BakiWorkerScore(
        string WorkerId,
        string Nom,
        double Average);

    public sealed class AttributeStats
    {
        private readonly int _bins;
        private readonly double _min;
        private readonly double _max;
        private readonly int[] _histogram;
        private double _sum;
        private double _count;
        private double _minObserved = double.MaxValue;
        private double _maxObserved = double.MinValue;

        public AttributeStats(int bins, double min, double max)
        {
            _bins = bins;
            _min = min;
            _max = max;
            _histogram = new int[bins];
        }

        [JsonPropertyName("min")]
        public double Min => _count == 0 ? 0 : _minObserved;

        [JsonPropertyName("max")]
        public double Max => _count == 0 ? 0 : _maxObserved;

        [JsonPropertyName("average")]
        public double Average => _count == 0 ? 0 : _sum / _count;

        [JsonPropertyName("histogram")]
        public IReadOnlyList<int> Histogram => _histogram;

        public void Add(double value)
        {
            _sum += value;
            _count++;
            _minObserved = Math.Min(_minObserved, value);
            _maxObserved = Math.Max(_maxObserved, value);

            var clamped = Math.Min(Math.Max(value, _min), _max);
            var index = (int)Math.Floor((clamped - _min) / (_max - _min) * _bins);
            index = Math.Min(Math.Max(index, 0), _bins - 1);
            _histogram[index]++;
        }
    }
}
