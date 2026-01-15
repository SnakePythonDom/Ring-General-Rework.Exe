using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Repository interface for gimmick management
/// </summary>
public interface IGimmickRepository
{
    // ====================================================================
    // GIMMICK QUERIES
    // ====================================================================

    /// <summary>
    /// Get all available gimmicks
    /// </summary>
    List<Gimmick> GetAllGimmicks();

    /// <summary>
    /// Get gimmicks by category
    /// </summary>
    List<Gimmick> GetGimmicksByCategory(string category);

    /// <summary>
    /// Get gimmicks by preferred alignment
    /// </summary>
    List<Gimmick> GetGimmicksByAlignment(string alignment);

    /// <summary>
    /// Get gimmicks by popularity tier
    /// </summary>
    List<Gimmick> GetGimmicksByPopularityTier(string tier);

    /// <summary>
    /// Get gimmick by ID
    /// </summary>
    Gimmick? GetGimmickById(string gimmickId);

    /// <summary>
    /// Search gimmicks by name
    /// </summary>
    List<Gimmick> SearchGimmicks(string searchTerm);

    /// <summary>
    /// Get recommended gimmicks for a worker based on their attributes
    /// </summary>
    List<Gimmick> GetRecommendedGimmicks(int workerId, int limit = 10);

    // ====================================================================
    // GIMMICK CATEGORIES
    // ====================================================================

    /// <summary>
    /// Get all gimmick categories
    /// </summary>
    List<GimmickCategoryInfo> GetAllCategories();

    /// <summary>
    /// Get category by ID
    /// </summary>
    GimmickCategoryInfo? GetCategoryById(string categoryId);

    // ====================================================================
    // GIMMICK HISTORY
    // ====================================================================

    /// <summary>
    /// Get gimmick history for a worker
    /// </summary>
    List<GimmickHistory> GetWorkerGimmickHistory(int workerId);

    /// <summary>
    /// Get current gimmick for a worker
    /// </summary>
    GimmickHistory? GetCurrentGimmick(int workerId);

    /// <summary>
    /// Assign a gimmick to a worker
    /// </summary>
    void AssignGimmickToWorker(int workerId, string gimmickId, string reason);

    /// <summary>
    /// Assign a custom gimmick to a worker (not from predefined list)
    /// </summary>
    void AssignCustomGimmick(int workerId, string gimmickName, string reason);

    /// <summary>
    /// End the current gimmick for a worker
    /// </summary>
    void EndCurrentGimmick(int workerId, int successRating, string? notes = null);

    /// <summary>
    /// Update success rating for a gimmick
    /// </summary>
    void UpdateGimmickSuccessRating(int historyId, int successRating);

    // ====================================================================
    // STATISTICS
    // ====================================================================

    /// <summary>
    /// Get total count of gimmicks
    /// </summary>
    int GetTotalGimmickCount();

    /// <summary>
    /// Get count of gimmicks by category
    /// </summary>
    Dictionary<string, int> GetGimmickCountByCategory();

    /// <summary>
    /// Get most popular gimmicks (most used)
    /// </summary>
    List<(Gimmick Gimmick, int UsageCount)> GetMostPopularGimmicks(int limit = 10);

    /// <summary>
    /// Get most successful gimmicks (highest average success rating)
    /// </summary>
    List<(Gimmick Gimmick, double AverageRating)> GetMostSuccessfulGimmicks(int limit = 10);
}
