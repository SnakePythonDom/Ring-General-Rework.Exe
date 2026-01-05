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
    public void RafraichirSemaine_CreeRapport_EtPersiste()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ringgeneral-scouting-{Guid.NewGuid():N}.db");
        try
        {
            var repository = InitialiserRepository(dbPath);
            InsererFreeAgent(dbPath, "FA-001", "Louis", "Moreau", "FR");

            var mission = new ScoutMission("MIS-001", "FR", 1, 1, 0, "EN_COURS", null);
            repository.AjouterMission(mission);

            var service = new ScoutingService(new SeededRandomProvider(7));
            var refresh = service.RafraichirSemaine(
                repository.ChargerMissions(),
                repository.ChargerScoutReports(),
                repository.ChargerCiblesScouting(),
                2);

            foreach (var missionMaj in refresh.MissionsMaj)
            {
                repository.MettreAJourMission(missionMaj);
            }

            foreach (var rapport in refresh.NouveauxRapports)
            {
                repository.AjouterScoutReport(rapport);
            }

            var rapports = repository.ChargerScoutReports();

            Assert.NotEmpty(rapports);
            Assert.Contains(rapports, rapport => rapport.WorkerId == "FA-001");
        }
        finally
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    [Fact]
    public void RafraichirSemaine_FaitProgresserMission_AuFilDesSemaines()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ringgeneral-scouting-{Guid.NewGuid():N}.db");
        try
        {
            var repository = InitialiserRepository(dbPath);
            var mission = new ScoutMission("MIS-002", "JP", 1, 3, 0, "EN_COURS", null);
            repository.AjouterMission(mission);

            var service = new ScoutingService(new SeededRandomProvider(11));

            for (var semaine = 2; semaine <= 4; semaine++)
            {
                var refresh = service.RafraichirSemaine(
                    repository.ChargerMissions(),
                    repository.ChargerScoutReports(),
                    Array.Empty<ScoutTargetProfile>(),
                    semaine);

                foreach (var missionMaj in refresh.MissionsMaj)
                {
                    repository.MettreAJourMission(missionMaj);
                }
            }

            var missionsFinales = repository.ChargerMissions();
            var missionFinale = Assert.Single(missionsFinales);

            Assert.Equal(3, missionFinale.Progression);
            Assert.Equal("TERMINEE", missionFinale.Statut);
        }
        finally
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    private static GameRepository InitialiserRepository(string dbPath)
    {
        var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
        var repository = new GameRepository(factory);
        repository.Initialiser();
        return repository;
    }

    private static void InsererFreeAgent(string dbPath, string workerId, string prenom, string nom, string region)
    {
        using var connexion = new SqliteConnection($"Data Source={dbPath}");
        connexion.Open();

        using var workerCommand = connexion.CreateCommand();
        workerCommand.CommandText = """
            INSERT INTO workers (worker_id, nom, prenom, company_id, in_ring, entertainment, story, popularite, fatigue, blessure, momentum, role_tv, type_worker)
            VALUES ($workerId, $nom, $prenom, NULL, 72, 64, 58, 51, 0, 'AUCUNE', 3, 'MID', 'FREE_AGENT');
            """;
        workerCommand.Parameters.AddWithValue("$workerId", workerId);
        workerCommand.Parameters.AddWithValue("$nom", nom);
        workerCommand.Parameters.AddWithValue("$prenom", prenom);
        workerCommand.ExecuteNonQuery();

        using var popularityCommand = connexion.CreateCommand();
        popularityCommand.CommandText = """
            INSERT INTO popularity_regionale (entity_type, entity_id, region, valeur)
            VALUES ('worker', $workerId, $region, 52);
            """;
        popularityCommand.Parameters.AddWithValue("$workerId", workerId);
        popularityCommand.Parameters.AddWithValue("$region", region);
        popularityCommand.ExecuteNonQuery();
    }
}
