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
            var initializer = new DbInitializer();
            initializer.CreateDatabaseIfMissing(tempFile);

            using (var connexion = new SqliteConnection($"Data Source={tempFile}"))
            {
                connexion.Open();
                using var command = connexion.CreateCommand();
                command.CommandText = """
                    INSERT INTO Workers (WorkerId, Name, Nationality, InjuryStatus)
                    VALUES ($workerId, $name, $nationality, 'AUCUNE');
                    """;
                command.Parameters.AddWithValue("$workerId", "W-1");
                command.Parameters.AddWithValue("$name", "Test Worker");
                command.Parameters.AddWithValue("$nationality", "FR");
                command.ExecuteNonQuery();
            }

            var factory = new SqliteConnectionFactory($"Data Source={tempFile}");
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
            var initializer = new DbInitializer();
            initializer.CreateDatabaseIfMissing(tempFile);

            using (var connexion = new SqliteConnection($"Data Source={tempFile}"))
            {
                connexion.Open();
                using var command = connexion.CreateCommand();
                command.CommandText = """
                    INSERT INTO Workers (WorkerId, Name, Nationality, InjuryStatus)
                    VALUES ($workerId, $name, $nationality, 'AUCUNE');
                    """;
                command.Parameters.AddWithValue("$workerId", "W-1");
                command.Parameters.AddWithValue("$name", "Test Worker");
                command.Parameters.AddWithValue("$nationality", "FR");
                command.ExecuteNonQuery();
            }

            var factory = new SqliteConnectionFactory($"Data Source={tempFile}");
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
}
