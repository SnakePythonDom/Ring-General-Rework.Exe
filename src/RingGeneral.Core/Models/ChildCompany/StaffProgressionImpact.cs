using System.ComponentModel.DataAnnotations;

namespace RingGeneral.Core.Models.ChildCompany;

/// <summary>
/// Planning hebdomadaire de partage de staff
/// </summary>
public sealed record StaffSharingSchedule(
    /// <summary>
    /// Identifiant unique du planning
    /// </summary>
    [Required]
    string ScheduleId,

    /// <summary>
    /// Identifiant du membre du staff
    /// </summary>
    [Required]
    string StaffId,

    /// <summary>
    /// Numéro de semaine
    /// </summary>
    [Required]
    int WeekNumber,

    /// <summary>
    /// Localisation lundi (null = compagnie mère, sinon "CHILD:{childCompanyId}")
    /// </summary>
    string? MondayLocation,

    /// <summary>
    /// Localisation mardi
    /// </summary>
    string? TuesdayLocation,

    /// <summary>
    /// Localisation mercredi
    /// </summary>
    string? WednesdayLocation,

    /// <summary>
    /// Localisation jeudi
    /// </summary>
    string? ThursdayLocation,

    /// <summary>
    /// Localisation vendredi
    /// </summary>
    string? FridayLocation,

    /// <summary>
    /// Localisation samedi
    /// </summary>
    string? SaturdayLocation,

    /// <summary>
    /// Localisation dimanche
    /// </summary>
    string? SundayLocation);

/// <summary>
/// Résultat d'une opération d'assignation
/// </summary>
public sealed record AssignmentResult(
    /// <summary>
    /// Succès de l'opération
    /// </summary>
    bool Success,

    /// <summary>
    /// ID de l'assignation créée/modifiée
    /// </summary>
    string? AssignmentId,

    /// <summary>
    /// Message d'information
    /// </summary>
    string Message,

    /// <summary>
    /// Erreurs rencontrées
    /// </summary>
    IReadOnlyList<string> Errors,

    /// <summary>
    /// Avertissements générés
    /// </summary>
    IReadOnlyList<string> Warnings);

/// <summary>
/// Résultat d'une opération générique
/// </summary>
public sealed record OperationResult(
    /// <summary>
    /// Succès de l'opération
    /// </summary>
    bool Success,

    /// <summary>
    /// Message d'information
    /// </summary>
    string Message,

    /// <summary>
    /// Erreurs rencontrées
    /// </summary>
    IReadOnlyList<string> Errors);

/// <summary>
/// Résultat de validation
/// </summary>
public sealed record ValidationResult(
    /// <summary>
    /// L'assignation est valide
    /// </summary>
    bool IsValid,

    /// <summary>
    /// Erreurs de validation
    /// </summary>
    IReadOnlyList<string> Errors,

    /// <summary>
    /// Avertissements de validation
    /// </summary>
    IReadOnlyList<string> Warnings,

    /// <summary>
    /// Score de validation (0-100)
    /// </summary>
    int ValidationScore);

/// <summary>
/// Arrangement de partage de staff (pour les propositions)
/// </summary>
public sealed record StaffSharingArrangement(
    /// <summary>
    /// ID du staff
    /// </summary>
    string StaffId,

    /// <summary>
    /// ID de la Child Company
    /// </summary>
    string ChildCompanyId,

    /// <summary>
    /// Type d'assignation
    /// </summary>
    string AssignmentType,

    /// <summary>
    /// Pourcentage de temps
    /// </summary>
    double TimePercentage,

    /// <summary>
    /// Date de début
    /// </summary>
    DateTime StartDate,

    /// <summary>
    /// Date de fin (optionnel)
    /// </summary>
    DateTime? EndDate,

    /// <summary>
    /// Objectif de la mission
    /// </summary>
    string? MissionObjective);

