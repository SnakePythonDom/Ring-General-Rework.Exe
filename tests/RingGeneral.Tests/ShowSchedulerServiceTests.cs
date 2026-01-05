using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using Xunit;

namespace RingGeneral.Tests;

public sealed class ShowSchedulerServiceTests
{
    [Fact]
    public void CreerShow_AjouteUneEntreeCalendrier()
    {
        var store = new InMemoryShowSchedulerStore();
        store.Settings.Add(new ShowSettings(
            "SET-001",
            "COMP-001",
            ShowType.Tv,
            90,
            180,
            10,
            150,
            true));

        var service = new ShowSchedulerService(store);
        var draft = new ShowScheduleDraft(
            "COMP-001",
            "RG Weekly",
            ShowType.Tv,
            new DateOnly(2025, 1, 18),
            120,
            "VEN-001",
            "RG Network",
            35);

        var result = service.CreerShow(draft);

        Assert.True(result.Reussite);
        Assert.NotNull(result.Show);
        Assert.NotNull(result.CalendarEntry);
        Assert.Single(store.CalendarEntries);
        Assert.Equal(result.Show!.ShowId, store.CalendarEntries[0].ReferenceId);
    }

    [Fact]
    public void CreerShow_RefuseRuntimeHorsLimite()
    {
        var store = new InMemoryShowSchedulerStore();
        store.Settings.Add(new ShowSettings(
            "SET-002",
            "COMP-001",
            ShowType.Tv,
            90,
            120,
            10,
            100,
            false));

        var service = new ShowSchedulerService(store);
        var draft = new ShowScheduleDraft(
            "COMP-001",
            "RG Weekly",
            ShowType.Tv,
            new DateOnly(2025, 1, 25),
            45,
            null,
            null,
            25);

        var result = service.CreerShow(draft);

        Assert.False(result.Reussite);
        Assert.Contains(result.Erreurs, erreur => erreur.Contains("durée du show", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class InMemoryShowSchedulerStore : IShowSchedulerStore
    {
        public List<ShowSchedule> Shows { get; } = [];
        public List<CalendarEntry> CalendarEntries { get; } = [];
        public List<ShowSettings> Settings { get; } = [];

        public ShowSchedule? ChargerShow(string showId)
        {
            return Shows.FirstOrDefault(show => show.ShowId == showId);
        }

        public IReadOnlyList<ShowSchedule> ChargerShows(string companyId)
        {
            return Shows.Where(show => show.CompanyId == companyId).ToList();
        }

        public IReadOnlyList<CalendarEntry> ChargerCalendarEntries(string companyId)
        {
            return CalendarEntries.Where(entry => entry.CompanyId == companyId).ToList();
        }

        public IReadOnlyList<ShowSettings> ChargerShowSettings(string companyId)
        {
            return Settings.Where(setting => setting.CompanyId == companyId).ToList();
        }

        public void AjouterShow(ShowSchedule show)
        {
            Shows.Add(show);
        }

        public void MettreAJourShow(ShowSchedule show)
        {
            var index = Shows.FindIndex(item => item.ShowId == show.ShowId);
            if (index >= 0)
            {
                Shows[index] = show;
            }
            else
            {
                Shows.Add(show);
            }
        }

        public void SupprimerShow(string showId)
        {
            Shows.RemoveAll(show => show.ShowId == showId);
        }

        public void AjouterCalendarEntry(CalendarEntry entry)
        {
            CalendarEntries.Add(entry);
        }

        public void MettreAJourCalendarEntry(CalendarEntry entry)
        {
            var index = CalendarEntries.FindIndex(item => item.CalendarEntryId == entry.CalendarEntryId);
            if (index >= 0)
            {
                CalendarEntries[index] = entry;
            }
            else
            {
                CalendarEntries.Add(entry);
            }
        }

        public void SupprimerCalendarEntry(string entryId)
        {
            CalendarEntries.RemoveAll(entry => entry.CalendarEntryId == entryId);
        }
    }
}
