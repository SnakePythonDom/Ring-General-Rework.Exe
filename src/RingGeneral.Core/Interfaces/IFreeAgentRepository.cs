using RingGeneral.Core.Models.Recruitment;

namespace RingGeneral.Core.Interfaces;

public interface IFreeAgentRepository
{
    /// <summary>
    /// Fetches all active free agents (Wrestlers with no company and Staff in FREE_AGENT pool).
    /// </summary>
    Task<List<FreeAgentCandidate>> GetFreeAgentMarketAsync(FreeAgentFilter filter);

    /// <summary>
    /// Counts total free agents for pagination or stats.
    /// </summary>
    Task<int> CountFreeAgentsAsync(FreeAgentFilter filter);
}

public class FreeAgentFilter
{
    public string? SearchText { get; set; }
    public FreeAgentType? Type { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public string? Region { get; set; }
    public string? Role { get; set; }

    public string? SortBy { get; set; } // Popularity, Salary, Skill
    public bool SortDescending { get; set; } = true;
}
