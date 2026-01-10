using RingGeneral.Core.Models.ChildCompany;
using RingGeneral.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le service de gestion du staff des Child Companies.
/// Orchestre la logique métier de partage et d'assignation du staff.
/// </summary>
public interface IChildCompanyStaffService
{
    // ====================================================================
    // STAFF ASSIGNMENT MANAGEMENT
    // ====================================================================

    /// <summary>
    /// Assigne un membre du staff à une Child Company
    /// </summary>
    /// <param name="staffId">ID du staff à assigner</param>
    /// <param name="childCompanyId">ID de la Child Company cible</param>
    /// <param name="assignmentType">Type d'assignation</param>
    /// <param name="timePercentage">Pourcentage de temps (0.1-1.0)</param>
    /// <param name="missionObjective">Objectif de la mission (optionnel)</param>
    /// <returns>Résultat de l'opération</returns>
    Task<AssignmentResult> AssignStaffToChildCompanyAsync(
        string staffId,
        string childCompanyId,
        StaffAssignmentType assignmentType,
        double timePercentage,
        string? missionObjective = null);

    /// <summary>
    /// Supprime une assignation de staff
    /// </summary>
    /// <param name="assignmentId">ID de l'assignation à supprimer</param>
    /// <returns>Résultat de l'opération</returns>
    Task<AssignmentResult> RemoveStaffAssignmentAsync(string assignmentId);

    /// <summary>
    /// Met à jour une assignation existante
    /// </summary>
    /// <param name="assignmentId">ID de l'assignation</param>
    /// <param name="timePercentage">Nouveau pourcentage de temps</param>
    /// <param name="missionObjective">Nouvel objectif (optionnel)</param>
    /// <returns>Résultat de l'opération</returns>
    Task<AssignmentResult> UpdateStaffAssignmentAsync(
        string assignmentId,
        double timePercentage,
        string? missionObjective = null);

    // ====================================================================
    // AVAILABILITY & CONFLICT MANAGEMENT
    // ====================================================================

    /// <summary>
    /// Calcule la disponibilité d'un staff pour une période donnée
    /// </summary>
    /// <param name="staffId">ID du staff</param>
    /// <param name="period">Période de référence</param>
    /// <returns>Résultat de disponibilité</returns>
    Task<StaffAvailabilityResult> CalculateStaffAvailabilityAsync(string staffId, DateTime period);

    /// <summary>
    /// Détecte les conflits de partage pour une compagnie
    /// </summary>
    /// <param name="companyId">ID de la compagnie</param>
    /// <param name="startDate">Date de début</param>
    /// <param name="endDate">Date de fin</param>
    /// <returns>Liste des conflits détectés</returns>
    Task<IReadOnlyList<StaffSharingConflict>> DetectSharingConflictsAsync(
        string companyId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Valide les règles métier de partage de staff
    /// </summary>
    /// <param name="assignment">Assignation à valider</param>
    /// <returns>Résultat de validation</returns>
    Task<ValidationResult> ValidateStaffSharingRulesAsync(ChildCompanyStaffAssignment assignment);

    // ====================================================================
    // PROGRESSION IMPACT CALCULATION
    // ====================================================================

    /// <summary>
    /// Calcule l'impact d'un staff sur la progression d'une structure de jeunes
    /// </summary>
    /// <param name="staffId">ID du staff</param>
    /// <param name="youthStructureId">ID de la structure de jeunes</param>
    /// <returns>Impact calculé</returns>
    Task<StaffProgressionImpact> CalculateProgressionImpactAsync(
        string staffId,
        string youthStructureId);

    /// <summary>
    /// Recalcule tous les impacts pour une structure de jeunes
    /// </summary>
    /// <param name="youthStructureId">ID de la structure</param>
    /// <returns>Nombre d'impacts recalculés</returns>
    Task<int> RecalculateAllImpactsForYouthStructureAsync(string youthStructureId);

    /// <summary>
    /// Obtient le résumé consolidé des impacts pour une structure
    /// </summary>
    /// <param name="youthStructureId">ID de la structure</param>
    /// <returns>Résumé des impacts</returns>
    Task<StaffImpactSummary> GetStaffImpactSummaryAsync(string youthStructureId);

    // ====================================================================
    // SCHEDULING MANAGEMENT
    // ====================================================================

    /// <summary>
    /// Met à jour le planning hebdomadaire d'un staff
    /// </summary>
    /// <param name="staffId">ID du staff</param>
    /// <param name="weekNumber">Numéro de semaine</param>
    /// <param name="schedule">Planning détaillé</param>
    /// <returns>Résultat de l'opération</returns>
    Task<OperationResult> UpdateStaffWeeklyScheduleAsync(
        string staffId,
        int weekNumber,
        Dictionary<DayOfWeek, string?> schedule);

    /// <summary>
    /// Obtient le planning actuel d'un staff
    /// </summary>
    /// <param name="staffId">ID du staff</param>
    /// <param name="weekNumber">Numéro de semaine</param>
    /// <returns>Planning hebdomadaire</returns>
    Task<StaffSharingSchedule?> GetStaffWeeklyScheduleAsync(string staffId, int weekNumber);
}