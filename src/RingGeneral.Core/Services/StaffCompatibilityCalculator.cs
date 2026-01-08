using System;
using System.Collections.Generic;
using System.Linq;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Core.Models.Staff;

namespace RingGeneral.Core.Services;

/// <summary>
/// Calculateur de compatibilité entre staff créatif et booker.
/// Détermine si un staff créatif peut "ruiner" les storylines ou s'il est en synergie.
/// </summary>
public class StaffCompatibilityCalculator
{
    /// <summary>
    /// Calcule la compatibilité complète entre un staff créatif et un booker
    /// </summary>
    public StaffCompatibility CalculateCompatibility(
        CreativeStaff creativeStaff,
        Booker booker,
        StaffMember staffMember,
        int? existingSuccessfulCollaborations = null,
        int? existingFailedCollaborations = null,
        int? existingConflictHistory = null)
    {
        // 1. Vision créative
        var creativeVisionScore = CalculateCreativeVisionCompatibility(
            creativeStaff.CreativityScore,
            creativeStaff.ConsistencyScore,
            booker.CreativityScore,
            booker.LogicScore
        );

        // 2. Style de booking
        var bookingStyleScore = CalculateBookingStyleCompatibility(
            creativeStaff.LongTermStorylinePreference,
            booker.PreferredStyle,
            booker.LikesSlowBurn
        );

        // 3. Alignement des biais
        var biasAlignmentScore = CalculateBiasAlignment(
            creativeStaff.WorkerBias,
            creativeStaff.PreferredStyle,
            booker
        );

        // 4. Tolérance au risque
        var riskToleranceScore = CalculateRiskToleranceCompatibility(
            creativeStaff.CreativeRiskTolerance,
            booker.CreativityScore
        );

        // 5. Chimie personnelle
        var personalChemistryScore = CalculatePersonalChemistry(
            staffMember.PersonalityScore,
            booker.BiasResistance,
            existingConflictHistory ?? 0
        );

        // Score global (moyenne pondérée)
        var overallScore = (int)(
            creativeVisionScore * 0.25 +
            bookingStyleScore * 0.25 +
            biasAlignmentScore * 0.20 +
            riskToleranceScore * 0.15 +
            personalChemistryScore * 0.15
        );

        // Identifier facteurs positifs et négatifs
        var (positiveFactors, negativeFactors) = IdentifyFactors(
            creativeVisionScore,
            bookingStyleScore,
            biasAlignmentScore,
            riskToleranceScore,
            personalChemistryScore,
            creativeStaff,
            booker
        );

        return new StaffCompatibility
        {
            CompatibilityId = $"compat_{Guid.NewGuid():N}",
            StaffId = creativeStaff.StaffId,
            BookerId = booker.BookerId,
            OverallScore = Math.Clamp(overallScore, 0, 100),
            CreativeVisionScore = creativeVisionScore,
            BookingStyleScore = bookingStyleScore,
            BiasAlignmentScore = biasAlignmentScore,
            RiskToleranceScore = riskToleranceScore,
            PersonalChemistryScore = personalChemistryScore,
            PositiveFactors = positiveFactors,
            NegativeFactors = negativeFactors,
            SuccessfulCollaborations = existingSuccessfulCollaborations ?? 0,
            FailedCollaborations = existingFailedCollaborations ?? 0,
            ConflictHistory = existingConflictHistory ?? 0,
            LastCalculatedAt = DateTime.Now,
            CreatedAt = DateTime.Now
        };
    }

    /// <summary>
    /// Calcule la compatibilité de vision créative
    /// </summary>
    private int CalculateCreativeVisionCompatibility(
        int staffCreativity,
        int staffConsistency,
        int bookerCreativity,
        int bookerLogic)
    {
        // Comparaison créativité
        var creativityDiff = Math.Abs(staffCreativity - bookerCreativity);
        var creativityScore = 100 - creativityDiff;

        // Comparaison cohérence/logique
        var consistencyDiff = Math.Abs(staffConsistency - bookerLogic);
        var consistencyScore = 100 - consistencyDiff;

        // Bonus si les deux sont élevés (genius minds think alike)
        var geniusBonus = 0;
        if (staffCreativity >= 70 && bookerCreativity >= 70)
        {
            geniusBonus = 15;
        }

        var totalScore = (int)((creativityScore * 0.6) + (consistencyScore * 0.4) + geniusBonus);

        return Math.Clamp(totalScore, 0, 100);
    }

