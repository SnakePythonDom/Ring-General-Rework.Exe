using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
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

    public List<SegmentDefinition> GenerateAutoBooking(
        string bookerId,
        ShowContext showContext,
        List<SegmentDefinition>? existingSegments = null,
        AutoBookingConstraints? constraints = null)
    {
        if (_bookerRepository == null)
            return new List<SegmentDefinition>();

        var booker = _bookerRepository.GetBookerByIdAsync(bookerId).Result;
        if (booker == null || !booker.CanAutoBook())
            return new List<SegmentDefinition>();

        // Initialiser les contraintes par défaut
        constraints ??= new AutoBookingConstraints();
        existingSegments ??= new List<SegmentDefinition>();

        // Récupérer les mémoires influentes
        var memories = GetInfluentialMemories(bookerId);

        // Filtrer les workers disponibles selon les contraintes
        var availableWorkers = FilterAvailableWorkers(showContext, constraints, existingSegments);

        // Calculer la durée restante à remplir
        var existingDuration = existingSegments.Sum(s => s.DureeMinutes);
        var targetDuration = constraints.TargetDuration ?? showContext.Show.DureeMinutes;
        var remainingDuration = targetDuration - existingDuration;

        // Générer les segments
        var generatedSegments = new List<SegmentDefinition>();
        var usedWorkerIds = new HashSet<string>(
            existingSegments.SelectMany(s => s.Participants)
        );

        // Vérifier si un main event existe déjà
        var hasMainEvent = existingSegments.Any(s => s.EstMainEvent);

        // 1. Créer le main event si nécessaire et si durée suffisante
        if (!hasMainEvent && constraints.RequireMainEvent && remainingDuration >= 20)
        {
            var mainEvent = CreateMainEvent(booker, showContext, availableWorkers, usedWorkerIds, memories, constraints);
            if (mainEvent != null)
            {
                generatedSegments.Add(mainEvent);
                remainingDuration -= mainEvent.DureeMinutes;

                if (constraints.ForbidMultipleAppearances)
                {
                    foreach (var participantId in mainEvent.Participants)
                    {
                        usedWorkerIds.Add(participantId);
                    }
                }
            }
        }

        // 2. Utiliser les storylines actives si priorité
        if (constraints.PrioritizeActiveStorylines)
        {
            var storylineSegments = CreateStorylineSegments(
                booker, showContext, availableWorkers, usedWorkerIds, memories, constraints, remainingDuration);

            foreach (var segment in storylineSegments)
            {
                if (remainingDuration < 10) break;

                generatedSegments.Add(segment);
                remainingDuration -= segment.DureeMinutes;

                if (constraints.ForbidMultipleAppearances)
                {
                    foreach (var participantId in segment.Participants)
                    {
                        usedWorkerIds.Add(participantId);
                    }
                }
            }
        }

        // 3. Remplir avec des segments basés sur les préférences du booker
        while (remainingDuration >= 10 && generatedSegments.Count < constraints.MaxSegments)
        {
            var segment = CreateSegmentBasedOnPreferences(
                booker, showContext, availableWorkers, usedWorkerIds, memories, constraints, remainingDuration);

            if (segment == null)
                break;

            generatedSegments.Add(segment);
            remainingDuration -= segment.DureeMinutes;

            if (constraints.ForbidMultipleAppearances)
            {
                foreach (var participantId in segment.Participants)
                {
                    usedWorkerIds.Add(participantId);
                }
            }
        }

        return generatedSegments;
    }

    // ====================================================================
    // HELPER METHODS FOR AUTO-BOOKING
    // ====================================================================

    /// <summary>
    /// Filtre les workers disponibles selon les contraintes
    /// </summary>
    private List<WorkerSnapshot> FilterAvailableWorkers(
        ShowContext context,
        AutoBookingConstraints constraints,
        List<SegmentDefinition> existingSegments)
    {
        var workers = context.Workers.ToList();

        // Exclure les workers bannis
        workers = workers.Where(w => !constraints.BannedWorkers.Contains(w.WorkerId)).ToList();

        // Exclure les workers blessés si interdit
        if (constraints.ForbidInjuredWorkers)
        {
            workers = workers.Where(w => string.IsNullOrEmpty(w.Blessure) || w.Blessure == "Aucune").ToList();
        }

        // Exclure les workers avec fatigue trop élevée
        workers = workers.Where(w => w.Fatigue <= constraints.MaxFatigueLevel).ToList();

        // Exclure les workers déjà utilisés si ForbidMultipleAppearances
        if (constraints.ForbidMultipleAppearances)
        {
            var usedWorkerIds = existingSegments.SelectMany(s => s.Participants).ToHashSet();
            workers = workers.Where(w => !usedWorkerIds.Contains(w.WorkerId)).ToList();
        }

        return workers;
    }

    /// <summary>
    /// Crée un main event basé sur les préférences du booker
    /// </summary>
    private SegmentDefinition? CreateMainEvent(
        Booker booker,
        ShowContext context,
        List<WorkerSnapshot> availableWorkers,
        HashSet<string> usedWorkerIds,
        List<BookerMemory> memories,
        AutoBookingConstraints constraints)
    {
        // Filtrer les workers non utilisés
        var candidates = availableWorkers
            .Where(w => !usedWorkerIds.Contains(w.WorkerId))
            .OrderByDescending(w => w.Popularite)
            .Take(10)
            .ToList();

        if (candidates.Count < 2)
            return null;

        // Sélectionner les deux meilleurs workers
        var worker1 = candidates[0];
        var worker2 = candidates[1];

        // Déterminer la durée selon PreferredProductType
        var duration = booker.PreferredProductType switch
        {
            "Puroresu" => 30, // Matchs longs
            "Hardcore" => 25,
            "Technical" => 25,
            _ => 20
        };

        // Déterminer l'intensité selon PreferredProductType
        var intensity = booker.PreferredProductType switch
        {
            "Hardcore" => 90, // Très intense
            "Puroresu" => 80,
            "Technical" => 70,
            "Entertainment" => 60,
            _ => 75
        };

        // Chercher un titre disponible
        string? titreId = null;
        if (constraints.UseTitles)
        {
            var availableTitle = context.Titres.FirstOrDefault(t =>
                t.DetenteurId != null &&
                (t.DetenteurId == worker1.WorkerId || t.DetenteurId == worker2.WorkerId));
            titreId = availableTitle?.TitreId;
        }

        return new SegmentDefinition(
            $"SEG-AUTO-{Guid.NewGuid():N}".ToUpperInvariant(),
            "match",
            new List<string> { worker1.WorkerId, worker2.WorkerId },
            duration,
            true, // EstMainEvent
            null, // StorylineId (sera défini si nécessaire)
            titreId,
            intensity,
            null, // VainqueurId (sera déterminé lors de la simulation)
            null, // PerdantId
            new Dictionary<string, string>()
        );
    }

    /// <summary>
    /// Crée des segments basés sur les storylines actives
    /// </summary>
    private List<SegmentDefinition> CreateStorylineSegments(
        Booker booker,
        ShowContext context,
        List<WorkerSnapshot> availableWorkers,
        HashSet<string> usedWorkerIds,
        List<BookerMemory> memories,
        AutoBookingConstraints constraints,
        int remainingDuration)
    {
        var segments = new List<SegmentDefinition>();

        // Filtrer les storylines actives
        var activeStorylines = context.Storylines
            .Where(s => s.Status == StorylineStatus.Active)
            .OrderByDescending(s => s.Heat)
            .ToList();

        foreach (var storyline in activeStorylines)
        {
            if (remainingDuration < 10 || segments.Count >= 3)
                break;

            // Vérifier si les participants de la storyline sont disponibles
            var storylineWorkerIds = storyline.Participants.Select(p => p.WorkerId).ToList();
            var availableStorylineWorkers = storylineWorkerIds
                .Where(wId => !usedWorkerIds.Contains(wId))
                .Where(wId => availableWorkers.Any(w => w.WorkerId == wId))
                .Take(2)
                .ToList();

            if (availableStorylineWorkers.Count < 2)
                continue;

            // Déterminer le type de segment selon PreferredProductType
            var segmentType = booker.PreferredProductType == "Entertainment" ? "promo" : "match";
            var duration = segmentType == "promo" ? 10 : 15;

            segments.Add(new SegmentDefinition(
                $"SEG-AUTO-{Guid.NewGuid():N}".ToUpperInvariant(),
                segmentType,
                availableStorylineWorkers,
                duration,
                false,
                storyline.StorylineId,
                null,
                segmentType == "match" ? 70 : 0,
                null,
                null,
                new Dictionary<string, string>()
            ));

            remainingDuration -= duration;

            if (constraints.ForbidMultipleAppearances)
            {
                foreach (var wId in availableStorylineWorkers)
                {
                    usedWorkerIds.Add(wId);
                }
            }
        }

        return segments;
    }

    /// <summary>
    /// Crée un segment basé sur les préférences du booker
    /// </summary>
    private SegmentDefinition? CreateSegmentBasedOnPreferences(
        Booker booker,
        ShowContext context,
        List<WorkerSnapshot> availableWorkers,
        HashSet<string> usedWorkerIds,
        List<BookerMemory> memories,
        AutoBookingConstraints constraints,
        int remainingDuration)
    {
        // Filtrer les workers non utilisés
        var candidates = availableWorkers
            .Where(w => !usedWorkerIds.Contains(w.WorkerId))
            .ToList();

        if (candidates.Count < 2)
            return null;

        // Déterminer le type de segment selon PreferredProductType
        string segmentType;
        int duration;
        int intensity;
        int participantCount;

        switch (booker.PreferredProductType)
        {
            case "Hardcore":
                segmentType = "match";
                duration = Math.Min(20, remainingDuration);
                intensity = 85;
                participantCount = 2;
                break;

            case "Puroresu":
                segmentType = "match";
                duration = Math.Min(25, remainingDuration);
                intensity = 75;
                participantCount = 2;
                break;

            case "Technical":
                segmentType = "match";
                duration = Math.Min(20, remainingDuration);
                intensity = 70;
                participantCount = 2;
                break;

            case "Entertainment":
                // Alterner entre promos et matchs
                segmentType = _random.Next(2) == 0 ? "promo" : "match";
                duration = segmentType == "promo" ? 10 : 15;
                intensity = segmentType == "match" ? 60 : 0;
                participantCount = segmentType == "promo" ? _random.Next(1, 3) : 2;
                break;

            default: // Balanced
                segmentType = _random.Next(4) == 0 ? "promo" : "match";
                duration = segmentType == "promo" ? 10 : 15;
                intensity = segmentType == "match" ? 70 : 0;
                participantCount = 2;
                break;
        }

        // Sélectionner les participants selon les préférences
        var participants = SelectParticipants(booker, candidates, participantCount, memories);

        if (participants.Count < participantCount)
            return null;

        return new SegmentDefinition(
            $"SEG-AUTO-{Guid.NewGuid():N}".ToUpperInvariant(),
            segmentType,
            participants.Select(p => p.WorkerId).ToList(),
            duration,
            false,
            null,
            null,
            intensity,
            null,
            null,
            new Dictionary<string, string>()
        );
    }

    /// <summary>
    /// Sélectionne les participants basé sur les préférences du booker
    /// </summary>
    private List<WorkerSnapshot> SelectParticipants(
        Booker booker,
        List<WorkerSnapshot> candidates,
        int count,
        List<BookerMemory> memories)
    {
        var scored = new List<(WorkerSnapshot worker, int score)>();

        foreach (var worker in candidates)
        {
            var score = 50; // Score de base

            // Appliquer les préférences du booker
            if (booker.LikesUnderdog && worker.Popularite < 40)
                score += 20;

            if (booker.LikesVeteran && worker.InRing >= 75)
                score += 20;

            if (booker.LikesFastRise && worker.Momentum > 60)
                score += 15;

            // Bonus pour popularité et skills
            score += worker.Popularite / 5;
            score += (worker.InRing + worker.Entertainment + worker.Story) / 15;

            // Bonus si mémoires positives
            var workerMemories = memories
                .Where(m => m.EventDescription.Contains(worker.WorkerId))
                .ToList();

            if (workerMemories.Any())
            {
                var avgImpact = workerMemories.Average(m => m.ImpactScore);
                score += (int)(avgImpact / 10);
            }

            // Aléatoire pour créativité
            if (booker.CreativityScore >= 70)
            {
                score += _random.Next(-15, 25);
            }

            scored.Add((worker, score));
        }

        return scored
            .OrderByDescending(s => s.score)
            .Take(count)
            .Select(s => s.worker)
            .ToList();
    }
}
