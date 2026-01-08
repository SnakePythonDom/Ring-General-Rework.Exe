using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models.Owner;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des propriétaires (Owners).
/// </summary>
public sealed class OwnerRepository : IOwnerRepository
{
    private readonly string _connectionString;

    public OwnerRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task SaveOwnerAsync(Owner owner)
    {
        if (!owner.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Owner invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Owners (
                OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                PreferredProductType, ShowFrequencyPreference,
                TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                CreatedAt
            ) VALUES (
                @OwnerId, @CompanyId, @Name, @VisionType, @RiskTolerance,
                @PreferredProductType, @ShowFrequencyPreference,
                @TalentDevelopmentFocus, @FinancialPriority, @FanSatisfactionPriority,
                @CreatedAt
            )";

        command.Parameters.AddWithValue("@OwnerId", owner.OwnerId);
        command.Parameters.AddWithValue("@CompanyId", owner.CompanyId);
        command.Parameters.AddWithValue("@Name", owner.Name);
        command.Parameters.AddWithValue("@VisionType", owner.VisionType);
        command.Parameters.AddWithValue("@RiskTolerance", owner.RiskTolerance);
        command.Parameters.AddWithValue("@PreferredProductType", owner.PreferredProductType);
        command.Parameters.AddWithValue("@ShowFrequencyPreference", owner.ShowFrequencyPreference);
        command.Parameters.AddWithValue("@TalentDevelopmentFocus", owner.TalentDevelopmentFocus);
        command.Parameters.AddWithValue("@FinancialPriority", owner.FinancialPriority);
        command.Parameters.AddWithValue("@FanSatisfactionPriority", owner.FanSatisfactionPriority);
        command.Parameters.AddWithValue("@CreatedAt", owner.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Owner?> GetOwnerByIdAsync(string ownerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                   PreferredProductType, ShowFrequencyPreference,
                   TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                   CreatedAt
            FROM Owners
            WHERE OwnerId = @OwnerId";

        command.Parameters.AddWithValue("@OwnerId", ownerId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapOwner(reader);
        }

        return null;
    }

    public async Task<Owner?> GetOwnerByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                   PreferredProductType, ShowFrequencyPreference,
                   TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                   CreatedAt
            FROM Owners
            WHERE CompanyId = @CompanyId";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapOwner(reader);
        }

        return null;
    }

    public async Task<List<Owner>> GetAllOwnersAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                   PreferredProductType, ShowFrequencyPreference,
                   TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                   CreatedAt
            FROM Owners
            ORDER BY Name";

        var owners = new List<Owner>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            owners.Add(MapOwner(reader));
        }

        return owners;
    }

    public async Task<List<Owner>> GetOwnersByVisionTypeAsync(string visionType)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                   PreferredProductType, ShowFrequencyPreference,
                   TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                   CreatedAt
            FROM Owners
            WHERE VisionType = @VisionType
            ORDER BY Name";

        command.Parameters.AddWithValue("@VisionType", visionType);

        var owners = new List<Owner>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            owners.Add(MapOwner(reader));
        }

        return owners;
    }

    public async Task<List<Owner>> GetOwnersWithRiskToleranceAboveAsync(int minRiskTolerance)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                   PreferredProductType, ShowFrequencyPreference,
                   TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                   CreatedAt
            FROM Owners
            WHERE RiskTolerance >= @MinRiskTolerance
            ORDER BY RiskTolerance DESC";

        command.Parameters.AddWithValue("@MinRiskTolerance", minRiskTolerance);

        var owners = new List<Owner>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            owners.Add(MapOwner(reader));
        }

        return owners;
    }

    public async Task UpdateOwnerAsync(Owner owner)
    {
        if (!owner.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Owner invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Owners SET
                CompanyId = @CompanyId,
                Name = @Name,
                VisionType = @VisionType,
                RiskTolerance = @RiskTolerance,
                PreferredProductType = @PreferredProductType,
                ShowFrequencyPreference = @ShowFrequencyPreference,
                TalentDevelopmentFocus = @TalentDevelopmentFocus,
                FinancialPriority = @FinancialPriority,
                FanSatisfactionPriority = @FanSatisfactionPriority
            WHERE OwnerId = @OwnerId";

        command.Parameters.AddWithValue("@OwnerId", owner.OwnerId);
        command.Parameters.AddWithValue("@CompanyId", owner.CompanyId);
        command.Parameters.AddWithValue("@Name", owner.Name);
        command.Parameters.AddWithValue("@VisionType", owner.VisionType);
        command.Parameters.AddWithValue("@RiskTolerance", owner.RiskTolerance);
        command.Parameters.AddWithValue("@PreferredProductType", owner.PreferredProductType);
        command.Parameters.AddWithValue("@ShowFrequencyPreference", owner.ShowFrequencyPreference);
        command.Parameters.AddWithValue("@TalentDevelopmentFocus", owner.TalentDevelopmentFocus);
        command.Parameters.AddWithValue("@FinancialPriority", owner.FinancialPriority);
        command.Parameters.AddWithValue("@FanSatisfactionPriority", owner.FanSatisfactionPriority);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteOwnerAsync(string ownerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Owners WHERE OwnerId = @OwnerId";
        command.Parameters.AddWithValue("@OwnerId", ownerId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CountOwnersAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Owners";

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> CompanyHasOwnerAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Owners WHERE CompanyId = @CompanyId";
        command.Parameters.AddWithValue("@CompanyId", companyId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private static Owner MapOwner(SqliteDataReader reader)
    {
        return new Owner
        {
            OwnerId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            Name = reader.GetString(2),
            VisionType = reader.GetString(3),
            RiskTolerance = reader.GetInt32(4),
            PreferredProductType = reader.GetString(5),
            ShowFrequencyPreference = reader.GetString(6),
            TalentDevelopmentFocus = reader.GetInt32(7),
            FinancialPriority = reader.GetInt32(8),
            FanSatisfactionPriority = reader.GetInt32(9),
            CreatedAt = DateTime.Parse(reader.GetString(10))
        };
    }
}
