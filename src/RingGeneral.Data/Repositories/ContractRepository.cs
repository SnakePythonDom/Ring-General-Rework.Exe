using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class ContractRepository : RepositoryBase, IContractRepository
{
    public ContractRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public void AjouterOffre(ContractOffer offre, IReadOnlyList<ContractClause> clauses)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        using var command = connexion.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO contract_offers (
                offer_id, negotiation_id, worker_id, company_id, type, debut_semaine, fin_semaine,
                salaire, bonus_show, buyout, non_compete_weeks, auto_renew, exclusif, statut,
                created_week, expiration_week, parent_offer_id, est_ia
            )
            VALUES (
                $offerId, $negotiationId, $workerId, $companyId, $type, $debutSemaine, $finSemaine,
                $salaire, $bonusShow, $buyout, $nonCompete, $autoRenew, $exclusif, $statut,
                $createdWeek, $expirationWeek, $parentOfferId, $estIa
            );
            """;
        command.Parameters.AddWithValue("$offerId", offre.OfferId);
        command.Parameters.AddWithValue("$negotiationId", offre.NegociationId);
        command.Parameters.AddWithValue("$workerId", offre.WorkerId);
        command.Parameters.AddWithValue("$companyId", offre.CompanyId);
        command.Parameters.AddWithValue("$type", offre.TypeContrat);
        command.Parameters.AddWithValue("$debutSemaine", offre.StartWeek);
        command.Parameters.AddWithValue("$finSemaine", offre.EndWeek);
        command.Parameters.AddWithValue("$salaire", offre.SalaireHebdo);
        command.Parameters.AddWithValue("$bonusShow", offre.BonusShow);
        command.Parameters.AddWithValue("$buyout", offre.Buyout);
        command.Parameters.AddWithValue("$nonCompete", offre.NonCompeteWeeks);
        command.Parameters.AddWithValue("$autoRenew", offre.RenouvellementAuto ? 1 : 0);
        command.Parameters.AddWithValue("$exclusif", offre.EstExclusif ? 1 : 0);
        command.Parameters.AddWithValue("$statut", offre.Statut);
        command.Parameters.AddWithValue("$createdWeek", offre.CreatedWeek);
        command.Parameters.AddWithValue("$expirationWeek", offre.ExpirationWeek);
        command.Parameters.AddWithValue("$parentOfferId", (object?)offre.ParentOfferId ?? DBNull.Value);
        command.Parameters.AddWithValue("$estIa", offre.EstIa ? 1 : 0);
        command.ExecuteNonQuery();

        InsererClauses(transaction, clauses, null, offre.OfferId);
        transaction.Commit();
    }

    public ContractOffer? ChargerOffre(string offerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT offer_id, negotiation_id, worker_id, company_id, type, debut_semaine, fin_semaine,
                   salaire, bonus_show, buyout, non_compete_weeks, auto_renew, exclusif, statut,
                   created_week, expiration_week, parent_offer_id, est_ia
            FROM contract_offers
            WHERE offer_id = $offerId;
            """;
        command.Parameters.AddWithValue("$offerId", offerId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ContractOffer(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetInt32(5),
            reader.GetInt32(6),
            LireDecimal(reader, 7),
            LireDecimal(reader, 8),
            LireDecimal(reader, 9),
            reader.GetInt32(10),
            reader.GetInt32(11) == 1,
            reader.GetInt32(12) == 1,
            reader.GetString(13),
            reader.GetInt32(14),
            reader.GetInt32(15),
            reader.IsDBNull(16) ? null : reader.GetString(16),
            reader.GetInt32(17) == 1);
    }

    public IReadOnlyList<ContractOffer> ChargerOffres(string companyId, int offset, int limit)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT offer_id, negotiation_id, worker_id, company_id, type, debut_semaine, fin_semaine,
                   salaire, bonus_show, buyout, non_compete_weeks, auto_renew, exclusif, statut,
                   created_week, expiration_week, parent_offer_id, est_ia
            FROM contract_offers
            WHERE company_id = $companyId
            ORDER BY created_week DESC
            LIMIT $limit OFFSET $offset;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        command.Parameters.AddWithValue("$limit", limit);
        command.Parameters.AddWithValue("$offset", offset);
        using var reader = command.ExecuteReader();
        var offres = new List<ContractOffer>();
        while (reader.Read())
        {
            offres.Add(new ContractOffer(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                LireDecimal(reader, 7),
                LireDecimal(reader, 8),
                LireDecimal(reader, 9),
                reader.GetInt32(10),
                reader.GetInt32(11) == 1,
                reader.GetInt32(12) == 1,
                reader.GetString(13),
                reader.GetInt32(14),
                reader.GetInt32(15),
                reader.IsDBNull(16) ? null : reader.GetString(16),
                reader.GetInt32(17) == 1));
        }

        return offres;
    }

    public IReadOnlyList<ContractOffer> ChargerOffresExpirant(int semaine)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT offer_id, negotiation_id, worker_id, company_id, type, debut_semaine, fin_semaine,
                   salaire, bonus_show, buyout, non_compete_weeks, auto_renew, exclusif, statut,
                   created_week, expiration_week, parent_offer_id, est_ia
            FROM contract_offers
            WHERE expiration_week <= $semaine
              AND statut IN ('proposee', 'contre');
            """;
        command.Parameters.AddWithValue("$semaine", semaine);
        using var reader = command.ExecuteReader();
        var offres = new List<ContractOffer>();
        while (reader.Read())
        {
            offres.Add(new ContractOffer(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                LireDecimal(reader, 7),
                LireDecimal(reader, 8),
                LireDecimal(reader, 9),
                reader.GetInt32(10),
                reader.GetInt32(11) == 1,
                reader.GetInt32(12) == 1,
                reader.GetString(13),
                reader.GetInt32(14),
                reader.GetInt32(15),
                reader.IsDBNull(16) ? null : reader.GetString(16),
                reader.GetInt32(17) == 1));
        }

        return offres;
    }

    public IReadOnlyList<ContractClause> ChargerClausesPourOffre(string offerId)
    {
        return ChargerClauses("offer_id", offerId);
    }

    public void MettreAJourStatutOffre(string offerId, string statut)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE contract_offers SET statut = $statut WHERE offer_id = $offerId;";
        command.Parameters.AddWithValue("$statut", statut);
        command.Parameters.AddWithValue("$offerId", offerId);
        command.ExecuteNonQuery();
    }

    public void AjouterContratActif(ActiveContract contrat, IReadOnlyList<ContractClause> clauses)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        using var command = connexion.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO contracts (
                contract_id, worker_id, company_id, type, debut_semaine, fin_semaine, salaire, bonus_show,
                buyout, non_compete_weeks, auto_renew, exclusif, statut, created_week
            )
            VALUES (
                $contractId, $workerId, $companyId, $type, $debutSemaine, $finSemaine, $salaire, $bonusShow,
                $buyout, $nonCompete, $autoRenew, $exclusif, $statut, $createdWeek
            );
            """;
        command.Parameters.AddWithValue("$contractId", contrat.ContractId);
        command.Parameters.AddWithValue("$workerId", contrat.WorkerId);
        command.Parameters.AddWithValue("$companyId", contrat.CompanyId);
        command.Parameters.AddWithValue("$type", contrat.TypeContrat);
        command.Parameters.AddWithValue("$debutSemaine", contrat.StartWeek);
        command.Parameters.AddWithValue("$finSemaine", contrat.EndWeek);
        command.Parameters.AddWithValue("$salaire", contrat.SalaireHebdo);
        command.Parameters.AddWithValue("$bonusShow", contrat.BonusShow);
        command.Parameters.AddWithValue("$buyout", contrat.Buyout);
        command.Parameters.AddWithValue("$nonCompete", contrat.NonCompeteWeeks);
        command.Parameters.AddWithValue("$autoRenew", contrat.RenouvellementAuto ? 1 : 0);
        command.Parameters.AddWithValue("$exclusif", contrat.EstExclusif ? 1 : 0);
        command.Parameters.AddWithValue("$statut", contrat.Statut);
        command.Parameters.AddWithValue("$createdWeek", contrat.CreatedWeek);
        command.ExecuteNonQuery();

        InsererClauses(transaction, clauses, contrat.ContractId, null);
        transaction.Commit();
    }

    public ActiveContract? ChargerContratActif(string contractId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT contract_id, worker_id, company_id, type, debut_semaine, fin_semaine, salaire, bonus_show,
                   buyout, non_compete_weeks, auto_renew, exclusif, statut, created_week
            FROM contracts
            WHERE contract_id = $contractId;
            """;
        command.Parameters.AddWithValue("$contractId", contractId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ActiveContract(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetInt32(4),
            reader.GetInt32(5),
            LireDecimal(reader, 6),
            LireDecimal(reader, 7),
            LireDecimal(reader, 8),
            reader.GetInt32(9),
            reader.GetInt32(10) == 1,
            reader.GetInt32(11) == 1,
            reader.GetString(12),
            reader.GetInt32(13));
    }

    public ActiveContract? ChargerContratActif(string workerId, string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT contract_id, worker_id, company_id, type, debut_semaine, fin_semaine, salaire, bonus_show,
                   buyout, non_compete_weeks, auto_renew, exclusif, statut, created_week
            FROM contracts
            WHERE worker_id = $workerId
              AND company_id = $companyId
              AND statut = 'actif'
            ORDER BY fin_semaine DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ActiveContract(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetInt32(4),
            reader.GetInt32(5),
            LireDecimal(reader, 6),
            LireDecimal(reader, 7),
            LireDecimal(reader, 8),
            reader.GetInt32(9),
            reader.GetInt32(10) == 1,
            reader.GetInt32(11) == 1,
            reader.GetString(12),
            reader.GetInt32(13));
    }

    public IReadOnlyList<ContractClause> ChargerClausesPourContrat(string contractId)
    {
        return ChargerClauses("contract_id", contractId);
    }

    public void ResilierContrat(string contractId, int finSemaine)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE contracts
            SET fin_semaine = $finSemaine,
                statut = 'libere'
            WHERE contract_id = $contractId;
            """;
        command.Parameters.AddWithValue("$finSemaine", finSemaine);
        command.Parameters.AddWithValue("$contractId", contractId);
        command.ExecuteNonQuery();
    }

    public void EnregistrerNegociation(ContractNegotiationState negociation)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO negotiation_state (negotiation_id, worker_id, company_id, statut, last_offer_id, updated_week)
            VALUES ($negociationId, $workerId, $companyId, $statut, $lastOfferId, $updatedWeek)
            ON CONFLICT(negotiation_id) DO UPDATE SET
                worker_id = excluded.worker_id,
                company_id = excluded.company_id,
                statut = excluded.statut,
                last_offer_id = excluded.last_offer_id,
                updated_week = excluded.updated_week;
            """;
        command.Parameters.AddWithValue("$negociationId", negociation.NegociationId);
        command.Parameters.AddWithValue("$workerId", negociation.WorkerId);
        command.Parameters.AddWithValue("$companyId", negociation.CompanyId);
        command.Parameters.AddWithValue("$statut", negociation.Statut);
        command.Parameters.AddWithValue("$lastOfferId", (object?)negociation.DerniereOffreId ?? DBNull.Value);
        command.Parameters.AddWithValue("$updatedWeek", negociation.DerniereMiseAJourSemaine);
        command.ExecuteNonQuery();
    }

    public ContractNegotiationState? ChargerNegociation(string negociationId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT negotiation_id, worker_id, company_id, statut, last_offer_id, updated_week
            FROM negotiation_state
            WHERE negotiation_id = $negociationId;
            """;
        command.Parameters.AddWithValue("$negociationId", negociationId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ContractNegotiationState(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.GetInt32(5));
    }

    public ContractNegotiationState? ChargerNegociationPourWorker(string workerId, string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT negotiation_id, worker_id, company_id, statut, last_offer_id, updated_week
            FROM negotiation_state
            WHERE worker_id = $workerId AND company_id = $companyId
            ORDER BY updated_week DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ContractNegotiationState(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.GetInt32(5));
    }

    // === Helpers privés (Catégorie B - Contract domain) ===

    private IReadOnlyList<ContractClause> ChargerClauses(string colonne, string id)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = $"SELECT type, valeur FROM contract_clauses WHERE {colonne} = $id;";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        var clauses = new List<ContractClause>();
        while (reader.Read())
        {
            clauses.Add(new ContractClause(reader.GetString(0), reader.GetString(1)));
        }

        return clauses;
    }

    private static void InsererClauses(SqliteTransaction transaction, IReadOnlyList<ContractClause> clauses, string? contractId, string? offerId)
    {
        foreach (var clause in clauses)
        {
            using var command = transaction.Connection!.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO contract_clauses (contract_id, offer_id, type, valeur)
                VALUES ($contractId, $offerId, $type, $valeur);
                """;
            command.Parameters.AddWithValue("$contractId", (object?)contractId ?? DBNull.Value);
            command.Parameters.AddWithValue("$offerId", (object?)offerId ?? DBNull.Value);
            command.Parameters.AddWithValue("$type", clause.Type);
            command.Parameters.AddWithValue("$valeur", clause.Valeur);
            command.ExecuteNonQuery();
        }
    }
}
