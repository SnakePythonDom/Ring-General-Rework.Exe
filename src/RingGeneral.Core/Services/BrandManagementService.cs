using System;
using System.Collections.Generic;
using System.Linq;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Models.Company;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service de gestion des brands dans une structure multi-brand.
/// Gère la création, configuration, et coordination des brands.
/// </summary>
public class BrandManagementService
{
    /// <summary>
    /// Crée un nouveau brand pour une compagnie
    /// </summary>
    public Brand CreateBrand(
        string companyId,
        string name,
        BrandObjective objective,
        int priority,
        double budgetPerShow,
        string? targetRegion = null)
    {
        return new Brand
        {
            BrandId = $"brand_{Guid.NewGuid():N}",
            CompanyId = companyId,
            Name = name,
            Objective = objective,
            BookerId = null, // À assigner plus tard
            CurrentEraId = null, // À définir plus tard
            Prestige = CalculateInitialPrestige(objective, priority),
            BudgetPerShow = budgetPerShow,
            AverageAudience = 0,
            Priority = priority,
            IsActive = true,
            AirDay = null,
            ShowDuration = 120, // 2 heures par défaut
            TargetRegion = targetRegion,
            CreatedAt = DateTime.Now,
            DeactivatedAt = null
        };
    }

    /// <summary>
    /// Calcule le prestige initial d'un brand selon son objectif et priorité
    /// </summary>
    private int CalculateInitialPrestige(BrandObjective objective, int priority)
    {
        var basePrestige = objective switch
        {
            BrandObjective.Flagship => 70,
            BrandObjective.Prestige => 65,
            BrandObjective.Mainstream => 60,
            BrandObjective.Experimental => 45,
            BrandObjective.Development => 40,
            BrandObjective.Regional => 50,
            BrandObjective.Women => 55,
            BrandObjective.Touring => 50,
            _ => 50
        };

        // Ajuster selon priorité (flagship = priorité 1)
        var priorityBonus = priority == 1 ? 10 : 0;
        var priorityPenalty = priority >= 3 ? -10 : 0;

        return Math.Clamp(basePrestige + priorityBonus + priorityPenalty, 20, 100);
    }

    /// <summary>
    /// Valide qu'une hiérarchie peut supporter un nouveau brand
    /// </summary>
    public bool CanAddBrand(CompanyHierarchy hierarchy, int currentBrandCount)
    {
        // Multi-brand peut avoir jusqu'à 10 brands
        if (hierarchy.Type == CompanyHierarchyType.MultiBrand)
        {
            return currentBrandCount < 10;
        }

        // Mono-brand ne peut avoir qu'1 brand
        return false;
    }

    /// <summary>
    /// Convertit une structure mono-brand en multi-brand
    /// </summary>
    public CompanyHierarchy ConvertToMultiBrand(
        CompanyHierarchy currentHierarchy,
        string headBookerId,
        int centralizationLevel = 50,
        bool allowBrandAutonomy = true)
    {
        if (currentHierarchy.Type == CompanyHierarchyType.MultiBrand)
        {
            throw new InvalidOperationException("La hiérarchie est déjà multi-brand");
        }

        return currentHierarchy with
        {
            Type = CompanyHierarchyType.MultiBrand,
            HeadBookerId = headBookerId,
            ActiveBrandCount = 1, // Sera incrémenté quand brands ajoutés
            AllowsBrandAutonomy = allowBrandAutonomy,
            CentralizationLevel = centralizationLevel,
            LastModifiedAt = DateTime.Now
        };
    }

    /// <summary>
    /// Assigne un booker à un brand
    /// </summary>
    public Brand AssignBooker(Brand brand, string bookerId)
    {
        return brand with
        {
            BookerId = bookerId
        };
    }

    /// <summary>
    /// Calcule le budget total nécessaire pour tous les brands
    /// </summary>
    public double CalculateTotalBrandBudget(IEnumerable<Brand> brands, int showsPerMonth = 4)
    {
        return brands
            .Where(b => b.IsActive)
            .Sum(b => b.BudgetPerShow * showsPerMonth);
    }

