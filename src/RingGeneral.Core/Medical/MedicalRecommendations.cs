using RingGeneral.Core.Models;

namespace RingGeneral.Core.Medical;

public sealed class MedicalRecommendations
{
    public MedicalRecommendation Proposer(int fatigue, InjurySeverity severity)
    {
        var reposBase = severity switch
        {
            InjurySeverity.Legere => 1,
            InjurySeverity.Moyenne => 3,
            InjurySeverity.Grave => 6,
            _ => 2
        };

        var bonusFatigue = fatigue switch
        {
            >= 85 => 2,
            >= 65 => 1,
            _ => 0
        };

        var reposTotal = reposBase + bonusFatigue;
        var risque = EvaluerRisque(fatigue, severity);
        var message = $"Repos conseillé : {reposTotal} semaine(s). Risque {risque.ToLowerInvariant()}.";

        return new MedicalRecommendation(reposTotal, risque, message);
    }

    private static string EvaluerRisque(int fatigue, InjurySeverity severity)
    {
        var score = CalculerRisque(fatigue, severity);
        return score switch
        {
            >= 0.75 => "ÉLEVÉ",
            >= 0.45 => "MODÉRÉ",
            _ => "FAIBLE"
        };
    }

    private static double CalculerRisque(int fatigue, InjurySeverity severity)
    {
        var baseScore = severity switch
        {
            InjurySeverity.Legere => 0.2,
            InjurySeverity.Moyenne => 0.45,
            InjurySeverity.Grave => 0.7,
            _ => 0.35
        };

        var fatigueScore = Math.Clamp(fatigue / 100.0, 0, 1);
        return Math.Clamp(baseScore + fatigueScore * 0.3, 0, 1);
    }
}
