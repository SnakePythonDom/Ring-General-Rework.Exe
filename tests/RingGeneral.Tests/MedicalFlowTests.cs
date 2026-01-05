using Microsoft.Data.Sqlite;
using RingGeneral.Core.Services;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests;

public sealed class MedicalFlowTests
{
    [Fact]
    public void Blessure_est_persistee_et_recuperee()
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
            var service = new InjuryService(repository, new MedicalRecommendations());

            var application = service.AppliquerBlessure("W-1", "Genou", 65, 12, 40, "IRM nécessaire");

            Assert.True(application.Injury.InjuryId > 0);
            Assert.True(application.Plan.RecoveryPlanId > 0);

            var injury = repository.ChargerBlessure(application.Injury.InjuryId);
            Assert.NotNull(injury);
            Assert.True(injury!.IsActive);
            Assert.Equal("Genou", injury.Type);
            Assert.True(injury.RiskLevel > 0);

            var statut = repository.ChargerStatutBlessureWorker("W-1");
            Assert.Equal("Genou", statut);

            var plan = repository.ChargerPlanPourBlessure(application.Injury.InjuryId);
            Assert.NotNull(plan);
            Assert.Equal("EN_COURS", plan!.Status);

            var recovery = service.RecupererBlessure(application.Injury.InjuryId, 15, "Retour validé");
            Assert.Equal("TERMINE", recovery.Status);

            var injuryUpdated = repository.ChargerBlessure(application.Injury.InjuryId);
            Assert.NotNull(injuryUpdated);
            Assert.False(injuryUpdated!.IsActive);
            Assert.Equal(15, injuryUpdated.EndWeek);

            var statutFinal = repository.ChargerStatutBlessureWorker("W-1");
            Assert.Equal("AUCUNE", statutFinal);

            var planUpdated = repository.ChargerPlanPourBlessure(application.Injury.InjuryId);
            Assert.NotNull(planUpdated);
            Assert.Equal("TERMINE", planUpdated!.Status);
            Assert.Equal(15, planUpdated.CompletedWeek);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
