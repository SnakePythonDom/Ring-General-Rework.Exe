using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RingGeneral.Core.Models.Decisions;

/// <summary>
/// Rapport sur l'humeur du roster pour le Booker
/// </summary>
public sealed record RosterMoodReport
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
    /// Morale moyenne du roster (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double AverageMorale { get; init; }

    /// <summary>
    /// Nombre de workers avec morale critique (< 30)
    /// </summary>
    [Range(0, int.MaxValue)]
    public required int CriticalMoraleCount { get; init; }

    /// <summary>
    /// Liste des workers demandant un push
    /// </summary>
    [Required]
    public required List<string> WorkersRequestingPush { get; init; }

    /// <summary>
    /// Liste des workers insatisfaits du style actuel
    /// </summary>
    [Required]
    public required List<string> WorkersDissatisfiedWithStyle { get; init; }

    /// <summary>
    /// Recommandations pour le Booker
    /// </summary>
    [Required]
    public required string Recommendations { get; init; }

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

        return true;
    }
}
