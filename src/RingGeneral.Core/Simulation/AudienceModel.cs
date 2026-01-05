using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class AudienceModel
{
    public AudienceResult Calculer(CompanyState compagnie, int showScore, int stars, int reachBonus, int audienceCap)
    {
        var reachScore = Math.Clamp(compagnie.Reach * 10 + reachBonus, 0, 100);
        var saturationImpact = Math.Clamp((compagnie.AudienceMoyenne - showScore) / 4, -8, 8);

        var audienceBrute = (reachScore * 0.35) + (showScore * 0.45) + (stars * 0.20) - saturationImpact;
        var audience = (int)Math.Round(Math.Clamp(audienceBrute, 0, 100));
        if (audienceCap > 0)
        {
            audience = Math.Min(audience, audienceCap);
        }

        return new AudienceResult(audience, reachScore, showScore, stars, saturationImpact, audienceCap);
    }

    public int CalculerStars(IReadOnlyList<WorkerSnapshot> participants)
    {
        if (participants.Count == 0)
        {
            return 0;
        }

        var top = participants
            .OrderByDescending(worker => worker.Popularite)
            .Take(3)
            .ToList();

        return (int)Math.Round(top.Average(worker => worker.Popularite));
    }
}
