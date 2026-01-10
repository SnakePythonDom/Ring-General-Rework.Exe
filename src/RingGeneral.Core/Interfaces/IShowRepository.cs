using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour accéder aux données des shows
/// </summary>
public interface IShowRepository
{
    /// <summary>
    /// Charge les shows récents d'une compagnie avec leurs notes
    /// </summary>
    IReadOnlyList<ShowHistoryEntry> ChargerShowsRecents(string companyId, int count);
}
