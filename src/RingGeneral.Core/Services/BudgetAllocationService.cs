using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service pour gérer l'allocation budgétaire
/// </summary>
public sealed class BudgetAllocationService : IBudgetAllocationService
{
    public BudgetAllocation AllocateBudget(string companyId, decimal totalBudget, BudgetAllocation? currentAllocation = null)
    {
        // Si allocation actuelle existe, la réutiliser avec le nouveau budget total
        if (currentAllocation != null)
        {
            var talentPercent = currentAllocation.TotalBudget > 0
                ? currentAllocation.TalentAllocation / currentAllocation.TotalBudget
                : 0.5m; // 50% par défaut

            var productionPercent = currentAllocation.TotalBudget > 0
                ? currentAllocation.ProductionAllocation / currentAllocation.TotalBudget
                : 0.2m; // 20% par défaut

            var youthPercent = currentAllocation.TotalBudget > 0
                ? currentAllocation.YouthDevAllocation / currentAllocation.TotalBudget
                : 0.1m; // 10% par défaut

            var marketingPercent = currentAllocation.TotalBudget > 0
                ? currentAllocation.MarketingAllocation / currentAllocation.TotalBudget
                : 0.15m; // 15% par défaut

            var medicalPercent = currentAllocation.TotalBudget > 0
                ? currentAllocation.MedicalAllocation / currentAllocation.TotalBudget
                : 0.05m; // 5% par défaut

            return new BudgetAllocation(
                companyId,
                totalBudget,
                totalBudget * talentPercent,
                totalBudget * productionPercent,
                totalBudget * youthPercent,
                totalBudget * marketingPercent,
                totalBudget * medicalPercent);
        }

        // Allocation par défaut
        return new BudgetAllocation(
            companyId,
            totalBudget,
            totalBudget * 0.5m,      // Talent: 50%
            totalBudget * 0.2m,      // Production: 20%
            totalBudget * 0.1m,      // Youth Dev: 10%
            totalBudget * 0.15m,     // Marketing: 15%
            totalBudget * 0.05m);    // Medical: 5%
    }

    public IReadOnlyList<AllocationImpact> CalculateImpacts(BudgetAllocation allocation)
    {
        var impacts = new List<AllocationImpact>();

        var total = allocation.TotalBudget;
        if (total == 0) return impacts;

        // Talent Allocation Impact
        var talentPercent = allocation.TalentAllocation / total;
        if (talentPercent > 0.6m)
        {
            impacts.Add(new AllocationImpact(
                "Talent",
                talentPercent * 100m,
                "Allocation élevée: Meilleure rétention des talents",
                0.15)); // +15% rétention
        }
        else if (talentPercent < 0.4m)
        {
            impacts.Add(new AllocationImpact(
                "Talent",
                talentPercent * 100m,
                "Allocation faible: Risque de départ des talents",
                -0.10)); // -10% rétention
        }

        // Youth Dev Impact
        var youthPercent = allocation.YouthDevAllocation / total;
        var youthImpact = (double)((youthPercent - 0.1m) * 2.0m); // Multiplicateur pour impact
        impacts.Add(new AllocationImpact(
            "Youth Development",
            youthPercent * 100m,
            $"Progression des trainees: {youthImpact * 100:F1}%",
            youthImpact));

        // Production Impact
        var productionPercent = allocation.ProductionAllocation / total;
        if (productionPercent > 0.25m)
        {
            impacts.Add(new AllocationImpact(
                "Production",
                productionPercent * 100m,
                "Qualité de production élevée: +10% qualité shows",
                0.10));
        }

        // Marketing Impact
        var marketingPercent = allocation.MarketingAllocation / total;
        var marketingImpact = (double)((marketingPercent - 0.1m) * 1.5m);
        impacts.Add(new AllocationImpact(
            "Marketing",
            marketingPercent * 100m,
            $"Reach et audience: {marketingImpact * 100:F1}%",
            marketingImpact));

        // Medical Impact
        var medicalPercent = allocation.MedicalAllocation / total;
        if (medicalPercent > 0.08m)
        {
            impacts.Add(new AllocationImpact(
                "Medical",
                medicalPercent * 100m,
                "Prévention blessures: -20% risque blessures",
                -0.20));
        }

        return impacts;
    }
}
