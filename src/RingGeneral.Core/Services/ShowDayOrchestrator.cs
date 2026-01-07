using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
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
    private readonly IRandomProvider _random;

    public ShowDayOrchestrator(
        IShowSchedulerStore? showScheduler = null,
        TitleService? titleService = null,
        IRandomProvider? random = null)
    {
        _showScheduler = showScheduler;
        _titleService = titleService;
        _random = random ?? new SeededRandomProvider((int)DateTime.Now.Ticks);
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
            foreach (var segment in context.Segments.Where(s => s.TitreId is not null))
            {
                var segmentReport = resultat.RapportShow.Segments
                    .FirstOrDefault(r => r.SegmentId == segment.SegmentId);

                if (segmentReport is not null && !string.IsNullOrWhiteSpace(segment.VainqueurId))
                {
                    var titre = context.Titres.FirstOrDefault(t => t.TitreId == segment.TitreId);
                    if (titre is null)
                    {
                        continue;
                    }

                    var championActuel = titre.DetenteurId;
                    var challengerId = segment.Participants
                        .FirstOrDefault(p => p != segment.VainqueurId);

                    var input = new TitleMatchInput(
                        segment.TitreId,
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
