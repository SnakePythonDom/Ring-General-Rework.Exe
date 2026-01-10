using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Decisions;

/// <summary>
/// Rapport de viabilité d'une niche pour l'Owner
/// </summary>
public sealed record NicheViabilityReport
{
    /// <summary>
    /// Identifiant unique du rapport
    /// </summary>
    [Required]
    public required string ReportId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Type de niche évalué
    /// </summary>
    [Required]
    public required NicheType NicheType { get; init; }

    /// <summary>
    /// Score de viabilité (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double ViabilityScore { get; init; }

    /// <summary>
    /// Bénéfices économiques estimés
    /// </summary>
    [Range(0, double.MaxValue)]
    public required double EstimatedEconomicBenefits { get; init; }

    /// <summary>
    /// Risques identifiés
    /// </summary>
    [Required]
    public required string Risks { get; init; }

    /// <summary>
    /// Recommandation
    /// </summary>
    [Required]
    public required string Recommendation { get; init; }

    /// <summary>
    /// Date du rapport
    /// </summary>
    public required DateTime ReportedAt { get; init; }

    /// <summary>
    /// Valide que le rapport respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(ReportId))
        {
            errorMessage = "ReportId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Risks))
        {
            errorMessage = "Risks ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Recommendation))
        {
            errorMessage = "Recommendation ne peut pas être vide";
            return false;
        }

        return true;
    }
}
