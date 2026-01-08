using System;

namespace RingGeneral.Core.Models.Morale;

public class BackstageMorale
{
    public int Id { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public int MoraleScore { get; set; } = 70;
    public int PreviousMoraleScore { get; set; } = 70;
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public bool IsLow => MoraleScore < 40;
    public bool IsCritical => MoraleScore < 20;
    public bool IsImproving => MoraleScore > PreviousMoraleScore;
    public string MoraleLabel => MoraleScore switch
    {
        >= 80 => "Excellent",
        >= 60 => "Good",
        >= 40 => "Average",
        >= 20 => "Low",
        _ => "Critical"
    };
}
