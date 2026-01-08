using System;
using System.ComponentModel.DataAnnotations;

namespace RingGeneral.Core.Models.Booker;

/// <summary>
/// Représente une mémoire persistante du booker pour des décisions cohérentes.
/// Permet au booker de se souvenir de décisions passées et d'en tirer des leçons.
/// </summary>
public sealed record BookerMemory
{
    /// <summary>
    /// Identifiant unique de la mémoire
    /// </summary>
    public int MemoryId { get; init; }

    /// <summary>
    /// Identifiant du booker propriétaire de la mémoire
    /// </summary>
    [Required]
    public required string BookerId { get; init; }

    /// <summary>
    /// Type d'événement mémorisé:
    /// - GoodMatch: Match réussi (haute note)
    /// - BadMatch: Match raté (basse note)
    /// - WorkerComplaint: Plainte de worker sur le booking
    /// - FanReaction: Réaction des fans (positive/négative)
    /// - OwnerFeedback: Feedback du propriétaire
    /// - ChampionshipDecision: Décision de championnat
    /// - PushSuccess: Push de worker réussi
    /// - PushFailure: Push de worker raté
    /// </summary>
    [Required]
    public required string EventType { get; init; }

    /// <summary>
    /// Description textuelle de l'événement
    /// Ex: "Match John Cena vs Randy Orton à WrestleMania a reçu 5 étoiles"
    /// </summary>
    [Required]
    [StringLength(500)]
    public required string EventDescription { get; init; }

    /// <summary>
    /// Score d'impact de l'événement (-100 à +100)
    /// - Négatif: Événement négatif (échec, plainte, bad match)
    /// - Positif: Événement positif (succès, félicitations, good match)
    /// L'amplitude indique l'intensité
    /// </summary>
    [Range(-100, 100)]
    public required int ImpactScore { get; init; }

    /// <summary>
    /// Force de rappel (0-100)
    /// - 0-30: Mémoire faible, peu influente
    /// - 31-70: Mémoire modérée
    /// - 71-100: Mémoire forte, très influente sur futures décisions
    /// Décroît naturellement avec le temps (decay)
    /// </summary>
    [Range(0, 100)]
    public required int RecallStrength { get; init; }

    /// <summary>
    /// Date de création de la mémoire
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que la mémoire respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(BookerId))
        {
            errorMessage = "BookerId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(EventType))
        {
            errorMessage = "EventType ne peut pas être vide";
            return false;
        }

        var validEventTypes = new[]
        {
            "GoodMatch", "BadMatch", "WorkerComplaint", "FanReaction",
            "OwnerFeedback", "ChampionshipDecision", "PushSuccess", "PushFailure"
        };

        if (!validEventTypes.Contains(EventType))
        {
            errorMessage = $"EventType doit être: {string.Join(", ", validEventTypes)}";
            return false;
        }

        if (string.IsNullOrWhiteSpace(EventDescription) || EventDescription.Length > 500)
        {
            errorMessage = "EventDescription doit être entre 1 et 500 caractères";
            return false;
        }

        if (ImpactScore is < -100 or > 100)
        {
            errorMessage = "ImpactScore doit être entre -100 et 100";
            return false;
        }

        if (RecallStrength is < 0 or > 100)
        {
            errorMessage = "RecallStrength doit être entre 0 et 100";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si la mémoire est positive
    /// </summary>
    public bool IsPositive() => ImpactScore > 0;

    /// <summary>
    /// Détermine si la mémoire est négative
    /// </summary>
    public bool IsNegative() => ImpactScore < 0;

    /// <summary>
    /// Détermine si la mémoire est forte (influence importante)
    /// </summary>
    public bool IsStrong() => RecallStrength >= 70;

    /// <summary>
    /// Calcule le decay naturel de la mémoire avec le temps
    /// Les mémoires perdent 1 point de RecallStrength par semaine
    /// </summary>
    public BookerMemory ApplyDecay(int weeksPassed)
    {
        var newRecallStrength = Math.Max(0, RecallStrength - weeksPassed);
        return this with { RecallStrength = newRecallStrength };
    }

    /// <summary>
    /// Renforce la mémoire (rappel récurrent)
    /// </summary>
    public BookerMemory Reinforce(int strengthBonus)
    {
        var newRecallStrength = Math.Min(100, RecallStrength + strengthBonus);
        return this with { RecallStrength = newRecallStrength };
    }

    /// <summary>
    /// Calcule le poids d'influence de cette mémoire sur une décision future
    /// Combinaison de ImpactScore et RecallStrength
    /// </summary>
    public double GetInfluenceWeight()
    {
        // Formule: (ImpactScore * RecallStrength) / 100
        // Une mémoire avec ImpactScore=80 et RecallStrength=100 → poids = 80
        // Une mémoire avec ImpactScore=80 et RecallStrength=50 → poids = 40
        return (Math.Abs(ImpactScore) * RecallStrength) / 100.0;
    }

    /// <summary>
    /// Retourne une description lisible de l'intensité de la mémoire
    /// </summary>
    public string GetIntensityLabel()
    {
        var absImpact = Math.Abs(ImpactScore);
        return absImpact switch
        {
            >= 80 => "Très intense",
            >= 60 => "Intense",
            >= 40 => "Modéré",
            >= 20 => "Léger",
            _ => "Mineur"
        };
    }
}
