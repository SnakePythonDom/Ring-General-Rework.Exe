using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class MedicalRepository : RepositoryBase
{
    public MedicalRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public int AjouterBlessure(InjuryRecord blessure)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO Injuries (WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes)
            VALUES ($workerId, $type, $severity, $startDate, $endDate, $isActive, $notes);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$workerId", blessure.WorkerId);
        command.Parameters.AddWithValue("$type", blessure.Type);
        command.Parameters.AddWithValue("$severity", (int)blessure.Severity);
        command.Parameters.AddWithValue("$startDate", blessure.StartWeek);
        command.Parameters.AddWithValue("$endDate", blessure.EndWeek.HasValue ? blessure.EndWeek.Value : DBNull.Value);
        command.Parameters.AddWithValue("$isActive", blessure.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$notes", blessure.Notes ?? (object)DBNull.Value);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int AjouterPlanRecuperation(RecoveryPlan plan)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO RecoveryPlans (InjuryId, WorkerId, StartDate, TargetDate, RecommendedRestWeeks, RiskLevel, Status)
            VALUES ($injuryId, $workerId, $startDate, $targetDate, $repos, $risque, $status);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$injuryId", plan.InjuryId);
        command.Parameters.AddWithValue("$workerId", plan.WorkerId);
        command.Parameters.AddWithValue("$startDate", plan.StartWeek);
        command.Parameters.AddWithValue("$targetDate", plan.TargetWeek);
        command.Parameters.AddWithValue("$repos", plan.RecommendedRestWeeks);
        command.Parameters.AddWithValue("$risque", plan.RiskLevel);
        command.Parameters.AddWithValue("$status", plan.Status);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int AjouterNoteMedicale(MedicalNote note)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO MedicalNotes (InjuryId, WorkerId, Note)
            VALUES ($injuryId, $workerId, $note);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$injuryId", note.InjuryId.HasValue ? note.InjuryId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$workerId", note.WorkerId);
        command.Parameters.AddWithValue("$note", note.Note);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public InjuryRecord? ChargerBlessure(int injuryId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT InjuryId, WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes
            FROM Injuries
            WHERE InjuryId = $injuryId;
            """;
        command.Parameters.AddWithValue("$injuryId", injuryId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new InjuryRecord(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            (InjurySeverity)reader.GetInt32(3),
            reader.GetInt32(4),
            reader.IsDBNull(5) ? null : reader.GetInt32(5),
            reader.GetInt32(6) == 1,
            reader.IsDBNull(7) ? null : reader.GetString(7));
    }

    public void MettreAJourBlessure(InjuryRecord blessure)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE Injuries
            SET EndDate = $endDate,
                IsActive = $isActive,
                Notes = $notes
            WHERE InjuryId = $injuryId;
            """;
        command.Parameters.AddWithValue("$endDate", blessure.EndWeek.HasValue ? blessure.EndWeek.Value : DBNull.Value);
        command.Parameters.AddWithValue("$isActive", blessure.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$notes", blessure.Notes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$injuryId", blessure.InjuryId);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Charge toutes les blessures depuis la base de données
    /// </summary>
    public IReadOnlyList<InjuryRecord> ChargerToutesBlessures()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT InjuryId, WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes
            FROM Injuries
            ORDER BY StartDate DESC;
            """;
        
        var injuries = new List<InjuryRecord>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var injury = new InjuryRecord(
                reader.GetInt32(0), // InjuryId
                reader.GetString(1), // WorkerId
                reader.GetString(2), // Type
                (InjurySeverity)reader.GetInt32(3), // Severity
                reader.GetInt32(4), // StartDate (StartWeek)
                reader.IsDBNull(5) ? null : reader.GetInt32(5), // EndDate (EndWeek)
                reader.GetInt32(6) == 1, // IsActive
                reader.IsDBNull(7) ? null : reader.GetString(7) // Notes
            );
            injuries.Add(injury);
        }

        return injuries;
    }

    /// <summary>
    /// Charge toutes les blessures actives pour un worker spécifique
    /// </summary>
    public IReadOnlyList<InjuryRecord> ChargerBlessuresWorker(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT InjuryId, WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes
            FROM Injuries
            WHERE WorkerId = $workerId
            ORDER BY StartDate DESC;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        
        var injuries = new List<InjuryRecord>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var injury = new InjuryRecord(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                (InjurySeverity)reader.GetInt32(3),
                reader.GetInt32(4),
                reader.IsDBNull(5) ? null : reader.GetInt32(5),
                reader.GetInt32(6) == 1,
                reader.IsDBNull(7) ? null : reader.GetString(7)
            );
            injuries.Add(injury);
        }

        return injuries;
    }
}
