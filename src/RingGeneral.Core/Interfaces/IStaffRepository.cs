using RingGeneral.Core.Models.Staff;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository du staff.
/// Gère les opérations CRUD pour StaffMember, CreativeStaff, StructuralStaff, et Trainer.
/// </summary>
public interface IStaffRepository
{
    // ====================================================================
    // STAFF MEMBER CRUD OPERATIONS
    // ====================================================================

    Task SaveStaffMemberAsync(StaffMember staffMember);
    Task<StaffMember?> GetStaffMemberByIdAsync(string staffId);
    Task<List<StaffMember>> GetStaffByCompanyIdAsync(string companyId);
    Task<List<StaffMember>> GetActiveStaffByCompanyIdAsync(string companyId);
    Task<List<StaffMember>> GetStaffByDepartmentAsync(string companyId, string department);
    Task<List<StaffMember>> GetStaffByRoleAsync(string companyId, string role);
    Task<List<StaffMember>> GetStaffByBrandIdAsync(string brandId);
    Task UpdateStaffMemberAsync(StaffMember staffMember);
    Task UpdateEmploymentStatusAsync(string staffId, string status);
    Task DeleteStaffMemberAsync(string staffId);

    // ====================================================================
    // CREATIVE STAFF OPERATIONS
    // ====================================================================

    Task SaveCreativeStaffAsync(CreativeStaff creativeStaff);
    Task<CreativeStaff?> GetCreativeStaffByIdAsync(string staffId);
    Task<List<CreativeStaff>> GetCreativeStaffByCompanyIdAsync(string companyId);
    Task<List<CreativeStaff>> GetCreativeStaffByBookerIdAsync(string bookerId);
    Task UpdateCreativeStaffAsync(CreativeStaff creativeStaff);
    Task UpdateCompatibilityScoreAsync(string staffId, int compatibilityScore);

    // ====================================================================
    // STRUCTURAL STAFF OPERATIONS
    // ====================================================================

    Task SaveStructuralStaffAsync(StructuralStaff structuralStaff);
    Task<StructuralStaff?> GetStructuralStaffByIdAsync(string staffId);
    Task<List<StructuralStaff>> GetStructuralStaffByCompanyIdAsync(string companyId);
    Task<List<StructuralStaff>> GetStructuralStaffByExpertiseAsync(string companyId, string expertiseDomain);
    Task UpdateStructuralStaffAsync(StructuralStaff structuralStaff);
    Task IncrementInterventionsAsync(string staffId, bool successful);

    // ====================================================================
    // TRAINER OPERATIONS
    // ====================================================================

    Task SaveTrainerAsync(Trainer trainer);
    Task<Trainer?> GetTrainerByIdAsync(string staffId);
    Task<List<Trainer>> GetTrainersByCompanyIdAsync(string companyId);
    Task<List<Trainer>> GetTrainersByInfrastructureIdAsync(string infrastructureId);
    Task<List<Trainer>> GetTrainersBySpecializationAsync(string companyId, string specialization);
    Task UpdateTrainerAsync(Trainer trainer);
    Task UpdateStudentCountAsync(string staffId, int currentStudents);
    Task IncrementGraduationStatsAsync(string staffId, bool graduated);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    Task<int> CountActiveStaffByDepartmentAsync(string companyId, string department);
    Task<List<StaffMember>> GetExpiringContractsAsync(string companyId, int daysThreshold = 90);
    Task<double> CalculateMonthlyStaffCostAsync(string companyId);
    Task<List<CreativeStaff>> GetDangerousCreativeStaffAsync(string companyId);
}
