using RingGeneral.Core.Models;
<<<<<<< HEAD
=======
using RingGeneral.Core.Models.Crisis;
>>>>>>> temp-work
using System;
using System.Collections.Generic;

namespace RingGeneral.Core.Interfaces;

public enum CrisisType
{
    MassWalkout,
    FinancialInsolvency,
    PublicScandal,
    LockerRoomRevolt
}

public record CrisisEvent(CrisisType Type, string Description, int Severity, DateTime Date);

public interface ICrisisEngine
{
    IEnumerable<CrisisEvent> CheckForCrises(string companyId, IEnumerable<Worker> roster, CompanyState companyState);
    void ResolveCrisis(string companyId, CrisisEvent crisis);
<<<<<<< HEAD
=======

    // Stateful Management (Phase 5)
    bool ShouldTriggerCrisis(string companyId, int moraleScore, int activeRumorsCount);
    Task<Crisis> CreateCrisisAsync(string companyId, string triggerReason, int severity);
    Task ProgressCrisesAsync(string companyId);
    Task<List<Crisis>> GetCriticalCrisesAsync(string companyId);
>>>>>>> temp-work
}
