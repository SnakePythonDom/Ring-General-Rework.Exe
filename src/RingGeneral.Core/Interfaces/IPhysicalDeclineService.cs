using RingGeneral.Core.Models;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service for managing physical and mental decline of workers over time.
/// (Phase 5: World Evolution)
/// </summary>
public interface IPhysicalDeclineService
{
    /// <summary>
    /// Applies yearly stat decline (or growth closure) based on age and style.
    /// </summary>
    Task ApplyYearlyDeclineAsync(Worker worker);

    /// <summary>
    /// Applies stat penalties due to specific injuries.
    /// </summary>
    Task ApplyInjuryEffectsAsync(Worker worker, string injuryType);

    /// <summary>
    /// Updates the worker's Legacy Score (Peak Popularity tracking).
    /// </summary>
    Task UpdateLegacyScoreAsync(Worker worker);
}
