using RingGeneral.Core.Models.Company;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des fédérations de niche
/// </summary>
public interface INicheFederationRepository
{
    // ====================================================================
    // NICHE FEDERATION PROFILE OPERATIONS
    // ====================================================================

    Task SaveNicheFederationProfileAsync(NicheFederationProfile profile);
    Task<NicheFederationProfile?> GetNicheFederationProfileByCompanyIdAsync(string companyId);
    Task<IReadOnlyList<NicheFederationProfile>> GetActiveNicheFederationsAsync();
    Task<IReadOnlyList<NicheFederationProfile>> GetNicheFederationsByTypeAsync(string nicheType);
    Task UpdateNicheFederationProfileAsync(NicheFederationProfile profile);
    Task DeleteNicheFederationProfileAsync(string companyId);
}
