using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Company;

/// <summary>
/// Profil de fédération de niche - Caractéristiques économiques et stratégiques
/// </summary>
public sealed record NicheFederationProfile
{
    /// <summary>
    /// Identifiant unique du profil
    /// </summary>
    [Required]
    public required string ProfileId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Indique si c'est une fédération de niche
    /// </summary>
    public required bool IsNicheFederation { get; init; }

    /// <summary>
    /// Type de niche
    /// </summary>
    public NicheType? NicheType { get; init; }

    /// <summary>
    /// Pourcentage d'audience captif (0-100)
    /// Audience qui ne baisse pas même si le produit est démodé
    /// </summary>
    [Range(0, 100)]
    public required double CaptiveAudiencePercentage { get; init; }

    /// <summary>
    /// Réduction dépendance TV (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double TvDependencyReduction { get; init; }

    /// <summary>
    /// Multiplicateur de merchandising (1.0-2.0)
    /// </summary>
    [Range(1.0, 2.0)]
    public required double MerchandiseMultiplier { get; init; }

    /// <summary>
    /// Stabilité billetterie (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double TicketSalesStability { get; init; }

    /// <summary>
    /// Réduction salaires demandés par les talents (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double TalentSalaryReduction { get; init; }

    /// <summary>
    /// Bonus loyauté talents (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double TalentLoyaltyBonus { get; init; }

    /// <summary>
    /// Indique si la niche a un plafond de croissance
    /// </summary>
    public required bool HasGrowthCeiling { get; init; }

    /// <summary>
    /// Taille maximale atteignable si plafond
    /// </summary>
    public CompanySize? MaxSize { get; init; }

    /// <summary>
    /// Date d'établissement de la niche
    /// </summary>
    public required DateTime EstablishedAt { get; init; }

    /// <summary>
    /// Date d'abandon de la niche (null si active)
    /// </summary>
    public DateTime? CeasedAt { get; init; }

    /// <summary>
    /// Valide que le profil respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(ProfileId))
        {
            errorMessage = "ProfileId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (IsNicheFederation && !NicheType.HasValue)
        {
            errorMessage = "NicheType est requis si IsNicheFederation est true";
            return false;
        }

        if (HasGrowthCeiling && !MaxSize.HasValue)
        {
            errorMessage = "MaxSize est requis si HasGrowthCeiling est true";
            return false;
        }

        if (CeasedAt.HasValue && CeasedAt.Value < EstablishedAt)
        {
            errorMessage = "CeasedAt ne peut pas être avant EstablishedAt";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si la niche est active
    /// </summary>
    public bool IsActive() => IsNicheFederation && !CeasedAt.HasValue;
}
