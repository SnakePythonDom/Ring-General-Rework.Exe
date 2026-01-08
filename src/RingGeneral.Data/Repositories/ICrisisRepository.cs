using RingGeneral.Core.Models.Crisis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Interface pour le repository des crises et communications.
/// Gère les opérations CRUD pour Crises, Communications, et CommunicationOutcomes.
/// </summary>
public interface ICrisisRepository
{
    // ====================================================================
    // CRISIS OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde une nouvelle crise
    /// </summary>
    Task SaveCrisisAsync(Crisis crisis);

    /// <summary>
    /// Récupère une crise par son identifiant
    /// </summary>
    Task<Crisis?> GetCrisisByIdAsync(int crisisId);

    /// <summary>
    /// Récupère toutes les crises actives d'une compagnie
    /// </summary>
    Task<List<Crisis>> GetActiveCrisesAsync(string companyId);

    /// <summary>
    /// Récupère les crises par stage
    /// </summary>
    Task<List<Crisis>> GetCrisesByStageAsync(string companyId, string stage);

    /// <summary>
    /// Récupère les crises critiques (Severity >= 4 ou Stage = Declared)
    /// </summary>
    Task<List<Crisis>> GetCriticalCrisesAsync(string companyId);

    /// <summary>
    /// Met à jour une crise existante
    /// </summary>
    Task UpdateCrisisAsync(Crisis crisis);

    /// <summary>
    /// Supprime une crise
    /// </summary>
    Task DeleteCrisisAsync(int crisisId);

    /// <summary>
    /// Compte le nombre de crises actives
    /// </summary>
    Task<int> CountActiveCrisesAsync(string companyId);

    // ====================================================================
    // COMMUNICATION OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde une nouvelle communication
    /// </summary>
    Task SaveCommunicationAsync(Communication communication);

    /// <summary>
    /// Récupère une communication par son identifiant
    /// </summary>
    Task<Communication?> GetCommunicationByIdAsync(int communicationId);

    /// <summary>
    /// Récupère toutes les communications pour une crise
    /// </summary>
    Task<List<Communication>> GetCommunicationsForCrisisAsync(int crisisId);

    /// <summary>
    /// Récupère les communications récentes d'une compagnie
    /// </summary>
    Task<List<Communication>> GetRecentCommunicationsAsync(string companyId, int limit = 10);

    /// <summary>
    /// Récupère les communications par type
    /// </summary>
    Task<List<Communication>> GetCommunicationsByTypeAsync(string companyId, string communicationType);

    /// <summary>
    /// Met à jour une communication
    /// </summary>
    Task UpdateCommunicationAsync(Communication communication);

    /// <summary>
    /// Supprime une communication
    /// </summary>
    Task DeleteCommunicationAsync(int communicationId);

    // ====================================================================
    // COMMUNICATION OUTCOME OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde un résultat de communication
    /// </summary>
    Task SaveCommunicationOutcomeAsync(CommunicationOutcome outcome);

    /// <summary>
    /// Récupère le résultat d'une communication
    /// </summary>
    Task<CommunicationOutcome?> GetCommunicationOutcomeAsync(int communicationId);

    /// <summary>
    /// Récupère tous les résultats pour une crise
    /// </summary>
    Task<List<CommunicationOutcome>> GetOutcomesForCrisisAsync(int crisisId);

    /// <summary>
    /// Récupère les résultats réussis récents
    /// </summary>
    Task<List<CommunicationOutcome>> GetSuccessfulOutcomesAsync(string companyId, int limit = 10);

    /// <summary>
    /// Met à jour un résultat
    /// </summary>
    Task UpdateCommunicationOutcomeAsync(CommunicationOutcome outcome);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    /// <summary>
    /// Calcule le taux de succès des communications pour une compagnie
    /// </summary>
    Task<double> CalculateCommunicationSuccessRateAsync(string companyId);

    /// <summary>
    /// Récupère l'historique complet d'une crise (crise + communications + outcomes)
    /// </summary>
    Task<(Crisis Crisis, List<Communication> Communications, List<CommunicationOutcome> Outcomes)?> GetCrisisHistoryAsync(int crisisId);

    /// <summary>
    /// Nettoie les vieilles crises résolues (> X jours)
    /// </summary>
    Task CleanupOldCrisesAsync(string companyId, int daysToKeep = 90);
}
