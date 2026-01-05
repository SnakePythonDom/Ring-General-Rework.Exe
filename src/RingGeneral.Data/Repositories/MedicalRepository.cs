using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class MedicalRepository
{
    private readonly SqliteConnectionFactory _factory;

    public MedicalRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public void AjouterBlessure(InjuryRecord injury)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO injuries (injury_id, worker_id, type, severite, statut, semaine_debut, semaine_fin, duree_semaines)
            VALUES ($id, $workerId, $type, $severite, $statut, $debut, $fin, $duree);
            """;
        command.Parameters.AddWithValue("$id", injury.InjuryId);
        command.Parameters.AddWithValue("$workerId", injury.WorkerId);
        command.Parameters.AddWithValue("$type", injury.Type);
        command.Parameters.AddWithValue("$severite", injury.Severity.ToString().ToUpperInvariant());
        command.Parameters.AddWithValue("$statut", injury.Status.ToString().ToUpperInvariant());
        command.Parameters.AddWithValue("$debut", injury.WeekStart);
        command.Parameters.AddWithValue("$fin", (object?)injury.WeekEnd ?? DBNull.Value);
        command.Parameters.AddWithValue("$duree", injury.DurationWeeks);
        command.ExecuteNonQuery();
    }

    public void MettreAJourBlessure(InjuryRecord injury)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE injuries
            SET statut = $statut,
                semaine_fin = $fin,
                duree_semaines = $duree
            WHERE injury_id = $id;
            """;
        command.Parameters.AddWithValue("$id", injury.InjuryId);
        command.Parameters.AddWithValue("$statut", injury.Status.ToString().ToUpperInvariant());
        command.Parameters.AddWithValue("$fin", (object?)injury.WeekEnd ?? DBNull.Value);
        command.Parameters.AddWithValue("$duree", injury.DurationWeeks);
        command.ExecuteNonQuery();
    }

    public void AjouterNote(MedicalNote note)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO medical_notes (note_id, worker_id, injury_id, type_note, contenu, semaine, auteur)
            VALUES ($id, $workerId, $injuryId, $type, $contenu, $semaine, $auteur);
            """;
        command.Parameters.AddWithValue("$id", note.NoteId);
        command.Parameters.AddWithValue("$workerId", note.WorkerId);
        command.Parameters.AddWithValue("$injuryId", (object?)note.InjuryId ?? DBNull.Value);
        command.Parameters.AddWithValue("$type", note.Type);
        command.Parameters.AddWithValue("$contenu", note.Content);
        command.Parameters.AddWithValue("$semaine", note.Week);
        command.Parameters.AddWithValue("$auteur", (object?)note.Author ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void AjouterPlan(RecoveryPlan plan)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO recovery_plans (plan_id, injury_id, worker_id, statut, semaine_debut, semaine_fin, duree_semaines, repos_conseille, restrictions, notes)
            VALUES ($id, $injuryId, $workerId, $statut, $debut, $fin, $duree, $repos, $restrictions, $notes);
            """;
        command.Parameters.AddWithValue("$id", plan.PlanId);
        command.Parameters.AddWithValue("$injuryId", plan.InjuryId);
        command.Parameters.AddWithValue("$workerId", plan.WorkerId);
        command.Parameters.AddWithValue("$statut", plan.Status.ToString().ToUpperInvariant());
        command.Parameters.AddWithValue("$debut", plan.WeekStart);
        command.Parameters.AddWithValue("$fin", (object?)plan.WeekEnd ?? DBNull.Value);
        command.Parameters.AddWithValue("$duree", plan.DurationWeeks);
        command.Parameters.AddWithValue("$repos", plan.RecommendedRestWeeks);
        command.Parameters.AddWithValue("$restrictions", (object?)plan.Restrictions ?? DBNull.Value);
        command.Parameters.AddWithValue("$notes", (object?)plan.Notes ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<InjuryRecord> ChargerBlessures(string workerId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT injury_id, worker_id, type, severite, statut, semaine_debut, semaine_fin, duree_semaines
            FROM injuries
            WHERE worker_id = $workerId
            ORDER BY semaine_debut DESC;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        using var reader = command.ExecuteReader();
        var blessures = new List<InjuryRecord>();
        while (reader.Read())
        {
            var severite = Enum.Parse<InjurySeverity>(reader.GetString(3), true);
            var statut = Enum.Parse<InjuryStatus>(reader.GetString(4), true);
            blessures.Add(new InjuryRecord(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                severite,
                statut,
                reader.GetInt32(5),
                reader.IsDBNull(6) ? null : reader.GetInt32(6),
                reader.GetInt32(7)));
        }

        return blessures;
    }
}
