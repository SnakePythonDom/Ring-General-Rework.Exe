using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Core.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

/// <summary>
/// Moteur AI de booking permettant aux bookers de prendre des décisions autonomes.
/// Utilise les préférences du booker et ses mémoires pour cohérence long terme.
/// Intègre la conscience des ères pour influencer les décisions.
/// </summary>
public sealed class BookerAIEngine : IBookerAIEngine
{
    private readonly IBookerRepository? _bookerRepository;
    private readonly IEraRepository? _eraRepository;
    private readonly PersonalityDetectorService? _personalityDetector;
    private readonly IWorkerAttributesRepository? _workerAttributesRepository;
    private readonly System.Random _random = new();

    public BookerAIEngine(
        IBookerRepository? bookerRepository = null,
        IEraRepository? eraRepository = null,
        PersonalityDetectorService? personalityDetector = null,
        IWorkerAttributesRepository? workerAttributesRepository = null)
    {
        _bookerRepository = bookerRepository;
        _eraRepository = eraRepository;
        _personalityDetector = personalityDetector;
        _workerAttributesRepository = workerAttributesRepository;
    }

    /// <summary>
    /// Phase 1.1 - Récupère les préférences du creative staff pour influencer les décisions
    /// TODO: Implémenter quand ICreativeStaffRepository sera disponible
    /// </summary>
    private (string PreferredNarrativeStyle, int ChaosTolerance, bool PrefersLongStorylines) GetCreativeStaffPreferences(string companyId)
    {
        // Phase 1.1 - Pour l'instant, retourner des valeurs par défaut basées sur l'era si disponible
        // Sera étendu quand le système de Creative Staff sera implémenté
        var era = _eraRepository?.GetCurrentEraAsync(companyId).Result;
        if (era != null)
        {
            return era.Type switch
            {
                Enums.EraType.Technical => ("Technical", 30, true),
                Enums.EraType.Entertainment => ("Entertainment", 70, false),
                Enums.EraType.Hardcore => ("Hardcore", 80, false),
                _ => ("Balanced", 50, true)
            };
        }
        return ("Balanced", 50, true);
    }

    /// <summary>
    /// Applique les influences de l'era actuelle aux décisions de booking
    /// </summary>
    private SegmentDefinition ApplyEraInfluence(SegmentDefinition segment, Models.Company.Era? currentEra)
    {
        if (currentEra == null)
            return segment;

        // Adapter selon le type d'era
        return currentEra.Type switch
        {
            Enums.EraType.Technical => segment with {
                DureeMinutes = Math.Min(segment.DureeMinutes + 5, 30), // Matchs plus longs
                Intensite = Math.Min(segment.Intensite + 10, 100) // Plus technique
            },
            Enums.EraType.Entertainment => segment with {
                DureeMinutes = Math.Max(segment.DureeMinutes - 3, 5), // Segments plus courts
                Intensite = Math.Min(segment.Intensite + 20, 100) // Plus spectaculaire
            },
            Enums.EraType.Hardcore => segment with {
                Intensite = Math.Min(segment.Intensite + 25, 100) // Plus violent
            },
            _ => segment
        };
    }

    /// <summary>
    /// Évalue la stratégie à long terme basée sur l'archétype créatif du booker
    /// </summary>
    public string EvaluateLongTermStrategy(string bookerId, Models.ShowContext context)
    {
        if (_bookerRepository == null)
            return "Analyse impossible : Repository non disponible";

        var booker = _bookerRepository.GetBookerByIdAsync(bookerId).Result;
        if (booker == null)
            return "Booker introuvable";

        var memories = GetInfluentialMemories(bookerId);
        var currentEra = _eraRepository?.GetCurrentEraAsync(context.Show.CompagnieId).Result;
        var creativeStaffPrefs = GetCreativeStaffPreferences(context.Show.CompagnieId);

        return EvaluateLongTermStrategy(booker, context, memories, currentEra, creativeStaffPrefs);
    }

    /// <summary>
    /// Applique les préférences stylistiques de l'archétype créatif aux segments
    /// </summary>
    private SegmentDefinition ApplyArchetypeStyling(Booker booker, SegmentDefinition segment, int showImportance)
    {
        var (preferredDuration, preferredSegments, dominantStyle) = booker.GetArchetypePreferences();

        return booker.CreativeArchetype switch
        {
            BookerCreativeArchetype.PowerBooker => ApplyPowerBookerStyling(segment, showImportance),
            BookerCreativeArchetype.Puroresu => ApplyPuroresuStyling(segment, preferredDuration),
            BookerCreativeArchetype.AttitudeEra => ApplyAttitudeEraStyling(segment),
            BookerCreativeArchetype.ModernIndie => ApplyModernIndieStyling(segment),
            _ => segment
        };
    }

