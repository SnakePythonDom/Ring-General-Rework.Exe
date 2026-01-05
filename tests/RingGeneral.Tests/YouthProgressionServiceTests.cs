using System;
using System.Collections.Generic;
using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;
using Xunit;

namespace RingGeneral.Tests;

public sealed class YouthProgressionServiceTests
{
    [Fact]
    public void SimulerSemaine_AugmenteAttributsSelonDistribution()
    {
        var spec = new YouthSpec(
            new YouthStructureSpec(
                new YouthStructureDefaults(80000, 24, 3, 12, "HYBRIDE"),
                new Dictionary<string, YouthStructureDefaults>()),
            new YouthProgressionSpec(
                1.0,
                0,
                0,
                Array.Empty<YouthProgressionBudgetTier>(),
                1.0,
                new Dictionary<string, double>
                {
                    ["in_ring"] = 1.0,
                    ["entertainment"] = 0.0,
                    ["story"] = 0.0
                },
                new Dictionary<string, YouthProgressionPhilosophyBonus>()),
            new YouthGraduationSpec(20, 20, 20, 20));

        var structure = new YouthStructureState(
            "YOUTH-001",
            "Academy",
            "COMP-001",
            "FR",
            "ACADEMY",
            80000,
            24,
            3,
            12,
            "HYBRIDE",
            true,
            null,
            0);

        var trainee = new YouthTraineeProgressionState("TR-001", "YOUTH-001", 5, 5, 5, "EN_FORMATION");
        var service = new YouthProgressionService(new SeededRandomProvider(1), spec);

        var report = service.SimulerSemaine(1, new[] { structure }, new[] { trainee }, 1);

        var update = Assert.Single(report.Updates);
        Assert.Equal(6, update.NouveauInRing);
        Assert.Equal(5, update.NouveauEntertainment);
        Assert.Equal(5, update.NouveauStory);
        Assert.False(update.EstGradue);
    }

    [Fact]
    public void SimulerSemaine_DeclencheGraduation()
    {
        var spec = new YouthSpec(
            new YouthStructureSpec(
                new YouthStructureDefaults(80000, 24, 3, 12, "HYBRIDE"),
                new Dictionary<string, YouthStructureDefaults>()),
            new YouthProgressionSpec(
                0,
                0,
                0,
                Array.Empty<YouthProgressionBudgetTier>(),
                0,
                new Dictionary<string, double>
                {
                    ["in_ring"] = 1.0,
                    ["entertainment"] = 1.0,
                    ["story"] = 1.0
                },
                new Dictionary<string, YouthProgressionPhilosophyBonus>()),
            new YouthGraduationSpec(6, 6, 6, 6));

        var structure = new YouthStructureState(
            "YOUTH-001",
            "Academy",
            "COMP-001",
            "FR",
            "ACADEMY",
            80000,
            24,
            3,
            12,
            "HYBRIDE",
            true,
            null,
            0);

        var trainee = new YouthTraineeProgressionState("TR-002", "YOUTH-001", 6, 6, 6, "EN_FORMATION");
        var service = new YouthProgressionService(new SeededRandomProvider(3), spec);

        var report = service.SimulerSemaine(1, new[] { structure }, new[] { trainee }, 3);

        var update = Assert.Single(report.Updates);
        Assert.True(update.EstGradue);
    }
}
