using Microsoft.Data.Sqlite;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Trends;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des tendances
/// </summary>
public class TrendRepository : RepositoryBase, ITrendRepository
{
    public TrendRepository(SqliteConnectionFactory factory)
        : base(factory)
    {
    }

    public async Task SaveTrendAsync(Trend trend)
    {
        await Task.Run(() =>
        {
            if (!trend.IsValid(out var errorMessage))
            {
                throw new ArgumentException($"Trend invalide: {errorMessage}");
            }

            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO Trends (
                    TrendId, Name, Type, Category, Description,
                    HardcoreAffinity, TechnicalAffinity, LuchaAffinity,
                    EntertainmentAffinity, StrongStyleAffinity,
                    StartDate, EndDate, Intensity, DurationWeeks,
                    MarketPenetration, AffectedRegions, IsActive, CreatedAt
                ) VALUES (
                    $trendId, $name, $type, $category, $description,
                    $hardcoreAffinity, $technicalAffinity, $luchaAffinity,
                    $entertainmentAffinity, $strongStyleAffinity,
                    $startDate, $endDate, $intensity, $durationWeeks,
                    $marketPenetration, $affectedRegions, $isActive, $createdAt
                )
                ON CONFLICT(TrendId) DO UPDATE SET
                    Name = $name, Type = $type, Category = $category, Description = $description,
                    HardcoreAffinity = $hardcoreAffinity, TechnicalAffinity = $technicalAffinity,
                    LuchaAffinity = $luchaAffinity, EntertainmentAffinity = $entertainmentAffinity,
                    StrongStyleAffinity = $strongStyleAffinity,
                    StartDate = $startDate, EndDate = $endDate, Intensity = $intensity,
                    DurationWeeks = $durationWeeks, MarketPenetration = $marketPenetration,
                    AffectedRegions = $affectedRegions, IsActive = $isActive;";

            AjouterParametre(command, "$trendId", trend.TrendId);
            AjouterParametre(command, "$name", trend.Name);
            AjouterParametre(command, "$type", trend.Type.ToString());
            AjouterParametre(command, "$category", trend.Category.ToString());
            AjouterParametre(command, "$description", trend.Description);
            AjouterParametre(command, "$hardcoreAffinity", trend.HardcoreAffinity);
            AjouterParametre(command, "$technicalAffinity", trend.TechnicalAffinity);
            AjouterParametre(command, "$luchaAffinity", trend.LuchaAffinity);
            AjouterParametre(command, "$entertainmentAffinity", trend.EntertainmentAffinity);
            AjouterParametre(command, "$strongStyleAffinity", trend.StrongStyleAffinity);
            AjouterParametre(command, "$startDate", trend.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
            AjouterParametre(command, "$endDate", trend.EndDate?.ToString("yyyy-MM-dd HH:mm:ss"));
            AjouterParametre(command, "$intensity", trend.Intensity);
            AjouterParametre(command, "$durationWeeks", trend.DurationWeeks);
            AjouterParametre(command, "$marketPenetration", trend.MarketPenetration);
            AjouterParametre(command, "$affectedRegions", JsonSerializer.Serialize(trend.AffectedRegions));
            AjouterParametre(command, "$isActive", trend.IsActive ? 1 : 0);
            AjouterParametre(command, "$createdAt", trend.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        });
    }

    public async Task<Trend?> GetTrendByIdAsync(string trendId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT TrendId, Name, Type, Category, Description,
                       HardcoreAffinity, TechnicalAffinity, LuchaAffinity,
                       EntertainmentAffinity, StrongStyleAffinity,
                       StartDate, EndDate, Intensity, DurationWeeks,
                       MarketPenetration, AffectedRegions, IsActive, CreatedAt
                FROM Trends
                WHERE TrendId = $trendId
                LIMIT 1;";

            AjouterParametre(command, "$trendId", trendId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapTrend(reader);
            }

            return null;
        });
    }

    public async Task<IReadOnlyList<Trend>> GetActiveTrendsAsync(string? region = null)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            var whereClause = "WHERE IsActive = 1";
            if (!string.IsNullOrWhiteSpace(region))
            {
                whereClause += " AND (Type = 'Global' OR AffectedRegions LIKE $region)";
            }

            command.CommandText = $@"
                SELECT TrendId, Name, Type, Category, Description,
                       HardcoreAffinity, TechnicalAffinity, LuchaAffinity,
                       EntertainmentAffinity, StrongStyleAffinity,
                       StartDate, EndDate, Intensity, DurationWeeks,
                       MarketPenetration, AffectedRegions, IsActive, CreatedAt
                FROM Trends
                {whereClause}
                ORDER BY StartDate DESC;";

