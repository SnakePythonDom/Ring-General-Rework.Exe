using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;

namespace RingGeneral.Core.Services;

/// <summary>
/// Orchestre le flux "Show Day" (Match Day).
/// Gère la détection d'événements, la simulation et l'application des impacts.
/// </summary>
public sealed class ShowDayOrchestrator
{
    private readonly IShowSchedulerStore? _showScheduler;
    private readonly TitleService? _titleService;
    private readonly IBookerAIEngine? _bookerAIEngine;
    private readonly IRandomProvider _random;
    private readonly IImpactApplier? _impactApplier;
    private readonly IMoraleEngine? _moraleEngine;
    private readonly Func<string, ShowContext?>? _contextLoader;
    private readonly Action<string, ShowStatus>? _statusUpdater;

    public ShowDayOrchestrator(
        IShowSchedulerStore? showScheduler = null,
        TitleService? titleService = null,
        IRandomProvider? random = null,
        IBookerAIEngine? bookerAIEngine = null,
        IImpactApplier? impactApplier = null,
        IMoraleEngine? moraleEngine = null,
        Func<string, ShowContext?>? contextLoader = null,
        Action<string, ShowStatus>? statusUpdater = null)
    {
        _showScheduler = showScheduler;
        _titleService = titleService;
        _bookerAIEngine = bookerAIEngine;
        _random = random ?? new SeededRandomProvider((int)DateTime.Now.Ticks);
        _impactApplier = impactApplier;
        _moraleEngine = moraleEngine;
        _contextLoader = contextLoader;
        _statusUpdater = statusUpdater;
    }

    /// <summary>
    /// Vérifie s'il existe un événement (show) planifié à la semaine spécifiée
    /// </summary>
    public ShowDayDetectionResult DetecterShowAVenir(string companyId, int currentWeek)
    {
        if (_showScheduler is null)
        {
            return new ShowDayDetectionResult(false, null, "Scheduler non disponible");
        }

        var shows = _showScheduler.ChargerShows(companyId);
        var show = shows.FirstOrDefault(s => s.Statut == ShowStatus.ABooker);

        if (show is null)
        {
            return new ShowDayDetectionResult(false, null, "Aucun show prévu cette semaine");
        }

        return new ShowDayDetectionResult(true, show, $"Show '{show.Nom}' prévu");
    }

    /// <summary>
    /// Exécute la simulation complète d'un show et retourne le résultat
    /// </summary>
    public ShowSimulationResult SimulerShow(ShowContext context)
    {
        var seed = HashCode.Combine(context.Show.ShowId, context.Show.Semaine);
        var engine = new ShowSimulationEngine(new SeededRandomProvider(seed));
        return engine.Simuler(context);
    }

    /// <summary>
    /// Génère automatiquement un booking pour une compagnie IA (non-joueur).
    /// Le Booker IA crée une carte complète basée sur ses préférences et mémoires.
    /// </summary>
    /// <param name="bookerId">ID du booker de la compagnie</param>
    /// <param name="context">Contexte du show à booker</param>
    /// <param name="isPlayerCompany">True si la compagnie est contrôlée par le joueur</param>
    /// <returns>Liste des segments générés automatiquement</returns>
    public List<SegmentDefinition> GenerateAICompanyBooking(
        string bookerId,
        ShowContext context,
        bool isPlayerCompany)
    {
        // Si c'est la compagnie du joueur, on ne génère pas automatiquement
        if (isPlayerCompany)
        {
            return new List<SegmentDefinition>();
        }

        // Si le BookerAIEngine n'est pas disponible, retourner vide
        if (_bookerAIEngine is null)
        {
            return new List<SegmentDefinition>();
        }

        // Contraintes par défaut pour les compagnies IA
        var constraints = new AutoBookingConstraints
        {
            ForbidInjuredWorkers = true,
            MaxFatigueLevel = 85, // Les IA peuvent pousser un peu plus
            MinSegments = 5,
            MaxSegments = 8,
            ForbidMultipleAppearances = true,
            PrioritizeActiveStorylines = true,
            UseTitles = true,
            RequireMainEvent = true,
            TargetDuration = context.Show.DureeMinutes
        };

        // Générer le booking automatiquement
        return _bookerAIEngine.GenerateAutoBooking(
            bookerId,
            context,
            existingSegments: null, // Les IA partent de zéro
            constraints: constraints
        );
    }

