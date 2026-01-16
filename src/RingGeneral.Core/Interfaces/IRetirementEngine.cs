using RingGeneral.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Engine for evaluating and processing worker retirements.
/// (Phase 5: World Evolution)
/// </summary>
public interface IRetirementEngine
{
    /// <summary>
    /// Checks if a worker should retire based on age, injury, morale, and fulfillment.
    /// </summary>
    /// <returns>True if the worker decides to retire.</returns>
    Task<bool> ShouldRetireAsync(Worker worker);

    /// <summary>
    /// Processes the retirement of a worker (contracts, history, announcement).
    /// </summary>
    Task ProcessRetirementAsync(Worker worker, string reason = "Retraite");

    /// <summary>
    /// Gets a list of workers who are likely to retire soon (for UI warnings).
    /// </summary>
    Task<List<Worker>> GetPotentialRetireesAsync(string companyId);
}
