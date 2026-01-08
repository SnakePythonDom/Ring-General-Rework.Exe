using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Company;

/// <summary>
/// Représente une ère pour une compagnie de catch.
/// Une ère définit le style de produit, la structure des shows, et les attentes du public.
/// Les transitions d'ères sont progressives et peuvent causer des conflits si trop brusques.
/// </summary>
public sealed record Era
{
    /// <summary>
    /// Identifiant unique de l'ère
    /// </summary>
    [Required]
    public required string EraId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Type d'ère
    /// </summary>
    [Required]
    public required EraType Type { get; init; }

    /// <summary>
    /// Nom personnalisé de l'ère (optionnel)
    /// Ex: "Attitude Era", "Ruthless Aggression Era", "New Generation"
    /// </summary>
    public string? CustomName { get; init; }

    /// <summary>
    /// Date de début de l'ère
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// Date de fin de l'ère (null si ère actuelle)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Intensité de l'ère (0-100)
    /// - 0-30: Ère timide, peu marquée
    /// - 31-70: Ère modérée, équilibrée
    /// - 71-100: Ère très marquée, identité forte
    /// </summary>
    [Range(0, 100)]
    public required int Intensity { get; init; }

    /// <summary>
    /// Préférence de durée de match moyenne (en minutes)
    /// </summary>
    [Range(5, 60)]
    public int PreferredMatchDuration { get; init; } = 15;

    /// <summary>
    /// Nombre de segments par show préféré
    /// </summary>
    [Range(1, 20)]
    public int PreferredSegmentCount { get; init; } = 8;

    /// <summary>
    /// Ratio de matchs vs segments (0-100)
    /// - 0: Que des segments/promos
    /// - 50: Équilibré
    /// - 100: Que des matchs
    /// </summary>
    [Range(0, 100)]
    public int MatchToSegmentRatio { get; init; } = 60;

    /// <summary>
    /// Types de matchs dominants pour cette ère
    /// Ex: "Singles,TagTeam,Hardcore" pour une ère Hardcore
    /// </summary>
    public string DominantMatchTypes { get; init; } = "Singles";

    /// <summary>
    /// Attentes du public spécifiques à cette ère
    /// Ex: "Blood,Weapons,Intense" pour Hardcore
    /// </summary>
    public string AudienceExpectations { get; init; } = "Quality";

    /// <summary>
    /// Indique si c'est l'ère actuelle de la compagnie
    /// </summary>
    public bool IsCurrentEra { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que l'Era respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(EraId))
        {
            errorMessage = "EraId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (Intensity is < 0 or > 100)
        {
            errorMessage = "Intensity doit être entre 0 et 100";
            return false;
        }

        if (PreferredMatchDuration is < 5 or > 60)
        {
            errorMessage = "PreferredMatchDuration doit être entre 5 et 60 minutes";
            return false;
        }

        if (PreferredSegmentCount is < 1 or > 20)
        {
            errorMessage = "PreferredSegmentCount doit être entre 1 et 20";
            return false;
        }

        if (MatchToSegmentRatio is < 0 or > 100)
        {
            errorMessage = "MatchToSegmentRatio doit être entre 0 et 100";
            return false;
        }

        if (EndDate.HasValue && EndDate.Value < StartDate)
        {
            errorMessage = "EndDate ne peut pas être avant StartDate";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si l'ère est active
    /// </summary>
    public bool IsActive() => !EndDate.HasValue || EndDate.Value > DateTime.Now;

    /// <summary>
    /// Calcule la durée de l'ère en jours
    /// </summary>
    public int GetDurationInDays()
    {
        var end = EndDate ?? DateTime.Now;
        return (end - StartDate).Days;
    }

    /// <summary>
    /// Détermine si l'ère est mature (plus de 6 mois)
    /// </summary>
    public bool IsMature() => GetDurationInDays() >= 180;

    /// <summary>
    /// Retourne le nom d'affichage de l'ère
    /// </summary>
    public string GetDisplayName() => !string.IsNullOrWhiteSpace(CustomName) ? CustomName : $"{Type} Era";

    /// <summary>
    /// Calcule un score de compatibilité avec un type d'ère cible (0-100)
    /// Utilisé pour calculer la difficulté de transition
    /// </summary>
    public int GetCompatibilityWith(EraType targetType)
    {
        // Même type = 100% compatible
        if (Type == targetType) return 100;

        // Définir les compatibilités entre types d'ères
        return (Type, targetType) switch
        {
            // Technical est compatible avec Sports Entertainment et Strong Style
            (EraType.Technical, EraType.SportsEntertainment) => 70,
            (EraType.Technical, EraType.StrongStyle) => 85,
            (EraType.Technical, EraType.LuchaLibre) => 60,
            (EraType.Technical, EraType.Entertainment) => 40,
            (EraType.Technical, EraType.Hardcore) => 20,

            // Entertainment est compatible avec Mainstream et Sports Entertainment
            (EraType.Entertainment, EraType.SportsEntertainment) => 80,
            (EraType.Entertainment, EraType.Mainstream) => 85,
            (EraType.Entertainment, EraType.Technical) => 40,
            (EraType.Entertainment, EraType.Hardcore) => 50,

            // Hardcore est incompatible avec Mainstream et Developmental
            (EraType.Hardcore, EraType.Mainstream) => 10,
            (EraType.Hardcore, EraType.Developmental) => 20,
            (EraType.Hardcore, EraType.Entertainment) => 50,
            (EraType.Hardcore, EraType.StrongStyle) => 60,

            // Sports Entertainment est le plus flexible
            (EraType.SportsEntertainment, _) => 65,
            (_, EraType.SportsEntertainment) => 65,

            // Lucha Libre est compatible avec HighFlyer styles
            (EraType.LuchaLibre, EraType.Technical) => 60,
            (EraType.LuchaLibre, EraType.Entertainment) => 55,

            // Strong Style est compatible avec Technical et Hardcore
            (EraType.StrongStyle, EraType.Technical) => 85,
            (EraType.StrongStyle, EraType.Hardcore) => 60,
            (EraType.StrongStyle, EraType.Entertainment) => 30,

            // Developmental peut s'adapter à tout mais difficilement
            (EraType.Developmental, _) => 50,
            (_, EraType.Developmental) => 50,

            // Mainstream est incompatible avec Hardcore
            (EraType.Mainstream, EraType.Hardcore) => 10,
            (EraType.Mainstream, EraType.Entertainment) => 85,
            (EraType.Mainstream, EraType.Technical) => 50,

            // Par défaut, incompatibilité modérée
            _ => 40
        };
    }
}
