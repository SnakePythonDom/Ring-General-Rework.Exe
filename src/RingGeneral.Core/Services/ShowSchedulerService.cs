using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class ShowSchedulerService
{
    private readonly IShowSchedulerStore _store;

    public ShowSchedulerService(IShowSchedulerStore store)
    {
        _store = store;
    }

    public ShowSchedulerResult CreerShow(ShowScheduleDraft draft)
    {
        var erreurs = new List<string>();
        var avertissements = new List<string>();

        var settings = ChargerSettings(draft.CompanyId, draft.Type, erreurs);
        if (settings is not null)
        {
            ValiderRuntime(draft.RuntimeMinutes, settings, erreurs);
            ValiderPrixBillet(draft.TicketPrice, settings, erreurs);
            if (settings.DiffusionObligatoire && string.IsNullOrWhiteSpace(draft.Broadcast))
            {
                avertissements.Add("La diffusion est requise pour ce type de show.");
            }
        }

        if (DetecterConflitCalendrier(draft.CompanyId, draft.Date, null))
        {
            erreurs.Add("Conflit de calendrier détecté pour cette date.");
        }

        if (erreurs.Count > 0)
        {
            return new ShowSchedulerResult(false, erreurs, null, null, avertissements);
        }

        var show = new ShowSchedule(
            Guid.NewGuid().ToString("N"),
            draft.CompanyId,
            draft.Nom,
            draft.Type,
            draft.Date,
            draft.RuntimeMinutes,
            draft.VenueId,
            draft.Broadcast,
            draft.TicketPrice,
            ShowStatus.ABooker);

        var entry = new CalendarEntry(
            Guid.NewGuid().ToString("N"),
            draft.CompanyId,
            draft.Date,
            "SHOW",
            show.ShowId,
            show.Nom);

        _store.AjouterShow(show);
        _store.AjouterCalendarEntry(entry);

        return new ShowSchedulerResult(true, Array.Empty<string>(), show, entry, avertissements);
    }

    public ShowSchedulerResult MettreAJourShow(string showId, ShowScheduleUpdate update)
    {
        var erreurs = new List<string>();
        var avertissements = new List<string>();

        var show = _store.ChargerShow(showId);
        if (show is null)
        {
            erreurs.Add("Show introuvable.");
            return new ShowSchedulerResult(false, erreurs, null, null, avertissements);
        }

        var updated = show with
        {
            Nom = update.Nom ?? show.Nom,
            Date = update.Date ?? show.Date,
            RuntimeMinutes = update.RuntimeMinutes ?? show.RuntimeMinutes,
            VenueId = update.VenueId ?? show.VenueId,
            Broadcast = update.Broadcast ?? show.Broadcast,
            TicketPrice = update.TicketPrice ?? show.TicketPrice,
            Statut = update.Statut ?? show.Statut
        };

        var settings = ChargerSettings(updated.CompanyId, updated.Type, erreurs);
        if (settings is not null)
        {
            ValiderRuntime(updated.RuntimeMinutes, settings, erreurs);
            ValiderPrixBillet(updated.TicketPrice, settings, erreurs);
            if (settings.DiffusionObligatoire && string.IsNullOrWhiteSpace(updated.Broadcast))
            {
                avertissements.Add("La diffusion est requise pour ce type de show.");
            }
        }

        if (DetecterConflitCalendrier(updated.CompanyId, updated.Date, show.ShowId))
        {
            erreurs.Add("Conflit de calendrier détecté pour cette date.");
        }

        if (erreurs.Count > 0)
        {
            return new ShowSchedulerResult(false, erreurs, null, null, avertissements);
        }

        _store.MettreAJourShow(updated);

        var entry = _store.ChargerCalendarEntries(updated.CompanyId)
            .FirstOrDefault(item => item.ReferenceId == updated.ShowId);

        if (entry is null)
        {
            entry = new CalendarEntry(
                Guid.NewGuid().ToString("N"),
                updated.CompanyId,
                updated.Date,
                "SHOW",
                updated.ShowId,
                updated.Nom);
            _store.AjouterCalendarEntry(entry);
        }
        else
        {
            entry = entry with { Date = updated.Date, Titre = updated.Nom };
            _store.MettreAJourCalendarEntry(entry);
        }

        return new ShowSchedulerResult(true, Array.Empty<string>(), updated, entry, avertissements);
    }

    public ShowSchedulerResult AnnulerShow(string showId)
    {
        var erreurs = new List<string>();
        var avertissements = new List<string>();

        var show = _store.ChargerShow(showId);
        if (show is null)
        {
            erreurs.Add("Show introuvable.");
            return new ShowSchedulerResult(false, erreurs, null, null, avertissements);
        }

        var updated = show with { Statut = ShowStatus.Annule };
        _store.MettreAJourShow(updated);

        var entry = _store.ChargerCalendarEntries(updated.CompanyId)
            .FirstOrDefault(item => item.ReferenceId == updated.ShowId);

        if (entry is not null)
        {
            _store.SupprimerCalendarEntry(entry.CalendarEntryId);
        }

        return new ShowSchedulerResult(true, Array.Empty<string>(), updated, entry, avertissements);
    }

    private ShowSettings? ChargerSettings(string companyId, ShowType type, List<string> erreurs)
    {
        var settings = _store.ChargerShowSettings(companyId)
            .FirstOrDefault(item => item.Type == type);
        if (settings is null)
        {
            erreurs.Add("Paramètres de show introuvables.");
        }

        return settings;
    }

    private static void ValiderRuntime(int runtime, ShowSettings settings, List<string> erreurs)
    {
        if (runtime < settings.RuntimeMinMinutes || runtime > settings.RuntimeMaxMinutes)
        {
            erreurs.Add($"La durée du show ({runtime} min) doit rester entre {settings.RuntimeMinMinutes} et {settings.RuntimeMaxMinutes} minutes.");
        }
    }

    private static void ValiderPrixBillet(decimal prix, ShowSettings settings, List<string> erreurs)
    {
        if (settings.TicketPriceMin.HasValue && prix < settings.TicketPriceMin.Value)
        {
            erreurs.Add($"Le prix billet ({prix}) est inférieur au minimum autorisé ({settings.TicketPriceMin.Value}).");
        }

        if (settings.TicketPriceMax.HasValue && prix > settings.TicketPriceMax.Value)
        {
            erreurs.Add($"Le prix billet ({prix}) dépasse le maximum autorisé ({settings.TicketPriceMax.Value}).");
        }
    }

    private bool DetecterConflitCalendrier(string companyId, DateOnly date, string? showId)
    {
        return _store.ChargerCalendarEntries(companyId)
            .Any(entry => entry.Date == date && entry.ReferenceId != showId);
    }
}