    /// <summary>
    /// Applique le style Power Booker : stabilité, hiérarchie claire, narration simple
    /// </summary>
    private SegmentDefinition ApplyPowerBookerStyling(SegmentDefinition segment, int showImportance)
    {
        if (segment.EstMainEvent && showImportance >= 70)
        {
            // Favoriser les champions établis, règnes longs
            return segment with { Intensite = Math.Min(segment.Intensite + 10, 100) };
        }

        // Rotation limitée, stabilité
        return segment with { Intensite = Math.Max(segment.Intensite - 5, 40) };
    }

    /// <summary>
    /// Applique le style Puroresu : priorité qualité in-ring, matchs longs
    /// </summary>
    private SegmentDefinition ApplyPuroresuStyling(SegmentDefinition segment, int preferredDuration)
    {
        if (segment.TypeSegment == "match")
        {
            // Matchs plus longs, priorité technique
            var newDuration = Math.Max(segment.DureeMinutes, preferredDuration);
            return segment with { DureeMinutes = newDuration, Intensite = 85 };
        }

        return segment;
    }

    /// <summary>
    /// Applique le style Attitude Era : segments percutants, chaos, Star Power
    /// </summary>
    private SegmentDefinition ApplyAttitudeEraStyling(SegmentDefinition segment)
    {
        if (segment.TypeSegment != "match")
        {
            // Segments plus importants
            return segment with { DureeMinutes = Math.Min(segment.DureeMinutes + 3, 15) };
        }
        else
        {
            // Matchs plus intenses et imprévisibles
            return segment with { Intensite = Math.Min(segment.Intensite + 15, 100) };
        }
    }

    /// <summary>
    /// Applique le style Modern/Indie : rotation élevée, renouvellement, gimmicks organiques
    /// </summary>
    private SegmentDefinition ApplyModernIndieStyling(SegmentDefinition segment)
    {
        if (segment.TypeSegment == "match")
        {
            // Matchs plus longs mais moins intenses que Puroresu
            return segment with { DureeMinutes = Math.Min(segment.DureeMinutes + 5, 25) };
        }

        return segment;
    }

    /// <summary>
    /// Phase 1.1 - Sélectionne les workers selon les préférences de l'archétype créatif
    /// </summary>
    public List<WorkerSnapshot> SelectWorkersByArchetype(
        Booker booker,
        List<WorkerSnapshot> availableWorkers,
        int showImportance,
        List<BookerMemory> memories)
    {
        var selected = booker.CreativeArchetype switch
        {
            BookerCreativeArchetype.PowerBooker => SelectPowerBookerWorkers(availableWorkers, showImportance),
            BookerCreativeArchetype.Puroresu => SelectPuroresuWorkers(availableWorkers),
            BookerCreativeArchetype.AttitudeEra => SelectAttitudeEraWorkers(availableWorkers),
            BookerCreativeArchetype.ModernIndie => SelectModernIndieWorkers(availableWorkers),
            _ => availableWorkers.OrderByDescending(w => w.Popularite).Take(10).ToList()
        };

        // Phase 3.3 - Filtrer les incompatibilités de personnalité
        return FilterPersonalityConflicts(selected);
    }

    /// <summary>
    /// Phase 1.1 - Détermine la structure des matchs selon l'archétype créatif
    /// </summary>
    public (int Duration, int Intensity, int Participants) DetermineMatchStructure(
        Booker booker,
        bool isMainEvent,
        int showImportance)
    {
        return booker.CreativeArchetype switch
        {
            BookerCreativeArchetype.PowerBooker => (
                isMainEvent ? 18 : 12,
                isMainEvent ? 70 : 60,
                2),
            BookerCreativeArchetype.Puroresu => (
                isMainEvent ? 30 : 20,
                85,
                2),
            BookerCreativeArchetype.AttitudeEra => (
                isMainEvent ? 15 : 10,
                90,
                2),
            BookerCreativeArchetype.ModernIndie => (
                isMainEvent ? 25 : 15,
                80,
                2),
            _ => (20, 75, 2)
        };
    }