    /// <summary>
    /// Détermine le brand flagship (priorité la plus haute)
    /// </summary>
    public Brand? GetFlagshipBrand(IEnumerable<Brand> brands)
    {
        return brands
            .Where(b => b.IsActive)
            .OrderBy(b => b.Priority)
            .ThenByDescending(b => b.Prestige)
            .FirstOrDefault();
    }

    /// <summary>
    /// Calcule les conflits potentiels entre brands
    /// </summary>
    public List<string> DetectBrandConflicts(IEnumerable<Brand> brands, CompanyHierarchy hierarchy)
    {
        var conflicts = new List<string>();
        var activeBrands = brands.Where(b => b.IsActive).ToList();

        // Vérifier: plusieurs brands flagship
        var flagshipCount = activeBrands.Count(b => b.Objective == BrandObjective.Flagship);
        if (flagshipCount > 1)
        {
            conflicts.Add($"⚠️ {flagshipCount} brands Flagship détectés - Un seul flagship recommandé");
        }

        // Vérifier: aucun flagship
        if (flagshipCount == 0 && activeBrands.Count > 1)
        {
            conflicts.Add("⚠️ Aucun brand Flagship - Définissez un flagship principal");
        }

        // Vérifier: surcharge budgétaire si trop de brands prioritaires
        var highPriorityBrands = activeBrands.Count(b => b.Priority <= 2);
        if (highPriorityBrands > 3)
        {
            conflicts.Add($"💰 {highPriorityBrands} brands haute priorité - Risque budgétaire");
        }

        // Vérifier: autonomie faible mais beaucoup de brands
        if (hierarchy.CentralizationLevel >= 70 && activeBrands.Count >= 3)
        {
            conflicts.Add("🎯 Centralisation élevée avec {activeBrands.Count} brands - Complexité managériale");
        }

        // Vérifier: même jour de diffusion
        var airDayGroups = activeBrands
            .Where(b => !string.IsNullOrWhiteSpace(b.AirDay))
            .GroupBy(b => b.AirDay)
            .Where(g => g.Count() > 1);

        foreach (var group in airDayGroups)
        {
            conflicts.Add($"📅 {group.Count()} brands diffusés le {group.Key} - Risque de cannibalisation");
        }

        return conflicts;
    }

    /// <summary>
    /// Calcule la santé globale d'un portfolio de brands
    /// </summary>
    public int CalculatePortfolioHealth(IEnumerable<Brand> brands)
    {
        var activeBrands = brands.Where(b => b.IsActive).ToList();

        if (!activeBrands.Any())
        {
            return 0;
        }

        // Moyenne des scores de santé individuels
        var averageHealth = activeBrands.Average(b => b.CalculateHealthScore());

        // Bonus si diversification d'objectifs
        var uniqueObjectives = activeBrands.Select(b => b.Objective).Distinct().Count();
        var diversificationBonus = Math.Min(uniqueObjectives * 5, 20);

        // Pénalité si trop de brands inactifs
        var totalBrands = brands.Count();
        var inactiveBrands = totalBrands - activeBrands.Count;
        var inactivePenalty = inactiveBrands * 10;

        var totalHealth = (int)(averageHealth + diversificationBonus - inactivePenalty);

        return Math.Clamp(totalHealth, 0, 100);
    }

