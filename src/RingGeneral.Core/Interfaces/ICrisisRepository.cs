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
    Task<List<Crisis>> GetCriticalCrisesAsync(string companyId);
    Task<int> GetResolvedCrisesCountAsync(string companyId);
    Task SaveCrisisAsync(Crisis crisis);
    Task UpdateCrisisAsync(Crisis crisis);

    // Communication operations
    Task<Communication?> GetCommunicationByIdAsync(int communicationId);
    Task<List<Communication>> GetCommunicationsByCrisisAsync(int crisisId);
    Task SaveCommunicationAsync(Communication communication);

    // Communication outcome operations
    Task<CommunicationOutcome?> GetCommunicationOutcomeByIdAsync(int outcomeId);
    Task<List<CommunicationOutcome>> GetOutcomesByCommunicationAsync(int communicationId);
    Task SaveCommunicationOutcomeAsync(CommunicationOutcome outcome);

    // Business queries
    Task<double> CalculateCommunicationSuccessRateAsync(string companyId);
}