    private List<WorkerSnapshot> SelectPowerBookerWorkers(List<WorkerSnapshot> workers, int showImportance)
    {
        if (showImportance >= 80)
        {
            // Shows importants : stars établies uniquement
            return workers
                .Where(w => w.Popularite >= 70)
                .OrderByDescending(w => w.Popularite)
                .Take(8)
                .ToList();
        }
        else
        {
            // Shows normaux : mix équilibré
            return workers
                .OrderByDescending(w => w.Popularite)
                .Take(10)
                .ToList();
        }
    }

    private List<WorkerSnapshot> SelectPuroresuWorkers(List<WorkerSnapshot> workers)
    {
        // Priorité absolue aux attributs techniques (InRing + Story pour la psychologie de match)
        return workers
            .OrderByDescending(w => w.InRing + w.Story)
            .Take(10)
            .ToList();
    }

    private List<WorkerSnapshot> SelectAttitudeEraWorkers(List<WorkerSnapshot> workers)
    {
        // Favorise Star Power et Entertainment
        return workers
            .OrderByDescending(w => w.Popularite + w.Entertainment)
            .Take(10)
            .ToList();
    }

    private List<WorkerSnapshot> SelectModernIndieWorkers(List<WorkerSnapshot> workers)
    {
        // Favorise mélange jeunes/talents montants
        return workers
            .Where(w => w.Popularite <= 60) // Pas trop de stars établies
            .OrderByDescending(w => w.InRing + w.Entertainment)
            .Take(12) // Plus de rotation
            .ToList();
    }

