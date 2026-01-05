using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class TitleRepository : RepositoryBase, ITitleRepository, IContenderRankingRepository
{
    private readonly SqliteConnectionFactory _factory;

    public TitleRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public TitleRecord? ChargerTitre(string titleId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT TitleId, CompanyId, Name, Prestige, HolderWorkerId
            FROM Titles
            WHERE TitleId = $titleId;
            """;
        command.Parameters.AddWithValue("$titleId", titleId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new TitleRecord(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.IsDBNull(4) ? null : reader.GetString(4));
    }

    public IReadOnlyList<TitleRecord> ChargerTitresCompagnie(string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT TitleId, CompanyId, Name, Prestige, HolderWorkerId
            FROM Titles
            WHERE CompanyId = $companyId;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var titres = new List<TitleRecord>();
        while (reader.Read())
        {
            titres.Add(new TitleRecord(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetString(4)));
        }

        return titres;
    }

    public void CreerTitre(TitleRecord title)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO Titles (TitleId, CompanyId, Name, Prestige, HolderWorkerId)
            VALUES ($titleId, $companyId, $name, $prestige, $holderWorkerId);
            """;
        command.Parameters.AddWithValue("$titleId", title.TitleId);
        command.Parameters.AddWithValue("$companyId", title.CompanyId);
        command.Parameters.AddWithValue("$name", title.Name);
        command.Parameters.AddWithValue("$prestige", title.Prestige);
        AjouterParametre(command, "$holderWorkerId", title.HolderWorkerId);
        command.ExecuteNonQuery();
    }

    public void MettreAJourTitre(TitleRecord title)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE Titles
            SET Name = $name,
                Prestige = $prestige,
                HolderWorkerId = $holderWorkerId
            WHERE TitleId = $titleId;
            """;
        command.Parameters.AddWithValue("$titleId", title.TitleId);
        command.Parameters.AddWithValue("$name", title.Name);
        command.Parameters.AddWithValue("$prestige", title.Prestige);
        AjouterParametre(command, "$holderWorkerId", title.HolderWorkerId);
        command.ExecuteNonQuery();
    }

    public void SupprimerTitre(string titleId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();

        using var deleteContenders = connexion.CreateCommand();
        deleteContenders.Transaction = transaction;
        deleteContenders.CommandText = "DELETE FROM ContenderRankings WHERE TitleId = $titleId;";
        deleteContenders.Parameters.AddWithValue("$titleId", titleId);
        deleteContenders.ExecuteNonQuery();

        using var deleteMatches = connexion.CreateCommand();
        deleteMatches.Transaction = transaction;
        deleteMatches.CommandText = "DELETE FROM TitleMatches WHERE TitleId = $titleId;";
        deleteMatches.Parameters.AddWithValue("$titleId", titleId);
        deleteMatches.ExecuteNonQuery();

        using var deleteReigns = connexion.CreateCommand();
        deleteReigns.Transaction = transaction;
        deleteReigns.CommandText = "DELETE FROM TitleReigns WHERE TitleId = $titleId;";
        deleteReigns.Parameters.AddWithValue("$titleId", titleId);
        deleteReigns.ExecuteNonQuery();

        using var deleteTitle = connexion.CreateCommand();
        deleteTitle.Transaction = transaction;
        deleteTitle.CommandText = "DELETE FROM Titles WHERE TitleId = $titleId;";
        deleteTitle.Parameters.AddWithValue("$titleId", titleId);
        deleteTitle.ExecuteNonQuery();

        transaction.Commit();
    }

    public TitleReignRecord? ChargerRegneActuel(string titleId)
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

        return new TitleReignRecord(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.IsDBNull(4) ? null : reader.GetInt32(4),
            reader.GetInt32(5) == 1);
    }

    public IReadOnlyList<TitleReignRecord> ChargerRegnes(string titleId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT TitleReignId, TitleId, WorkerId, StartDate, EndDate, IsCurrent
            FROM TitleReigns
            WHERE TitleId = $titleId
            ORDER BY StartDate DESC;
            """;
        command.Parameters.AddWithValue("$titleId", titleId);
        using var reader = command.ExecuteReader();
        var regnes = new List<TitleReignRecord>();
        while (reader.Read())
        {
            regnes.Add(new TitleReignRecord(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetInt32(4),
                reader.GetInt32(5) == 1));
        }

        return regnes;
    }

    public int AjouterRegne(TitleReignRecord reign)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO TitleReigns (TitleId, WorkerId, StartDate, EndDate, IsCurrent)
            VALUES ($titleId, $workerId, $startDate, $endDate, $isCurrent);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$titleId", reign.TitleId);
        command.Parameters.AddWithValue("$workerId", reign.WorkerId);
        command.Parameters.AddWithValue("$startDate", reign.StartDate);
        AjouterParametre(command, "$endDate", reign.EndDate);
        command.Parameters.AddWithValue("$isCurrent", reign.IsCurrent ? 1 : 0);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void CloreRegne(int reignId, int endDate)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE TitleReigns
            SET EndDate = $endDate,
                IsCurrent = 0
            WHERE TitleReignId = $reignId;
            """;
        command.Parameters.AddWithValue("$reignId", reignId);
        command.Parameters.AddWithValue("$endDate", endDate);
        command.ExecuteNonQuery();
    }

    public void MettreAJourDetenteur(string titleId, string? workerId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE Titles
            SET HolderWorkerId = $holderWorkerId
            WHERE TitleId = $titleId;
            """;
        command.Parameters.AddWithValue("$titleId", titleId);
        AjouterParametre(command, "$holderWorkerId", workerId);
        command.ExecuteNonQuery();
    }

    public void AjouterMatchTitre(TitleMatchRecord match)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO TitleMatches (
                TitleId,
                ChampionWorkerId,
                ChallengerWorkerId,
                WinnerWorkerId,
                LoserWorkerId,
                Week,
                ShowId,
                SegmentId,
                IsTitleChange
            )
            VALUES (
                $titleId,
                $championId,
                $challengerId,
                $winnerId,
                $loserId,
                $week,
                $showId,
                $segmentId,
                $isTitleChange
            );
            """;
        command.Parameters.AddWithValue("$titleId", match.TitleId);
        command.Parameters.AddWithValue("$championId", match.ChampionWorkerId);
        command.Parameters.AddWithValue("$challengerId", match.ChallengerWorkerId);
        command.Parameters.AddWithValue("$winnerId", match.WinnerWorkerId);
        command.Parameters.AddWithValue("$loserId", match.LoserWorkerId);
        command.Parameters.AddWithValue("$week", match.Week);
        AjouterParametre(command, "$showId", match.ShowId);
        AjouterParametre(command, "$segmentId", match.SegmentId);
        command.Parameters.AddWithValue("$isTitleChange", match.IsTitleChange ? 1 : 0);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<TitleMatchRecord> ChargerMatchsTitrePourWorker(string workerId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT TitleMatchId, TitleId, ChampionWorkerId, ChallengerWorkerId, WinnerWorkerId, LoserWorkerId, Week, ShowId, SegmentId, IsTitleChange
            FROM TitleMatches
            WHERE ChampionWorkerId = $workerId
               OR ChallengerWorkerId = $workerId
               OR WinnerWorkerId = $workerId
               OR LoserWorkerId = $workerId
            ORDER BY Week DESC, TitleMatchId DESC;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        using var reader = command.ExecuteReader();
        var matches = new List<TitleMatchRecord>();
        while (reader.Read())
        {
            matches.Add(new TitleMatchRecord(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.GetInt32(9) == 1));
        }

        return matches;
    }

    public void AjusterPrestige(string titleId, int delta)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE Titles
            SET Prestige = MAX(0, MIN(100, Prestige + $delta))
            WHERE TitleId = $titleId;
            """;
        command.Parameters.AddWithValue("$titleId", titleId);
        command.Parameters.AddWithValue("$delta", delta);
        command.ExecuteNonQuery();
    }

    public void RemplacerClassement(string titleId, int week, IReadOnlyList<ContenderRankingEntry> entries)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();

        using var deleteCommand = connexion.CreateCommand();
        deleteCommand.Transaction = transaction;
        deleteCommand.CommandText = "DELETE FROM ContenderRankings WHERE TitleId = $titleId;";
        deleteCommand.Parameters.AddWithValue("$titleId", titleId);
        deleteCommand.ExecuteNonQuery();

        foreach (var entry in entries)
        {
            using var insertCommand = connexion.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO ContenderRankings (TitleId, WorkerId, Rank, Score, Week)
                VALUES ($titleId, $workerId, $rank, $score, $week);
                """;
            insertCommand.Parameters.AddWithValue("$titleId", titleId);
            insertCommand.Parameters.AddWithValue("$workerId", entry.WorkerId);
            insertCommand.Parameters.AddWithValue("$rank", entry.Rank);
            insertCommand.Parameters.AddWithValue("$score", entry.Score);
            insertCommand.Parameters.AddWithValue("$week", week);
            insertCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public IReadOnlyList<ContenderRankingEntry> ChargerClassement(string titleId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT TitleId, WorkerId, Rank, Score, Week
            FROM ContenderRankings
            WHERE TitleId = $titleId
            ORDER BY Rank ASC;
            """;
        command.Parameters.AddWithValue("$titleId", titleId);
        using var reader = command.ExecuteReader();
        var classement = new List<ContenderRankingEntry>();
        while (reader.Read())
        {
            classement.Add(new ContenderRankingEntry(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4)));
        }

        return classement;
    }
}
