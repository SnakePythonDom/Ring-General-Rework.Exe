using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.ChildCompany;
using RingGeneral.Core.Models.Staff;
using RingGeneral.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service principal de gestion du staff des Child Companies.
/// Orchestre la logique métier de partage et d'assignation du staff.
/// </summary>
public sealed class ChildCompanyStaffService : IChildCompanyStaffService
{
    private readonly IChildCompanyStaffRepository _staffRepository;
    private readonly IStaffRepository _baseStaffRepository;
    private readonly IChildCompanyRepository _childCompanyRepository;

    public ChildCompanyStaffService(
        IChildCompanyStaffRepository staffRepository,
        IStaffRepository baseStaffRepository,
        IChildCompanyRepository childCompanyRepository)
    {
        _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
        _baseStaffRepository = baseStaffRepository ?? throw new ArgumentNullException(nameof(baseStaffRepository));
        _childCompanyRepository = childCompanyRepository ?? throw new ArgumentNullException(nameof(childCompanyRepository));
    }

    // ====================================================================
    // STAFF ASSIGNMENT MANAGEMENT
    // ====================================================================

    public async Task<AssignmentResult> AssignStaffToChildCompanyAsync(
        string staffId,
        string childCompanyId,
        StaffAssignmentType assignmentType,
        double timePercentage,
        string? missionObjective = null)
    {
        try
        {
            // Validation des paramètres
            if (string.IsNullOrWhiteSpace(staffId))
                return AssignmentResult(false, null, "StaffId requis", new[] { "StaffId ne peut pas être vide" }, Array.Empty<string>());

            if (string.IsNullOrWhiteSpace(childCompanyId))
                return AssignmentResult(false, null, "ChildCompanyId requis", new[] { "ChildCompanyId ne peut pas être vide" }, Array.Empty<string>());

            if (timePercentage < 0.1 || timePercentage > 1.0)
                return AssignmentResult(false, null, "Pourcentage de temps invalide", new[] { "TimePercentage doit être entre 0.1 et 1.0" }, Array.Empty<string>());

            // Vérifier que la Child Company existe
            var childCompany = await _childCompanyRepository.GetChildCompanyByIdAsync(childCompanyId);
            if (childCompany is null)
                return AssignmentResult(false, null, "Child Company introuvable", new[] { $"Child Company {childCompanyId} n'existe pas" }, Array.Empty<string>());

            // Créer l'assignation
            var assignment = new ChildCompanyStaffAssignment(
                AssignmentId: Guid.NewGuid().ToString(),
                StaffId: staffId,
                ChildCompanyId: childCompanyId,
                AssignmentType: assignmentType,
                TimePercentage: timePercentage,
                StartDate: DateTime.Now,
                EndDate: assignmentType == StaffAssignmentType.TemporarySupport ? DateTime.Now.AddDays(28) : null,
                MissionObjective: missionObjective,
                CreatedAt: DateTime.Now);

            // Valider les règles métier
            var validation = await ValidateStaffSharingRulesAsync(assignment);
            if (!validation.IsValid)
                return AssignmentResult(false, null, "Validation échouée", validation.Errors, validation.Warnings);

            // Sauvegarder l'assignation
            await _staffRepository.SaveStaffAssignmentAsync(assignment);

            // Recalculer les impacts
            await RecalculateAllImpactsForYouthStructureAsync(childCompanyId);

            return AssignmentResult(
                true,
                assignment.AssignmentId,
                "Assignation créée avec succès",
                Array.Empty<string>(),
                validation.Warnings);
        }
        catch (Exception ex)
        {
            return AssignmentResult(false, null, $"Erreur lors de l'assignation: {ex.Message}", new[] { ex.Message }, Array.Empty<string>());
        }
    }

