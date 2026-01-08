using Microsoft.Data.Sqlite;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des brands et hiérarchies.
/// </summary>
public sealed class BrandRepository : IBrandRepository
{
    private readonly string _connectionString;

    public BrandRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // BRAND CRUD OPERATIONS
    // ====================================================================

    public async Task SaveBrandAsync(Brand brand)
    {
        if (!brand.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Brand invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Brands (
                BrandId, CompanyId, Name, Objective, BookerId, CurrentEraId, Prestige,
                BudgetPerShow, AverageAudience, Priority, IsActive, AirDay, ShowDuration,
                TargetRegion, CreatedAt, DeactivatedAt
            ) VALUES (
                @BrandId, @CompanyId, @Name, @Objective, @BookerId, @CurrentEraId, @Prestige,
                @BudgetPerShow, @AverageAudience, @Priority, @IsActive, @AirDay, @ShowDuration,
                @TargetRegion, @CreatedAt, @DeactivatedAt
            )";

        command.Parameters.AddWithValue("@BrandId", brand.BrandId);
        command.Parameters.AddWithValue("@CompanyId", brand.CompanyId);
        command.Parameters.AddWithValue("@Name", brand.Name);
        command.Parameters.AddWithValue("@Objective", brand.Objective.ToString());
        command.Parameters.AddWithValue("@BookerId", brand.BookerId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CurrentEraId", brand.CurrentEraId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Prestige", brand.Prestige);
        command.Parameters.AddWithValue("@BudgetPerShow", brand.BudgetPerShow);
        command.Parameters.AddWithValue("@AverageAudience", brand.AverageAudience);
        command.Parameters.AddWithValue("@Priority", brand.Priority);
        command.Parameters.AddWithValue("@IsActive", brand.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@AirDay", brand.AirDay ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ShowDuration", brand.ShowDuration);
        command.Parameters.AddWithValue("@TargetRegion", brand.TargetRegion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", brand.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@DeactivatedAt", brand.DeactivatedAt?.ToString("O") ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Brand?> GetBrandByIdAsync(string brandId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BrandId, CompanyId, Name, Objective, BookerId, CurrentEraId, Prestige,
                   BudgetPerShow, AverageAudience, Priority, IsActive, AirDay, ShowDuration,
                   TargetRegion, CreatedAt, DeactivatedAt
            FROM Brands
            WHERE BrandId = @BrandId";

        command.Parameters.AddWithValue("@BrandId", brandId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapBrand(reader);
        }

        return null;
    }

    public async Task<List<Brand>> GetBrandsByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BrandId, CompanyId, Name, Objective, BookerId, CurrentEraId, Prestige,
                   BudgetPerShow, AverageAudience, Priority, IsActive, AirDay, ShowDuration,
                   TargetRegion, CreatedAt, DeactivatedAt
            FROM Brands
            WHERE CompanyId = @CompanyId
            ORDER BY Priority, Name";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var brands = new List<Brand>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            brands.Add(MapBrand(reader));
        }

        return brands;
    }

    public async Task<List<Brand>> GetActiveBrandsByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BrandId, CompanyId, Name, Objective, BookerId, CurrentEraId, Prestige,
                   BudgetPerShow, AverageAudience, Priority, IsActive, AirDay, ShowDuration,
                   TargetRegion, CreatedAt, DeactivatedAt
            FROM Brands
            WHERE CompanyId = @CompanyId AND IsActive = 1
            ORDER BY Priority, Name";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var brands = new List<Brand>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            brands.Add(MapBrand(reader));
        }

