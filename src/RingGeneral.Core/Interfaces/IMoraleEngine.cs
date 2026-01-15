using RingGeneral.Core.Models;
<<<<<<< HEAD
=======
using RingGeneral.Core.Models.Morale;
>>>>>>> temp-work
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
<<<<<<< HEAD
    void ApplyMoraleImpact(Worker worker, MoraleImpactType type);
    int CalculateWeeklyMoraleChange(Worker worker, int matchesThisWeek);
=======
    void ApplyMoraleImpact(string workerId, MoraleImpactType type);
    int CalculateWeeklyMoraleChange(Worker worker, int matchesThisWeek);
    CompanyMorale CalculateCompanyMorale(string companyId);
    IEnumerable<string> DetectWeakSignals(string companyId);
>>>>>>> temp-work
}
