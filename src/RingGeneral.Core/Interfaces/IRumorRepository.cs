using RingGeneral.Core.Models.Morale;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des rumeurs (Phase 3).
/// Gère les rumeurs backstage et leur cycle de vie.
/// </summary>
public interface IRumorRepository
{
    Task<Rumor?> GetRumorByIdAsync(int rumorId);
    Task<List<Rumor>> GetActiveRumorsAsync(string companyId);
    Task<List<Rumor>> GetWidespreadRumorsAsync(string companyId);
    Task<int> GetActiveRumorCountAsync(string companyId);
    Task SaveRumorAsync(Rumor rumor);
    Task UpdateRumorAsync(Rumor rumor);
    Task ResolveRumorAsync(int rumorId);
    Task CleanupOldRumorsAsync(string companyId, int daysToKeep);
}
