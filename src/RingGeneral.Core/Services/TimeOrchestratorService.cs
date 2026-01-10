using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

/// <summary>
/// Orchestre le passage du temps jour par jour (remplace WeeklyLoopService)
/// Gère UNIQUEMENT le paiement mensuel garanti (fin du mois)
/// Les frais d'apparition sont gérés séparément par ShowDayOrchestrator
/// </summary>
public sealed class TimeOrchestratorService
{
    private readonly IGameRepository _repository;
    private readonly IDailyStatUpdateService? _statUpdateService;
    private readonly IEventGeneratorService? _eventGenerator;
    private readonly ShowDayOrchestrator? _showDayOrchestrator;
    private readonly IDailyFinanceService? _financeService;

    public TimeOrchestratorService(
        IGameRepository repository,
        IDailyStatUpdateService? statUpdateService = null,
        IEventGeneratorService? eventGenerator = null,
        ShowDayOrchestrator? showDayOrchestrator = null,
        IDailyFinanceService? financeService = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _statUpdateService = statUpdateService;
        _eventGenerator = eventGenerator;
        _showDayOrchestrator = showDayOrchestrator;
        _financeService = financeService;
    }

    /// <summary>
    /// Avance d'un jour dans le jeu
    /// </summary>
    public DailyTickResult PasserJourSuivant(string companyId)
    {
        // 1. Incrémenter le jour
        var newDay = _repository.IncrementerJour(companyId);
        var currentDate = _repository.GetCurrentDate(companyId);

        // 2. Mise à jour des états (fatigue, blessures)
        _statUpdateService?.UpdateDailyStats(companyId, newDay);

        // 3. Génération d'événements aléatoires
        var events = _eventGenerator?.GenerateDailyEvents(companyId, newDay) ?? [];

        // 4. Vérifier si c'est un jour de show
        // IMPORTANT : Les frais d'apparition seront traités par ShowDayOrchestrator
        // après la simulation, PAS ici dans TimeOrchestratorService
        var showDetection = _showDayOrchestrator?.DetecterShowAVenir(companyId, newDay);
        if (showDetection?.ShowDetecte == true)
        {
            // Le show sera simulé manuellement par le joueur
            // Les frais d'apparition seront traités par ShowDayOrchestrator.ExecuterFluxComplet()
        }

        // 5. Vérifier si c'est la fin du mois (UNIQUEMENT pour paiement mensuel garanti)
        // ATTENTION : TimeOrchestratorService ne gère QUE le paiement mensuel fixe
        // Les frais d'apparition sont gérés séparément par ShowDayOrchestrator
        var eventsList = events.ToList();
        if (EstFinDuMois(currentDate))
        {
            _financeService?.ProcessMonthlyPayroll(companyId, currentDate);
            eventsList.Add($"💰 Paiements mensuels effectués pour {currentDate:MMMM yyyy}");
        }

        return new DailyTickResult(newDay, currentDate, eventsList);
    }

    private static bool EstFinDuMois(DateTime date)
    {
        return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
    }
}
