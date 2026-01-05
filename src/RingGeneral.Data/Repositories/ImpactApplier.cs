using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Medical;
using RingGeneral.Core.Models;

namespace RingGeneral.Data.Repositories;

public sealed class ImpactApplier : IImpactApplier
{
    private readonly GameRepository _repository;
    private readonly MedicalRepository _medicalRepository;
    private readonly InjuryService _injuryService;

    public ImpactApplier(GameRepository repository, MedicalRepository medicalRepository, InjuryService injuryService)
    {
        _repository = repository;
        _medicalRepository = medicalRepository;
        _injuryService = injuryService;
    }

    public ImpactReport AppliquerImpacts(ImpactContext context)
    {
        if (context.RapportShow is not null)
        {
            _repository.EnregistrerRapport(context.RapportShow);
        }

        if (context.Delta is not null)
        {
            _repository.AppliquerDelta(context.ShowId, context.Delta);
        }

        var changements = new List<string>();
        if (context.Delta is not null)
        {
            EnregistrerBlessures(context.ShowId, context.Delta.Blessures);
            foreach (var (workerId, fatigue) in context.Delta.FatigueDelta)
            {
                changements.Add($"Fatigue {workerId}: {fatigue:+#;-#;0}");
            }

            foreach (var (workerId, momentum) in context.Delta.MomentumDelta)
            {
                changements.Add($"Momentum {workerId}: {momentum:+#;-#;0}");
            }

            foreach (var (workerId, popularite) in context.Delta.PopulariteWorkersDelta)
            {
                changements.Add($"Popularité {workerId}: {popularite:+#;-#;0}");
            }

            foreach (var (storylineId, heat) in context.Delta.StorylineHeatDelta)
            {
                changements.Add($"Storyline {storylineId}: {heat:+#;-#;0}");
            }

            foreach (var (titreId, prestige) in context.Delta.TitrePrestigeDelta)
            {
                changements.Add($"Prestige {titreId}: {prestige:+#;-#;0}");
            }
        }

        var resume = context.RapportShow is null
            ? "Impacts appliqués."
            : $"Impacts appliqués pour le show {context.RapportShow.ShowId}.";

        return new ImpactReport(changements, resume);
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
