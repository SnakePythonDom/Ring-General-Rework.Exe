using RingGeneral.Core.Models.Company;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des ères et transitions.
/// Gère les opérations CRUD et requêtes métier pour les ères et leurs transitions.
/// </summary>
public interface IEraRepository
{
    // ====================================================================
    // ERA CRUD OPERATIONS
    // ====================================================================

    Task SaveEraAsync(Era era);
    Task<Era?> GetEraByIdAsync(string eraId);
    Task<List<Era>> GetErasByCompanyIdAsync(string companyId);
    Task<Era?> GetCurrentEraAsync(string companyId);
    Task UpdateEraAsync(Era era);
    Task EndEraAsync(string eraId);
    Task SetCurrentEraAsync(string companyId, string eraId);

    // ====================================================================
    // ERA TRANSITION OPERATIONS
    // ====================================================================

    Task SaveEraTransitionAsync(EraTransition transition);
    Task<EraTransition?> GetTransitionByIdAsync(string transitionId);
    Task<EraTransition?> GetActiveTransitionAsync(string companyId);
    Task<List<EraTransition>> GetTransitionsByCompanyIdAsync(string companyId);
    Task<List<EraTransition>> GetTransitionsByBookerAsync(string bookerId);
    Task UpdateEraTransitionAsync(EraTransition transition);
    Task CompleteTransitionAsync(string transitionId);
    Task CancelTransitionAsync(string transitionId);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    Task<bool> HasActiveTransitionAsync(string companyId);
    Task<int> CountErasAsync(string companyId);
    Task<List<Era>> GetEraHistoryAsync(string companyId);
}
