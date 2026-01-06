using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class ScoutingRepository : RepositoryBase
{
    public ScoutingRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public ScoutingTarget? ChargerCibleScouting(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT w.worker_id,
                   w.prenom,
                   w.nom,
                   w.in_ring,
                   w.entertainment,
                   w.story,
                   w.popularite,
                   COALESCE(MIN(pr.region), 'INCONNU') AS region
            FROM workers w
            LEFT JOIN popularity_regionale pr
                ON pr.entity_type = 'worker' AND pr.entity_id = w.worker_id
            WHERE w.worker_id = $workerId
            GROUP BY w.worker_id;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ScoutingTarget(
            reader.GetString(0),
            $"{reader.GetString(1)} {reader.GetString(2)}",
            reader.GetString(7),
            reader.GetInt32(3),
            reader.GetInt32(4),
            reader.GetInt32(5),
            reader.GetInt32(6));
    }

    public IReadOnlyList<ScoutingTarget> ChargerCiblesScouting(int limite)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT w.worker_id,
                   w.prenom,
                   w.nom,
                   w.in_ring,
                   w.entertainment,
                   w.story,
                   w.popularite,
                   COALESCE(MIN(pr.region), 'INCONNU') AS region
            FROM workers w
            LEFT JOIN popularity_regionale pr
                ON pr.entity_type = 'worker' AND pr.entity_id = w.worker_id
            WHERE w.company_id IS NULL
            GROUP BY w.worker_id
            ORDER BY w.popularite DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", limite);
        using var reader = command.ExecuteReader();
        var cibles = new List<ScoutingTarget>();
        while (reader.Read())
        {
            cibles.Add(new ScoutingTarget(
                reader.GetString(0),
                $"{reader.GetString(1)} {reader.GetString(2)}",
                reader.GetString(7),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6)));
        }

        return cibles;
    }

    public bool RapportExiste(string workerId, int semaine)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM scout_reports WHERE worker_id = $workerId AND semaine = $semaine;";
        command.Parameters.AddWithValue("$workerId", workerId);
        command.Parameters.AddWithValue("$semaine", semaine);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public void AjouterScoutReport(ScoutReport report)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT OR IGNORE INTO scout_reports
                (report_id, worker_id, nom, region, potentiel, in_ring, entertainment, story, resume, notes, semaine, source)
            VALUES
                ($reportId, $workerId, $nom, $region, $potentiel, $inRing, $entertainment, $story, $resume, $notes, $semaine, $source);
            """;
        command.Parameters.AddWithValue("$reportId", report.ReportId);
        command.Parameters.AddWithValue("$workerId", report.WorkerId);
        command.Parameters.AddWithValue("$nom", report.Nom);
        command.Parameters.AddWithValue("$region", report.Region);
        command.Parameters.AddWithValue("$potentiel", report.Potentiel);
        command.Parameters.AddWithValue("$inRing", report.InRing);
        command.Parameters.AddWithValue("$entertainment", report.Entertainment);
        command.Parameters.AddWithValue("$story", report.Story);
        command.Parameters.AddWithValue("$resume", report.Resume);
        command.Parameters.AddWithValue("$notes", report.Notes);
        command.Parameters.AddWithValue("$semaine", report.Semaine);
        command.Parameters.AddWithValue("$source", report.Source);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<ScoutReport> ChargerScoutReports()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT report_id,
                   worker_id,
                   nom,
                   region,
                   potentiel,
                   in_ring,
                   entertainment,
                   story,
                   resume,
                   notes,
                   semaine,
                   source
            FROM scout_reports
            ORDER BY semaine DESC;
            """;
        using var reader = command.ExecuteReader();
        var rapports = new List<ScoutReport>();
        while (reader.Read())
        {
            rapports.Add(new ScoutReport(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetInt32(7),
                reader.GetString(8),
                reader.GetString(9),
                reader.GetInt32(10),
                reader.GetString(11)));
        }

        return rapports;
    }

    public void AjouterShortlist(ShortlistEntry entry)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO shortlists (shortlist_id, worker_id, nom, note, semaine)
            VALUES ($shortlistId, $workerId, $nom, $note, $semaine);
            """;
        command.Parameters.AddWithValue("$shortlistId", entry.ShortlistId);
        command.Parameters.AddWithValue("$workerId", entry.WorkerId);
        command.Parameters.AddWithValue("$nom", entry.Nom);
        command.Parameters.AddWithValue("$note", entry.Note);
        command.Parameters.AddWithValue("$semaine", entry.Semaine);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<ShortlistEntry> ChargerShortlist()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT shortlist_id, worker_id, nom, note, semaine
            FROM shortlists
            ORDER BY semaine DESC;
            """;
        using var reader = command.ExecuteReader();
        var entries = new List<ShortlistEntry>();
        while (reader.Read())
        {
            entries.Add(new ShortlistEntry(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4)));
        }

        return entries;
    }

    public void AjouterMission(ScoutMission mission)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO scout_missions
                (mission_id, titre, region, focus, progression, objectif, statut, semaine_debut, semaine_maj)
            VALUES
                ($missionId, $titre, $region, $focus, $progression, $objectif, $statut, $semaineDebut, $semaineMaj);
            """;
        command.Parameters.AddWithValue("$missionId", mission.MissionId);
        command.Parameters.AddWithValue("$titre", mission.Titre);
        command.Parameters.AddWithValue("$region", mission.Region);
        command.Parameters.AddWithValue("$focus", mission.Focus);
        command.Parameters.AddWithValue("$progression", mission.Progression);
        command.Parameters.AddWithValue("$objectif", mission.Objectif);
        command.Parameters.AddWithValue("$statut", mission.Statut);
        command.Parameters.AddWithValue("$semaineDebut", mission.SemaineDebut);
        command.Parameters.AddWithValue("$semaineMaj", mission.SemaineMaj);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<ScoutMission> ChargerMissionsActives()
    {
        return ChargerMissions("WHERE statut = 'active'");
    }

    public IReadOnlyList<ScoutMission> ChargerScoutMissions()
    {
        return ChargerMissions(string.Empty);
    }

    public void MettreAJourMissionProgress(string missionId, int progression, string statut, int semaineMaj)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE scout_missions
            SET progression = $progression,
                statut = $statut,
                semaine_maj = $semaineMaj
            WHERE mission_id = $missionId;
            """;
        command.Parameters.AddWithValue("$progression", progression);
        command.Parameters.AddWithValue("$statut", statut);
        command.Parameters.AddWithValue("$semaineMaj", semaineMaj);
        command.Parameters.AddWithValue("$missionId", missionId);
        command.ExecuteNonQuery();
    }

    // === Helpers privés (Catégorie B - Scouting domain) ===

    private IReadOnlyList<ScoutMission> ChargerMissions(string clause)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = $"""
            SELECT mission_id,
                   titre,
                   region,
                   focus,
                   progression,
                   objectif,
                   statut,
                   semaine_debut,
                   semaine_maj
            FROM scout_missions
            {clause}
            ORDER BY semaine_debut DESC;
            """;
        using var reader = command.ExecuteReader();
        var missions = new List<ScoutMission>();
        while (reader.Read())
        {
            missions.Add(new ScoutMission(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetString(6),
                reader.GetInt32(7),
                reader.GetInt32(8)));
        }

        return missions;
    }
}
