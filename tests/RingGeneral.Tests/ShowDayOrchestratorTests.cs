using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Services;
using Xunit;

namespace RingGeneral.Tests;

public sealed class ShowDayOrchestratorTests
{
    [Fact]
    public void DetecterShowAVenir_QuandShowPlanifie_RetourneDetecte()
    {
        // Arrange
        var store = new FakeShowSchedulerStore();
        var show = new ShowSchedule(
            "SHOW-001",
            "COMPANY-001",
            "Monday Night Raw",
            ShowType.TvWeekly,
            new DateOnly(2024, 1, 15),
            120,
            "VENUE-001",
            "USA Network",
            25.0m,
            ShowStatus.ABooker);

        store.Shows.Add(show);

        var orchestrator = new ShowDayOrchestrator(store);

        // Act
        var result = orchestrator.DetecterShowAVenir("COMPANY-001", 2024);

        // Assert
        Assert.True(result.ShowDetecte);
        Assert.NotNull(result.Show);
        Assert.Equal("Monday Night Raw", result.Show.Nom);
    }

    [Fact]
    public void DetecterShowAVenir_QuandAucunShow_RetourneNonDetecte()
    {
        // Arrange
        var store = new FakeShowSchedulerStore();
        var orchestrator = new ShowDayOrchestrator(store);

        // Act
        var result = orchestrator.DetecterShowAVenir("COMPANY-001", 1);

        // Assert
        Assert.False(result.ShowDetecte);
        Assert.Null(result.Show);
    }

