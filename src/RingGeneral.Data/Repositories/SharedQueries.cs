using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Helpers métier partagés entre plusieurs domaines (Catégorie C).
/// Liste fermée, pas de nouvelle logique.
/// </summary>
internal static class SharedQueries
{
    internal static ShowDefinition? ChargerShow(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT show_id, nom, semaine, region, duree, compagnie_id, tv_deal_id, lieu, diffusion
            FROM shows
            WHERE show_id = $showId;
            """;
        command.Parameters.AddWithValue("$showId", showId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var lieu = reader.IsDBNull(7) ? reader.GetString(3) : reader.GetString(7);
        if (string.IsNullOrWhiteSpace(lieu))
        {
            lieu = reader.GetString(3);
        }

        var diffusion = reader.IsDBNull(8) ? "Non défini" : reader.GetString(8);
        if (string.IsNullOrWhiteSpace(diffusion))
        {
            diffusion = "Non défini";
        }

        return new ShowDefinition(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetInt32(2),
            reader.GetString(3),
            reader.GetInt32(4),
            reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            lieu,
            diffusion);
    }

    internal static CompanyState? ChargerCompagnie(SqliteConnection connexion, string companyId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT company_id, nom, region, prestige, tresorerie, audience_moyenne, reach
            FROM companies
            WHERE company_id = $companyId;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new CompanyState(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetDouble(4),
            reader.GetInt32(5),
            reader.GetInt32(6));
    }

    internal static TvDeal? ChargerTvDeal(SqliteConnection connexion, string? dealId)
    {
        if (string.IsNullOrWhiteSpace(dealId))
        {
            return null;
        }

        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT tv_deal_id, company_id, network_name, reach_bonus, audience_cap, audience_min,
                   base_revenue, revenue_per_point, penalty, constraints
            FROM tv_deals
            WHERE tv_deal_id = $dealId;
            """;
        command.Parameters.AddWithValue("$dealId", dealId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new TvDeal
        {
            TvDealId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            NetworkName = reader.GetString(2),
            ReachBonus = reader.GetInt32(3),
            AudienceCap = reader.GetInt32(4),
            MinimumAudience = reader.GetInt32(5),
            BaseRevenue = reader.GetDouble(6),
            RevenuePerPoint = reader.GetDouble(7),
            Penalty = reader.GetDouble(8),
            Constraints = reader.GetString(9)
        };
    }

    internal static List<WorkerSnapshot> ChargerWorkers(SqliteConnection connexion, IReadOnlyList<string> workerIds)
    {
        if (workerIds.Count == 0)
        {
            return new List<WorkerSnapshot>();
        }

        using var command = connexion.CreateCommand();
        var placeholders = workerIds.Select((id, index) => $"$id{index}").ToList();
        command.CommandText = $"""
            SELECT worker_id, nom || ' ' || prenom, in_ring, entertainment, story, popularite, fatigue, blessure, momentum, role_tv, morale
            FROM workers
            WHERE worker_id IN ({string.Join(", ", placeholders)});
            """;
        for (var i = 0; i < workerIds.Count; i++)
        {
            command.Parameters.AddWithValue(placeholders[i], workerIds[i]);
        }

        using var reader = command.ExecuteReader();
        var workers = new List<WorkerSnapshot>();
        while (reader.Read())
        {
            var nomComplet = reader.GetString(1);
            workers.Add(new WorkerSnapshot(
                reader.GetString(0),
                nomComplet,
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetString(7),
                reader.GetInt32(8),
                reader.GetString(9),
                reader.GetInt32(10)));
        }

        return workers;
    }

    internal static List<TitleInfo> ChargerTitres(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT title_id, nom, prestige, detenteur_id FROM titles;";
        using var reader = command.ExecuteReader();
        var titres = new List<TitleInfo>();
        while (reader.Read())
        {
            titres.Add(new TitleInfo(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetString(3)));
        }

        return titres;
    }

    internal static List<StorylineInfo> ChargerStorylines(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT storyline_id, nom, heat FROM storylines;";
        using var reader = command.ExecuteReader();
        var storylines = new List<StorylineInfo>();
        while (reader.Read())
        {
            var storylineId = reader.GetString(0);
            storylines.Add(new StorylineInfo(
                storylineId,
                reader.GetString(1),
                StorylinePhase.Setup,
                reader.GetInt32(2),
                StorylineStatus.Active,
                null,
                ChargerStorylineParticipants(connexion, storylineId)));
        }

        return storylines;
    }

    internal static List<StorylineParticipant> ChargerStorylineParticipants(SqliteConnection connexion, string storylineId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_id, role FROM storyline_participants WHERE storyline_id = $storylineId;";
        command.Parameters.AddWithValue("$storylineId", storylineId);
        using var reader = command.ExecuteReader();
        var participants = new List<StorylineParticipant>();
        while (reader.Read())
        {
            participants.Add(new StorylineParticipant(
                reader.GetString(0),
                reader.IsDBNull(1) ? "principal" : reader.GetString(1)));
        }

        return participants;
    }

    internal static void MettreAJourStorylineParticipants(
        SqliteConnection connexion,
        SqliteTransaction transaction,
        string storylineId,
        IReadOnlyList<StorylineParticipant> participants)
    {
        using var deleteCommand = connexion.CreateCommand();
        deleteCommand.Transaction = transaction;
        deleteCommand.CommandText = "DELETE FROM storyline_participants WHERE storyline_id = $storylineId;";
        deleteCommand.Parameters.AddWithValue("$storylineId", storylineId);
        deleteCommand.ExecuteNonQuery();

        foreach (var participant in participants)
        {
            using var insertCommand = connexion.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO storyline_participants (storyline_id, worker_id, role)
                VALUES ($storylineId, $workerId, $role);
                """;
            insertCommand.Parameters.AddWithValue("$storylineId", storylineId);
            insertCommand.Parameters.AddWithValue("$workerId", participant.WorkerId);
            insertCommand.Parameters.AddWithValue("$role", participant.Role);
            insertCommand.ExecuteNonQuery();
        }
    }

    internal static Dictionary<string, int> ChargerChimies(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_a, worker_b, valeur FROM chimies;";
        using var reader = command.ExecuteReader();
        var chimies = new Dictionary<string, int>();
        while (reader.Read())
        {
            chimies[$"{reader.GetString(0)}|{reader.GetString(1)}"] = reader.GetInt32(2);
        }

        return chimies;
    }

    internal static string ChargerCompanyIdPourWorker(SqliteConnection connexion, string workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT company_id FROM workers WHERE worker_id = $workerId;";
        command.Parameters.AddWithValue("$workerId", workerId);
        return Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
    }

    internal static int DeterminerSeveriteBlessure(string blessure)
    {
        return blessure.ToUpperInvariant() switch
        {
            "LEGERE" => 1,
            "MOYENNE" => 2,
            "GRAVE" => 3,
            _ => 1
        };
    }

    internal static int MapperGraviteDiscipline(string actionType)
    {
        return actionType switch
        {
            "SUSPENSION" => 3,
            "AMENDE" => 2,
            "AVERTISSEMENT" => 1,
            _ => 1
        };
    }
}
