using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests;

public sealed class ScoutingServiceTests
{
    [Fact]
    public void CreerRapport_PersisteEtEstVisible()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ringgeneral-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
            var repository = new GameRepository(factory);
            repository.Initialiser();

            InsererFreeAgent(factory, "FA-TEST-001", "Nina", "Libre", "FR");

            var service = new ScoutingService(repository, new SeededRandomProvider(10));
            var report = service.CreerRapport("FA-TEST-001", 3, "Note test.");

            var rapports = repository.ChargerScoutReports();
            Assert.Contains(rapports, item => item.ReportId == report.ReportId && item.WorkerId == "FA-TEST-001");

            var reload = new GameRepository(factory);
            var rapportsReload = reload.ChargerScoutReports();
            Assert.Contains(rapportsReload, item => item.ReportId == report.ReportId);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    [Fact]
    public void MissionProgresse_AuFilDesSemaines()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ringgeneral-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
            var repository = new GameRepository(factory);
            repository.Initialiser();

            var mission = new ScoutMission("MS-TEST-001", "Observer les free agents", "FR", "free_agents", 0, 10, "active", 1, 1);
            repository.AjouterMission(mission);

            var service = new ScoutingService(repository, new SeededRandomProvider(5));
            service.RafraichirHebdo(2);
            service.RafraichirHebdo(3);

            var missions = repository.ChargerScoutMissions();
            var updated = missions.Single(item => item.MissionId == "MS-TEST-001");

            Assert.True(updated.Progression > mission.Progression);
            Assert.Equal(3, updated.SemaineMaj);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    private static void InsererFreeAgent(SqliteConnectionFactory factory, string workerId, string prenom, string nom, string region)
    {
        using var connexion = factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();

        using var workerCommand = connexion.CreateCommand();
        workerCommand.Transaction = transaction;
        workerCommand.CommandText = """
            INSERT INTO workers (worker_id, nom, prenom, company_id, in_ring, entertainment, story, popularite, fatigue, blessure, momentum, role_tv, type_worker)
            VALUES ($workerId, $nom, $prenom, NULL, 62, 58, 61, 45, 0, 'AUCUNE', 0, 'LOWER_MID', 'CATCHEUR');
            """;
        workerCommand.Parameters.AddWithValue("$workerId", workerId);
        workerCommand.Parameters.AddWithValue("$nom", nom);
        workerCommand.Parameters.AddWithValue("$prenom", prenom);
        workerCommand.ExecuteNonQuery();

        using var regionCommand = connexion.CreateCommand();
        regionCommand.Transaction = transaction;
        regionCommand.CommandText = """
            INSERT INTO popularity_regionale (entity_type, entity_id, region, valeur)
            VALUES ('worker', $workerId, $region, 45);
            """;
        regionCommand.Parameters.AddWithValue("$workerId", workerId);
        regionCommand.Parameters.AddWithValue("$region", region);
        regionCommand.ExecuteNonQuery();

        transaction.Commit();
    }
}