    /// <summary>
    /// Finalise un show en appliquant tous les impacts :
    /// - Finances (via GameStateDelta)
    /// - Moral/Popularité (via GameStateDelta)
    /// - Titres (via TitleService si isTitleMatch)
    /// - Blessures (via ImpactApplier)
    /// </summary>
    public ShowDayFinalizationResult FinaliserShow(ShowSimulationResult resultat, ShowContext context)
    {
        var changements = new List<string>();
        var titresChanges = new List<TitleChangeInfo>();

        // 1. Traiter les changements de titres
        if (_titleService is not null)
        {
            foreach (var segment in context.Segments.Where(s => !string.IsNullOrWhiteSpace(s.TitreId)))
            {
                var segmentReport = resultat.RapportShow.Segments
                    .FirstOrDefault(r => r.SegmentId == segment.SegmentId);

                if (segmentReport is not null && !string.IsNullOrWhiteSpace(segment.VainqueurId))
                {
                    var titreId = segment.TitreId;
                    if (string.IsNullOrWhiteSpace(titreId))
                    {
                        continue;
                    }

                    var titre = context.Titres.FirstOrDefault(t => t.TitreId == titreId);
                    if (titre is null)
                    {
                        continue;
                    }

                    var championActuel = titre.DetenteurId;
                    var challengerId = segment.Participants
                        .FirstOrDefault(p => p != segment.VainqueurId);

                    var input = new TitleMatchInput(
                        titreId,
                        challengerId ?? string.Empty,
                        segment.VainqueurId,
                        context.Show.Semaine,
                        championActuel,
                        context.Show.ShowId);

                    var outcome = _titleService.EnregistrerMatch(input);

                    if (outcome.TitleChanged)
                    {
                        var ancienChampion = context.Workers
                            .FirstOrDefault(w => w.WorkerId == championActuel);
                        var nouveauChampion = context.Workers
                            .FirstOrDefault(w => w.WorkerId == segment.VainqueurId);

                        titresChanges.Add(new TitleChangeInfo(
                            segment.TitreId,
                            titre.Nom,
                            ancienChampion?.NomComplet ?? "Vacant",
                            nouveauChampion?.NomComplet ?? "Unknown",
                            outcome.PrestigeDelta));

                        changements.Add(
                            $"🏆 TITLE CHANGE: {nouveauChampion?.NomComplet} remporte le {titre.Nom}");
                    }
                    else
                    {
                        changements.Add(
                            $"✓ {championActuel} conserve le {titre.Nom} (Prestige {outcome.PrestigeDelta:+#;-#;0})");
                    }
                }
            }
        }

        // 2. Les finances, popularité, moral, fatigue sont gérés par GameStateDelta
        // Ces impacts seront appliqués par ImpactApplier plus tard

        // 3. Construire le résumé
        var finances = resultat.RapportShow.Billetterie + resultat.RapportShow.Merch + resultat.RapportShow.Tv;
        changements.Insert(0, $"💰 Revenus totaux: ${finances:N2}");
        changements.Insert(0, $"📊 Note du show: {resultat.RapportShow.NoteGlobale}/100");
        changements.Insert(0, $"👥 Audience: {resultat.RapportShow.Audience}");

        return new ShowDayFinalizationResult(
            true,
            changements,
            titresChanges,
            resultat.Delta);
    }

