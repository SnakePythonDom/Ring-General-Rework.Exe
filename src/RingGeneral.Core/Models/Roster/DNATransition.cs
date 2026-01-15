using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Models.Roster;

namespace RingGeneral.Core.Models.Roster;

/// <summary>
/// Transition progressive de l'ADN d'un roster
/// </summary>
public sealed record DNATransition
{
    /// <summary>
    /// Identifiant unique de la transition
    /// </summary>
    [Required]
    public required string TransitionId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// ADN de départ
    /// </summary>
    [Required]
    public required string StartDNAId { get; init; }

    /// <summary>
    /// ADN cible
    /// </summary>
    [Required]
    public required string TargetDNAId { get; init; }

    /// <summary>
    /// Semaine actuelle de la transition
    /// </summary>
    [Range(0, int.MaxValue)]
    public required int CurrentWeek { get; init; }

    /// <summary>
    /// Nombre total de semaines prévues
    /// </summary>
    [Range(1, int.MaxValue)]
    public required int TotalWeeks { get; init; }

    /// <summary>
    /// Pourcentage de progression (0-100)
    /// </summary>
    [Range(0, 100)]
    public required double ProgressPercentage { get; init; }

    /// <summary>
    /// Score d'inertie (0-100)
    /// Résistance au changement
    /// </summary>
    [Range(0, 100)]
    public required double InertiaScore { get; init; }

    /// <summary>
    /// Date de début de la transition
    /// </summary>
    public required DateTime StartedAt { get; init; }

    /// <summary>
    /// Date de fin de la transition (null si en cours)
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Indique si la transition est active
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Valide que la transition respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(TransitionId))
        {
            errorMessage = "TransitionId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(StartDNAId))
        {
            errorMessage = "StartDNAId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(TargetDNAId))
        {
            errorMessage = "TargetDNAId ne peut pas être vide";
            return false;
        }

        if (StartDNAId == TargetDNAId)
        {
            errorMessage = "StartDNAId et TargetDNAId ne peuvent pas être identiques";
            return false;
        }

        if (CurrentWeek > TotalWeeks)
        {
            errorMessage = "CurrentWeek ne peut pas être supérieur à TotalWeeks";
            return false;
        }

        if (CompletedAt.HasValue && CompletedAt.Value < StartedAt)
        {
            errorMessage = "CompletedAt ne peut pas être avant StartedAt";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Calcule le pourcentage de progression basé sur CurrentWeek
    /// </summary>
    public double CalculateProgressPercentage()
    {
        if (TotalWeeks == 0) return 0;
        return Math.Clamp((double)CurrentWeek / TotalWeeks * 100, 0, 100);
    }

    /// <summary>
    /// Détermine si la transition est complète
    /// </summary>
    public bool IsCompleted() => CompletedAt.HasValue || CurrentWeek >= TotalWeeks;
}