/// <summary>
/// Objectif de partage de staff
/// </summary>
public sealed record SharingObjective(
    /// <summary>
    /// Type d'objectif
    /// </summary>
    string ObjectiveType,

    /// <summary>
    /// Priorité de l'objectif
    /// </summary>
    Priority Priority,

    /// <summary>
    /// Durée souhaitée en jours
    /// </summary>
    int DurationDays,

    /// <summary>
    /// Focus sur les attributs (optionnel)
    /// </summary>
    IReadOnlyList<string>? AttributeFocus);

/// <summary>
/// Niveau de priorité
/// </summary>
public enum Priority
{
    /// <summary>
    /// Priorité basse
    /// </summary>
    Low,

    /// <summary>
    /// Priorité moyenne
    /// </summary>
    Medium,

    /// <summary>
    /// Priorité haute
    /// </summary>
    High
}

/// <summary>
/// Détail des coûts de partage
/// </summary>
public sealed record SharingCostBreakdown(
    /// <summary>
    /// Coût mensuel de base du staff
    /// </summary>
    decimal BaseMonthlyCost,

    /// <summary>
    /// Coût ajusté selon le temps
    /// </summary>
    decimal TimeAdjustedCost,

    /// <summary>
    /// Ajustement selon le type d'assignation
    /// </summary>
    decimal TypeAdjustedCost,

    /// <summary>
    /// Coût total mensuel
    /// </summary>
    decimal TotalMonthlyCost,

    /// <summary>
    /// Coût proraté selon la durée
    /// </summary>
    decimal ProratedCost,

    /// <summary>
    /// Durée de référence
    /// </summary>
    TimeSpan Duration);

/// <summary>
/// Plan d'assignation optimisé
/// </summary>
public sealed record OptimizedAssignmentPlan(
    /// <summary>
    /// ID de la Child Company
    /// </summary>
    string ChildCompanyId,

    /// <summary>
    /// Propositions sélectionnées
    /// </summary>
    IReadOnlyList<StaffSharingProposal> SelectedProposals,

    /// <summary>
    /// Coût total estimé
    /// </summary>
    decimal TotalEstimatedCost,

    /// <summary>
    /// Gain d'efficacité attendu
    /// </summary>
    double ExpectedEfficiencyGain,

    /// <summary>
    /// Score d'optimisation (0-100)
    /// </summary>
    int OptimizationScore);

/// <summary>
/// Informations sur une structure de jeunes pour calcul de compatibilité
/// </summary>
public sealed record YouthStructureInfo(
    /// <summary>
    /// ID de la structure
    /// </summary>
    string YouthStructureId,

    /// <summary>
    /// Philosophie de développement
    /// </summary>
    string Philosophie,

    /// <summary>
    /// Niveau d'équipements
    /// </summary>
    int NiveauEquipements,

    /// <summary>
    /// Budget annuel
    /// </summary>
    int BudgetAnnuel,

    /// <summary>
    /// Type de structure
    /// </summary>
    string Type,

    /// <summary>
    /// Région
    /// </summary>
    string Region);

/// <summary>
/// Compatibilité détaillée avec analyse
/// </summary>
public sealed record DetailedCompatibility(
    /// <summary>
    /// Score global de compatibilité
    /// </summary>
    double OverallScore,

    /// <summary>
    /// Score de compatibilité philosophique
    /// </summary>
    double PhilosophyCompatibility,

    /// <summary>
    /// Score de compatibilité de rôle
    /// </summary>
    double RoleCompatibility,

    /// <summary>
    /// Score de compatibilité d'expérience
    /// </summary>
    double ExperienceCompatibility,

    /// <summary>
    /// Score de compatibilité de spécialisation
    /// </summary>
    double SpecializationCompatibility,

    /// <summary>
    /// Points forts de la compatibilité
    /// </summary>
    IReadOnlyList<string> Strengths,

    /// <summary>
    /// Points faibles de la compatibilité
    /// </summary>
    IReadOnlyList<string> Weaknesses,

    /// <summary>
    /// Recommandations d'amélioration
    /// </summary>
    IReadOnlyList<string> Recommendations);

