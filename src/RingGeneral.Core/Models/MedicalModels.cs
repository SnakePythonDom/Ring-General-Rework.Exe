namespace RingGeneral.Core.Models;

public sealed record Injury(
    int InjuryId,
    string WorkerId,
    string Type,
    int Severity,
    int StartWeek,
    int? EndWeek,
    bool IsActive,
    string? Notes,
    double RiskLevel);

public sealed record MedicalNote(
    int MedicalNoteId,
    string WorkerId,
    int? InjuryId,
    int Week,
    string Content);

public sealed record RecoveryPlan(
    int RecoveryPlanId,
    int InjuryId,
    string WorkerId,
    int StartWeek,
    int TargetWeek,
    string Status,
    string? Notes,
    int? CompletedWeek);

public sealed record MedicalRecommendation(
    int ReposSemaines,
    double Risque,
    string Niveau,
    string Conseil);

public sealed record InjuryApplicationResult(
    Injury Injury,
    RecoveryPlan Plan,
    MedicalRecommendation Recommendation);

public sealed record InjuryRecoveryResult(
    int InjuryId,
    string WorkerId,
    int Week,
    string Status);
