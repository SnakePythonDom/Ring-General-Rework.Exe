using RingGeneral.Core.Models;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository interface for managing worker notes, specializations, contracts, and history.
/// </summary>
public interface INotesRepository
{
    // ====================================================================
    // WORKER NOTES
    // ====================================================================

    /// <summary>
    /// Get a note by ID
    /// </summary>
    WorkerNote? GetNote(int noteId);

    /// <summary>
    /// Get all notes for a worker
    /// </summary>
    List<WorkerNote> GetNotesForWorker(int workerId);

    /// <summary>
    /// Get notes by category for a worker
    /// </summary>
    List<WorkerNote> GetNotesByCategory(int workerId, NoteCategory category);

    /// <summary>
    /// Create a new note
    /// </summary>
    int CreateNote(WorkerNote note);

    /// <summary>
    /// Update an existing note
    /// </summary>
    void UpdateNote(WorkerNote note);

    /// <summary>
    /// Delete a note
    /// </summary>
    void DeleteNote(int noteId);

    // ====================================================================
    // WORKER SPECIALIZATIONS
    // ====================================================================

    /// <summary>
    /// Get all specializations for a worker
    /// </summary>
    List<WorkerSpecialization> GetSpecializations(int workerId);

    /// <summary>
    /// Get primary specialization for a worker
    /// </summary>
    WorkerSpecialization? GetPrimarySpecialization(int workerId);

    /// <summary>
    /// Add a specialization to a worker
    /// </summary>
    int AddSpecialization(WorkerSpecialization specialization);

    /// <summary>
    /// Update a specialization
    /// </summary>
    void UpdateSpecialization(WorkerSpecialization specialization);

    /// <summary>
    /// Delete a specialization
    /// </summary>
    void DeleteSpecialization(int specializationId);

    /// <summary>
    /// Delete all specializations for a worker
    /// </summary>
    void DeleteAllSpecializations(int workerId);

    // ====================================================================
    // CONTRACT HISTORY
    // ====================================================================

    /// <summary>
    /// Get a contract by ID
    /// </summary>
    ContractHistory? GetContract(int contractId);

    /// <summary>
    /// Get all contracts for a worker
    /// </summary>
    List<ContractHistory> GetContractHistory(int workerId);

    /// <summary>
    /// Get active contract for a worker
    /// </summary>
    ContractHistory? GetActiveContract(int workerId);

    /// <summary>
    /// Create a new contract
    /// </summary>
    int CreateContract(ContractHistory contract);

    /// <summary>
    /// Update a contract
    /// </summary>
    void UpdateContract(ContractHistory contract);

    /// <summary>
    /// Expire a contract (set status to Expired)
    /// </summary>
    void ExpireContract(int contractId);

    /// <summary>
    /// Terminate a contract (set status to Terminated)
    /// </summary>
    void TerminateContract(int contractId);

    /// <summary>
    /// Get contracts expiring soon (within 30 days)
    /// </summary>
    List<ContractHistory> GetExpiringSoonContracts();

    // ====================================================================
    // MATCH HISTORY
    // ====================================================================

    /// <summary>
    /// Get a match by ID
    /// </summary>
    MatchHistoryItem? GetMatch(int matchId);

    /// <summary>
    /// Get all matches for a worker
    /// </summary>
    List<MatchHistoryItem> GetMatchHistory(int workerId);

    /// <summary>
    /// Get recent matches for a worker (limit)
    /// </summary>
    List<MatchHistoryItem> GetRecentMatches(int workerId, int limit);

    /// <summary>
    /// Add a match to history
    /// </summary>
    int AddMatch(MatchHistoryItem match);

    /// <summary>
    /// Update a match
    /// </summary>
    void UpdateMatch(MatchHistoryItem match);

    /// <summary>
    /// Delete a match
    /// </summary>
    void DeleteMatch(int matchId);

    /// <summary>
    /// Get match statistics for a worker (wins, losses, etc.)
    /// </summary>
    (int TotalMatches, int Wins, int Losses, int Draws, double WinPercentage) GetMatchStats(int workerId);

    // ====================================================================
    // TITLE REIGNS
    // ====================================================================

    /// <summary>
    /// Get a title reign by ID
    /// </summary>
    TitleReign? GetTitleReign(int reignId);

    /// <summary>
    /// Get all title reigns for a worker
    /// </summary>
    List<TitleReign> GetTitleReigns(int workerId);

    /// <summary>
    /// Get current title reigns for a worker (currently held championships)
    /// </summary>
    List<TitleReign> GetCurrentTitleReigns(int workerId);

    /// <summary>
    /// Add a title reign
    /// </summary>
    int AddTitleReign(TitleReign reign);

    /// <summary>
    /// Update a title reign
    /// </summary>
    void UpdateTitleReign(TitleReign reign);

    /// <summary>
    /// End a title reign (set lost date)
    /// </summary>
    void EndTitleReign(int reignId, DateTime lostDate, int? lostShowId = null);

    /// <summary>
    /// Delete a title reign
    /// </summary>
    void DeleteTitleReign(int reignId);
}
