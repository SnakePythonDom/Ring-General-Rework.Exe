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
            var initializer = new DbInitializer();
            initializer.CreateDatabaseIfMissing(chemin);

            using (var connexion = new SqliteConnection($"Data Source={chemin}"))
            {
                connexion.Open();
                using var command = connexion.CreateCommand();
                command.CommandText = """
                    INSERT INTO Workers (WorkerId, Name, Nationality)
                    VALUES ($workerId, $nom, $nationalite);
                    """;
                command.Parameters.AddWithValue("$workerId", "W-1");
                command.Parameters.AddWithValue("$nom", "Alpha");
                command.Parameters.AddWithValue("$nationalite", "FR");
                command.ExecuteNonQuery();
            }

            var factory = new SqliteConnectionFactory($"Data Source={chemin}");
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
            if (File.Exists(chemin))
            {
                File.Delete(chemin);
            }
        }
    }
}
