using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Models.Staff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Extensions du BookerAIEngine pour intégrer la conscience des ères et du staff créatif.
/// Permet au booker de prendre des décisions influencées par l'ère actuelle et les propositions du staff.
/// </summary>
public static class BookerAIEngineExtensions
{
    // ====================================================================
    // ERA-AWARE BOOKING DECISIONS
    // ====================================================================

    /// <summary>
    /// Propose un main event en tenant compte de l'ère actuelle de la compagnie.
    /// L'ère influence le type de match et les workers sélectionnés.
    /// </summary>
    public static async Task<(string Worker1Id, string Worker2Id)?> ProposeMainEventWithEraAwarenessAsync(
        this BookerAIEngine engine,
        string bookerId,
        string companyId,
        List<string> availableWorkers,
        int showImportance,
        IEraRepository eraRepository)
    {
        if (availableWorkers.Count < 2)
            return null;

        // Récupérer l'ère actuelle
        var currentEra = await eraRepository.GetCurrentEraAsync(companyId);
        if (currentEra == null)
        {
            // Fallback sur la méthode classique si pas d'ère
            return engine.ProposeMainEvent(bookerId, availableWorkers, showImportance);
        }

        // Ajuster la sélection des workers basée sur l'ère
        var eraInfluencedWorkers = FilterWorkersByEraPreferences(availableWorkers, currentEra);

        // Proposer le main event avec les workers filtrés
        return engine.ProposeMainEvent(bookerId, eraInfluencedWorkers, showImportance);
    }

    /// <summary>
    /// Évalue la qualité d'un match en tenant compte de l'alignement avec l'ère actuelle.
    /// Un match qui correspond à l'ère donne un bonus de qualité.
    /// </summary>
    public static async Task<int> EvaluateMatchQualityWithEraAlignmentAsync(
        this BookerAIEngine engine,
        string bookerId,
        string companyId,
        int matchRating,
        int fanReaction,
        string worker1Id,
        string worker2Id,
        int matchDuration,
        IEraRepository eraRepository)
    {
        // Score de base
        var baseQuality = engine.EvaluateMatchQuality(bookerId, matchRating, fanReaction, worker1Id, worker2Id);

        // Récupérer l'ère actuelle
        var currentEra = await eraRepository.GetCurrentEraAsync(companyId);
        if (currentEra == null)
            return baseQuality;

        // Bonus si le match correspond aux préférences de l'ère
        var eraBonus = CalculateEraAlignmentBonus(currentEra, matchDuration, matchRating);

        return Math.Clamp(baseQuality + eraBonus, 0, 100);
    }

    /// <summary>
    /// Détermine si le booker devrait initier une transition d'ère.
    /// Basé sur sa vision créative et la situation actuelle.
    /// </summary>
    public static async Task<(bool ShouldTransition, EraType? TargetEra, EraTransitionSpeed Speed)>
        ShouldInitiateEraTransitionAsync(
            this BookerAIEngine engine,
            string bookerId,
            string companyId,
            int fanSatisfaction,
            int audienceGrowth,
            IBookerRepository bookerRepository,
            IEraRepository eraRepository)
    {
        var booker = await bookerRepository.GetBookerByIdAsync(bookerId);
        if (booker == null)
            return (false, null, EraTransitionSpeed.Moderate);

        var currentEra = await eraRepository.GetCurrentEraAsync(companyId);
        if (currentEra == null)
            return (false, null, EraTransitionSpeed.Moderate);

        // Booker créatif (70+) est plus enclin à changer d'ère
        var transitionThreshold = booker.CreativityScore >= 70 ? 40 :
                                  booker.CreativityScore >= 50 ? 30 : 20;

        // Décider si transition basée sur fan satisfaction et croissance
        var shouldTransition = fanSatisfaction < 50 || audienceGrowth < transitionThreshold;

        if (!shouldTransition)
            return (false, null, EraTransitionSpeed.Moderate);

        // Déterminer l'ère cible basée sur la vision du booker
        var targetEra = DetermineTargetEra(booker, currentEra);

        // Déterminer la vitesse basée sur la créativité du booker
        // Un booker créatif est plus enclin aux changements rapides
        var speed = booker.CreativityScore >= 70 ? EraTransitionSpeed.Fast :
                    booker.CreativityScore >= 50 ? EraTransitionSpeed.Moderate :
                    booker.CreativityScore >= 30 ? EraTransitionSpeed.Slow :
                    EraTransitionSpeed.VerySlow;

        return (true, targetEra, speed);
    }

    // ====================================================================
    // STAFF CREATIVE INTEGRATION
    // ====================================================================

