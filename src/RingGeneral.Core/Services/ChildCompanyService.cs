using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Models.Trends;
using System;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service pour gérer les filiales avec objectifs stratégiques
/// </summary>
public class ChildCompanyService
{
    private readonly IChildCompanyExtendedRepository _childCompanyRepository;
    private readonly ITrendRepository _trendRepository;
    private readonly IYouthRepository? _youthRepository;

    public ChildCompanyService(
        IChildCompanyExtendedRepository childCompanyRepository,
        ITrendRepository trendRepository,
        IYouthRepository? youthRepository = null)
    {
        _childCompanyRepository = childCompanyRepository ?? throw new ArgumentNullException(nameof(childCompanyRepository));
        _trendRepository = trendRepository ?? throw new ArgumentNullException(nameof(trendRepository));
        _youthRepository = youthRepository;
    }

    /// <summary>
    /// Crée une nouvelle filiale avec un objectif
    /// </summary>
    public async Task<ChildCompanyExtended> CreateChildCompanyAsync(
        string childCompanyId,
        string parentCompanyId,
        ChildCompanyObjective objective,
        string? regionId = null)
    {
        string? youthStructureId = null;

        // Phase 2.3 - Si objectif = Development, créer automatiquement une YouthStructure
        if (objective == ChildCompanyObjective.Development && _youthRepository != null)
        {
            youthStructureId = await CreateYouthStructureForChildCompanyAsync(childCompanyId, parentCompanyId, regionId);
        }

        var childCompany = new ChildCompanyExtended
        {
            ChildCompanyId = childCompanyId,
            ParentCompanyId = parentCompanyId,
            Objective = objective,
            HasFullAutonomy = objective == ChildCompanyObjective.Independence,
            AssignedBookerId = null, // Sera assigné plus tard
            IsLaboratory = objective == ChildCompanyObjective.Entertainment,
            TestStyle = objective == ChildCompanyObjective.Entertainment ? "Experimental" : null,
            NicheType = objective == ChildCompanyObjective.Niche ? DetermineNicheType(parentCompanyId) : null,
            CreatedAt = DateTime.Now,
            IsActive = true,
            YouthStructureId = youthStructureId // Phase 2.3
        };

        await _childCompanyRepository.SaveChildCompanyExtendedAsync(childCompany);
        return childCompany;
    }

    /// <summary>
    /// Phase 2.3 - Crée une YouthStructure pour une filiale de développement
    /// </summary>
    private async Task<string> CreateYouthStructureForChildCompanyAsync(
        string childCompanyId,
        string parentCompanyId,
        string? regionId)
    {
        if (_youthRepository == null)
            throw new InvalidOperationException("YouthRepository non disponible");

        var youthStructureId = $"YS-{Guid.NewGuid():N}".ToUpperInvariant();
        var name = $"Development Center - {childCompanyId.Substring(0, Math.Min(8, childCompanyId.Length))}";

        // Créer la YouthStructure avec des valeurs par défaut pour une filiale de développement
        // Utiliser parentCompanyId comme CompanyId car la structure appartient à la compagnie mère
        await _youthRepository.CreateYouthStructureAsync(
            youthStructureId,
            parentCompanyId, // Utiliser parentCompanyId car la structure appartient à la compagnie mère
            name,
            regionId,
            "DEVELOPMENT", // Type
            100_000m, // BudgetAnnuel par défaut
            20, // CapaciteMax par défaut
            1, // NiveauEquipements basique
            10, // QualiteCoaching par défaut
            "HYBRIDE"); // Philosophie par défaut

        return youthStructureId;
    }

    /// <summary>
    /// Assigne un objectif à une filiale existante
    /// </summary>
    public async Task AssignObjectiveAsync(string childCompanyId, ChildCompanyObjective objective)
    {
        var childCompany = await _childCompanyRepository.GetChildCompanyExtendedByIdAsync(childCompanyId);
        if (childCompany == null)
        {
            throw new InvalidOperationException($"Filiale {childCompanyId} introuvable");
        }

        var updated = new ChildCompanyExtended
        {
            ChildCompanyId = childCompany.ChildCompanyId,
            ParentCompanyId = childCompany.ParentCompanyId,
            Objective = objective,
            HasFullAutonomy = objective == ChildCompanyObjective.Independence || childCompany.HasFullAutonomy,
            AssignedBookerId = childCompany.AssignedBookerId,
            IsLaboratory = objective == ChildCompanyObjective.Entertainment,
            TestStyle = objective == ChildCompanyObjective.Entertainment ? "Experimental" : childCompany.TestStyle,
            NicheType = objective == ChildCompanyObjective.Niche ? DetermineNicheType(childCompany.ParentCompanyId) : childCompany.NicheType,
            CreatedAt = childCompany.CreatedAt,
            IsActive = childCompany.IsActive
        };

        await _childCompanyRepository.UpdateChildCompanyExtendedAsync(updated);
    }

    /// <summary>
    /// Définit l'autonomie d'une filiale
    /// </summary>
    public async Task SetAutonomyAsync(string childCompanyId, bool fullAutonomy)
    {
        var childCompany = await _childCompanyRepository.GetChildCompanyExtendedByIdAsync(childCompanyId);
        if (childCompany == null)
        {
            throw new InvalidOperationException($"Filiale {childCompanyId} introuvable");
        }

        var updated = new ChildCompanyExtended
        {
            ChildCompanyId = childCompany.ChildCompanyId,
            ParentCompanyId = childCompany.ParentCompanyId,
            Objective = childCompany.Objective,
            HasFullAutonomy = fullAutonomy,
            AssignedBookerId = childCompany.AssignedBookerId,
            IsLaboratory = childCompany.IsLaboratory,
            TestStyle = childCompany.TestStyle,
            NicheType = childCompany.NicheType,
            CreatedAt = childCompany.CreatedAt,
            IsActive = childCompany.IsActive
        };

        await _childCompanyRepository.UpdateChildCompanyExtendedAsync(updated);
    }

    /// <summary>
    /// Détecte l'émergence d'une contre-tendance depuis une filiale
    /// </summary>
    public async Task<Trend?> DetectCounterTrendAsync(string childCompanyId)
    {
        var childCompany = await _childCompanyRepository.GetChildCompanyExtendedByIdAsync(childCompanyId);
        if (childCompany == null || !childCompany.IsActive) return null;

        // Si la filiale a une autonomie complète et est active depuis plus de 6 mois,
        // il y a une chance qu'une contre-tendance émerge
        var monthsActive = (DateTime.Now - childCompany.CreatedAt).TotalDays / 30;
        if (monthsActive < 6) return null;

        var random = new System.Random();
        if (random.Next(100) < 10) // 10% de chance
        {
            // Générer une tendance locale basée sur l'objectif de la filiale
            var category = childCompany.Objective switch
            {
                ChildCompanyObjective.Entertainment => TrendCategory.Audience,
                ChildCompanyObjective.Niche => TrendCategory.Style,
                _ => TrendCategory.Format
            };

            // Cette tendance serait créée par TrendEngine, mais pour l'instant on retourne null
            // L'implémentation complète nécessiterait TrendEngine
            return null;
        }

        return null;
    }

    private NicheType? DetermineNicheType(string parentCompanyId)
    {
        // Par défaut, déterminer le type de niche basé sur la compagnie mère
        // Pour l'instant, retourner Hardcore par défaut
        // L'implémentation complète nécessiterait d'accéder à l'ADN de la compagnie mère
        return NicheType.Hardcore;
    }
}
