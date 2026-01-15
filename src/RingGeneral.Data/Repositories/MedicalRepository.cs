using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace RingGeneral.Data.Repositories;

public sealed class MedicalRepository : RepositoryBase, IMedicalRepository
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

    public int AjouterPlan(RecoveryPlan plan)
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

    public void AjouterNote(MedicalNote note)
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
        command.ExecuteNonQuery();
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

    public void SupprimerBlessure(int injuryId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "DELETE FROM Injuries WHERE InjuryId = $injuryId;";
        command.Parameters.AddWithValue("$injuryId", injuryId);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Charge toutes les blessures depuis la base de données
    /// </summary>
    /// <summary>
    /// Charge toutes les blessures depuis la base de données
    /// </summary>
    public List<InjuryRecord> ChargerToutesBlessures()
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
    public RecoveryPlan? ChargerPlanPourBlessure(int injuryId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT RecoveryPlanId, InjuryId, WorkerId, StartDate, TargetDate, RecommendedRestWeeks, RiskLevel, Status, CreatedAt
            FROM RecoveryPlans
            WHERE InjuryId = $injuryId;
            """;
        command.Parameters.AddWithValue("$injuryId", injuryId);
        
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new RecoveryPlan(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetInt32(4),
            reader.GetInt32(5),
            reader.GetString(6),
            reader.GetString(7),
            DateTimeOffset.Parse(reader.GetString(8))
        );
    }

    public void MettreAJourPlanStatut(int injuryId, string statut, int? completedWeek)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        // Note: completedWeek is not currently stored in the schema
        command.CommandText = "UPDATE RecoveryPlans SET Status = $statut WHERE InjuryId = $injuryId;";
        command.Parameters.AddWithValue("$statut", statut);
        command.Parameters.AddWithValue("$injuryId", injuryId);
        command.ExecuteNonQuery();
    }

    public void MettreAJourStatutBlessureWorker(string workerId, string statut)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE Workers SET blessure = $statut WHERE WorkerId = $workerId;";
        command.Parameters.AddWithValue("$statut", statut);
        command.Parameters.AddWithValue("$workerId", workerId);
        command.ExecuteNonQuery();
    }

    public string? ChargerStatutBlessureWorker(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT blessure FROM Workers WHERE WorkerId = $workerId;";
        command.Parameters.AddWithValue("$workerId", workerId);
        return command.ExecuteScalar()?.ToString();
    }

    /// <summary>
    /// Charge toutes les blessures actives pour un worker spécifique
    /// </summary>
    public List<InjuryRecord> ChargerBlessuresWorker(string workerId)
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
