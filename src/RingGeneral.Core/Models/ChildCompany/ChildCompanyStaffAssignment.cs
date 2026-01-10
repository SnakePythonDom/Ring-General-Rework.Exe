using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.ChildCompany;

/// <summary>
/// Enregistrement d'assignation d'un membre du staff à une Child Company
/// </summary>
public sealed record ChildCompanyStaffAssignment(
    /// <summary>
    /// Identifiant unique de l'assignation
    /// </summary>
    [Required]
    string AssignmentId,

    /// <summary>
    /// Identifiant du membre du staff assigné
    /// </summary>
    [Required]
    string StaffId,

    /// <summary>
    /// Identifiant de la Child Company cible
    /// </summary>
    [Required]
    string ChildCompanyId,

    /// <summary>
    /// Type d'assignation (PartTime, TemporarySupport, DedicatedRotation)
    /// </summary>
    [Required]
    StaffAssignmentType AssignmentType,

    /// <summary>
    /// Pourcentage de temps passé à la Child Company (0.1 à 1.0)
    /// </summary>
    [Range(0.1, 1.0)]
    double TimePercentage,

    /// <summary>
    /// Date de début de l'assignation
    /// </summary>
    [Required]
    DateTime StartDate,

    /// <summary>
    /// Date de fin de l'assignation (null pour indéterminé)
    /// </summary>
    DateTime? EndDate,

    /// <summary>
    /// Objectif spécifique de la mission (optionnel)
    /// </summary>
    string? MissionObjective,

    /// <summary>
    /// Date de création de l'assignation
    /// </summary>
    DateTime CreatedAt);

/// <summary>
/// Résultat de calcul de disponibilité d'un staff
/// </summary>
public sealed record StaffAvailabilityResult(
    /// <summary>
    /// Identifiant du staff
    /// </summary>
    string StaffId,

    /// <summary>
    /// Pourcentage de disponibilité pour la période (0.0 à 1.0)
    /// </summary>
    double AvailabilityPercentage,

    /// <summary>
    /// Conflits détectés pour cette période
    /// </summary>
    IReadOnlyList<StaffSharingConflict> Conflicts,

    /// <summary>
    /// Temps maximum disponible en heures par semaine
    /// </summary>
    int MaxWeeklyHours,

    /// <summary>
    /// Recommandations pour optimiser la disponibilité
    /// </summary>
    IReadOnlyList<string> Recommendations);

/// <summary>
/// Conflit de partage détecté pour un membre du staff
/// </summary>
public sealed record StaffSharingConflict(
    /// <summary>
    /// Type de conflit
    /// </summary>
    StaffConflictType ConflictType,

    /// <summary>
    /// Description du conflit
    /// </summary>
    string Description,

    /// <summary>
    /// Sévérité du conflit (1-10)
    /// </summary>
    int Severity,

    /// <summary>
    /// Suggestion de résolution
    /// </summary>
    string ResolutionSuggestion);

/// <summary>
/// Types de conflits possibles dans le partage de staff
/// </summary>
public enum StaffConflictType
{
    /// <summary>
    /// Unicité physique violée
    /// </summary>
    PhysicalUniqueness,

    /// <summary>
    /// Limite de temps dépassée
    /// </summary>
    TimeLimitExceeded,

    /// <summary>
    /// Compétence insuffisante
    /// </summary>
    InsufficientSkill,

    /// <summary>
    /// Contrat sans clause de mobilité
    /// </summary>
    ContractRestriction,

    /// <summary>
    /// Fatigue potentielle
    /// </summary>
    FatigueRisk
}