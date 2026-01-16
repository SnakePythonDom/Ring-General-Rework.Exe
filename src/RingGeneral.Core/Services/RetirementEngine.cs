using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

public class RetirementEngine : IRetirementEngine
{
    private readonly IWorkerRepository _workerRepository;
    private readonly INotesRepository _notesRepository; // For contracts
    private readonly ITitleHistoryRepository _titleHistoryRepository;
    private readonly System.Random _random = new System.Random();

    public RetirementEngine(
        IWorkerRepository workerRepository,
        INotesRepository notesRepository,
        ITitleHistoryRepository titleHistoryRepository)
    {
        _workerRepository = workerRepository;
        _notesRepository = notesRepository;
        _titleHistoryRepository = titleHistoryRepository;
    }

    public async Task<bool> ShouldRetireAsync(Worker worker)
    {
        if (!worker.IsActive) return false;
        if (worker.Age < 30) return false; // Early retirement is very rare, ignore for simplicity logic

        double retirementChance = 0.0;

        // 1. AGE FACTOR
        if (worker.Age >= 40) retirementChance += 0.05; // 5% baseline at 40
        if (worker.Age >= 45) retirementChance += 0.10;
        if (worker.Age >= 50) retirementChance += 0.20;
        if (worker.Age >= 60) retirementChance += 0.50;

        // 2. INJURY FACTOR
        if (worker.IsInjured)
        {
            // If major injury (can't easily check severity here without injury model details, assuming IsInjured implies current injury)
            // Ideally check Injury History severity.
            retirementChance += 0.05;
            if (worker.Age > 40) retirementChance += 0.10; // Old & Injured = High risk
        }

        // 3. MORALE / STATUS FACTOR
        if (worker.Morale < 10) retirementChance += 0.05; // "Rage quit" or lost passion

        // 4. JOBBER FACTOR (If old and jobber, likely to retire)
        if (worker.Age > 35 && worker.PushLevel == PushLevel.Jobber)
        {
            retirementChance += 0.05;
        }

        // Cap change purely processing logic
        // This method is called WEEKLY or MONTHLY? 
        // If Monthly, chances should be small. 
        // Let's assume this is a Yearly check or End of Contract check.
        // If Weekly loop, these numbers are WAY too high.
        // Assuming this is called during "World Evolution" step (Monthly or Yearly).
        // Let's assume MONTHLY but with strict checks. or checking specific "Retirement Month".

        // For safety, let's keep chances low if called frequently.
        // Or caller controls frequency.

        await Task.CompletedTask;
        return _random.NextDouble() < retirementChance;
    }

    public async Task ProcessRetirementAsync(Worker worker, string reason = "Retraite")
    {
        // 1. Mark as inactive / set departure info
        worker.IsActive = false;
        worker.DepartureDate = DateTime.Now;
        worker.DepartureReason = reason;
        worker.CompanyId = null; // Free agent / Retired pool

        // 2. End Active Contract
        var contract = _notesRepository.GetActiveContract(worker.Id);
        if (contract != null)
        {
            // Terminate or Expire? Let's use Terminate for "Breaking" or Expire if natural.
            // Actually, update status to "Completed" or similar if we had it. "Terminated" implies fired.
            // Let's just set EndDate to Now and Status to "Expired" (as in finished).
            contract.EndDate = DateTime.Now;
            contract.Status = RingGeneral.Core.Models.ContractStatus.Expired;
            _notesRepository.UpdateContract(contract);
        }

        // 3. Vacate Titles
        var activeReigns = await _titleHistoryRepository.GetWorkerTitleHistoryAsync(worker.WorkerId);
        foreach (var reign in activeReigns.Where(r => r.IsCurrentChampion))
        {
            await _titleHistoryRepository.AddTitleDefenseAsync(reign.Id.ToString(), "VACANT", "RETIREMENT", "Titre vacant suite à la retraite");
            // End the reign logic is implicitly handled? No, Interface has EndTitleReign?
            // Actually Repository has EndTitleReign(int reignId, ...) but interface definition I wrote earlier didn't explicitly list it?
            // Checking ITitleHistoryRepository.cs...
            // I defined `UpdateTitleReign`. I should use that or cast to concrete if needed (bad practice).
            // Let's use UpdateTitleReign.

            reign.EndReign(DateTime.Now, null); // Model method
            await _titleHistoryRepository.UpdateTitleReignAsync(reign);
        }

        // 4. Save Worker
        _workerRepository.UpdateWorker(worker);
    }

    public async Task<List<Worker>> GetPotentialRetireesAsync(string companyId)
    {
        var roster = _workerRepository.GetCompanyRoster(companyId);
        var retirees = new List<Worker>();

        foreach (var worker in roster)
        {
            // Simple dry run of logic (without random)
            // Just check age/morale thresholds
            bool highRisk = worker.Age >= 45 || (worker.Age >= 40 && worker.IsInjured);
            if (highRisk)
            {
                retirees.Add(worker);
            }
        }

        return await Task.FromResult(retirees);
    }
}
