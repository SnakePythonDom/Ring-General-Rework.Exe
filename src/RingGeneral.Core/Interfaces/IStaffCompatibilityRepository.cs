using RingGeneral.Core.Models.Staff;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des compatibilités staff-booker.
/// Gère le cache des compatibilités calculées.
/// </summary>
public interface IStaffCompatibilityRepository
{
    // ====================================================================
    // STAFF COMPATIBILITY CRUD OPERATIONS
    // ====================================================================

    Task SaveCompatibilityAsync(StaffCompatibility compatibility);
    Task<StaffCompatibility?> GetCompatibilityByIdAsync(string compatibilityId);
    Task<StaffCompatibility?> GetCompatibilityAsync(string staffId, string bookerId);
    Task<List<StaffCompatibility>> GetCompatibilitiesByStaffIdAsync(string staffId);
    Task<List<StaffCompatibility>> GetCompatibilitiesByBookerIdAsync(string bookerId);
    Task UpdateCompatibilityAsync(StaffCompatibility compatibility);
    Task DeleteCompatibilityAsync(string compatibilityId);
    Task IncrementCollaborationAsync(string compatibilityId, bool successful);
    Task IncrementConflictHistoryAsync(string compatibilityId);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    Task<List<StaffCompatibility>> GetDangerousCompatibilitiesAsync(string companyId);
    Task<List<StaffCompatibility>> GetExcellentCompatibilitiesAsync(string companyId);
    Task<List<StaffCompatibility>> GetCompatibilitiesNeedingRecalculationAsync();
    Task<bool> CompatibilityExistsAsync(string staffId, string bookerId);
    Task<double> CalculateAverageCompatibilityScoreAsync(string bookerId);
}
