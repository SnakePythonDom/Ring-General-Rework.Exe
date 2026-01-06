using Microsoft.Data.Sqlite;
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
            var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
            CreerSchema(factory);
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
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    private static void CreerSchema(SqliteConnectionFactory factory)
    {
        using var connexion = factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Workers (
                WorkerId TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Nationality TEXT NOT NULL,
                CompanyId TEXT
            );

            CREATE TABLE IF NOT EXISTS BackstageIncidents (
                IncidentId TEXT PRIMARY KEY,
                WorkerId TEXT NOT NULL,
                IncidentType TEXT NOT NULL,
                Description TEXT NOT NULL,
                Severity INTEGER NOT NULL,
                Week INTEGER NOT NULL,
                Status TEXT NOT NULL DEFAULT 'pending'
            );

            CREATE TABLE IF NOT EXISTS DisciplinaryActions (
                ActionId TEXT PRIMARY KEY,
                IncidentId TEXT NOT NULL,
                WorkerId TEXT NOT NULL,
                ActionType TEXT NOT NULL,
                MoraleDelta INTEGER NOT NULL,
                Week INTEGER NOT NULL,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS MoraleHistory (
                MoraleHistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                WorkerId TEXT NOT NULL,
                Week INTEGER NOT NULL,
                Delta INTEGER NOT NULL,
                Value INTEGER NOT NULL,
                Reason TEXT NOT NULL,
                IncidentId TEXT
            );

            CREATE TABLE IF NOT EXISTS inbox_items (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                type TEXT NOT NULL,
                titre TEXT NOT NULL,
                contenu TEXT NOT NULL,
                semaine INTEGER NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    private static void SeedData(SqliteConnectionFactory factory)
    {
        using var connexion = factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO Workers (WorkerId, Name, Nationality, CompanyId)
            VALUES ('W-1', 'Alpha', 'FR', 'COMP-1');
            """;
        command.ExecuteNonQuery();
    }
}
