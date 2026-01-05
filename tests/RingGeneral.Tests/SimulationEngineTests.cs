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
    public void Storyline_heat_baisse_si_ignoree()
    {
        var context = ConstruireContexte(segments: new[]
        {
            new SegmentDefinition("SEG-1", "promo", new[] { "W-1" }, 6, false, null, null, 40, null, null)
        });

        var engine = new ShowSimulationEngine(new SeededRandomProvider(21));
        var result = engine.Simuler(context);

        Assert.True(result.Delta.StorylineHeatDelta["S-1"] < 0);
    }

    [Fact]
    public void Storyline_heat_monte_si_segments_lies_et_bien_notes()
    {
        var context = ConstruireContexte(segments: new[]
        {
            new SegmentDefinition("SEG-1", "match", new[] { "W-1", "W-2" }, 10, true, "S-1", null, 70, "W-1", "W-2"),
            new SegmentDefinition("SEG-2", "promo", new[] { "W-1" }, 6, false, "S-1", null, 60, null, null)
        });

        var engine = new ShowSimulationEngine(new SeededRandomProvider(33));
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

    [Fact]
    public void Deal_influence_audience_et_revenus()
    {
        var deals = new Dictionary<string, TvDealDefinition>
        {
            ["TV-1"] = new(
                "TV-1",
                "COMP-1",
                "Canal Ring",
                20,
                95,
                4000,
                30,
                new[] { "Prime time", "Exclusivité" })
        };
        var engine = new ShowSimulationEngine(
            new SeededRandomProvider(9),
            new AudienceModel(),
            new DealRevenueModel(deals));

        var contextAvecDeal = ConstruireContexte(dealTvId: "TV-1");
        var contextSansDeal = ConstruireContexte(dealTvId: null);

        var resultAvec = engine.Simuler(contextAvecDeal);
        var resultSans = engine.Simuler(contextSansDeal);

        Assert.True(resultAvec.RapportShow.Audience > resultSans.RapportShow.Audience);
        Assert.True(resultAvec.RapportShow.Tv > 0);
        Assert.Equal(0, resultSans.RapportShow.Tv);
    }

    private static ShowContext ConstruireContexte(
        IReadOnlyList<SegmentDefinition>? segments = null,
        string? dealTvId = "TV-1")
    {
        var show = new ShowDefinition("SHOW-TEST", "Test Show", 1, "FR", 90, "COMP-1", dealTvId, "Paris", "Canal Ring");
        var company = new CompanyState("COMP-1", "Compagnie Test", "FR", 55, 10000, 50, 4);
        var workersList = workers?.ToList() ?? new List<WorkerSnapshot>
        {
            new("W-1", "Alpha", 70, 60, 55, 50, 10, "AUCUNE", 2, "MAIN_EVENT"),
            new("W-2", "Beta", 65, 65, 58, 48, 12, "AUCUNE", 1, "MID")
        };
        var titles = new List<TitleInfo> { new("T-1", "Titre Test", 60, "W-1") };
        var storylines = new List<StorylineInfo>
        {
            new("S-1", "Storyline Test", "BUILD", 45, "ACTIVE", null, new[] { new StorylineParticipant("W-1", "principal") })
        };
        var segmentsList = segments ?? new List<SegmentDefinition>
        {
            new("SEG-1", "match", new[] { "W-1", "W-2" }, 12, true, "S-1", "T-1", 70, "W-1", "W-2")
        };
        var chimies = new Dictionary<string, int> { ["W-1|W-2"] = 5 };

        return new ShowContext(show, company, workersList, titles, storylines, segmentsList, chimies);
    }
}
