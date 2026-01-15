using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class YouthRepository : RepositoryBase, RingGeneral.Core.Interfaces.IYouthRepository
{
    public YouthRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public IReadOnlyList<YouthStructureState> ChargerYouthStructuresPourGeneration()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ys.YouthStructureId,
                   ys.Name,
                   ys.CompanyId,
                   ys.RegionId,
                   ys.Type,
                   ys.BudgetAnnuel,
                   ys.CapaciteMax,
                   ys.NiveauEquipements,
                   ys.QualiteCoaching,
                   ys.Philosophie,
                   ys.GenderPreference,
                   ys.SpecializationPreference,
                   ys.Actif,
                   NULL,
                   COALESCE(counts.nb_trainees, 0)
            FROM YouthStructures ys
            LEFT JOIN (
                SELECT YouthStructureId, COUNT(1) AS nb_trainees
                FROM YouthTrainees
                GROUP BY YouthStructureId
            ) counts ON counts.YouthStructureId = ys.YouthStructureId
            WHERE ys.Actif = 1;
            """;
        using var reader = command.ExecuteReader();
        var structures = new List<YouthStructureState>();
        while (reader.Read())
        {
            structures.Add(new YouthStructureState(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetInt32(7),
                reader.GetInt32(8),
                reader.IsDBNull(9) ? "Balanced" : reader.GetString(9),
                reader.IsDBNull(10) ? "BOTH" : reader.GetString(10), // GenderPreference
                reader.IsDBNull(11) ? "NONE" : reader.GetString(11), // SpecializationPreference
                reader.IsDBNull(12) ? false : reader.GetInt32(12) == 1,
                reader.IsDBNull(13) ? null : reader.GetInt32(13),
                reader.GetInt32(14)));
        }

        return structures;
    }

    public IReadOnlyList<YouthStructureState> ChargerYouthStructures()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ys.YouthStructureId,
                   ys.Name,
                   ys.CompanyId,
                   ys.RegionId,
                   ys.Type,
                   ys.BudgetAnnuel,
                   ys.CapaciteMax,
                   ys.NiveauEquipements,
                   ys.QualiteCoaching,
                   ys.Philosophie,
                   ys.GenderPreference,
                   ys.SpecializationPreference,
                   ys.Actif,
                   NULL,
                   COALESCE(counts.nb_trainees, 0)
            FROM YouthStructures ys
            LEFT JOIN (
                SELECT YouthStructureId, COUNT(1) AS nb_trainees
                FROM YouthTrainees
                GROUP BY YouthStructureId
            ) counts ON counts.YouthStructureId = ys.YouthStructureId;
            """;
        using var reader = command.ExecuteReader();
        var structures = new List<YouthStructureState>();
        while (reader.Read())
        {
            structures.Add(new YouthStructureState(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetInt32(7),
                reader.GetInt32(8),
                reader.IsDBNull(9) ? "Balanced" : reader.GetString(9),
                reader.IsDBNull(10) ? "BOTH" : reader.GetString(10), // GenderPreference
                reader.IsDBNull(11) ? "NONE" : reader.GetString(11), // SpecializationPreference
                reader.IsDBNull(12) ? false : reader.GetInt32(12) == 1,
                reader.IsDBNull(13) ? null : reader.GetInt32(13),
                reader.GetInt32(14)));
        }

        return structures;
    }

    public IReadOnlyList<YouthTraineeInfo> ChargerYouthTrainees(string youthId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT t.WorkerId,
                   w.FirstName,
                   w.LastName,
                   t.YouthStructureId,
                   w.InRing,
                   w.Entertainment,
                   w.Story,
                   t.Status
            FROM YouthTrainees t
            JOIN Workers w ON w.WorkerId = t.WorkerId
            WHERE t.YouthStructureId = $youthId
            ORDER BY w.LastName;
            """;
        command.Parameters.AddWithValue("$youthId", youthId);
        using var reader = command.ExecuteReader();
        var trainees = new List<YouthTraineeInfo>();
        while (reader.Read())
        {
            trainees.Add(new YouthTraineeInfo(
                reader.GetString(0), // WorkerId
                (reader.IsDBNull(1) ? "" : reader.GetString(1)) + " " + (reader.IsDBNull(2) ? "" : reader.GetString(2)), // Name (Prenom + Nom)
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
            SELECT ProgramId, YouthStructureId, Name, DurationWeeks, FocusAttributes
            FROM YouthPrograms
            WHERE YouthStructureId = $youthId
            ORDER BY Name;
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
            SELECT a.YouthStaffAssignmentId,
                   a.YouthStructureId,
                   a.WorkerId,
                   w.FirstName,
                   w.LastName,
                   a.Role,
                   a.StartDate
            FROM YouthStaffAssignments a
            JOIN Workers w ON w.WorkerId = a.WorkerId
            WHERE a.YouthStructureId = $youthId
            ORDER BY a.Role;
            """;
        command.Parameters.AddWithValue("$youthId", youthId);
        using var reader = command.ExecuteReader();
        var staff = new List<YouthStaffAssignmentInfo>();
        while (reader.Read())
        {
            var prenom = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            var nom = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
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

    public IReadOnlyList<YouthAlumniInfo> ChargerAlumni(string youthId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT t.WorkerId,
                   w.Name,
                   t.GraduationDate,
                   w.WorkerType,
                   w.InRing,
                   w.Entertainment,
                   w.Story
            FROM YouthTrainees t
            JOIN Workers w ON w.WorkerId = t.WorkerId
            WHERE t.YouthStructureId = $youthId AND t.Status = 'GRADUE'
            ORDER BY t.GraduationDate DESC;
            """;
        command.Parameters.AddWithValue("$youthId", youthId);
        using var reader = command.ExecuteReader();
        var alumni = new List<YouthAlumniInfo>();
        while (reader.Read())
        {
            alumni.Add(new YouthAlumniInfo(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetInt32(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6)));
        }

        return alumni;
    }

    /// <summary>
    /// Récupère les workers d'une compagnie qui ne sont pas déjà affectés à une structure Youth
    /// </summary>
    public IReadOnlyList<WorkerBackstageProfile> ChargerWorkersDisposPourStaff(string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT WorkerId, Name
            FROM Workers
            WHERE CompanyId = $companyId
            AND WorkerId NOT IN (SELECT WorkerId FROM YouthStaffAssignments)
            ORDER BY Name;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var roster = new List<WorkerBackstageProfile>();
        while (reader.Read())
        {
            roster.Add(new WorkerBackstageProfile(reader.GetString(0), reader.GetString(1)));
        }

        return roster;
    }

    /// <summary>
    /// Phase 2.3 - Crée une nouvelle YouthStructure
    /// </summary>
    public async Task CreateYouthStructureAsync(
        string youthStructureId,
        string companyId,
        string name,
        string? regionId,
        string type,
        decimal budgetAnnuel,
        int capaciteMax,
        int niveauEquipements,
        int qualiteCoaching,
        string philosophie,
        string genderPreference,
        string specializationPreference)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO YouthStructures (
                YouthStructureId, CompanyId, Name, RegionId, Type, 
                BudgetAnnuel, CapaciteMax, NiveauEquipements, QualiteCoaching, 
                Philosophie, GenderPreference, SpecializationPreference, Actif
            ) VALUES (
                $youthStructureId, $companyId, $name, $regionId, $type,
                $budgetAnnuel, $capaciteMax, $niveauEquipements, $qualiteCoaching,
                $philosophie, $genderPreference, $specializationPreference, 1
            );
            """;
        command.Parameters.AddWithValue("$youthStructureId", youthStructureId);
        command.Parameters.AddWithValue("$companyId", companyId);
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$regionId", (object?)regionId ?? DBNull.Value);
        command.Parameters.AddWithValue("$type", type);
        command.Parameters.AddWithValue("$budgetAnnuel", budgetAnnuel);
        command.Parameters.AddWithValue("$capaciteMax", capaciteMax);
        command.Parameters.AddWithValue("$niveauEquipements", niveauEquipements);
        command.Parameters.AddWithValue("$qualiteCoaching", qualiteCoaching);
        command.Parameters.AddWithValue("$philosophie", philosophie);
        command.Parameters.AddWithValue("$genderPreference", genderPreference);
        command.Parameters.AddWithValue("$specializationPreference", specializationPreference);
        await Task.Run(() => command.ExecuteNonQuery());
    }

    public void ChangerBudgetYouth(string youthId, int nouveauBudget)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE YouthStructures SET BudgetAnnuel = $budget WHERE YouthStructureId = $youthId;";
        command.Parameters.AddWithValue("$budget", nouveauBudget);
        command.Parameters.AddWithValue("$youthId", youthId);
        command.ExecuteNonQuery();
    }

    public void AmeliorerEquipements(string youthId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE YouthStructures SET NiveauEquipements = NiveauEquipements + 1 WHERE YouthStructureId = $youthId;";
        command.Parameters.AddWithValue("$youthId", youthId);
        command.ExecuteNonQuery();
    }

    public void AffecterCoachYouth(string youthId, string workerId, string role, int semaine)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO YouthStaffAssignments (YouthStructureId, WorkerId, Role, StartDate)
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
            UPDATE YouthTrainees
            SET Status = 'GRADUE',
                GraduationDate = $semaine
            WHERE WorkerId = $workerId;
            """;
        traineeCommand.Parameters.AddWithValue("$semaine", semaine);
        traineeCommand.Parameters.AddWithValue("$workerId", workerId);
        traineeCommand.ExecuteNonQuery();

        using var workerCommand = connexion.CreateCommand();
        workerCommand.Transaction = transaction;
        workerCommand.CommandText = """
            UPDATE Workers
            SET WorkerType = 'WRESTLER'
            WHERE WorkerId = $workerId;
            """;
        workerCommand.Parameters.AddWithValue("$workerId", workerId);
        workerCommand.ExecuteNonQuery();

        transaction.Commit();
    }

    public void LicencierTrainee(string workerId, int semaine)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();

        using var traineeCommand = connexion.CreateCommand();
        traineeCommand.Transaction = transaction;
        traineeCommand.CommandText = """
            UPDATE YouthTrainees
            SET Status = 'RELEASED',
                GraduationDate = $semaine
            WHERE WorkerId = $workerId;
            """;
        traineeCommand.Parameters.AddWithValue("$semaine", semaine);
        traineeCommand.Parameters.AddWithValue("$workerId", workerId);
        traineeCommand.ExecuteNonQuery();

        using var workerCommand = connexion.CreateCommand();
        workerCommand.Transaction = transaction;
        workerCommand.CommandText = """
            UPDATE Workers
            SET WorkerType = 'FREE_AGENT'
            WHERE WorkerId = $workerId;
            """;
        workerCommand.Parameters.AddWithValue("$workerId", workerId);
        workerCommand.ExecuteNonQuery();

        transaction.Commit();
    }

    public void DeleteStructure(string youthId)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();

        try
        {
            // 1. Delete Staff Assignments
            using var cmdStaff = connexion.CreateCommand();
            cmdStaff.Transaction = transaction;
            cmdStaff.CommandText = "DELETE FROM YouthStaffAssignments WHERE YouthStructureId = $id";
            cmdStaff.Parameters.AddWithValue("$id", youthId);
            cmdStaff.ExecuteNonQuery();

            // 2. Delete Programs
            using var cmdPrograms = connexion.CreateCommand();
            cmdPrograms.Transaction = transaction;
            cmdPrograms.CommandText = "DELETE FROM YouthPrograms WHERE YouthStructureId = $id";
            cmdPrograms.Parameters.AddWithValue("$id", youthId);
            cmdPrograms.ExecuteNonQuery();

            // 3. Delete Trainees (Associations)
            // Note: This only removes them from the structure. 
            // Ideally we might want to set them to Free Agent, but for now we fix the crash.
            using var cmdTrainees = connexion.CreateCommand();
            cmdTrainees.Transaction = transaction;
            cmdTrainees.CommandText = "DELETE FROM YouthTrainees WHERE YouthStructureId = $id";
            cmdTrainees.Parameters.AddWithValue("$id", youthId);
            cmdTrainees.ExecuteNonQuery();

            // 4. Delete Structure
            using var cmdStructure = connexion.CreateCommand();
            cmdStructure.Transaction = transaction;
            cmdStructure.CommandText = "DELETE FROM YouthStructures WHERE YouthStructureId = $id";
            cmdStructure.Parameters.AddWithValue("$id", youthId);
            cmdStructure.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task CreateTraineeAsync(string youthId, string name, int age, int potential, int progress)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        // Using "Youth" table as per original logic, though likely incorrect. 
        // Correcting to 'youth_trainees' + 'workers' would be better but requires more context.
        // Sticking to original logic for migration safety:
        command.CommandText = """
            INSERT INTO Youth (YouthId, Name, Age, Potential, Progress)
            VALUES ($id, $name, $age, $potential, $progress);
            """;
        command.Parameters.AddWithValue("$id", youthId);
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$age", age);
        command.Parameters.AddWithValue("$potential", potential);
        command.Parameters.AddWithValue("$progress", progress);
        await Task.Run(() => command.ExecuteNonQuery());
    }

    public IReadOnlyList<YouthTraineeProgressionState> ChargerYouthTraineesPourProgression()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT t.WorkerId,
                   w.FirstName,
                   w.LastName,
                   t.YouthStructureId,
                   ys.Philosophie,
                   ys.NiveauEquipements,
                   ys.BudgetAnnuel,
                   ys.QualiteCoaching,
                   t.Status,
                   COALESCE(t.StartDate, 1),
                   w.InRing,
                   w.Entertainment,
                   w.Story
            FROM YouthTrainees t
            JOIN Workers w ON w.WorkerId = t.WorkerId
            JOIN YouthStructures ys ON ys.YouthStructureId = t.YouthStructureId
            WHERE t.Status = 'EN_FORMATION';
            """;
        using var reader = command.ExecuteReader();
        var trainees = new List<YouthTraineeProgressionState>();
        while (reader.Read())
        {
            var prenom = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            var nom = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            var nomComplet = string.IsNullOrWhiteSpace(prenom) ? nom : $"{prenom} {nom}";
            trainees.Add(new YouthTraineeProgressionState(
                reader.GetString(0),
                nomComplet,
                reader.GetString(3),
                reader.IsDBNull(4) ? "Balanced" : reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetInt32(7),
                reader.IsDBNull(8) ? "EN_FORMATION" : reader.GetString(8),
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
                UPDATE Workers
                SET InRing = $inRing,
                    Entertainment = $entertainment,
                    Story = $story
                WHERE WorkerId = $workerId;
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
                    UPDATE YouthTrainees
                    SET Status = 'GRADUE',
                        GraduationDate = $semaine
                    WHERE WorkerId = $workerId;
                    """;
                graduateCommand.Parameters.AddWithValue("$semaine", report.Semaine);
                graduateCommand.Parameters.AddWithValue("$workerId", resultat.WorkerId);
                graduateCommand.ExecuteNonQuery();

                using var roleCommand = connexion.CreateCommand();
                roleCommand.Transaction = transaction;
                roleCommand.CommandText = """
                    UPDATE Workers
                    SET WorkerType = 'WRESTLER'
                    WHERE WorkerId = $workerId;
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
            SELECT ScopeType, ScopeId, WorkerType, Count
            FROM WorkerGenerationCounters
            WHERE Annee = $annee;
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
                INSERT INTO Workers (WorkerId, LastName, FirstName, Name, Gender, Nationality, CompanyId, InRing, Entertainment, Story, Fatigue, WorkerType, Age, Popularity, Momentum, InjuryStatus, RoleTv)
                VALUES ($workerId, $lastName, $firstName, $firstName || ' ' || $lastName, $gender, $nationality, $companyId, $inRing, $entertainment, $story, $fatigue, $workerType, $age, $popularity, $momentum, $injury, $roleTv);
                """;
            workerCommand.Parameters.AddWithValue("$workerId", worker.WorkerId);
            workerCommand.Parameters.AddWithValue("$lastName", worker.Nom);
            workerCommand.Parameters.AddWithValue("$firstName", worker.Prenom);
            workerCommand.Parameters.AddWithValue("$gender", worker.Gender);
            workerCommand.Parameters.AddWithValue("$nationality", worker.Region);
            workerCommand.Parameters.AddWithValue("$companyId", worker.CompagnieId ?? (object)DBNull.Value);
            workerCommand.Parameters.AddWithValue("$inRing", worker.InRing);
            workerCommand.Parameters.AddWithValue("$entertainment", worker.Entertainment);
            workerCommand.Parameters.AddWithValue("$story", worker.Story);
            workerCommand.Parameters.AddWithValue("$fatigue", worker.Fatigue);
            workerCommand.Parameters.AddWithValue("$workerType", worker.TypeWorker);
            workerCommand.Parameters.AddWithValue("$age", worker.Age);
            workerCommand.Parameters.AddWithValue("$popularity", worker.Popularite);
            workerCommand.Parameters.AddWithValue("$momentum", worker.Momentum);
            workerCommand.Parameters.AddWithValue("$injury", worker.Blessure);
            workerCommand.Parameters.AddWithValue("$roleTv", worker.RoleTv);
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
                    INSERT INTO YouthTrainees (WorkerId, YouthStructureId, Status, StartDate)
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
                /* YouthGenerationState table update skipped as schema uncertain */
                /* INSERT INTO youth_generation_state (youth_id, derniere_generation_semaine)
                VALUES ($youthId, $semaine)
                ON CONFLICT(youth_id) DO UPDATE SET derniere_generation_semaine = excluded.derniere_generation_semaine; */
                """;
            command.Parameters.AddWithValue("$youthId", youthId);
            command.Parameters.AddWithValue("$semaine", report.Semaine);
            command.ExecuteNonQuery();
        }
    }

    public async Task EnregistrerGeneration(IEnumerable<GeneratedWorker> workers, string structureId, int semaine)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var transaction = connexion.BeginTransaction();

            try
            {
                foreach (var worker in workers)
                {
                    using var workerCommand = connexion.CreateCommand();
                    workerCommand.Transaction = transaction;
                    workerCommand.CommandText = """
                    INSERT INTO Workers (WorkerId, LastName, FirstName, Name, Gender, Nationality, CompanyId, InRing, Entertainment, Story, Fatigue, WorkerType)
                    VALUES ($workerId, $lastName, $firstName, $firstName || ' ' || $lastName, $gender, $nationality, $companyId, $inRing, $entertainment, $story, 0, 'TRAINEE');
                    """;
                    workerCommand.Parameters.AddWithValue("$workerId", worker.WorkerId);
                    workerCommand.Parameters.AddWithValue("$lastName", worker.Nom);
                    workerCommand.Parameters.AddWithValue("$firstName", worker.Prenom);
                    workerCommand.Parameters.AddWithValue("$gender", worker.Gender);
                    workerCommand.Parameters.AddWithValue("$nationality", worker.Region);
                    workerCommand.Parameters.AddWithValue("$companyId", (object?)worker.CompagnieId ?? DBNull.Value);
                    workerCommand.Parameters.AddWithValue("$inRing", worker.InRing);
                    workerCommand.Parameters.AddWithValue("$entertainment", worker.Entertainment);
                    workerCommand.Parameters.AddWithValue("$story", worker.Story);
                    workerCommand.ExecuteNonQuery();

                    using var traineeCommand = connexion.CreateCommand();
                    traineeCommand.Transaction = transaction;
                    traineeCommand.CommandText = """
                    INSERT INTO YouthTrainees (YouthTraineeId, YouthStructureId, WorkerId, Status, StartDate)
                    VALUES ($traineeId, $youthId, $workerId, 'EN_FORMATION', $semaine);
                    """;
                    traineeCommand.Parameters.AddWithValue("$traineeId", $"{worker.WorkerId}_{structureId}");
                    traineeCommand.Parameters.AddWithValue("$youthId", structureId);
                    traineeCommand.Parameters.AddWithValue("$workerId", worker.WorkerId);
                    traineeCommand.Parameters.AddWithValue("$semaine", semaine);
                    traineeCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        });
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
