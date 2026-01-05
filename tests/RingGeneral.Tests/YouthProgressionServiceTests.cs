using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;
using Xunit;

namespace RingGeneral.Tests;

public sealed class YouthProgressionServiceTests
{
    [Fact]
    public void AppliquerProgression_RespecteCapGains()
    {
        var spec = ConstruireSpec(chanceBase: 1.0, maxGain: 2);
        var trainee = new YouthTraineeProgressionState(
            "TR-001",
            "Alex Dubois",
            "YOUTH-001",
            "HYBRIDE",
            3,
            80000,
            12,
            "EN_FORMATION",
            1,
            9,
            9,
            9);
        var service = new YouthProgressionService(new SeededRandomProvider(7), spec);

        var report = service.AppliquerProgression(10, new[] { trainee });

        var result = Assert.Single(report.Resultats);
        Assert.True(result.DeltaInRing + result.DeltaEntertainment + result.DeltaStory <= 2);
        Assert.Equal(10, result.InRing);
        Assert.Equal(10, result.Entertainment);
        Assert.Equal(9, result.Story);
    }

    [Fact]
    public void AppliquerProgression_DiplomeQuandSeuilsAtteints()
    {
        var spec = ConstruireSpec(chanceBase: 0.0, maxGain: 1, minSemaines: 52, seuilMoyen: 10);
        var trainee = new YouthTraineeProgressionState(
            "TR-002",
            "Maya Roche",
            "YOUTH-001",
            "HYBRIDE",
            2,
            50000,
            10,
            "EN_FORMATION",
            1,
            12,
            12,
            12);
        var service = new YouthProgressionService(new SeededRandomProvider(3), spec);

        var report = service.AppliquerProgression(60, new[] { trainee });

        var result = Assert.Single(report.Resultats);
        Assert.True(result.Diplome);
    }

    private static YouthSpec ConstruireSpec(double chanceBase, int maxGain, int minSemaines = 0, int seuilMoyen = 12)
    {
        return new YouthSpec(
            new YouthSpecMeta("fr", "test", null),
            new YouthStructureSpec(
                new[] { "ACADEMY" },
                new YouthStructureInfrastructureSpec(1, 5),
                new YouthStructureBudgetSpec(0, 100000, new[] { new YouthStructureBudgetTier(0, 100000, 0) }),
                new YouthStructureStaffSpec(new[] { "Coach" }, 2),
                new Dictionary<string, YouthPhilosophyFocus> { ["HYBRIDE"] = new YouthPhilosophyFocus(1, 1, 1) }),
            new YouthProgressionSpec(chanceBase, 0.0, 0.0, maxGain, 20),
            new YouthGraduationSpec(minSemaines, seuilMoyen, 10, 10, 10));
    }
}
