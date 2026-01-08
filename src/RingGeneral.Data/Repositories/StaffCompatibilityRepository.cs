using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models.Staff;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des compatibilités staff-booker.
/// </summary>
public sealed class StaffCompatibilityRepository : IStaffCompatibilityRepository
{
    private readonly string _connectionString;

    public StaffCompatibilityRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // STAFF COMPATIBILITY CRUD OPERATIONS
    // ====================================================================

    public async Task SaveCompatibilityAsync(StaffCompatibility compatibility)
    {
        if (!compatibility.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"StaffCompatibility invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO StaffCompatibilities (
                CompatibilityId, StaffId, BookerId, OverallScore, CreativeVisionScore,
                BookingStyleScore, BiasAlignmentScore, RiskToleranceScore, PersonalChemistryScore,
                PositiveFactors, NegativeFactors, SuccessfulCollaborations, FailedCollaborations,
                ConflictHistory, LastCalculatedAt, CreatedAt
            ) VALUES (
                @CompatibilityId, @StaffId, @BookerId, @OverallScore, @CreativeVisionScore,
                @BookingStyleScore, @BiasAlignmentScore, @RiskToleranceScore, @PersonalChemistryScore,
                @PositiveFactors, @NegativeFactors, @SuccessfulCollaborations, @FailedCollaborations,
                @ConflictHistory, @LastCalculatedAt, @CreatedAt
            )";

        command.Parameters.AddWithValue("@CompatibilityId", compatibility.CompatibilityId);
        command.Parameters.AddWithValue("@StaffId", compatibility.StaffId);
        command.Parameters.AddWithValue("@BookerId", compatibility.BookerId);
        command.Parameters.AddWithValue("@OverallScore", compatibility.OverallScore);
        command.Parameters.AddWithValue("@CreativeVisionScore", compatibility.CreativeVisionScore);
        command.Parameters.AddWithValue("@BookingStyleScore", compatibility.BookingStyleScore);
        command.Parameters.AddWithValue("@BiasAlignmentScore", compatibility.BiasAlignmentScore);
        command.Parameters.AddWithValue("@RiskToleranceScore", compatibility.RiskToleranceScore);
        command.Parameters.AddWithValue("@PersonalChemistryScore", compatibility.PersonalChemistryScore);
        command.Parameters.AddWithValue("@PositiveFactors", compatibility.PositiveFactors);
        command.Parameters.AddWithValue("@NegativeFactors", compatibility.NegativeFactors);
        command.Parameters.AddWithValue("@SuccessfulCollaborations", compatibility.SuccessfulCollaborations);
        command.Parameters.AddWithValue("@FailedCollaborations", compatibility.FailedCollaborations);
        command.Parameters.AddWithValue("@ConflictHistory", compatibility.ConflictHistory);
        command.Parameters.AddWithValue("@LastCalculatedAt", compatibility.LastCalculatedAt.ToString("O"));
        command.Parameters.AddWithValue("@CreatedAt", compatibility.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<StaffCompatibility?> GetCompatibilityByIdAsync(string compatibilityId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CompatibilityId, StaffId, BookerId, OverallScore, CreativeVisionScore,
                   BookingStyleScore, BiasAlignmentScore, RiskToleranceScore, PersonalChemistryScore,
                   PositiveFactors, NegativeFactors, SuccessfulCollaborations, FailedCollaborations,
                   ConflictHistory, LastCalculatedAt, CreatedAt
            FROM StaffCompatibilities
            WHERE CompatibilityId = @CompatibilityId";

        command.Parameters.AddWithValue("@CompatibilityId", compatibilityId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCompatibility(reader);
        }

        return null;
    }

    public async Task<StaffCompatibility?> GetCompatibilityAsync(string staffId, string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CompatibilityId, StaffId, BookerId, OverallScore, CreativeVisionScore,
                   BookingStyleScore, BiasAlignmentScore, RiskToleranceScore, PersonalChemistryScore,
                   PositiveFactors, NegativeFactors, SuccessfulCollaborations, FailedCollaborations,
                   ConflictHistory, LastCalculatedAt, CreatedAt
            FROM StaffCompatibilities
            WHERE StaffId = @StaffId AND BookerId = @BookerId
            LIMIT 1";

        command.Parameters.AddWithValue("@StaffId", staffId);
        command.Parameters.AddWithValue("@BookerId", bookerId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCompatibility(reader);
        }

        return null;
    }

    public async Task<List<StaffCompatibility>> GetCompatibilitiesByStaffIdAsync(string staffId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CompatibilityId, StaffId, BookerId, OverallScore, CreativeVisionScore,
                   BookingStyleScore, BiasAlignmentScore, RiskToleranceScore, PersonalChemistryScore,
                   PositiveFactors, NegativeFactors, SuccessfulCollaborations, FailedCollaborations,
                   ConflictHistory, LastCalculatedAt, CreatedAt
            FROM StaffCompatibilities
            WHERE StaffId = @StaffId
            ORDER BY OverallScore DESC";

        command.Parameters.AddWithValue("@StaffId", staffId);

        return await ReadCompatibilities(command);
    }

    public async Task<List<StaffCompatibility>> GetCompatibilitiesByBookerIdAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CompatibilityId, StaffId, BookerId, OverallScore, CreativeVisionScore,
                   BookingStyleScore, BiasAlignmentScore, RiskToleranceScore, PersonalChemistryScore,
                   PositiveFactors, NegativeFactors, SuccessfulCollaborations, FailedCollaborations,
                   ConflictHistory, LastCalculatedAt, CreatedAt
            FROM StaffCompatibilities
            WHERE BookerId = @BookerId
            ORDER BY OverallScore DESC";

        command.Parameters.AddWithValue("@BookerId", bookerId);

        return await ReadCompatibilities(command);
    }

    public async Task UpdateCompatibilityAsync(StaffCompatibility compatibility)
    {
        if (!compatibility.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"StaffCompatibility invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE StaffCompatibilities SET
                OverallScore = @OverallScore,
                CreativeVisionScore = @CreativeVisionScore,
                BookingStyleScore = @BookingStyleScore,
                BiasAlignmentScore = @BiasAlignmentScore,
                RiskToleranceScore = @RiskToleranceScore,
                PersonalChemistryScore = @PersonalChemistryScore,
                PositiveFactors = @PositiveFactors,
                NegativeFactors = @NegativeFactors,
                SuccessfulCollaborations = @SuccessfulCollaborations,
                FailedCollaborations = @FailedCollaborations,
                ConflictHistory = @ConflictHistory,
                LastCalculatedAt = @LastCalculatedAt
            WHERE CompatibilityId = @CompatibilityId";

        command.Parameters.AddWithValue("@CompatibilityId", compatibility.CompatibilityId);
        command.Parameters.AddWithValue("@OverallScore", compatibility.OverallScore);
        command.Parameters.AddWithValue("@CreativeVisionScore", compatibility.CreativeVisionScore);
        command.Parameters.AddWithValue("@BookingStyleScore", compatibility.BookingStyleScore);
        command.Parameters.AddWithValue("@BiasAlignmentScore", compatibility.BiasAlignmentScore);
        command.Parameters.AddWithValue("@RiskToleranceScore", compatibility.RiskToleranceScore);
        command.Parameters.AddWithValue("@PersonalChemistryScore", compatibility.PersonalChemistryScore);
        command.Parameters.AddWithValue("@PositiveFactors", compatibility.PositiveFactors);
        command.Parameters.AddWithValue("@NegativeFactors", compatibility.NegativeFactors);
        command.Parameters.AddWithValue("@SuccessfulCollaborations", compatibility.SuccessfulCollaborations);
        command.Parameters.AddWithValue("@FailedCollaborations", compatibility.FailedCollaborations);
        command.Parameters.AddWithValue("@ConflictHistory", compatibility.ConflictHistory);
        command.Parameters.AddWithValue("@LastCalculatedAt", DateTime.Now.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteCompatibilityAsync(string compatibilityId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM StaffCompatibilities WHERE CompatibilityId = @CompatibilityId";

        command.Parameters.AddWithValue("@CompatibilityId", compatibilityId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task IncrementCollaborationAsync(string compatibilityId, bool successful)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = successful
            ? "UPDATE StaffCompatibilities SET SuccessfulCollaborations = SuccessfulCollaborations + 1 WHERE CompatibilityId = @CompatibilityId"
            : "UPDATE StaffCompatibilities SET FailedCollaborations = FailedCollaborations + 1 WHERE CompatibilityId = @CompatibilityId";

        command.Parameters.AddWithValue("@CompatibilityId", compatibilityId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task IncrementConflictHistoryAsync(string compatibilityId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE StaffCompatibilities SET ConflictHistory = ConflictHistory + 1 WHERE CompatibilityId = @CompatibilityId";

        command.Parameters.AddWithValue("@CompatibilityId", compatibilityId);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    public async Task<List<StaffCompatibility>> GetDangerousCompatibilitiesAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT sc.CompatibilityId, sc.StaffId, sc.BookerId, sc.OverallScore, sc.CreativeVisionScore,
                   sc.BookingStyleScore, sc.BiasAlignmentScore, sc.RiskToleranceScore, sc.PersonalChemistryScore,
                   sc.PositiveFactors, sc.NegativeFactors, sc.SuccessfulCollaborations, sc.FailedCollaborations,
                   sc.ConflictHistory, sc.LastCalculatedAt, sc.CreatedAt
            FROM StaffCompatibilities sc
            INNER JOIN CreativeStaff cs ON sc.StaffId = cs.StaffId
            WHERE cs.CompanyId = @CompanyId AND sc.OverallScore <= 30
            ORDER BY sc.OverallScore";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        return await ReadCompatibilities(command);
    }

    public async Task<List<StaffCompatibility>> GetExcellentCompatibilitiesAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT sc.CompatibilityId, sc.StaffId, sc.BookerId, sc.OverallScore, sc.CreativeVisionScore,
                   sc.BookingStyleScore, sc.BiasAlignmentScore, sc.RiskToleranceScore, sc.PersonalChemistryScore,
                   sc.PositiveFactors, sc.NegativeFactors, sc.SuccessfulCollaborations, sc.FailedCollaborations,
                   sc.ConflictHistory, sc.LastCalculatedAt, sc.CreatedAt
            FROM StaffCompatibilities sc
            INNER JOIN CreativeStaff cs ON sc.StaffId = cs.StaffId
            WHERE cs.CompanyId = @CompanyId AND sc.OverallScore >= 80
            ORDER BY sc.OverallScore DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        return await ReadCompatibilities(command);
    }

    public async Task<List<StaffCompatibility>> GetCompatibilitiesNeedingRecalculationAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var thresholdDate = DateTime.Now.AddDays(-30).ToString("O");

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CompatibilityId, StaffId, BookerId, OverallScore, CreativeVisionScore,
                   BookingStyleScore, BiasAlignmentScore, RiskToleranceScore, PersonalChemistryScore,
                   PositiveFactors, NegativeFactors, SuccessfulCollaborations, FailedCollaborations,
                   ConflictHistory, LastCalculatedAt, CreatedAt
            FROM StaffCompatibilities
            WHERE LastCalculatedAt < @ThresholdDate
            ORDER BY LastCalculatedAt";

        command.Parameters.AddWithValue("@ThresholdDate", thresholdDate);

        return await ReadCompatibilities(command);
    }

    public async Task<bool> CompatibilityExistsAsync(string staffId, string bookerId)
    {
        var compatibility = await GetCompatibilityAsync(staffId, bookerId);
        return compatibility != null;
    }

    public async Task<double> CalculateAverageCompatibilityScoreAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT AVG(OverallScore)
            FROM StaffCompatibilities
            WHERE BookerId = @BookerId";

        command.Parameters.AddWithValue("@BookerId", bookerId);

        var result = await command.ExecuteScalarAsync();
        return result == DBNull.Value ? 0.0 : Convert.ToDouble(result);
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private static async Task<List<StaffCompatibility>> ReadCompatibilities(SqliteCommand command)
    {
        var compatibilities = new List<StaffCompatibility>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            compatibilities.Add(MapCompatibility(reader));
        }
        return compatibilities;
    }

    private static StaffCompatibility MapCompatibility(SqliteDataReader reader)
    {
        return new StaffCompatibility
        {
            CompatibilityId = reader.GetString(0),
            StaffId = reader.GetString(1),
            BookerId = reader.GetString(2),
            OverallScore = reader.GetInt32(3),
            CreativeVisionScore = reader.GetInt32(4),
            BookingStyleScore = reader.GetInt32(5),
            BiasAlignmentScore = reader.GetInt32(6),
            RiskToleranceScore = reader.GetInt32(7),
            PersonalChemistryScore = reader.GetInt32(8),
            PositiveFactors = reader.GetString(9),
            NegativeFactors = reader.GetString(10),
            SuccessfulCollaborations = reader.GetInt32(11),
            FailedCollaborations = reader.GetInt32(12),
            ConflictHistory = reader.GetInt32(13),
            LastCalculatedAt = DateTime.Parse(reader.GetString(14)),
            CreatedAt = DateTime.Parse(reader.GetString(15))
        };
    }
}
