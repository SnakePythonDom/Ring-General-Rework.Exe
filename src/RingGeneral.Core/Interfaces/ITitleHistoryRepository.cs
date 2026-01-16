using RingGeneral.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Repository for accessing title history, reigns, and defenses.
/// (Phase 5: World Evolution)
/// </summary>
public interface ITitleHistoryRepository
{
    /// <summary>
    /// Gets the full title history for a specific title.
    /// </summary>
    Task<List<TitleReign>> GetTitleHistoryAsync(string titleId);

    /// <summary>
    /// Gets all title reigns for a specific worker.
    /// </summary>
    Task<List<TitleReign>> GetWorkerTitleHistoryAsync(string workerId);

    /// <summary>
    /// Gets current champions for a company.
    /// </summary>
    Task<List<TitleReign>> GetCurrentChampionsAsync(string companyId);

    /// <summary>
    /// Adds a new title reign (championship change).
    /// </summary>
    Task AddTitleReignAsync(TitleReign reign);

    /// <summary>
    /// Updates an existing title reign (e.g. setting end date).
    /// </summary>
    Task UpdateTitleReignAsync(TitleReign reign);

    /// <summary>
    /// Records a successful title defense.
    /// </summary>
    Task AddTitleDefenseAsync(string reignId, string opponentId, string showId, string matchNote);
}
