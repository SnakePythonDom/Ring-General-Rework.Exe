using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Owner;

namespace RingGeneral.Core.Services;

/// <summary>
/// Orchestre le passage du temps jour par jour (remplace WeeklyLoopService)
/// Gère UNIQUEMENT le paiement mensuel garanti (fin du mois)
/// Les frais d'apparition sont gérés séparément par ShowDayOrchestrator
/// </summary>
public sealed class TimeOrchestratorService : ITimeOrchestratorService
{
    private readonly IGameRepository _repository;
    private readonly IDailyServices? _dailyServices;
    private readonly IEventGeneratorService? _eventGenerator;
    private readonly IShowDayOrchestrator? _showDayOrchestrator;
    private readonly DailyShowSchedulerService? _dailyShowScheduler;
    private readonly IOwnerDecisionEngine? _ownerDecisionEngine;
    private readonly IRelationshipEvolutionService? _relationshipEvolution;
    private readonly OwnerGoalGenerator _goalGenerator = new();

    public TimeOrchestratorService(
        IGameRepository repository,
        IDailyServices? dailyServices = null,
        IEventGeneratorService? eventGenerator = null,
        IShowDayOrchestrator? showDayOrchestrator = null,
        DailyShowSchedulerService? dailyShowScheduler = null,
        IOwnerDecisionEngine? ownerDecisionEngine = null,
        IRelationshipEvolutionService? relationshipEvolution = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dailyServices = dailyServices;
        _eventGenerator = eventGenerator;
        _showDayOrchestrator = showDayOrchestrator;
        _dailyShowScheduler = dailyShowScheduler;
        _ownerDecisionEngine = ownerDecisionEngine;
        _relationshipEvolution = relationshipEvolution;
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
        _dailyServices?.UpdateDailyStats(companyId, newDay);

        // 3. Gestion des Objectifs (Directives)
        if (_ownerDecisionEngine != null)
        {
            var owner = _ownerDecisionEngine.GetOwnerByCompanyId(companyId);
            if (owner != null)
            {
                // Évaluer les objectifs existants
                _ownerDecisionEngine.EvaluateGoals(owner.OwnerId, currentDate);

                // Générer un nouvel objectif si besoin (ex: chaque lundi)
                if (currentDate.DayOfWeek == DayOfWeek.Monday)
                {
                    var activeGoals = _ownerDecisionEngine.GetGoals(owner.OwnerId);
                    if (!activeGoals.Any(g => g.Status == GoalStatus.Active))
                    {
                        var newGoal = _goalGenerator.GenerateGoal(owner, currentDate);
                        _ownerDecisionEngine.SetGoal(owner.OwnerId, newGoal);
                    }
                }
            }
        }

        // 4. Evolution Sociale (chaque lundi)
        if (currentDate.DayOfWeek == DayOfWeek.Monday && _relationshipEvolution != null)
        {
            // Note: Normalement on passerait par une méthode async, mais ici on est dans un flux synchrone
            // On le lance en Task.Run ou on change l'interface si besoin.
            // Pour l'instant on garde la cohérence avec le reste du service qui est synchrone (malheureusement)
            Task.Run(async () => await _relationshipEvolution.ProcessWeeklyEvolutionAsync(newDay / 7, currentDate)).Wait();
        }

        // 5. Planification automatique des shows pour compagnies IA
        // Planifier pour les 8 prochaines semaines si on avance significativement
        // (ex: tous les 30 jours ou au démarrage)
        if (_dailyShowScheduler != null && (newDay % 30 == 0 || newDay == 1))
        {
            try
            {
                var startDate = DateOnly.FromDateTime(currentDate);
                _dailyShowScheduler.PlanifierShowsAutomatiques(companyId, startDate, 8);
            }
            catch
            {
                // Ignorer les erreurs de planification pour ne pas bloquer le jeu
            }
        }

        // 5. Génération d'événements aléatoires
        var events = _eventGenerator?.GenerateDailyEvents(companyId, newDay) ?? [];

        // 6. Vérifier si c'est un jour de show
        var showDetection = _showDayOrchestrator?.DetecterShowAVenir(companyId, newDay);
        if (showDetection?.ShowDetecte == true)
        {
            // Le show sera simulé manuellement par le joueur
        }

        // 7. Vérifier si c'est la fin du mois
        var eventsList = events.ToList();
        if (EstFinDuMois(currentDate))
        {
            _dailyServices?.ProcessMonthlyPayroll(companyId, currentDate);
            eventsList.Add($"💰 Paiements mensuels effectués pour {currentDate:MMMM yyyy}");
        }

        return new DailyTickResult(newDay, currentDate, eventsList);
    }

    private static bool EstFinDuMois(DateTime date)
    {
        return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
    }
}
