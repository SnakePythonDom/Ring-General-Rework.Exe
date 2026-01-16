using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Relations;
using System;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

public class PhysicalDeclineService : IPhysicalDeclineService
{
    private readonly IWorkerRepository _workerRepository;

    public PhysicalDeclineService(IWorkerRepository workerRepository)
    {
        _workerRepository = workerRepository;
    }

    public async Task ApplyYearlyDeclineAsync(Worker worker)
    {
        await Task.CompletedTask;
        if (worker.DateOfBirth == default) return;
        if (worker.Age < 35)
        {
            // Prime years: Ensure potential growth is capped or check for early burnout
            // Update legacy score (peak tracking)
            await UpdateLegacyScoreAsync(worker);
            return;
        }

        // PHYSICAL ATTRIBUTES DECLINE
        // Starts mild at 35, accelerates at 40+
        double declineFactor = 0.0;
        if (worker.Age >= 35 && worker.Age < 40) declineFactor = 0.02; // 2%
        else if (worker.Age >= 40 && worker.Age < 50) declineFactor = 0.05; // 5%
        else if (worker.Age >= 50) declineFactor = 0.10; // 10%

        // High Flyers decline faster physically
        var isHighFlyer = worker.PrimarySpecialization?.Specialization == RingGeneral.Core.Models.SpecializationType.HighFlyer;

        if (isHighFlyer) declineFactor *= 1.5;

        // Apply to Physical Stats
        if (worker.InRingAttributes != null)
        {
            worker.InRingAttributes.Agility = ApplyDecline(worker.InRingAttributes.Agility, declineFactor);
            worker.InRingAttributes.Stamina = ApplyDecline(worker.InRingAttributes.Stamina, declineFactor);
            worker.InRingAttributes.Toughness = ApplyDecline(worker.InRingAttributes.Toughness, declineFactor);
            // Power declines slower than speed
            worker.InRingAttributes.Powerhouse = ApplyDecline(worker.InRingAttributes.Powerhouse, declineFactor * 0.5);
        }

        // MENTAL ATTRIBUTES GROWTH (Veteran Instincts)
        // Check for slight boost to Psychology/Basics if not senile (over 60?)
        if (worker.Age < 60 && worker.InRingAttributes != null && worker.MentalAttributes != null)
        {
            worker.InRingAttributes.Psychology = Math.Min(100, worker.InRingAttributes.Psychology + 1);
            worker.MentalAttributes.Consistency = Math.Min(100, worker.MentalAttributes.Consistency + 1);
        }

        await UpdateLegacyScoreAsync(worker);
        _workerRepository.UpdateWorker(worker);
    }

    public async Task ApplyInjuryEffectsAsync(Worker worker, string injuryType)
    {
        // Simple logic for now: major injuries reduce stats permanently
        int penalty = 0;
        switch (injuryType.ToLower())
        {
            case "knee":
            case "acl":
                penalty = 5; // Agility hit
                if (worker.InRingAttributes != null)
                    worker.InRingAttributes.Agility = Math.Max(1, worker.InRingAttributes.Agility - penalty);
                break;
            case "neck":
            case "spine":
                penalty = 5; // General impact
                if (worker.InRingAttributes != null)
                    worker.InRingAttributes.Toughness = Math.Max(1, worker.InRingAttributes.Toughness - penalty);
                break;
            case "concussion":
                // Mental stats impact?
                break;
        }

        _workerRepository.UpdateWorker(worker);
        await Task.CompletedTask;
    }

    private async Task TriggerRandomLifeEventAsync(WorkerRelation relation)
    {
        await Task.CompletedTask;
        // Friendship -> Brotherhood or Romance?
    }

    public async Task UpdateLegacyScoreAsync(Worker worker)
    {
        // Legacy Score tracks PEAK popularity/importance.
        // It never goes down, only up.

        // Simple formula: Popularity + (TvRole/2) + (Overness factors)
        int currentScore = worker.Popularity + (worker.TvRole / 2);

        // Add Major Title weight (if we had access to full title history here easily, but sticking to worker properties)
        if (worker.IsChampion) currentScore += 10;
        if (worker.PushLevel == PushLevel.MainEvent) currentScore += 20;

        if (currentScore > worker.LegacyScore)
        {
            worker.LegacyScore = currentScore;
            _workerRepository.UpdateWorker(worker);
        }
        await Task.CompletedTask;
    }

    private int ApplyDecline(int value, double percentage)
    {
        int reduction = (int)(value * percentage);
        if (reduction < 1) reduction = 1; // Minimum 1 point drop if any decline
        return Math.Max(1, value - reduction);
    }
}
