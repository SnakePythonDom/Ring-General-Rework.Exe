using System.Text.Json;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class BackstageRepository : RepositoryBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public BackstageRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public void AjouterIncident(BackstageIncident incident)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO BackstageIncidents (IncidentId, WorkerId, IncidentType, Description, Severity, Week, Status)
            VALUES ($id, $workerId, $type, $description, $severity, $week, $status);
            """;
        command.Parameters.AddWithValue("$id", incident.IncidentId);
        command.Parameters.AddWithValue("$workerId", incident.WorkerId);
        command.Parameters.AddWithValue("$type", incident.IncidentType);
        command.Parameters.AddWithValue("$description", incident.Description);
        command.Parameters.AddWithValue("$severity", incident.Severity);
        command.Parameters.AddWithValue("$week", incident.Week);
        command.Parameters.AddWithValue("$status", incident.Status);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<BackstageIncident> ChargerIncidents()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT IncidentId, WorkerId, IncidentType, Description, Severity, Week, Status
            FROM BackstageIncidents
            ORDER BY Week DESC, IncidentId DESC;
            """;
        using var reader = command.ExecuteReader();
        var incidents = new List<BackstageIncident>();
        while (reader.Read())
        {
            incidents.Add(new BackstageIncident(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetString(6)));
        }

        return incidents;
    }

    public void AjouterActionDisciplinaire(DisciplinaryAction action)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO DisciplinaryActions (ActionId, IncidentId, WorkerId, ActionType, MoraleDelta, Week, Notes)
            VALUES ($id, $incidentId, $workerId, $actionType, $moraleDelta, $week, $notes);
            """;
        command.Parameters.AddWithValue("$id", action.ActionId);
        command.Parameters.AddWithValue("$incidentId", action.IncidentId);
        command.Parameters.AddWithValue("$workerId", action.WorkerId);
        command.Parameters.AddWithValue("$actionType", action.ActionType);
        command.Parameters.AddWithValue("$moraleDelta", action.MoraleDelta);
        command.Parameters.AddWithValue("$week", action.Week);
        command.Parameters.AddWithValue("$notes", action.Notes ?? string.Empty);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<DisciplinaryAction> ChargerActions(string? incidentId = null)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = incidentId is null
            ? """
                SELECT ActionId, IncidentId, WorkerId, ActionType, MoraleDelta, Week, Notes
                FROM DisciplinaryActions
                ORDER BY Week DESC, ActionId DESC;
                """
            : """
                SELECT ActionId, IncidentId, WorkerId, ActionType, MoraleDelta, Week, Notes
                FROM DisciplinaryActions
                WHERE IncidentId = $incidentId
                ORDER BY Week DESC, ActionId DESC;
                """;
        if (incidentId is not null)
        {
            command.Parameters.AddWithValue("$incidentId", incidentId);
        }

        using var reader = command.ExecuteReader();
        var actions = new List<DisciplinaryAction>();
        while (reader.Read())
        {
            actions.Add(new DisciplinaryAction(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.IsDBNull(6) ? null : reader.GetString(6)));
        }

        return actions;
    }

    public void AjouterMoraleHistorique(MoraleHistoryEntry entry)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO MoraleHistory (WorkerId, Week, Delta, Value, Reason, IncidentId)
            VALUES ($workerId, $week, $delta, $value, $reason, $incidentId);
            """;
        command.Parameters.AddWithValue("$workerId", entry.WorkerId);
        command.Parameters.AddWithValue("$week", entry.Week);
        command.Parameters.AddWithValue("$delta", entry.Delta);
        command.Parameters.AddWithValue("$value", entry.Value);
        command.Parameters.AddWithValue("$reason", entry.Reason);
        command.Parameters.AddWithValue("$incidentId", entry.IncidentId ?? (object)DBNull.Value);
        command.ExecuteNonQuery();
    }

    public int ChargerMoraleActuelle(string workerId, int valeurDefaut = 50)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT Value
            FROM MoraleHistory
            WHERE WorkerId = $workerId
            ORDER BY Week DESC, MoraleHistoryId DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        var result = command.ExecuteScalar();
        return result is null || result is DBNull ? valeurDefaut : Convert.ToInt32(result);
    }

    // REFACTORED: Legacy methods now delegate to PascalCase methods
    // Old snake_case tables (backstage_incidents, disciplinary_actions, morale_history)
    // have been migrated to PascalCase (BackstageIncidents, DisciplinaryActions, MoraleHistory)
    // See migration 003_consolidate_schema.sql

    [Obsolete("Use AjouterIncident instead - this is a compatibility wrapper")]
    public void EnregistrerBackstageIncident(BackstageIncident incident)
    {
        // Delegate to the new PascalCase method
        AjouterIncident(incident);
    }

    [Obsolete("Use AjouterActionDisciplinaire instead - this is a compatibility wrapper")]
    public void EnregistrerDisciplinaryAction(DisciplinaryAction action)
    {
        // Delegate to the new PascalCase method
        AjouterActionDisciplinaire(action);
    }

    public IReadOnlyList<MoraleHistoryEntry> AppliquerMoraleImpacts(IReadOnlyList<BackstageMoraleImpact> impacts, int week)
    {
        if (impacts.Count == 0)
        {
            return Array.Empty<MoraleHistoryEntry>();
        }

        var historiques = new List<MoraleHistoryEntry>();

        foreach (var impact in impacts)
        {
            // Get current morale value, apply delta, and record history
            var moraleAvant = ChargerMoraleActuelle(impact.WorkerId, 50);
            var moraleApres = Math.Clamp(moraleAvant + impact.Delta, 0, 100);

            // Record the morale change via the new method
            var entry = new MoraleHistoryEntry(
                impact.WorkerId,
                week,
                impact.Delta,
                moraleApres,
                impact.Raison,
                impact.IncidentId);

            AjouterMoraleHistorique(entry);
            historiques.Add(entry);
        }

        return historiques;
    }
}
