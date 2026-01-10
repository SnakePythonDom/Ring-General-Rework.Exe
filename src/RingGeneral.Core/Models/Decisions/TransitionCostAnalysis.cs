using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Models.Roster;

namespace RingGeneral.Core.Models.Decisions;

/// <summary>
/// Analyse des coûts de transition d'ADN pour l'Owner
/// </summary>
public sealed record TransitionCostAnalysis
{
    /// <summary>
    /// Identifiant unique de l'analyse
    /// </summary>
    [Required]
    public required string AnalysisId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// ADN actuel
    /// </summary>
    [Required]
    public required RosterDNA CurrentDNA { get; init; }

    /// <summary>
    /// ADN cible
    /// </summary>
    [Required]
    public required RosterDNA TargetDNA { get; init; }

    /// <summary>
    /// Coût financier total estimé
    /// </summary>
    [Range(0, double.MaxValue)]
    public required double EstimatedCost { get; init; }

    /// <summary>
    /// Durée estimée en semaines
    /// </summary>
    [Range(1, int.MaxValue)]
    public required int EstimatedDurationWeeks { get; init; }

    /// <summary>
    /// Risque de départ des talents (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double TalentTurnoverRisk { get; init; }

    /// <summary>
    /// Risque de perte d'audience (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double AudienceLossRisk { get; init; }

    /// <summary>
    /// Indique si la transition est viable
    /// </summary>
    public required bool IsViable { get; init; }

    /// <summary>
    /// Recommandation textuelle
    /// </summary>
    [Required]
    public required string Recommendation { get; init; }

    /// <summary>
    /// Date de l'analyse
    /// </summary>
    public required DateTime AnalyzedAt { get; init; }

    /// <summary>
    /// Valide que l'analyse respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(AnalysisId))
        {
            errorMessage = "AnalysisId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
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
