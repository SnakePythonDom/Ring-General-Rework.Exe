using System;

namespace RingGeneral.Core.Models.Morale;

public class Rumor
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;
    public string RumorType { get; set; } = string.Empty; // Nepotism, UnfairPush, etc.
    public string RumorText { get; set; } = string.Empty;
    public string Stage { get; set; } = "Emerging"; // Emerging, Growing, Widespread, Resolved, Ignored
    public int Severity { get; set; } = 1;
    public int AmplificationScore { get; set; } = 10;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsActive => Stage != "Resolved" && Stage != "Ignored";
    public bool IsWidespread => AmplificationScore >= 70;
}
