using RingGeneral.Core.Models.Company;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Interface pour le repository des ères et transitions.
/// Gère les opérations CRUD et requêtes métier pour les ères et leurs transitions.
/// </summary>
public interface IEraRepository
{
    // ====================================================================
    // ERA CRUD OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde une nouvelle ère dans la base de données
    /// </summary>
    Task SaveEraAsync(Era era);

    /// <summary>
    /// Récupère une ère par son identifiant
    /// </summary>
    Task<Era?> GetEraByIdAsync(string eraId);

    /// <summary>
    /// Récupère toutes les ères d'une compagnie
    /// </summary>
    Task<List<Era>> GetErasByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère l'ère actuelle d'une compagnie
    /// </summary>
    Task<Era?> GetCurrentEraAsync(string companyId);

    /// <summary>
    /// Met à jour une ère existante
    /// </summary>
    Task UpdateEraAsync(Era era);

    /// <summary>
    /// Termine une ère (set EndDate)
    /// </summary>
    Task EndEraAsync(string eraId);

    /// <summary>
    /// Définit une ère comme actuelle
    /// </summary>
    Task SetCurrentEraAsync(string companyId, string eraId);

    // ====================================================================
    // ERA TRANSITION OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde une nouvelle transition d'ère
    /// </summary>
    Task SaveEraTransitionAsync(EraTransition transition);

    /// <summary>
    /// Récupère une transition par son identifiant
    /// </summary>
    Task<EraTransition?> GetTransitionByIdAsync(string transitionId);

    /// <summary>
    /// Récupère la transition active pour une compagnie
    /// </summary>
    Task<EraTransition?> GetActiveTransitionAsync(string companyId);

    /// <summary>
    /// Récupère toutes les transitions d'une compagnie
    /// </summary>
    Task<List<EraTransition>> GetTransitionsByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère les transitions initiées par un booker
    /// </summary>
    Task<List<EraTransition>> GetTransitionsByBookerAsync(string bookerId);

    /// <summary>
    /// Met à jour une transition existante
    /// </summary>
    Task UpdateEraTransitionAsync(EraTransition transition);

    /// <summary>
    /// Complète une transition (set ActualEndDate, IsActive=false)
    /// </summary>
    Task CompleteTransitionAsync(string transitionId);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    /// <summary>
    /// Vérifie si une compagnie a une transition active
    /// </summary>
    Task<bool> HasActiveTransitionAsync(string companyId);

    /// <summary>
    /// Compte le nombre d'ères pour une compagnie
    /// </summary>
    Task<int> CountErasAsync(string companyId);

    /// <summary>
    /// Récupère l'historique des ères (terminées) pour une compagnie
    /// </summary>
    Task<List<Era>> GetEraHistoryAsync(string companyId);
}
