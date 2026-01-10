using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service pour gérer l'allocation budgétaire
/// </summary>
public interface IBudgetAllocationService
{
    /// <summary>
    /// Répartit le budget par département
    /// </summary>
    BudgetAllocation AllocateBudget(string companyId, decimal totalBudget, BudgetAllocation? currentAllocation = null);

    /// <summary>
    /// Calcule les impacts d'une allocation budgétaire
    /// </summary>
    IReadOnlyList<AllocationImpact> CalculateImpacts(BudgetAllocation allocation);
}