/// <summary>
/// Classement de compatibilité pour un membre du staff
/// </summary>
public sealed record StaffCompatibilityRanking(
    /// <summary>
    /// Membre du staff
    /// </summary>
    RingGeneral.Core.Models.Staff.StaffMember Staff,

    /// <summary>
    /// Compatibilité détaillée
    /// </summary>
    DetailedCompatibility Compatibility,

    /// <summary>
    /// Rang dans le classement
    /// </summary>
    int Rank);

/// <summary>
/// Calcul des impacts d'un membre du staff sur la progression des jeunes talents
/// </summary>
public sealed record StaffProgressionImpact(
    /// <summary>
    /// Identifiant du membre du staff
    /// </summary>
    [Required]
    string StaffId,

    /// <summary>
    /// Identifiant de la structure de jeunes concernée
    /// </summary>
    [Required]
    string YouthStructureId,

    /// <summary>
    /// Bonus apportés à chaque attribut de progression
    /// Clés: "inring", "entertainment", "story", "mental"
    /// Valeurs: multiplicateurs (ex: 0.15 = +15%)
    /// </summary>
    [Required]
    IReadOnlyDictionary<string, double> AttributeBonuses,

    /// <summary>
    /// Score de compatibilité entre le staff et la structure (0.7-1.3)
    /// </summary>
    [Range(0.7, 1.3)]
    double CompatibilityScore,

    /// <summary>
    /// Modificateur de fatigue basé sur la surcharge de travail (0.8-1.0)
    /// </summary>
    [Range(0.8, 1.0)]
    double FatigueModifier,

    /// <summary>
    /// Date et heure du calcul de l'impact
    /// </summary>
    [Required]
    DateTime CalculatedAt);

/// <summary>
/// Résumé consolidé des impacts staff pour une structure
/// </summary>
public sealed record StaffImpactSummary(
    /// <summary>
    /// Identifiant de la structure de jeunes
    /// </summary>
    string YouthStructureId,

    /// <summary>
    /// Nombre total de membres du staff assignés
    /// </summary>
    int TotalAssignedStaff,

    /// <summary>
    /// Bonus total par attribut (multiplicateurs cumulés)
    /// </summary>
    IReadOnlyDictionary<string, double> TotalBonuses,

    /// <summary>
    /// Score de compatibilité moyen pondéré
    /// </summary>
    double AverageCompatibilityScore,

    /// <summary>
    /// Modificateur de fatigue moyen
    /// </summary>
    double AverageFatigueModifier,

    /// <summary>
    /// Impact net estimé sur la progression (multiplicateur global)
    /// </summary>
    double EstimatedProgressionMultiplier,

    /// <summary>
    /// Coût mensuel total du staff assigné
    /// </summary>
    decimal TotalMonthlyCost,

    /// <summary>
    /// ROI estimé (bénéfice progression / coût)
    /// </summary>
    double EstimatedROI,

    /// <summary>
    /// Date de la dernière mise à jour
    /// </summary>
    DateTime LastUpdated);

/// <summary>
/// Proposition d'arrangement de partage de staff
/// </summary>
public sealed record StaffSharingProposal(
    /// <summary>
    /// Identifiant du staff concerné
    /// </summary>
    string StaffId,

    /// <summary>
    /// Child Company proposée
    /// </summary>
    string ChildCompanyId,

    /// <summary>
    /// Type d'assignation recommandé
    /// </summary>
    string RecommendedAssignmentType,

    /// <summary>
    /// Pourcentage de temps recommandé
    /// </summary>
    double RecommendedTimePercentage,

    /// <summary>
    /// Impact estimé sur la progression
    /// </summary>
    StaffProgressionImpact EstimatedImpact,

    /// <summary>
    /// Coût mensuel estimé
    /// </summary>
    decimal EstimatedMonthlyCost,

    /// <summary>
    /// Score de recommandation (0-100)
    /// </summary>
    double RecommendationScore,

    /// <summary>
    /// Raisons de la recommandation
    /// </summary>
    IReadOnlyList<string> RecommendationReasons,

    /// <summary>
    /// Risques potentiels
    /// </summary>
    IReadOnlyList<string> PotentialRisks);