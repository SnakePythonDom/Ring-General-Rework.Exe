using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service pour planifier automatiquement les shows des compagnies IA
/// </summary>
public sealed class DailyShowSchedulerService
{
    private readonly ShowSchedulerService _showScheduler;
    private readonly IOwnerDecisionEngine? _ownerDecisionEngine;
    private readonly IGameRepository? _gameRepository;
    private readonly IShowDayOrchestrator? _showDayOrchestrator;
    private readonly IBookerAIEngine? _bookerAIEngine;

    public DailyShowSchedulerService(
        ShowSchedulerService showScheduler,
        IOwnerDecisionEngine? ownerDecisionEngine = null,
        IGameRepository? gameRepository = null,
        IShowDayOrchestrator? showDayOrchestrator = null,
        IBookerAIEngine? bookerAIEngine = null)
    {
        _showScheduler = showScheduler ?? throw new ArgumentNullException(nameof(showScheduler));
        _ownerDecisionEngine = ownerDecisionEngine;
        _gameRepository = gameRepository;
        _showDayOrchestrator = showDayOrchestrator;
        _bookerAIEngine = bookerAIEngine;
    }

    /// <summary>
    /// Planifie automatiquement les shows pour une compagnie IA sur une période donnée
    /// </summary>
    public void PlanifierShowsAutomatiques(
        string companyId,
        DateOnly startDate,
        int weeksAhead)
    {
        // Vérifier si compagnie joueur
        if (EstCompagnieJoueur(companyId))
        {
            return; // Pas de planification auto pour compagnie joueur
        }

        var ownerId = ObtenirOwnerId(companyId);
        if (ownerId == null)
        {
            return; // Pas d'owner, pas de planification
        }

        var frequency = _ownerDecisionEngine?.GetOptimalShowFrequency(ownerId, 1) ?? 4; // Default monthly
        var endDate = startDate.AddDays(weeksAhead * 7);
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            // Vérifier si show nécessaire selon fréquence
            if (ShouldCreateShow(currentDate, startDate, frequency))
            {
                var showType = DetermineShowType(currentDate, frequency);
                var result = _showScheduler.CreerShowRapide(companyId, showType, currentDate);

                if (result.Reussite && result.Show != null)
                {
                    // Booker IA génère automatiquement la carte si nécessaire
                    AutoBookShow(result.Show, companyId);
                }
            }

            currentDate = currentDate.AddDays(1);
        }
    }

    /// <summary>
    /// Vérifie si une compagnie est contrôlée par le joueur
    /// </summary>
    private bool EstCompagnieJoueur(string companyId)
    {
        return _gameRepository?.EstCompagnieJoueur(companyId) ?? false;
    }

    /// <summary>
    /// Obtient l'ID de l'owner d'une compagnie
    /// </summary>
    private string? ObtenirOwnerId(string companyId)
    {
        return _gameRepository?.ObtenirOwnerId(companyId);
    }

    /// <summary>
    /// Détermine si un show doit être créé à une date donnée selon la fréquence
    /// </summary>
    private static bool ShouldCreateShow(DateOnly currentDate, DateOnly startDate, int frequency)
    {
        var daysSinceStart = (currentDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;
        
        return frequency switch
        {
            1 => currentDate.DayOfWeek == DayOfWeek.Monday, // Weekly: chaque lundi
            2 => currentDate.DayOfWeek == DayOfWeek.Monday && daysSinceStart % 14 == 0, // BiWeekly: tous les 14 jours
            4 => currentDate.Day == DateTime.DaysInMonth(currentDate.Year, currentDate.Month), // Monthly: dernier jour du mois
            _ => false
        };
    }

    /// <summary>
    /// Détermine le type de show selon la date et la fréquence
    /// </summary>
    private static ShowType DetermineShowType(DateOnly date, int frequency)
    {
        // PPV généralement le dernier dimanche du mois
        if (date.DayOfWeek == DayOfWeek.Sunday && 
            date.Day > DateTime.DaysInMonth(date.Year, date.Month) - 7)
        {
            return ShowType.Ppv;
        }

        return frequency switch
        {
            1 => ShowType.Tv, // Weekly = TV show
            2 => ShowType.Tv, // BiWeekly = TV show
            4 => ShowType.Ppv, // Monthly = PPV
            _ => ShowType.Tv
        };
    }

    /// <summary>
    /// Génère automatiquement la carte d'un show pour une compagnie IA
    /// </summary>
    private void AutoBookShow(ShowSchedule show, string companyId)
    {
        if (_showDayOrchestrator == null || _bookerAIEngine == null)
            return;

        // Pour l'instant, on laisse le show en statut ABOOKER
        // Le booking automatique sera fait lors du Show Day selon le ControlLevel
        // Cette méthode peut être étendue pour générer la carte immédiatement si nécessaire
    }
}
