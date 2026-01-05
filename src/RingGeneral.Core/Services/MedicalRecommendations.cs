using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class MedicalRecommendations
{
    public MedicalRecommendation Evaluer(int fatigue, InjurySeverity severity)
    {
        var baseRest = severity switch
        {
            InjurySeverity.Legere => 1,
            InjurySeverity.Moyenne => 3,
            InjurySeverity.Grave => 6,
            _ => 0
        };

        var fatigueBonus = fatigue switch
        {
            >= 85 => 2,
            >= 70 => 1,
            _ => 0
        };

        var repos = baseRest + fatigueBonus;
        var niveau = DeterminerRisque(fatigue, severity);
        var message = niveau switch
        {
            "Élevé" => "Repos renforcé recommandé pour limiter le risque.",
            "Modéré" => "Repos et suivi conseillés avant le retour.",
            _ => "Surveiller la charge de travail."
        };

        return new MedicalRecommendation(repos, niveau, message);
    }

    public MedicalRiskAssessment EvaluerRisque(int fatigue, InjurySeverity severity)
    {
        var niveau = DeterminerRisque(fatigue, severity);
        var message = niveau switch
        {
            "Élevé" => "Risque élevé de rechute, ajuster la charge immédiatement.",
            "Modéré" => "Risque modéré, prévoir un repos supplémentaire.",
            _ => "Risque faible, maintien sous surveillance."
        };

        return new MedicalRiskAssessment(niveau, message);
    }

    private static string DeterminerRisque(int fatigue, InjurySeverity severity)
    {
        if (severity == InjurySeverity.Grave || fatigue >= 70)
        {
            return "Élevé";
        }

        if (severity == InjurySeverity.Moyenne || fatigue >= 50)
        {
            return "Modéré";
        }

        return "Faible";
    }
}
