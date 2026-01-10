using Microsoft.Data.Sqlite;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.ChildCompany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository pour les assignations staff aux Child Companies.
/// </summary>
public sealed class ChildCompanyStaffRepository : IChildCompanyStaffRepository
{
    private readonly string _connectionString;

    public ChildCompanyStaffRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // STAFF ASSIGNMENT CRUD OPERATIONS
    // ====================================================================

    public async Task SaveStaffAssignmentAsync(ChildCompanyStaffAssignment assignment)
    {
        if (!IsValid(assignment, out var errorMessage))
        {
            throw new ArgumentException($"StaffAssignment invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO ChildCompanyStaffAssignments (
                AssignmentId, StaffId, ChildCompanyId, AssignmentType, TimePercentage,
                StartDate, EndDate, MissionObjective, CreatedAt
            ) VALUES (
                @AssignmentId, @StaffId, @ChildCompanyId, @AssignmentType, @TimePercentage,
                @StartDate, @EndDate, @MissionObjective, @CreatedAt
            )";

        command.Parameters.AddWithValue("@AssignmentId", assignment.AssignmentId);
        command.Parameters.AddWithValue("@StaffId", assignment.StaffId);
        command.Parameters.AddWithValue("@ChildCompanyId", assignment.ChildCompanyId);
        command.Parameters.AddWithValue("@AssignmentType", assignment.AssignmentType.ToString());
        command.Parameters.AddWithValue("@TimePercentage", assignment.TimePercentage);
        command.Parameters.AddWithValue("@StartDate", assignment.StartDate.ToString("O"));
        command.Parameters.AddWithValue("@EndDate", assignment.EndDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@MissionObjective", assignment.MissionObjective ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", assignment.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<ChildCompanyStaffAssignment?> GetStaffAssignmentByIdAsync(string assignmentId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT AssignmentId, StaffId, ChildCompanyId, AssignmentType, TimePercentage,
                   StartDate, EndDate, MissionObjective, CreatedAt
            FROM ChildCompanyStaffAssignments
            WHERE AssignmentId = @AssignmentId";

        command.Parameters.AddWithValue("@AssignmentId", assignmentId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ChildCompanyStaffAssignment(
                AssignmentId: reader.GetString(0),
                StaffId: reader.GetString(1),
                ChildCompanyId: reader.GetString(2),
                AssignmentType: Enum.Parse<StaffAssignmentType>(reader.GetString(3)),
                TimePercentage: reader.GetDouble(4),
                StartDate: DateTime.Parse(reader.GetString(5)),
                EndDate: reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                MissionObjective: reader.IsDBNull(7) ? null : reader.GetString(7),
                CreatedAt: DateTime.Parse(reader.GetString(8))
            );
        }

        return null;
    }

    public async Task<IReadOnlyList<ChildCompanyStaffAssignment>> GetStaffAssignmentsByStaffAsync(string staffId)
    {
        var result = new List<ChildCompanyStaffAssignment>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT AssignmentId, StaffId, ChildCompanyId, AssignmentType, TimePercentage,
                   StartDate, EndDate, MissionObjective, CreatedAt
            FROM ChildCompanyStaffAssignments
            WHERE StaffId = @StaffId
            ORDER BY StartDate DESC";

        command.Parameters.AddWithValue("@StaffId", staffId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ChildCompanyStaffAssignment(
                AssignmentId: reader.GetString(0),
                StaffId: reader.GetString(1),
                ChildCompanyId: reader.GetString(2),
                AssignmentType: Enum.Parse<StaffAssignmentType>(reader.GetString(3)),
                TimePercentage: reader.GetDouble(4),
                StartDate: DateTime.Parse(reader.GetString(5)),
                EndDate: reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                MissionObjective: reader.IsDBNull(7) ? null : reader.GetString(7),
                CreatedAt: DateTime.Parse(reader.GetString(8))
            ));
        }

        return result;
    }

    public async Task<IReadOnlyList<ChildCompanyStaffAssignment>> GetStaffAssignmentsByChildCompanyAsync(string childCompanyId)
    {
        var result = new List<ChildCompanyStaffAssignment>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT AssignmentId, StaffId, ChildCompanyId, AssignmentType, TimePercentage,
                   StartDate, EndDate, MissionObjective, CreatedAt
            FROM ChildCompanyStaffAssignments
            WHERE ChildCompanyId = @ChildCompanyId
            ORDER BY StartDate DESC";

        command.Parameters.AddWithValue("@ChildCompanyId", childCompanyId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ChildCompanyStaffAssignment(
                AssignmentId: reader.GetString(0),
                StaffId: reader.GetString(1),
                ChildCompanyId: reader.GetString(2),
                AssignmentType: Enum.Parse<StaffAssignmentType>(reader.GetString(3)),
                TimePercentage: reader.GetDouble(4),
                StartDate: DateTime.Parse(reader.GetString(5)),
                EndDate: reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                MissionObjective: reader.IsDBNull(7) ? null : reader.GetString(7),
                CreatedAt: DateTime.Parse(reader.GetString(8))
            ));
        }

        return result;
    }

    public async Task<IReadOnlyList<ChildCompanyStaffAssignment>> GetActiveStaffAssignmentsAsync(string childCompanyId)
    {
        var result = new List<ChildCompanyStaffAssignment>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT AssignmentId, StaffId, ChildCompanyId, AssignmentType, TimePercentage,
                   StartDate, EndDate, MissionObjective, CreatedAt
            FROM ChildCompanyStaffAssignments
            WHERE ChildCompanyId = @ChildCompanyId
              AND StartDate <= @Now
              AND (EndDate IS NULL OR EndDate > @Now)
            ORDER BY StartDate DESC";

        command.Parameters.AddWithValue("@ChildCompanyId", childCompanyId);
        command.Parameters.AddWithValue("@Now", DateTime.Now.ToString("O"));

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ChildCompanyStaffAssignment(
                AssignmentId: reader.GetString(0),
                StaffId: reader.GetString(1),
                ChildCompanyId: reader.GetString(2),
                AssignmentType: Enum.Parse<StaffAssignmentType>(reader.GetString(3)),
                TimePercentage: reader.GetDouble(4),
                StartDate: DateTime.Parse(reader.GetString(5)),
                EndDate: reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                MissionObjective: reader.IsDBNull(7) ? null : reader.GetString(7),
                CreatedAt: DateTime.Parse(reader.GetString(8))
            ));
        }

