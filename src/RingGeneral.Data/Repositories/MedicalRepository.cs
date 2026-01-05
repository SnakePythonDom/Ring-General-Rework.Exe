using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class MedicalRepository : RepositoryBase, IMedicalRepository
{
    private readonly SqliteConnectionFactory _factory;

    public MedicalRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public int AjouterBlessure(Injury injury)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO Injuries (WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes, RiskLevel)
            VALUES ($workerId, $type, $severity, $startDate, $endDate, $isActive, $notes, $risk);
            SELECT last_insert_rowid();
            """;
        AjouterParametre(command, "$workerId", injury.WorkerId);
        AjouterParametre(command, "$type", injury.Type);
        AjouterParametre(command, "$severity", injury.Severity);
        AjouterParametre(command, "$startDate", injury.StartWeek);
        AjouterParametre(command, "$endDate", (object?)injury.EndWeek ?? DBNull.Value);
        AjouterParametre(command, "$isActive", injury.IsActive ? 1 : 0);
        AjouterParametre(command, "$notes", (object?)injury.Notes ?? DBNull.Value);
        AjouterParametre(command, "$risk", injury.RiskLevel);

        var result = command.ExecuteScalar();
        return result is null or DBNull ? 0 : Convert.ToInt32(result);
    }

    public Injury? ChargerBlessure(int injuryId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT InjuryId, WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes, RiskLevel
            FROM Injuries
            WHERE InjuryId = $injuryId;
            """;
        AjouterParametre(command, "$injuryId", injuryId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Injury(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetInt32(4),
            reader.IsDBNull(5) ? null : reader.GetInt32(5),
            reader.GetInt32(6) == 1,
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? 0 : reader.GetDouble(8));
    }

    public void MettreAJourBlessure(Injury injury)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE Injuries
            SET Type = $type,
                Severity = $severity,
                StartDate = $startDate,
                EndDate = $endDate,
                IsActive = $isActive,
                Notes = $notes,
                RiskLevel = $risk
            WHERE InjuryId = $injuryId;
            """;
        AjouterParametre(command, "$injuryId", injury.InjuryId);
        AjouterParametre(command, "$type", injury.Type);
        AjouterParametre(command, "$severity", injury.Severity);
        AjouterParametre(command, "$startDate", injury.StartWeek);
        AjouterParametre(command, "$endDate", (object?)injury.EndWeek ?? DBNull.Value);
        AjouterParametre(command, "$isActive", injury.IsActive ? 1 : 0);
        AjouterParametre(command, "$notes", (object?)injury.Notes ?? DBNull.Value);
        AjouterParametre(command, "$risk", injury.RiskLevel);
        command.ExecuteNonQuery();
    }

    public int AjouterPlan(RecoveryPlan plan)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO RecoveryPlans (InjuryId, WorkerId, StartWeek, TargetWeek, Status, Notes, CompletedWeek)
            VALUES ($injuryId, $workerId, $startWeek, $targetWeek, $status, $notes, $completedWeek);
            SELECT last_insert_rowid();
            """;
        AjouterParametre(command, "$injuryId", plan.InjuryId);
        AjouterParametre(command, "$workerId", plan.WorkerId);
        AjouterParametre(command, "$startWeek", plan.StartWeek);
        AjouterParametre(command, "$targetWeek", plan.TargetWeek);
        AjouterParametre(command, "$status", plan.Status);
        AjouterParametre(command, "$notes", (object?)plan.Notes ?? DBNull.Value);
        AjouterParametre(command, "$completedWeek", (object?)plan.CompletedWeek ?? DBNull.Value);

        var result = command.ExecuteScalar();
        return result is null or DBNull ? 0 : Convert.ToInt32(result);
    }

    public RecoveryPlan? ChargerPlanPourBlessure(int injuryId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT RecoveryPlanId, InjuryId, WorkerId, StartWeek, TargetWeek, Status, Notes, CompletedWeek
            FROM RecoveryPlans
            WHERE InjuryId = $injuryId
            ORDER BY RecoveryPlanId DESC
            LIMIT 1;
            """;
        AjouterParametre(command, "$injuryId", injuryId);

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
            reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.IsDBNull(7) ? null : reader.GetInt32(7));
    }

    public void MettreAJourPlanStatut(int injuryId, string statut, int? completedWeek)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE RecoveryPlans
            SET Status = $status,
                CompletedWeek = $completedWeek
            WHERE InjuryId = $injuryId;
            """;
        AjouterParametre(command, "$injuryId", injuryId);
        AjouterParametre(command, "$status", statut);
        AjouterParametre(command, "$completedWeek", (object?)completedWeek ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void AjouterNote(MedicalNote note)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO MedicalNotes (WorkerId, InjuryId, Week, Content)
            VALUES ($workerId, $injuryId, $week, $content);
            """;
        AjouterParametre(command, "$workerId", note.WorkerId);
        AjouterParametre(command, "$injuryId", (object?)note.InjuryId ?? DBNull.Value);
        AjouterParametre(command, "$week", note.Week);
        AjouterParametre(command, "$content", note.Content);
        command.ExecuteNonQuery();
    }

    public void MettreAJourStatutBlessureWorker(string workerId, string statut)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE Workers
            SET InjuryStatus = $status
            WHERE WorkerId = $workerId;
            """;
        AjouterParametre(command, "$workerId", workerId);
        AjouterParametre(command, "$status", statut);
        command.ExecuteNonQuery();
    }

    public string? ChargerStatutBlessureWorker(string workerId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT InjuryStatus
            FROM Workers
            WHERE WorkerId = $workerId;
            """;
        AjouterParametre(command, "$workerId", workerId);
        var result = command.ExecuteScalar();
        return result is null or DBNull ? null : Convert.ToString(result);
    }
}
