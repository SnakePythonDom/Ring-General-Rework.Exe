using RingGeneral.Core.Models.Company;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des brands et hiérarchies.
/// Gère les opérations CRUD et requêtes métier pour les brands et structures hiérarchiques.
/// </summary>
public interface IBrandRepository
{
    // ====================================================================
    // BRAND CRUD OPERATIONS
    // ====================================================================

    Task SaveBrandAsync(Brand brand);
    Task<Brand?> GetBrandByIdAsync(string brandId);
    Task<List<Brand>> GetBrandsByCompanyIdAsync(string companyId);
    Task<List<Brand>> GetActiveBrandsByCompanyIdAsync(string companyId);
    Task<Brand?> GetFlagshipBrandAsync(string companyId);
    Task UpdateBrandAsync(Brand brand);
    Task DeactivateBrandAsync(string brandId);
    Task DeleteBrandAsync(string brandId);
    Task AssignBookerToBrandAsync(string brandId, string bookerId);
    Task RemoveBookerFromBrandAsync(string brandId);

    // ====================================================================
    // COMPANY HIERARCHY OPERATIONS
    // ====================================================================

    Task SaveHierarchyAsync(CompanyHierarchy hierarchy);
    Task<CompanyHierarchy?> GetHierarchyByCompanyIdAsync(string companyId);
    Task UpdateHierarchyAsync(CompanyHierarchy hierarchy);
    Task AssignHeadBookerAsync(string companyId, string headBookerId);
    Task RemoveHeadBookerAsync(string companyId);
    Task ConvertToMultiBrandAsync(string companyId, string headBookerId);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    Task<int> CountActiveBrandsAsync(string companyId);
    Task<List<Brand>> GetBrandsWithoutBookerAsync(string companyId);
    Task<List<Brand>> GetBrandsByObjectiveAsync(string companyId, string objective);
    Task<bool> IsMultiBrandAsync(string companyId);
    Task<double> CalculateTotalBrandBudgetAsync(string companyId);
}