    /// <summary>
    /// Phase 3.3 - Filtre les workers avec des incompatibilités de personnalité connues
    /// </summary>
    private List<WorkerSnapshot> FilterPersonalityConflicts(List<WorkerSnapshot> workers)
    {
        if (_personalityDetector == null || _workerAttributesRepository == null)
            return workers; // Pas de filtrage si services non disponibles

        var filtered = new List<WorkerSnapshot>();
        var detectedProfiles = new Dictionary<string, PersonalityProfile>();

        // Détecter les personnalités pour chaque worker
        foreach (var worker in workers)
        {
            if (int.TryParse(worker.WorkerId, out var workerIdInt))
            {
                var mental = _workerAttributesRepository.GetMentalAttributes(workerIdInt);
                if (mental != null)
                {
                    var profile = _personalityDetector.DetectProfile(mental);
                    detectedProfiles[worker.WorkerId] = profile;
                }
            }
        }

        // Filtrer les profils dangereux/incompatibles
        var dangerousProfiles = new[]
        {
            PersonalityProfile.InstableChronique,
            PersonalityProfile.SaboteurPassif,
            PersonalityProfile.PoidsMort
        };

        // Éviter de mettre ensemble des profils toxiques
        var toxicProfiles = new[]
        {
            PersonalityProfile.Diva,
            PersonalityProfile.Égoïste,
            PersonalityProfile.Paresseux
        };

        // Compter les profils toxiques déjà sélectionnés
        var toxicCount = detectedProfiles.Values.Count(p => toxicProfiles.Contains(p));

        foreach (var worker in workers)
        {
            if (!detectedProfiles.TryGetValue(worker.WorkerId, out var profile))
            {
                filtered.Add(worker); // Si pas de profil détecté, inclure quand même
                continue;
            }

            // Exclure les profils dangereux sauf si nécessaire
            if (dangerousProfiles.Contains(profile))
            {
                // Ne pas exclure complètement, mais réduire la priorité
                // (pourrait être utile dans certains contextes narratifs)
                continue; // Pour l'instant, exclure complètement
            }

            // Limiter le nombre de profils toxiques ensemble (max 1 par show)
            if (toxicProfiles.Contains(profile) && toxicCount >= 1)
            {
                continue;
            }

            filtered.Add(worker);
            if (toxicProfiles.Contains(profile))
                toxicCount++;
        }

        return filtered.Any() ? filtered : workers; // Fallback si tous filtrés
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

        // Récupérer l'era actuelle de la compagnie pour influencer les décisions
        var currentEra = _eraRepository?.GetCurrentEraAsync(showContext.Show.CompagnieId).Result;

        // Phase 1.1 - Récupérer les préférences du creative staff si disponible
        var creativeStaffPreferences = GetCreativeStaffPreferences(showContext.Show.CompagnieId);

        // Phase 1.1 - Filtrer les workers disponibles selon les contraintes
        var availableWorkers = FilterAvailableWorkers(showContext, constraints, existingSegments);

        // Phase 1.1 - Sélectionner les workers selon l'archétype créatif pour stratégie long terme
        var archetypeWorkers = SelectWorkersByArchetype(booker, availableWorkers, 50, memories);
        availableWorkers = archetypeWorkers; // Utiliser les workers filtrés par archétype

        // Calculer la durée restante à remplir
        var existingDuration = existingSegments.Sum(s => s.DureeMinutes);
        var targetDuration = constraints.TargetDuration ?? showContext.Show.DureeMinutes;
        var remainingDuration = targetDuration - existingDuration;

        // Générer les segments
        var generatedSegments = new List<SegmentDefinition>();
        
        // Séparer les workers utilisés dans des matches vs segments non-match
        // Un worker peut apparaître dans un match ET dans un segment non-match, mais pas dans deux matches
        var usedWorkerIdsInMatches = new HashSet<string>(
            existingSegments
                .Where(s => s.TypeSegment == "match")
                .SelectMany(s => s.Participants)
        );
        var usedWorkerIdsInNonMatches = new HashSet<string>(
            existingSegments
                .Where(s => s.TypeSegment != "match")
                .SelectMany(s => s.Participants)
        );
        
        // Pour compatibilité avec le code existant, garder un HashSet global mais on l'utilisera différemment
        var usedWorkerIds = new HashSet<string>(
            existingSegments.SelectMany(s => s.Participants)
        );

        // Vérifier si un main event existe déjà
        var hasMainEvent = existingSegments.Any(s => s.EstMainEvent);

        // 1. Créer le main event si nécessaire et si durée suffisante
        if (!hasMainEvent && constraints.RequireMainEvent && remainingDuration >= 20)
        {
            var mainEvent = CreateMainEvent(booker, showContext, availableWorkers, usedWorkerIdsInMatches, memories, constraints);
            if (mainEvent != null)
            {
                // Appliquer les préférences stylistiques de l'archétype créatif
                mainEvent = ApplyArchetypeStyling(booker, mainEvent, 70); // Importance moyenne-élevée pour main event

                generatedSegments.Add(mainEvent);
                remainingDuration -= mainEvent.DureeMinutes;

                if (constraints.ForbidMultipleAppearances)
                {
                    // Main event est toujours un match, donc ajouter seulement à usedWorkerIdsInMatches
                    foreach (var participantId in mainEvent.Participants)
                    {
                        usedWorkerIdsInMatches.Add(participantId);
                        usedWorkerIds.Add(participantId); // Garder pour compatibilité
                    }
                }
            }
        }

        // 2. Utiliser les storylines actives si priorité
        if (constraints.PrioritizeActiveStorylines)
        {
            var storylineSegments = CreateStorylineSegments(
                booker, showContext, availableWorkers, usedWorkerIdsInMatches, usedWorkerIdsInNonMatches, memories, constraints, remainingDuration);

            foreach (var segment in storylineSegments)
            {
                if (remainingDuration < 10) break;

                generatedSegments.Add(segment);
                remainingDuration -= segment.DureeMinutes;

                if (constraints.ForbidMultipleAppearances)
                {
                    // Ajouter selon le type de segment
                    foreach (var participantId in segment.Participants)
                    {
                        if (segment.TypeSegment == "match")
                        {
                            usedWorkerIdsInMatches.Add(participantId);
                        }
                        else
                        {
                            usedWorkerIdsInNonMatches.Add(participantId);
                        }
                        usedWorkerIds.Add(participantId); // Garder pour compatibilité
                    }
                }
            }
        }

        // 3. Remplir avec des segments basés sur les préférences du booker
        while (remainingDuration >= 10 && generatedSegments.Count < constraints.MaxSegments)
        {
            var segment = CreateSegmentBasedOnPreferences(
                booker, showContext, availableWorkers, usedWorkerIdsInMatches, usedWorkerIdsInNonMatches, memories, constraints, remainingDuration);

            if (segment == null)
                break;

            // Phase 1.1 - Appliquer les préférences stylistiques de l'archétype créatif
            segment = ApplyArchetypeStyling(booker, segment, 50); // Importance moyenne pour segments secondaires

            // Phase 1.1 - Appliquer l'influence de l'era actuelle
            segment = ApplyEraInfluence(segment, currentEra);

            generatedSegments.Add(segment);
            remainingDuration -= segment.DureeMinutes;

            if (constraints.ForbidMultipleAppearances)
            {
                // Ajouter selon le type de segment
                foreach (var participantId in segment.Participants)
                {
                    if (segment.TypeSegment == "match")
                    {
                        usedWorkerIdsInMatches.Add(participantId);
                    }
                    else
                    {
                        usedWorkerIdsInNonMatches.Add(participantId);
                    }
                    usedWorkerIds.Add(participantId); // Garder pour compatibilité
                }
            }
        }

        // Phase 3.3 - Créer des mémoires pour les segments générés (stratégie long terme)
        foreach (var segment in generatedSegments)
        {
            if (segment.Participants.Count >= 2)
            {
                // Phase 3.3 - Créer une mémoire pour ce booking avec contexte narratif
                var participants = segment.Participants.Take(2).ToList();
                var memoryDescription = segment.EstMainEvent
                    ? $"Main Event: {string.Join(" vs ", participants)}"
                    : $"Segment: {segment.TypeSegment} avec {string.Join(" vs ", participants)}";
                
                // Phase 3.3 - Créer mémoire avec impact selon importance
                var impactScore = segment.EstMainEvent ? 70 : 50;
                CreateMemoryFromMatch(bookerId, impactScore, memoryDescription);
            }
        }

        // Phase 3.3 - Analyser les mémoires pour créer des arcs narratifs cohérents
        var ongoingRivalries = DetectOngoingRivalries(memories);
        var buildingStories = DetectBuildingStories(memories, generatedSegments);
        
        // Phase 3.3 - Si une rivalité existe, continuer l'arc narratif
        if (ongoingRivalries.Any())
        {
            var rivalry = ongoingRivalries.First();
            // Phase 3.3 - Continuer l'arc de rivalité détecté
            // Logger.Debug($"Continuing rivalry arc: {rivalry.Worker1Id} vs {rivalry.Worker2Id} (Shows: {rivalry.ShowCount})");
        }

        return generatedSegments;
    }

