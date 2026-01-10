using RingGeneral.Core.Models.ChildCompany;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des assignations staff aux Child Companies.
/// Gère les opérations CRUD pour les affectations de staff.
/// </summary>
public interface IChildCompanyStaffRepository
{
    // ====================================================================
    // STAFF ASSIGNMENT CRUD OPERATIONS
    // ====================================================================

    Task SaveStaffAssignmentAsync(ChildCompanyStaffAssignment assignment);
    Task<ChildCompanyStaffAssignment?> GetStaffAssignmentByIdAsync(string assignmentId);
    Task<IReadOnlyList<ChildCompanyStaffAssignment>> GetStaffAssignmentsByStaffAsync(string staffId);
    Task<IReadOnlyList<ChildCompanyStaffAssignment>> GetStaffAssignmentsByChildCompanyAsync(string childCompanyId);
    Task<IReadOnlyList<ChildCompanyStaffAssignment>> GetActiveStaffAssignmentsAsync(string childCompanyId);
    Task UpdateStaffAssignmentAsync(ChildCompanyStaffAssignment assignment);
    Task DeleteStaffAssignmentAsync(string assignmentId);

    // ====================================================================
    // STAFF SHARING SCHEDULE OPERATIONS
    // ====================================================================

    Task SaveStaffSharingScheduleAsync(StaffSharingSchedule schedule);
    Task<StaffSharingSchedule?> GetStaffSharingScheduleAsync(string staffId, int weekNumber);
    Task<IReadOnlyList<StaffSharingSchedule>> GetStaffSharingSchedulesAsync(string staffId);
    Task UpdateStaffSharingScheduleAsync(StaffSharingSchedule schedule);
    Task DeleteStaffSharingScheduleAsync(string staffId, int weekNumber);

    // ====================================================================
    // PROGRESSION IMPACT OPERATIONS
    // ====================================================================

    Task SaveStaffProgressionImpactAsync(StaffProgressionImpact impact);
    Task<StaffProgressionImpact?> GetStaffProgressionImpactAsync(string staffId, string youthStructureId);
    Task<IReadOnlyList<StaffProgressionImpact>> GetStaffImpactsForYouthStructureAsync(string youthStructureId);
    Task<IReadOnlyList<StaffProgressionImpact>> GetStaffImpactsForStaffAsync(string staffId);
    Task UpdateStaffProgressionImpactAsync(StaffProgressionImpact impact);
    Task DeleteStaffProgressionImpactAsync(string staffId, string youthStructureId);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    Task<IReadOnlyList<StaffAvailabilityResult>> CalculateStaffAvailabilitiesAsync(string companyId, DateTime period);
    Task<IReadOnlyList<StaffSharingConflict>> DetectStaffSharingConflictsAsync(string companyId, DateTime startDate, DateTime endDate);
    Task<StaffImpactSummary> CalculateStaffImpactSummaryAsync(string youthStructureId);
    Task<int> CountAssignedStaffByChildCompanyAsync(string childCompanyId);
}