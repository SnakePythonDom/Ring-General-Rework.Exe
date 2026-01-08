using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Morale;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository pour la gestion du moral backstage et de la compagnie.
/// </summary>
public class MoraleRepository : RepositoryBase, IMoraleRepository
{
    public MoraleRepository(SqliteConnectionFactory factory)
        : base(factory)
    {
    }

    // === BackstageMorale ===

    public async Task<BackstageMorale?> GetBackstageMoraleAsync(string entityId, string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, EntityId, EntityType, CompanyId,
                       MoraleScore, PreviousMoraleScore, LastUpdated
                FROM BackstageMorale
                WHERE EntityId = $entityId AND CompanyId = $companyId
                LIMIT 1;";

            AjouterParametre(command, "$entityId", entityId);
            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapBackstageMorale(reader);
            }

            return null;
        });
    }

    public async Task SaveBackstageMoraleAsync(BackstageMorale morale)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            // Upsert (INSERT OR REPLACE)
            command.CommandText = @"
                INSERT INTO BackstageMorale (
                    EntityId, EntityType, CompanyId,
                    MoraleScore, PreviousMoraleScore, LastUpdated
                ) VALUES (
                    $entityId, $entityType, $companyId,
                    $moraleScore, $previousMoraleScore, $lastUpdated
                )
                ON CONFLICT(EntityId, CompanyId) DO UPDATE SET
                    EntityType = $entityType,
                    MoraleScore = $moraleScore,
                    PreviousMoraleScore = $previousMoraleScore,
                    LastUpdated = $lastUpdated;";

            AjouterParametre(command, "$entityId", morale.EntityId);
            AjouterParametre(command, "$entityType", morale.EntityType);
            AjouterParametre(command, "$companyId", morale.CompanyId);
            AjouterParametre(command, "$moraleScore", morale.MoraleScore);
            AjouterParametre(command, "$previousMoraleScore", morale.PreviousMoraleScore);
            AjouterParametre(command, "$lastUpdated", morale.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        });
    }

    public async Task<List<BackstageMorale>> GetLowMoraleEntitiesAsync(string companyId, int threshold = 40)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, EntityId, EntityType, CompanyId,
                       MoraleScore, PreviousMoraleScore, LastUpdated
                FROM BackstageMorale
                WHERE CompanyId = $companyId
                  AND MoraleScore < $threshold
                ORDER BY MoraleScore ASC;";

            AjouterParametre(command, "$companyId", companyId);
            AjouterParametre(command, "$threshold", threshold);

            using var reader = command.ExecuteReader();
            var results = new List<BackstageMorale>();

            while (reader.Read())
            {
                results.Add(MapBackstageMorale(reader));
            }

            return results;
        });
    }

    public async Task<List<BackstageMorale>> GetCriticalMoraleEntitiesAsync(string companyId, int threshold = 20)
    {
        return await GetLowMoraleEntitiesAsync(companyId, threshold);
    }

    public async Task<List<BackstageMorale>> GetAllBackstageMoraleAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, EntityId, EntityType, CompanyId,
                       MoraleScore, PreviousMoraleScore, LastUpdated
                FROM BackstageMorale
                WHERE CompanyId = $companyId
                ORDER BY MoraleScore ASC;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            var results = new List<BackstageMorale>();

            while (reader.Read())
            {
                results.Add(MapBackstageMorale(reader));
            }

            return results;
        });
    }

    // === CompanyMorale ===

    public async Task<CompanyMorale?> GetCompanyMoraleAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, CompanyId, GlobalMoraleScore,
                       WorkersMoraleAvg, StaffMoraleAvg, Trend, LastUpdated
                FROM CompanyMorale
                WHERE CompanyId = $companyId
                LIMIT 1;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapCompanyMorale(reader);
            }

            return null;
        });
    }

    public async Task SaveCompanyMoraleAsync(CompanyMorale morale)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            // Upsert (INSERT OR REPLACE)
            command.CommandText = @"
                INSERT INTO CompanyMorale (
                    CompanyId, GlobalMoraleScore, WorkersMoraleAvg,
                    StaffMoraleAvg, Trend, LastUpdated
                ) VALUES (
                    $companyId, $globalMoraleScore, $workersMoraleAvg,
                    $staffMoraleAvg, $trend, $lastUpdated
                )
                ON CONFLICT(CompanyId) DO UPDATE SET
                    GlobalMoraleScore = $globalMoraleScore,
                    WorkersMoraleAvg = $workersMoraleAvg,
                    StaffMoraleAvg = $staffMoraleAvg,
                    Trend = $trend,
                    LastUpdated = $lastUpdated;";

            AjouterParametre(command, "$companyId", morale.CompanyId);
            AjouterParametre(command, "$globalMoraleScore", morale.GlobalMoraleScore);
            AjouterParametre(command, "$workersMoraleAvg", morale.WorkersMoraleAvg);
            AjouterParametre(command, "$staffMoraleAvg", morale.StaffMoraleAvg);
            AjouterParametre(command, "$trend", morale.Trend);
            AjouterParametre(command, "$lastUpdated", morale.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        });
    }

    public async Task RecalculateCompanyMoraleAsync(string companyId)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();

            // Get all backstage morale entries for the company
            var allMorale = GetAllBackstageMoraleAsync(companyId).Result;

            if (!allMorale.Any())
            {
                // No data, create default company morale
                var defaultMorale = new CompanyMorale
                {
                    CompanyId = companyId,
                    GlobalMoraleScore = 70,
                    WorkersMoraleAvg = 70,
                    StaffMoraleAvg = 70,
                    Trend = "Stable",
                    LastUpdated = DateTime.Now
                };
                SaveCompanyMoraleAsync(defaultMorale).Wait();
                return;
            }

            // Calculate averages by entity type
            var workers = allMorale.Where(m => m.EntityType == "Worker").ToList();
            var staff = allMorale.Where(m => m.EntityType == "Staff").ToList();

            int workersMoraleAvg = workers.Any()
                ? (int)workers.Average(m => m.MoraleScore)
                : 70;

            int staffMoraleAvg = staff.Any()
                ? (int)staff.Average(m => m.MoraleScore)
                : 70;

            int globalMoraleScore = (int)allMorale.Average(m => m.MoraleScore);

            // Determine trend
            var existingMorale = GetCompanyMoraleAsync(companyId).Result;
            string trend = "Stable";

            if (existingMorale != null)
            {
                if (globalMoraleScore > existingMorale.GlobalMoraleScore + 5)
                    trend = "Improving";
                else if (globalMoraleScore < existingMorale.GlobalMoraleScore - 5)
                    trend = "Declining";
            }

            // Save updated company morale
            var companyMorale = new CompanyMorale
            {
                CompanyId = companyId,
                GlobalMoraleScore = globalMoraleScore,
                WorkersMoraleAvg = workersMoraleAvg,
                StaffMoraleAvg = staffMoraleAvg,
                Trend = trend,
                LastUpdated = DateTime.Now
            };

            SaveCompanyMoraleAsync(companyMorale).Wait();
        });
    }

    // === Mapping Methods ===

    private BackstageMorale MapBackstageMorale(SqliteDataReader reader)
    {
        return new BackstageMorale
        {
            Id = reader.GetInt32(0),
            EntityId = reader.GetString(1),
            EntityType = reader.GetString(2),
            CompanyId = reader.GetString(3),
            MoraleScore = reader.GetInt32(4),
            PreviousMoraleScore = reader.GetInt32(5),
            LastUpdated = DateTime.Parse(reader.GetString(6))
        };
    }

    private CompanyMorale MapCompanyMorale(SqliteDataReader reader)
    {
        return new CompanyMorale
        {
            Id = reader.GetInt32(0),
            CompanyId = reader.GetString(1),
            GlobalMoraleScore = reader.GetInt32(2),
            WorkersMoraleAvg = reader.GetInt32(3),
            StaffMoraleAvg = reader.GetInt32(4),
            Trend = reader.GetString(5),
            LastUpdated = DateTime.Parse(reader.GetString(6))
        };
    }
}
