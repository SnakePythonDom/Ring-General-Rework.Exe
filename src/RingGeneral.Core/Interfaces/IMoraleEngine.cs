using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Morale;
using System.Collections.Generic;

namespace RingGeneral.Core.Interfaces;

public enum MoraleImpactType
{
    Win,
    Loss,
    Buried, // Losing to much lower card status
    MainEventWin,
    TitleWin,
    TitleLoss,
    LeftOffShow,
    BonusPaid,
    FineIssued
}

public interface IMoraleEngine
{
    void ApplyMoraleImpact(string workerId, MoraleImpactType type);
    int CalculateWeeklyMoraleChange(Worker worker, int matchesThisWeek);
    CompanyMorale CalculateCompanyMorale(string companyId);
    IEnumerable<string> DetectWeakSignals(string companyId);
}
