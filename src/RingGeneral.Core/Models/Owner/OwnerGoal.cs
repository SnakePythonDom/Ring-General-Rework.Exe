using System;

namespace RingGeneral.Core.Models.Owner;

public enum GoalMetric
{
    Revenue,
    FanSatisfaction,
    RosterMorale,
    AverageMatchRating,
    AverageShowRating,
    TalentDevelopment,
    WinningStreak // For specific talent
}

public enum GoalStatus
{
    Active,
    Met,
    Failed,
    Cancelled
}

public class OwnerGoal
{
    public string GoalId { get; set; } = Guid.NewGuid().ToString("N");
    public string OwnerId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public GoalMetric Metric { get; set; }
    public double TargetValue { get; set; }
    public double CurrentValue { get; set; }

    public DateTime Deadline { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.Active;

    public string? TargetEntityId { get; set; } // e.g., WorkerId if Metric is WinningStreak

    public bool IsExpired => DateTime.Now > Deadline && Status == GoalStatus.Active;

    public int Importance { get; set; } = 50; // 0-100, affects owner satisfaction if failed
}
