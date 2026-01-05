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
    public void Incident_inbox_sanction_modifie_morale()
    {
        var cheminDb = Path.Combine(Path.GetTempPath(), $"backstage_{Guid.NewGuid():N}.db");
        var initializer = new DbInitializer();
        initializer.CreateDatabaseIfMissing(cheminDb);

        using (var connexion = new SqliteConnection($"Data Source={cheminDb}"))
        {
            connexion.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = """
                INSERT INTO Countries (CountryId, Code, Name) VALUES ('C-1', 'FR', 'France');
                INSERT INTO Regions (RegionId, CountryId, Name) VALUES ('R-1', 'C-1', 'France');
                INSERT INTO Companies (CompanyId, Name, RegionId, Prestige, Treasury, AverageAudience, Reach)
                VALUES ('COMP-1', 'Promo Test', 'R-1', 40, 5000, 35, 3);
                INSERT INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv, Morale)
                VALUES
                    ('W-1', 'Alpha', 'COMP-1', 'FR', 60, 55, 50, 45, 10, 'AUCUNE', 2, 'MID', 60),
                    ('W-2', 'Beta', 'COMP-1', 'FR', 58, 52, 48, 42, 8, 'AUCUNE', 1, 'LOW', 62);
                INSERT INTO Shows (ShowId, CompanyId, Name, Week, RegionId, DurationMinutes)
                VALUES ('SHOW-1', 'COMP-1', 'Show Test', 1, 'R-1', 120);
                """;
            command.ExecuteNonQuery();
        }

        var factory = new SqliteConnectionFactory($"Data Source={cheminDb}");
        var repository = new GameRepository(factory);

        var roster = repository.ChargerBackstageRoster("COMP-1");
        var morales = repository.ChargerMorales("COMP-1");
        var moraleInitiale = repository.ChargerMorale(roster[0].WorkerId);

        var definition = new BackstageIncidentDefinition(
            "incident_test",
            "Incident test",
            "{worker} provoque un incident.",
            1.0,
            1,
            1,
            2,
            2,
            -4,
            -4);

        var service = new BackstageService(new SeededRandomProvider(12));
        var resultat = service.LancerIncidents(2, "COMP-1", roster, morales, new[] { definition });

        foreach (var incident in resultat.Incidents)
        {
            repository.EnregistrerBackstageIncident(incident);
        }

        foreach (var inboxItem in resultat.InboxItems)
        {
            repository.AjouterInboxItem(inboxItem);
        }

        repository.AppliquerMoraleImpacts(resultat.MoraleImpacts, 2);
        var moraleApresIncident = repository.ChargerMorale(roster[0].WorkerId);

        var discipline = new DisciplineService();
        var actionDef = new DisciplinaryActionDefinition("amende", "Amende", 2, -5);
        var disciplineResult = discipline.AppliquerAction(
            2,
            "COMP-1",
            resultat.Incidents[0],
            roster[0],
            actionDef,
            "Test sanction");
        repository.EnregistrerDisciplinaryAction(disciplineResult.Action);
        repository.AppliquerMoraleImpacts(new[] { disciplineResult.MoraleImpact }, 2);

        var moraleFinale = repository.ChargerMorale(roster[0].WorkerId);
        var inbox = repository.ChargerInbox();

        Assert.NotEmpty(resultat.Incidents);
        Assert.NotEmpty(resultat.InboxItems);
        Assert.Contains(inbox, item => item.Type == "incident");
        Assert.True(moraleApresIncident < moraleInitiale);
        Assert.True(moraleFinale < moraleApresIncident);
    }
}