    /// <summary>
    /// Calcule la compatibilité de style de booking
    /// </summary>
    private int CalculateBookingStyleCompatibility(
        int staffLongTermPreference,
        string bookerPreferredStyle,
        bool bookerLikesSlowBurn)
    {
        var baseScore = 50;

        // Aligner préférence long-term avec style du booker
        if (bookerPreferredStyle == "Long-Term")
        {
            // Booker long-term aime staff qui préfère storylines longues
            baseScore = staffLongTermPreference;
        }
        else if (bookerPreferredStyle == "Short-Term")
        {
            // Booker short-term aime staff qui préfère angles courts
            baseScore = 100 - staffLongTermPreference;
        }
        else // Flexible
        {
            // Booker flexible s'adapte à tout
            baseScore = 70;
        }

        // Bonus si alignement avec SlowBurn
        if (bookerLikesSlowBurn && staffLongTermPreference >= 70)
        {
            baseScore += 15;
        }
        else if (!bookerLikesSlowBurn && staffLongTermPreference <= 30)
        {
            baseScore += 10;
        }

        return Math.Clamp(baseScore, 0, 100);
    }

    /// <summary>
    /// Calcule l'alignement des biais
    /// </summary>
    private int CalculateBiasAlignment(
        WorkerTypeBias staffBias,
        ProductStyle staffStyle,
        Booker booker)
    {
        var baseScore = 60; // Neutre par défaut

        // Vérifier alignement avec préférences du booker
        if (staffBias == WorkerTypeBias.Veterans && booker.LikesVeteran)
        {
            baseScore = 85;
        }
        else if (staffBias == WorkerTypeBias.Rookies && !booker.LikesVeteran)
        {
            baseScore = 80;
        }
        else if (staffBias == WorkerTypeBias.BigMen && booker.LikesFastRise)
        {
            // Big men + fast rise = incompatible généralement
            baseScore = 40;
        }

        // Ajuster selon résistance au biais du booker
        if (booker.BiasResistance >= 70)
        {
            // Booker méritocratique ignore les biais = toujours bon
            baseScore = Math.Max(baseScore, 75);
        }

        return Math.Clamp(baseScore, 0, 100);
    }

    /// <summary>
    /// Calcule la compatibilité de tolérance au risque
    /// </summary>
    private int CalculateRiskToleranceCompatibility(
        int staffRiskTolerance,
        int bookerCreativity)
    {
        // Staff très risk-taker avec booker très créatif = excellent
        if (staffRiskTolerance >= 70 && bookerCreativity >= 70)
        {
            return 90;
        }

        // Staff très conservateur avec booker peu créatif = bon
        if (staffRiskTolerance <= 30 && bookerCreativity <= 30)
        {
            return 75;
        }

        // Staff risk-taker avec booker peu créatif = incompatible
        if (staffRiskTolerance >= 70 && bookerCreativity <= 30)
        {
            return 30;
        }

        // Calculer différence
        var diff = Math.Abs(staffRiskTolerance - bookerCreativity);
        var score = 100 - diff;

        return Math.Clamp(score, 0, 100);
    }

    /// <summary>
    /// Calcule la chimie personnelle
    /// </summary>
    private int CalculatePersonalChemistry(
        int staffPersonality,
        int bookerBiasResistance,
        int conflictHistory)
    {
        // Score de base selon personnalité du staff
        var baseScore = staffPersonality;

        // Pénalité pour historique de conflits
        var conflictPenalty = Math.Min(conflictHistory * 10, 40);

        // Bonus si booker a haute résistance au biais (professionnel)
        var professionalismBonus = bookerBiasResistance >= 70 ? 10 : 0;

        var totalScore = baseScore + professionalismBonus - conflictPenalty;

        return Math.Clamp(totalScore, 0, 100);
    }

