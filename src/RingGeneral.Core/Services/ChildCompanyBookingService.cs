using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service pour gérer le contrôle de booking des child companies
/// </summary>
public sealed class ChildCompanyBookingService
{
    private readonly IChildCompanyBookingRepository _repository;
    private readonly DailyShowSchedulerService? _dailyShowScheduler;
    private readonly ShowSchedulerService? _showScheduler;
    private readonly IBookerAIEngine? _bookerAIEngine;

    public ChildCompanyBookingService(
        IChildCompanyBookingRepository repository,
        DailyShowSchedulerService? dailyShowScheduler = null,
        ShowSchedulerService? showScheduler = null,
        IBookerAIEngine? bookerAIEngine = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dailyShowScheduler = dailyShowScheduler;
        _showScheduler = showScheduler;
        _bookerAIEngine = bookerAIEngine;
    }

    /// <summary>
    /// Obtient le niveau de contrôle actuel pour une child company
    /// </summary>
    public BookingControlLevel GetBookingControlLevel(string childCompanyId)
    {
        var controle = _repository.ChargerControle(childCompanyId);
        return controle?.ControlLevel ?? BookingControlLevel.CoBooker; // Par défaut
    }

    /// <summary>
    /// Change le niveau de contrôle pour une child company
    /// </summary>
    public void SetBookingControlLevel(string childCompanyId, BookingControlLevel level)
    {
        _repository.MettreAJourNiveauControle(childCompanyId, level);
    }

    /// <summary>
    /// Obtient si la planification automatique est activée
    /// </summary>
    public bool GetAutoScheduleShows(string childCompanyId)
    {
        var controle = _repository.ChargerControle(childCompanyId);
        return controle?.AutoScheduleShows ?? false;
    }

    /// <summary>
    /// Active ou désactive la planification automatique
    /// </summary>
    public void SetAutoScheduleShows(string childCompanyId, bool enabled)
    {
        _repository.MettreAJourPlanificationAuto(childCompanyId, enabled);
    }

    /// <summary>
    /// Planifie les shows pour une child company selon ses paramètres
    /// </summary>
    public void PlanifierShowsChildCompany(string childCompanyId, DateOnly startDate, int daysAhead)
    {
        var controle = _repository.ChargerControle(childCompanyId);
        
        // Vérifier si planification automatique activée
        if (controle?.AutoScheduleShows != true)
        {
            return; // Planification automatique désactivée
        }

        // Vérifier HasFullAutonomy (doit être vérifié depuis ChildCompanyExtended)
        // Pour l'instant, on assume que si AutoScheduleShows = true, alors autonomie activée
        
        if (_dailyShowScheduler != null)
        {
            var weeksAhead = (int)Math.Ceiling(daysAhead / 7.0);
            _dailyShowScheduler.PlanifierShowsAutomatiques(childCompanyId, startDate, weeksAhead);
        }
    }

    /// <summary>
    /// Génère automatiquement la carte d'un show pour une child company selon son niveau de contrôle
    /// </summary>
    public IReadOnlyList<SegmentDefinition> GenererCarteShow(
        string childCompanyId,
        ShowContext context,
        List<SegmentDefinition>? existingSegments = null)
    {
        var controle = _repository.ChargerControle(childCompanyId);
        var controlLevel = controle?.ControlLevel ?? BookingControlLevel.CoBooker;

        if (_bookerAIEngine == null)
            return existingSegments ?? new List<SegmentDefinition>();

        // Obtenir le booker de la child company (simplifié: utiliser childCompanyId comme bookerId)
        var bookerId = ObtenirBookerId(childCompanyId);
        if (string.IsNullOrWhiteSpace(bookerId))
            return existingSegments ?? new List<SegmentDefinition>();

        return controlLevel switch
        {
            BookingControlLevel.Spectator => _bookerAIEngine.GenerateAutoBooking(
                bookerId, context, existingSegments, null),
            BookingControlLevel.Producer => _bookerAIEngine.GenerateAutoBooking(
                bookerId, context, existingSegments, null),
            BookingControlLevel.CoBooker => GenererCarteCoBooker(
                bookerId, context, existingSegments),
            BookingControlLevel.Dictator => existingSegments ?? new List<SegmentDefinition>(),
            _ => existingSegments ?? new List<SegmentDefinition>()
        };
    }

    /// <summary>
    /// Génère la carte en mode CoBooker (joueur crée main event, IA complète midcard)
    /// </summary>
    private List<SegmentDefinition> GenererCarteCoBooker(
        string bookerId,
        ShowContext context,
        List<SegmentDefinition>? existingSegments)
    {
        if (_bookerAIEngine == null)
            return existingSegments ?? new List<SegmentDefinition>();

        existingSegments ??= new List<SegmentDefinition>();

        // Vérifier si le joueur a déjà créé des segments pour titres majeurs
        var playerMainEventSegments = existingSegments.Where(s => s.EstMainEvent || s.TitreId != null).ToList();
        var playerMidcardSegments = existingSegments.Except(playerMainEventSegments).ToList();

        // Si le joueur a déjà créé le main event, l'IA développe seulement la midcard
        if (playerMainEventSegments.Any())
        {
            var constraints = new AutoBookingConstraints
            {
                RequireMainEvent = false, // Pas de main event nécessaire
                MaxSegments = Math.Max(4, 8 - playerMainEventSegments.Count)
            };

            // Passer tous les segments existants (y compris main event) pour que l'IA
            // puisse éviter de réutiliser les workers déjà utilisés dans les segments du joueur
            var aiSegments = _bookerAIEngine.GenerateAutoBooking(
                bookerId,
                context,
                existingSegments,
                constraints);

            return playerMainEventSegments.Concat(aiSegments).ToList();
        }

        // Si le joueur n'a pas créé de main event, l'IA peut le proposer
        return _bookerAIEngine.GenerateAutoBooking(bookerId, context, existingSegments, null);
    }

    /// <summary>
    /// Obtient l'ID du booker d'une child company
    /// </summary>
    private string? ObtenirBookerId(string childCompanyId)
    {
        // TODO: Implémenter récupération du booker depuis ChildCompanyExtended
        // Pour l'instant, retourner childCompanyId comme bookerId (simplifié)
        return childCompanyId;
    }
}
