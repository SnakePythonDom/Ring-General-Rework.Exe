using Microsoft.Data.Sqlite;
using RingGeneral.Core.Contracts;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests;

public sealed class ContractNegotiationTests
{
    [Fact]
    public void Creation_contre_proposition_acceptation_cree_un_contrat_actif()
    {
        var (contractRepo, gameRepo, dbPath) = CreerRepository();
        try
        {
            var service = new ContractNegotiationService(contractRepo);
            var draft = new ContractOfferDraft(
                "W-001",
                "COMP-001",
                "exclusif",
                1,
                20,
                1500m,
                200m,
                0m,
                4,
                false,
                true,
                2);

            var offre = service.CreerOffre(draft, 1, false);

            var contreDraft = draft with
            {
                SalaireHebdo = 1700m,
                EndWeek = 24,
                ExpirationDelaiSemaines = 2
            };

            var contre = service.ContreProposer(offre.OfferId, contreDraft, 2);
            var contrat = service.AccepterOffre(contre.OfferId, 2);

            var contratCharge = contractRepo.ChargerContratActif(contrat.ContractId);
            Assert.NotNull(contratCharge);
            Assert.Equal("actif", contratCharge!.Statut);
            Assert.Equal(contreDraft.EndWeek, contratCharge.EndWeek);
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

    [Fact]
    public void Offres_expirantes_sont_retournees_par_le_repository()
    {
        var (contractRepo, gameRepo, dbPath) = CreerRepository();
        try
        {
            var service = new ContractNegotiationService(contractRepo);
            var draft = new ContractOfferDraft(
                "W-002",
                "COMP-001",
                "non_exclusif",
                1,
                10,
                900m,
                0m,
                0m,
                0,
                false,
                false,
                1);

            service.CreerOffre(draft, 1, false);

            var expirantes = contractRepo.ChargerOffresExpirant(3);
            Assert.Single(expirantes);
            Assert.Equal("W-002", expirantes[0].WorkerId);
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

    [Fact]
    public void Pagination_offres_limite_les_resultats()
    {
        var (contractRepo, gameRepo, dbPath) = CreerRepository();
        try
        {
            var service = new ContractNegotiationService(contractRepo);
            var draft = new ContractOfferDraft(
                "W-003",
                "COMP-001",
                "appearance",
                1,
                6,
                500m,
                50m,
                0m,
                0,
                false,
                false,
                2);

            service.CreerOffre(draft with { WorkerId = "W-003" }, 1, true);
            service.CreerOffre(draft with { WorkerId = "W-004" }, 2, true);
            service.CreerOffre(draft with { WorkerId = "W-001" }, 3, true);

            var premierePage = contractRepo.ChargerOffres("COMP-001", 0, 2);
            var secondePage = contractRepo.ChargerOffres("COMP-001", 2, 2);

            Assert.Equal(2, premierePage.Count);
            Assert.Single(secondePage);
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

    private static (IContractRepository ContractRepository, GameRepository GameRepository, string DbPath) CreerRepository()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ringgeneral-tests-{Guid.NewGuid():N}.db");
        new DbInitializer().CreateDatabaseIfMissing(dbPath);
        var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
        var repositories = RepositoryFactory.CreateRepositories(factory);
        return (repositories.ContractRepository, repositories.GameRepository, dbPath);
    }
}
