using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Trends;

/// <summary>
/// Matrice de compatibilité entre une tendance et l'ADN d'un roster
/// </summary>
public sealed record CompatibilityMatrix
{
    /// <summary>
    /// Identifiant unique de la matrice
    /// </summary>
    [Required]
    public required string MatrixId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Identifiant de la tendance
    /// </summary>
    [Required]
    public required string TrendId { get; init; }

    /// <summary>
    /// Coefficient de Compatibilité C
    /// C = Σ(ADN[i] * Tendance[i]) / Σ(Tendance[i]²)
    /// </summary>
    [Required]
    public required double CompatibilityCoefficient { get; init; }

    /// <summary>
    /// Niveau de compatibilité (interprétation de C)
    /// </summary>
    [Required]
    public required CompatibilityLevel Level { get; init; }

    /// <summary>
    /// Bonus qualité de show si C > 1 (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double QualityBonus { get; init; }

    /// <summary>
    /// Multiplicateur de croissance si C > 1
    /// </summary>
    [Range(0.5, 2.0)]
    public required double GrowthMultiplier { get; init; }

    /// <summary>
    /// Bonus fidélité niche si C < 1 (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double NicheLoyaltyBonus { get; init; }

    /// <summary>
    /// Réduction coûts marketing si C < 1 (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double MarketingCostReduction { get; init; }

    /// <summary>
    /// Date de calcul
    /// </summary>
    public required DateTime CalculatedAt { get; init; }

    /// <summary>
    /// Valide que la matrice respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(MatrixId))
        {
            errorMessage = "MatrixId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(TrendId))
        {
            errorMessage = "TrendId ne peut pas être vide";
            return false;
        }

        // Vérifier la cohérence entre le coefficient et le niveau
        var expectedLevel = CompatibilityCoefficient switch
        {
            > 1.2 => CompatibilityLevel.Alignment,
            > 0.8 => CompatibilityLevel.Hybridation,
            _ => CompatibilityLevel.Refusal
        };

        if (Level != expectedLevel)
        {
            errorMessage = $"Niveau de compatibilité incohérent avec le coefficient C={CompatibilityCoefficient:F2}";
            return false;
        }

        return true;
    }
}
