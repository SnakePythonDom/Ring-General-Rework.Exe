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

        if (DetecterConflitCalendrier(draft.CompanyId, draft.Date, null, draft.BrandId))
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
            ShowStatus.ABooker,
            draft.BrandId);

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

    private bool DetecterConflitCalendrier(string companyId, DateOnly date, string? showId, string? brandId = null)
    {
        var shows = _store.ChargerShowsParDate(companyId, date);
        
        if (!shows.Any())
            return false;
        
        // Si brandId fourni, vérifier conflit avec même brand
        if (brandId != null)
        {
            return shows.Any(s => s.BrandId == brandId && s.ShowId != showId);
        }
        
        // Sinon, vérifier conflit avec même compagnie sans brand
        return shows.Any(s => s.CompanyId == companyId && s.BrandId == null && s.ShowId != showId);
    }

    /// <summary>
    /// Crée un show rapidement avec des valeurs par défaut selon le type
    /// </summary>
    public ShowSchedulerResult CreerShowRapide(
        string companyId,
        ShowType type,
        DateOnly date,
        string? brandId = null)
    {
        var defaults = GetDefaultValuesForType(type);
        
        var draft = new ShowScheduleDraft(
            companyId,
            GenerateDefaultName(type, date),
            type,
            date,
            defaults.Duration,
            defaults.VenueId,
            defaults.Broadcast,
            defaults.TicketPrice,
            brandId);
        
        // Vérifier conflits (basé sur BrandId)
        if (DetecterConflitCalendrier(companyId, date, null, brandId))
        {
            return new ShowSchedulerResult(
                false,
                new[] { "Conflit de calendrier détecté pour cette date." },
                null, null, null);
        }
        
        return CreerShow(draft);
    }

    /// <summary>
    /// Crée plusieurs shows récurrents à partir d'un template
    /// </summary>
    public IReadOnlyList<ShowSchedulerResult> CreerShowRecurrent(
        ShowTemplate template,
        DateOnly startDate,
        int numberOfOccurrences)
    {
        var results = new List<ShowSchedulerResult>();
        var currentDate = startDate;
        
        // Obtenir les valeurs par défaut pour le type de show, notamment le prix du ticket
        var defaults = GetDefaultValuesForType(template.ShowType);
        
        for (int i = 0; i < numberOfOccurrences; i++)
        {
            var draft = new ShowScheduleDraft(
                template.CompanyId,
                template.Name,
                template.ShowType,
                currentDate,
                template.DefaultDuration,
                template.DefaultVenueId,
                template.DefaultBroadcast,
                defaults.TicketPrice);
            
            var result = CreerShow(draft);
            results.Add(result);
            
            // Calculer prochaine date selon pattern
            currentDate = CalculateNextDate(currentDate, template.RecurrencePattern, template.DayOfWeek);
        }
        
        return results;
    }

    /// <summary>
    /// Détecte tous les shows d'un jour spécifique
    /// </summary>
    public IReadOnlyList<ShowSchedule> DetecterShowsDuJour(string companyId, DateOnly date)
    {
        return _store.ChargerShowsParDate(companyId, date);
    }

    private static (int Duration, string? VenueId, string Broadcast, decimal TicketPrice) GetDefaultValuesForType(ShowType type)
    {
        return type switch
        {
            ShowType.Tv => (120, null, "TBA", 25m),
            ShowType.Ppv => (180, null, "Pay-Per-View", 50m),
            ShowType.House => (90, null, "", 20m),
            ShowType.Youth => (60, null, "", 10m),
            _ => (120, null, "TBA", 25m)
        };
    }

    private static string GenerateDefaultName(ShowType type, DateOnly date)
    {
        var typeName = type switch
        {
            ShowType.Tv => "TV Show",
            ShowType.Ppv => "PPV",
            ShowType.House => "House Show",
            ShowType.Youth => "Youth Show",
            _ => "Show"
        };
        
        return $"{typeName} - {date:dd/MM/yyyy}";
    }

    private static DateOnly CalculateNextDate(DateOnly currentDate, ShowRecurrencePattern pattern, DayOfWeek? dayOfWeek)
    {
        return pattern switch
        {
            ShowRecurrencePattern.Weekly => dayOfWeek.HasValue 
                ? GetNextWeekday(currentDate, dayOfWeek.Value)
                : currentDate.AddDays(7),
            ShowRecurrencePattern.BiWeekly => dayOfWeek.HasValue
                ? GetNextWeekday(currentDate.AddDays(7), dayOfWeek.Value)
                : currentDate.AddDays(14),
            ShowRecurrencePattern.Monthly => currentDate.AddMonths(1),
            _ => currentDate.AddDays(7)
        };
    }

    private static DateOnly GetNextWeekday(DateOnly date, DayOfWeek targetDay)
    {
        var daysUntilTarget = ((int)targetDay - (int)date.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0) daysUntilTarget = 7; // Si c'est le même jour, prendre la semaine suivante
        return date.AddDays(daysUntilTarget);
    }
}
