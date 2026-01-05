using RingGeneral.Core.Models;

namespace RingGeneral.Core.Medical;

public sealed class InjuryService
{
    private readonly MedicalRecommendations _recommendations;

    public InjuryService(MedicalRecommendations recommendations)
    {
        _recommendations = recommendations;
    }

    public InjuryApplicationResult AppliquerBlessure(
        string workerId,
        string type,
        InjurySeverity severity,
        int semaineCourante,
        int fatigue,
        string? notes = null)
    {
        var recommendation = _recommendations.Proposer(fatigue, severity);
        var finSemaine = semaineCourante + recommendation.RecommendedRestWeeks;

        var blessure = new InjuryRecord(
            0,
            workerId,
            type,
            severity,
            semaineCourante,
            finSemaine,
            true,
            notes);

        var plan = new RecoveryPlan(
            0,
            0,
            workerId,
            semaineCourante,
            finSemaine,
            recommendation.RecommendedRestWeeks,
            recommendation.RiskLevel,
            "EN_COURS",
            DateTimeOffset.UtcNow);

        return new InjuryApplicationResult(blessure, plan, recommendation);
    }

    public InjuryRecord RecupererBlessure(InjuryRecord blessure, int semaineCourante, string? note)
        => blessure with
        {
            EndWeek = semaineCourante,
            IsActive = false,
            Notes = note ?? blessure.Notes
        };

    public double CalculerRisque(int fatigue, InjurySeverity severity)
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

    public string EvaluerRisque(int fatigue, InjurySeverity severity)
    {
        var score = CalculerRisque(fatigue, severity);
        return score switch
        {
            >= 0.75 => "ÉLEVÉ",
            >= 0.45 => "MODÉRÉ",
            _ => "FAIBLE"
        };
    }

    public InjurySeverity ParserSeverite(string? statutBlessure)
    {
        if (string.IsNullOrWhiteSpace(statutBlessure))
        {
            return InjurySeverity.Legere;
        }

        return statutBlessure.Trim().ToUpperInvariant() switch
        {
            "MOYENNE" => InjurySeverity.Moyenne,
            "GRAVE" => InjurySeverity.Grave,
            _ => InjurySeverity.Legere
        };
    }
}
