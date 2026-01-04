using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Data.Repositories;

public sealed class ImpactApplier : IImpactApplier
{
    private readonly GameRepository _repository;

    public ImpactApplier(GameRepository repository)
    {
        _repository = repository;
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
}
