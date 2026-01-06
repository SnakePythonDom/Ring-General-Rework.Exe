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

    // LEGACY METHODS: Use snake_case table names for backward compatibility
    // TODO: Migrate to PascalCase tables and remove these methods

    public void EnregistrerBackstageIncident(BackstageIncident incident)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO backstage_incidents (incident_id, company_id, semaine, type_id, titre, description, gravite, workers_json)
            VALUES ($id, $companyId, $week, $typeId, $title, $description, $severity, $workersJson);
            """;
        command.Parameters.AddWithValue("$id", incident.IncidentId);
        command.Parameters.AddWithValue("$companyId", ChargerCompanyIdPourWorker(connexion, incident.WorkerId));
        command.Parameters.AddWithValue("$week", incident.Week);
        command.Parameters.AddWithValue("$typeId", incident.IncidentType);
        command.Parameters.AddWithValue("$title", incident.IncidentType);
        command.Parameters.AddWithValue("$description", incident.Description);
        command.Parameters.AddWithValue("$severity", incident.Severity);
        command.Parameters.AddWithValue("$workersJson", JsonSerializer.Serialize(new[] { incident.WorkerId }, JsonOptions));
        command.ExecuteNonQuery();
    }

    public void EnregistrerDisciplinaryAction(DisciplinaryAction action)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO disciplinary_actions (action_id, company_id, worker_id, semaine, type_id, gravite, morale_delta, notes, incident_id)
            VALUES ($id, $companyId, $workerId, $week, $typeId, $severity, $moraleDelta, $notes, $incidentId);
            """;
        command.Parameters.AddWithValue("$id", action.ActionId);
        command.Parameters.AddWithValue("$companyId", ChargerCompanyIdPourWorker(connexion, action.WorkerId));
        command.Parameters.AddWithValue("$workerId", action.WorkerId);
        command.Parameters.AddWithValue("$week", action.Week);
        command.Parameters.AddWithValue("$typeId", action.ActionType);
        command.Parameters.AddWithValue("$severity", MapperGraviteDiscipline(action.ActionType));
        command.Parameters.AddWithValue("$moraleDelta", action.MoraleDelta);
        command.Parameters.AddWithValue("$notes", action.Notes ?? string.Empty);
        command.Parameters.AddWithValue("$incidentId", action.IncidentId);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<MoraleHistoryEntry> AppliquerMoraleImpacts(IReadOnlyList<BackstageMoraleImpact> impacts, int week)
    {
        if (impacts.Count == 0)
        {
            return Array.Empty<MoraleHistoryEntry>();
        }

        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        var historiques = new List<MoraleHistoryEntry>();

        foreach (var impact in impacts)
        {
            using var selectCommand = connexion.CreateCommand();
            selectCommand.Transaction = transaction;
            selectCommand.CommandText = "SELECT morale FROM workers WHERE worker_id = $workerId;";
            selectCommand.Parameters.AddWithValue("$workerId", impact.WorkerId);
            var moraleAvant = Convert.ToInt32(selectCommand.ExecuteScalar());
            var moraleApres = Math.Clamp(moraleAvant + impact.Delta, 0, 100);

            using var updateCommand = connexion.CreateCommand();
            updateCommand.Transaction = transaction;
            updateCommand.CommandText = "UPDATE workers SET morale = $morale WHERE worker_id = $workerId;";
            updateCommand.Parameters.AddWithValue("$morale", moraleApres);
            updateCommand.Parameters.AddWithValue("$workerId", impact.WorkerId);
            updateCommand.ExecuteNonQuery();

            using var historyCommand = connexion.CreateCommand();
            historyCommand.Transaction = transaction;
            historyCommand.CommandText = """
                INSERT INTO morale_history (worker_id, semaine, morale_avant, morale_apres, delta, raison, incident_id, action_id)
                VALUES ($workerId, $week, $moraleAvant, $moraleApres, $delta, $reason, $incidentId, $actionId);
                """;
            historyCommand.Parameters.AddWithValue("$workerId", impact.WorkerId);
            historyCommand.Parameters.AddWithValue("$week", week);
            historyCommand.Parameters.AddWithValue("$moraleAvant", moraleAvant);
            historyCommand.Parameters.AddWithValue("$moraleApres", moraleApres);
            historyCommand.Parameters.AddWithValue("$delta", impact.Delta);
            historyCommand.Parameters.AddWithValue("$reason", impact.Raison);
            historyCommand.Parameters.AddWithValue("$incidentId", (object?)impact.IncidentId ?? DBNull.Value);
            historyCommand.Parameters.AddWithValue("$actionId", (object?)impact.ActionId ?? DBNull.Value);
            historyCommand.ExecuteNonQuery();

            historiques.Add(new MoraleHistoryEntry(
                impact.WorkerId,
                week,
                impact.Delta,
                moraleApres,
                impact.Raison,
                impact.IncidentId));
        }

        transaction.Commit();
        return historiques;
    }

    private static string ChargerCompanyIdPourWorker(SqliteConnection connexion, string workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT company_id FROM workers WHERE worker_id = $workerId;";
        command.Parameters.AddWithValue("$workerId", workerId);
        return Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
    }

    private static int MapperGraviteDiscipline(string actionType)
    {
        return actionType switch
        {
            "SUSPENSION" => 3,
            "AMENDE" => 2,
            "AVERTISSEMENT" => 1,
            _ => 0
        };
    }
}
