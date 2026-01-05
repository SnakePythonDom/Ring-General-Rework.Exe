using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests;

public sealed class TitleServiceTests
{
    [Fact]
    public void ChangementChampionCreeUnNouveauRegne()
    {
        using var db = CreerBaseDeTest();
        var repository = new TitleRepository(db.Factory);
        var service = new TitleService(repository);

        var outcome = service.EnregistrerMatch(new TitleMatchInput(
            "TITLE-1",
            "WORKER-2",
            "WORKER-2",
            5,
            "WORKER-1"));

        Assert.True(outcome.TitleChanged);

        using var connexion = db.Factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT WorkerId, StartDate, EndDate, IsCurrent FROM TitleReigns ORDER BY TitleReignId;";
        using var reader = command.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal("WORKER-1", reader.GetString(0));
        Assert.Equal(1, reader.GetInt32(1));
        Assert.Equal(5, reader.GetInt32(2));
        Assert.Equal(0, reader.GetInt32(3));
        Assert.True(reader.Read());
        Assert.Equal("WORKER-2", reader.GetString(0));
        Assert.Equal(5, reader.GetInt32(1));
        Assert.True(reader.IsDBNull(2));
        Assert.Equal(1, reader.GetInt32(3));
    }

    [Fact]
    public void PrestigeVarieApresUneDefenseReussie()
    {
        using var db = CreerBaseDeTest();
        var repository = new TitleRepository(db.Factory);
        var service = new TitleService(repository);

        var outcome = service.EnregistrerMatch(new TitleMatchInput(
            "TITLE-1",
            "WORKER-2",
            "WORKER-1",
            3,
            "WORKER-1"));

        Assert.False(outcome.TitleChanged);
        Assert.True(outcome.PrestigeDelta > 0);

        using var connexion = db.Factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Prestige FROM Titles WHERE TitleId = 'TITLE-1';";
        var prestige = Convert.ToInt32(command.ExecuteScalar());
        Assert.True(prestige > 40);
    }

    private static TestDatabase CreerBaseDeTest()
    {
        var chemin = Path.Combine(Path.GetTempPath(), $"rg-test-{Guid.NewGuid():N}.db");
        var initializer = new DbInitializer();
        initializer.CreateDatabaseIfMissing(chemin);

        var factory = new SqliteConnectionFactory($"Data Source={chemin}");
        using var connexion = factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();

        InsererDonnees(command, "INSERT INTO Countries (CountryId, Code, Name) VALUES ('COUNTRY-1', 'FR', 'France');");
        InsererDonnees(command, "INSERT INTO Regions (RegionId, CountryId, Name) VALUES ('REGION-1', 'COUNTRY-1', 'Ile-de-France');");
        InsererDonnees(command, """
            INSERT INTO Companies (CompanyId, Name, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel)
            VALUES ('COMP-1', 'Compagnie Test', 'REGION-1', 50, 0, 0, 0, 0);
            """);
        InsererDonnees(command, """
            INSERT INTO Workers (WorkerId, Name, Nationality, CompanyId, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv, SimLevel)
            VALUES ('WORKER-1', 'Champion', 'FR', 'COMP-1', 70, 60, 50, 65, 0, 'AUCUNE', 10, 'NONE', 0);
            """);
        InsererDonnees(command, """
            INSERT INTO Workers (WorkerId, Name, Nationality, CompanyId, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv, SimLevel)
            VALUES ('WORKER-2', 'Aspirant', 'FR', 'COMP-1', 65, 55, 45, 60, 0, 'AUCUNE', 8, 'NONE', 0);
            """);
        InsererDonnees(command, """
            INSERT INTO Titles (TitleId, CompanyId, Name, Prestige, HolderWorkerId)
            VALUES ('TITLE-1', 'COMP-1', 'Titre Test', 40, 'WORKER-1');
            """);
        InsererDonnees(command, """
            INSERT INTO TitleReigns (TitleId, WorkerId, StartDate, IsCurrent)
            VALUES ('TITLE-1', 'WORKER-1', 1, 1);
            """);

        return new TestDatabase(factory, chemin);
    }

    private static void InsererDonnees(SqliteCommand command, string sql)
    {
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed record TestDatabase(SqliteConnectionFactory Factory, string Chemin) : IDisposable
    {
        public void Dispose()
        {
            if (File.Exists(Chemin))
            {
                File.Delete(Chemin);
            }
        }
    }
}
