namespace RingGeneral.Core.Models;

/// <summary>
/// Phase 3.1 - Template de contrat pour pré-remplir les négociations
/// </summary>
public sealed record ContractTemplate(
    string TemplateId,
    string Name,
    string Description,
    decimal MonthlyWage,
    decimal AppearanceFee,
    int DurationMonths,
    bool IsExclusive,
    bool HasRenewalOption);
