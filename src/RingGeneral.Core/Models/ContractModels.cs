namespace RingGeneral.Core.Models;

public sealed record ContractClause(
    string Type,
    string Valeur);

public sealed record ContractOffer(
    string OfferId,
    string NegociationId,
    string WorkerId,
    string CompanyId,
    string TypeContrat,
    int StartWeek,
    int EndWeek,
    decimal SalaireHebdo,
    decimal BonusShow,
    decimal Buyout,
    int NonCompeteWeeks,
    bool RenouvellementAuto,
    bool EstExclusif,
    string Statut,
    int CreatedWeek,
    int ExpirationWeek,
    string? ParentOfferId,
    bool EstIa);

public sealed record ActiveContract(
    string ContractId,
    string WorkerId,
    string CompanyId,
    string TypeContrat,
    int StartWeek,
    int EndWeek,
    decimal SalaireHebdo,
    decimal BonusShow,
    decimal Buyout,
    int NonCompeteWeeks,
    bool RenouvellementAuto,
    bool EstExclusif,
    string Statut,
    int CreatedWeek);

public sealed record ContractNegotiationState(
    string NegociationId,
    string WorkerId,
    string CompanyId,
    string Statut,
    string? DerniereOffreId,
    int DerniereMiseAJourSemaine);

public sealed record ContractOfferDraft(
    string WorkerId,
    string CompanyId,
    string TypeContrat,
    int StartWeek,
    int EndWeek,
    decimal SalaireHebdo,
    decimal BonusShow,
    decimal Buyout,
    int NonCompeteWeeks,
    bool RenouvellementAuto,
    bool EstExclusif,
    int ExpirationDelaiSemaines);

public sealed record ContractAiContext(
    decimal BudgetHebdo,
    int Popularite,
    int Morale,
    int BesoinRoster);

public sealed record ContractAiDecision(
    string Decision,
    ContractOfferDraft? ContreProposition,
    string Message);