    /// <summary>
    /// Évalue la stratégie à long terme basée sur l'archétype créatif
    /// </summary>
    public string EvaluateLongTermStrategy(
        Booker booker,
        ShowContext context,
        List<BookerMemory> memories,
        Models.Company.Era? currentEra = null,
        (string PreferredNarrativeStyle, int ChaosTolerance, bool PrefersLongStorylines) creativeStaffPrefs = default)
    {
        var baseStrategy = booker.CreativeArchetype switch
        {
            BookerCreativeArchetype.PowerBooker =>
                EvaluatePowerBookerStrategy(booker, context, memories),
            BookerCreativeArchetype.Puroresu =>
                EvaluatePuroresuStrategy(booker, context, memories),
            BookerCreativeArchetype.AttitudeEra =>
                EvaluateAttitudeEraStrategy(booker, context, memories),
            BookerCreativeArchetype.ModernIndie =>
                EvaluateModernIndieStrategy(booker, context, memories),
            _ => "Maintenir équilibre entre stabilité et innovation"
        };

        // Intégrer l'influence de l'era actuelle
        if (currentEra != null)
        {
            baseStrategy += $" | Era {currentEra.Type}: {GetEraInfluenceDescription(currentEra)}";
        }

        // Intégrer les préférences du creative staff
        if (creativeStaffPrefs.PreferredNarrativeStyle != "Balanced")
        {
            baseStrategy += $" | Creative Staff: {creativeStaffPrefs.PreferredNarrativeStyle} narrative style";
        }

        return baseStrategy;
    }

    private string GetEraInfluenceDescription(Models.Company.Era era)
    {
        return era.Type switch
        {
            Enums.EraType.Technical => $"Focus technique ({era.PreferredMatchDuration}min/match)",
            Enums.EraType.Entertainment => $"Spectacle narratif ({era.PreferredSegmentCount} segments/show)",
            Enums.EraType.Hardcore => $"Intensité maximale (chaos toléré)",
            _ => "Style équilibré"
        };
    }

    private string EvaluatePowerBookerStrategy(Booker booker, ShowContext context, List<BookerMemory> memories)
    {
        var veteranMatches = memories.Count(m => m.EventDescription.Contains("veteran"));
        var stableChampions = context.Titres.Count(t => t.DetenteurId != null);

        if (veteranMatches > 5 && stableChampions >= 2)
            return "Stratégie optimale: Hiérarchie stable, champions vétérans dominants";

        return "Renforcer la stabilité: Favoriser les vétérans et maintenir les règnes de championnat";
    }

