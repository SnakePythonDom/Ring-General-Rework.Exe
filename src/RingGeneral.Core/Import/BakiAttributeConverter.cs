using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Random;
using RingGeneral.Specs.Models.Import;

namespace RingGeneral.Core.Import;

public sealed class BakiAttributeConverter
{
    private readonly BakiAttributeMappingSpec _spec;
    private readonly IRandomProvider _random;

    public BakiAttributeConverter(BakiAttributeMappingSpec spec, IRandomProvider? random = null)
    {
        _spec = spec;
        _random = random ?? new SeededRandomProvider(spec.RandomVariation?.Seed ?? 0);
    }

    public IReadOnlyDictionary<string, int> ConvertWorker(
        BakiWorkerContext context,
        IReadOnlyDictionary<string, BakiQuantileMap>? quantiles = null)
    {
        var results = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var derived = BakiDerivedValues.Create(_spec.ExperienceFactor, context);

        foreach (var (attributeId, mapping) in _spec.Mappings)
        {
            var rawValue = ResolveSourceValue(attributeId, context, mapping);
            if (rawValue is null)
            {
                missing.Add(attributeId);
                results[attributeId] = mapping.DefaultIfMissing;
                continue;
            }

            var mapped = MapValue(attributeId, mapping, rawValue.Value, quantiles);
            results[attributeId] = mapped;
        }

        foreach (var attributeId in missing)
        {
            var mapping = _spec.Mappings[attributeId];
            if (mapping.Fallback is null)
            {
                continue;
            }

            if (TryResolveFallback(mapping.Fallback, results, derived, out var fallbackValue))
            {
                results[attributeId] = fallbackValue;
            }
        }

        ApplyRandomVariation(results);

        if (_spec.RoleCoherence is { Enabled: true } coherence)
        {
            BakiAttributeNormalizer.ApplyRoleCoherence(results, coherence, context);
        }

        if (_spec.Normalization is { Enabled: true } normalization)
        {
            BakiAttributeNormalizer.Normalize(results, normalization);
            foreach (var key in results.Keys.ToList())
            {
                results[key] = BakiAttributeMappingMath.Clamp(
                    results[key],
                    _spec.TargetScale.Min,
                    _spec.TargetScale.Max);
            }
        }

        return results;
    }

    private double? ResolveSourceValue(string attributeId, BakiWorkerContext context, BakiAttributeMapping mapping)
    {
        if (!string.IsNullOrWhiteSpace(mapping.SourceField)
            && context.TryGetNumber(mapping.SourceField!, out var value))
        {
            return value;
        }

        if (!string.IsNullOrWhiteSpace(mapping.GroupSource)
            && context.TryGetNumber(mapping.GroupSource!, out var groupValue))
        {
            if (_spec.GroupWeights.Groups.TryGetValue(mapping.GroupSource!, out var group)
                && group.TryGetValue(attributeId, out var weight))
            {
                return groupValue * weight;
            }

            return groupValue;
        }

        return null;
    }

    private int MapValue(
        string attributeId,
        BakiAttributeMapping mapping,
        double value,
        IReadOnlyDictionary<string, BakiQuantileMap>? quantiles)
    {
        var scale = ResolveScale(attributeId);
        var targetScale = _spec.TargetScale;

        var mapped = mapping.MappingMethod switch
        {
            "Linear" => BakiAttributeMappingMath.MapLinear(value, scale, targetScale),
            "Piecewise" => BakiAttributeMappingMath.MapPiecewise(value, mapping.Piecewise, targetScale),
            "Quantile" => BakiAttributeMappingMath.MapQuantile(value, quantiles, attributeId, targetScale),
            _ => BakiAttributeMappingMath.MapLinear(value, scale, targetScale)
        };

        var rounded = BakiAttributeMappingMath.ApplyRounding(mapped, mapping.Rounding);
        var clampMin = mapping.ClampMin ?? targetScale.Min;
        var clampMax = mapping.ClampMax ?? targetScale.Max;
        return BakiAttributeMappingMath.Clamp(rounded, clampMin, clampMax);
    }

    private BakiAttributeScale ResolveScale(string attributeId)
    {
        if (_spec.SourceScales.ByAttribute is not null
            && _spec.SourceScales.ByAttribute.TryGetValue(attributeId, out var scale))
        {
            return scale;
        }

        return _spec.SourceScales.Default;
    }

    private bool TryResolveFallback(
        BakiAttributeFallbackRule fallback,
        IReadOnlyDictionary<string, int> results,
        BakiDerivedValues derived,
        out int value)
    {
        value = 0;

        if (!string.Equals(fallback.Method, "Average", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var values = new List<int>();
        foreach (var field in fallback.Fields)
        {
            if (results.TryGetValue(field, out var attributeValue))
            {
                values.Add(attributeValue);
                continue;
            }

            if (string.Equals(field, "ExperienceFactor", StringComparison.OrdinalIgnoreCase)
                && derived.ExperienceFactor.HasValue)
            {
                values.Add(derived.ExperienceFactor.Value);
            }
        }

        if (values.Count == 0)
        {
            return false;
        }

        value = (int)Math.Round(values.Average());
        return true;
    }

    private void ApplyRandomVariation(Dictionary<string, int> results)
    {
        if (_spec.RandomVariation is not { Enabled: true } variation)
        {
            return;
        }

        foreach (var attribute in variation.Attributes)
        {
            if (!results.TryGetValue(attribute, out var value))
            {
                continue;
            }

            var delta = _random.Next(0, variation.MaxDelta + 1);
            var sign = _random.Next(0, 2) == 0 ? -1 : 1;
            var adjusted = value + (delta * sign);
            results[attribute] = BakiAttributeMappingMath.Clamp(adjusted, _spec.TargetScale.Min, _spec.TargetScale.Max);
        }
    }
}
