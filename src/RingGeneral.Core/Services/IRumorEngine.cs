using RingGeneral.Core.Models.Morale;
using System.Collections.Generic;

namespace RingGeneral.Core.Services;

public interface IRumorEngine
{
    bool ShouldTriggerRumor(string companyId, string eventType, int eventSeverity);
    Rumor GenerateRumor(string companyId, string rumorType, string triggerEvent);
    void AmplifyRumor(int rumorId, string influencerWorkerId);
    void ProgressRumors(string companyId);
    List<Rumor> GetActiveRumors(string companyId);
    List<Rumor> GetWidespreadRumors(string companyId);
}
