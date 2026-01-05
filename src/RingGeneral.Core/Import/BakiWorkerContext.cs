using RingGeneral.Specs.Models.Import;

namespace RingGeneral.Core.Import;

public sealed class BakiWorkerContext
{
    private readonly Dictionary<string, object?> _fields;

    public BakiWorkerContext(IReadOnlyDictionary<string, object?> fields)
    {
        _fields = new Dictionary<string, object?>(fields, StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGetNumber(string field, out double value)
    {
        value = 0;
        if (!_fields.TryGetValue(field, out var raw) || raw is null)
        {
            return false;
        }

        return raw switch
        {
            double doubleValue => TryAssign(doubleValue, out value),
            float floatValue => TryAssign(floatValue, out value),
            int intValue => TryAssign(intValue, out value),
            long longValue => TryAssign(longValue, out value),
            decimal decimalValue => TryAssign(decimalValue, out value),
            string textValue when double.TryParse(textValue, out var parsed) => TryAssign(parsed, out value),
            _ => false
        };
    }

    public bool TryGetString(string field, out string value)
    {
        value = string.Empty;
        if (!_fields.TryGetValue(field, out var raw) || raw is null)
        {
            return false;
        }

        value = raw.ToString() ?? string.Empty;
        return true;
    }

    private static bool TryAssign(double input, out double value)
    {
        value = input;
        return true;
    }

    private static bool TryAssign(decimal input, out double value)
    {
        value = (double)input;
        return true;
    }
}

public sealed record BakiDerivedValues(int? ExperienceFactor)
{
    public static BakiDerivedValues Create(BakiAttributeExperienceFactorSpec? spec, BakiWorkerContext context)
    {
        if (spec is null)
        {
            return new BakiDerivedValues((int?)null);
        }

        var hasAge = context.TryGetNumber(spec.AgeField, out var age);
        var hasExperience = context.TryGetNumber(spec.ExperienceYearsField, out var experience);

        if (!hasAge && !hasExperience)
        {
            return new BakiDerivedValues((int?)null);
        }

        var ageNormalized = hasAge
            ? Normalize(age, spec.AgeMin, spec.AgeMax)
            : 0.5;
        var experienceNormalized = hasExperience
            ? Normalize(experience, spec.ExperienceMin, spec.ExperienceMax)
            : 0.5;

        var weightTotal = spec.AgeWeight + spec.ExperienceWeight;
        var combined = weightTotal > 0
            ? ((ageNormalized * spec.AgeWeight) + (experienceNormalized * spec.ExperienceWeight)) / weightTotal
            : (ageNormalized + experienceNormalized) / 2.0;

        var value = 1 + (combined * 19);
        return new BakiDerivedValues((int)Math.Round(value));
    }

    private static double Normalize(double value, int min, int max)
    {
        if (max <= min)
        {
            return 0.5;
        }

        var clamped = Math.Min(Math.Max(value, min), max);
        return (clamped - min) / (max - min);
    }
}