        return brands;
    }

    public async Task<Brand?> GetFlagshipBrandAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BrandId, CompanyId, Name, Objective, BookerId, CurrentEraId, Prestige,
                   BudgetPerShow, AverageAudience, Priority, IsActive, AirDay, ShowDuration,
                   TargetRegion, CreatedAt, DeactivatedAt
            FROM Brands
            WHERE CompanyId = @CompanyId AND IsActive = 1
            ORDER BY Priority, Prestige DESC
            LIMIT 1";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapBrand(reader);
        }

        return null;
    }

    public async Task UpdateBrandAsync(Brand brand)
    {
        if (!brand.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Brand invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Brands SET
                Name = @Name,
                Objective = @Objective,
                BookerId = @BookerId,
                CurrentEraId = @CurrentEraId,
                Prestige = @Prestige,
                BudgetPerShow = @BudgetPerShow,
                AverageAudience = @AverageAudience,
                Priority = @Priority,
                IsActive = @IsActive,
                AirDay = @AirDay,
                ShowDuration = @ShowDuration,
                TargetRegion = @TargetRegion,
                DeactivatedAt = @DeactivatedAt
            WHERE BrandId = @BrandId";

        command.Parameters.AddWithValue("@BrandId", brand.BrandId);
        command.Parameters.AddWithValue("@Name", brand.Name);
        command.Parameters.AddWithValue("@Objective", brand.Objective.ToString());
        command.Parameters.AddWithValue("@BookerId", brand.BookerId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CurrentEraId", brand.CurrentEraId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Prestige", brand.Prestige);
        command.Parameters.AddWithValue("@BudgetPerShow", brand.BudgetPerShow);
        command.Parameters.AddWithValue("@AverageAudience", brand.AverageAudience);
        command.Parameters.AddWithValue("@Priority", brand.Priority);
        command.Parameters.AddWithValue("@IsActive", brand.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@AirDay", brand.AirDay ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ShowDuration", brand.ShowDuration);
        command.Parameters.AddWithValue("@TargetRegion", brand.TargetRegion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DeactivatedAt", brand.DeactivatedAt?.ToString("O") ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeactivateBrandAsync(string brandId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Brands
            SET IsActive = 0, DeactivatedAt = @DeactivatedAt
            WHERE BrandId = @BrandId";

        command.Parameters.AddWithValue("@BrandId", brandId);
        command.Parameters.AddWithValue("@DeactivatedAt", DateTime.Now.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task AssignBookerToBrandAsync(string brandId, string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Brands SET BookerId = @BookerId WHERE BrandId = @BrandId";

        command.Parameters.AddWithValue("@BrandId", brandId);
        command.Parameters.AddWithValue("@BookerId", bookerId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveBookerFromBrandAsync(string brandId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Brands SET BookerId = NULL WHERE BrandId = @BrandId";

        command.Parameters.AddWithValue("@BrandId", brandId);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // COMPANY HIERARCHY OPERATIONS
    // ====================================================================

    public async Task SaveHierarchyAsync(CompanyHierarchy hierarchy)
    {
        if (!hierarchy.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"CompanyHierarchy invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO CompanyHierarchies (
                HierarchyId, CompanyId, Type, OwnerId, HeadBookerId, ActiveBrandCount,
                AllowsBrandAutonomy, CentralizationLevel, IsActive, CreatedAt, LastModifiedAt
            ) VALUES (
                @HierarchyId, @CompanyId, @Type, @OwnerId, @HeadBookerId, @ActiveBrandCount,
                @AllowsBrandAutonomy, @CentralizationLevel, @IsActive, @CreatedAt, @LastModifiedAt
            )";

        command.Parameters.AddWithValue("@HierarchyId", hierarchy.HierarchyId);
        command.Parameters.AddWithValue("@CompanyId", hierarchy.CompanyId);
        command.Parameters.AddWithValue("@Type", hierarchy.Type.ToString());
        command.Parameters.AddWithValue("@OwnerId", hierarchy.OwnerId);
        command.Parameters.AddWithValue("@HeadBookerId", hierarchy.HeadBookerId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ActiveBrandCount", hierarchy.ActiveBrandCount);
        command.Parameters.AddWithValue("@AllowsBrandAutonomy", hierarchy.AllowsBrandAutonomy ? 1 : 0);
        command.Parameters.AddWithValue("@CentralizationLevel", hierarchy.CentralizationLevel);
        command.Parameters.AddWithValue("@IsActive", hierarchy.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@CreatedAt", hierarchy.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@LastModifiedAt", hierarchy.LastModifiedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<CompanyHierarchy?> GetHierarchyByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT HierarchyId, CompanyId, Type, OwnerId, HeadBookerId, ActiveBrandCount,
                   AllowsBrandAutonomy, CentralizationLevel, IsActive, CreatedAt, LastModifiedAt
            FROM CompanyHierarchies
            WHERE CompanyId = @CompanyId AND IsActive = 1
            LIMIT 1";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapHierarchy(reader);
        }

        return null;
    }

    public async Task UpdateHierarchyAsync(CompanyHierarchy hierarchy)
    {
        if (!hierarchy.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"CompanyHierarchy invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CompanyHierarchies SET
                Type = @Type,
                OwnerId = @OwnerId,
                HeadBookerId = @HeadBookerId,
                ActiveBrandCount = @ActiveBrandCount,
                AllowsBrandAutonomy = @AllowsBrandAutonomy,
                CentralizationLevel = @CentralizationLevel,
                IsActive = @IsActive,
                LastModifiedAt = @LastModifiedAt
            WHERE HierarchyId = @HierarchyId";

        command.Parameters.AddWithValue("@HierarchyId", hierarchy.HierarchyId);
        command.Parameters.AddWithValue("@Type", hierarchy.Type.ToString());
        command.Parameters.AddWithValue("@OwnerId", hierarchy.OwnerId);
        command.Parameters.AddWithValue("@HeadBookerId", hierarchy.HeadBookerId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ActiveBrandCount", hierarchy.ActiveBrandCount);
        command.Parameters.AddWithValue("@AllowsBrandAutonomy", hierarchy.AllowsBrandAutonomy ? 1 : 0);
        command.Parameters.AddWithValue("@CentralizationLevel", hierarchy.CentralizationLevel);
        command.Parameters.AddWithValue("@IsActive", hierarchy.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@LastModifiedAt", DateTime.Now.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task AssignHeadBookerAsync(string companyId, string headBookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CompanyHierarchies
            SET HeadBookerId = @HeadBookerId, LastModifiedAt = @LastModifiedAt
            WHERE CompanyId = @CompanyId";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@HeadBookerId", headBookerId);
        command.Parameters.AddWithValue("@LastModifiedAt", DateTime.Now.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveHeadBookerAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CompanyHierarchies
            SET HeadBookerId = NULL, LastModifiedAt = @LastModifiedAt
            WHERE CompanyId = @CompanyId";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@LastModifiedAt", DateTime.Now.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task ConvertToMultiBrandAsync(string companyId, string headBookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CompanyHierarchies
            SET Type = @Type, HeadBookerId = @HeadBookerId, LastModifiedAt = @LastModifiedAt
            WHERE CompanyId = @CompanyId";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Type", CompanyHierarchyType.MultiBrand.ToString());
        command.Parameters.AddWithValue("@HeadBookerId", headBookerId);
        command.Parameters.AddWithValue("@LastModifiedAt", DateTime.Now.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    public async Task<int> CountActiveBrandsAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Brands WHERE CompanyId = @CompanyId AND IsActive = 1";
        command.Parameters.AddWithValue("@CompanyId", companyId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<Brand>> GetBrandsWithoutBookerAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BrandId, CompanyId, Name, Objective, BookerId, CurrentEraId, Prestige,
                   BudgetPerShow, AverageAudience, Priority, IsActive, AirDay, ShowDuration,
                   TargetRegion, CreatedAt, DeactivatedAt
            FROM Brands
            WHERE CompanyId = @CompanyId AND IsActive = 1 AND BookerId IS NULL
            ORDER BY Priority";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var brands = new List<Brand>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            brands.Add(MapBrand(reader));
        }

        return brands;
    }

    public async Task<List<Brand>> GetBrandsByObjectiveAsync(string companyId, string objective)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BrandId, CompanyId, Name, Objective, BookerId, CurrentEraId, Prestige,
                   BudgetPerShow, AverageAudience, Priority, IsActive, AirDay, ShowDuration,
                   TargetRegion, CreatedAt, DeactivatedAt
            FROM Brands
            WHERE CompanyId = @CompanyId AND Objective = @Objective AND IsActive = 1
            ORDER BY Priority";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Objective", objective);

        var brands = new List<Brand>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            brands.Add(MapBrand(reader));
        }

        return brands;
    }

    public async Task<bool> IsMultiBrandAsync(string companyId)
    {
        var hierarchy = await GetHierarchyByCompanyIdAsync(companyId);
        return hierarchy?.Type == CompanyHierarchyType.MultiBrand;
    }

    public async Task<double> CalculateTotalBrandBudgetAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT SUM(BudgetPerShow) FROM Brands WHERE CompanyId = @CompanyId AND IsActive = 1";
        command.Parameters.AddWithValue("@CompanyId", companyId);

        var result = await command.ExecuteScalarAsync();
        return result == DBNull.Value ? 0.0 : Convert.ToDouble(result);
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private static Brand MapBrand(SqliteDataReader reader)
    {
        return new Brand
        {
            BrandId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            Name = reader.GetString(2),
            Objective = Enum.Parse<BrandObjective>(reader.GetString(3)),
            BookerId = reader.IsDBNull(4) ? null : reader.GetString(4),
            CurrentEraId = reader.IsDBNull(5) ? null : reader.GetString(5),
            Prestige = reader.GetInt32(6),
            BudgetPerShow = reader.GetDouble(7),
            AverageAudience = reader.GetInt32(8),
            Priority = reader.GetInt32(9),
            IsActive = reader.GetInt32(10) == 1,
            AirDay = reader.IsDBNull(11) ? null : reader.GetString(11),
            ShowDuration = reader.GetInt32(12),
            TargetRegion = reader.IsDBNull(13) ? null : reader.GetString(13),
            CreatedAt = DateTime.Parse(reader.GetString(14)),
            DeactivatedAt = reader.IsDBNull(15) ? null : DateTime.Parse(reader.GetString(15))
        };
    }

    private static CompanyHierarchy MapHierarchy(SqliteDataReader reader)
    {
        return new CompanyHierarchy
        {
            HierarchyId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            Type = Enum.Parse<CompanyHierarchyType>(reader.GetString(2)),
            OwnerId = reader.GetString(3),
            HeadBookerId = reader.IsDBNull(4) ? null : reader.GetString(4),
            ActiveBrandCount = reader.GetInt32(5),
            AllowsBrandAutonomy = reader.GetInt32(6) == 1,
            CentralizationLevel = reader.GetInt32(7),
            IsActive = reader.GetInt32(8) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(9)),
            LastModifiedAt = DateTime.Parse(reader.GetString(10))
        };
    }
}
