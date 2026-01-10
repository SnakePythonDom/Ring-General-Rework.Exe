using RingGeneral.Core.Models.ChildCompany;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des Child Companies.
/// Gère les opérations CRUD pour les compagnies affiliées de développement.
/// </summary>
public interface IChildCompanyRepository
{
    // ====================================================================
    // CHILD COMPANY CRUD OPERATIONS
    // ====================================================================

    Task SaveChildCompanyAsync(ChildCompany childCompany);
    Task<ChildCompany?> GetChildCompanyByIdAsync(string childCompanyId);
    Task<IReadOnlyList<ChildCompany>> GetChildCompaniesByParentAsync(string parentCompanyId);
    Task<IReadOnlyList<ChildCompany>> GetChildCompaniesByRegionAsync(string regionId);
    Task UpdateChildCompanyAsync(ChildCompany childCompany);
    Task DeleteChildCompanyAsync(string childCompanyId);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    Task<int> CountActiveChildCompaniesAsync(string parentCompanyId);
    Task<decimal> CalculateTotalMonthlyBudgetAsync(string parentCompanyId);
    Task<IReadOnlyList<ChildCompany>> GetChildCompaniesByLevelAsync(string parentCompanyId, ChildCompanyLevel level);
}