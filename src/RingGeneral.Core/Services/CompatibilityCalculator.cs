using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Roster;
using RingGeneral.Core.Models.Trends;
using System;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Calculateur de compatibilité entre tendances et ADN de roster
/// </summary>
public class CompatibilityCalculator
{
    private readonly ITrendRepository _trendRepository;

    public CompatibilityCalculator(ITrendRepository trendRepository)
    {
        _trendRepository = trendRepository ?? throw new ArgumentNullException(nameof(trendRepository));
    }

    /// <summary>
    /// Calcule le coefficient de compatibilité C entre une tendance et l'ADN d'un roster
    /// Formule: C = Σ(ADN[i] * Tendance[i]) / Σ(Tendance[i]²)
    /// </summary>
    public CompatibilityMatrix CalculateCompatibility(RosterDNA dna, Trend trend)
    {
        // Normaliser les pourcentages de l'ADN (somme = 100)
        var dnaSum = dna.HardcorePercentage + dna.TechnicalPercentage + dna.LuchaPercentage +
                     dna.EntertainmentPercentage + dna.StrongStylePercentage;
        
        var normalizedHardcore = dnaSum > 0 ? (dna.HardcorePercentage / dnaSum) * 100 : 0;
        var normalizedTechnical = dnaSum > 0 ? (dna.TechnicalPercentage / dnaSum) * 100 : 0;
        var normalizedLucha = dnaSum > 0 ? (dna.LuchaPercentage / dnaSum) * 100 : 0;
        var normalizedEntertainment = dnaSum > 0 ? (dna.EntertainmentPercentage / dnaSum) * 100 : 0;
        var normalizedStrongStyle = dnaSum > 0 ? (dna.StrongStylePercentage / dnaSum) * 100 : 0;

        // Normaliser les affinités de la tendance (somme = 100)
        var trendSum = trend.HardcoreAffinity + trend.TechnicalAffinity + trend.LuchaAffinity +
                       trend.EntertainmentAffinity + trend.StrongStyleAffinity;
        
        var normalizedTrendHardcore = trendSum > 0 ? (trend.HardcoreAffinity / trendSum) * 100 : 0;
        var normalizedTrendTechnical = trendSum > 0 ? (trend.TechnicalAffinity / trendSum) * 100 : 0;
        var normalizedTrendLucha = trendSum > 0 ? (trend.LuchaAffinity / trendSum) * 100 : 0;
        var normalizedTrendEntertainment = trendSum > 0 ? (trend.EntertainmentAffinity / trendSum) * 100 : 0;
        var normalizedTrendStrongStyle = trendSum > 0 ? (trend.StrongStyleAffinity / trendSum) * 100 : 0;

        // Calculer C = Σ(ADN[i] * Tendance[i]) / Σ(Tendance[i]²)
        var numerator = (normalizedHardcore * normalizedTrendHardcore) +
                       (normalizedTechnical * normalizedTrendTechnical) +
                       (normalizedLucha * normalizedTrendLucha) +
                       (normalizedEntertainment * normalizedTrendEntertainment) +
                       (normalizedStrongStyle * normalizedTrendStrongStyle);

        var denominator = Math.Pow(normalizedTrendHardcore, 2) +
                         Math.Pow(normalizedTrendTechnical, 2) +
                         Math.Pow(normalizedTrendLucha, 2) +
                         Math.Pow(normalizedTrendEntertainment, 2) +
                         Math.Pow(normalizedTrendStrongStyle, 2);

        var compatibilityCoefficient = denominator > 0 ? numerator / denominator : 0;

        // Déterminer le niveau de compatibilité
        var level = compatibilityCoefficient switch
        {
            > 1.2 => CompatibilityLevel.Alignment,
            > 0.8 => CompatibilityLevel.Hybridation,
            _ => CompatibilityLevel.Refusal
        };

        // Calculer les impacts
        double qualityBonus = 0;
        double growthMultiplier = 1.0;
        double nicheLoyaltyBonus = 0;
        double marketingCostReduction = 0;

        if (level == CompatibilityLevel.Alignment)
        {
            // Bonus pour alignement
            qualityBonus = Math.Min(100, (compatibilityCoefficient - 1.0) * 50);
            growthMultiplier = 1.0 + ((compatibilityCoefficient - 1.0) * 0.2);
        }
        else if (level == CompatibilityLevel.Refusal)
        {
            // Bonus pour niche
            nicheLoyaltyBonus = Math.Min(100, (1.0 - compatibilityCoefficient) * 30);
            marketingCostReduction = Math.Min(100, (1.0 - compatibilityCoefficient) * 20);
        }

        var matrixId = Guid.NewGuid().ToString("N");
        return new CompatibilityMatrix
        {
            MatrixId = matrixId,
            CompanyId = dna.CompanyId,
            TrendId = trend.TrendId,
            CompatibilityCoefficient = compatibilityCoefficient,
            Level = level,
            QualityBonus = qualityBonus,
            GrowthMultiplier = growthMultiplier,
            NicheLoyaltyBonus = nicheLoyaltyBonus,
            MarketingCostReduction = marketingCostReduction,
            CalculatedAt = DateTime.Now
        };
    }

    /// <summary>
    /// Calcule et sauvegarde la matrice de compatibilité
    /// </summary>
    public async Task<CompatibilityMatrix> CalculateAndSaveCompatibilityAsync(RosterDNA dna, Trend trend)
    {
        var matrix = CalculateCompatibility(dna, trend);
        await _trendRepository.SaveCompatibilityMatrixAsync(matrix);
        return matrix;
    }
}
