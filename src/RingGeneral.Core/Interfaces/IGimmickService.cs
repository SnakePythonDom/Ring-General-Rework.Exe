using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service interface for gimmick business logic
/// </summary>
public interface IGimmickService
{
    // ====================================================================
    // GIMMICK ASSIGNMENT
    // ====================================================================

    /// <summary>
    /// Check if a worker can accept a gimmick change based on personality
    /// </summary>
    GimmickAcceptanceResult CheckGimmickAcceptance(Worker worker, Gimmick gimmick);

    /// <summary>
    /// Calculate the success potential of a gimmick for a worker
    /// </summary>
    int CalculateGimmickPotential(Worker worker, Gimmick gimmick);

    /// <summary>
    /// Assign a gimmick to a worker with validation
    /// </summary>
    GimmickAssignmentResult AssignGimmick(int workerId, string gimmickId, string reason, bool forceAssign = false);

    /// <summary>
    /// Assign a custom gimmick to a worker
    /// </summary>
    GimmickAssignmentResult AssignCustomGimmick(int workerId, string gimmickName, string reason);

    // ====================================================================
    // RECOMMENDATIONS
    // ====================================================================

    /// <summary>
    /// Get recommended gimmicks for a worker
    /// </summary>
    List<GimmickRecommendation> GetRecommendations(int workerId, int limit = 10);

    /// <summary>
    /// Get gimmicks compatible with worker's alignment
    /// </summary>
    List<Gimmick> GetCompatibleGimmicks(Worker worker);

    // ====================================================================
    // PERFORMANCE EFFECTS
    // ====================================================================

    /// <summary>
    /// Calculate entertainment modifier for a match based on gimmicks
    /// </summary>
    int CalculateGimmickEntertainmentBonus(Worker worker);

    /// <summary>
    /// Calculate crowd reaction modifier based on gimmick
    /// </summary>
    int CalculateGimmickCrowdBonus(Worker worker, bool isHeelTerritory);

    /// <summary>
    /// Calculate gimmick synergy between two workers (for matches/storylines)
    /// </summary>
    int CalculateGimmickSynergy(Worker worker1, Worker worker2);

    // ====================================================================
    // HISTORY & EVOLUTION
    // ====================================================================

    /// <summary>
    /// Update gimmick success rating based on performance
    /// </summary>
    void UpdateGimmickSuccess(int workerId, int performanceScore);

    /// <summary>
    /// Check if worker's gimmick should evolve
    /// </summary>
    GimmickEvolutionSuggestion? CheckEvolutionOpportunity(int workerId);

    /// <summary>
    /// Get gimmick performance statistics
    /// </summary>
    GimmickPerformanceStats GetGimmickPerformanceStats(int workerId);
}

/// <summary>
/// Result of gimmick acceptance check
/// </summary>
public class GimmickAcceptanceResult
{
    public bool WillAccept { get; set; }
    public int AcceptanceChance { get; set; }
    public string Message { get; set; } = string.Empty;
    public int MoraleImpact { get; set; }
    public List<string> Concerns { get; set; } = new();
}

/// <summary>
/// Result of gimmick assignment
/// </summary>
public class GimmickAssignmentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int MoraleChange { get; set; }
    public GimmickHistory? History { get; set; }

    public static GimmickAssignmentResult Failed(string message) => new() { Success = false, Message = message };
    public static GimmickAssignmentResult Succeeded(string message, GimmickHistory? history = null)
        => new() { Success = true, Message = message, History = history };
}

/// <summary>
/// Gimmick recommendation with score
/// </summary>
public class GimmickRecommendation
{
    public Gimmick Gimmick { get; set; } = null!;
    public int Score { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsAlignmentMatch { get; set; }
    public bool IsCategoryMatch { get; set; }
}

/// <summary>
/// Suggestion for gimmick evolution
/// </summary>
public class GimmickEvolutionSuggestion
{
    public string CurrentGimmick { get; set; } = string.Empty;
    public List<Gimmick> SuggestedEvolutions { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
    public bool IsNaturalProgression { get; set; }
}

/// <summary>
/// Gimmick performance statistics
/// </summary>
public class GimmickPerformanceStats
{
    public string CurrentGimmick { get; set; } = string.Empty;
    public int DaysWithCurrentGimmick { get; set; }
    public int MatchesWithCurrentGimmick { get; set; }
    public double AverageMatchRating { get; set; }
    public int CrowdReactionTrend { get; set; }
    public int TotalGimmicksUsed { get; set; }
    public GimmickHistory? MostSuccessfulGimmick { get; set; }
}