    [Fact]
    public void SimulerShow_AvecContexteValide_RetourneResultat()
    {
        // Arrange
        var orchestrator = new ShowDayOrchestrator();
        var context = CreerContexteTest();

        // Act
        var result = orchestrator.SimulerShow(context);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.RapportShow);
        Assert.NotNull(result.Delta);
        Assert.True(result.RapportShow.NoteGlobale >= 0);
        Assert.True(result.RapportShow.NoteGlobale <= 100);
    }

    [Fact]
    public void FinaliserShow_AvecChangementTitre_InclutInfoChangement()
    {
        // Arrange
        var store = new FakeShowSchedulerStore();
        var titleRepo = new FakeTitleRepository();
        var titleService = new TitleService(titleRepo);
        var orchestrator = new ShowDayOrchestrator(store, titleService);

        var context = CreerContexteAvecTitre();
        var resultatSimulation = orchestrator.SimulerShow(context);

        // Act
        var finalisation = orchestrator.FinaliserShow(resultatSimulation, context);

        // Assert
        Assert.True(finalisation.Succes);
        Assert.NotEmpty(finalisation.Changements);
        // Vérifier qu'il y a au moins un message sur l'audience et les finances
        Assert.Contains(finalisation.Changements, c => c.Contains("Audience"));
        Assert.Contains(finalisation.Changements, c => c.Contains("Revenus"));
    }

    [Fact]
    public void FinaliserShow_SansChangementTitre_IncluQuandMemeFinances()
    {
        // Arrange
        var orchestrator = new ShowDayOrchestrator();
        var context = CreerContexteTest();
        var resultatSimulation = orchestrator.SimulerShow(context);

        // Act
        var finalisation = orchestrator.FinaliserShow(resultatSimulation, context);

        // Assert
        Assert.True(finalisation.Succes);
        Assert.NotEmpty(finalisation.Changements);
        Assert.Contains(finalisation.Changements, c => c.Contains("Revenus"));
        Assert.Contains(finalisation.Changements, c => c.Contains("Note du show"));
    }

    // Helpers

    private static ShowContext CreerContexteTest()
    {
        var show = new ShowDefinition(
            "SHOW-001",
            "Test Show",
            1,
            "USA",
            120,
            "COMPANY-001",
            null,
            "Arena",
            "TV");

        var compagnie = new CompanyState(
            "COMPANY-001",
            "Test Company",
            "USA",
            75,
            80,
            65,
            50000);

        var worker = new WorkerSnapshot(
            "WORKER-001",
            "John Doe",
            "Face",
            75,
            70,
            65,
            80,
            50,
            10,
            null);

        var segment = new SegmentDefinition(
            "SEG-001",
            "match",
            new[] { "WORKER-001" },
            15,
            true,
            null,
            null,
            60,
            "WORKER-001",
            null,
            new Dictionary<string, string>());

        return new ShowContext(
            show,
            compagnie,
            new[] { worker },
            new[] { segment },
            Array.Empty<StorylineSnapshot>(),
            Array.Empty<TitleSnapshot>(),
            new Dictionary<string, int>(),
            null);
    }

    private static ShowContext CreerContexteAvecTitre()
    {
        var show = new ShowDefinition(
            "SHOW-001",
            "Test Show",
            1,
            "USA",
            120,
            "COMPANY-001",
            null,
            "Arena",
            "TV");

        var compagnie = new CompanyState(
            "COMPANY-001",
            "Test Company",
            "USA",
            75,
            80,
            65,
            50000);

        var champion = new WorkerSnapshot(
            "WORKER-001",
            "Champion",
            "Face",
            75,
            70,
            65,
            80,
            50,
            10,
            null);

        var challenger = new WorkerSnapshot(
            "WORKER-002",
            "Challenger",
            "Heel",
            70,
            75,
            60,
            75,
            45,
            15,
            null);

        var titre = new TitleSnapshot(
            "TITLE-001",
            "World Championship",
            "WORKER-001",
            75);

        var segment = new SegmentDefinition(
            "SEG-001",
            "match",
            new[] { "WORKER-001", "WORKER-002" },
            20,
            true,
            null,
            "TITLE-001",
            75,
            "WORKER-002", // Le challenger gagne
            "WORKER-001",
            new Dictionary<string, string>());

        return new ShowContext(
            show,
            compagnie,
            new[] { champion, challenger },
            new[] { segment },
            Array.Empty<StorylineSnapshot>(),
            new[] { titre },
            new Dictionary<string, int>(),
            null);
    }

    // Fakes pour les tests

    private sealed class FakeShowSchedulerStore : IShowSchedulerStore
    {
        public List<ShowSchedule> Shows { get; } = new();

        public void AjouterShow(ShowSchedule show) => Shows.Add(show);
        public void MettreAJourShow(ShowSchedule show) { }
        public ShowSchedule? ChargerShow(string showId) => Shows.FirstOrDefault(s => s.ShowId == showId);

        public IReadOnlyList<ShowSchedule> ChargerShowsAVenir(string companyId, int minWeek, int maxWeek)
        {
            return Shows
                .Where(s => s.CompanyId == companyId && s.Date.Year >= minWeek && s.Date.Year <= maxWeek)
                .ToList();
        }

        public void AjouterCalendarEntry(CalendarEntry entry) { }
        public void MettreAJourCalendarEntry(CalendarEntry entry) { }
        public void SupprimerCalendarEntry(string entryId) { }
        public IReadOnlyList<CalendarEntry> ChargerCalendarEntries(string companyId) => Array.Empty<CalendarEntry>();
        public IReadOnlyList<ShowSettings> ChargerShowSettings(string companyId) => Array.Empty<ShowSettings>();
    }

    private sealed class FakeTitleRepository : ITitleRepository
    {
        private readonly Dictionary<string, TitleData> _titles = new();
        private readonly List<TitleMatchRecord> _matches = new();
        private readonly Dictionary<string, TitleReignData> _reigns = new();

        public TitleData? ChargerTitre(string titleId)
        {
            if (!_titles.ContainsKey(titleId))
            {
                _titles[titleId] = new TitleData(titleId, "Test Title", "WORKER-001", 75);
            }
            return _titles[titleId];
        }

        public TitleReignData? ChargerRegneCourant(string titleId) =>
            _reigns.Values.FirstOrDefault(r => r.TitleId == titleId && r.EndWeek == null);

        public int CompterDefenses(string titleId, int startWeek) =>
            _matches.Count(m => m.TitleId == titleId && m.Week >= startWeek && !m.TitleChange);

        public void CloreRegne(string reignId, int endWeek)
        {
            if (_reigns.ContainsKey(reignId))
            {
                var reign = _reigns[reignId];
                _reigns[reignId] = reign with { EndWeek = endWeek };
            }
        }

        public string CreerRegne(string titleId, string workerId, int startWeek)
        {
            var reignId = $"REIGN-{Guid.NewGuid():N}";
            _reigns[reignId] = new TitleReignData(reignId, titleId, workerId, startWeek, null);
            return reignId;
        }

        public void MettreAJourChampion(string titleId, string workerId)
        {
            if (_titles.ContainsKey(titleId))
            {
                var title = _titles[titleId];
                _titles[titleId] = title with { HolderWorkerId = workerId };
            }
        }

        public void AjouterMatch(TitleMatchRecord match) => _matches.Add(match);

        public void MettreAJourPrestige(string titleId, int delta)
        {
            if (_titles.ContainsKey(titleId))
            {
                var title = _titles[titleId];
                _titles[titleId] = title with { Prestige = Math.Clamp(title.Prestige + delta, 0, 100) };
            }
        }
    }

    private sealed record TitleData(string TitleId, string Nom, string? HolderWorkerId, int Prestige);
    private sealed record TitleReignData(string TitleReignId, string TitleId, string WorkerId, int StartWeek, int? EndWeek);
}
