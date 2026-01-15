using RingGeneral.Specs.Models.Import;

namespace RingGeneral.Core.Import;

public static class BakiAttributeNormalizer
{
    public static void Normalize(Dictionary<string, int> attributes, BakiAttributeNormalizationRules normalization)
    {
        ApplyEliteCap(attributes, normalization.EliteCap);
        ApplyCatastrophicCap(attributes, normalization.CatastrophicCap);
        ApplyVarianceClamp(attributes, normalization.VarianceClamp);
    }

    public static void ApplyRoleCoherence(
        Dictionary<string, int> attributes,
        BakiAttributeRoleCoherence coherence,
        BakiWorkerContext context)
    {
        if (!context.TryGetString(coherence.RoleField, out var role) || string.IsNullOrWhiteSpace(role))
        {
            return;
        }

        if (coherence.MainEventerValues.Any(value => string.Equals(value, role, StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var (attribute, minimum) in coherence.MainEventerMinimums)
            {
                if (attributes.TryGetValue(attribute, out var current))
                {
                    attributes[attribute] = Math.Max(current, minimum);
                }
            }
        }

        if (coherence.RookieValues.Any(value => string.Equals(value, role, StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var (attribute, maximum) in coherence.RookieMaximums)
            {
                if (attributes.TryGetValue(attribute, out var current))
                {
                    attributes[attribute] = Math.Min(current, maximum);
                }
            }
        }
    }

    private static void ApplyEliteCap(Dictionary<string, int> attributes, BakiAttributeEliteCap elite)
    {
        if (!elite.Enabled)
        {
            return;
        }

        var eliteAttributes = attributes
            .Where(kvp => kvp.Value >= elite.Threshold)
            .OrderByDescending(kvp => kvp.Value)
            .ToList();

        if (eliteAttributes.Count <= elite.MaxCount)
        {
            return;
        }

        var protectedSet = new HashSet<string>(elite.SignatureAttributes, StringComparer.OrdinalIgnoreCase);
        var reducible = eliteAttributes.Where(kvp => !protectedSet.Contains(kvp.Key)).ToList();
        var index = 0;

        while (eliteAttributes.Count > elite.MaxCount && index < reducible.Count)
        {
            var key = reducible[index].Key;
            attributes[key] = Math.Min(attributes[key], elite.ReduceTo);
            eliteAttributes = attributes
                .Where(kvp => kvp.Value >= elite.Threshold)
                .OrderByDescending(kvp => kvp.Value)
                .ToList();
            index++;
        }
    }

    private static void ApplyCatastrophicCap(Dictionary<string, int> attributes, BakiAttributeCatastrophicCap catastrophic)
    {
        if (!catastrophic.Enabled)
        {
            return;
        }

        var lowAttributes = attributes
            .Where(kvp => kvp.Value <= catastrophic.Threshold)
            .OrderBy(kvp => kvp.Value)
            .ToList();

        if (lowAttributes.Count <= catastrophic.MaxCount)
        {
            return;
        }

        var index = 0;
        while (lowAttributes.Count > catastrophic.MaxCount && index < lowAttributes.Count)
        {
            var key = lowAttributes[index].Key;
            attributes[key] = Math.Max(attributes[key], catastrophic.RaiseTo);
            lowAttributes = attributes
                .Where(kvp => kvp.Value <= catastrophic.Threshold)
                .OrderBy(kvp => kvp.Value)
                .ToList();
            index++;
        }
    }

    private static void ApplyVarianceClamp(Dictionary<string, int> attributes, BakiAttributeVarianceClamp clamp)
    {
        if (!clamp.Enabled || attributes.Count == 0)
        {
            return;
        }

        var values = attributes.Values.Select(v => (double)v).ToArray();
        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Length;
        var stdDev = Math.Sqrt(variance);

        if (stdDev <= clamp.MaxStandardDeviation)
        {
            return;
        }

        foreach (var key in attributes.Keys.ToList())
        {
            var value = attributes[key];
            var adjusted = mean + ((value - mean) * clamp.BlendFactor);
            attributes[key] = (int)Math.Round(adjusted);
        }
    }
}
