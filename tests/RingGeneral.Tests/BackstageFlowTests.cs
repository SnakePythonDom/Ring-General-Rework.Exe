using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests;

public sealed class BackstageFlowTests
{
    [Fact]
    public void Incident_declenche_inbox_sanction_et_morale()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"backstage-{Guid.NewGuid():N}.db");
        try
        {
            var initializer = new DbInitializer();
            initializer.CreateDatabaseIfMissing(dbPath);

            var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
            SeedData(factory);

            var definitions = new List<IncidentDefinition>
            {
                new(
                    "ALTERCATION",
                    "Altercation",
                    "Deux talents se sont disputés.",
                    55,
                    -10,
                    -6,
                    new[] { "conflit" })
            };

            var backstageService = new BackstageService(new SeededRandomProvider(9), definitions);
            var disciplineService = new DisciplineService(new SeededRandomProvider(4));
            var backstageRepo = new BackstageRepository(factory);
            var gameRepo = new GameRepository(factory);

            var incidents = backstageService.RollIncidents(3, new[]
            {
                new WorkerBackstageProfile("W-1", "Alpha")
            }, 1);

            Assert.Single(incidents);
            var incident = incidents[0];

            backstageRepo.AjouterIncident(incident);
            gameRepo.AjouterInboxItem(new InboxItem(
                "backstage",
                "Incident backstage",
                incident.Description,
                incident.Week));

            var moraleAvant = backstageRepo.ChargerMoraleActuelle(incident.WorkerId);
            var (action, moraleEntry) = disciplineService.AppliquerSanction(incident, moraleAvant, incident.Week, "Sanction automatique");
            backstageRepo.AjouterActionDisciplinaire(action);
            backstageRepo.AjouterMoraleHistorique(moraleEntry);

            var incidentsDb = backstageRepo.ChargerIncidents();
            var actionsDb = backstageRepo.ChargerActions(incident.IncidentId);
            var moraleApres = backstageRepo.ChargerMoraleActuelle(incident.WorkerId);
            var inbox = gameRepo.ChargerInbox();

            Assert.Single(incidentsDb);
            Assert.Single(actionsDb);
            Assert.Single(inbox);
            Assert.NotEqual(moraleAvant, moraleApres);
        }
        finally
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    private static void SeedData(SqliteConnectionFactory factory)
    {
        using var connexion = factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO Countries (CountryId, Code, Name)
            VALUES ('FR', 'FR', 'France');

            INSERT INTO Regions (RegionId, CountryId, Name)
            VALUES ('FR-IDF', 'FR', 'Île-de-France');

            INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel)
            VALUES ('COMP-1', 'Compagnie Test', 'FR', 'FR-IDF', 50, 10000, 45, 3, 0);

            INSERT INTO Workers (WorkerId, Name, Nationality, CompanyId)
            VALUES ('W-1', 'Alpha', 'FR', 'COMP-1');
            """;
        command.ExecuteNonQuery();
    }
}
