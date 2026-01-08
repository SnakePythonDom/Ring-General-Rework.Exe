using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository for managing worker notes, specializations, contracts, and history.
/// </summary>
public sealed class NotesRepository : RepositoryBase, INotesRepository
{
    public NotesRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    // ====================================================================
    // WORKER NOTES
    // ====================================================================

    public WorkerNote? GetNote(int noteId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, Text, Category, CreatedDate, ModifiedDate
            FROM WorkerNotes
            WHERE Id = $id";

        AjouterParametre(command, "$id", noteId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapNote(reader);
        }

        return null;
    }

    public List<WorkerNote> GetNotesForWorker(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, Text, Category, CreatedDate, ModifiedDate
            FROM WorkerNotes
            WHERE WorkerId = $workerId
            ORDER BY ModifiedDate DESC, CreatedDate DESC";

        AjouterParametre(command, "$workerId", workerId);

        var notes = new List<WorkerNote>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            notes.Add(MapNote(reader));
        }

        return notes;
    }

    public List<WorkerNote> GetNotesByCategory(int workerId, NoteCategory category)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, Text, Category, CreatedDate, ModifiedDate
            FROM WorkerNotes
            WHERE WorkerId = $workerId AND Category = $category
            ORDER BY ModifiedDate DESC, CreatedDate DESC";

        AjouterParametre(command, "$workerId", workerId);
        AjouterParametre(command, "$category", category.ToString());

        var notes = new List<WorkerNote>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            notes.Add(MapNote(reader));
        }

        return notes;
    }

    public int CreateNote(WorkerNote note)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT INTO WorkerNotes (WorkerId, Text, Category, CreatedDate, ModifiedDate)
            VALUES ($workerId, $text, $category, $createdDate, $modifiedDate);
            SELECT last_insert_rowid()";

        AjouterParametre(command, "$workerId", note.WorkerId);
        AjouterParametre(command, "$text", note.Text);
        AjouterParametre(command, "$category", note.Category.ToString());
        AjouterParametre(command, "$createdDate", note.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
        AjouterParametre(command, "$modifiedDate", note.ModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss"));

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateNote(WorkerNote note)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            UPDATE WorkerNotes
            SET Text = $text,
                Category = $category,
                ModifiedDate = $modifiedDate
            WHERE Id = $id";

        AjouterParametre(command, "$id", note.Id);
        AjouterParametre(command, "$text", note.Text);
        AjouterParametre(command, "$category", note.Category.ToString());
        AjouterParametre(command, "$modifiedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        command.ExecuteNonQuery();
    }

    public void DeleteNote(int noteId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "DELETE FROM WorkerNotes WHERE Id = $id";
        AjouterParametre(command, "$id", noteId);
        command.ExecuteNonQuery();
    }

    // ====================================================================
    // WORKER SPECIALIZATIONS
    // ====================================================================

    public List<WorkerSpecialization> GetSpecializations(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, Specialization, Level
            FROM WorkerSpecializations
            WHERE WorkerId = $workerId
            ORDER BY Level ASC";

        AjouterParametre(command, "$workerId", workerId);

        var specs = new List<WorkerSpecialization>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            specs.Add(MapSpecialization(reader));
        }

        return specs;
    }

    public WorkerSpecialization? GetPrimarySpecialization(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, Specialization, Level
            FROM WorkerSpecializations
            WHERE WorkerId = $workerId AND Level = 1";

        AjouterParametre(command, "$workerId", workerId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapSpecialization(reader);
        }

        return null;
    }

    public int AddSpecialization(WorkerSpecialization specialization)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT INTO WorkerSpecializations (WorkerId, Specialization, Level)
            VALUES ($workerId, $specialization, $level);
            SELECT last_insert_rowid()";

        AjouterParametre(command, "$workerId", specialization.WorkerId);
        AjouterParametre(command, "$specialization", specialization.Specialization.ToString());
        AjouterParametre(command, "$level", specialization.Level);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateSpecialization(WorkerSpecialization specialization)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            UPDATE WorkerSpecializations
            SET Specialization = $specialization,
                Level = $level
            WHERE Id = $id";

        AjouterParametre(command, "$id", specialization.Id);
        AjouterParametre(command, "$specialization", specialization.Specialization.ToString());
        AjouterParametre(command, "$level", specialization.Level);

        command.ExecuteNonQuery();
    }

    public void DeleteSpecialization(int specializationId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "DELETE FROM WorkerSpecializations WHERE Id = $id";
        AjouterParametre(command, "$id", specializationId);
        command.ExecuteNonQuery();
    }

    public void DeleteAllSpecializations(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "DELETE FROM WorkerSpecializations WHERE WorkerId = $workerId";
        AjouterParametre(command, "$workerId", workerId);
        command.ExecuteNonQuery();
    }

    // ====================================================================
    // CONTRACT HISTORY
    // ====================================================================

    public ContractHistory? GetContract(int contractId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, StartDate, EndDate, WeeklySalary, SigningBonus, ContractType, Status
            FROM ContractHistory
            WHERE Id = $id";

        AjouterParametre(command, "$id", contractId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapContract(reader);
        }

        return null;
    }

    public List<ContractHistory> GetContractHistory(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, StartDate, EndDate, WeeklySalary, SigningBonus, ContractType, Status
            FROM ContractHistory
            WHERE WorkerId = $workerId
            ORDER BY StartDate DESC";

        AjouterParametre(command, "$workerId", workerId);

        var contracts = new List<ContractHistory>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            contracts.Add(MapContract(reader));
        }

        return contracts;
    }

    public ContractHistory? GetActiveContract(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, StartDate, EndDate, WeeklySalary, SigningBonus, ContractType, Status
            FROM ContractHistory
            WHERE WorkerId = $workerId AND Status = 'Active'
            ORDER BY StartDate DESC
            LIMIT 1";

        AjouterParametre(command, "$workerId", workerId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapContract(reader);
        }

        return null;
    }

    public int CreateContract(ContractHistory contract)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT INTO ContractHistory (WorkerId, StartDate, EndDate, WeeklySalary, SigningBonus, ContractType, Status)
            VALUES ($workerId, $startDate, $endDate, $weeklySalary, $signingBonus, $contractType, $status);
            SELECT last_insert_rowid()";

        AjouterParametre(command, "$workerId", contract.WorkerId);
        AjouterParametre(command, "$startDate", contract.StartDate.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$endDate", contract.EndDate.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$weeklySalary", contract.WeeklySalary);
        AjouterParametre(command, "$signingBonus", contract.SigningBonus);
        AjouterParametre(command, "$contractType", contract.ContractType.ToString());
        AjouterParametre(command, "$status", contract.Status.ToString());

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateContract(ContractHistory contract)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            UPDATE ContractHistory
            SET StartDate = $startDate,
                EndDate = $endDate,
                WeeklySalary = $weeklySalary,
                SigningBonus = $signingBonus,
                ContractType = $contractType,
                Status = $status
            WHERE Id = $id";

        AjouterParametre(command, "$id", contract.Id);
        AjouterParametre(command, "$startDate", contract.StartDate.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$endDate", contract.EndDate.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$weeklySalary", contract.WeeklySalary);
        AjouterParametre(command, "$signingBonus", contract.SigningBonus);
        AjouterParametre(command, "$contractType", contract.ContractType.ToString());
        AjouterParametre(command, "$status", contract.Status.ToString());

        command.ExecuteNonQuery();
    }

    public void ExpireContract(int contractId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE ContractHistory SET Status = 'Expired' WHERE Id = $id";
        AjouterParametre(command, "$id", contractId);
        command.ExecuteNonQuery();
    }

    public void TerminateContract(int contractId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE ContractHistory SET Status = 'Terminated' WHERE Id = $id";
        AjouterParametre(command, "$id", contractId);
        command.ExecuteNonQuery();
    }

    public List<ContractHistory> GetExpiringSoonContracts()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, StartDate, EndDate, WeeklySalary, SigningBonus, ContractType, Status
            FROM ContractHistory
            WHERE Status = 'Active'
              AND DATE(EndDate) <= DATE('now', '+30 days')
            ORDER BY EndDate ASC";

        var contracts = new List<ContractHistory>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            contracts.Add(MapContract(reader));
        }

        return contracts;
    }

    // ====================================================================
    // MATCH HISTORY
    // ====================================================================

    public MatchHistoryItem? GetMatch(int matchId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, ShowId, MatchDate, MatchType, OpponentId, Result, Rating, Duration
            FROM MatchHistory
            WHERE Id = $id";

        AjouterParametre(command, "$id", matchId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapMatch(reader);
        }

        return null;
    }

    public List<MatchHistoryItem> GetMatchHistory(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, ShowId, MatchDate, MatchType, OpponentId, Result, Rating, Duration
            FROM MatchHistory
            WHERE WorkerId = $workerId
            ORDER BY MatchDate DESC";

        AjouterParametre(command, "$workerId", workerId);

        var matches = new List<MatchHistoryItem>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            matches.Add(MapMatch(reader));
        }

        return matches;
    }

    public List<MatchHistoryItem> GetRecentMatches(int workerId, int limit)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, ShowId, MatchDate, MatchType, OpponentId, Result, Rating, Duration
            FROM MatchHistory
            WHERE WorkerId = $workerId
            ORDER BY MatchDate DESC
            LIMIT $limit";

        AjouterParametre(command, "$workerId", workerId);
        AjouterParametre(command, "$limit", limit);

        var matches = new List<MatchHistoryItem>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            matches.Add(MapMatch(reader));
        }

        return matches;
    }

    public int AddMatch(MatchHistoryItem match)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT INTO MatchHistory (WorkerId, ShowId, MatchDate, MatchType, OpponentId, Result, Rating, Duration)
            VALUES ($workerId, $showId, $matchDate, $matchType, $opponentId, $result, $rating, $duration);
            SELECT last_insert_rowid()";

        AjouterParametre(command, "$workerId", match.WorkerId);
        AjouterParametre(command, "$showId", match.ShowId);
        AjouterParametre(command, "$matchDate", match.MatchDate.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$matchType", match.MatchType);
        AjouterParametre(command, "$opponentId", match.OpponentId);
        AjouterParametre(command, "$result", match.Result?.ToString());
        AjouterParametre(command, "$rating", match.Rating);
        AjouterParametre(command, "$duration", match.Duration);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateMatch(MatchHistoryItem match)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            UPDATE MatchHistory
            SET ShowId = $showId,
                MatchDate = $matchDate,
                MatchType = $matchType,
                OpponentId = $opponentId,
                Result = $result,
                Rating = $rating,
                Duration = $duration
            WHERE Id = $id";

        AjouterParametre(command, "$id", match.Id);
        AjouterParametre(command, "$showId", match.ShowId);
        AjouterParametre(command, "$matchDate", match.MatchDate.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$matchType", match.MatchType);
        AjouterParametre(command, "$opponentId", match.OpponentId);
        AjouterParametre(command, "$result", match.Result?.ToString());
        AjouterParametre(command, "$rating", match.Rating);
        AjouterParametre(command, "$duration", match.Duration);

        command.ExecuteNonQuery();
    }

    public void DeleteMatch(int matchId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "DELETE FROM MatchHistory WHERE Id = $id";
        AjouterParametre(command, "$id", matchId);
        command.ExecuteNonQuery();
    }

    public (int TotalMatches, int Wins, int Losses, int Draws, double WinPercentage) GetMatchStats(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT
                COUNT(*) as TotalMatches,
                SUM(CASE WHEN Result = 'Win' THEN 1 ELSE 0 END) as Wins,
                SUM(CASE WHEN Result = 'Loss' THEN 1 ELSE 0 END) as Losses,
                SUM(CASE WHEN Result = 'Draw' THEN 1 ELSE 0 END) as Draws
            FROM MatchHistory
            WHERE WorkerId = $workerId";

        AjouterParametre(command, "$workerId", workerId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            int total = reader.GetInt32(0);
            int wins = reader.GetInt32(1);
            int losses = reader.GetInt32(2);
            int draws = reader.GetInt32(3);
            double winPct = total > 0 ? Math.Round((double)wins / total * 100, 1) : 0;

            return (total, wins, losses, draws, winPct);
        }

        return (0, 0, 0, 0, 0);
    }

    // ====================================================================
    // TITLE REIGNS
    // ====================================================================

    public TitleReign? GetTitleReign(int reignId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, TitleId, WonDate, WonShowId, LostDate, LostShowId, DaysHeld, ReignNumber
            FROM TitleReigns
            WHERE Id = $id";

        AjouterParametre(command, "$id", reignId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapTitleReign(reader);
        }

        return null;
    }

    public List<TitleReign> GetTitleReigns(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, TitleId, WonDate, WonShowId, LostDate, LostShowId, DaysHeld, ReignNumber
            FROM TitleReigns
            WHERE WorkerId = $workerId
            ORDER BY WonDate DESC";

        AjouterParametre(command, "$workerId", workerId);

        var reigns = new List<TitleReign>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            reigns.Add(MapTitleReign(reader));
        }

        return reigns;
    }

    public List<TitleReign> GetCurrentTitleReigns(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId, TitleId, WonDate, WonShowId, LostDate, LostShowId, DaysHeld, ReignNumber
            FROM TitleReigns
            WHERE WorkerId = $workerId AND LostDate IS NULL
            ORDER BY WonDate DESC";

        AjouterParametre(command, "$workerId", workerId);

        var reigns = new List<TitleReign>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            reigns.Add(MapTitleReign(reader));
        }

        return reigns;
    }

    public int AddTitleReign(TitleReign reign)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT INTO TitleReigns (WorkerId, TitleId, WonDate, WonShowId, LostDate, LostShowId, DaysHeld, ReignNumber)
            VALUES ($workerId, $titleId, $wonDate, $wonShowId, $lostDate, $lostShowId, $daysHeld, $reignNumber);
            SELECT last_insert_rowid()";

        AjouterParametre(command, "$workerId", reign.WorkerId);
        AjouterParametre(command, "$titleId", reign.TitleId);
        AjouterParametre(command, "$wonDate", reign.WonDate.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$wonShowId", reign.WonShowId);
        AjouterParametre(command, "$lostDate", reign.LostDate?.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$lostShowId", reign.LostShowId);
        AjouterParametre(command, "$daysHeld", reign.DaysHeld);
        AjouterParametre(command, "$reignNumber", reign.ReignNumber);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateTitleReign(TitleReign reign)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            UPDATE TitleReigns
            SET WonDate = $wonDate,
                WonShowId = $wonShowId,
                LostDate = $lostDate,
                LostShowId = $lostShowId,
                DaysHeld = $daysHeld,
                ReignNumber = $reignNumber
            WHERE Id = $id";

        AjouterParametre(command, "$id", reign.Id);
        AjouterParametre(command, "$wonDate", reign.WonDate.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$wonShowId", reign.WonShowId);
        AjouterParametre(command, "$lostDate", reign.LostDate?.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$lostShowId", reign.LostShowId);
        AjouterParametre(command, "$daysHeld", reign.DaysHeld);
        AjouterParametre(command, "$reignNumber", reign.ReignNumber);

        command.ExecuteNonQuery();
    }

    public void EndTitleReign(int reignId, DateTime lostDate, int? lostShowId = null)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            UPDATE TitleReigns
            SET LostDate = $lostDate,
                LostShowId = $lostShowId,
                DaysHeld = JULIANDAY($lostDate) - JULIANDAY(WonDate)
            WHERE Id = $id";

        AjouterParametre(command, "$id", reignId);
        AjouterParametre(command, "$lostDate", lostDate.ToString("yyyy-MM-dd"));
        AjouterParametre(command, "$lostShowId", lostShowId);

        command.ExecuteNonQuery();
    }

    public void DeleteTitleReign(int reignId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "DELETE FROM TitleReigns WHERE Id = $id";
        AjouterParametre(command, "$id", reignId);
        command.ExecuteNonQuery();
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private static WorkerNote MapNote(SqliteDataReader reader)
    {
        return new WorkerNote
        {
            Id = reader.GetInt32(0),
            WorkerId = reader.GetInt32(1),
            Text = reader.GetString(2),
            Category = Enum.Parse<NoteCategory>(reader.GetString(3)),
            CreatedDate = DateTime.Parse(reader.GetString(4)),
            ModifiedDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5))
        };
    }

    private static WorkerSpecialization MapSpecialization(SqliteDataReader reader)
    {
        return new WorkerSpecialization
        {
            Id = reader.GetInt32(0),
            WorkerId = reader.GetInt32(1),
            Specialization = Enum.Parse<SpecializationType>(reader.GetString(2)),
            Level = reader.GetInt32(3)
        };
    }

    private static ContractHistory MapContract(SqliteDataReader reader)
    {
        return new ContractHistory
        {
            Id = reader.GetInt32(0),
            WorkerId = reader.GetInt32(1),
            StartDate = DateTime.Parse(reader.GetString(2)),
            EndDate = DateTime.Parse(reader.GetString(3)),
            WeeklySalary = reader.GetDecimal(4),
            SigningBonus = reader.GetDecimal(5),
            ContractType = Enum.Parse<ContractType>(reader.GetString(6)),
            Status = Enum.Parse<ContractStatus>(reader.GetString(7))
        };
    }

    private static MatchHistoryItem MapMatch(SqliteDataReader reader)
    {
        return new MatchHistoryItem
        {
            Id = reader.GetInt32(0),
            WorkerId = reader.GetInt32(1),
            ShowId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
            MatchDate = DateTime.Parse(reader.GetString(3)),
            MatchType = reader.IsDBNull(4) ? null : reader.GetString(4),
            OpponentId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            Result = reader.IsDBNull(6) ? null : Enum.Parse<MatchResult>(reader.GetString(6)),
            Rating = reader.IsDBNull(7) ? null : reader.GetInt32(7),
            Duration = reader.IsDBNull(8) ? null : reader.GetInt32(8)
        };
    }

    private static TitleReign MapTitleReign(SqliteDataReader reader)
    {
        return new TitleReign
        {
            Id = reader.GetInt32(0),
            WorkerId = reader.GetInt32(1),
            TitleId = reader.GetInt32(2),
            WonDate = DateTime.Parse(reader.GetString(3)),
            WonShowId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            LostDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
            LostShowId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
            DaysHeld = reader.IsDBNull(7) ? null : reader.GetInt32(7),
            ReignNumber = reader.GetInt32(8)
        };
    }
}
