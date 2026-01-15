using RingGeneral.Core.Models.Crisis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des crises (Phase 5).
/// Gère les crises, communications et leurs résultats.
/// </summary>
public interface ICrisisRepository
{
    // Crisis operations
    Task<Crisis?> GetCrisisByIdAsync(int crisisId);
    Task<List<Crisis>> GetActiveCrisesAsync(string companyId);
    Task<List<Crisis>> GetCrisesByStageAsync(string companyId, string stage);
    Task<List<Crisis>> GetCriticalCrisesAsync(string companyId);
    Task<int> GetResolvedCrisesCountAsync(string companyId);
    Task<int> CountActiveCrisesAsync(string companyId);
    Task SaveCrisisAsync(Crisis crisis);
    Task UpdateCrisisAsync(Crisis crisis);
    Task DeleteCrisisAsync(int crisisId);
    Task CleanupOldCrisesAsync(string companyId, int daysToKeep = 90);

    // Communication operations
    Task<Communication?> GetCommunicationByIdAsync(int communicationId);
    Task<List<Communication>> GetCommunicationsForCrisisAsync(int crisisId);
    Task<List<Communication>> GetRecentCommunicationsAsync(string companyId, int limit = 10);
    Task<List<Communication>> GetCommunicationsByTypeAsync(string companyId, string communicationType);
    Task SaveCommunicationAsync(Communication communication);
    Task UpdateCommunicationAsync(Communication communication);
    Task DeleteCommunicationAsync(int communicationId);

    // Communication outcome operations
    Task<CommunicationOutcome?> GetCommunicationOutcomeAsync(int communicationId);
    Task<List<CommunicationOutcome>> GetOutcomesForCrisisAsync(int crisisId);
    Task<List<CommunicationOutcome>> GetSuccessfulOutcomesAsync(string companyId, int limit = 10);
    Task SaveCommunicationOutcomeAsync(CommunicationOutcome outcome);
    Task UpdateCommunicationOutcomeAsync(CommunicationOutcome outcome);

    // Business queries
    Task<double> CalculateCommunicationSuccessRateAsync(string companyId);
    Task<(Crisis Crisis, List<Communication> Communications, List<CommunicationOutcome> Outcomes)?> GetCrisisHistoryAsync(int crisisId);
}
