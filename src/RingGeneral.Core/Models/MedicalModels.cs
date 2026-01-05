namespace RingGeneral.Core.Models;

public enum InjurySeverity
{
    Legere = 1,
    Moyenne = 2,
    Grave = 3
}

public sealed record InjuryRecord(
    int InjuryId,
    string WorkerId,
    string Type,
    InjurySeverity Severity,
    int StartWeek,
    int? EndWeek,
    bool IsActive,
    string? Notes);

public sealed record MedicalNote(
    int MedicalNoteId,
    int? InjuryId,
    string WorkerId,
    string Note,
    DateTimeOffset CreatedAt);

public sealed record RecoveryPlan(
    int RecoveryPlanId,
    int InjuryId,
    string WorkerId,
    int StartWeek,
    int TargetWeek,
    int RecommendedRestWeeks,
    string RiskLevel,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record MedicalRecommendation(
    int RecommendedRestWeeks,
    string RiskLevel,
    string Message);

public sealed record InjuryApplicationResult(
    InjuryRecord Blessure,
    RecoveryPlan Plan,
    MedicalRecommendation Recommendation);
