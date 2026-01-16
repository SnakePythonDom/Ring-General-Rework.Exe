using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Core.Models.Owner;
using RingGeneral.Core.Models.Relations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

public class BookerAIEngine : IBookerAIEngine
{
    private readonly IBookerRepository _bookerRepository;
    private readonly IRelationsRepository _relationsRepository;

    public BookerAIEngine(IBookerRepository bookerRepository, IRelationsRepository relationsRepository)
    {
        _bookerRepository = bookerRepository;
        _relationsRepository = relationsRepository;
    }

    public double GetPreferenceScore(string bookerId, string workerId)
    {
        double score = 50.0; // Base neutral score

        // 1. Memory / Trauma Check
        // Note: Repository is async but we are in a sync method. 
        // In a real scenario, we'd want this to be async, but following existing patterns for now.
        // Or we use .Result (caution!) or change interface.
        // Given IBookerAIEngine is currently sync, I'll use .Result/GetAwaiter().GetResult() 
        // to minimize breaking changes, or better, keep it as is if GameRepository already cached them.
        // But we want to break the circular dependency.

        var memories = _bookerRepository.GetBookerMemoriesAsync(bookerId).GetAwaiter().GetResult();
        var workerMemories = memories.Where(m => m.WorkerId == workerId);

        foreach (var memory in workerMemories)
        {
            score += memory.ImpactScore * (memory.RecallStrength / 100.0);
        }

        // 2. Relation / Bias Check
        var relations = _relationsRepository.GetRelationsForWorker(bookerId);
        var directRelation = relations.FirstOrDefault(r => r.InvolvesWorker(workerId));

        if (directRelation != null)
        {
            double relationMod = directRelation.RelationType switch
            {
                RelationType.Amitie => 15.0,
                RelationType.Fraternite => 25.0,
                RelationType.Protege => 30.0,
                RelationType.Rivalite => -15.0,
                RelationType.Haine => -30.0,
                _ => 0.0
            };

            score += relationMod * (directRelation.RelationStrength / 100.0);
        }

        return Math.Clamp(score, 0, 100);
    }

    public IEnumerable<string> GetTraumaWorkers(string bookerId)
    {
        var memories = _bookerRepository.GetBookerMemoriesAsync(bookerId).GetAwaiter().GetResult();
        return memories
            .Where(m => m.ImpactScore < -50 && (m.EventType == "InjuryDuringMatch" || m.EventType == "BadMatch"))
            .Select(m => m.WorkerId)
            .Where(id => id != null)
            .Distinct()!;
    }

    public IEnumerable<string> GetFavorites(string bookerId)
    {
        var relations = _relationsRepository.GetRelationsForWorker(bookerId);
        return relations
            .Where(r => r.RelationType == RelationType.Amitie ||
                        r.RelationType == RelationType.Fraternite ||
                        r.RelationType == RelationType.Protege)
            .Select(r => r.GetOtherWorkerId(bookerId))
            .Distinct();
    }

    public void ProcessDecisionImpact(string bookerId, string decisionType, string targetId, bool isBiased)
    {
        // Placeholder for future logic
    }

    public List<SegmentDefinition> GenerateAutoBooking(
        string bookerId,
        ShowContext showContext,
        List<SegmentDefinition>? existingSegments = null,
        AutoBookingConstraints? constraints = null)
    {
        var result = existingSegments?.ToList() ?? new List<SegmentDefinition>();
        var availableWorkers = showContext.Workers
            .Where(w => w.Fatigue < 100 && string.IsNullOrEmpty(w.Blessure))
            .ToList();

        // Exclure les workers déjà utilisés dans existingSegments
        var usedWorkerIds = result.SelectMany(s => s.Participants).ToHashSet();
        availableWorkers = availableWorkers.Where(w => !usedWorkerIds.Contains(w.WorkerId)).ToList();

        var targetCount = constraints?.MaxSegments ?? 6;
        var currentCount = result.Count;

        // Logique de booking simplifiée : créer des matchs 1 vs 1 jusqu'à atteindre le compte
        while (currentCount < targetCount && availableWorkers.Count >= 2)
        {
            // Trier par score de préférence pour le booker
            var sortedWorkers = availableWorkers
                .OrderByDescending(w => GetPreferenceScore(bookerId, w.WorkerId))
                .ToList();

            var w1 = sortedWorkers[0];
            var w2 = sortedWorkers[1];

            var segment = new SegmentDefinition(
                $"AI-{Guid.NewGuid():N}".ToUpperInvariant(),
                "match",
                new List<string> { w1.WorkerId, w2.WorkerId },
                10,
                false, // EstMainEvent
                null,  // StorylineId
                null,  // TitreId
                50,    // Intensite
                null,  // VainqueurId
                null,  // PerdantId
                true,  // IsBroadcast
                null   // Settings
            );

            result.Add(segment);
            availableWorkers.Remove(w1);
            availableWorkers.Remove(w2);
            currentCount++;
        }

        return result;
    }

    public (string Worker1Id, string Worker2Id)? ProposeMainEvent(string bookerId, List<string> availableWorkers, int showImportance)
    {
        if (availableWorkers.Count < 2) return null;

        var sorted = availableWorkers
            .OrderByDescending(w => GetPreferenceScore(bookerId, w))
            .Take(2)
            .ToList();

        return (sorted[0], sorted[1]);
    }

    public int EvaluateMatchQuality(string bookerId, int matchRating, int fanReaction, string worker1Id, string worker2Id)
    {
        return (matchRating + fanReaction) / 2;
    }

    public void CreateMemoryFromMatch(string bookerId, string companyId, int score, string description)
    {
        var memory = new BookerMemory
        {
            MemoryId = $"BM-{Guid.NewGuid()}",
            BookerId = bookerId,
            CompanyId = companyId,
            EventType = score >= 0 ? "PositiveFeedback" : "NegativeFeedback",
            EventDescription = description,
            ImpactScore = Math.Clamp(score, -100, 100),
            RecallStrength = 100,
            CreatedAt = DateTime.Now
        };

        _bookerRepository.SaveBookerMemoryAsync(memory).GetAwaiter().GetResult();
    }

    public void ApplyMemoryDecay(string bookerId)
    {
        ApplyMemoryDecay(bookerId, 1);
    }

    public void ApplyMemoryDecay(string bookerId, int weeksPassed)
    {
        var memories = _bookerRepository.GetBookerMemoriesAsync(bookerId).GetAwaiter().GetResult();
        foreach (var memory in memories)
        {
            var decayed = memory.ApplyDecay(weeksPassed);
            if (decayed.RecallStrength != memory.RecallStrength)
            {
                _bookerRepository.SaveBookerMemoryAsync(decayed).GetAwaiter().GetResult();
            }
        }
    }
}
