using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.ChildCompany;
using RingGeneral.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository pour les Child Companies.
/// </summary>
public sealed class ChildCompanyRepository : IChildCompanyRepository
{
    private readonly string _connectionString;

    public ChildCompanyRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // CHILD COMPANY CRUD OPERATIONS
    // ====================================================================

    public async Task SaveChildCompanyAsync(ChildCompany childCompany)
    {
        if (!IsValid(childCompany, out var errorMessage))
        {
            throw new ArgumentException($"ChildCompany invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO ChildCompanies (
                ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget, CreatedAt
            ) VALUES (
                @ChildCompanyId, @ParentCompanyId, @Name, @RegionId, @Level, @MonthlyBudget, @CreatedAt
            )";

        command.Parameters.AddWithValue("@ChildCompanyId", childCompany.ChildCompanyId);
        command.Parameters.AddWithValue("@ParentCompanyId", childCompany.ParentCompanyId);
        command.Parameters.AddWithValue("@Name", childCompany.Name);
        command.Parameters.AddWithValue("@RegionId", childCompany.RegionId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Level", childCompany.Level.ToString());
        command.Parameters.AddWithValue("@MonthlyBudget", childCompany.MonthlyBudget);
        command.Parameters.AddWithValue("@CreatedAt", childCompany.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<ChildCompany?> GetChildCompanyByIdAsync(string childCompanyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget, CreatedAt
            FROM ChildCompanies
            WHERE ChildCompanyId = @ChildCompanyId";

        command.Parameters.AddWithValue("@ChildCompanyId", childCompanyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ChildCompany(
                ChildCompanyId: reader.GetString(0),
                ParentCompanyId: reader.GetString(1),
                Name: reader.GetString(2),
                RegionId: reader.IsDBNull(3) ? null : reader.GetString(3),
                Level: Enum.Parse<ChildCompanyLevel>(reader.GetString(4)),
                MonthlyBudget: reader.GetDecimal(5),
                CreatedAt: DateTime.Parse(reader.GetString(6))
            );
        }

        return null;
    }

    public async Task<IReadOnlyList<ChildCompany>> GetChildCompaniesByParentAsync(string parentCompanyId)
    {
        var result = new List<ChildCompany>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget, CreatedAt
            FROM ChildCompanies
            WHERE ParentCompanyId = @ParentCompanyId
            ORDER BY Name";

        command.Parameters.AddWithValue("@ParentCompanyId", parentCompanyId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ChildCompany(
                ChildCompanyId: reader.GetString(0),
                ParentCompanyId: reader.GetString(1),
                Name: reader.GetString(2),
                RegionId: reader.IsDBNull(3) ? null : reader.GetString(3),
                Level: Enum.Parse<ChildCompanyLevel>(reader.GetString(4)),
                MonthlyBudget: reader.GetDecimal(5),
                CreatedAt: DateTime.Parse(reader.GetString(6))
            ));
        }

        return result;
    }

    public async Task<IReadOnlyList<ChildCompany>> GetChildCompaniesByRegionAsync(string regionId)
    {
        var result = new List<ChildCompany>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget, CreatedAt
            FROM ChildCompanies
            WHERE RegionId = @RegionId
            ORDER BY Name";

        command.Parameters.AddWithValue("@RegionId", regionId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ChildCompany(
                ChildCompanyId: reader.GetString(0),
                ParentCompanyId: reader.GetString(1),
                Name: reader.GetString(2),
                RegionId: reader.IsDBNull(3) ? null : reader.GetString(3),
                Level: Enum.Parse<ChildCompanyLevel>(reader.GetString(4)),
                MonthlyBudget: reader.GetDecimal(5),
                CreatedAt: DateTime.Parse(reader.GetString(6))
            ));
        }

        return result;
    }

    public async Task UpdateChildCompanyAsync(ChildCompany childCompany)
    {
        if (!IsValid(childCompany, out var errorMessage))
        {
            throw new ArgumentException($"ChildCompany invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE ChildCompanies
            SET ParentCompanyId = @ParentCompanyId,
                Name = @Name,
                RegionId = @RegionId,
                Level = @Level,
                MonthlyBudget = @MonthlyBudget
            WHERE ChildCompanyId = @ChildCompanyId";

        command.Parameters.AddWithValue("@ChildCompanyId", childCompany.ChildCompanyId);
        command.Parameters.AddWithValue("@ParentCompanyId", childCompany.ParentCompanyId);
        command.Parameters.AddWithValue("@Name", childCompany.Name);
        command.Parameters.AddWithValue("@RegionId", childCompany.RegionId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Level", childCompany.Level.ToString());
        command.Parameters.AddWithValue("@MonthlyBudget", childCompany.MonthlyBudget);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"ChildCompany avec ID {childCompany.ChildCompanyId} introuvable");
        }
    }

    public async Task DeleteChildCompanyAsync(string childCompanyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM ChildCompanies WHERE ChildCompanyId = @ChildCompanyId";

        command.Parameters.AddWithValue("@ChildCompanyId", childCompanyId);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    public async Task<int> CountActiveChildCompaniesAsync(string parentCompanyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM ChildCompanies WHERE ParentCompanyId = @ParentCompanyId";

        command.Parameters.AddWithValue("@ParentCompanyId", parentCompanyId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<decimal> CalculateTotalMonthlyBudgetAsync(string parentCompanyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(SUM(MonthlyBudget), 0) FROM ChildCompanies WHERE ParentCompanyId = @ParentCompanyId";

        command.Parameters.AddWithValue("@ParentCompanyId", parentCompanyId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToDecimal(result);
    }

    public async Task<IReadOnlyList<ChildCompany>> GetChildCompaniesByLevelAsync(string parentCompanyId, ChildCompanyLevel level)
    {
        var result = new List<ChildCompany>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget, CreatedAt
            FROM ChildCompanies
            WHERE ParentCompanyId = @ParentCompanyId AND Level = @Level
            ORDER BY Name";

        command.Parameters.AddWithValue("@ParentCompanyId", parentCompanyId);
        command.Parameters.AddWithValue("@Level", level.ToString());

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ChildCompany(
                ChildCompanyId: reader.GetString(0),
                ParentCompanyId: reader.GetString(1),
                Name: reader.GetString(2),
                RegionId: reader.IsDBNull(3) ? null : reader.GetString(3),
                Level: Enum.Parse<ChildCompanyLevel>(reader.GetString(4)),
                MonthlyBudget: reader.GetDecimal(5),
                CreatedAt: DateTime.Parse(reader.GetString(6))
            ));
        }

        return result;
    }

    private static bool IsValid(ChildCompany childCompany, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(childCompany.ChildCompanyId))
        {
            errorMessage = "ChildCompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(childCompany.ParentCompanyId))
        {
            errorMessage = "ParentCompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(childCompany.Name) || childCompany.Name.Length < 2)
        {
            errorMessage = "Name doit contenir au moins 2 caractères";
            return false;
        }

        if (childCompany.MonthlyBudget < 0)
        {
            errorMessage = "MonthlyBudget ne peut pas être négatif";
            return false;
        }

        return true;
    }
}