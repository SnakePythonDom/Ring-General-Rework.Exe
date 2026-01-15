using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Crisis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

public class CrisisEngine : ICrisisEngine
{
    private readonly ICrisisRepository _crisisRepository;

    public CrisisEngine(ICrisisRepository crisisRepository)
    {
        _crisisRepository = crisisRepository;
    }

    public IEnumerable<CrisisEvent> CheckForCrises(string companyId, IEnumerable<Worker> roster, CompanyState companyState)
    {
        var crises = new List<CrisisEvent>();
        var workerList = roster.ToList();

        // 1. Locker Room Revolt (Low Morale)
        // Check for widespread low morale
        int revoltCount = workerList.Count(w => w.Morale < 20);
        if (revoltCount >= 3 && revoltCount >= workerList.Count * 0.15) // At least 3 workers and 15% of roster
        {
            crises.Add(new CrisisEvent(
               CrisisType.LockerRoomRevolt,
               $"Locker Room Revolt detected! {revoltCount} workers have critically low morale.",
               8,
               DateTime.Now));
        }

        // 2. Financial Insolvency
        if (companyState.Tresorerie < 0)
        {
            crises.Add(new CrisisEvent(
                CrisisType.FinancialInsolvency,
                $"Company treasury is negative ({companyState.Tresorerie:C0}). Bankruptcy risk imminent.",
                10,
                DateTime.Now));
        }

        // 3. Mass Walkout logic (example: many contracts expiring without renewal + low morale)
        // For now, simple check: multiple main eventers unhappy
        int unhappyMainEventers = workerList.Count(w => w.PushLevel == PushLevel.MainEvent && w.Morale < 30);
        if (unhappyMainEventers >= 2)
        {
            crises.Add(new CrisisEvent(
                CrisisType.MassWalkout,
                $"Potential Mass Walkout! {unhappyMainEventers} Main Eventers are extremely unhappy.",
                9,
                DateTime.Now));
        }

        // Persist detected crises if they don't already exist (logic could be expanded)
        // For this engine, we return the events. The calling service/orchestrator handles persistence via Repository if needed,
        // or we do it here. Given the interface returns IEnumerable, it suggests detection. 
        // However, we should check if these are NEW crises or ongoing.
        // For simplicity in this phase, we just return what we find.

        return crises;
<<<<<<< HEAD
=======
    }

    public void ResolveCrisis(string companyId, CrisisEvent crisisEvent)
    {
        // Logic to mark crisis as resolving or apply fix
        // This likely involves interacting with the repository to update status

        // As the interface defines CrisisEvent (value object) but Repo uses Crisis (entity), 
        // implementation details would map between them.

        // Example placeholder for repository interaction:
        // var activeCrises = _repository.GetActiveCrisesAsync(companyId).Result;
        // logic to find matching crisis and update it.
        // logic to find matching crisis and update it.
    }

    // ====================================================================
    // STATEFUL MANAGEMENT (Phase 5)
    // ====================================================================

    public bool ShouldTriggerCrisis(string companyId, int moraleScore, int activeRumorsCount)
    {
        // Simple logic: Low morale (< 40) OR High Rumors (> 5) triggers check
        // Add random component to avoid deterministic spam
        if (moraleScore < 40) return true;
        if (activeRumorsCount >= 5) return true;
        return false;
>>>>>>> temp-work
    }

    public void ResolveCrisis(string companyId, CrisisEvent crisisEvent)
    {
<<<<<<< HEAD
        // Logic to mark crisis as resolving or apply fix
        // This likely involves interacting with the repository to update status

        // As the interface defines CrisisEvent (value object) but Repo uses Crisis (entity), 
        // implementation details would map between them.

        // Example placeholder for repository interaction:
        // var activeCrises = _repository.GetActiveCrisesAsync(companyId).Result;
        // logic to find matching crisis and update it.
=======
        var crisis = new Crisis
        {
            CrisisId = 0, // Auto-increment in DB usually, but for new object 0 is fine
            CompanyId = companyId,
            CrisisType = "MoraleCollapse", // Default or derived from reason
            Stage = "WeakSignals",
            Severity = severity,
            Description = triggerReason,
            EscalationScore = 0,
            ResolutionAttempts = 0
        };

        // If repository supports async, use it. For now, assuming sync repo for simplicity or wrapping
        // _crisisRepository.SaveCrisisAsync(crisis); 
        await _crisisRepository.SaveCrisisAsync(crisis); // Assuming this method exists or will be added
        return crisis;
    }

    public async Task ProgressCrisesAsync(string companyId)
    {
        var activeCrises = await _crisisRepository.GetActiveCrisesAsync(companyId);
        foreach (var crisis in activeCrises)
        {
            // Simple escalation logic
            var updatedCrisis = crisis.IncreaseEscalation(10); // +10 per week
            if (updatedCrisis.EscalationScore >= 100)
            {
                updatedCrisis = updatedCrisis.Escalate();
            }
            await _crisisRepository.UpdateCrisisAsync(updatedCrisis);
        }
    }

    public async Task<List<Crisis>> GetCriticalCrisesAsync(string companyId)
    {
        var activeCrises = await _crisisRepository.GetActiveCrisesAsync(companyId);
        return activeCrises.Where(c => c.IsCritical()).ToList();
>>>>>>> temp-work
    }
}
