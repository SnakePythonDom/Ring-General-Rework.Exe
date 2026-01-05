using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

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
        int semaine,
        int fatigue)
    {
        var recommendation = _recommendations.Evaluer(fatigue, severity);
        var injury = new InjuryRecord(
            Guid.NewGuid().ToString("N"),
            workerId,
            type,
            severity,
            InjuryStatus.Active,
            semaine,
            null,
            recommendation.RecommendedRestWeeks);

        var plan = new RecoveryPlan(
            Guid.NewGuid().ToString("N"),
            injury.InjuryId,
            workerId,
            InjuryStatus.Repos,
            semaine,
            null,
            recommendation.RecommendedRestWeeks,
            recommendation.RecommendedRestWeeks,
            "Limiter les segments à faible intensité.",
            recommendation.Message);

        var note = new MedicalNote(
            Guid.NewGuid().ToString("N"),
            workerId,
            injury.InjuryId,
            "DIAGNOSTIC",
            $"Blessure {type} ({severity}) détectée. {recommendation.Message}",
            semaine,
            "Equipe médicale");

        var risk = _recommendations.EvaluerRisque(fatigue, severity);

        return new InjuryApplicationResult(injury, plan, note, risk);
    }

    public InjuryRecord Recuperer(InjuryRecord injury, int semaine)
    {
        var status = semaine >= injury.WeekStart + injury.DurationWeeks
            ? InjuryStatus.Retabli
            : InjuryStatus.RetourEnCours;

        return injury with
        {
            Status = status,
            WeekEnd = semaine
        };
    }

    public MedicalRiskAssessment EvaluerRisque(int fatigue, InjurySeverity severity)
    {
        return _recommendations.EvaluerRisque(fatigue, severity);
    }
}