    private string EvaluatePuroresuStrategy(Booker booker, ShowContext context, List<BookerMemory> memories)
    {
        var technicalMatches = memories.Count(m => m.EventDescription.Contains("technical") || m.EventDescription.Contains("workrate"));
        var matchSegments = context.Segments.Where(s => s.TypeSegment == "match");
        var averageMatchLength = matchSegments.Any() ? matchSegments.Average(s => s.DureeMinutes) : 0;

        if (technicalMatches > 3 && averageMatchLength >= 25)
            return "Stratégie optimale: Produit technique de haute qualité, respect du puroresu";

        return "Améliorer la qualité: Augmenter durée des matchs et focus technique";
    }

    private string EvaluateAttitudeEraStrategy(Booker booker, ShowContext context, List<BookerMemory> memories)
    {
        var entertainmentSegments = context.Segments.Count(s => s.TypeSegment != "match");
        var highIntensityMatches = context.Segments.Count(s => s.Intensite >= 80);

        if (entertainmentSegments > 3 && highIntensityMatches > 2)
            return "Stratégie optimale: Chaos créatif maximal, segments percutants";

        return "Augmenter l'intensité: Plus de segments et matchs imprévisibles";
    }

    private string EvaluateModernIndieStrategy(Booker booker, ShowContext context, List<BookerMemory> memories)
    {
        var newChampions = memories.Count(m => m.EventDescription.Contains("new champion"));
        var youngWorkers = context.Workers.Count(w => w.Popularite <= 50);

        if (newChampions > 2 && youngWorkers >= 8)
            return "Stratégie optimale: Rotation constante, développement des jeunes talents";

        return "Accélérer le renouvellement: Couronner de nouveaux champions et pousser les jeunes";
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

        // Note: Le filtrage des workers déjà utilisés selon le type de segment
        // est maintenant géré de manière plus nuancée dans les méthodes de création
        // (CreateMainEvent, CreateSegmentBasedOnPreferences, CreateStorylineSegments)
        // qui vérifient séparément les workers dans des matches vs segments non-match

        return workers;
    }

