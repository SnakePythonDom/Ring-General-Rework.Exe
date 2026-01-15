using RingGeneral.Specs.Models.Import;

namespace RingGeneral.Core.Import;

public static class BakiAttributeMappingMath
{
    public static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);

    public static int ApplyRounding(double value, string rounding)
        => rounding switch
        {
            "floor" => (int)Math.Floor(value),
            "ceil" => (int)Math.Ceiling(value),
            _ => (int)Math.Round(value)
        };

    public static double MapLinear(double value, BakiAttributeScale source, BakiAttributeScale target)
    {
        if (source.Max == source.Min)
        {
            return target.Min;
        }

        var normalized = (value - source.Min) / (double)(source.Max - source.Min);
        var raw = target.Min + (normalized * (target.Max - target.Min));
        return raw;
    }

    public static double MapPiecewise(double value, BakiAttributePiecewiseSpec? piecewise, BakiAttributeScale target)
    {
        if (piecewise is null || piecewise.Ranges.Count == 0)
        {
            return MapLinear(value, new BakiAttributeScale(0, 100), target);
        }

        var range = piecewise.Ranges.FirstOrDefault(r => value >= r.SourceMin && value <= r.SourceMax);
        if (range is null)
        {
            var first = piecewise.Ranges.First();
            var last = piecewise.Ranges.Last();
            return value < first.SourceMin
                ? target.Min
                : target.Max;
        }

        var normalized = (value - range.SourceMin) / (double)(range.SourceMax - range.SourceMin);
        return range.TargetMin + (normalized * (range.TargetMax - range.TargetMin));
    }

    public static double MapQuantile(
        double value,
        IReadOnlyDictionary<string, BakiQuantileMap>? quantiles,
        string attributeId,
        BakiAttributeScale target)
    {
        if (quantiles is null || !quantiles.TryGetValue(attributeId, out var map))
        {
            return MapLinear(value, new BakiAttributeScale(0, 100), target);
        }

        var percentile = map.PercentileFor(value);
        return target.Min + (percentile * (target.Max - target.Min));
    }
}
