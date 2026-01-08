using System;
using System.ComponentModel.DataAnnotations;

namespace RingGeneral.Core.Models.Crisis;

/// <summary>
/// Représente le résultat d'une communication backstage.
/// Contient les impacts sur moral, relations, et escalade de crise.
/// </summary>
public sealed record CommunicationOutcome
{
    /// <summary>
    /// Identifiant unique du résultat
    /// </summary>
    public int OutcomeId { get; init; }

    /// <summary>
    /// Identifiant de la communication
    /// </summary>
    public required int CommunicationId { get; init; }

    /// <summary>
    /// La communication a-t-elle réussi?
    /// </summary>
    public required bool WasSuccessful { get; init; }

    /// <summary>
    /// Impact sur le moral (-50 à +50)
    /// - Négatif: Moral diminué
    /// - Positif: Moral amélioré
    /// </summary>
    [Range(-50, 50)]
    public required int MoraleImpact { get; init; }

    /// <summary>
    /// Impact sur les relations (-30 à +30)
    /// - Négatif: Relations détériorées
    /// - Positif: Relations améliorées
    /// </summary>
    [Range(-30, 30)]
    public required int RelationshipImpact { get; init; }

    /// <summary>
    /// Changement du score d'escalade de crise (-50 à +50)
    /// - Négatif: Crise apaisée
    /// - Positif: Crise aggravée
    /// </summary>
    [Range(-50, 50)]
    public required int CrisisEscalationChange { get; init; }

    /// <summary>
    /// Feedback textuel du résultat
    /// </summary>
    [StringLength(500)]
    public string? Feedback { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que le résultat respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (MoraleImpact is < -50 or > 50)
        {
            errorMessage = "MoraleImpact doit être entre -50 et 50";
            return false;
        }

        if (RelationshipImpact is < -30 or > 30)
        {
            errorMessage = "RelationshipImpact doit être entre -30 et 30";
            return false;
        }

        if (CrisisEscalationChange is < -50 or > 50)
        {
            errorMessage = "CrisisEscalationChange doit être entre -50 et 50";
            return false;
        }

        if (Feedback != null && Feedback.Length > 500)
        {
            errorMessage = "Feedback doit être maximum 500 caractères";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si le résultat est globalement positif
    /// </summary>
    public bool IsPositiveOutcome()
    {
        return WasSuccessful &&
               MoraleImpact >= 0 &&
               RelationshipImpact >= 0 &&
               CrisisEscalationChange <= 0; // Escalade réduite = positif
    }

    /// <summary>
    /// Détermine si le résultat est globalement négatif
    /// </summary>
    public bool IsNegativeOutcome()
    {
        return !WasSuccessful ||
               MoraleImpact < -20 ||
               RelationshipImpact < -15 ||
               CrisisEscalationChange > 20;
    }

    /// <summary>
    /// Calcule un score global d'impact (-100 à +100)
    /// </summary>
    public int CalculateOverallImpact()
    {
        // Formule pondérée:
        // - Moral: 40%
        // - Relations: 30%
        // - Escalade (inversé): 30%
        var moralePart = MoraleImpact * 0.4;
        var relationshipPart = RelationshipImpact * 0.3;
        var crisisPart = -CrisisEscalationChange * 0.3; // Inversé car réduction = bon

        var overall = moralePart + relationshipPart + crisisPart;

        return Math.Clamp((int)overall, -100, 100);
    }

    /// <summary>
    /// Retourne un label descriptif pour le succès
    /// </summary>
    public string GetSuccessLabel()
    {
        if (!WasSuccessful)
            return "Échec";

        var impact = CalculateOverallImpact();
        return impact switch
        {
            >= 40 => "Grand succès",
            >= 20 => "Succès",
            >= 0 => "Succès modéré",
            _ => "Succès mitigé"
        };
    }

    /// <summary>
    /// Génère un feedback automatique si non fourni
    /// </summary>
    public string GetFeedbackOrGenerate()
    {
        if (!string.IsNullOrWhiteSpace(Feedback))
            return Feedback;

        if (WasSuccessful)
        {
            return CalculateOverallImpact() switch
            {
                >= 40 => "La communication a été très bien reçue et a grandement amélioré la situation.",
                >= 20 => "La communication a été efficace et a apaisé les tensions.",
                >= 0 => "La communication a eu un effet positif mais modéré.",
                _ => "La communication a réussi mais avec des résultats mitigés."
            };
        }
        else
        {
            return CalculateOverallImpact() switch
            {
                <= -40 => "La communication a échoué et a considérablement aggravé la situation.",
                <= -20 => "La communication a échoué et a détérioré les relations.",
                _ => "La communication n'a pas eu l'effet escompté."
            };
        }
    }

    /// <summary>
    /// Crée un résultat positif standard
    /// </summary>
    public static CommunicationOutcome CreateSuccessful(int communicationId, int moraleBonus = 20, int relationshipBonus = 10)
    {
        return new CommunicationOutcome
        {
            CommunicationId = communicationId,
            WasSuccessful = true,
            MoraleImpact = moraleBonus,
            RelationshipImpact = relationshipBonus,
            CrisisEscalationChange = -30, // Réduit escalade
            Feedback = "Communication réussie.",
            CreatedAt = DateTime.Now
        };
    }

    /// <summary>
    /// Crée un résultat d'échec standard
    /// </summary>
    public static CommunicationOutcome CreateFailed(int communicationId, int moralePenalty = -15, int relationshipPenalty = -10)
    {
        return new CommunicationOutcome
        {
            CommunicationId = communicationId,
            WasSuccessful = false,
            MoraleImpact = moralePenalty,
            RelationshipImpact = relationshipPenalty,
            CrisisEscalationChange = 15, // Augmente escalade
            Feedback = "Communication échouée.",
            CreatedAt = DateTime.Now
        };
    }
}
