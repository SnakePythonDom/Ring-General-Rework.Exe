using RingGeneral.Core.Models.Attributes;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository interface for managing worker performance attributes (30 attributes total).
/// Handles InRing, Entertainment, and Story attributes.
/// </summary>
public interface IWorkerAttributesRepository
{
    // ====================================================================
    // IN-RING ATTRIBUTES (10 attributes)
    // ====================================================================

    /// <summary>
    /// Get InRing attributes for a worker
    /// </summary>
    WorkerInRingAttributes? GetInRingAttributes(int workerId);

    /// <summary>
    /// Save or update InRing attributes for a worker
    /// </summary>
    void SaveInRingAttributes(WorkerInRingAttributes attributes);

    /// <summary>
    /// Update a specific InRing attribute value
    /// </summary>
    void UpdateInRingAttribute(int workerId, string attributeName, int value);

    // ====================================================================
    // ENTERTAINMENT ATTRIBUTES (10 attributes)
    // ====================================================================

    /// <summary>
    /// Get Entertainment attributes for a worker
    /// </summary>
    WorkerEntertainmentAttributes? GetEntertainmentAttributes(int workerId);

    /// <summary>
    /// Save or update Entertainment attributes for a worker
    /// </summary>
    void SaveEntertainmentAttributes(WorkerEntertainmentAttributes attributes);

    /// <summary>
    /// Update a specific Entertainment attribute value
    /// </summary>
    void UpdateEntertainmentAttribute(int workerId, string attributeName, int value);

    // ====================================================================
    // STORY ATTRIBUTES (10 attributes)
    // ====================================================================

    /// <summary>
    /// Get Story attributes for a worker
    /// </summary>
    WorkerStoryAttributes? GetStoryAttributes(int workerId);

    /// <summary>
    /// Save or update Story attributes for a worker
    /// </summary>
    void SaveStoryAttributes(WorkerStoryAttributes attributes);

    /// <summary>
    /// Update a specific Story attribute value
    /// </summary>
    void UpdateStoryAttribute(int workerId, string attributeName, int value);

    // ====================================================================
    // BULK OPERATIONS
    // ====================================================================

    /// <summary>
    /// Get all attributes (InRing, Entertainment, Story) for a worker
    /// </summary>
    (WorkerInRingAttributes? InRing, WorkerEntertainmentAttributes? Entertainment, WorkerStoryAttributes? Story) GetAllAttributes(int workerId);

    /// <summary>
    /// Initialize default attributes for a new worker (all 30 attributes = 50)
    /// </summary>
    void InitializeDefaultAttributes(int workerId);

    /// <summary>
    /// Delete all attributes for a worker
    /// </summary>
    void DeleteAllAttributes(int workerId);

    /// <summary>
    /// Get workers with InRingAvg above threshold
    /// </summary>
    List<int> GetWorkersByInRingAvg(int minAvg);

    /// <summary>
    /// Get workers with EntertainmentAvg above threshold
    /// </summary>
    List<int> GetWorkersByEntertainmentAvg(int minAvg);

    /// <summary>
    /// Get workers with StoryAvg above threshold
    /// </summary>
    List<int> GetWorkersByStoryAvg(int minAvg);
}
