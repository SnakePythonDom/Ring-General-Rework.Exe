using Microsoft.Data.Sqlite;
using RingGeneral.Core.Medical;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests;

public sealed class MedicalWorkflowTests
{
    [Fact]
    public void Blessure_est_persisted_et_recuperee()
    {
        var chemin = Path.Combine(Path.GetTempPath(), $"ringgeneral-medical-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new SqliteConnectionFactory($"Data Source={chemin}");
            CreerSchema(factory);
            InsererWorker(factory, "W-1", "Alpha", "FR");

            var repository = new MedicalRepository(factory);
            var injuryService = new InjuryService(new MedicalRecommendations());

            var result = injuryService.AppliquerBlessure("W-1", "Entorse", InjurySeverity.Moyenne, 4, 72, "Test initial");
            var injuryId = repository.AjouterBlessure(result.Blessure);
            var planId = repository.AjouterPlanRecuperation(result.Plan with { InjuryId = injuryId });

            Assert.True(injuryId > 0);
            Assert.True(planId > 0);

            var blessure = repository.ChargerBlessure(injuryId);
            Assert.NotNull(blessure);
            Assert.True(blessure!.IsActive);
            Assert.Equal(4 + result.Recommendation.RecommendedRestWeeks, blessure.EndWeek);

            var recuperation = injuryService.RecupererBlessure(blessure, blessure.EndWeek ?? 5, "Rétabli");
            repository.MettreAJourBlessure(recuperation);

            var blessureFinale = repository.ChargerBlessure(injuryId);
            Assert.NotNull(blessureFinale);
            Assert.False(blessureFinale!.IsActive);
            Assert.Equal("Rétabli", blessureFinale.Notes);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(chemin))
            {
                File.Delete(chemin);
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
                InjuryStatus TEXT NOT NULL DEFAULT 'AUCUNE'
            );

            CREATE TABLE IF NOT EXISTS Injuries (
                InjuryId INTEGER PRIMARY KEY AUTOINCREMENT,
                WorkerId TEXT NOT NULL,
                Type TEXT NOT NULL,
                Severity INTEGER NOT NULL,
                StartDate INTEGER NOT NULL,
                EndDate INTEGER,
                IsActive INTEGER NOT NULL DEFAULT 1,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS MedicalNotes (
                MedicalNoteId INTEGER PRIMARY KEY AUTOINCREMENT,
                InjuryId INTEGER,
                WorkerId TEXT NOT NULL,
                Note TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS RecoveryPlans (
                RecoveryPlanId INTEGER PRIMARY KEY AUTOINCREMENT,
                InjuryId INTEGER NOT NULL,
                WorkerId TEXT NOT NULL,
                StartDate INTEGER NOT NULL,
                TargetDate INTEGER NOT NULL,
                RecommendedRestWeeks INTEGER NOT NULL,
                RiskLevel TEXT NOT NULL,
                Status TEXT NOT NULL DEFAULT 'EN_COURS',
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            """;
        command.ExecuteNonQuery();
    }

    private static void InsererWorker(SqliteConnectionFactory factory, string workerId, string nom, string nationalite)
    {
        using var connexion = factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO Workers (WorkerId, Name, Nationality)
            VALUES ($workerId, $nom, $nationalite);
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        command.Parameters.AddWithValue("$nom", nom);
        command.Parameters.AddWithValue("$nationalite", nationalite);
        command.ExecuteNonQuery();
    }
}
