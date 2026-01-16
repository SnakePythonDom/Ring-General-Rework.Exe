using RingGeneral.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service for managing Hall of Fame inductions and eligibility.
/// (Phase 5: World Evolution)
/// </summary>
public interface IHallOfFameService
{
    /// <summary>
    /// Checks if a worker meets the criteria for Hall of Fame induction.
    /// </summary>
    Task<bool> IsEligibleForHallOfFameAsync(Worker worker);

    /// <summary>
    /// Calculates a detailed HoF score explaining why (or why not) they qualify.
    /// </summary>
    Task<(int Score, List<string> Reasons)> CalculateHallOfFameScoreAsync(Worker worker);

    /// <summary>
    /// Processes inductions for a specific year/class.
    /// </summary>
    Task<List<Worker>> InductClassAsync(string companyId, int maxInductees = 3);
}
