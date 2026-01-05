using RingGeneral.Core.Import;
using RingGeneral.Core.Random;
using RingGeneral.Specs.Models.Import;
using Xunit;

namespace RingGeneral.Tests;

public sealed class BakiAttributeConversionTests
{
    [Fact]
    public void LinearMapping_MapsBounds()
    {
        var spec = BuildSpec(new Dictionary<string, BakiAttributeMapping>
        {
            ["technique"] = new(
                "technique",
                "Linear",
                1,
                20,
                "round",
                5,
                null,
                null,
                null)
        });

        var converter = new BakiAttributeConverter(spec);
        var low = converter.ConvertWorker(new BakiWorkerContext(new Dictionary<string, object?>
        {
            ["technique"] = 0
        }));
        var high = converter.ConvertWorker(new BakiWorkerContext(new Dictionary<string, object?>
        {
            ["technique"] = 100
        }));

        Assert.Equal(1, low["technique"]);
        Assert.Equal(20, high["technique"]);
    }

    [Fact]
    public void PiecewiseMapping_RespectsThresholds()
    {
        var piecewise = new BakiAttributePiecewiseSpec(new[]
        {
            new BakiAttributePiecewiseRange(0, 49, 1, 10),
            new BakiAttributePiecewiseRange(50, 74, 11, 15),
            new BakiAttributePiecewiseRange(75, 89, 16, 18),
            new BakiAttributePiecewiseRange(90, 100, 19, 20)
        });

        var spec = BuildSpec(new Dictionary<string, BakiAttributeMapping>
        {
            ["selling"] = new(
                "selling",
                "Piecewise",
                1,
                20,
                "round",
                5,
                piecewise,
                null,
                null)
        });

        var converter = new BakiAttributeConverter(spec);

        var low = converter.ConvertWorker(new BakiWorkerContext(new Dictionary<string, object?>
        {
            ["selling"] = 49
        }));
        var mid = converter.ConvertWorker(new BakiWorkerContext(new Dictionary<string, object?>
        {
            ["selling"] = 50
        }));
        var high = converter.ConvertWorker(new BakiWorkerContext(new Dictionary<string, object?>
        {
            ["selling"] = 90
        }));

        Assert.Equal(10, low["selling"]);
        Assert.Equal(11, mid["selling"]);
        Assert.Equal(19, high["selling"]);
    }

    [Fact]
    public void NormalizationCaps_LimitExtremes()
    {
        var spec = BuildSpec(new Dictionary<string, BakiAttributeMapping>
        {
            ["technique"] = BuildLinearMapping("technique"),
            ["selling"] = BuildLinearMapping("selling"),
            ["psychologie"] = BuildLinearMapping("psychologie"),
            ["micro"] = BuildLinearMapping("micro")
        },
        normalization: new BakiAttributeNormalizationRules(
            true,
            new BakiAttributeEliteCap(true, 18, 2, 17, new[] { "micro" }),
            new BakiAttributeCatastrophicCap(true, 3, 1, 4),
            new BakiAttributeVarianceClamp(false, 10, 0.9)));

        var converter = new BakiAttributeConverter(spec);
        var converted = converter.ConvertWorker(new BakiWorkerContext(new Dictionary<string, object?>
        {
            ["technique"] = 100,
            ["selling"] = 100,
            ["psychologie"] = 100,
            ["micro"] = 100
        }));

        var eliteCount = converted.Values.Count(value => value >= 18);
        Assert.True(eliteCount <= 2);
        Assert.True(converted.Values.All(value => value >= 4));
    }

    [Fact]
    public void RandomVariation_IsDeterministic()
    {
        var spec = BuildSpec(new Dictionary<string, BakiAttributeMapping>
        {
            ["technique"] = BuildLinearMapping("technique")
        },
        randomVariation: new BakiAttributeRandomVariation(true, 42, 1, new[] { "technique" }));

        var converterA = new BakiAttributeConverter(spec, new SeededRandomProvider(42));
        var converterB = new BakiAttributeConverter(spec, new SeededRandomProvider(42));

        var input = new BakiWorkerContext(new Dictionary<string, object?>
        {
            ["technique"] = 50
        });

        var first = converterA.ConvertWorker(input);
        var second = converterB.ConvertWorker(input);

        Assert.Equal(first["technique"], second["technique"]);
    }

    private static BakiAttributeMapping BuildLinearMapping(string sourceField)
        => new(sourceField, "Linear", 1, 20, "round", 5, null, null, null);

    private static BakiAttributeMappingSpec BuildSpec(
        IReadOnlyDictionary<string, BakiAttributeMapping> mappings,
        BakiAttributeNormalizationRules? normalization = null,
        BakiAttributeRandomVariation? randomVariation = null)
    {
        return new BakiAttributeMappingSpec(
            "workers",
            new BakiAttributeScaleSet(
                new BakiAttributeScale(0, 100),
                null),
            new BakiAttributeScale(1, 20),
            mappings,
            new BakiAttributeGroupWeights(new Dictionary<string, IReadOnlyDictionary<string, double>>()),
            normalization ?? new BakiAttributeNormalizationRules(
                false,
                new BakiAttributeEliteCap(false, 18, 3, 17, Array.Empty<string>()),
                new BakiAttributeCatastrophicCap(false, 3, 4, 4),
                new BakiAttributeVarianceClamp(false, 10, 0.9)),
            randomVariation,
            null,
            null,
            null);
    }
}
