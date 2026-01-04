using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;
using Xunit;

namespace RingGeneral.Tests;

public sealed class WorldSimSchedulerTests
{
    [Fact]
    public void Planifier_ResteDeterministe_AvecMemeSeed()
    {
        var settings = new WorldSimSettings(2, 20, 1, 4, 123);
        var compagnies = new List<CompanyState>
        {
            new("COMP-001", "Alpha", "FR", 70, 10000, 40, 5),
            new("COMP-002", "Beta", "US", 65, 12000, 38, 6),
            new("COMP-003", "Gamma", "JP", 50, 9000, 34, 4),
            new("COMP-004", "Delta", "UK", 45, 8000, 30, 3)
        };

        var randomA = new SeededRandomProvider(settings.Seed);
        var randomB = new SeededRandomProvider(settings.Seed);

        var schedulerA = new WorldSimScheduler(randomA, settings);
        var schedulerB = new WorldSimScheduler(randomB, settings);

        var planA = schedulerA.Planifier(8, "COMP-001", compagnies);
        var planB = schedulerB.Planifier(8, "COMP-001", compagnies);

        Assert.Equal(planA.Compagnies, planB.Compagnies);
    }
}