    /// <summary>
    /// Exécute le flux complet Show Day :
    /// 1. Charge le ShowContext
    /// 2. Simule le show
    /// 3. Applique les impacts (finances, titres, blessures, popularité, etc.)
    /// 4. Gère le moral post-show (workers non utilisés)
    /// 5. Met à jour le statut du show
    /// </summary>
    public ShowDayFluxCompletResult ExecuterFluxComplet(string showId, string companyId)
    {
        var erreurs = new List<string>();
        var changements = new List<string>();

        // 1. Charger le ShowContext
        if (_contextLoader is null)
        {
            erreurs.Add("Le chargeur de contexte n'est pas configuré.");
            return new ShowDayFluxCompletResult(false, erreurs, changements, null);
        }

        var context = _contextLoader(showId);
        if (context is null)
        {
            erreurs.Add($"Impossible de charger le contexte pour le show {showId}.");
            return new ShowDayFluxCompletResult(false, erreurs, changements, null);
        }

        // 2. Simuler le show
        changements.Add($"🎬 Début de la simulation : {context.Show.Nom}");
        var resultatSimulation = SimulerShow(context);
        changements.Add($"📊 Note globale : {resultatSimulation.RapportShow.NoteGlobale}/100");
        changements.Add($"👥 Audience : {resultatSimulation.RapportShow.Audience}");

        // 3. Appliquer les impacts via ImpactApplier
        if (_impactApplier is not null)
        {
            var impactContext = new ImpactContext(
                showId,
                resultatSimulation.RapportShow.Segments.Select(s => new SegmentResult(s.SegmentId, s.Note, "", s)).ToList(),
                context.Storylines.Select(s => s.StorylineId).ToList(),
                resultatSimulation.RapportShow,
                resultatSimulation.Delta);

            var impactReport = _impactApplier.AppliquerImpacts(impactContext);
            changements.AddRange(impactReport.Changements);
        }
        else
        {
            changements.Add("⚠️ ImpactApplier non configuré, impacts non appliqués.");
        }

        // 4. Gérer le moral post-show (workers non utilisés)
        if (_moraleEngine is not null)
        {
            var workersUtilises = context.Segments
                .SelectMany(s => s.Participants)
                .Distinct()
                .ToHashSet();

            var workersNonUtilises = context.Workers
                .Where(w => !workersUtilises.Contains(w.WorkerId))
                .ToList();

            foreach (var worker in workersNonUtilises)
            {
                // Impact négatif sur le moral des workers non utilisés
                _moraleEngine.UpdateMorale(worker.WorkerId, "NotBooked", impact: -3);
                changements.Add($"📉 {worker.NomComplet} : Moral -3 (non utilisé dans le show)");
            }

            // Recalculer le moral de la compagnie
            _moraleEngine.CalculateCompanyMorale(companyId);
        }

        // 5. Mettre à jour le statut du show
        if (_statusUpdater is not null)
        {
            _statusUpdater(showId, ShowStatus.Simule);
            changements.Add($"✅ Show marqué comme SIMULÉ");
        }

        changements.Add($"🎉 Simulation terminée avec succès !");

        return new ShowDayFluxCompletResult(true, erreurs, changements, resultatSimulation.RapportShow);
    }
}

/// <summary>
/// Résultat de la détection d'un show
/// </summary>
public sealed record ShowDayDetectionResult(
    bool ShowDetecte,
    ShowSchedule? Show,
    string Message);

/// <summary>
/// Résultat de la finalisation d'un show
/// </summary>
public sealed record ShowDayFinalizationResult(
    bool Succes,
    IReadOnlyList<string> Changements,
    IReadOnlyList<TitleChangeInfo> TitresChanges,
    GameStateDelta? Delta);

/// <summary>
/// Information sur un changement de titre
/// </summary>
public sealed record TitleChangeInfo(
    string TitreId,
    string TitreNom,
    string AncienChampion,
    string NouveauChampion,
    int PrestigeDelta);

/// <summary>
/// Résultat du flux complet Show Day
/// </summary>
public sealed record ShowDayFluxCompletResult(
    bool Succes,
    IReadOnlyList<string> Erreurs,
    IReadOnlyList<string> Changements,
    ShowReport? Rapport);
