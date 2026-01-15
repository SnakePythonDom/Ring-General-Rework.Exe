using Dapper;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository for gimmick management
/// </summary>
public class GimmickRepository : IGimmickRepository
{
    private readonly string _connectionString;

    public GimmickRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection GetConnection() => new SqliteConnection(_connectionString);

    // ====================================================================
    // GIMMICK QUERIES
    // ====================================================================

    public List<Gimmick> GetAllGimmicks()
    {
        using var conn = GetConnection();
        var sql = @"
            SELECT * FROM Gimmicks 
            WHERE IsActive = 1 
            ORDER BY Category, Name";
        return conn.Query<Gimmick>(sql).ToList();
    }

    public List<Gimmick> GetGimmicksByCategory(string category)
    {
        using var conn = GetConnection();
        var sql = @"
            SELECT * FROM Gimmicks 
            WHERE Category = @Category AND IsActive = 1 
            ORDER BY Name";
        return conn.Query<Gimmick>(sql, new { Category = category }).ToList();
    }

    public List<Gimmick> GetGimmicksByAlignment(string alignment)
    {
        using var conn = GetConnection();
        var sql = @"
            SELECT * FROM Gimmicks 
            WHERE (PreferredAlignment = @Alignment OR PreferredAlignment = 'Any') 
            AND IsActive = 1 
            ORDER BY Category, Name";
        return conn.Query<Gimmick>(sql, new { Alignment = alignment }).ToList();
    }

    public List<Gimmick> GetGimmicksByPopularityTier(string tier)
    {
        using var conn = GetConnection();
        var sql = @"
            SELECT * FROM Gimmicks 
            WHERE PopularityTier = @Tier AND IsActive = 1 
            ORDER BY Category, Name";
        return conn.Query<Gimmick>(sql, new { Tier = tier }).ToList();
    }

    public Gimmick? GetGimmickById(string gimmickId)
    {
        using var conn = GetConnection();
        var sql = "SELECT * FROM Gimmicks WHERE GimmickId = @GimmickId";
        return conn.QueryFirstOrDefault<Gimmick>(sql, new { GimmickId = gimmickId });
    }

    public List<Gimmick> SearchGimmicks(string searchTerm)
    {
        using var conn = GetConnection();
        var sql = @"
            SELECT * FROM Gimmicks 
            WHERE (Name LIKE @Search OR Description LIKE @Search) 
            AND IsActive = 1 
            ORDER BY Category, Name 
            LIMIT 50";
        return conn.Query<Gimmick>(sql, new { Search = $"%{searchTerm}%" }).ToList();
    }

