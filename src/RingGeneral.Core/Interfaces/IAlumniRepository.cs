using RingGeneral.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Repository for accessing retired workers and Hall of Fame data.
/// (Phase 5: World Evolution)
/// </summary>
public interface IAlumniRepository
{
    /// <summary>
    /// Gets all workers who have departed/retired from a specific company.
    /// </summary>
    Task<List<Worker>> GetCompanyAlumniAsync(string companyId);

    /// <summary>
    /// Gets all Hall of Fame inductees globally or for a specific company (if shared HoF logic).
    /// </summary>
    Task<List<Worker>> GetHallOfFameInducteesAsync(string companyId);

    /// <summary>
    /// Checks if a worker qualifies for the Hall of Fame based on criteria.
    /// </summary>
    Task<bool> CheckHallOfFameEligibilityAsync(string workerId, int legacyScoreThreshold);

    /// <summary>
    /// Inducts a worker into the Hall of Fame.
    /// </summary>
    Task InductIntoHallOfFameAsync(string workerId);
}