            if (!string.IsNullOrWhiteSpace(region))
            {
                AjouterParametre(command, "$region", $"%{region}%");
            }

            using var reader = command.ExecuteReader();
            var results = new List<Trend>();

            while (reader.Read())
            {
                results.Add(MapTrend(reader));
            }

            return results;
        });
    }

    public async Task<IReadOnlyList<Trend>> GetTrendsByTypeAsync(string type)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT TrendId, Name, Type, Category, Description,
                       HardcoreAffinity, TechnicalAffinity, LuchaAffinity,
                       EntertainmentAffinity, StrongStyleAffinity,
                       StartDate, EndDate, Intensity, DurationWeeks,
                       MarketPenetration, AffectedRegions, IsActive, CreatedAt
                FROM Trends
                WHERE Type = $type
                ORDER BY StartDate DESC;";

            AjouterParametre(command, "$type", type);

            using var reader = command.ExecuteReader();
            var results = new List<Trend>();

            while (reader.Read())
            {
                results.Add(MapTrend(reader));
            }

            return results;
        });
    }

    public async Task<IReadOnlyList<Trend>> GetTrendsByCategoryAsync(string category)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT TrendId, Name, Type, Category, Description,
                       HardcoreAffinity, TechnicalAffinity, LuchaAffinity,
                       EntertainmentAffinity, StrongStyleAffinity,
                       StartDate, EndDate, Intensity, DurationWeeks,
                       MarketPenetration, AffectedRegions, IsActive, CreatedAt
                FROM Trends
                WHERE Category = $category
                ORDER BY StartDate DESC;";

            AjouterParametre(command, "$category", category);

            using var reader = command.ExecuteReader();
            var results = new List<Trend>();

            while (reader.Read())
            {
                results.Add(MapTrend(reader));
            }

            return results;
        });
    }

    public async Task UpdateTrendAsync(Trend trend)
    {
        await SaveTrendAsync(trend); // Utilise l'upsert
    }

    public async Task DeleteTrendAsync(string trendId)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = "UPDATE Trends SET IsActive = 0 WHERE TrendId = $trendId;";
            AjouterParametre(command, "$trendId", trendId);
            command.ExecuteNonQuery();
        });
    }

    // ====================================================================
    // COMPATIBILITY MATRIX OPERATIONS
    // ====================================================================

    public async Task SaveCompatibilityMatrixAsync(CompatibilityMatrix matrix)
    {
        await Task.Run(() =>
        {
            if (!matrix.IsValid(out var errorMessage))
            {
                throw new ArgumentException($"CompatibilityMatrix invalide: {errorMessage}");
            }

            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO CompatibilityMatrices (
                    MatrixId, CompanyId, TrendId, CompatibilityCoefficient, Level,
                    QualityBonus, GrowthMultiplier, NicheLoyaltyBonus,
                    MarketingCostReduction, CalculatedAt
                ) VALUES (
                    $matrixId, $companyId, $trendId, $compatibilityCoefficient, $level,
                    $qualityBonus, $growthMultiplier, $nicheLoyaltyBonus,
                    $marketingCostReduction, $calculatedAt
                )
                ON CONFLICT(CompanyId, TrendId) DO UPDATE SET
                    CompatibilityCoefficient = $compatibilityCoefficient,
                    Level = $level,
                    QualityBonus = $qualityBonus,
                    GrowthMultiplier = $growthMultiplier,
                    NicheLoyaltyBonus = $nicheLoyaltyBonus,
                    MarketingCostReduction = $marketingCostReduction,
                    CalculatedAt = $calculatedAt;";

            AjouterParametre(command, "$matrixId", matrix.MatrixId);
            AjouterParametre(command, "$companyId", matrix.CompanyId);
            AjouterParametre(command, "$trendId", matrix.TrendId);
            AjouterParametre(command, "$compatibilityCoefficient", matrix.CompatibilityCoefficient);
            AjouterParametre(command, "$level", matrix.Level.ToString());
            AjouterParametre(command, "$qualityBonus", matrix.QualityBonus);
            AjouterParametre(command, "$growthMultiplier", matrix.GrowthMultiplier);
            AjouterParametre(command, "$nicheLoyaltyBonus", matrix.NicheLoyaltyBonus);
            AjouterParametre(command, "$marketingCostReduction", matrix.MarketingCostReduction);
            AjouterParametre(command, "$calculatedAt", matrix.CalculatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        });
    }

    public async Task<CompatibilityMatrix?> GetCompatibilityMatrixAsync(string companyId, string trendId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT MatrixId, CompanyId, TrendId, CompatibilityCoefficient, Level,
                       QualityBonus, GrowthMultiplier, NicheLoyaltyBonus,
                       MarketingCostReduction, CalculatedAt
                FROM CompatibilityMatrices
                WHERE CompanyId = $companyId AND TrendId = $trendId
                LIMIT 1;";

            AjouterParametre(command, "$companyId", companyId);
            AjouterParametre(command, "$trendId", trendId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapCompatibilityMatrix(reader);
            }

            return null;
        });
    }

    public async Task<IReadOnlyList<CompatibilityMatrix>> GetCompatibilityMatricesByCompanyIdAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT MatrixId, CompanyId, TrendId, CompatibilityCoefficient, Level,
                       QualityBonus, GrowthMultiplier, NicheLoyaltyBonus,
                       MarketingCostReduction, CalculatedAt
                FROM CompatibilityMatrices
                WHERE CompanyId = $companyId
                ORDER BY CalculatedAt DESC;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            var results = new List<CompatibilityMatrix>();

            while (reader.Read())
            {
                results.Add(MapCompatibilityMatrix(reader));
            }

            return results;
        });
    }

    public async Task<IReadOnlyList<CompatibilityMatrix>> GetCompatibilityMatricesByTrendIdAsync(string trendId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT MatrixId, CompanyId, TrendId, CompatibilityCoefficient, Level,
                       QualityBonus, GrowthMultiplier, NicheLoyaltyBonus,
                       MarketingCostReduction, CalculatedAt
                FROM CompatibilityMatrices
                WHERE TrendId = $trendId
                ORDER BY CalculatedAt DESC;";

            AjouterParametre(command, "$trendId", trendId);

            using var reader = command.ExecuteReader();
            var results = new List<CompatibilityMatrix>();

            while (reader.Read())
            {
                results.Add(MapCompatibilityMatrix(reader));
            }

            return results;
        });
    }

    public async Task UpdateCompatibilityMatrixAsync(CompatibilityMatrix matrix)
    {
        await SaveCompatibilityMatrixAsync(matrix); // Utilise l'upsert
    }

    public async Task DeleteCompatibilityMatrixAsync(string matrixId)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = "DELETE FROM CompatibilityMatrices WHERE MatrixId = $matrixId;";
            AjouterParametre(command, "$matrixId", matrixId);
            command.ExecuteNonQuery();
        });
    }

    // ====================================================================
    // MAPPING METHODS
    // ====================================================================

    private static Trend MapTrend(SqliteDataReader reader)
    {
        var affectedRegionsJson = reader.GetString(15);
        var affectedRegions = JsonSerializer.Deserialize<string[]>(affectedRegionsJson) ?? Array.Empty<string>();

        return new Trend
        {
            TrendId = reader.GetString(0),
            Name = reader.GetString(1),
            Type = Enum.Parse<TrendType>(reader.GetString(2)),
            Category = Enum.Parse<TrendCategory>(reader.GetString(3)),
            Description = reader.GetString(4),
            HardcoreAffinity = reader.GetDouble(5),
            TechnicalAffinity = reader.GetDouble(6),
            LuchaAffinity = reader.GetDouble(7),
            EntertainmentAffinity = reader.GetDouble(8),
            StrongStyleAffinity = reader.GetDouble(9),
            StartDate = DateTime.Parse(reader.GetString(10)),
            EndDate = reader.IsDBNull(11) ? null : DateTime.Parse(reader.GetString(11)),
            Intensity = reader.GetInt32(12),
            DurationWeeks = reader.GetInt32(13),
            MarketPenetration = reader.GetDouble(14),
            AffectedRegions = string.Join(",", affectedRegions),
            IsActive = reader.GetInt32(16) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(17))
        };
    }

    private static CompatibilityMatrix MapCompatibilityMatrix(SqliteDataReader reader)
    {
        return new CompatibilityMatrix
        {
            MatrixId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            TrendId = reader.GetString(2),
            CompatibilityCoefficient = reader.GetDouble(3),
            Level = Enum.Parse<CompatibilityLevel>(reader.GetString(4)),
            QualityBonus = reader.GetDouble(5),
            GrowthMultiplier = reader.GetDouble(6),
            NicheLoyaltyBonus = reader.GetDouble(7),
            MarketingCostReduction = reader.GetDouble(8),
            CalculatedAt = DateTime.Parse(reader.GetString(9))
        };
    }
}
