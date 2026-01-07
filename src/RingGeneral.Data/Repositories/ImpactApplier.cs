using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Medical;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;

namespace RingGeneral.Data.Repositories;

public sealed class ImpactApplier : IImpactApplier
{
    private readonly GameRepository _repository;
    private readonly MedicalRepository _medicalRepository;
    private readonly InjuryService _injuryService;
    private readonly TitleService? _titleService;

    public ImpactApplier(
        GameRepository repository,
        MedicalRepository medicalRepository,
        InjuryService injuryService,
        TitleService? titleService = null)
    {
        _repository = repository;
        _medicalRepository = medicalRepository;
        _injuryService = injuryService;
        _titleService = titleService;
    }

    public ImpactReport AppliquerImpacts(ImpactContext context)
    {
        if (context.RapportShow is not null)
        {
            _repository.EnregistrerRapport(context.RapportShow);
        }

        // Traiter les changements de titres AVANT d'appliquer le delta général
        // Cela permet de mettre à jour les champions et le prestige des titres
        var changementsTitres = TraiterChangementsTitres(context);

        if (context.Delta is not null)
        {
            _repository.AppliquerDelta(context.ShowId, context.Delta);
        }

        var changements = new List<string>();

        // Ajouter les changements de titres en premier
        changements.AddRange(changementsTitres);

        if (context.Delta is not null)
        {
            EnregistrerBlessures(context.ShowId, context.Delta.Blessures);
            foreach (var (workerId, fatigue) in context.Delta.FatigueDelta)
            {
                changements.Add($"Fatigue {workerId}: {fatigue:+#;-#;0}");
            }

            foreach (var (workerId, blessure) in context.Delta.Blessures)
            {
                changements.Add($"Blessure {workerId}: {blessure}");
            }

            foreach (var (workerId, momentum) in context.Delta.MomentumDelta)
            {
                changements.Add($"Momentum {workerId}: {momentum:+#;-#;0}");
            }

            foreach (var (workerId, popularite) in context.Delta.PopulariteWorkersDelta)
            {
                changements.Add($"Popularité {workerId}: {popularite:+#;-#;0}");
            }

            foreach (var (compagnieId, popularite) in context.Delta.PopulariteCompagnieDelta)
            {
                changements.Add($"Popularité compagnie {compagnieId}: {popularite:+#;-#;0}");
            }

            foreach (var (storylineId, heat) in context.Delta.StorylineHeatDelta)
            {
                changements.Add($"Storyline {storylineId}: {heat:+#;-#;0}");
            }

            foreach (var (titreId, prestige) in context.Delta.TitrePrestigeDelta)
            {
                changements.Add($"Prestige {titreId}: {prestige:+#;-#;0}");
            }

            foreach (var transaction in context.Delta.Finances)
            {
                changements.Add($"Finance {transaction.Libelle}: {transaction.Montant:+#;-#;0}");
            }
        }

        var resume = context.RapportShow is null
            ? "Impacts appliqués."
            : $"Impacts appliqués pour le show {context.RapportShow.ShowId}.";

        return new ImpactReport(changements, resume);
    }

    /// <summary>
    /// Traite les changements de titres pour tous les segments avec un titre
    /// Retourne une liste de messages décrivant les changements
    /// </summary>
    private List<string> TraiterChangementsTitres(ImpactContext context)
    {
        var changements = new List<string>();

        if (_titleService is null || context.RapportShow is null)
        {
            return changements;
        }

        // Charger le contexte du show pour avoir accès aux segments
        var showContext = _repository.ChargerShowContext(context.ShowId);
        if (showContext is null)
        {
            return changements;
        }

        foreach (var segmentReport in context.RapportShow.Segments)
        {
            var segmentDef = showContext.Segments.FirstOrDefault(s => s.SegmentId == segmentReport.SegmentId);
            if (segmentDef is null || segmentDef.TitreId is null || segmentDef.VainqueurId is null)
            {
                continue;
            }

            var titre = showContext.Titres.FirstOrDefault(t => t.TitreId == segmentDef.TitreId);
            if (titre is null)
            {
                continue;
            }

            var championActuel = titre.HolderWorkerId;
            var challengerId = segmentDef.Participants
                .FirstOrDefault(p => p != segmentDef.VainqueurId);

            var semaine = showContext.Show.Semaine;
            var input = new TitleMatchInput(
                segmentDef.TitreId,
                context.ShowId,
                semaine,
                championActuel,
                challengerId,
                segmentDef.VainqueurId);

            try
            {
                var outcome = _titleService.EnregistrerMatch(input);

                if (outcome.TitleChange)
                {
                    var ancienNom = championActuel is not null
                        ? showContext.Workers.FirstOrDefault(w => w.WorkerId == championActuel)?.NomComplet ?? "Vacant"
                        : "Vacant";
                    var nouveauNom = showContext.Workers
                        .FirstOrDefault(w => w.WorkerId == segmentDef.VainqueurId)?.NomComplet ?? "Unknown";

                    changements.Add($"🏆 TITLE CHANGE: {nouveauNom} remporte le {titre.Nom} (Prestige {outcome.PrestigeDelta:+#;-#;0})");
                }
                else
                {
                    var championNom = showContext.Workers
                        .FirstOrDefault(w => w.WorkerId == championActuel)?.NomComplet ?? championActuel;
                    changements.Add($"✓ {championNom} conserve le {titre.Nom} (Prestige {outcome.PrestigeDelta:+#;-#;0})");
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine($"[ImpactApplier] Erreur lors du traitement du titre {segmentDef.TitreId}: {ex.Message}");
                changements.Add($"⚠️ Erreur lors du traitement du titre {titre.Nom}");
            }
        }

        return changements;
    }

    private void EnregistrerBlessures(string showId, IReadOnlyDictionary<string, string> blessures)
    {
        if (blessures.Count == 0)
        {
            return;
        }

        var semaine = _repository.ChargerSemaineShow(showId);
        var noms = _repository.ChargerNomsWorkers();

        foreach (var (workerId, statut) in blessures)
        {
            var fatigue = _repository.ChargerFatigueWorker(workerId);
            var severite = _injuryService.ParserSeverite(statut);
            var result = _injuryService.AppliquerBlessure(workerId, "Match", severite, semaine, fatigue, "Blessure détectée pendant le show.");
            var injuryId = _medicalRepository.AjouterBlessure(result.Blessure);
            var plan = result.Plan with { InjuryId = injuryId };
            _medicalRepository.AjouterPlanRecuperation(plan);
            _medicalRepository.AjouterNoteMedicale(new MedicalNote(0, injuryId, workerId, result.Recommendation.Message, DateTimeOffset.UtcNow));

            var nom = noms.TryGetValue(workerId, out var workerNom) ? workerNom : workerId;
            var contenu = $"{nom} est blessé ({statut}). {result.Recommendation.Message} Retour visé S{plan.TargetWeek}.";
            _repository.AjouterInboxItem(new InboxItem("blessures", "Blessure signalée", contenu, semaine));
        }
    }
}