    public async Task<AssignmentResult> RemoveStaffAssignmentAsync(string assignmentId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return AssignmentResult(false, null, "AssignmentId requis", new[] { "AssignmentId ne peut pas être vide" }, Array.Empty<string>());

            // Récupérer l'assignation pour connaître la Child Company
            var assignment = await _staffRepository.GetStaffAssignmentByIdAsync(assignmentId);
            if (assignment is null)
                return AssignmentResult(false, null, "Assignation introuvable", new[] { $"Assignation {assignmentId} n'existe pas" }, Array.Empty<string>());

            // Supprimer l'assignation
            await _staffRepository.DeleteStaffAssignmentAsync(assignmentId);

            // Recalculer les impacts
            await RecalculateAllImpactsForYouthStructureAsync(assignment.ChildCompanyId);

            return AssignmentResult(true, assignmentId, "Assignation supprimée avec succès", Array.Empty<string>(), Array.Empty<string>());
        }
        catch (Exception ex)
        {
            return AssignmentResult(false, null, $"Erreur lors de la suppression: {ex.Message}", new[] { ex.Message }, Array.Empty<string>());
        }
    }

    public async Task<AssignmentResult> UpdateStaffAssignmentAsync(
        string assignmentId,
        double timePercentage,
        string? missionObjective = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return AssignmentResult(false, null, "AssignmentId requis", new[] { "AssignmentId ne peut pas être vide" }, Array.Empty<string>());

            if (timePercentage < 0.1 || timePercentage > 1.0)
                return AssignmentResult(false, null, "Pourcentage de temps invalide", new[] { "TimePercentage doit être entre 0.1 et 1.0" }, Array.Empty<string>());

            // Récupérer l'assignation existante
            var existingAssignment = await _staffRepository.GetStaffAssignmentByIdAsync(assignmentId);
            if (existingAssignment is null)
                return AssignmentResult(false, null, "Assignation introuvable", new[] { $"Assignation {assignmentId} n'existe pas" }, Array.Empty<string>());

            // Créer la nouvelle assignation mise à jour
            var updatedAssignment = existingAssignment with
            {
                TimePercentage = timePercentage,
                MissionObjective = missionObjective
            };

            // Valider les règles métier
            var validation = await ValidateStaffSharingRulesAsync(updatedAssignment);
            if (!validation.IsValid)
                return AssignmentResult(false, null, "Validation échouée", validation.Errors, validation.Warnings);

            // Mettre à jour l'assignation
            await _staffRepository.UpdateStaffAssignmentAsync(updatedAssignment);

            // Recalculer les impacts
            await RecalculateAllImpactsForYouthStructureAsync(updatedAssignment.ChildCompanyId);

            return AssignmentResult(
                true,
                assignmentId,
                "Assignation mise à jour avec succès",
                Array.Empty<string>(),
                validation.Warnings);
        }
        catch (Exception ex)
        {
            return AssignmentResult(false, null, $"Erreur lors de la mise à jour: {ex.Message}", new[] { ex.Message }, Array.Empty<string>());
        }
    }

    // ====================================================================
    // AVAILABILITY & CONFLICT MANAGEMENT
    // ====================================================================

    public async Task<StaffAvailabilityResult> CalculateStaffAvailabilityAsync(string staffId, DateTime period)
    {
        try
        {
            // Récupérer les assignations actives pour ce staff
            var assignments = await _staffRepository.GetStaffAssignmentsByStaffAsync(staffId);
            var activeAssignments = assignments.Where(a =>
                a.StartDate <= period &&
                (!a.EndDate.HasValue || a.EndDate.Value > period)).ToList();

            // Calculer le temps déjà assigné
            var totalTimeAssigned = activeAssignments.Sum(a => a.TimePercentage);

            // Temps disponible maximum (60h/semaine = 1.0)
            var maxTimeAvailable = 1.0 - totalTimeAssigned;
            var availabilityPercentage = Math.Max(0, maxTimeAvailable);

            // Détecter les conflits
            var conflicts = new List<StaffSharingConflict>();
            if (totalTimeAssigned > 1.0)
            {
                conflicts.Add(new StaffSharingConflict(
                    StaffConflictType.TimeLimitExceeded,
                    $"Temps assigné dépassé: {totalTimeAssigned:P0} utilisé",
                    8,
                    "Réduire le temps d'au moins une assignation"));
            }

            // Vérifier l'unicité physique pour la période
            var physicalConflicts = activeAssignments
                .GroupBy(a => a.ChildCompanyId)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .Select(a => new StaffSharingConflict(
                    StaffConflictType.PhysicalUniqueness,
                    $"Assignation multiple à {a.ChildCompanyId}",
                    9,
                    "Un staff ne peut être physiquement qu'à un endroit"));

            conflicts.AddRange(physicalConflicts);

            // Déterminer le nombre maximum d'heures par semaine
            var maxWeeklyHours = (int)(availabilityPercentage * 60);

            // Générer des recommandations
            var recommendations = new List<string>();
            if (availabilityPercentage < 0.3)
                recommendations.Add("Disponibilité limitée - envisager réduction d'assignations existantes");
            if (conflicts.Any())
                recommendations.Add("Résoudre les conflits détectés avant nouvelle assignation");

            return new StaffAvailabilityResult(
                StaffId: staffId,
                AvailabilityPercentage: availabilityPercentage,
                Conflicts: conflicts,
                MaxWeeklyHours: maxWeeklyHours,
                Recommendations: recommendations);
        }
        catch (Exception ex)
        {
            return new StaffAvailabilityResult(
                StaffId: staffId,
                AvailabilityPercentage: 0,
                Conflicts: new[] { new StaffSharingConflict(
                    StaffConflictType.PhysicalUniqueness,
                    $"Erreur de calcul: {ex.Message}",
                    10,
                    "Contacter le support technique") },
                MaxWeeklyHours: 0,
                Recommendations: new[] { "Erreur système - réessayer plus tard" });
        }
    }

    public async Task<IReadOnlyList<StaffSharingConflict>> DetectSharingConflictsAsync(
        string companyId,
        DateTime startDate,
        DateTime endDate)
    {
        var conflicts = new List<StaffSharingConflict>();

        try
        {
            // Récupérer tous les staff de la compagnie
            var companyStaff = await _baseStaffRepository.GetActiveStaffByCompanyIdAsync(companyId);

            foreach (var staff in companyStaff)
            {
                // Calculer la disponibilité pour chaque membre du staff
                var availability = await CalculateStaffAvailabilityAsync(staff.StaffId, startDate);

                // Ajouter les conflits détectés
                conflicts.AddRange(availability.Conflicts);
            }
        }
        catch (Exception ex)
        {
            conflicts.Add(new StaffSharingConflict(
                StaffConflictType.PhysicalUniqueness,
                $"Erreur de détection des conflits: {ex.Message}",
                10,
                "Contacter le support technique"));
        }

        return conflicts;
    }

    public async Task<ValidationResult> ValidateStaffSharingRulesAsync(ChildCompanyStaffAssignment assignment)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var validationScore = 100;

        try
        {
            // Règle 1: Unicité physique - vérifier les conflits de localisation
            var existingAssignments = await _staffRepository.GetStaffAssignmentsByStaffAsync(assignment.StaffId);
            var overlappingAssignments = existingAssignments.Where(a =>
                a.AssignmentId != assignment.AssignmentId && // Exclure l'assignation actuelle si mise à jour
                a.StartDate < assignment.EndDate.GetValueOrDefault(DateTime.MaxValue) &&
                assignment.StartDate < a.EndDate.GetValueOrDefault(DateTime.MaxValue)).ToList();

            if (overlappingAssignments.Any())
            {
                var totalTime = overlappingAssignments.Sum(a => a.TimePercentage) + assignment.TimePercentage;
                if (totalTime > 1.0)
                {
                    errors.Add($"Conflit de temps: {totalTime:P0} utilisé (max 100%)");
                    validationScore -= 30;
                }
                else if (totalTime > 0.8)
                {
                    warnings.Add($"Charge élevée: {totalTime:P0} du temps disponible utilisé");
                    validationScore -= 10;
                }
            }

            // Règle 2: Compétence minimum (SkillScore >= 60)
            var staff = await _baseStaffRepository.GetStaffMemberByIdAsync(assignment.StaffId);
            if (staff is null)
            {
                errors.Add("Membre du staff introuvable");
                validationScore = 0;
            }
            else
            {
                if (!staff.CanBeShared)
                {
                    errors.Add("Ce membre du staff ne peut pas être partagé");
                    validationScore -= 40;
                }

                if (staff.SkillScore < 60)
                {
                    errors.Add($"Compétence insuffisante: {staff.SkillScore}/100 (min 60 requis)");
                    validationScore -= 20;
                }

                if (staff.EmploymentStatus != "Active")
                {
                    errors.Add($"Statut d'emploi invalide: {staff.EmploymentStatus}");
                    validationScore -= 50;
                }
            }

            // Règle 3: Contrat valide
            if (staff?.ContractEndDate.HasValue == true &&
                staff.ContractEndDate.Value < DateTime.Now.AddMonths(3))
            {
                warnings.Add("Contrat arrive à expiration dans moins de 3 mois");
                validationScore -= 5;
            }

            // Règle 4: Mobilité appropriée pour le type d'assignation
            if (assignment.AssignmentType == StaffAssignmentType.DedicatedRotation &&
                staff?.MobilityRating == StaffMobilityRating.Low)
            {
                warnings.Add("Mobilité faible pour une rotation dédiée - peut causer du stress");
                validationScore -= 10;
            }

            // Règle 5: Durée appropriée
            if (assignment.AssignmentType == StaffAssignmentType.TemporarySupport &&
                (!assignment.EndDate.HasValue || assignment.EndDate.Value > assignment.StartDate.AddMonths(6)))
            {
                warnings.Add("Mission temporaire trop longue (>6 mois)");
                validationScore -= 5;
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Erreur de validation: {ex.Message}");
            validationScore = 0;
        }

        return new ValidationResult(
            IsValid: errors.Count == 0,
            Errors: errors,
            Warnings: warnings,
            ValidationScore: Math.Max(0, validationScore));
    }

    // ====================================================================
    // PROGRESSION IMPACT CALCULATION
    // ====================================================================

    public async Task<StaffProgressionImpact> CalculateProgressionImpactAsync(
        string staffId,
        string youthStructureId)
    {
        try
        {
            // Récupérer les informations du staff
            var staff = await _baseStaffRepository.GetStaffMemberByIdAsync(staffId);
            if (staff is null)
                throw new InvalidOperationException($"Staff {staffId} introuvable");

            // Récupérer l'assignation active pour cette structure
            var assignments = await _staffRepository.GetActiveStaffAssignmentsAsync(youthStructureId);
            var assignment = assignments.FirstOrDefault(a => a.StaffId == staffId);

            if (assignment is null)
            {
                // Pas d'assignation active - impact neutre
                return new StaffProgressionImpact(
                    StaffId: staffId,
                    YouthStructureId: youthStructureId,
                    AttributeBonuses: new Dictionary<string, double>
                    {
                        ["inring"] = 0.0,
                        ["entertainment"] = 0.0,
                        ["story"] = 0.0,
                        ["mental"] = 0.0
                    },
                    CompatibilityScore: 1.0,
                    FatigueModifier: 1.0,
                    CalculatedAt: DateTime.Now);
            }

            // Calculer les bonus selon le rôle du staff
            var attributeBonuses = CalculateAttributeBonuses(staff, assignment.TimePercentage);

            // Score de compatibilité (simplifié pour l'instant)
            var compatibilityScore = 1.0; // TODO: Implémenter logique de compatibilité

            // Modificateur de fatigue basé sur la charge de travail
            var fatigueModifier = CalculateFatigueModifier(assignment.TimePercentage);

            var impact = new StaffProgressionImpact(
                StaffId: staffId,
                YouthStructureId: youthStructureId,
                AttributeBonuses: attributeBonuses,
                CompatibilityScore: compatibilityScore,
                FatigueModifier: fatigueModifier,
                CalculatedAt: DateTime.Now);

            // Sauvegarder l'impact
            await _staffRepository.SaveStaffProgressionImpactAsync(impact);

            return impact;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erreur lors du calcul d'impact: {ex.Message}", ex);
        }
    }

    public async Task<int> RecalculateAllImpactsForYouthStructureAsync(string youthStructureId)
    {
        var assignments = await _staffRepository.GetActiveStaffAssignmentsAsync(youthStructureId);
        var impactCount = 0;

        foreach (var assignment in assignments)
        {
            await CalculateProgressionImpactAsync(assignment.StaffId, youthStructureId);
            impactCount++;
        }

        return impactCount;
    }

    public async Task<StaffImpactSummary> GetStaffImpactSummaryAsync(string youthStructureId)
    {
        return await _staffRepository.CalculateStaffImpactSummaryAsync(youthStructureId);
    }

    // ====================================================================
    // SCHEDULING MANAGEMENT
    // ====================================================================

    public async Task<OperationResult> UpdateStaffWeeklyScheduleAsync(
        string staffId,
        int weekNumber,
        Dictionary<DayOfWeek, string?> schedule)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(staffId))
                return new OperationResult(false, "StaffId requis", new[] { "StaffId ne peut pas être vide" });

            // Créer le planning hebdomadaire
            var sharingSchedule = new StaffSharingSchedule(
                ScheduleId: Guid.NewGuid().ToString(),
                StaffId: staffId,
                WeekNumber: weekNumber,
                MondayLocation: schedule.GetValueOrDefault(DayOfWeek.Monday),
                TuesdayLocation: schedule.GetValueOrDefault(DayOfWeek.Tuesday),
                WednesdayLocation: schedule.GetValueOrDefault(DayOfWeek.Wednesday),
                ThursdayLocation: schedule.GetValueOrDefault(DayOfWeek.Thursday),
                FridayLocation: schedule.GetValueOrDefault(DayOfWeek.Friday),
                SaturdayLocation: schedule.GetValueOrDefault(DayOfWeek.Saturday),
                SundayLocation: schedule.GetValueOrDefault(DayOfWeek.Sunday));

            await _staffRepository.SaveStaffSharingScheduleAsync(sharingSchedule);

            return new OperationResult(true, "Planning mis à jour avec succès", Array.Empty<string>());
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Erreur lors de la mise à jour du planning: {ex.Message}", new[] { ex.Message });
        }
    }

    public async Task<StaffSharingSchedule?> GetStaffWeeklyScheduleAsync(string staffId, int weekNumber)
    {
        if (string.IsNullOrWhiteSpace(staffId))
            return null;

        return await _staffRepository.GetStaffSharingScheduleAsync(staffId, weekNumber);
    }

    // ====================================================================
    // PRIVATE HELPER METHODS
    // ====================================================================

    private static AssignmentResult AssignmentResult(
        bool success,
        string? assignmentId,
        string message,
        IEnumerable<string> errors,
        IEnumerable<string> warnings)
    {
        return new AssignmentResult(success, assignmentId, message, errors.ToArray(), warnings.ToArray());
    }

    private static Dictionary<string, double> CalculateAttributeBonuses(StaffMember staff, double timePercentage)
    {
        // Formules simplifiées basées sur le rôle du staff (PRD section 3.2)
        var baseBonuses = staff.Role switch
        {
            StaffRole.HeadTrainer or StaffRole.WrestlingTrainer =>
                new Dictionary<string, double> { ["inring"] = 0.25, ["entertainment"] = 0.05, ["story"] = 0.05, ["mental"] = 0.10 },

            StaffRole.Booker =>
                new Dictionary<string, double> { ["inring"] = 0.10, ["entertainment"] = 0.15, ["story"] = 0.15, ["mental"] = 0.05 },

            StaffRole.LeadWriter or StaffRole.CreativeWriter =>
                new Dictionary<string, double> { ["inring"] = 0.05, ["entertainment"] = 0.30, ["story"] = 0.30, ["mental"] = 0.15 },

            StaffRole.PerformancePsychologist =>
                new Dictionary<string, double> { ["inring"] = 0.05, ["entertainment"] = 0.10, ["story"] = 0.15, ["mental"] = 0.35 },

            StaffRole.MedicalDirector or StaffRole.MedicalStaff =>
                new Dictionary<string, double> { ["inring"] = 0.10, ["entertainment"] = 0.05, ["story"] = 0.05, ["mental"] = 0.15 },

            _ => new Dictionary<string, double> { ["inring"] = 0.05, ["entertainment"] = 0.05, ["story"] = 0.05, ["mental"] = 0.05 }
        };

        // Appliquer le multiplicateur de compétence
        var skillMultiplier = staff.SkillScore / 100.0;

        // Appliquer le facteur temps
        var timeFactor = timePercentage;

        // Calculer les bonus finaux
        return baseBonuses.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value * skillMultiplier * timeFactor);
    }

    private static double CalculateFatigueModifier(double timePercentage)
    {
        // Modificateur de fatigue basé sur la charge de travail
        // Charge normale (≤60%): 1.0
        // Charge élevée (61-80%): 0.95
        // Charge excessive (81-100%): 0.85
        return timePercentage switch
        {
            <= 0.6 => 1.0,
            <= 0.8 => 0.95,
            _ => 0.85
        };
    }
}