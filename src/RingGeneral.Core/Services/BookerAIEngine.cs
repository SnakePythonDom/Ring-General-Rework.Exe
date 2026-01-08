using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Booker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

/// <summary>
/// Moteur AI de booking permettant aux bookers de prendre des décisions autonomes.
/// Utilise les préférences du booker et ses mémoires pour cohérence long terme.
/// </summary>
public sealed class BookerAIEngine : IBookerAIEngine
{
    private readonly IBookerRepository? _bookerRepository;
    private readonly System.Random _random = new();

    public BookerAIEngine(IBookerRepository? bookerRepository = null)
    {
        _bookerRepository = bookerRepository;
    }

    public (string Worker1Id, string Worker2Id)? ProposeMainEvent(
        string bookerId,
        List<string> availableWorkers,
        int showImportance)
    {
        if (_bookerRepository == null || availableWorkers.Count < 2)
            return null;

        var booker = _bookerRepository.GetBookerByIdAsync(bookerId).Result;
        if (booker == null || !booker.CanAutoBook())
            return null;

        // Récupérer les mémoires influentes pour éclairer la décision
        var memories = GetInfluentialMemories(bookerId);

        // Sélectionner deux workers basés sur les préférences du booker
        var selectedWorkers = SelectWorkersForMainEvent(booker, availableWorkers, showImportance, memories);

        return selectedWorkers.Count >= 2 ? (selectedWorkers[0], selectedWorkers[1]) : null;
    }

    public int EvaluateMatchQuality(
        string bookerId,
        int matchRating,
        int fanReaction,
        string worker1Id,
        string worker2Id)
    {
        if (_bookerRepository == null)
            return 0;

        var booker = _bookerRepository.GetBookerByIdAsync(bookerId).Result;
        if (booker == null)
            return 0;

        // Formule d'évaluation:
        // - 60% match rating (qualité technique)
        // - 40% fan reaction (réception du public)
        var baseScore = (matchRating * 0.6) + ((fanReaction + 100) / 2 * 0.4);

        // Bonus si le booker est créatif et le match était innovant (rating > 85)
        if (booker.CreativityScore >= 70 && matchRating >= 85)
        {
            baseScore += 5; // Bonus créativité
        }

        // Bonus si le booker est logique et le match avait du sens narratif (fan reaction > 60)
        if (booker.LogicScore >= 70 && fanReaction >= 60)
        {
            baseScore += 5; // Bonus logique
        }

        return Math.Clamp((int)baseScore, 0, 100);
    }

    public void CreateMemoryFromMatch(string bookerId, int matchQuality, string matchDescription)
    {
        if (_bookerRepository == null)
            return;

        var booker = _bookerRepository.GetBookerByIdAsync(bookerId).Result;
        if (booker == null)
            return;

        // Déterminer le type d'événement et l'impact
        var eventType = matchQuality >= 70 ? "GoodMatch" : matchQuality <= 40 ? "BadMatch" : "GoodMatch";
        var impactScore = matchQuality >= 70 ? matchQuality : -(100 - matchQuality);

        // Déterminer la force de rappel basée sur la créativité/logique du booker
        // Un booker logique se souvient mieux des patterns
        var recallStrength = Math.Clamp(50 + (booker.LogicScore / 2), 0, 100);

        var memory = new BookerMemory
        {
            BookerId = bookerId,
            EventType = eventType,
            EventDescription = matchDescription,
            ImpactScore = impactScore,
            RecallStrength = recallStrength,
            CreatedAt = DateTime.Now
        };

        _bookerRepository.SaveBookerMemoryAsync(memory).Wait();
    }

    public bool ShouldPushWorker(
        string bookerId,
        string workerId,
        int workerPopularity,
        int workerSkill)
    {
        if (_bookerRepository == null)
            return false;

        var booker = _bookerRepository.GetBookerByIdAsync(bookerId).Result;
        if (booker == null)
            return false;

        // Logique de push basée sur les préférences du booker
        var shouldPush = false;

        // Si le booker aime les underdogs ET le worker a faible popularité
        if (booker.LikesUnderdog && workerPopularity < 40)
        {
            shouldPush = true;
        }

        // Si le booker aime les vétérans ET le worker a haute skill
        if (booker.LikesVeteran && workerSkill >= 75)
        {
            shouldPush = true;
        }

        // Si le booker aime les fast rise ET le worker a potentiel (skill >= 60, pop < 50)
        if (booker.LikesFastRise && workerSkill >= 60 && workerPopularity < 50)
        {
            shouldPush = true;
        }

        // Consulter les mémoires pour vérifier si ce worker a déjà été poussé avec succès
        var memories = _bookerRepository.GetBookerMemoriesAsync(bookerId).Result;
        var relevantMemories = memories
            .Where(m => m.EventType is "PushSuccess" or "PushFailure")
            .Where(m => m.EventDescription.Contains(workerId))
            .ToList();

        if (relevantMemories.Any())
        {
            var avgImpact = relevantMemories.Average(m => m.ImpactScore);
            // Si les mémoires passées sont positives, renforcer la décision
            if (avgImpact > 50)
            {
                shouldPush = true;
            }
            // Si les mémoires passées sont négatives, annuler la décision
            else if (avgImpact < -50)
            {
                shouldPush = false;
            }
        }

        return shouldPush;
    }

