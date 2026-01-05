using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;
using Xunit;

namespace RingGeneral.Tests;

public sealed class SimulationEngineTests
{
    [Fact]
    public void Simulation_est_deterministe_avec_seed()
    {
        var context = ConstruireContexte();
        var engineA = new ShowSimulationEngine(new SeededRandomProvider(123));
        var engineB = new ShowSimulationEngine(new SeededRandomProvider(123));

        var resultA = engineA.Simuler(context);
        var resultB = engineB.Simuler(context);

        Assert.Equal(resultA.RapportShow.NoteGlobale, resultB.RapportShow.NoteGlobale);
    }

    [Fact]
    public void Pacing_penalise_les_promos_enchainees()
    {
        var context = ConstruireContexte(segments: new[]
        {
            new SegmentDefinition("SEG-1", "promo", new[] { "W-1" }, 6, false, null, null, 30, null, null),
            new SegmentDefinition("SEG-2", "promo", new[] { "W-2" }, 6, false, null, null, 30, null, null)
        });

        var engine = new ShowSimulationEngine(new SeededRandomProvider(42));
        var result = engine.Simuler(context);

        Assert.True(result.RapportShow.Segments[1].PacingPenalty < 0);
    }

    [Fact]
    public void Fatigue_augmente_avec_la_duree()
    {
        var context = ConstruireContexte(segments: new[]
        {
            new SegmentDefinition("SEG-1", "match", new[] { "W-1", "W-2" }, 6, false, null, null, 60, "W-1", "W-2"),
            new SegmentDefinition("SEG-2", "match", new[] { "W-1", "W-2" }, 12, false, null, null, 60, "W-1", "W-2")
        });

        var engine = new ShowSimulationEngine(new SeededRandomProvider(7));
        var result = engine.Simuler(context);

        var fatigue1 = result.RapportShow.Segments[0].Impact.FatigueDelta["W-1"];
        var fatigue2 = result.RapportShow.Segments[1].Impact.FatigueDelta["W-1"];

        Assert.True(fatigue2 > fatigue1);
    }

    [Fact]
    public void Storyline_heat_monte_si_segment_lie()
    {
        var context = ConstruireContexte(segments: new[]
        {
            new SegmentDefinition("SEG-1", "promo", new[] { "W-1" }, 6, false, "S-1", null, 40, null, null)
        });

        var engine = new ShowSimulationEngine(new SeededRandomProvider(12));
        var result = engine.Simuler(context);

        Assert.True(result.Delta.StorylineHeatDelta["S-1"] > 0);
    }

    [Fact]
    public void Title_prestige_varie_sur_match_de_titre()
    {
        var context = ConstruireContexte(segments: new[]
        {
            new SegmentDefinition("SEG-1", "match", new[] { "W-1", "W-2" }, 12, true, null, "T-1", 70, "W-1", "W-2")
        });

        var engine = new ShowSimulationEngine(new SeededRandomProvider(5));
        var result = engine.Simuler(context);

        Assert.True(result.Delta.TitrePrestigeDelta["T-1"] != 0);
    }

    private static ShowContext ConstruireContexte(IReadOnlyList<SegmentDefinition>? segments = null)
    {
        var show = new ShowDefinition("SHOW-TEST", "Test Show", 1, "FR", 90, "COMP-1", "TV-1");
        var company = new CompanyState("COMP-1", "Compagnie Test", "FR", 55, 10000, 50, 4);
        var workers = new List<WorkerSnapshot>
        {
            new("W-1", "Alpha", 70, 60, 55, 50, 10, "AUCUNE", 2, "MAIN_EVENT", 62),
            new("W-2", "Beta", 65, 65, 58, 48, 12, "AUCUNE", 1, "MID", 58)
        };
        var titles = new List<TitleInfo> { new("T-1", "Titre Test", 60, "W-1") };
        var storylines = new List<StorylineInfo> { new("S-1", "Storyline Test", 45, new[] { "W-1" }) };
        var segmentsList = segments ?? new List<SegmentDefinition>
        {
            new("SEG-1", "match", new[] { "W-1", "W-2" }, 12, true, "S-1", "T-1", 70, "W-1", "W-2")
        };
        var chimies = new Dictionary<string, int> { ["W-1|W-2"] = 5 };

        return new ShowContext(show, company, workers, titles, storylines, segmentsList, chimies);
    }
}
