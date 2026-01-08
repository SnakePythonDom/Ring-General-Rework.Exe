using RingGeneral.Core.Models.Relations;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository interface for managing worker relations and factions.
/// </summary>
public interface IRelationsRepository
{
    // ====================================================================
    // WORKER RELATIONS
    // ====================================================================

    /// <summary>
    /// Get a relation by ID
    /// </summary>
    WorkerRelation? GetRelation(int relationId);

    /// <summary>
    /// Get all relations for a specific worker
    /// </summary>
    List<WorkerRelation> GetRelationsForWorker(int workerId);

    /// <summary>
    /// Get relations of a specific type for a worker
    /// </summary>
    List<WorkerRelation> GetRelationsByType(int workerId, RelationType relationType);

    /// <summary>
    /// Create a new relation between two workers
    /// </summary>
    int CreateRelation(WorkerRelation relation);

    /// <summary>
    /// Update an existing relation
    /// </summary>
    void UpdateRelation(WorkerRelation relation);

    /// <summary>
    /// Delete a relation
    /// </summary>
    void DeleteRelation(int relationId);

    /// <summary>
    /// Check if a relation exists between two workers
    /// </summary>
    bool RelationExists(int workerId1, int workerId2);

    /// <summary>
    /// Get strong relations (strength >= 70) for a worker
    /// </summary>
    List<WorkerRelation> GetStrongRelations(int workerId);

    // ====================================================================
    // FACTIONS
    // ====================================================================

    /// <summary>
    /// Get a faction by ID
    /// </summary>
    Faction? GetFaction(int factionId);

    /// <summary>
    /// Get all active factions
    /// </summary>
    List<Faction> GetActiveFactions();

    /// <summary>
    /// Get all factions (including inactive/disbanded)
    /// </summary>
    List<Faction> GetAllFactions();

    /// <summary>
    /// Create a new faction
    /// </summary>
    int CreateFaction(Faction faction);

    /// <summary>
    /// Update an existing faction
    /// </summary>
    void UpdateFaction(Faction faction);

    /// <summary>
    /// Delete a faction
    /// </summary>
    void DeleteFaction(int factionId);

    /// <summary>
    /// Disband a faction (sets status to Disbanded and removes all members)
    /// </summary>
    void DisbandFaction(int factionId, int week, int year);

    /// <summary>
    /// Set faction status to inactive
    /// </summary>
    void SetFactionInactive(int factionId);

    /// <summary>
    /// Reactivate a faction
    /// </summary>
    void ReactivateFaction(int factionId);

    // ====================================================================
    // FACTION MEMBERS
    // ====================================================================

    /// <summary>
    /// Get a faction member by ID
    /// </summary>
    FactionMember? GetFactionMember(int memberId);

    /// <summary>
    /// Get all members of a faction
    /// </summary>
    List<FactionMember> GetFactionMembers(int factionId);

    /// <summary>
    /// Get active members of a faction
    /// </summary>
    List<FactionMember> GetActiveFactionMembers(int factionId);

    /// <summary>
    /// Get all faction memberships for a worker (past and present)
    /// </summary>
    List<FactionMember> GetWorkerFactionHistory(int workerId);

    /// <summary>
    /// Get current faction memberships for a worker
    /// </summary>
    List<FactionMember> GetCurrentFactionMemberships(int workerId);

    /// <summary>
    /// Add a member to a faction
    /// </summary>
    int AddFactionMember(FactionMember member);

    /// <summary>
    /// Remove a member from a faction (set left date)
    /// </summary>
    void RemoveFactionMember(int memberId, int week, int year);

    /// <summary>
    /// Check if a worker is currently in a faction
    /// </summary>
    bool IsWorkerInFaction(int workerId, int factionId);

    /// <summary>
    /// Get factions where worker is the leader
    /// </summary>
    List<Faction> GetFactionsLedByWorker(int workerId);
}
