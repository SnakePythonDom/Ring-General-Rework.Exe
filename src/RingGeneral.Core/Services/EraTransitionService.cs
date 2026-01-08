using System;
using System.Collections.Generic;
using System.Linq;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Models.Company;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service de gestion des transitions d'ères pour une compagnie.
/// Gère les transitions progressives avec calcul d'impacts sur morale et audience.
/// </summary>
public class EraTransitionService
{
    /// <summary>
    /// Initialise une nouvelle transition d'ère
    /// </summary>
    /// <param name="fromEra">Ère actuelle</param>
    /// <param name="toEraType">Type de la nouvelle ère</param>
    /// <param name="speed">Vitesse de transition souhaitée</param>
    /// <param name="bookerId">ID du booker qui initie la transition</param>
    /// <returns>EraTransition configurée</returns>
    public EraTransition InitiateTransition(
        Era fromEra,
        EraType toEraType,
        EraTransitionSpeed speed,
        string? bookerId = null)
    {
        if (!fromEra.IsActive())
        {
            throw new InvalidOperationException("Impossible d'initier une transition depuis une ère inactive");
        }

        if (fromEra.Type == toEraType)
        {
            throw new InvalidOperationException("L'ère cible doit être différente de l'ère actuelle");
        }

        var startDate = DateTime.Now;
        var durationInDays = CalculateTransitionDuration(speed);
        var plannedEndDate = startDate.AddDays(durationInDays);

        var compatibility = fromEra.GetCompatibilityWith(toEraType);
        var changeResistance = CalculateChangeResistance(fromEra, compatibility, speed);
        var impacts = CalculateTransitionImpacts(fromEra, compatibility, speed, changeResistance);

        // Créer l'ère cible (placeholder, devra être créée séparément)
        var toEraId = $"era_{Guid.NewGuid():N}";

        return new EraTransition
        {
            TransitionId = $"transition_{Guid.NewGuid():N}",
            CompanyId = fromEra.CompanyId,
            FromEraId = fromEra.EraId,
            ToEraId = toEraId,
            StartDate = startDate,
            PlannedEndDate = plannedEndDate,
            ActualEndDate = null,
            ProgressPercentage = 0,
            Speed = speed,
            MoraleImpact = impacts.MoraleImpact,
            AudienceImpact = impacts.AudienceImpact,
            ChangeResistance = changeResistance,
            InitiatedByBookerId = bookerId,
            IsActive = true,
            Notes = $"Transition {fromEra.Type} -> {toEraType} initiée. Compatibilité: {compatibility}%",
            CreatedAt = DateTime.Now
        };
    }

    /// <summary>
    /// Met à jour la progression d'une transition active
    /// </summary>
    /// <param name="transition">Transition à mettre à jour</param>
    /// <param name="weeksPassed">Nombre de semaines écoulées depuis dernier update</param>
    /// <returns>Transition mise à jour</returns>
    public EraTransition UpdateTransitionProgress(EraTransition transition, int weeksPassed)
    {
        if (!transition.IsActive)
        {
            throw new InvalidOperationException("Impossible de mettre à jour une transition inactive");
        }

        if (transition.IsCompleted())
        {
            return transition;
        }

        // Calculer progression basée sur le temps écoulé et la vitesse
        var totalDuration = transition.GetPlannedDurationInDays();
        var currentDuration = transition.GetActualDurationInDays();
        var progressIncrement = (weeksPassed * 7.0 / totalDuration) * 100;

        var newProgress = Math.Min(100, transition.ProgressPercentage + (int)progressIncrement);

        // Ajuster les impacts en fonction de la progression
        var adjustedImpacts = AdjustImpactsBasedOnProgress(transition, newProgress);

        // Déterminer si la transition est terminée
        DateTime? actualEndDate = null;
        bool isActive = true;

        if (newProgress >= 100)
        {
            actualEndDate = DateTime.Now;
            isActive = false;
        }

        return transition with
        {
            ProgressPercentage = newProgress,
            MoraleImpact = adjustedImpacts.MoraleImpact,
            AudienceImpact = adjustedImpacts.AudienceImpact,
            ActualEndDate = actualEndDate,
            IsActive = isActive
        };
    }

    /// <summary>
    /// Calcule la durée de la transition en jours selon la vitesse
    /// </summary>
    private int CalculateTransitionDuration(EraTransitionSpeed speed)
    {
        return speed switch
        {
            EraTransitionSpeed.VerySlow => 365, // 12 mois
            EraTransitionSpeed.Slow => 270,     // 9 mois
            EraTransitionSpeed.Moderate => 180, // 6 mois
            EraTransitionSpeed.Fast => 90,      // 3 mois
            EraTransitionSpeed.Brutal => 30,    // 1 mois
            _ => 180
        };
    }

    /// <summary>
    /// Calcule la résistance au changement
    /// </summary>
    private int CalculateChangeResistance(Era fromEra, int compatibility, EraTransitionSpeed speed)
    {
        // Base: incompatibilité des ères
        var incompatibilityResistance = 100 - compatibility;

        // Bonus: ère actuelle mature = plus de résistance (attachement du public)
        var maturityBonus = fromEra.IsMature() ? 20 : 0;

        // Bonus: intensité élevée de l'ère actuelle = plus de résistance
        var intensityBonus = fromEra.Intensity >= 70 ? 15 : 0;

        // Malus: vitesse trop rapide = augmente résistance
        var speedPenalty = speed switch
        {
            EraTransitionSpeed.Brutal => 30,
            EraTransitionSpeed.Fast => 15,
            _ => 0
        };

        var totalResistance = (int)(incompatibilityResistance * 0.6) + maturityBonus + intensityBonus + speedPenalty;

        return Math.Clamp(totalResistance, 0, 100);
    }

