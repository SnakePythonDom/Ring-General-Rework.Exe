using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IShowSchedulerStore
{
    ShowSchedule? ChargerShow(string showId);
    IReadOnlyList<ShowSchedule> ChargerShows(string companyId);
    IReadOnlyList<CalendarEntry> ChargerCalendarEntries(string companyId);
    IReadOnlyList<ShowSettings> ChargerShowSettings(string companyId);
    void AjouterShow(ShowSchedule show);
    void MettreAJourShow(ShowSchedule show);
    void SupprimerShow(string showId);
    void AjouterCalendarEntry(CalendarEntry entry);
    void MettreAJourCalendarEntry(CalendarEntry entry);
    void SupprimerCalendarEntry(string entryId);
}
