namespace RingGeneral.Core.Models;

public enum InjurySeverity
{
    Aucune,
    Legere,
    Moyenne,
    Grave
}

public enum InjuryStatus
{
    Active,
    Repos,
    RetourEnCours,
    Retabli
}

public sealed record InjuryRecord(
    string InjuryId,
    string WorkerId,
    string Type,
    InjurySeverity Severity,
    InjuryStatus Status,
    int WeekStart,
    int? WeekEnd,
    int DurationWeeks);

public sealed record MedicalNote(
    string NoteId,
    string WorkerId,
    string? InjuryId,
    string Type,
    string Content,
    int Week,
    string? Author);

public sealed record RecoveryPlan(
    string PlanId,
    string InjuryId,
    string WorkerId,
    InjuryStatus Status,
    int WeekStart,
    int? WeekEnd,
    int DurationWeeks,
    int RecommendedRestWeeks,
    string? Restrictions,
    string? Notes);

public sealed record MedicalRecommendation(
    int RecommendedRestWeeks,
    string RiskLevel,
    string Message);

public sealed record MedicalRiskAssessment(
    string RiskLevel,
    string Message);

public sealed record InjuryApplicationResult(
    InjuryRecord Injury,
    RecoveryPlan RecoveryPlan,
    MedicalNote MedicalNote,
    MedicalRiskAssessment RiskAssessment);