    /// <summary>
    /// Calcule les impacts initiaux de la transition
    /// </summary>
    private (int MoraleImpact, int AudienceImpact) CalculateTransitionImpacts(
        Era fromEra,
        int compatibility,
        EraTransitionSpeed speed,
        int changeResistance)
    {
        // Impact moral: dépend de la vitesse et de la résistance
        var moraleImpact = speed switch
        {
            EraTransitionSpeed.VerySlow => 5,    // Minimal
            EraTransitionSpeed.Slow => 0,        // Neutre
            EraTransitionSpeed.Moderate => -5,   // Légèrement négatif
            EraTransitionSpeed.Fast => -15,      // Négatif
            EraTransitionSpeed.Brutal => -30,    // Très négatif
            _ => 0
        };

        // Ajuster selon résistance
        if (changeResistance >= 70)
        {
            moraleImpact -= 10; // Encore plus négatif si forte résistance
        }

        // Impact audience: dépend de la compatibilité des ères
        var audienceImpact = compatibility switch
        {
            >= 80 => 10,    // Positif si très compatible
            >= 60 => 5,     // Légèrement positif
            >= 40 => -5,    // Légèrement négatif
            >= 20 => -15,   // Négatif
            _ => -25        // Très négatif
        };

        // Ajuster selon vitesse
        if (speed == EraTransitionSpeed.Brutal)
        {
            audienceImpact -= 15; // Choc pour l'audience
        }

        return (
            MoraleImpact: Math.Clamp(moraleImpact, -50, 50),
            AudienceImpact: Math.Clamp(audienceImpact, -50, 50)
        );
    }

    /// <summary>
    /// Ajuste les impacts en fonction de la progression
    /// Les impacts deviennent moins négatifs au fil du temps (adaptation)
    /// </summary>
    private (int MoraleImpact, int AudienceImpact) AdjustImpactsBasedOnProgress(
        EraTransition transition,
        int newProgress)
    {
        var moraleImpact = transition.MoraleImpact;
        var audienceImpact = transition.AudienceImpact;

        // Après 50% de progression, impacts négatifs commencent à s'atténuer
        if (newProgress >= 50 && newProgress < 100)
        {
            var recoveryFactor = (newProgress - 50) / 50.0; // 0 à 1

            if (moraleImpact < 0)
            {
                moraleImpact = (int)(moraleImpact * (1 - recoveryFactor * 0.5));
            }

            if (audienceImpact < 0)
            {
                audienceImpact = (int)(audienceImpact * (1 - recoveryFactor * 0.5));
            }
        }

        // À 100%, impacts deviennent neutres ou positifs
        if (newProgress >= 100)
        {
            moraleImpact = Math.Max(0, moraleImpact);
            audienceImpact = Math.Max(0, audienceImpact);
        }

        return (
            MoraleImpact: Math.Clamp(moraleImpact, -50, 50),
            AudienceImpact: Math.Clamp(audienceImpact, -50, 50)
        );
    }

    /// <summary>
    /// Détermine si une transition peut être accélérée sans danger
    /// </summary>
    public bool CanAccelerateTransition(EraTransition transition)
    {
        // Impossible si déjà brutale ou complétée
        if (transition.Speed == EraTransitionSpeed.Brutal || transition.IsCompleted())
        {
            return false;
        }

        // Risqué si résistance élevée
        if (transition.ChangeResistance >= 70)
        {
            return false;
        }

        // Risqué si impacts déjà très négatifs
        if (transition.MoraleImpact <= -30 || transition.AudienceImpact <= -30)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si une transition peut être ralentie
    /// </summary>
    public bool CanSlowDownTransition(EraTransition transition)
    {
        // Impossible si déjà très lente ou complétée
        if (transition.Speed == EraTransitionSpeed.VerySlow || transition.IsCompleted())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Calcule le risque global d'une transition (0-100)
    /// </summary>
    public int CalculateOverallRisk(EraTransition transition)
    {
        var rejectionRisk = transition.CalculateRejectionRisk();
        var resistanceRisk = transition.ChangeResistance;
        var impactRisk = Math.Abs(transition.MoraleImpact + transition.AudienceImpact);

        var overallRisk = (int)((rejectionRisk * 0.4) + (resistanceRisk * 0.4) + (impactRisk * 0.2));

        return Math.Clamp(overallRisk, 0, 100);
    }

    /// <summary>
    /// Génère des recommandations pour gérer une transition
    /// </summary>
    public List<string> GetTransitionRecommendations(EraTransition transition)
    {
        var recommendations = new List<string>();

        var risk = CalculateOverallRisk(transition);

        if (risk >= 70)
        {
            recommendations.Add("⚠️ RISQUE ÉLEVÉ - Envisagez de ralentir la transition");
        }

        if (transition.ChangeResistance >= 70)
        {
            recommendations.Add("📢 Résistance élevée - Communiquez davantage avec le public et le roster");
        }

        if (transition.MoraleImpact <= -20)
        {
            recommendations.Add("😟 Moral en baisse - Organisez des réunions avec le roster");
        }

        if (transition.AudienceImpact <= -20)
        {
            recommendations.Add("📉 Audience en baisse - Ajustez le produit progressivement");
        }

        if (transition.Speed == EraTransitionSpeed.Brutal)
        {
            recommendations.Add("⚡ Transition BRUTALE - Attendez-vous à des réactions fortes");
        }

        if (transition.IsAheadOfSchedule())
        {
            recommendations.Add("✅ Transition en avance - Bon timing");
        }

        if (transition.ProgressPercentage >= 75 && risk <= 30)
        {
            recommendations.Add("🎯 Transition presque terminée - Continuez sur cette lancée");
        }

        return recommendations;
    }
}
