using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Morale;
using System;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

public class MoraleEngine : IMoraleEngine
{
    private const int MaxMorale = 100;
    private const int MinMorale = 0;
    private readonly IMoraleRepository _moraleRepository;

<<<<<<< HEAD
    public MoraleEngine(IMoraleRepository moraleRepository)
    {
        _moraleRepository = moraleRepository;
    }

    public void ApplyMoraleImpact(Worker worker, MoraleImpactType type)
    {
        int delta = CalculateDelta(type);

        // Adjust based on personality (using Mental Attributes)
        if (worker.MentalAttributes != null)
        {
            // Professionalism dampens negative impacts (Resilience)
            if (delta < 0 && worker.MentalAttributes.Professionnalisme > 70)
            {
                delta = (int)(delta * 0.5);
            }

            // Ambition amplifies positive impacts but also clarifies negative ones (Entitlement)
            if (worker.MentalAttributes.Ambition > 80)
            {
=======
    private readonly IWorkerRepository _workerRepository;

    public MoraleEngine(IMoraleRepository moraleRepository, IWorkerRepository workerRepository)
    {
        _moraleRepository = moraleRepository;
        _workerRepository = workerRepository;
    }

    public void ApplyMoraleImpact(string workerId, MoraleImpactType type)
    {
        var worker = _workerRepository.GetWorker(workerId);
        if (worker != null)
        {
            ApplyMoraleImpact(worker, type);
            _workerRepository.UpdateWorker(worker);
        }
    }

    public CompanyMorale CalculateCompanyMorale(string companyId)
    {
        // Placeholder for company-wide morale calculation
        // In real implementation, this would aggregate worker morale
        return new CompanyMorale
        {
            CompanyId = companyId,
            GlobalMoraleScore = 70,
            LastUpdated = DateTime.Now
        };
    }

    public IEnumerable<string> DetectWeakSignals(string companyId)
    {
        // Placeholder
        return new List<string>();
    }

    public void ApplyMoraleImpact(Worker worker, MoraleImpactType type)
    {
        int delta = CalculateDelta(type);

        // Adjust based on personality (using Mental Attributes)
        if (worker.MentalAttributes != null)
        {
            // Professionalism dampens negative impacts (Resilience)
            if (delta < 0 && worker.MentalAttributes.Professionnalisme > 70)
            {
                delta = (int)(delta * 0.5);
            }

            // Ambition amplifies positive impacts but also clarifies negative ones (Entitlement)
            if (worker.MentalAttributes.Ambition > 80)
            {
>>>>>>> temp-work
                if (delta > 0) delta += 2;
                if (type == MoraleImpactType.Buried || type == MoraleImpactType.LeftOffShow) delta -= 5;
            }

            // Loyalists are less affected by general losses but more by buried
            if (worker.MentalAttributes.Loyauté > 85 && type == MoraleImpactType.Loss)
            {
                delta = 0;
            }
        }

        // Apply change to Worker object
        worker.Morale = Math.Clamp(worker.Morale + delta, MinMorale, MaxMorale);

        // Persist to MoraleRepository as a BackstageMorale record
        // We do this fire-and-forget or we should eventually make the whole engine async.
        // For now, following current sync pattern of engines.
        SyncWithRepository(worker);
    }

    private int CalculateDelta(MoraleImpactType type)
    {
        return type switch
        {
            MoraleImpactType.Win => 2,
            MoraleImpactType.Loss => -1,
            MoraleImpactType.Buried => -10,
            MoraleImpactType.MainEventWin => 5,
            MoraleImpactType.TitleWin => 15,
            MoraleImpactType.TitleLoss => -5,
            MoraleImpactType.LeftOffShow => -3,
            MoraleImpactType.BonusPaid => 10,
            MoraleImpactType.FineIssued => -15,
            _ => 0
        };
    }

    private void SyncWithRepository(Worker worker)
    {
        // This is a bridge between the Worker model and the Morale persistence system
        var backstageMorale = new BackstageMorale
        {
            EntityId = worker.WorkerId,
            EntityType = "Worker",
            CompanyId = worker.CompanyId ?? "Unknown",
            MoraleScore = worker.Morale,
            LastUpdated = DateTime.Now
        };

        // Note: Repository is async. Syncing here might cause deadlocks if not careful, 
        // but given its a simple SQLite write, we'll use .GetAwaiter().GetResult() for now
        // to maintain the IMoraleEngine synchronous interface contract.
        _moraleRepository.SaveBackstageMoraleAsync(backstageMorale).GetAwaiter().GetResult();
    }

    public int CalculateWeeklyMoraleChange(Worker worker, int matchesThisWeek)
    {
        int change = 0;

        // Frustration from lack of booking
        if (matchesThisWeek == 0)
        {
            if (worker.PushLevel >= PushLevel.MainEvent) change -= 3;
            else if (worker.PushLevel >= PushLevel.MidCard) change -= 1;
        }

        // Overwork frustration
        if (matchesThisWeek > 3)
        {
            change -= (matchesThisWeek - 3);
        }

        return change;
    }
}
