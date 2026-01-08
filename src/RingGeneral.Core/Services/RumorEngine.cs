using RingGeneral.Core.Models.Morale;
using System;

namespace RingGeneral.Core.Services;

public class RumorEngine : IRumorEngine
{
    public bool ShouldTriggerRumor(string companyId, string eventType, int eventSeverity)
    {
        return eventSeverity >= 3; // Minimal logic
    }

    public Rumor GenerateRumor(string companyId, string rumorType, string triggerEvent)
    {
        return new Rumor
        {
            CompanyId = companyId,
            RumorType = rumorType,
            RumorText = $"Des tensions se font sentir concernant {rumorType}.",
            Stage = "Emerging",
            Severity = 2,
            CreatedAt = DateTime.Now
        };
    }

    public void AmplifyRumor(int rumorId, string influencerWorkerId)
    {
        // TODO: Implement with repository
    }

    public void ProgressRumors(string companyId)
    {
        // TODO: Implement with repository
    }
}
