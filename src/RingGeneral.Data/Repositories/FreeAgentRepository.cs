using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Recruitment;
using RingGeneral.Data.Database;
using System.Text;

namespace RingGeneral.Data.Repositories;

public sealed class FreeAgentRepository : RepositoryBase, IFreeAgentRepository
{
    public FreeAgentRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public async Task<List<FreeAgentCandidate>> GetFreeAgentMarketAsync(FreeAgentFilter filter)
    {
        var candidates = new List<FreeAgentCandidate>();
        using var connection = await OpenConnectionAsync();

        // 1. Fetch Workers (Wrestlers)
        if (filter.Type == null || filter.Type == FreeAgentType.Wrestler)
        {
            candidates.AddRange(await FetchWrestlerCandidates(connection, filter));
        }

        // 2. Fetch Staff
        if (filter.Type == null || filter.Type == FreeAgentType.Staff)
        {
            candidates.AddRange(await FetchStaffCandidates(connection, filter));
        }

        // 3. Post-fetch sorting (since we combined sources)
        return ApplySorting(candidates, filter);
    }

    public async Task<int> CountFreeAgentsAsync(FreeAgentFilter filter)
    {
        using var connection = await OpenConnectionAsync();
        int count = 0;

        if (filter.Type == null || filter.Type == FreeAgentType.Wrestler)
        {
            count += await CountWrestlerCandidates(connection, filter);
        }

        if (filter.Type == null || filter.Type == FreeAgentType.Staff)
        {
            count += await CountStaffCandidates(connection, filter);
        }

        return count;
    }

    private async Task<List<FreeAgentCandidate>> FetchWrestlerCandidates(SqliteConnection conn, FreeAgentFilter filter)
    {
        var sql = new StringBuilder(@"
            SELECT WorkerId, Name, Age, Nationality, Gender, Popularity, InRing, Entertainment, Story, RoleTv, Region
            FROM Workers
            WHERE (CompanyId IS NULL OR CompanyId = '') AND IsActive = 1");

        ApplyWrestlerFilters(sql, filter);

        using var command = conn.CreateCommand();
        command.CommandText = sql.ToString();
        AddFilterParameters(command, filter);

        var list = new List<FreeAgentCandidate>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new FreeAgentCandidate
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Type = FreeAgentType.Wrestler,
                Age = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                Nationality = reader.IsDBNull(3) ? null : reader.GetString(3),
                Gender = reader.IsDBNull(4) ? null : reader.GetString(4),
                Popularity = reader.GetInt32(5),
                PrimarySkill = reader.GetInt32(6),
                SecondarySkill = reader.GetInt32(7),
                TertiarySkill = reader.GetInt32(8),
                RoleDisplay = reader.IsDBNull(9) ? "Wrestler" : reader.GetString(9),
                Region = reader.IsDBNull(10) ? null : reader.GetString(10)
            });
        }
        return list;
    }

    private async Task<List<FreeAgentCandidate>> FetchStaffCandidates(SqliteConnection conn, FreeAgentFilter filter)
    {
        var sql = new StringBuilder(@"
            SELECT StaffId, Name, SkillScore, PersonalityScore, Role, Department, ExpertiseLevel, AnnualSalary
            FROM StaffMembers
            WHERE CompanyId = 'FREE_AGENT' AND IsActive = 1");

        ApplyStaffFilters(sql, filter);

        using var command = conn.CreateCommand();
        command.CommandText = sql.ToString();
        AddFilterParameters(command, filter);

        var list = new List<FreeAgentCandidate>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new FreeAgentCandidate
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Type = FreeAgentType.Staff,
                Age = 0, // StaffMember doesn't currently track Age explicitly, could be added later
                Nationality = "Unknown",
                Popularity = 0, // Staff popularity not yet implemented
                PrimarySkill = reader.GetInt32(2),
                SecondarySkill = reader.GetInt32(3),
                RoleDisplay = $"{reader.GetString(4)} ({reader.GetString(6)})",
                WeeklySalaryDemand = (decimal)(reader.GetDouble(7) / 52.0),
                Specialization = reader.GetString(5)
            });
        }
        return list;
    }

    private void ApplyWrestlerFilters(StringBuilder sql, FreeAgentFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            sql.Append(" AND (Name LIKE @Search OR WorkerId LIKE @Search)");
        if (filter.MinAge.HasValue)
            sql.Append(" AND Age >= @MinAge");
        if (filter.MaxAge.HasValue)
            sql.Append(" AND Age <= @MaxAge");
        if (!string.IsNullOrWhiteSpace(filter.Region))
            sql.Append(" AND Region = @Region");
    }

    private void ApplyStaffFilters(StringBuilder sql, FreeAgentFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            sql.Append(" AND (Name LIKE @Search OR StaffId LIKE @Search)");
        if (!string.IsNullOrWhiteSpace(filter.Role))
            sql.Append(" AND Role = @Role");
    }

    private void AddFilterParameters(SqliteCommand cmd, FreeAgentFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            cmd.Parameters.AddWithValue("@Search", $"%{filter.SearchText}%");
        if (filter.MinAge.HasValue)
            cmd.Parameters.AddWithValue("@MinAge", filter.MinAge.Value);
        if (filter.MaxAge.HasValue)
            cmd.Parameters.AddWithValue("@MaxAge", filter.MaxAge.Value);
        if (!string.IsNullOrWhiteSpace(filter.Region))
            cmd.Parameters.AddWithValue("@Region", filter.Region);
        if (!string.IsNullOrWhiteSpace(filter.Role))
            cmd.Parameters.AddWithValue("@Role", filter.Role);
    }

    private async Task<int> CountWrestlerCandidates(SqliteConnection conn, FreeAgentFilter filter)
    {
        var sql = new StringBuilder("SELECT COUNT(*) FROM Workers WHERE (CompanyId IS NULL OR CompanyId = '') AND IsActive = 1");
        ApplyWrestlerFilters(sql, filter);
        using var command = conn.CreateCommand();
        command.CommandText = sql.ToString();
        AddFilterParameters(command, filter);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task<int> CountStaffCandidates(SqliteConnection conn, FreeAgentFilter filter)
    {
        var sql = new StringBuilder("SELECT COUNT(*) FROM StaffMembers WHERE CompanyId = 'FREE_AGENT' AND IsActive = 1");
        ApplyStaffFilters(sql, filter);
        using var command = conn.CreateCommand();
        command.CommandText = sql.ToString();
        AddFilterParameters(command, filter);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private List<FreeAgentCandidate> ApplySorting(List<FreeAgentCandidate> list, FreeAgentFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SortBy)) return list;

        IEnumerable<FreeAgentCandidate> query = filter.SortBy.ToLowerInvariant() switch
        {
            "popularity" => filter.SortDescending ? list.OrderByDescending(c => c.Popularity) : list.OrderBy(c => c.Popularity),
            "salary" => filter.SortDescending ? list.OrderByDescending(c => c.WeeklySalaryDemand ?? 0) : list.OrderBy(c => c.WeeklySalaryDemand ?? 0),
            "skill" => filter.SortDescending ? list.OrderByDescending(c => c.PrimarySkill) : list.OrderBy(c => c.PrimarySkill),
            "name" => filter.SortDescending ? list.OrderByDescending(c => c.Name) : list.OrderBy(c => c.Name),
            "age" => filter.SortDescending ? list.OrderByDescending(c => c.Age) : list.OrderBy(c => c.Age),
            _ => list
        };

        return query.ToList();
    }
}
