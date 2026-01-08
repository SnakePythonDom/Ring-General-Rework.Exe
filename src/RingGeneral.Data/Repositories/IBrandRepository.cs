using RingGeneral.Core.Models.Company;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Interface pour le repository des brands et hiérarchies.
/// Gère les opérations CRUD et requêtes métier pour les brands et structures hiérarchiques.
/// </summary>
public interface IBrandRepository
{
    // ====================================================================
    // BRAND CRUD OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde un nouveau brand dans la base de données
    /// </summary>
    Task SaveBrandAsync(Brand brand);

    /// <summary>
    /// Récupère un brand par son identifiant
    /// </summary>
    Task<Brand?> GetBrandByIdAsync(string brandId);

    /// <summary>
    /// Récupère tous les brands d'une compagnie
    /// </summary>
    Task<List<Brand>> GetBrandsByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère les brands actifs d'une compagnie
    /// </summary>
    Task<List<Brand>> GetActiveBrandsByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère le brand flagship (priorité 1)
    /// </summary>
    Task<Brand?> GetFlagshipBrandAsync(string companyId);

    /// <summary>
    /// Met à jour un brand existant
    /// </summary>
    Task UpdateBrandAsync(Brand brand);

    /// <summary>
    /// Désactive un brand
    /// </summary>
    Task DeactivateBrandAsync(string brandId);

    /// <summary>
    /// Assigne un booker à un brand
    /// </summary>
    Task AssignBookerToBrandAsync(string brandId, string bookerId);

    /// <summary>
    /// Retire un booker d'un brand
    /// </summary>
    Task RemoveBookerFromBrandAsync(string brandId);

    // ====================================================================
    // COMPANY HIERARCHY OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde une nouvelle hiérarchie de compagnie
    /// </summary>
    Task SaveHierarchyAsync(CompanyHierarchy hierarchy);

    /// <summary>
    /// Récupère la hiérarchie d'une compagnie
    /// </summary>
    Task<CompanyHierarchy?> GetHierarchyByCompanyIdAsync(string companyId);

    /// <summary>
    /// Met à jour la hiérarchie d'une compagnie
    /// </summary>
    Task UpdateHierarchyAsync(CompanyHierarchy hierarchy);

    /// <summary>
    /// Assigne un Head Booker à une hiérarchie
    /// </summary>
    Task AssignHeadBookerAsync(string companyId, string headBookerId);

    /// <summary>
    /// Retire le Head Booker d'une hiérarchie
    /// </summary>
    Task RemoveHeadBookerAsync(string companyId);

    /// <summary>
    /// Convertit une hiérarchie en multi-brand
    /// </summary>
    Task ConvertToMultiBrandAsync(string companyId, string headBookerId);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    /// <summary>
    /// Compte le nombre de brands actifs pour une compagnie
    /// </summary>
    Task<int> CountActiveBrandsAsync(string companyId);

    /// <summary>
    /// Récupère les brands sans booker assigné
    /// </summary>
    Task<List<Brand>> GetBrandsWithoutBookerAsync(string companyId);

    /// <summary>
    /// Récupère les brands par objectif
    /// </summary>
    Task<List<Brand>> GetBrandsByObjectiveAsync(string companyId, string objective);

    /// <summary>
    /// Vérifie si une compagnie est multi-brand
    /// </summary>
    Task<bool> IsMultiBrandAsync(string companyId);

    /// <summary>
    /// Calcule le budget total de tous les brands actifs
    /// </summary>
    Task<double> CalculateTotalBrandBudgetAsync(string companyId);
}