    /// <summary>
    /// Génère des recommandations pour optimiser le portfolio de brands
    /// </summary>
    public List<string> GetPortfolioRecommendations(
        IEnumerable<Brand> brands,
        CompanyHierarchy hierarchy,
        double availableBudget)
    {
        var recommendations = new List<string>();
        var activeBrands = brands.Where(b => b.IsActive).ToList();

        // Budget
        var totalBudgetNeeded = CalculateTotalBrandBudget(activeBrands, 4);
        if (totalBudgetNeeded > availableBudget)
        {
            recommendations.Add($"💰 Budget insuffisant: {totalBudgetNeeded:C} nécessaire vs {availableBudget:C} disponible");
            recommendations.Add("💡 Réduisez le budget par show ou le nombre de brands");
        }

        // Structure
        if (hierarchy.Type == CompanyHierarchyType.MultiBrand && string.IsNullOrWhiteSpace(hierarchy.HeadBookerId))
        {
            recommendations.Add("⚠️ Structure multi-brand sans Head Booker - Assignez-en un");
        }

        // Brands sans booker
        var brandsWithoutBooker = activeBrands.Count(b => string.IsNullOrWhiteSpace(b.BookerId));
        if (brandsWithoutBooker > 0)
        {
            recommendations.Add($"👤 {brandsWithoutBooker} brand(s) sans booker - Assignez des bookers");
        }

        // Prestige
        var lowPrestigeBrands = activeBrands.Count(b => b.Prestige < 30);
        if (lowPrestigeBrands > 0)
        {
            recommendations.Add($"📉 {lowPrestigeBrands} brand(s) à faible prestige - Investissez dans ces brands");
        }

        // Audience
        var lowAudienceBrands = activeBrands.Count(b => b.AverageAudience < 1000);
        if (lowAudienceBrands > 0 && activeBrands.Any(b => b.AverageAudience > 0))
        {
            recommendations.Add($"📺 {lowAudienceBrands} brand(s) à faible audience - Revoyez le produit");
        }

        // Flagship
        var flagship = GetFlagshipBrand(activeBrands);
        if (flagship != null && flagship.Prestige < 60)
        {
            recommendations.Add("⭐ Flagship à faible prestige - Priorisez vos meilleurs talents");
        }

        // Développement
        var devBrands = activeBrands.Count(b => b.Objective == BrandObjective.Development);
        if (devBrands == 0 && activeBrands.Count >= 2)
        {
            recommendations.Add("🌱 Aucun brand Development - Envisagez un brand pour jeunes talents");
        }

        // Optimisation
        var portfolioHealth = CalculatePortfolioHealth(brands);
        if (portfolioHealth >= 80)
        {
            recommendations.Add("✅ Portfolio en excellente santé - Maintenez le cap");
        }
        else if (portfolioHealth <= 40)
        {
            recommendations.Add("⚠️ Portfolio en difficulté - Restructuration nécessaire");
        }

        return recommendations;
    }

    /// <summary>
    /// Calcule la distribution idéale de budget entre brands
    /// </summary>
    public Dictionary<string, double> CalculateOptimalBudgetDistribution(
        IEnumerable<Brand> brands,
        double totalAvailableBudget,
        int showsPerMonth = 4)
    {
        var activeBrands = brands.Where(b => b.IsActive).ToList();
        var distribution = new Dictionary<string, double>();

        if (!activeBrands.Any())
        {
            return distribution;
        }

        // Calculer poids total basé sur priorité et prestige
        var totalWeight = activeBrands.Sum(b => CalculateBrandWeight(b));

        // Budget mensuel disponible
        var monthlyBudget = totalAvailableBudget;

        // Distribuer selon poids
        foreach (var brand in activeBrands)
        {
            var weight = CalculateBrandWeight(brand);
            var brandMonthlyBudget = monthlyBudget * (weight / totalWeight);
            var budgetPerShow = brandMonthlyBudget / showsPerMonth;

            distribution[brand.BrandId] = budgetPerShow;
        }

        return distribution;
    }

    /// <summary>
    /// Calcule le poids d'un brand pour distribution budgétaire
    /// </summary>
    private double CalculateBrandWeight(Brand brand)
    {
        // Priorité inverse (1 = plus important)
        var priorityWeight = (11 - brand.Priority) * 10;

        // Objectif
        var objectiveWeight = brand.Objective switch
        {
            BrandObjective.Flagship => 50,
            BrandObjective.Prestige => 40,
            BrandObjective.Mainstream => 45,
            BrandObjective.Experimental => 25,
            BrandObjective.Development => 20,
            BrandObjective.Regional => 30,
            BrandObjective.Women => 35,
            BrandObjective.Touring => 35,
            _ => 30
        };

        // Prestige actuel
        var prestigeWeight = brand.Prestige * 0.3;

        return priorityWeight + objectiveWeight + prestigeWeight;
    }
}
