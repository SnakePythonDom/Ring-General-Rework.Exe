namespace RingGeneral.Core.Models;

public sealed record ShowFinanceContext(
    string ShowId,
    decimal TicketRevenue,
    decimal MerchRevenue,
    decimal TvRevenue,
    decimal TotalRevenue);

/// <summary>
/// Phase 2.2 - Projection de revenus sur 12 mois
/// </summary>
public sealed record RevenueProjection(
    string CompanyId,
    int StartMonth,
    IReadOnlyList<MonthlyRevenue> MonthlyRevenues,
    decimal TotalProjectedRevenue,
    decimal AverageMonthlyRevenue);

/// <summary>
/// Phase 2.2 - Revenus mensuels projetés
/// </summary>
public sealed record MonthlyRevenue(
    int Month,
    decimal TvRevenue,
    decimal TicketRevenue,
    decimal MerchRevenue,
    decimal SponsorRevenue,
    decimal TotalRevenue);

/// <summary>
/// Phase 2.2 - Allocation budgétaire par département
/// </summary>
public sealed record BudgetAllocation(
    string CompanyId,
    decimal TotalBudget,
    decimal TalentAllocation,
    decimal ProductionAllocation,
    decimal YouthDevAllocation,
    decimal MarketingAllocation,
    decimal MedicalAllocation);

/// <summary>
/// Phase 2.2 - Impact d'une allocation budgétaire
/// </summary>
public sealed record AllocationImpact(
    string Department,
    decimal AllocationPercent,
    string ImpactDescription,
    double ImpactValue);

/// <summary>
/// Phase 2.2 - Dette de la compagnie
/// </summary>
public sealed record CompanyDebt(
    string DebtId,
    string CompanyId,
    decimal PrincipalAmount,
    decimal InterestRate,
    int TermMonths,
    DateTime StartDate,
    decimal MonthlyPayment,
    decimal RemainingBalance);
