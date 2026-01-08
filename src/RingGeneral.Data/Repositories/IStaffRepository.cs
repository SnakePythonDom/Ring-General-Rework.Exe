using RingGeneral.Core.Models.Staff;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Interface pour le repository du staff.
/// Gère les opérations CRUD pour StaffMember, CreativeStaff, StructuralStaff, et Trainer.
/// </summary>
public interface IStaffRepository
{
    // ====================================================================
    // STAFF MEMBER CRUD OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde un nouveau membre du staff
    /// </summary>
    Task SaveStaffMemberAsync(StaffMember staffMember);

    /// <summary>
    /// Récupère un membre du staff par son identifiant
    /// </summary>
    Task<StaffMember?> GetStaffMemberByIdAsync(string staffId);

    /// <summary>
    /// Récupère tous les staff members d'une compagnie
    /// </summary>
    Task<List<StaffMember>> GetStaffByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère les staff actifs d'une compagnie
    /// </summary>
    Task<List<StaffMember>> GetActiveStaffByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère les staff members par département
    /// </summary>
    Task<List<StaffMember>> GetStaffByDepartmentAsync(string companyId, string department);

    /// <summary>
    /// Récupère les staff members par rôle
    /// </summary>
    Task<List<StaffMember>> GetStaffByRoleAsync(string companyId, string role);

    /// <summary>
    /// Récupère les staff members d'un brand spécifique
    /// </summary>
    Task<List<StaffMember>> GetStaffByBrandIdAsync(string brandId);

    /// <summary>
    /// Met à jour un membre du staff
    /// </summary>
    Task UpdateStaffMemberAsync(StaffMember staffMember);

    /// <summary>
    /// Change le statut d'emploi d'un staff
    /// </summary>
    Task UpdateEmploymentStatusAsync(string staffId, string status);

    /// <summary>
    /// Supprime un membre du staff
    /// </summary>
    Task DeleteStaffMemberAsync(string staffId);

    // ====================================================================
    // CREATIVE STAFF OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde un staff créatif
    /// </summary>
    Task SaveCreativeStaffAsync(CreativeStaff creativeStaff);

    /// <summary>
    /// Récupère un staff créatif par ID
    /// </summary>
    Task<CreativeStaff?> GetCreativeStaffByIdAsync(string staffId);

    /// <summary>
    /// Récupère tous les staff créatifs d'une compagnie
    /// </summary>
    Task<List<CreativeStaff>> GetCreativeStaffByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère les staff créatifs travaillant avec un booker
    /// </summary>
    Task<List<CreativeStaff>> GetCreativeStaffByBookerIdAsync(string bookerId);

    /// <summary>
    /// Met à jour un staff créatif
    /// </summary>
    Task UpdateCreativeStaffAsync(CreativeStaff creativeStaff);

    /// <summary>
    /// Met à jour le score de compatibilité d'un staff créatif
    /// </summary>
    Task UpdateCompatibilityScoreAsync(string staffId, int compatibilityScore);

    // ====================================================================
    // STRUCTURAL STAFF OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde un staff structurel
    /// </summary>
    Task SaveStructuralStaffAsync(StructuralStaff structuralStaff);

    /// <summary>
    /// Récupère un staff structurel par ID
    /// </summary>
    Task<StructuralStaff?> GetStructuralStaffByIdAsync(string staffId);

    /// <summary>
    /// Récupère tous les staff structurels d'une compagnie
    /// </summary>
    Task<List<StructuralStaff>> GetStructuralStaffByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère les staff structurels par domaine d'expertise
    /// </summary>
    Task<List<StructuralStaff>> GetStructuralStaffByExpertiseAsync(string companyId, string expertiseDomain);

    /// <summary>
    /// Met à jour un staff structurel
    /// </summary>
    Task UpdateStructuralStaffAsync(StructuralStaff structuralStaff);

    /// <summary>
    /// Incrémente les compteurs d'interventions
    /// </summary>
    Task IncrementInterventionsAsync(string staffId, bool successful);

    // ====================================================================
    // TRAINER OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde un trainer
    /// </summary>
    Task SaveTrainerAsync(Trainer trainer);

    /// <summary>
    /// Récupère un trainer par ID
    /// </summary>
    Task<Trainer?> GetTrainerByIdAsync(string staffId);

    /// <summary>
    /// Récupère tous les trainers d'une compagnie
    /// </summary>
    Task<List<Trainer>> GetTrainersByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère les trainers d'une infrastructure spécifique
    /// </summary>
    Task<List<Trainer>> GetTrainersByInfrastructureIdAsync(string infrastructureId);

    /// <summary>
    /// Récupère les trainers par spécialisation
    /// </summary>
    Task<List<Trainer>> GetTrainersBySpecializationAsync(string companyId, string specialization);

    /// <summary>
    /// Met à jour un trainer
    /// </summary>
    Task UpdateTrainerAsync(Trainer trainer);

    /// <summary>
    /// Met à jour le nombre d'élèves d'un trainer
    /// </summary>
    Task UpdateStudentCountAsync(string staffId, int currentStudents);

    /// <summary>
    /// Incrémente les compteurs de graduation/échec
    /// </summary>
    Task IncrementGraduationStatsAsync(string staffId, bool graduated);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    /// <summary>
    /// Compte le nombre de staff actifs par département
    /// </summary>
    Task<int> CountActiveStaffByDepartmentAsync(string companyId, string department);

    /// <summary>
    /// Récupère les staff dont le contrat expire bientôt (< 90 jours)
    /// </summary>
    Task<List<StaffMember>> GetExpiringContractsAsync(string companyId, int daysThreshold = 90);

    /// <summary>
    /// Calcule le coût mensuel total du staff
    /// </summary>
    Task<double> CalculateMonthlyStaffCostAsync(string companyId);

    /// <summary>
    /// Récupère les staff créatifs dangereux (peuvent ruiner storylines)
    /// </summary>
    Task<List<CreativeStaff>> GetDangerousCreativeStaffAsync(string companyId);
}