    public void ApplyMemoryDecay(string bookerId, int weeksPassed = 1)
    {
        if (_bookerRepository == null)
            return;

        var memories = _bookerRepository.GetBookerMemoriesAsync(bookerId).Result;

        foreach (var memory in memories)
        {
            var decayedMemory = memory.ApplyDecay(weeksPassed);
            _bookerRepository.UpdateBookerMemoryAsync(decayedMemory).Wait();
        }

        // Nettoyer les mémoires très faibles
        _bookerRepository.CleanupWeakMemoriesAsync(bookerId).Wait();
    }

    public List<BookerMemory> GetInfluentialMemories(string bookerId)
    {
        if (_bookerRepository == null)
            return new List<BookerMemory>();

        // Récupérer les mémoires fortes (RecallStrength >= 70)
        var strongMemories = _bookerRepository.GetStrongMemoriesAsync(bookerId).Result;

        // Récupérer aussi les mémoires récentes (12 dernières semaines) même si faibles
        var recentMemories = _bookerRepository.GetRecentMemoriesAsync(bookerId, 12).Result;

        // Combiner et dédupliquer
        var influential = strongMemories
            .Union(recentMemories)
            .OrderByDescending(m => m.GetInfluenceWeight())
            .Take(10) // Limiter aux 10 plus influentes
            .ToList();

        return influential;
    }

    public int CalculateBookerConsistency(string bookerId)
    {
        if (_bookerRepository == null)
            return 0;

        var booker = _bookerRepository.GetBookerByIdAsync(bookerId).Result;
        if (booker == null)
            return 0;

        var memories = _bookerRepository.GetBookerMemoriesAsync(bookerId).Result;
        if (!memories.Any())
            return booker.GetConsistencyScore(); // Score de base

        // Calculer la variance des ImpactScores
        // Un booker cohérent a des résultats homogènes (faible variance)
        var avgImpact = memories.Average(m => m.ImpactScore);
        var variance = memories.Average(m => Math.Pow(m.ImpactScore - avgImpact, 2));
        var stdDev = Math.Sqrt(variance);

        // Transformer en score de cohérence (0-100)
        // Faible stdDev = haute cohérence
        var consistencyFromMemories = Math.Clamp(100 - (int)(stdDev / 2), 0, 100);

        // Combiner avec le score de base du booker (CreativityScore + LogicScore) / 2
        var baseConsistency = booker.GetConsistencyScore();

        return (consistencyFromMemories + baseConsistency) / 2;
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    /// <summary>
    /// Sélectionne les workers pour un main event basé sur préférences et mémoires
    /// </summary>
    private List<string> SelectWorkersForMainEvent(
        Booker booker,
        List<string> availableWorkers,
        int showImportance,
        List<BookerMemory> memories)
    {
        // Simuler un scoring de chaque worker
        // En production, cela consulterait Workers table pour popularité/skill
        var workerScores = new Dictionary<string, int>();

        foreach (var workerId in availableWorkers)
        {
            var score = CalculateWorkerScore(booker, workerId, memories, showImportance);
            workerScores[workerId] = score;
        }

        // Retourner les 2 meilleurs
        return workerScores
            .OrderByDescending(kvp => kvp.Value)
            .Take(2)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    /// <summary>
    /// Calcule un score pour un worker basé sur les préférences du booker
    /// </summary>
    private int CalculateWorkerScore(
        Booker booker,
        string workerId,
        List<BookerMemory> memories,
        int showImportance)
    {
        var baseScore = 50; // Score de base

        // Bonus aléatoire pour créativité
        if (booker.CreativityScore >= 70)
        {
            baseScore += _random.Next(-10, 20); // Créatif = plus imprévisible
        }

        // Bonus pour logique (préférence workers cohérents)
        if (booker.LogicScore >= 70)
        {
            baseScore += 10; // Logique = préfère stabilité
        }

        // Consulter mémoires pour ce worker
        var workerMemories = memories
            .Where(m => m.EventDescription.Contains(workerId))
            .ToList();

        if (workerMemories.Any())
        {
            var avgImpact = workerMemories.Average(m => m.ImpactScore);
            baseScore += (int)(avgImpact / 10); // Ajuster basé sur mémoires passées
        }

        // Bonus si show important et booker aime les vétérans
        if (showImportance >= 70 && booker.LikesVeteran)
        {
            baseScore += 15;
        }

        return Math.Clamp(baseScore, 0, 100);
    }
}
