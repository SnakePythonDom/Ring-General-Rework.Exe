using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Trends;

/// <summary>
/// Tendance mondiale/régionale/locale affectant l'industrie du catch
/// </summary>
public sealed record Trend
{
    /// <summary>
    /// Identifiant unique de la tendance
    /// </summary>
    [Required]
    public required string TrendId { get; init; }

    /// <summary>
    /// Nom de la tendance (ex: "Lucha Boom", "Strong Style Era")
    /// </summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>
    /// Type de tendance (portée géographique)
    /// </summary>
    [Required]
    public required TrendType Type { get; init; }

    /// <summary>
    /// Catégorie de tendance
    /// </summary>
    [Required]
    public required TrendCategory Category { get; init; }

    /// <summary>
    /// Description de la tendance
    /// </summary>
    [Required]
    public required string Description { get; init; }

    /// <summary>
    /// Affinité Hardcore (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double HardcoreAffinity { get; init; }

    /// <summary>
    /// Affinité Technique (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double TechnicalAffinity { get; init; }

    /// <summary>
    /// Affinité Lucha Libre (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double LuchaAffinity { get; init; }

    /// <summary>
    /// Affinité Entertainment (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double EntertainmentAffinity { get; init; }

    /// <summary>
    /// Affinité Strong Style (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double StrongStyleAffinity { get; init; }

    /// <summary>
    /// Date de début de la tendance
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// Date de fin de la tendance (null si active)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Intensité de la tendance (0-100)
    /// </summary>
    [Range(0, 100)]
    public required int Intensity { get; init; }

    /// <summary>
    /// Durée prévue en semaines
    /// </summary>
    [Range(1, int.MaxValue)]
    public required int DurationWeeks { get; init; }

    /// <summary>
    /// Pénétration du marché (0-100)
    /// Pourcentage du marché affecté
    /// </summary>
    [Range(0, 100)]
    public required double MarketPenetration { get; init; }

    /// <summary>
    /// Régions affectées (sérialisé JSON)
    /// </summary>
    [Required]
    public required string AffectedRegions { get; init; }

    /// <summary>
    /// Indique si la tendance est active
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Valide que la tendance respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(TrendId))
        {
            errorMessage = "TrendId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            errorMessage = "Name ne peut pas être vide";
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
    /// Détermine si la tendance est active à une date donnée
    /// </summary>
    public bool IsActiveAt(DateTime date)
    {
        if (!IsActive) return false;
        if (date < StartDate) return false;
        if (EndDate.HasValue && date > EndDate.Value) return false;
        return true;
    }

    /// <summary>
    /// Calcule la progression de la tendance (0-100)
    /// </summary>
    public double GetProgress(DateTime currentDate)
    {
        if (!IsActiveAt(currentDate)) return 100;

        var elapsed = (currentDate - StartDate).TotalDays;
        var total = DurationWeeks * 7.0;
        
        return Math.Clamp(elapsed / total * 100, 0, 100);
    }
}
