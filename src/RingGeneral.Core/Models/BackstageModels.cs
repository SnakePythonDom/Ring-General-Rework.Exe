namespace RingGeneral.Core.Models;

public sealed record WorkerBackstageProfile(
    string WorkerId,
    string Nom);

public sealed record IncidentDefinition(
    string IncidentType,
    string Libelle,
    string Description,
    int Severity,
    int MoraleImpactMin,
    int MoraleImpactMax,
    IReadOnlyList<string> Tags);

public sealed record BackstageIncident(
    string IncidentId,
    string WorkerId,
    string IncidentType,
    string Description,
    int Severity,
    int Week,
    string Status);

public sealed record MoraleHistoryEntry(
    string WorkerId,
    int Week,
    int Delta,
    int Value,
    string Reason,
    string? IncidentId);

public sealed record BackstageMoraleImpact(
    string WorkerId,
    int Delta,
    string Raison,
    string? IncidentId,
    string? ActionId);

public sealed record DisciplinaryAction(
    string ActionId,
    string IncidentId,
    string WorkerId,
    string ActionType,
    int MoraleDelta,
    int Week,
    string? Notes);