    /// <summary>
    /// Crée un main event basé sur les préférences du booker et son archétype créatif
    /// </summary>
    private SegmentDefinition? CreateMainEvent(
        Booker booker,
        ShowContext context,
        List<WorkerSnapshot> availableWorkers,
        HashSet<string> usedWorkerIdsInMatches,
        List<BookerMemory> memories,
        AutoBookingConstraints constraints)
    {
        // Calculer l'importance du show (par défaut moyenne si pas disponible)
        var showImportance = 50; // Valeur par défaut moyenne

        // Sélectionner les workers selon l'archétype créatif du booker
        // Main event est toujours un match, donc vérifier seulement contre usedWorkerIdsInMatches
        var candidates = SelectWorkersByArchetype(booker, availableWorkers, showImportance, memories)
            .Where(w => !usedWorkerIdsInMatches.Contains(w.WorkerId))
            .ToList();

        if (candidates.Count < 2)
            return null;

        // Sélectionner les deux meilleurs workers
        var worker1 = candidates[0];
        var worker2 = candidates[1];

        // Phase 1.1 - Utiliser DetermineMatchStructure pour obtenir durée et intensité selon archétype
        var (duration, intensity, _) = DetermineMatchStructure(booker, true, showImportance);

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
        HashSet<string> usedWorkerIdsInMatches,
        HashSet<string> usedWorkerIdsInNonMatches,
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

            // Déterminer le type de segment selon PreferredProductType
            var segmentType = booker.PreferredProductType == "Entertainment" ? "promo" : "match";
            var duration = segmentType == "promo" ? 10 : 15;

            // Vérifier si les participants de la storyline sont disponibles
            // Utiliser le HashSet approprié selon le type de segment
            var relevantUsedWorkerIds = segmentType == "match" 
                ? usedWorkerIdsInMatches 
                : usedWorkerIdsInNonMatches;
            
            var storylineWorkerIds = storyline.Participants.Select(p => p.WorkerId).ToList();
            var availableStorylineWorkers = storylineWorkerIds
                .Where(wId => !relevantUsedWorkerIds.Contains(wId))
                .Where(wId => availableWorkers.Any(w => w.WorkerId == wId))
                .Take(2)
                .ToList();

            if (availableStorylineWorkers.Count < 2)
                continue;

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

            // Note: Les HashSets seront mis à jour dans GenerateAutoBooking après création
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
        HashSet<string> usedWorkerIdsInMatches,
        HashSet<string> usedWorkerIdsInNonMatches,
        List<BookerMemory> memories,
        AutoBookingConstraints constraints,
        int remainingDuration)
    {
        // Déterminer d'abord le type de segment qu'on va créer
        // (on le fait avant le filtrage pour utiliser le bon HashSet)
        string segmentType;
        int duration;
        int intensity;
        int participantCount;

        // Utiliser l'archétype créatif pour déterminer la structure
        var (defaultDuration, defaultIntensity, _) = DetermineMatchStructure(booker, false, 50);

        switch (booker.CreativeArchetype)
        {
            case BookerCreativeArchetype.PowerBooker:
                // Power Booker : Rotation limitée, matchs équilibrés
                segmentType = "match";
                duration = Math.Min(defaultDuration, remainingDuration);
                intensity = defaultIntensity;
                participantCount = 2;
                break;

            case BookerCreativeArchetype.Puroresu:
                // Puroresu : Priorité matchs longs et techniques
                segmentType = "match";
                duration = Math.Min(defaultDuration, remainingDuration);
                intensity = defaultIntensity;
                participantCount = 2;
                break;

            case BookerCreativeArchetype.AttitudeEra:
                // Attitude Era : Mix promos et matchs courts/intenses
                segmentType = _random.Next(3) == 0 ? "promo" : "match";
                duration = segmentType == "promo" 
                    ? Math.Min(12, remainingDuration)
                    : Math.Min(defaultDuration, remainingDuration);
                intensity = segmentType == "match" ? defaultIntensity : 0;
                participantCount = segmentType == "promo" ? _random.Next(1, 3) : 2;
                break;

            case BookerCreativeArchetype.ModernIndie:
                // Modern/Indie : Matchs longs, rotation élevée
                segmentType = "match";
                duration = Math.Min(defaultDuration, remainingDuration);
                intensity = defaultIntensity;
                participantCount = 2;
                break;

            default:
                // Fallback sur PreferredProductType si archétype non défini
                segmentType = _random.Next(4) == 0 ? "promo" : "match";
                duration = segmentType == "promo" ? 10 : 15;
                intensity = segmentType == "match" ? 70 : 0;
                participantCount = 2;
                break;
        }

        // Filtrer les workers selon le type de segment
        // Un worker peut apparaître dans un match ET dans un segment non-match, mais pas dans deux matches
        var relevantUsedWorkerIds = segmentType == "match" 
            ? usedWorkerIdsInMatches 
            : usedWorkerIdsInNonMatches;
        
        var candidates = availableWorkers
            .Where(w => !relevantUsedWorkerIds.Contains(w.WorkerId))
            .ToList();

        if (candidates.Count < participantCount)
            return null;

        // Phase 1.1 - Sélectionner les participants selon l'archétype créatif
        var selectedWorkers = SelectWorkersByArchetype(booker, candidates, 50, memories)
            .Take(participantCount)
            .ToList();
        
        if (selectedWorkers.Count < participantCount)
            return null;

        var participants = selectedWorkers;

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

    public RingGeneral.Core.Models.Decisions.RosterMoodReport GetRosterMoodReport(string companyId)
    {
        // Note: Cette implémentation nécessite l'accès à IMoraleRepository
        // Pour l'instant, retourner un rapport basique
        var reportId = Guid.NewGuid().ToString("N");
        return new RingGeneral.Core.Models.Decisions.RosterMoodReport
        {
            ReportId = reportId,
            CompanyId = companyId,
            AverageMorale = 50, // À calculer depuis le repository
            CriticalMoraleCount = 0, // À calculer depuis le repository
            WorkersRequestingPush = new List<string>(), // À calculer depuis le repository
            WorkersDissatisfiedWithStyle = new List<string>(), // À calculer depuis le repository
            Recommendations = "Surveiller le moral du roster. Considérer des ajustements de booking si nécessaire.",
            ReportedAt = DateTime.Now
        };
    }

    public bool ShouldAdaptToTrend(string companyId, RingGeneral.Core.Models.Trends.Trend trend)
    {
        // Le booker devrait s'adapter si :
        // - La tendance est forte (Intensity >= 70)
        // - La pénétration du marché est élevée (MarketPenetration >= 60)
        // - La tendance est globale ou régionale (pas locale)
        return trend.Intensity >= 70 &&
               trend.MarketPenetration >= 60 &&
               (trend.Type == RingGeneral.Core.Enums.TrendType.Global || trend.Type == RingGeneral.Core.Enums.TrendType.Regional);
    }

    public double CalculateAdaptationRisk(string companyId, RingGeneral.Core.Models.Trends.Trend trend)
    {
        // Le risque est basé sur :
        // - L'intensité de la tendance (plus intense = moins de risque)
        // - La pénétration du marché (plus élevée = moins de risque)
        // - Le type de tendance (locale = plus de risque)
        
        var intensityRisk = (100 - trend.Intensity) * 0.3;
        var penetrationRisk = (100 - trend.MarketPenetration) * 0.3;
        var typeRisk = trend.Type switch
        {
            RingGeneral.Core.Enums.TrendType.Global => 0,
            RingGeneral.Core.Enums.TrendType.Regional => 10,
            RingGeneral.Core.Enums.TrendType.Local => 30,
            _ => 20
        };

        var totalRisk = intensityRisk + penetrationRisk + typeRisk;
        return Math.Clamp(totalRisk, 0, 100);
    }

    /// <summary>
    /// Phase 3.3 - Détecte les rivalités en cours basées sur les mémoires
    /// </summary>
    private List<(string Worker1Id, string Worker2Id, int ShowCount)> DetectOngoingRivalries(List<BookerMemory> memories)
    {
        var rivalries = new Dictionary<(string, string), int>();

        foreach (var memory in memories.Where(m => m.EventType == "GoodMatch" || m.EventType == "BadMatch"))
        {
            // Extraire les worker IDs de la description (format: "Worker1 vs Worker2")
            var parts = memory.EventDescription.Split(new[] { " vs ", " VS " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var worker1 = parts[0].Trim();
                var worker2 = parts[1].Split(' ').First().Trim(); // Prendre le premier mot après "vs"
                
                var key = (worker1, worker2);
                if (!rivalries.ContainsKey(key))
                    rivalries[key] = 0;
                rivalries[key]++;
            }
        }

        // Retourner les rivalités avec au moins 2 shows
        return rivalries
            .Where(kvp => kvp.Value >= 2)
            .Select(kvp => (kvp.Key.Item1, kvp.Key.Item2, kvp.Value))
            .ToList();
    }

    /// <summary>
    /// Phase 3.3 - Détecte les histoires en construction basées sur les mémoires et segments générés
    /// </summary>
    private List<(string WorkerId, string StoryType, int Momentum)> DetectBuildingStories(
        List<BookerMemory> memories,
        List<SegmentDefinition> generatedSegments)
    {
        var stories = new Dictionary<string, (string StoryType, int Momentum)>();

        // Analyser les mémoires pour détecter les pushes/régressions
        foreach (var memory in memories)
        {
            if (memory.EventType == "PushSuccess")
            {
                // Extraire worker ID de la description
                var workerId = ExtractWorkerIdFromDescription(memory.EventDescription);
                if (workerId != null)
                {
                    stories[workerId] = ("Push", memory.ImpactScore);
                }
            }
            else if (memory.EventType == "PushFailure")
            {
                var workerId = ExtractWorkerIdFromDescription(memory.EventDescription);
                if (workerId != null)
                {
                    stories[workerId] = ("Regression", memory.ImpactScore);
                }
            }
        }

        // Analyser les segments générés pour détecter les workers avec momentum
        foreach (var segment in generatedSegments)
        {
            foreach (var participantId in segment.Participants)
            {
                if (!stories.ContainsKey(participantId))
                {
                    // Worker apparaît dans un segment = momentum positif
                    stories[participantId] = ("Building", 50);
                }
                else
                {
                    // Augmenter le momentum si déjà présent
                    var current = stories[participantId];
                    stories[participantId] = (current.StoryType, Math.Min(100, current.Momentum + 10));
                }
            }
        }

        return stories.Select(kvp => (kvp.Key, kvp.Value.StoryType, kvp.Value.Momentum)).ToList();
    }

    /// <summary>
    /// Phase 3.3 - Extrait un Worker ID d'une description de mémoire
    /// </summary>
    private string? ExtractWorkerIdFromDescription(string description)
    {
        // Format attendu: "Push de {WorkerId}" ou "{WorkerId} a réussi..."
        // Pour l'instant, retourner null car le format exact dépend de l'implémentation
        // TODO: Améliorer avec regex ou parsing plus sophistiqué
        return null;
    }
}