    public List<Gimmick> GetRecommendedGimmicks(int workerId, int limit = 10)
    {
        using var conn = GetConnection();

        // Get worker's primary specialization and alignment
        var worker = conn.QueryFirstOrDefault<dynamic>(@"
            SELECT w.Alignment, w.PushLevel, ws.Specialization
            FROM Workers w
            LEFT JOIN WorkerSpecializations ws ON w.Id = ws.WorkerId AND ws.Level = 1
            WHERE w.Id = @WorkerId", new { WorkerId = workerId });

        if (worker == null) return new List<Gimmick>();

        // Map specialization to category
        string category = worker.Specialization switch
        {
            "Power" => "POWER",
            "Technical" => "TECHNICAL",
            "HighFlyer" => "HIGHFLYER",
            "Brawler" => "BRAWLER",
            "Showman" => "SHOWMAN",
            "Hardcore" => "HARDCORE",
            "AllRounder" => "ALLROUNDER",
            _ => "ALLROUNDER"
        };

        var sql = @"
            SELECT * FROM Gimmicks 
            WHERE Category = @Category 
            AND (PreferredAlignment = @Alignment OR PreferredAlignment = 'Any')
            AND (PopularityTier = @PushLevel OR PopularityTier = 'MidCard')
            AND IsActive = 1 
            ORDER BY RANDOM() 
            LIMIT @Limit";

        return conn.Query<Gimmick>(sql, new
        {
            Category = category,
            Alignment = worker.Alignment?.ToString() ?? "Any",
            PushLevel = worker.PushLevel?.ToString() ?? "MidCard",
            Limit = limit
        }).ToList();
    }

    // ====================================================================
    // GIMMICK CATEGORIES
    // ====================================================================

    public List<GimmickCategoryInfo> GetAllCategories()
    {
        using var conn = GetConnection();
        var sql = "SELECT * FROM GimmickCategories ORDER BY SortOrder";
        return conn.Query<GimmickCategoryInfo>(sql).ToList();
    }

    public GimmickCategoryInfo? GetCategoryById(string categoryId)
    {
        using var conn = GetConnection();
        var sql = "SELECT * FROM GimmickCategories WHERE CategoryId = @CategoryId";
        return conn.QueryFirstOrDefault<GimmickCategoryInfo>(sql, new { CategoryId = categoryId });
    }

    // ====================================================================
    // GIMMICK HISTORY
    // ====================================================================

    public List<GimmickHistory> GetWorkerGimmickHistory(int workerId)
    {
        using var conn = GetConnection();
        var sql = @"
            SELECT * FROM GimmickHistory 
            WHERE WorkerId = @WorkerId 
            ORDER BY StartDate DESC";
        return conn.Query<GimmickHistory>(sql, new { WorkerId = workerId }).ToList();
    }

    public GimmickHistory? GetCurrentGimmick(int workerId)
    {
        using var conn = GetConnection();
        var sql = @"
            SELECT * FROM GimmickHistory 
            WHERE WorkerId = @WorkerId AND EndDate IS NULL 
            ORDER BY StartDate DESC 
            LIMIT 1";
        return conn.QueryFirstOrDefault<GimmickHistory>(sql, new { WorkerId = workerId });
    }

    public void AssignGimmickToWorker(int workerId, string gimmickId, string reason)
    {
        using var conn = GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            // End current gimmick if exists
            var current = GetCurrentGimmick(workerId);
            if (current != null)
            {
                conn.Execute(@"
                    UPDATE GimmickHistory 
                    SET EndDate = datetime('now') 
                    WHERE HistoryId = @HistoryId",
                    new { current.HistoryId }, transaction);
            }

            // Get gimmick name
            var gimmick = GetGimmickById(gimmickId);
            if (gimmick == null)
            {
                transaction.Rollback();
                return;
            }

            // Insert new gimmick history
            conn.Execute(@"
                INSERT INTO GimmickHistory (WorkerId, GimmickId, GimmickName, StartDate, AdoptionReason)
                VALUES (@WorkerId, @GimmickId, @GimmickName, datetime('now'), @Reason)",
                new
                {
                    WorkerId = workerId,
                    GimmickId = gimmickId,
                    GimmickName = gimmick.Name,
                    Reason = reason
                }, transaction);

            // Update worker's CurrentGimmick field
            conn.Execute(@"
                UPDATE Workers 
                SET CurrentGimmick = @GimmickName 
                WHERE Id = @WorkerId",
                new { WorkerId = workerId, GimmickName = gimmick.Name }, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void AssignCustomGimmick(int workerId, string gimmickName, string reason)
    {
        using var conn = GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            // End current gimmick if exists
            var current = GetCurrentGimmick(workerId);
            if (current != null)
            {
                conn.Execute(@"
                    UPDATE GimmickHistory 
                    SET EndDate = datetime('now') 
                    WHERE HistoryId = @HistoryId",
                    new { current.HistoryId }, transaction);
            }

            // Insert new custom gimmick history
            conn.Execute(@"
                INSERT INTO GimmickHistory (WorkerId, GimmickName, StartDate, AdoptionReason)
                VALUES (@WorkerId, @GimmickName, datetime('now'), @Reason)",
                new { WorkerId = workerId, GimmickName = gimmickName, Reason = reason }, transaction);

            // Update worker's CurrentGimmick field
            conn.Execute(@"
                UPDATE Workers 
                SET CurrentGimmick = @GimmickName 
                WHERE Id = @WorkerId",
                new { WorkerId = workerId, GimmickName = gimmickName }, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void EndCurrentGimmick(int workerId, int successRating, string? notes = null)
    {
        using var conn = GetConnection();
        var sql = @"
            UPDATE GimmickHistory 
            SET EndDate = datetime('now'), 
                SuccessRating = @SuccessRating,
                Notes = @Notes
            WHERE WorkerId = @WorkerId AND EndDate IS NULL";
        conn.Execute(sql, new { WorkerId = workerId, SuccessRating = successRating, Notes = notes });
    }

    public void UpdateGimmickSuccessRating(int historyId, int successRating)
    {
        using var conn = GetConnection();
        var sql = @"
            UPDATE GimmickHistory 
            SET SuccessRating = @SuccessRating 
            WHERE HistoryId = @HistoryId";
        conn.Execute(sql, new { HistoryId = historyId, SuccessRating = successRating });
    }

    // ====================================================================
    // STATISTICS
    // ====================================================================

    public int GetTotalGimmickCount()
    {
        using var conn = GetConnection();
        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Gimmicks WHERE IsActive = 1");
    }

    public Dictionary<string, int> GetGimmickCountByCategory()
    {
        using var conn = GetConnection();
        var sql = @"
            SELECT Category, COUNT(*) as Count 
            FROM Gimmicks 
            WHERE IsActive = 1 
            GROUP BY Category";
        var results = conn.Query<(string Category, int Count)>(sql);
        return results.ToDictionary(x => x.Category, x => x.Count);
    }

    public List<(Gimmick Gimmick, int UsageCount)> GetMostPopularGimmicks(int limit = 10)
    {
        using var conn = GetConnection();
        var sql = @"
            SELECT g.*, COUNT(gh.HistoryId) as UsageCount
            FROM Gimmicks g
            INNER JOIN GimmickHistory gh ON g.GimmickId = gh.GimmickId
            WHERE g.IsActive = 1
            GROUP BY g.GimmickId
            ORDER BY UsageCount DESC
            LIMIT @Limit";

        var results = conn.Query<dynamic>(sql, new { Limit = limit });
        var list = new List<(Gimmick, int)>();

        foreach (var row in results)
        {
            var gimmick = new Gimmick
            {
                GimmickId = row.GimmickId,
                Name = row.Name,
                Description = row.Description,
                Category = row.Category,
                SubCategory = row.SubCategory,
                EntertainmentModifier = row.EntertainmentModifier,
                CrowdReactionModifier = row.CrowdReactionModifier,
                PreferredAlignment = row.PreferredAlignment,
                EraCompatibility = row.EraCompatibility,
                PopularityTier = row.PopularityTier,
                IsActive = row.IsActive == 1
            };
            list.Add((gimmick, (int)row.UsageCount));
        }

        return list;
    }

    public List<(Gimmick Gimmick, double AverageRating)> GetMostSuccessfulGimmicks(int limit = 10)
    {
        using var conn = GetConnection();
        var sql = @"
            SELECT g.*, AVG(gh.SuccessRating) as AvgRating
            FROM Gimmicks g
            INNER JOIN GimmickHistory gh ON g.GimmickId = gh.GimmickId
            WHERE g.IsActive = 1 AND gh.EndDate IS NOT NULL
            GROUP BY g.GimmickId
            HAVING COUNT(gh.HistoryId) >= 3
            ORDER BY AvgRating DESC
            LIMIT @Limit";

        var results = conn.Query<dynamic>(sql, new { Limit = limit });
        var list = new List<(Gimmick, double)>();

        foreach (var row in results)
        {
            var gimmick = new Gimmick
            {
                GimmickId = row.GimmickId,
                Name = row.Name,
                Description = row.Description,
                Category = row.Category,
                SubCategory = row.SubCategory,
                EntertainmentModifier = row.EntertainmentModifier,
                CrowdReactionModifier = row.CrowdReactionModifier,
                PreferredAlignment = row.PreferredAlignment,
                EraCompatibility = row.EraCompatibility,
                PopularityTier = row.PopularityTier,
                IsActive = row.IsActive == 1
            };
            list.Add((gimmick, (double)row.AvgRating));
        }

        return list;
    }
}
