using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests;

public sealed class MedicalWorkflowTests
{
    [Fact]
    public void Blessure_PersisteEtSeRecupere()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ringgeneral-medical-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
            var repository = new GameRepository(factory);
            repository.Initialiser();

            using (var connexion = factory.OuvrirConnexion())
            using (var command = connexion.CreateCommand())
            {
                command.CommandText = """
                    INSERT INTO workers (worker_id, nom, prenom, company_id, in_ring, entertainment, story, popularite, fatigue, blessure, momentum, role_tv)
                    VALUES ('WORKER-TEST', 'Test', 'Medical', NULL, 50, 50, 50, 50, 20, 'AUCUNE', 0, 'NONE');
                    """;
                command.ExecuteNonQuery();
            }

            var medicalRepository = new MedicalRepository(factory);
            var injuryService = new InjuryService(new MedicalRecommendations());

            var resultat = injuryService.AppliquerBlessure("WORKER-TEST", "ENTORSE", InjurySeverity.Moyenne, 12, 78);

            medicalRepository.AjouterBlessure(resultat.Injury);
            medicalRepository.AjouterPlan(resultat.RecoveryPlan);
            medicalRepository.AjouterNote(resultat.MedicalNote);

            var blessures = medicalRepository.ChargerBlessures("WORKER-TEST");

            Assert.Single(blessures);
            Assert.Equal(InjurySeverity.Moyenne, blessures[0].Severity);
            Assert.Equal(InjuryStatus.Active, blessures[0].Status);

            var recuperation = injuryService.Recuperer(blessures[0], 16);
            medicalRepository.MettreAJourBlessure(recuperation);

            var blessuresMisesAJour = medicalRepository.ChargerBlessures("WORKER-TEST");
            Assert.Equal(InjuryStatus.RetourEnCours, blessuresMisesAJour[0].Status);
            Assert.Equal(16, blessuresMisesAJour[0].WeekEnd);
        }
        finally
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }
}
