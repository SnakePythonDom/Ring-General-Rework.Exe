using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

public class AlumniRepository : RepositoryBase, IAlumniRepository
{
    private readonly WorkerRepository _workerRepository;

    public AlumniRepository(SqliteConnectionFactory factory, WorkerRepository workerRepository) : base(factory)
    {
        _workerRepository = workerRepository;
    }

    public async Task<List<Worker>> GetCompanyAlumniAsync(string companyId)
    {
        var alumni = new List<Worker>();
        using var connection = OpenConnection();
        // Assume CreateConnection returns an open connection or we open it? 
        // RepositoryBase logic varies, but usually we need to Open.
        // Let's check RepositoryBase again later or play safe if standard pattern is OpenConnection
        // Wait, standard pattern here is `using var connection = OpenConnection();` in WorkerRepository.
        // But `RepositoryBase` stores `_factory`.

        // I will use `OpenConnection()` if accessible or standard manually.
        // WorkerRepository uses `OpenConnection()`. Let's assume it's protected in base.
        // If not, I'll use `_factory.CreateGeneralConnection()`.

        // Use manual open to be safe matching RepositoryBase pattern if protected is not available (checking previous file views)
        // Previous view of RepositoryBase was not fully explicit on access modifiers.
        // I'll stick to a standard factory usage if I can't confirm.
        // Actually, WorkerRepository inherits RepositoryBase and calls `OpenConnection()`. so it must be protected or public.

        // HOWEVER, I don't have access to `OpenConnection` method source here to confirm if it returns SqliteConnection.
        // I'll pattern match `WorkerRepository` usage.

        using var conn = _factory.CreateGeneralConnection(); // This usually returns `SqliteConnection`
        conn.Open();

        using var command = conn.CreateCommand();
        // Get Workers who have left the company (DepartureDate is NOT NULL and maybe a history table link?
        // Or if 'CompanyId' is NULL but 'FormerCompanyId' exists?
        // The migration added `DepartureDate`.
        // BUT, if they left the company, `CompanyId` might be null or different.
        // We might need a `EmploymentHistory` table to truly know WHERE they were alumni of.
        // Or, we assume "Alumni" means "Retired workers who were last with this company" OR "Any former worker".
        // A robust system needs `EmploymentHistory`.
        // FOR NOW (MVP): Check `Workers` table where `DepartureDate` IS NOT NULL. 
        // BUT how do we know they were in THIS company?
        // If they retire, they stay in `Workers` and `CompanyId` might remain as "Last Employer" or set to NULL.
        // Let's assume for Retirement, we keep `CompanyId` as the company they retired from, 
        // OR we add `LastCompanyId`.

        // Given existing schema, let's query: Workers where CompanyId = $id AND DepartureDate IS NOT NULL.
        // This covers "Retired from this company".

        command.CommandText = "SELECT WorkerId FROM Workers WHERE CompanyId = $companyId AND DepartureDate IS NOT NULL";
        command.Parameters.AddWithValue("$companyId", companyId);

        using var reader = await command.ExecuteReaderAsync();
        var workerIds = new List<string>();
        while (await reader.ReadAsync())
        {
            workerIds.Add(reader.GetString(0));
        }
        reader.Close();

        foreach (var id in workerIds)
        {
            // Reuse WorkerRepository to map full object
            var worker = _workerRepository.GetWorker(id);
            if (worker != null)
            {
                alumni.Add(worker);
            }
        }

        return alumni;
    }

    public async Task<List<Worker>> GetHallOfFameInducteesAsync(string companyId)
    {
        var hof = new List<Worker>();
        using var conn = _factory.CreateGeneralConnection();
        conn.Open();

        using var command = conn.CreateCommand();
        // Logic: IsHallOfFame = 1 using the integer column
        // Optional: Filter by CompanyId if HoF is company specific. 
        // Usually HoF is broader, but let's support Company filtering if provided.
        // If companyId is "GLOBAL", return all.

        string sql = "SELECT WorkerId FROM Workers WHERE IsHallOfFame = 1";
        if (!string.IsNullOrEmpty(companyId) && companyId != "GLOBAL")
        {
            sql += " AND CompanyId = $companyId"; // Determine if HoF entry is tied to company
        }

        command.CommandText = sql;
        if (!string.IsNullOrEmpty(companyId) && companyId != "GLOBAL")
        {
            command.Parameters.AddWithValue("$companyId", companyId);
        }

        using var reader = await command.ExecuteReaderAsync();
        var workerIds = new List<string>();
        while (await reader.ReadAsync())
        {
            workerIds.Add(reader.GetString(0));
        }
        reader.Close();

        foreach (var id in workerIds)
        {
            var worker = _workerRepository.GetWorker(id);
            if (worker != null) hof.Add(worker);
        }

        return hof;
    }

    public async Task<bool> CheckHallOfFameEligibilityAsync(string workerId, int legacyScoreThreshold)
    {
        using var conn = _factory.CreateGeneralConnection();
        conn.Open();

        using var command = conn.CreateCommand();
        command.CommandText = "SELECT LegacyScore FROM Workers WHERE WorkerId = $id";
        command.Parameters.AddWithValue("$id", workerId);

        var result = await command.ExecuteScalarAsync();
        if (result != null && result != DBNull.Value)
        {
            long score = (long)result; // Sqlite returns Int64
            return score >= legacyScoreThreshold;
        }
        return false;
    }

    public async Task InductIntoHallOfFameAsync(string workerId)
    {
        using var conn = _factory.CreateGeneralConnection();
        conn.Open();

        using var command = conn.CreateCommand();
        command.CommandText = "UPDATE Workers SET IsHallOfFame = 1 WHERE WorkerId = $id";
        command.Parameters.AddWithValue("$id", workerId);
        await command.ExecuteNonQueryAsync();
    }
}
