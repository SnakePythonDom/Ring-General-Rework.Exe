using System;
using System.ComponentModel.DataAnnotations;

namespace RingGeneral.Core.Models.Roster;

/// <summary>
/// ADN du roster - Composition stylistique d'une fédération
/// </summary>
public sealed record RosterDNA
{
    /// <summary>
    /// Identifiant unique de l'ADN
    /// </summary>
    [Required]
    public required string DnaId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Pourcentage Hardcore (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double HardcorePercentage { get; init; }

    /// <summary>
    /// Pourcentage Technique (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double TechnicalPercentage { get; init; }

    /// <summary>
    /// Pourcentage Lucha Libre (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double LuchaPercentage { get; init; }

    /// <summary>
    /// Pourcentage Entertainment (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double EntertainmentPercentage { get; init; }

    /// <summary>
    /// Pourcentage Strong Style (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double StrongStylePercentage { get; init; }

    /// <summary>
    /// Style dominant (celui avec le plus haut pourcentage)
    /// </summary>
    [Required]
    public required string DominantStyle { get; init; }

    /// <summary>
    /// Score de cohérence (0-100)
    /// À quel point le roster est homogène stylistiquement
    /// </summary>
    [Range(0, 100)]
    public required double CoherenceScore { get; init; }

    /// <summary>
    /// Date de calcul
    /// </summary>
    public required DateTime CalculatedAt { get; init; }

    /// <summary>
    /// Valide que l'ADN respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(DnaId))
        {
            errorMessage = "DnaId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        // Vérifier que les pourcentages sont dans la plage valide
        var percentages = new[] 
        { 
            HardcorePercentage, 
            TechnicalPercentage, 
            LuchaPercentage, 
            EntertainmentPercentage, 
            StrongStylePercentage 
        };

        foreach (var percentage in percentages)
        {
            if (percentage < 0 || percentage > 100)
            {
                errorMessage = $"Les pourcentages doivent être entre 0 et 100";
                return false;
            }
        }

        // Vérifier que le style dominant est valide
        var validStyles = new[] { "Hardcore", "Technical", "Lucha", "Entertainment", "StrongStyle" };
        if (!validStyles.Contains(DominantStyle))
        {
            errorMessage = $"Style dominant invalide: {DominantStyle}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Retourne le pourcentage d'un style spécifique
    /// </summary>
    public double GetStylePercentage(string style)
    {
        return style switch
        {
            "Hardcore" => HardcorePercentage,
            "Technical" => TechnicalPercentage,
            "Lucha" => LuchaPercentage,
            "Entertainment" => EntertainmentPercentage,
            "StrongStyle" => StrongStylePercentage,
            _ => 0
        };
    }

    /// <summary>
    /// Calcule la distance entre deux ADN (0-100)
    /// Plus la distance est élevée, plus la transition est difficile
    /// </summary>
    public double CalculateDistance(RosterDNA other)
    {
        var hardcoreDiff = Math.Abs(HardcorePercentage - other.HardcorePercentage);
        var technicalDiff = Math.Abs(TechnicalPercentage - other.TechnicalPercentage);
        var luchaDiff = Math.Abs(LuchaPercentage - other.LuchaPercentage);
        var entertainmentDiff = Math.Abs(EntertainmentPercentage - other.EntertainmentPercentage);
        var strongStyleDiff = Math.Abs(StrongStylePercentage - other.StrongStylePercentage);

        // Distance moyenne pondérée
        var totalDiff = (hardcoreDiff + technicalDiff + luchaDiff + entertainmentDiff + strongStyleDiff) / 5.0;
        
        return Math.Clamp(totalDiff, 0, 100);
    }
}