        return result;
    }

    public async Task UpdateStaffAssignmentAsync(ChildCompanyStaffAssignment assignment)
    {
        if (!IsValid(assignment, out var errorMessage))
        {
            throw new ArgumentException($"StaffAssignment invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE ChildCompanyStaffAssignments
            SET StaffId = @StaffId,
                ChildCompanyId = @ChildCompanyId,
                AssignmentType = @AssignmentType,
                TimePercentage = @TimePercentage,
                StartDate = @StartDate,
                EndDate = @EndDate,
                MissionObjective = @MissionObjective
            WHERE AssignmentId = @AssignmentId";

        command.Parameters.AddWithValue("@AssignmentId", assignment.AssignmentId);
        command.Parameters.AddWithValue("@StaffId", assignment.StaffId);
        command.Parameters.AddWithValue("@ChildCompanyId", assignment.ChildCompanyId);
        command.Parameters.AddWithValue("@AssignmentType", assignment.AssignmentType.ToString());
        command.Parameters.AddWithValue("@TimePercentage", assignment.TimePercentage);
        command.Parameters.AddWithValue("@StartDate", assignment.StartDate.ToString("O"));
        command.Parameters.AddWithValue("@EndDate", assignment.EndDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@MissionObjective", assignment.MissionObjective ?? (object)DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"StaffAssignment avec ID {assignment.AssignmentId} introuvable");
        }
    }

    public async Task DeleteStaffAssignmentAsync(string assignmentId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM ChildCompanyStaffAssignments WHERE AssignmentId = @AssignmentId";

        command.Parameters.AddWithValue("@AssignmentId", assignmentId);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // STAFF SHARING SCHEDULE OPERATIONS
    // ====================================================================

    public async Task SaveStaffSharingScheduleAsync(StaffSharingSchedule schedule)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO StaffSharingSchedules (
                ScheduleId, StaffId, WeekNumber, MondayLocation, TuesdayLocation,
                WednesdayLocation, ThursdayLocation, FridayLocation, SaturdayLocation, SundayLocation
            ) VALUES (
                @ScheduleId, @StaffId, @WeekNumber, @MondayLocation, @TuesdayLocation,
                @WednesdayLocation, @ThursdayLocation, @FridayLocation, @SaturdayLocation, @SundayLocation
            )";

        command.Parameters.AddWithValue("@ScheduleId", schedule.ScheduleId);
        command.Parameters.AddWithValue("@StaffId", schedule.StaffId);
        command.Parameters.AddWithValue("@WeekNumber", schedule.WeekNumber);
        command.Parameters.AddWithValue("@MondayLocation", schedule.MondayLocation ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@TuesdayLocation", schedule.TuesdayLocation ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@WednesdayLocation", schedule.WednesdayLocation ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ThursdayLocation", schedule.ThursdayLocation ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@FridayLocation", schedule.FridayLocation ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SaturdayLocation", schedule.SaturdayLocation ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SundayLocation", schedule.SundayLocation ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<StaffSharingSchedule?> GetStaffSharingScheduleAsync(string staffId, int weekNumber)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ScheduleId, StaffId, WeekNumber, MondayLocation, TuesdayLocation,
                   WednesdayLocation, ThursdayLocation, FridayLocation, SaturdayLocation, SundayLocation
            FROM StaffSharingSchedules
            WHERE StaffId = @StaffId AND WeekNumber = @WeekNumber";

        command.Parameters.AddWithValue("@StaffId", staffId);
        command.Parameters.AddWithValue("@WeekNumber", weekNumber);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new StaffSharingSchedule(
                ScheduleId: reader.GetString(0),
                StaffId: reader.GetString(1),
                WeekNumber: reader.GetInt32(2),
                MondayLocation: reader.IsDBNull(3) ? null : reader.GetString(3),
                TuesdayLocation: reader.IsDBNull(4) ? null : reader.GetString(4),
                WednesdayLocation: reader.IsDBNull(5) ? null : reader.GetString(5),
                ThursdayLocation: reader.IsDBNull(6) ? null : reader.GetString(6),
                FridayLocation: reader.IsDBNull(7) ? null : reader.GetString(7),
                SaturdayLocation: reader.IsDBNull(8) ? null : reader.GetString(8),
                SundayLocation: reader.IsDBNull(9) ? null : reader.GetString(9)
            );
        }

        return null;
    }

    public async Task<IReadOnlyList<StaffSharingSchedule>> GetStaffSharingSchedulesAsync(string staffId)
    {
        var result = new List<StaffSharingSchedule>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ScheduleId, StaffId, WeekNumber, MondayLocation, TuesdayLocation,
                   WednesdayLocation, ThursdayLocation, FridayLocation, SaturdayLocation, SundayLocation
            FROM StaffSharingSchedules
            WHERE StaffId = @StaffId
            ORDER BY WeekNumber DESC";

        command.Parameters.AddWithValue("@StaffId", staffId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new StaffSharingSchedule(
                ScheduleId: reader.GetString(0),
                StaffId: reader.GetString(1),
                WeekNumber: reader.GetInt32(2),
                MondayLocation: reader.IsDBNull(3) ? null : reader.GetString(3),
                TuesdayLocation: reader.IsDBNull(4) ? null : reader.GetString(4),
                WednesdayLocation: reader.IsDBNull(5) ? null : reader.GetString(5),
                ThursdayLocation: reader.IsDBNull(6) ? null : reader.GetString(6),
                FridayLocation: reader.IsDBNull(7) ? null : reader.GetString(7),
                SaturdayLocation: reader.IsDBNull(8) ? null : reader.GetString(8),
                SundayLocation: reader.IsDBNull(9) ? null : reader.GetString(9)
            ));
        }

        return result;
    }

    public async Task UpdateStaffSharingScheduleAsync(StaffSharingSchedule schedule)
    {
        await SaveStaffSharingScheduleAsync(schedule); // INSERT OR REPLACE
    }

    public async Task DeleteStaffSharingScheduleAsync(string staffId, int weekNumber)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM StaffSharingSchedules WHERE StaffId = @StaffId AND WeekNumber = @WeekNumber";

        command.Parameters.AddWithValue("@StaffId", staffId);
        command.Parameters.AddWithValue("@WeekNumber", weekNumber);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // PROGRESSION IMPACT OPERATIONS
    // ====================================================================

    public async Task SaveStaffProgressionImpactAsync(StaffProgressionImpact impact)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO StaffProgressionImpacts (
                StaffId, YouthStructureId, InRingBonus, EntertainmentBonus,
                StoryBonus, MentalBonus, CompatibilityScore, FatigueModifier, CalculatedAt
            ) VALUES (
                @StaffId, @YouthStructureId, @InRingBonus, @EntertainmentBonus,
                @StoryBonus, @MentalBonus, @CompatibilityScore, @FatigueModifier, @CalculatedAt
            )";

        command.Parameters.AddWithValue("@StaffId", impact.StaffId);
        command.Parameters.AddWithValue("@YouthStructureId", impact.YouthStructureId);
        command.Parameters.AddWithValue("@InRingBonus", impact.AttributeBonuses.GetValueOrDefault("inring", 0));
        command.Parameters.AddWithValue("@EntertainmentBonus", impact.AttributeBonuses.GetValueOrDefault("entertainment", 0));
        command.Parameters.AddWithValue("@StoryBonus", impact.AttributeBonuses.GetValueOrDefault("story", 0));
        command.Parameters.AddWithValue("@MentalBonus", impact.AttributeBonuses.GetValueOrDefault("mental", 0));
        command.Parameters.AddWithValue("@CompatibilityScore", impact.CompatibilityScore);
        command.Parameters.AddWithValue("@FatigueModifier", impact.FatigueModifier);
        command.Parameters.AddWithValue("@CalculatedAt", impact.CalculatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<StaffProgressionImpact?> GetStaffProgressionImpactAsync(string staffId, string youthStructureId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, YouthStructureId, InRingBonus, EntertainmentBonus,
                   StoryBonus, MentalBonus, CompatibilityScore, FatigueModifier, CalculatedAt
            FROM StaffProgressionImpacts
            WHERE StaffId = @StaffId AND YouthStructureId = @YouthStructureId";

        command.Parameters.AddWithValue("@StaffId", staffId);
        command.Parameters.AddWithValue("@YouthStructureId", youthStructureId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var attributeBonuses = new Dictionary<string, double>
            {
                ["inring"] = reader.GetDouble(2),
                ["entertainment"] = reader.GetDouble(3),
                ["story"] = reader.GetDouble(4),
                ["mental"] = reader.GetDouble(5)
            };

            return new StaffProgressionImpact(
                StaffId: reader.GetString(0),
                YouthStructureId: reader.GetString(1),
                AttributeBonuses: attributeBonuses,
                CompatibilityScore: reader.GetDouble(6),
                FatigueModifier: reader.GetDouble(7),
                CalculatedAt: DateTime.Parse(reader.GetString(8))
            );
        }

        return null;
    }

    public async Task<IReadOnlyList<StaffProgressionImpact>> GetStaffImpactsForYouthStructureAsync(string youthStructureId)
    {
        var result = new List<StaffProgressionImpact>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, YouthStructureId, InRingBonus, EntertainmentBonus,
                   StoryBonus, MentalBonus, CompatibilityScore, FatigueModifier, CalculatedAt
            FROM StaffProgressionImpacts
            WHERE YouthStructureId = @YouthStructureId
            ORDER BY CalculatedAt DESC";

        command.Parameters.AddWithValue("@YouthStructureId", youthStructureId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var attributeBonuses = new Dictionary<string, double>
            {
                ["inring"] = reader.GetDouble(2),
                ["entertainment"] = reader.GetDouble(3),
                ["story"] = reader.GetDouble(4),
                ["mental"] = reader.GetDouble(5)
            };

            result.Add(new StaffProgressionImpact(
                StaffId: reader.GetString(0),
                YouthStructureId: reader.GetString(1),
                AttributeBonuses: attributeBonuses,
                CompatibilityScore: reader.GetDouble(6),
                FatigueModifier: reader.GetDouble(7),
                CalculatedAt: DateTime.Parse(reader.GetString(8))
            ));
        }

        return result;
    }

    public async Task<IReadOnlyList<StaffProgressionImpact>> GetStaffImpactsForStaffAsync(string staffId)
    {
        var result = new List<StaffProgressionImpact>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, YouthStructureId, InRingBonus, EntertainmentBonus,
                   StoryBonus, MentalBonus, CompatibilityScore, FatigueModifier, CalculatedAt
            FROM StaffProgressionImpacts
            WHERE StaffId = @StaffId
            ORDER BY CalculatedAt DESC";

        command.Parameters.AddWithValue("@StaffId", staffId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var attributeBonuses = new Dictionary<string, double>
            {
                ["inring"] = reader.GetDouble(2),
                ["entertainment"] = reader.GetDouble(3),
                ["story"] = reader.GetDouble(4),
                ["mental"] = reader.GetDouble(5)
            };

            result.Add(new StaffProgressionImpact(
                StaffId: reader.GetString(0),
                YouthStructureId: reader.GetString(1),
                AttributeBonuses: attributeBonuses,
                CompatibilityScore: reader.GetDouble(6),
                FatigueModifier: reader.GetDouble(7),
                CalculatedAt: DateTime.Parse(reader.GetString(8))
            ));
        }

        return result;
    }

    public async Task UpdateStaffProgressionImpactAsync(StaffProgressionImpact impact)
    {
        await SaveStaffProgressionImpactAsync(impact); // INSERT OR REPLACE
    }

    public async Task DeleteStaffProgressionImpactAsync(string staffId, string youthStructureId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM StaffProgressionImpacts WHERE StaffId = @StaffId AND YouthStructureId = @YouthStructureId";

        command.Parameters.AddWithValue("@StaffId", staffId);
        command.Parameters.AddWithValue("@YouthStructureId", youthStructureId);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    public async Task<IReadOnlyList<StaffAvailabilityResult>> CalculateStaffAvailabilitiesAsync(string companyId, DateTime period)
    {
        // Cette méthode nécessiterait une logique complexe pour calculer les disponibilités
        // basée sur les assignations existantes, les plannings, etc.
        // Pour l'instant, on retourne une liste vide - sera implémentée plus tard
        return Array.Empty<StaffAvailabilityResult>();
    }

    public async Task<IReadOnlyList<StaffSharingConflict>> DetectStaffSharingConflictsAsync(string companyId, DateTime startDate, DateTime endDate)
    {
        // Cette méthode nécessiterait une logique complexe pour détecter les conflits
        // Pour l'instant, on retourne une liste vide - sera implémentée plus tard
        return Array.Empty<StaffSharingConflict>();
    }

    public async Task<StaffImpactSummary> CalculateStaffImpactSummaryAsync(string youthStructureId)
    {
        var impacts = await GetStaffImpactsForYouthStructureAsync(youthStructureId);

        if (!impacts.Any())
        {
            return new StaffImpactSummary(
                YouthStructureId: youthStructureId,
                TotalAssignedStaff: 0,
                TotalBonuses: new Dictionary<string, double>(),
                AverageCompatibilityScore: 1.0,
                AverageFatigueModifier: 1.0,
                EstimatedProgressionMultiplier: 1.0,
                TotalMonthlyCost: 0,
                EstimatedROI: 0,
                LastUpdated: DateTime.Now
            );
        }

        var totalBonuses = new Dictionary<string, double>
        {
            ["inring"] = impacts.Sum(i => i.AttributeBonuses.GetValueOrDefault("inring", 0)),
            ["entertainment"] = impacts.Sum(i => i.AttributeBonuses.GetValueOrDefault("entertainment", 0)),
            ["story"] = impacts.Sum(i => i.AttributeBonuses.GetValueOrDefault("story", 0)),
            ["mental"] = impacts.Sum(i => i.AttributeBonuses.GetValueOrDefault("mental", 0))
        };

        var avgCompatibility = impacts.Average(i => i.CompatibilityScore);
        var avgFatigue = impacts.Average(i => i.FatigueModifier);

        var progressionMultiplier = 1.0;
        foreach (var bonus in totalBonuses.Values)
        {
            progressionMultiplier *= (1.0 + bonus);
        }
        progressionMultiplier *= avgCompatibility * avgFatigue;

        return new StaffImpactSummary(
            YouthStructureId: youthStructureId,
            TotalAssignedStaff: impacts.Count,
            TotalBonuses: totalBonuses,
            AverageCompatibilityScore: avgCompatibility,
            AverageFatigueModifier: avgFatigue,
            EstimatedProgressionMultiplier: progressionMultiplier,
            TotalMonthlyCost: 0, // TODO: Calculer à partir des salaires
            EstimatedROI: progressionMultiplier * 100, // Simplifié
            LastUpdated: impacts.Max(i => i.CalculatedAt)
        );
    }

    public async Task<int> CountAssignedStaffByChildCompanyAsync(string childCompanyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(DISTINCT StaffId)
            FROM ChildCompanyStaffAssignments
            WHERE ChildCompanyId = @ChildCompanyId
              AND StartDate <= @Now
              AND (EndDate IS NULL OR EndDate > @Now)";

        command.Parameters.AddWithValue("@ChildCompanyId", childCompanyId);
        command.Parameters.AddWithValue("@Now", DateTime.Now.ToString("O"));

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static bool IsValid(ChildCompanyStaffAssignment assignment, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(assignment.AssignmentId))
        {
            errorMessage = "AssignmentId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(assignment.StaffId))
        {
            errorMessage = "StaffId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(assignment.ChildCompanyId))
        {
            errorMessage = "ChildCompanyId ne peut pas être vide";
            return false;
        }

        if (assignment.TimePercentage < 0.1 || assignment.TimePercentage > 1.0)
        {
            errorMessage = "TimePercentage doit être entre 0.1 et 1.0";
            return false;
        }

        if (assignment.EndDate.HasValue && assignment.EndDate.Value < assignment.StartDate)
        {
            errorMessage = "EndDate ne peut pas être avant StartDate";
            return false;
        }

        return true;
    }
}