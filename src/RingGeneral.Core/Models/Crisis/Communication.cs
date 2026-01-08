using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RingGeneral.Core.Models.Crisis;

/// <summary>
/// Représente une communication backstage pour résoudre une crise.
/// 4 types: One-on-One, Locker Room Meeting, Public Statement, Mediation
/// </summary>
public sealed record Communication
{
    /// <summary>
    /// Identifiant unique de la communication
    /// </summary>
    public int CommunicationId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Identifiant de la crise (NULL si communication préventive)
    /// </summary>
    public int? CrisisId { get; init; }

    /// <summary>
    /// Type de communication:
    /// - One-on-One: Discussion privée 1-à-1
    /// - LockerRoomMeeting: Réunion de vestiaire (groupe)
    /// - PublicStatement: Déclaration publique
    /// - Mediation: Médiation entre parties
    /// </summary>
    [Required]
    public required string CommunicationType { get; init; }

    /// <summary>
    /// Initiateur de la communication (Owner, Booker, ou Worker ID)
    /// </summary>
    [Required]
    public required string InitiatorId { get; init; }

    /// <summary>
    /// Cible de la communication (Worker ID, NULL pour communications publiques)
    /// </summary>
    public string? TargetId { get; init; }

    /// <summary>
    /// Message de la communication
    /// </summary>
    [Required]
    [StringLength(500)]
    public required string Message { get; init; }

    /// <summary>
    /// Ton de la communication:
    /// - Diplomatic: Diplomatique, conciliant
    /// - Firm: Ferme, autoritaire
    /// - Apologetic: Apologétique, regrets
    /// - Confrontational: Confrontation directe
    /// </summary>
    [Required]
    public required string Tone { get; init; }

    /// <summary>
    /// Chance de succès estimée (0-100)
    /// Calculée par CommunicationEngine basé sur contexte
    /// </summary>
    [Range(0, 100)]
    public required int SuccessChance { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que la communication respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(InitiatorId))
        {
            errorMessage = "InitiatorId ne peut pas être vide";
            return false;
        }

        var validTypes = new[] { "One-on-One", "LockerRoomMeeting", "PublicStatement", "Mediation" };
        if (!validTypes.Contains(CommunicationType))
        {
            errorMessage = $"CommunicationType doit être: {string.Join(", ", validTypes)}";
            return false;
        }

        var validTones = new[] { "Diplomatic", "Firm", "Apologetic", "Confrontational" };
        if (!validTones.Contains(Tone))
        {
            errorMessage = $"Tone doit être: {string.Join(", ", validTones)}";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Message) || Message.Length > 500)
        {
            errorMessage = "Message doit être entre 1 et 500 caractères";
            return false;
        }

        if (SuccessChance is < 0 or > 100)
        {
            errorMessage = "SuccessChance doit être entre 0 et 100";
            return false;
        }

        // Validation métier: One-on-One et Mediation requièrent TargetId
        if (CommunicationType is "One-on-One" or "Mediation" && string.IsNullOrWhiteSpace(TargetId))
        {
            errorMessage = $"{CommunicationType} requiert un TargetId";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si la communication est liée à une crise
    /// </summary>
    public bool IsRelatedToCrisis() => CrisisId.HasValue;

    /// <summary>
    /// Détermine si la communication est préventive
    /// </summary>
    public bool IsPreventive() => !CrisisId.HasValue;

    /// <summary>
    /// Détermine si la communication cible un individu spécifique
    /// </summary>
    public bool IsTargeted() => !string.IsNullOrWhiteSpace(TargetId);

    /// <summary>
    /// Détermine si le ton est positif/conciliant
    /// </summary>
    public bool IsPositiveTone() => Tone is "Diplomatic" or "Apologetic";

    /// <summary>
    /// Détermine si le ton est négatif/agressif
    /// </summary>
    public bool IsNegativeTone() => Tone is "Firm" or "Confrontational";

    /// <summary>
    /// Calcule un bonus de chance de succès basé sur le ton
    /// </summary>
    public int GetToneBonus()
    {
        return Tone switch
        {
            "Diplomatic" => 10,  // +10% chance
            "Apologetic" => 5,   // +5% chance
            "Firm" => 0,          // Neutre
            "Confrontational" => -10, // -10% chance
            _ => 0
        };
    }

    /// <summary>
    /// Retourne un label descriptif pour le type
    /// </summary>
    public string GetTypeLabel()
    {
        return CommunicationType switch
        {
            "One-on-One" => "Discussion privée",
            "LockerRoomMeeting" => "Réunion de vestiaire",
            "PublicStatement" => "Déclaration publique",
            "Mediation" => "Médiation",
            _ => "Inconnu"
        };
    }

    /// <summary>
    /// Retourne un label descriptif pour le ton
    /// </summary>
    public string GetToneLabel()
    {
        return Tone switch
        {
            "Diplomatic" => "Diplomatique",
            "Firm" => "Ferme",
            "Apologetic" => "Apologétique",
            "Confrontational" => "Confrontation",
            _ => "Neutre"
        };
    }
}