    /// <summary>
    /// Évalue une proposition créative d'un staff member.
    /// Prend en compte la compatibilité staff-booker et la qualité de la proposition.
    /// </summary>
    public static async Task<(bool IsAccepted, string Reason, int FinalScore)> EvaluateStaffProposalAsync(
        this BookerAIEngine engine,
        string bookerId,
        string staffId,
        int proposalQuality,
        int proposalOriginality,
        WorkerTypeBias proposalBias,
        IBookerRepository bookerRepository,
        IStaffCompatibilityRepository compatibilityRepository)
    {
        var booker = await bookerRepository.GetBookerByIdAsync(bookerId);
        if (booker == null)
            return (false, "Booker not found", 0);

        // Récupérer la compatibilité
        var compatibility = await compatibilityRepository.GetCompatibilityAsync(staffId, bookerId);
        if (compatibility == null)
            return (false, "Compatibility not calculated", 0);

        // Formule d'acceptation:
        // 40% qualité de la proposition
        // 30% compatibilité globale
        // 20% alignement de créativité
        // 10% originalité
        var baseScore = (int)(
            proposalQuality * 0.40 +
            compatibility.OverallScore * 0.30 +
            compatibility.CreativeVisionScore * 0.20 +
            proposalOriginality * 0.10
        );

        // Bonus si le booker est créatif et la proposition est originale
        if (booker.CreativityScore >= 70 && proposalOriginality >= 70)
        {
            baseScore += 10;
        }

        // Malus si le booker est logique et la proposition manque de cohérence
        if (booker.LogicScore >= 70 && proposalQuality < 50)
        {
            baseScore -= 15;
        }

        // Décision d'acceptation (seuil: 60)
        var isAccepted = baseScore >= 60;
        var reason = isAccepted
            ? $"Proposition acceptée (score: {baseScore}/100, compatibilité: {compatibility.OverallScore}/100)"
            : $"Proposition rejetée (score: {baseScore}/100 < 60, compatibilité: {compatibility.OverallScore}/100)";

        // Créer une mémoire de cette décision
        if (isAccepted)
        {
            engine.CreateMemoryFromMatch(
                bookerId,
                baseScore,
                $"Accepted creative proposal from staff {staffId} - Quality: {proposalQuality}");
        }

        return (isAccepted, reason, baseScore);
    }

    /// <summary>
    /// Détecte si un staff créatif est en train de "ruiner" une storyline.
    /// Permet au booker de réagir et potentiellement révoquer l'autorité du staff.
    /// </summary>
    public static async Task<(bool IsDamaging, int DamageScore, string Reason)> DetectStaffStorylineDamageAsync(
        this BookerAIEngine engine,
        string bookerId,
        string staffId,
        int storylineQualityBefore,
        int storylineQualityAfter,
        IStaffCompatibilityRepository compatibilityRepository,
        IStaffRepository staffRepository)
    {
        var compatibility = await compatibilityRepository.GetCompatibilityAsync(staffId, bookerId);
        if (compatibility == null)
            return (false, 0, "Compatibility unknown");

        var staff = await staffRepository.GetCreativeStaffByIdAsync(staffId);
        if (staff == null)
            return (false, 0, "Staff not found");

        // Calculer la dégradation
        var qualityDelta = storylineQualityAfter - storylineQualityBefore;

        // Si amélioration, pas de dégâts
        if (qualityDelta >= 0)
            return (false, 0, "Storyline quality maintained or improved");

        // Calculer le score de dégâts
        var damageScore = Math.Abs(qualityDelta);

        // Si compatibilité dangereuse ET staff peut ruiner storylines
        if (compatibility.IsDangerous() && staff.CanRuinStorylines)
        {
            damageScore = (int)(damageScore * 1.5); // Amplifier les dégâts
            return (true, damageScore,
                $"DANGEROUS: Low compatibility ({compatibility.OverallScore}/100) + Capable staff = High damage risk");
        }

        // Si compatibilité faible mais staff pas expert
        if (compatibility.OverallScore <= 50)
        {
            return (true, damageScore,
                $"Medium damage: Low compatibility ({compatibility.OverallScore}/100) affecting storyline");
        }

        return (false, damageScore, "Minor quality fluctuation");
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    /// <summary>
    /// Filtre les workers disponibles selon les préférences de l'ère actuelle.
    /// Par exemple, Technical era préfère les technical workers.
    /// </summary>
    private static List<string> FilterWorkersByEraPreferences(List<string> workers, Era era)
    {
        // En production, cela consulterait Workers table pour filtrer par style
        // Pour l'instant, retourner tous les workers (placeholder)
        // TODO: Implémenter le filtrage basé sur ProductStyle des workers
        return workers;
    }

    /// <summary>
    /// Calcule le bonus d'alignement entre un match et l'ère actuelle.
    /// </summary>
    private static int CalculateEraAlignmentBonus(Era era, int matchDuration, int matchRating)
    {
        var bonus = 0;

        // Bonus si durée du match correspond aux préférences de l'ère
        var durationDifference = Math.Abs(matchDuration - era.PreferredMatchDuration);
        if (durationDifference <= 3) // ± 3 minutes
        {
            bonus += 5;
        }

        // Bonus basé sur l'intensité de l'ère
        // Ère intense (70+) récompense les matchs de haute qualité (80+)
        if (era.Intensity >= 70 && matchRating >= 80)
        {
            bonus += 10;
        }

        // Ère modérée (40-70) récompense la consistance (60-79)
        if (era.Intensity >= 40 && era.Intensity < 70 && matchRating >= 60 && matchRating < 80)
        {
            bonus += 5;
        }

        return bonus;
    }

    /// <summary>
    /// Détermine l'ère cible pour une transition basée sur la vision du booker.
    /// </summary>
    private static EraType DetermineTargetEra(Booker booker, Era currentEra)
    {
        // Booker créatif préfère les ères créatives/expérimentales
        if (booker.CreativityScore >= 70)
        {
            return currentEra.Type switch
            {
                EraType.Technical => EraType.LuchaLibre,
                EraType.Mainstream => EraType.Entertainment,
                EraType.StrongStyle => EraType.Technical,
                _ => EraType.Entertainment
            };
        }

        // Booker logique préfère les ères structurées
        if (booker.LogicScore >= 70)
        {
            return currentEra.Type switch
            {
                EraType.Entertainment => EraType.SportsEntertainment,
                EraType.Hardcore => EraType.StrongStyle,
                _ => EraType.Technical
            };
        }

        // Default: transition vers Mainstream ou SportsEntertainment
        return EraType.SportsEntertainment;
    }
}
