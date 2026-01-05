namespace RingGeneral.Core.Models;

public sealed record BackstageWorker(
    string WorkerId,
    string NomComplet);

public sealed record BackstageIncidentDefinition(
    string TypeId,
    string Titre,
    string DescriptionTemplate,
    double Chance,
    int ParticipantsMin,
    int ParticipantsMax,
    int GraviteMin,
    int GraviteMax,
    int MoraleImpactMin,
    int MoraleImpactMax);

public sealed record DisciplinaryActionDefinition(
    string TypeId,
    string Libelle,
    int Gravite,
    int MoraleDelta);

public sealed record BackstageIncident(
    string IncidentId,
    string CompanyId,
    int Week,
    string TypeId,
    string Titre,
    string Description,
    int Gravite,
    IReadOnlyList<string> Workers);

public sealed record DisciplinaryAction(
    string ActionId,
    string CompanyId,
    string WorkerId,
    int Week,
    string TypeId,
    int Gravite,
    int MoraleDelta,
    string Notes,
    string? IncidentId);

public sealed record BackstageMoraleImpact(
    string WorkerId,
    int Delta,
    string Raison,
    string? IncidentId,
    string? ActionId);

public sealed record MoraleHistoryEntry(
    string WorkerId,
    int Week,
    int MoraleAvant,
    int MoraleApres,
    int Delta,
    string Raison,
    string? IncidentId,
    string? ActionId);

public sealed record BackstageRollResult(
    IReadOnlyList<BackstageIncident> Incidents,
    IReadOnlyList<BackstageMoraleImpact> MoraleImpacts,
    IReadOnlyList<InboxItem> InboxItems);

public sealed record DisciplineResult(
    DisciplinaryAction Action,
    BackstageMoraleImpact MoraleImpact);
