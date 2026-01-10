using RingGeneral.Core.Models.Roster;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des transitions d'ADN
/// </summary>
public interface IDNATransitionRepository
{
    // ====================================================================
    // DNA TRANSITION OPERATIONS
    // ====================================================================

    Task SaveDNATransitionAsync(DNATransition transition);
    Task<DNATransition?> GetDNATransitionByIdAsync(string transitionId);
    Task<DNATransition?> GetActiveTransitionByCompanyIdAsync(string companyId);
    Task<IReadOnlyList<DNATransition>> GetTransitionsByCompanyIdAsync(string companyId);
    Task UpdateDNATransitionAsync(DNATransition transition);
    Task DeleteDNATransitionAsync(string transitionId);
}
