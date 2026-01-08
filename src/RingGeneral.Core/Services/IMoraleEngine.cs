using RingGeneral.Core.Models.Morale;
using System.Collections.Generic;

namespace RingGeneral.Core.Services;

public interface IMoraleEngine
{
    void UpdateMorale(string entityId, string eventType, int impact);
    CompanyMorale CalculateCompanyMorale(string companyId);
    List<string> DetectWeakSignals(string companyId);
}
