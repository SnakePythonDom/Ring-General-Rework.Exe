using RingGeneral.Core.Models.Morale;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository du moral (Phase 3).
/// Gère les événements de moral, les états et les historiques.
/// </summary>
public interface IMoraleRepository
{
    Task<MoraleState?> GetMoraleStateAsync(string entityId);
    Task<List<MoraleState>> GetCompanyMoraleStatesAsync(string companyId);
    Task SaveMoraleStateAsync(MoraleState state);
    Task UpdateMoraleStateAsync(MoraleState state);

    Task<List<MoraleEvent>> GetRecentEventsAsync(string entityId, int limit);
    Task<List<MoraleEvent>> GetEventsByCompanyAsync(string companyId);
    Task SaveMoraleEventAsync(MoraleEvent moraleEvent);

    Task<List<MoraleHistory>> GetMoraleHistoryAsync(string entityId, int limit);
    Task SaveMoraleHistoryAsync(MoraleHistory history);
}
