using Microsoft.Data.Sqlite;
using RingGeneral.Core.Medical;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests;

public sealed class MedicalFlowTests
{
    [Fact]
    public void InjuryService_cree_blessure_et_plan()
    {
        var service = new InjuryService(new MedicalRecommendations());

        var result = service.AppliquerBlessure(
            workerId: "W-1",
            type: "Genou",
            severity: InjurySeverity.Moyenne,
            semaineCourante: 12,
            fatigue: 40,
            notes: "IRM nécessaire");

        Assert.Equal("W-1", result.Blessure.WorkerId);
        Assert.Equal("Genou", result.Blessure.Type);
        Assert.Equal(InjurySeverity.Moyenne, result.Blessure.Severity);
        Assert.Equal(12, result.Blessure.StartWeek);
        Assert.True(result.Blessure.IsActive);
        Assert.Equal("IRM nécessaire", result.Blessure.Notes);

        Assert.Equal("W-1", result.Plan.WorkerId);
        Assert.Equal(12, result.Plan.StartWeek);
        Assert.Equal("EN_COURS", result.Plan.Status);
        Assert.True(result.Recommendation.RecommendedRestWeeks > 0);
    }

    [Fact]
    public void InjuryService_recuperer_blessure()
    {
        var service = new InjuryService(new MedicalRecommendations());
        var blessure = new InjuryRecord(
            InjuryId: 1,
            WorkerId: "W-1",
            Type: "Genou",
            Severity: InjurySeverity.Moyenne,
            StartWeek: 12,
            EndWeek: null,
            IsActive: true,
            Notes: null);

        var recuperee = service.RecupererBlessure(blessure, semaineCourante: 16, note: "Retour validé");

        Assert.False(recuperee.IsActive);
        Assert.Equal(16, recuperee.EndWeek);
        Assert.Equal("Retour validé", recuperee.Notes);
    }

    [Fact]
    public void MedicalRepository_persiste_et_charge_blessure()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"ringgeneral-medical-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new SqliteConnectionFactory($"Data Source={tempFile}");
            CreerSchema(factory);
            InsererWorkerTest(factory, "W-1");

            var repository = new MedicalRepository(factory);

            var blessure = new InjuryRecord(
                InjuryId: 0,
                WorkerId: "W-1",
                Type: "Genou",
                Severity: InjurySeverity.Moyenne,
                StartWeek: 12,
                EndWeek: 16,
                IsActive: true,
                Notes: "Test notes");

            var injuryId = repository.AjouterBlessure(blessure);
            Assert.True(injuryId > 0);

            var loaded = repository.ChargerBlessure(injuryId);
            Assert.NotNull(loaded);
            Assert.Equal("W-1", loaded!.WorkerId);
            Assert.Equal("Genou", loaded.Type);
            Assert.Equal(InjurySeverity.Moyenne, loaded.Severity);
            Assert.Equal(12, loaded.StartWeek);
            Assert.True(loaded.IsActive);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void MedicalRepository_met_a_jour_blessure()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"ringgeneral-medical-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new SqliteConnectionFactory($"Data Source={tempFile}");
            CreerSchema(factory);
            InsererWorkerTest(factory, "W-1");

            var repository = new MedicalRepository(factory);

            var blessure = new InjuryRecord(
                InjuryId: 0,
                WorkerId: "W-1",
                Type: "Genou",
                Severity: InjurySeverity.Grave,
                StartWeek: 10,
                EndWeek: null,
                IsActive: true,
                Notes: null);

            var injuryId = repository.AjouterBlessure(blessure);

            var blessureRecuperee = blessure with
            {
                InjuryId = injuryId,
                EndWeek = 18,
                IsActive = false,
                Notes = "Retour au ring validé"
            };
            repository.MettreAJourBlessure(blessureRecuperee);

            var loaded = repository.ChargerBlessure(injuryId);
            Assert.NotNull(loaded);
            Assert.False(loaded!.IsActive);
            Assert.Equal(18, loaded.EndWeek);
            Assert.Equal("Retour au ring validé", loaded.Notes);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void InjuryService_calcule_risque_correctement()
    {
        var service = new InjuryService(new MedicalRecommendations());

        var risqueFaible = service.EvaluerRisque(fatigue: 20, severity: InjurySeverity.Legere);
        var risqueModere = service.EvaluerRisque(fatigue: 50, severity: InjurySeverity.Moyenne);
        var risqueEleve = service.EvaluerRisque(fatigue: 80, severity: InjurySeverity.Grave);

        Assert.Equal("FAIBLE", risqueFaible);
        Assert.Equal("MODÉRÉ", risqueModere);
        Assert.Equal("ÉLEVÉ", risqueEleve);
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

    private static void InsererWorkerTest(SqliteConnectionFactory factory, string workerId)
    {
        using var connexion = factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO Workers (WorkerId, Name, Nationality, InjuryStatus)
            VALUES ($workerId, 'Test Worker', 'FR', 'AUCUNE');
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        command.ExecuteNonQuery();
    }
}
