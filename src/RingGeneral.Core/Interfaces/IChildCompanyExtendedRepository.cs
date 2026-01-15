using RingGeneral.Core.Models.Company;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des filiales étendues (objectifs stratégiques)
/// </summary>
public interface IChildCompanyExtendedRepository
{
    // ====================================================================
    // CHILD COMPANY EXTENDED OPERATIONS
    // ====================================================================

    Task SaveChildCompanyExtendedAsync(ChildCompanyExtended childCompany);
    Task<ChildCompanyExtended?> GetChildCompanyExtendedByIdAsync(string childCompanyId);
    Task<IReadOnlyList<ChildCompanyExtended>> GetChildCompaniesByParentIdAsync(string parentCompanyId);
    Task<IReadOnlyList<ChildCompanyExtended>> GetChildCompaniesByObjectiveAsync(string objective);
    Task UpdateChildCompanyExtendedAsync(ChildCompanyExtended childCompany);
    Task DeleteChildCompanyExtendedAsync(string childCompanyId);
}
