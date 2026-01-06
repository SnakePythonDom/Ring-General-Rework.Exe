using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class YouthRepository : RepositoryBase
{
    public YouthRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public IReadOnlyList<YouthStructureState> ChargerYouthStructuresPourGeneration()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ys.youth_id,
                   ys.nom,
                   ys.company_id,
                   ys.region,
                   ys.type,
                   ys.budget_annuel,
                   ys.capacite_max,
                   ys.niveau_equipements,
                   ys.qualite_coaching,
                   ys.philosophie,
                   ys.actif,
                   COALESCE(state.derniere_generation_semaine, NULL),
                   COALESCE(counts.nb_trainees, 0)
            FROM youth_structures ys
            LEFT JOIN youth_generation_state state ON state.youth_id = ys.youth_id
            LEFT JOIN (
                SELECT youth_id, COUNT(1) AS nb_trainees
                FROM youth_trainees
                GROUP BY youth_id
            ) counts ON counts.youth_id = ys.youth_id
            WHERE ys.actif = 1;
            """;
        using var reader = command.ExecuteReader();
        var structures = new List<YouthStructureState>();
        while (reader.Read())
        {
            structures.Add(new YouthStructureState(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetInt32(7),
                reader.GetInt32(8),
                reader.GetString(9),
                reader.GetInt32(10) == 1,
                reader.IsDBNull(11) ? null : reader.GetInt32(11),
                reader.GetInt32(12)));
        }

        return structures;
    }

    public IReadOnlyList<YouthStructureState> ChargerYouthStructures()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ys.youth_id,
                   ys.nom,
                   ys.company_id,
                   ys.region,
                   ys.type,
                   ys.budget_annuel,
                   ys.capacite_max,
                   ys.niveau_equipements,
                   ys.qualite_coaching,
                   ys.philosophie,
                   ys.actif,
                   COALESCE(state.derniere_generation_semaine, NULL),
                   COALESCE(counts.nb_trainees, 0)
            FROM youth_structures ys
            LEFT JOIN youth_generation_state state ON state.youth_id = ys.youth_id
            LEFT JOIN (
                SELECT youth_id, COUNT(1) AS nb_trainees
                FROM youth_trainees
                GROUP BY youth_id
            ) counts ON counts.youth_id = ys.youth_id;
            """;
        using var reader = command.ExecuteReader();
        var structures = new List<YouthStructureState>();
        while (reader.Read())
        {
            structures.Add(new YouthStructureState(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetInt32(7),
                reader.GetInt32(8),
                reader.GetString(9),
                reader.GetInt32(10) == 1,
                reader.IsDBNull(11) ? null : reader.GetInt32(11),
                reader.GetInt32(12)));
        }

        return structures;
    }

    public IReadOnlyList<YouthTraineeInfo> ChargerYouthTrainees(string youthId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT t.worker_id,
                   w.prenom,
                   w.nom,
                   t.youth_id,
                   w.in_ring,
                   w.entertainment,
                   w.story,
                   t.statut
            FROM youth_trainees t
            JOIN workers w ON w.worker_id = t.worker_id
            WHERE t.youth_id = $youthId
            ORDER BY w.nom;
            """;
        command.Parameters.AddWithValue("$youthId", youthId);
        using var reader = command.ExecuteReader();
        var trainees = new List<YouthTraineeInfo>();
        while (reader.Read())
        {
            var prenom = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            var nom = reader.GetString(2);
            var nomComplet = string.IsNullOrWhiteSpace(prenom) ? nom : $"{prenom} {nom}";
            trainees.Add(new YouthTraineeInfo(
                reader.GetString(0),
                nomComplet,
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetString(7)));
        }

        return trainees;
    }

    public IReadOnlyList<YouthProgramInfo> ChargerYouthPrograms(string youthId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT program_id, youth_id, nom, duree_semaines, focus
            FROM youth_programs
            WHERE youth_id = $youthId
            ORDER BY nom;
            """;
        command.Parameters.AddWithValue("$youthId", youthId);
        using var reader = command.ExecuteReader();
        var programmes = new List<YouthProgramInfo>();
        while (reader.Read())
        {
            programmes.Add(new YouthProgramInfo(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetString(4)));
        }

        return programmes;
    }

    public IReadOnlyList<YouthStaffAssignmentInfo> ChargerYouthStaffAssignments(string youthId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT a.assignment_id,
                   a.youth_id,
                   a.worker_id,
                   w.prenom,
                   w.nom,
                   a.role,
                   a.semaine_debut
            FROM youth_staff_assignments a
            JOIN workers w ON w.worker_id = a.worker_id
            WHERE a.youth_id = $youthId
            ORDER BY a.role;
            """;
        command.Parameters.AddWithValue("$youthId", youthId);
        using var reader = command.ExecuteReader();
        var staff = new List<YouthStaffAssignmentInfo>();
        while (reader.Read())
        {
            var prenom = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            var nom = reader.GetString(4);
            var nomComplet = string.IsNullOrWhiteSpace(prenom) ? nom : $"{prenom} {nom}";
            staff.Add(new YouthStaffAssignmentInfo(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                nomComplet,
                reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetInt32(6)));
        }

        return staff;
    }

    public void ChangerBudgetYouth(string youthId, int nouveauBudget)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE youth_structures SET budget_annuel = $budget WHERE youth_id = $youthId;";
        command.Parameters.AddWithValue("$budget", nouveauBudget);
        command.Parameters.AddWithValue("$youthId", youthId);
        command.ExecuteNonQuery();
    }

    public void AffecterCoachYouth(string youthId, string workerId, string role, int semaine)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO youth_staff_assignments (youth_id, worker_id, role, semaine_debut)
            VALUES ($youthId, $workerId, $role, $semaine);
            """;
        command.Parameters.AddWithValue("$youthId", youthId);
        command.Parameters.AddWithValue("$workerId", workerId);
        command.Parameters.AddWithValue("$role", role);
        command.Parameters.AddWithValue("$semaine", semaine);
        command.ExecuteNonQuery();
    }

    public void DiplomerTrainee(string workerId, int semaine)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();

        using var traineeCommand = connexion.CreateCommand();
        traineeCommand.Transaction = transaction;
        traineeCommand.CommandText = """
            UPDATE youth_trainees
            SET statut = 'GRADUE',
                semaine_graduation = $semaine
            WHERE worker_id = $workerId;
            """;
        traineeCommand.Parameters.AddWithValue("$semaine", semaine);
        traineeCommand.Parameters.AddWithValue("$workerId", workerId);
        traineeCommand.ExecuteNonQuery();

        using var workerCommand = connexion.CreateCommand();
        workerCommand.Transaction = transaction;
        workerCommand.CommandText = """
            UPDATE workers
            SET type_worker = 'CATCHEUR'
            WHERE worker_id = $workerId;
            """;
        workerCommand.Parameters.AddWithValue("$workerId", workerId);
        workerCommand.ExecuteNonQuery();

        transaction.Commit();
    }

    public IReadOnlyList<YouthTraineeProgressionState> ChargerYouthTraineesPourProgression()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT t.worker_id,
                   w.prenom,
                   w.nom,
                   t.youth_id,
                   ys.philosophie,
                   ys.niveau_equipements,
                   ys.budget_annuel,
                   ys.qualite_coaching,
                   t.statut,
                   COALESCE(t.semaine_inscription, 1),
                   w.in_ring,
                   w.entertainment,
                   w.story
            FROM youth_trainees t
            JOIN workers w ON w.worker_id = t.worker_id
            JOIN youth_structures ys ON ys.youth_id = t.youth_id
            WHERE t.statut = 'EN_FORMATION';
            """;
        using var reader = command.ExecuteReader();
        var trainees = new List<YouthTraineeProgressionState>();
        while (reader.Read())
        {
            var prenom = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            var nom = reader.GetString(2);
            var nomComplet = string.IsNullOrWhiteSpace(prenom) ? nom : $"{prenom} {nom}";
            trainees.Add(new YouthTraineeProgressionState(
                reader.GetString(0),
                nomComplet,
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetInt32(7),
                reader.GetString(8),
                reader.GetInt32(9),
                reader.GetInt32(10),
                reader.GetInt32(11),
                reader.GetInt32(12)));
        }

        return trainees;
    }

    public void EnregistrerProgressionTrainees(YouthProgressionReport report)
    {
        if (report.Resultats.Count == 0)
        {
            return;
        }

        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();

        foreach (var resultat in report.Resultats)
        {
            using var workerCommand = connexion.CreateCommand();
            workerCommand.Transaction = transaction;
            workerCommand.CommandText = """
                UPDATE workers
                SET in_ring = $inRing,
                    entertainment = $entertainment,
                    story = $story
                WHERE worker_id = $workerId;
                """;
            workerCommand.Parameters.AddWithValue("$inRing", resultat.InRing);
            workerCommand.Parameters.AddWithValue("$entertainment", resultat.Entertainment);
            workerCommand.Parameters.AddWithValue("$story", resultat.Story);
            workerCommand.Parameters.AddWithValue("$workerId", resultat.WorkerId);
            workerCommand.ExecuteNonQuery();

            if (resultat.Diplome)
            {
                using var graduateCommand = connexion.CreateCommand();
                graduateCommand.Transaction = transaction;
                graduateCommand.CommandText = """
                    UPDATE youth_trainees
                    SET statut = 'GRADUE',
                        semaine_graduation = $semaine
                    WHERE worker_id = $workerId;
                    """;
                graduateCommand.Parameters.AddWithValue("$semaine", report.Semaine);
                graduateCommand.Parameters.AddWithValue("$workerId", resultat.WorkerId);
                graduateCommand.ExecuteNonQuery();

                using var roleCommand = connexion.CreateCommand();
                roleCommand.Transaction = transaction;
                roleCommand.CommandText = """
                    UPDATE workers
                    SET type_worker = 'CATCHEUR'
                    WHERE worker_id = $workerId;
                    """;
                roleCommand.Parameters.AddWithValue("$workerId", resultat.WorkerId);
                roleCommand.ExecuteNonQuery();
            }
        }

        transaction.Commit();
    }

    public GenerationCounters ChargerGenerationCounters(int annee)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT scope_type, scope_id, worker_type, count
            FROM worker_generation_counters
            WHERE annee = $annee;
            """;
        command.Parameters.AddWithValue("$annee", annee);
        using var reader = command.ExecuteReader();
        var traineesParPays = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var traineesParCompagnie = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var freeAgentsParPays = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var globalTrainees = 0;
        var globalFreeAgents = 0;

        while (reader.Read())
        {
            var scopeType = reader.GetString(0);
            var scopeId = reader.GetString(1);
            var workerType = reader.GetString(2);
            var count = reader.GetInt32(3);

            if (scopeType == "GLOBAL" && workerType == "TRAINEE")
            {
                globalTrainees = count;
            }
            else if (scopeType == "GLOBAL" && workerType == "FREE_AGENT")
            {
                globalFreeAgents = count;
            }
            else if (scopeType == "COUNTRY" && workerType == "TRAINEE")
            {
                traineesParPays[scopeId] = count;
            }
            else if (scopeType == "COMPANY" && workerType == "TRAINEE")
            {
                traineesParCompagnie[scopeId] = count;
            }
            else if (scopeType == "COUNTRY" && workerType == "FREE_AGENT")
            {
                freeAgentsParPays[scopeId] = count;
            }
        }

        return new GenerationCounters(annee, globalTrainees, traineesParPays, traineesParCompagnie, globalFreeAgents, freeAgentsParPays);
    }

    public void EnregistrerGeneration(WorkerGenerationReport report)
    {
        if (report.Workers.Count == 0)
        {
            return;
        }

        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();

        foreach (var worker in report.Workers)
        {
            using var workerCommand = connexion.CreateCommand();
            workerCommand.Transaction = transaction;
            workerCommand.CommandText = """
                INSERT INTO workers (worker_id, nom, prenom, company_id, in_ring, entertainment, story, popularite, fatigue, blessure, momentum, role_tv, type_worker)
                VALUES ($workerId, $nom, $prenom, $companyId, $inRing, $entertainment, $story, $popularite, $fatigue, $blessure, $momentum, $roleTv, $typeWorker);
                """;
            workerCommand.Parameters.AddWithValue("$workerId", worker.WorkerId);
            workerCommand.Parameters.AddWithValue("$nom", worker.Nom);
            workerCommand.Parameters.AddWithValue("$prenom", worker.Prenom);
            workerCommand.Parameters.AddWithValue("$companyId", worker.CompagnieId ?? (object)DBNull.Value);
            workerCommand.Parameters.AddWithValue("$inRing", worker.InRing);
            workerCommand.Parameters.AddWithValue("$entertainment", worker.Entertainment);
            workerCommand.Parameters.AddWithValue("$story", worker.Story);
            workerCommand.Parameters.AddWithValue("$popularite", worker.Popularite);
            workerCommand.Parameters.AddWithValue("$fatigue", worker.Fatigue);
            workerCommand.Parameters.AddWithValue("$blessure", worker.Blessure);
            workerCommand.Parameters.AddWithValue("$momentum", worker.Momentum);
            workerCommand.Parameters.AddWithValue("$roleTv", worker.RoleTv);
            workerCommand.Parameters.AddWithValue("$typeWorker", worker.TypeWorker);
            workerCommand.ExecuteNonQuery();

            foreach (var (attr, value) in worker.Attributes)
            {
                using var attrCommand = connexion.CreateCommand();
                attrCommand.Transaction = transaction;
                attrCommand.CommandText = """
                    INSERT INTO worker_attributes (worker_id, attribut_id, valeur)
                    VALUES ($workerId, $attrId, $valeur);
                    """;
                attrCommand.Parameters.AddWithValue("$workerId", worker.WorkerId);
                attrCommand.Parameters.AddWithValue("$attrId", attr);
                attrCommand.Parameters.AddWithValue("$valeur", value);
                attrCommand.ExecuteNonQuery();
            }

            using var popCommand = connexion.CreateCommand();
            popCommand.Transaction = transaction;
            popCommand.CommandText = """
                INSERT INTO popularity_regionale (entity_type, entity_id, region, valeur)
                VALUES ('worker', $workerId, $region, $valeur)
                ON CONFLICT(entity_type, entity_id, region) DO NOTHING;
                """;
            popCommand.Parameters.AddWithValue("$workerId", worker.WorkerId);
            popCommand.Parameters.AddWithValue("$region", worker.Region);
            popCommand.Parameters.AddWithValue("$valeur", worker.Popularite);
            popCommand.ExecuteNonQuery();

            if (!string.IsNullOrWhiteSpace(worker.YouthId))
            {
                using var youthCommand = connexion.CreateCommand();
                youthCommand.Transaction = transaction;
                youthCommand.CommandText = """
                    INSERT INTO youth_trainees (worker_id, youth_id, statut, semaine_inscription)
                    VALUES ($workerId, $youthId, 'EN_FORMATION', $semaineInscription);
                    """;
                youthCommand.Parameters.AddWithValue("$workerId", worker.WorkerId);
                youthCommand.Parameters.AddWithValue("$youthId", worker.YouthId);
                youthCommand.Parameters.AddWithValue("$semaineInscription", report.Semaine);
                youthCommand.ExecuteNonQuery();
            }

            using var eventCommand = connexion.CreateCommand();
            eventCommand.Transaction = transaction;
            eventCommand.CommandText = """
                INSERT INTO worker_generation_events (worker_id, worker_type, semaine, youth_id, region, company_id)
                VALUES ($workerId, $workerType, $semaine, $youthId, $region, $companyId);
                """;
            eventCommand.Parameters.AddWithValue("$workerId", worker.WorkerId);
            eventCommand.Parameters.AddWithValue("$workerType", worker.TypeWorker == "TRAINEE" ? "TRAINEE" : "FREE_AGENT");
            eventCommand.Parameters.AddWithValue("$semaine", report.Semaine);
            eventCommand.Parameters.AddWithValue("$youthId", worker.YouthId ?? (object)DBNull.Value);
            eventCommand.Parameters.AddWithValue("$region", worker.Region);
            eventCommand.Parameters.AddWithValue("$companyId", worker.CompagnieId ?? (object)DBNull.Value);
            eventCommand.ExecuteNonQuery();
        }

        MettreAJourCounters(transaction, report);
        MettreAJourGenerationState(transaction, report);

        transaction.Commit();
    }

    // === Helpers privés (Catégorie B - Youth domain) ===

    private void MettreAJourCounters(SqliteTransaction transaction, WorkerGenerationReport report)
    {
        var annee = ((report.Semaine - 1) / 52) + 1;
        var traineesParPays = report.Workers.Where(w => w.TypeWorker == "TRAINEE").GroupBy(w => w.Region);
        var traineesParCompagnie = report.Workers.Where(w => w.TypeWorker == "TRAINEE").GroupBy(w => w.CompagnieId ?? string.Empty);
        var freeAgentsParPays = report.Workers.Where(w => w.TypeWorker != "TRAINEE").GroupBy(w => w.Region);

        InsererOuMajCounter(transaction, annee, "GLOBAL", "GLOBAL", "TRAINEE", report.Workers.Count(w => w.TypeWorker == "TRAINEE"));
        InsererOuMajCounter(transaction, annee, "GLOBAL", "GLOBAL", "FREE_AGENT", report.Workers.Count(w => w.TypeWorker != "TRAINEE"));

        foreach (var group in traineesParPays)
        {
            InsererOuMajCounter(transaction, annee, "COUNTRY", group.Key, "TRAINEE", group.Count());
        }

        foreach (var group in traineesParCompagnie)
        {
            if (string.IsNullOrWhiteSpace(group.Key))
            {
                continue;
            }

            InsererOuMajCounter(transaction, annee, "COMPANY", group.Key, "TRAINEE", group.Count());
        }

        foreach (var group in freeAgentsParPays)
        {
            InsererOuMajCounter(transaction, annee, "COUNTRY", group.Key, "FREE_AGENT", group.Count());
        }
    }

    private void MettreAJourGenerationState(SqliteTransaction transaction, WorkerGenerationReport report)
    {
        var structures = report.Workers.Where(w => w.TypeWorker == "TRAINEE").Select(w => w.YouthId).Distinct();
        foreach (var youthId in structures)
        {
            if (string.IsNullOrWhiteSpace(youthId))
            {
                continue;
            }

            using var command = transaction.Connection!.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO youth_generation_state (youth_id, derniere_generation_semaine)
                VALUES ($youthId, $semaine)
                ON CONFLICT(youth_id) DO UPDATE SET derniere_generation_semaine = excluded.derniere_generation_semaine;
                """;
            command.Parameters.AddWithValue("$youthId", youthId);
            command.Parameters.AddWithValue("$semaine", report.Semaine);
            command.ExecuteNonQuery();
        }
    }

    private static void InsererOuMajCounter(SqliteTransaction transaction, int annee, string scopeType, string scopeId, string workerType, int delta)
    {
        if (delta <= 0)
        {
            return;
        }

        using var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO worker_generation_counters (annee, scope_type, scope_id, worker_type, count)
            VALUES ($annee, $scopeType, $scopeId, $workerType, $delta)
            ON CONFLICT(annee, scope_type, scope_id, worker_type)
            DO UPDATE SET count = worker_generation_counters.count + $delta;
            """;
        command.Parameters.AddWithValue("$annee", annee);
        command.Parameters.AddWithValue("$scopeType", scopeType);
        command.Parameters.AddWithValue("$scopeId", scopeId);
        command.Parameters.AddWithValue("$workerType", workerType);
        command.Parameters.AddWithValue("$delta", delta);
        command.ExecuteNonQuery();
    }
}
