using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class TitleRepository : ITitleRepository, IContenderRepository
{
    private readonly SqliteConnectionFactory _factory;

    public TitleRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public TitleDetail? ChargerTitre(string titleId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT TitleId, CompanyId, Prestige, HolderWorkerId FROM Titles WHERE TitleId = $titleId;";
        command.Parameters.AddWithValue("$titleId", titleId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new TitleDetail(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetInt32(2),
            reader.IsDBNull(3) ? null : reader.GetString(3));
    }

    public TitleReignDetail? ChargerRegneCourant(string titleId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT TitleReignId, TitleId, WorkerId, StartDate, EndDate, IsCurrent
            FROM TitleReigns
            WHERE TitleId = $titleId AND IsCurrent = 1
            ORDER BY StartDate DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$titleId", titleId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new TitleReignDetail(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.IsDBNull(4) ? null : reader.GetInt32(4),
            reader.GetInt32(5) == 1);
    }

    public int CompterDefenses(string titleId, int depuisSemaine)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT COUNT(1)
            FROM TitleMatches
            WHERE TitleId = $titleId AND Week >= $semaine AND IsTitleChange = 0;
            """;
        command.Parameters.AddWithValue("$titleId", titleId);
        command.Parameters.AddWithValue("$semaine", depuisSemaine);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void MettreAJourChampion(string titleId, string? workerId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE Titles SET HolderWorkerId = $workerId WHERE TitleId = $titleId;";
        command.Parameters.AddWithValue("$titleId", titleId);
        command.Parameters.AddWithValue("$workerId", (object?)workerId ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public int CreerRegne(string titleId, string workerId, int semaineDebut)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO TitleReigns (TitleId, WorkerId, StartDate, IsCurrent)
            VALUES ($titleId, $workerId, $startDate, 1);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$titleId", titleId);
        command.Parameters.AddWithValue("$workerId", workerId);
        command.Parameters.AddWithValue("$startDate", semaineDebut);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void CloreRegne(int titleReignId, int semaineFin)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE TitleReigns
            SET EndDate = $endDate,
                IsCurrent = 0
            WHERE TitleReignId = $reignId;
            """;
        command.Parameters.AddWithValue("$endDate", semaineFin);
        command.Parameters.AddWithValue("$reignId", titleReignId);
        command.ExecuteNonQuery();
    }

    public void AjouterMatch(TitleMatchRecord match)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO TitleMatches (TitleId, ShowId, Week, ChampionId, ChallengerId, WinnerId, IsTitleChange, PrestigeDelta)
            VALUES ($titleId, $showId, $week, $championId, $challengerId, $winnerId, $isTitleChange, $prestigeDelta);
            """;
        command.Parameters.AddWithValue("$titleId", match.TitleId);
        command.Parameters.AddWithValue("$showId", (object?)match.ShowId ?? DBNull.Value);
        command.Parameters.AddWithValue("$week", match.Week);
        command.Parameters.AddWithValue("$championId", (object?)match.ChampionId ?? DBNull.Value);
        command.Parameters.AddWithValue("$challengerId", match.ChallengerId);
        command.Parameters.AddWithValue("$winnerId", match.WinnerId);
        command.Parameters.AddWithValue("$isTitleChange", match.IsTitleChange ? 1 : 0);
        command.Parameters.AddWithValue("$prestigeDelta", match.PrestigeDelta);
        command.ExecuteNonQuery();
    }

    public void MettreAJourPrestige(string titleId, int delta)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE Titles SET Prestige = MAX(0, MIN(100, Prestige + $delta)) WHERE TitleId = $titleId;";
        command.Parameters.AddWithValue("$delta", delta);
        command.Parameters.AddWithValue("$titleId", titleId);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<WorkerSnapshot> ChargerWorkersCompagnie(string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT WorkerId, Name, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv, Morale
            FROM Workers
            WHERE CompanyId = $companyId;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var workers = new List<WorkerSnapshot>();
        while (reader.Read())
        {
            workers.Add(new WorkerSnapshot(
                reader.GetString(0),
                reader.GetString(1),
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

    public void EnregistrerClassement(string titleId, IReadOnlyList<ContenderRanking> classements)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();

        using (var deleteCommand = connexion.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM ContenderRankings WHERE TitleId = $titleId;";
            deleteCommand.Parameters.AddWithValue("$titleId", titleId);
            deleteCommand.ExecuteNonQuery();
        }

        foreach (var classement in classements)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO ContenderRankings (TitleId, WorkerId, Rank, Score, Reason)
                VALUES ($titleId, $workerId, $rank, $score, $reason);
                """;
            command.Parameters.AddWithValue("$titleId", classement.TitleId);
            command.Parameters.AddWithValue("$workerId", classement.WorkerId);
            command.Parameters.AddWithValue("$rank", classement.Rank);
            command.Parameters.AddWithValue("$score", classement.Score);
            command.Parameters.AddWithValue("$reason", classement.Reason);
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
