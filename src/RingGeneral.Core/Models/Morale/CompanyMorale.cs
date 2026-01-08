using System;

namespace RingGeneral.Core.Models.Morale;

public class CompanyMorale
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;
    public int GlobalMoraleScore { get; set; } = 70;
    public int WorkersMoraleAvg { get; set; } = 70;
    public int StaffMoraleAvg { get; set; } = 70;
    public string Trend { get; set; } = "Stable";
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public bool IsHealthy => GlobalMoraleScore >= 60;
    public bool IsCrisis => GlobalMoraleScore < 30;
}
