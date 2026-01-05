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
        var factory = new SqliteConnectionFactory($"Data Source={chemin}");

        using var connexion = factory.OuvrirConnexion();

        // Créer les tables nécessaires directement (sans dépendre des migrations)
        using var schemaCommand = connexion.CreateCommand();
        schemaCommand.CommandText = """
            CREATE TABLE IF NOT EXISTS Titles (
                TitleId TEXT PRIMARY KEY,
                CompanyId TEXT NOT NULL,
                Name TEXT NOT NULL,
                Prestige INTEGER NOT NULL DEFAULT 50,
                HolderWorkerId TEXT
            );

            CREATE TABLE IF NOT EXISTS TitleReigns (
                TitleReignId INTEGER PRIMARY KEY AUTOINCREMENT,
                TitleId TEXT NOT NULL,
                WorkerId TEXT NOT NULL,
                StartDate INTEGER NOT NULL,
                EndDate INTEGER,
                IsCurrent INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS TitleMatches (
                TitleMatchId INTEGER PRIMARY KEY AUTOINCREMENT,
                TitleId TEXT NOT NULL,
                ShowId TEXT,
                Week INTEGER NOT NULL,
                ChampionId TEXT,
                ChallengerId TEXT NOT NULL,
                WinnerId TEXT NOT NULL,
                IsTitleChange INTEGER NOT NULL DEFAULT 0,
                PrestigeDelta INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Workers (
                WorkerId TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Nationality TEXT NOT NULL,
                CompanyId TEXT,
                InRing INTEGER NOT NULL DEFAULT 50,
                Entertainment INTEGER NOT NULL DEFAULT 50,
                Story INTEGER NOT NULL DEFAULT 50,
                Popularity INTEGER NOT NULL DEFAULT 50,
                Fatigue INTEGER NOT NULL DEFAULT 0,
                InjuryStatus TEXT NOT NULL DEFAULT 'AUCUNE',
                Momentum INTEGER NOT NULL DEFAULT 0,
                RoleTv TEXT NOT NULL DEFAULT 'NONE',
                Morale INTEGER NOT NULL DEFAULT 60,
                SimLevel INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Companies (
                CompanyId TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                RegionId TEXT,
                Prestige INTEGER NOT NULL DEFAULT 50,
                Treasury REAL NOT NULL DEFAULT 0,
                AverageAudience INTEGER NOT NULL DEFAULT 0,
                Reach INTEGER NOT NULL DEFAULT 0,
                SimLevel INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS ContenderRankings (
                ContenderRankingId INTEGER PRIMARY KEY AUTOINCREMENT,
                TitleId TEXT NOT NULL,
                WorkerId TEXT NOT NULL,
                Rank INTEGER NOT NULL,
                Score INTEGER NOT NULL DEFAULT 0,
                Reason TEXT
            );
            """;
        schemaCommand.ExecuteNonQuery();

        // Insérer les données de test
        using var command = connexion.CreateCommand();
        InsererDonnees(command, """
            INSERT INTO Companies (CompanyId, Name, Prestige)
            VALUES ('COMP-1', 'Compagnie Test', 50);
            """);
        InsererDonnees(command, """
            INSERT INTO Workers (WorkerId, Name, Nationality, CompanyId, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv)
            VALUES ('WORKER-1', 'Champion', 'FR', 'COMP-1', 70, 60, 50, 65, 0, 'AUCUNE', 10, 'NONE');
            """);
        InsererDonnees(command, """
            INSERT INTO Workers (WorkerId, Name, Nationality, CompanyId, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv)
            VALUES ('WORKER-2', 'Aspirant', 'FR', 'COMP-1', 65, 55, 45, 60, 0, 'AUCUNE', 8, 'NONE');
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
            SqliteConnection.ClearAllPools();
            if (File.Exists(Chemin))
            {
                File.Delete(Chemin);
            }
        }
    }
}