    /// <summary>
    /// Identifie les facteurs positifs et négatifs de compatibilité
    /// </summary>
    private (string PositiveFactors, string NegativeFactors) IdentifyFactors(
        int creativeVisionScore,
        int bookingStyleScore,
        int biasAlignmentScore,
        int riskToleranceScore,
        int personalChemistryScore,
        CreativeStaff staff,
        Booker booker)
    {
        var positives = new List<string>();
        var negatives = new List<string>();

        // Vision créative
        if (creativeVisionScore >= 80)
            positives.Add("Vision créative alignée");
        else if (creativeVisionScore <= 40)
            negatives.Add("Vision créative divergente");

        // Style de booking
        if (bookingStyleScore >= 80)
            positives.Add("Style de booking compatible");
        else if (bookingStyleScore <= 40)
            negatives.Add("Style de booking incompatible");

        // Biais
        if (biasAlignmentScore >= 80)
            positives.Add("Préférences alignées");
        else if (biasAlignmentScore <= 40)
            negatives.Add("Biais opposés");

        // Risque
        if (riskToleranceScore >= 80)
            positives.Add("Tolérance au risque compatible");
        else if (riskToleranceScore <= 40)
            negatives.Add("Approche créative trop différente");

        // Chimie
        if (personalChemistryScore >= 80)
            positives.Add("Excellente chimie personnelle");
        else if (personalChemistryScore <= 40)
            negatives.Add("Conflits de personnalité");

        // Spécifiques
        if (staff.CreativityScore >= 80 && booker.CreativityScore >= 80)
            positives.Add("Deux esprits créatifs brillants");

        if (staff.ConsistencyScore >= 80 && booker.LogicScore >= 80)
            positives.Add("Approche méthodique partagée");

        if (booker.BiasResistance >= 70)
            positives.Add("Booker méritocratique");

        return (
            PositiveFactors: positives.Any() ? string.Join(", ", positives) : "Aucun",
            NegativeFactors: negatives.Any() ? string.Join(", ", negatives) : "Aucun"
        );
    }

    /// <summary>
    /// Détermine si un staff créatif peut ruiner des storylines
    /// Basé sur incompatibilité + expérience
    /// </summary>
    public bool CanRuinStorylines(
        StaffCompatibility compatibility,
        StaffMember staffMember)
    {
        // Staff incompatible (< 30) avec haute expertise = dangereux
        if (compatibility.OverallScore <= 30)
        {
            // Si expert ou légende avec forte personnalité = peut imposer sa vision
            if (staffMember.ExpertiseLevel >= StaffExpertiseLevel.Expert &&
                staffMember.PersonalityScore <= 40)
            {
                return true;
            }

            // Si beaucoup d'expérience mais incompatible
            if (staffMember.YearsOfExperience >= 10 && staffMember.SkillScore >= 70)
            {
                return true;
            }
        }

        // Historique de conflits élevé
        if (compatibility.ConflictHistory >= 5)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Génère des recommandations pour améliorer la compatibilité
    /// </summary>
    public List<string> GetImprovementRecommendations(StaffCompatibility compatibility)
    {
        var recommendations = new List<string>();

        if (compatibility.OverallScore >= 80)
        {
            recommendations.Add("✅ Excellente compatibilité - Maintenez cette collaboration");
            return recommendations;
        }

        if (compatibility.CreativeVisionScore <= 40)
        {
            recommendations.Add("💡 Organisez des sessions de brainstorming pour aligner les visions");
        }

        if (compatibility.BookingStyleScore <= 40)
        {
            recommendations.Add("📋 Clarifiez les attentes sur le style de booking (long-term vs short-term)");
        }

        if (compatibility.BiasAlignmentScore <= 40)
        {
            recommendations.Add("⚖️ Discutez des biais et préférences pour trouver un terrain d'entente");
        }

        if (compatibility.PersonalChemistryScore <= 40)
        {
            recommendations.Add("🤝 Envisagez des sessions de médiation ou de team-building");
        }

        if (compatibility.ConflictHistory >= 3)
        {
            recommendations.Add("⚠️ Historique de conflits - Envisagez un changement de staff ou booker");
        }

        if (compatibility.CalculateCollaborationSuccessRate() <= 40)
        {
            recommendations.Add("📉 Taux de succès faible - Réduisez les responsabilités créatives");
        }

        return recommendations;
    }
}
