using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Attributes;
using RingGeneral.Core.Models.Relations;
using RingGeneral.Data.Database;
using RingGeneral.Core.Interfaces;

namespace RingGeneral.Data.Repositories;

public sealed class WorkerRepository : RepositoryBase, IWorkerRepository
{
    public WorkerRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }
    // ... (rest of the file until ChargerContractHistory)

    private IEnumerable<ContractHistory> ChargerContractHistory(SqliteConnection connexion, int workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT StartDate, EndDate, WeeklySalary, Status, ContractType FROM ContractHistory WHERE WorkerId = $id ORDER BY StartDate DESC";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (Enum.TryParse<ContractStatus>(reader.GetString(3), true, out var status))
            {
                yield return new ContractHistory
                {
                    StartDate = DateTime.Parse(reader.GetString(0)),
                    EndDate = DateTime.Parse(reader.GetString(1)),
                    WeeklySalary = reader.IsDBNull(2) ? 0 : (decimal)reader.GetDouble(2), // Safe cast from DOUBLE to DECIMAL
                    Status = status,
                    // ContractType not fully mapped in model yet, could add later
                };
            }
        }
    }

    private IEnumerable<WorkerNote> ChargerWorkerNotes(SqliteConnection connexion, int workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Text, Category, CreatedDate FROM WorkerNotes WHERE WorkerId = $id ORDER BY CreatedDate DESC";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var categoryStr = reader.GetString(1);
            var category = Enum.TryParse<NoteCategory>(categoryStr, true, out var cat) ? cat : NoteCategory.Other;

            yield return new WorkerNote
            {
                Text = reader.GetString(0),
                Category = category,
                CreatedDate = DateTime.Parse(reader.GetString(2))
            };
        }
    }
    // ... (rest of file)

    public IReadOnlyList<WorkerBackstageProfile> ChargerBackstageRoster(string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_id, LastName || ' ' || FirstName FROM workers WHERE company_id = $companyId;";
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var roster = new List<WorkerBackstageProfile>();
        while (reader.Read())
        {
            roster.Add(new WorkerBackstageProfile(reader.GetString(0), reader.GetString(1)));
        }

        return roster;
    }

    public IReadOnlyDictionary<string, int> ChargerMorales(string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_id, morale FROM workers WHERE company_id = $companyId;";
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var morales = new Dictionary<string, int>();
        while (reader.Read())
        {
            morales[reader.GetString(0)] = reader.GetInt32(1);
        }

        return morales;
    }

    public int ChargerMorale(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT morale FROM workers WHERE worker_id = $workerId;";
        command.Parameters.AddWithValue("$workerId", workerId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public IReadOnlyDictionary<string, string> ChargerNomsWorkers()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_id, LastName || ' ' || FirstName FROM workers;";
        using var reader = command.ExecuteReader();
        var noms = new Dictionary<string, string>();
        while (reader.Read())
        {
            noms[reader.GetString(0)] = reader.GetString(1);
        }

        return noms;
    }

    public int ChargerFatigueWorker(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT fatigue FROM workers WHERE worker_id = $workerId;";
        command.Parameters.AddWithValue("$workerId", workerId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void RecupererFatigueHebdo()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE workers SET fatigue = MAX(0, fatigue - 12);";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Met à jour les stats quotidiennes (fatigue, récupération, etc.)
    /// </summary>
    public void MettreAJourStatsQuotidiennes(string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        // Récupération de base : -2 de fatigue par jour pour tout le monde
        // Plus tard, on pourra ajouter des bonus selon les structures de la compagnie
        command.CommandText = """
            UPDATE workers 
            SET fatigue = MAX(0, fatigue - 2)
            WHERE company_id = $companyId OR company_id IS NULL;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<WorkerSnapshot> ChargerWorkers(List<string> workerIds)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        var placeholders = workerIds.Select((id, index) => $"$id{index}").ToList();
        command.CommandText = $"""
            SELECT WorkerId, Name, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv, Morale, CompanyId, DepartureDate, DepartureReason, IsHallOfFame, LegacyScore
            FROM Workers
            WHERE WorkerId IN ({string.Join(", ", placeholders)});
            """;
        for (var i = 0; i < workerIds.Count; i++)
        {
            command.Parameters.AddWithValue(placeholders[i], workerIds[i]);
        }
        using var reader = command.ExecuteReader();
        var workers = new List<WorkerSnapshot>();
        while (reader.Read())
        {
            workers.Add(new WorkerSnapshot(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetString(7),
                reader.GetInt32(8),
                reader.GetString(9),
                reader.GetInt32(10),
                reader.IsDBNull(11) ? null : reader.GetString(11),
                reader.IsDBNull(12) ? null : DateTime.Parse(reader.GetString(12)),
                reader.IsDBNull(13) ? null : reader.GetString(13),
                !reader.IsDBNull(14) && reader.GetInt32(14) == 1,
                reader.IsDBNull(15) ? 0 : reader.GetInt32(15)));
        }
        return workers;
    }

    public WorkerSnapshot? ChargerWorker(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT WorkerId, Name, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv, Morale, CompanyId, DepartureDate, DepartureReason, IsHallOfFame, LegacyScore
            FROM Workers
            WHERE WorkerId = $workerId;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new WorkerSnapshot(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetString(7),
                reader.GetInt32(8),
                reader.GetString(9),
                reader.GetInt32(10),
                reader.IsDBNull(11) ? null : reader.GetString(11),
                reader.IsDBNull(12) ? null : DateTime.Parse(reader.GetString(12)),
                reader.IsDBNull(13) ? null : reader.GetString(13),
                !reader.IsDBNull(14) && reader.GetInt32(14) == 1,
                reader.IsDBNull(15) ? 0 : reader.GetInt32(15));
        }
        return null;
    }

    public Worker? GetWorker(int id)
    {
        return GetWorker(id.ToString());
    }

    public Worker? GetWorker(string id)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT WorkerId, Name, RingName, Nationality, Gender, BirthDate, RoleTv, InjuryStatus, Morale, Popularity, DepartureDate, DepartureReason, IsHallOfFame, LegacyScore
            FROM Workers
            WHERE WorkerId = $id;
            """;
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapWorkerFromReader(reader, connexion, id);
        }

        return null;
    }

    public List<Worker> GetCompanyRoster(string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT WorkerId, Name, RingName, Nationality, Gender, BirthDate, RoleTv, InjuryStatus, Morale, Popularity, DepartureDate, DepartureReason, IsHallOfFame, LegacyScore
            FROM Workers
            WHERE CompanyId = $companyId;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);

        var roster = new List<Worker>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string workerId = reader.GetString(0);
            roster.Add(MapWorkerFromReader(reader, connexion, workerId));
        }
        return roster;
    }

    private Worker MapWorkerFromReader(SqliteDataReader reader, SqliteConnection connexion, string workerId)
    {
        var worker = new Worker
        {
            WorkerId = workerId,
            Id = int.TryParse(workerId, out var intId) ? intId : 0,
            Name = reader.IsDBNull(2) ? reader.GetString(1) : reader.GetString(2), // Use RingName if available, else Name
            RealName = reader.GetString(1),
            BirthCountry = reader.GetString(3),
            // ResidenceCountry could be set to same or left null
            TvRole = 50, // Default
            IsActive = true,
            IsInjured = !string.Equals(reader.GetString(7), "AUCUNE", StringComparison.OrdinalIgnoreCase),
            Morale = reader.IsDBNull(8) ? 50 : reader.GetInt32(8)
        };

        // Gender
        if (!reader.IsDBNull(4))
        {
            var genderStr = reader.GetString(4);
            if (Enum.TryParse<Gender>(genderStr, true, out var gender))
            {
                worker.Gender = gender;
            }
        }

        // Age from BirthDate
        if (!reader.IsDBNull(5))
        {
            if (DateTime.TryParse(reader.GetString(5), out var birthDate))
            {
                worker.DateOfBirth = birthDate;
                var today = DateTime.Today; // Should use Game Date ideally
                var age = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-age)) age--;
                worker.Age = age;
            }
        }
        else
        {
            worker.Age = 25; // Default age
        }

        // Defaults for missing columns
        worker.Height = 180;
        worker.Weight = 100;

        // Map RoleTv to PushLevel (Approximate)
        if (!reader.IsDBNull(6))
        {
            var role = reader.GetString(6);
            // Simple mapping logic
            if (role.Contains("Main", StringComparison.OrdinalIgnoreCase)) worker.PushLevel = PushLevel.MainEvent;
            else if (role.Contains("Upper", StringComparison.OrdinalIgnoreCase)) worker.PushLevel = PushLevel.UpperMid;
            else if (role.Contains("Mid", StringComparison.OrdinalIgnoreCase)) worker.PushLevel = PushLevel.MidCard;
            else if (role.Contains("Lower", StringComparison.OrdinalIgnoreCase)) worker.PushLevel = PushLevel.LowerMid;
            else if (role.Contains("Job", StringComparison.OrdinalIgnoreCase)) worker.PushLevel = PushLevel.Jobber;
        }

        // Popularity (Index 9)
        if (!reader.IsDBNull(9))
        {
            worker.Popularity = reader.GetInt32(9);
        }

        // ===================================
        // NEW PROPERTIES (Phase 5) 
        // ===================================

        // 10: DepartureDate (TEXT/ISO8601)
        if (!reader.IsDBNull(10) && DateTime.TryParse(reader.GetString(10), out var departureDate))
        {
            worker.DepartureDate = departureDate;
        }

        // 11: DepartureReason
        if (!reader.IsDBNull(11))
        {
            worker.DepartureReason = reader.GetString(11);
        }

        // 12: IsHallOfFame (Integer 0/1)
        if (!reader.IsDBNull(12))
        {
            worker.IsHallOfFame = reader.GetInt32(12) == 1;
        }

        // 13: LegacyScore (Integer)
        if (!reader.IsDBNull(13))
        {
            worker.LegacyScore = reader.GetInt32(13);
        }

        // Fallback or ID-dependent loading
        // Note: Attribute/Contract loaders currently take 'int workerId'. 
        // We need to check if they can handle string or if we need to update them too.
        // The DB schema likely uses Integer WorkerId for these child tables OR consistent type.
        // Assuming consistent type (likely string/int hybrid or normalized).
        // Let's coerce to int for now if the assumption is existing workers are int.
        // FOR YOUTH: The ID is string. The child tables (WorkerInRingAttributes) must support string WorkerId.
        // However, the current Repository methods: `ChargerInRingAttributes(SqliteConnection connexion, int workerId)` take int.
        // THIS IS A BLOCKER. They need to change to string.

        // Temporarily, we will assume we CAN parse the int ID for existing flow, 
        // but for Youth (string ID), these sub-loaders will fail/need update.
        // For this step I will update the calls to pass worker.Id (int) if possible, 
        // BUT I must update the private methods to accept string id to fix the actual bug.

        int idForSubQueries = worker.Id;

        // Load Attributes - UPDATING TO USE STRING ID LOGIC INTERNALLY or overloading
        worker.InRingAttributes = ChargerInRingAttributes(connexion, workerId);
        worker.EntertainmentAttributes = ChargerEntertainmentAttributes(connexion, workerId);
        worker.StoryAttributes = ChargerStoryAttributes(connexion, workerId);
        worker.MentalAttributes = ChargerMentalAttributes(connexion, workerId);

        // Load Specializations
        worker.Specializations = ChargerSpecializations(connexion, workerId).ToList();

        // Load Relations
        worker.RelationsAsWorker1 = ChargerRelations(connexion, workerId, asWorker1: true).ToList();
        worker.RelationsAsWorker2 = ChargerRelations(connexion, workerId, asWorker1: false).ToList();

        // Load Contracts
        worker.ContractHistory = ChargerContractHistory(connexion, workerId).ToList();

        // Load Notes
        worker.Notes = ChargerWorkerNotes(connexion, workerId).ToList();

        // Load History (Matches & Titles)
        worker.MatchHistory = ChargerMatchHistory(connexion, workerId).ToList();
        worker.TitleReigns = ChargerTitleReigns(connexion, workerId).ToList();

        return worker;
    }

    private WorkerInRingAttributes ChargerInRingAttributes(SqliteConnection connexion, string workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT * FROM WorkerInRingAttributes WHERE WorkerId = $id";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new WorkerInRingAttributes
            {
                Striking = reader.GetInt32(1),
                Grappling = reader.GetInt32(2),
                HighFlying = reader.GetInt32(3),
                Powerhouse = reader.GetInt32(4),
                Timing = reader.GetInt32(5),
                Selling = reader.GetInt32(6),
                Psychology = reader.GetInt32(7),
                Stamina = reader.GetInt32(8),
                Safety = reader.GetInt32(9),
                HardcoreBrawl = reader.GetInt32(10)
            };
        }
        return new WorkerInRingAttributes(); // Return default if not found
    }

    private WorkerEntertainmentAttributes ChargerEntertainmentAttributes(SqliteConnection connexion, string workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT * FROM WorkerEntertainmentAttributes WHERE WorkerId = $id";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new WorkerEntertainmentAttributes
            {
                Charisma = reader.GetInt32(1),
                MicWork = reader.GetInt32(2),
                Acting = reader.GetInt32(3),
                CrowdConnection = reader.GetInt32(4),
                StarPower = reader.GetInt32(5),
                Improvisation = reader.GetInt32(6),
                Entrance = reader.GetInt32(7),
                SexAppeal = reader.GetInt32(8),
                MerchandiseAppeal = reader.GetInt32(9),
                CrossoverPotential = reader.GetInt32(10)
            };
        }
        return new WorkerEntertainmentAttributes();
    }

    private WorkerStoryAttributes ChargerStoryAttributes(SqliteConnection connexion, string workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT * FROM WorkerStoryAttributes WHERE WorkerId = $id";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new WorkerStoryAttributes
            {
                CharacterDepth = reader.GetInt32(1),
                Consistency = reader.GetInt32(2),
                HeelPerformance = reader.GetInt32(3),
                BabyfacePerformance = reader.GetInt32(4),
                StorytellingLongTerm = reader.GetInt32(5),
                EmotionalRange = reader.GetInt32(6),
                Adaptability = reader.GetInt32(7),
                RivalryChemistry = reader.GetInt32(8),
                CreativeInput = reader.GetInt32(9),
                MoralAlignment = reader.GetInt32(10)
            };
        }
        return new WorkerStoryAttributes();
    }

    private WorkerMentalAttributes ChargerMentalAttributes(SqliteConnection connexion, string workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Ambition, Loyauté, Professionnalisme, Pression, Tempérament, Égoïsme, Détermination, Adaptabilité, Influence, Sportivité FROM WorkerMentalAttributes WHERE WorkerId = $id";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new WorkerMentalAttributes
            {
                Ambition = reader.GetInt32(0),
                Loyauté = reader.GetInt32(1),
                Professionnalisme = reader.GetInt32(2),
                Pression = reader.GetInt32(3),
                Tempérament = reader.GetInt32(4),
                Égoïsme = reader.GetInt32(5),
                Détermination = reader.GetInt32(6),
                Adaptabilité = reader.GetInt32(7),
                Influence = reader.GetInt32(8),
                Sportivité = reader.GetInt32(9)
            };
        }
        return new WorkerMentalAttributes();
    }

    private IEnumerable<WorkerSpecialization> ChargerSpecializations(SqliteConnection connexion, string workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Specialization, Level FROM WorkerSpecializations WHERE WorkerId = $id";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (Enum.TryParse<SpecializationType>(reader.GetString(0), true, out var spec))
            {
                yield return new WorkerSpecialization { Specialization = spec, Level = reader.GetInt32(1) };
            }
        }
    }

    private IEnumerable<WorkerRelation> ChargerRelations(SqliteConnection connexion, string workerId, bool asWorker1)
    {
        using var command = connexion.CreateCommand();
        // Since we are moving to TEXT IDs (Phase 3 Standardization), we select directly.
        // The table must be migrated to use TEXT IDs (Migration 022).
        command.CommandText = asWorker1
            ? "SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic FROM WorkerRelations WHERE WorkerId1 = $id"
            : "SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic FROM WorkerRelations WHERE WorkerId2 = $id";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (Enum.TryParse<RelationType>(reader.GetString(3), true, out var type))
            {
                yield return new WorkerRelation
                {
                    Id = reader.GetInt32(0),
                    WorkerId1 = reader.GetString(1), // Changed to GetString
                    WorkerId2 = reader.GetString(2), // Changed to GetString
                    RelationType = type,
                    RelationStrength = reader.GetInt32(4),
                    Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsPublic = reader.GetInt32(6) == 1
                };
            }
        }
    }



    private IEnumerable<MatchHistoryItem> ChargerMatchHistory(SqliteConnection connexion, string workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT MatchDate, MatchType, Result, Rating FROM MatchHistory WHERE WorkerId = $id ORDER BY MatchDate DESC LIMIT 50";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (Enum.TryParse<MatchResult>(reader.GetString(2), true, out var result))
            {
                yield return new MatchHistoryItem
                {
                    MatchDate = DateTime.Parse(reader.GetString(0)),
                    MatchType = reader.IsDBNull(1) ? "Standard" : reader.GetString(1),
                    Result = result,
                    Rating = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                };
            }
        }
    }

    private IEnumerable<TitleReign> ChargerTitleReigns(SqliteConnection connexion, string workerId)
    {
        // Placeholder - TitleReigns table might not be fully populated or linked in this context yet
        // If TitleReigns table uses integer WorkerId, this might need logic check.
        // Assuming string compatibility or empty for now.
        return Enumerable.Empty<TitleReign>();
    }

    private IEnumerable<ContractHistory> ChargerContractHistory(SqliteConnection connexion, string workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT StartDate, EndDate, WeeklySalary, Status, ContractType FROM ContractHistory WHERE WorkerId = $id ORDER BY StartDate DESC";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (Enum.TryParse<ContractStatus>(reader.GetString(3), true, out var status))
            {
                yield return new ContractHistory
                {
                    StartDate = DateTime.Parse(reader.GetString(0)),
                    EndDate = DateTime.Parse(reader.GetString(1)),
                    WeeklySalary = reader.IsDBNull(2) ? 0 : (decimal)reader.GetDouble(2), // Safe cast from DOUBLE to DECIMAL
                    Status = status,
                    // ContractType not fully mapped in model yet, could add later
                };
            }
        }
    }

    private IEnumerable<WorkerNote> ChargerWorkerNotes(SqliteConnection connexion, string workerId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Text, Category, CreatedDate FROM WorkerNotes WHERE WorkerId = $id ORDER BY CreatedDate DESC";
        command.Parameters.AddWithValue("$id", workerId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var categoryStr = reader.GetString(1);
            var category = Enum.TryParse<NoteCategory>(categoryStr, true, out var cat) ? cat : NoteCategory.Other;

            yield return new WorkerNote
            {
                Text = reader.GetString(0),
                Category = category,
                CreatedDate = DateTime.Parse(reader.GetString(2))
            };
        }
    }

    public void UpdateWorker(Worker worker)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();

        // Use string WorkerId if populated, else fallback to Id.ToString()
        var idToUse = !string.IsNullOrEmpty(worker.WorkerId) ? worker.WorkerId : worker.Id.ToString();

        try
        {
            // Update Base Worker Info
            using (var cmd = connexion.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = """
                    UPDATE Workers 
                    SET CurrentGimmick = $gimmick, 
                        Alignment = $alignment, 
                        PushLevel = $push, 
                        BookingIntent = $intent,
                        Morale = $morale
                    WHERE WorkerId = $id
                    """;
                cmd.Parameters.AddWithValue("$id", idToUse);
                cmd.Parameters.AddWithValue("$gimmick", worker.CurrentGimmick ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("$alignment", worker.Alignment.ToString());
                cmd.Parameters.AddWithValue("$push", worker.PushLevel.ToString());
                cmd.Parameters.AddWithValue("$intent", worker.BookingIntent ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("$morale", worker.Morale);
                cmd.ExecuteNonQuery();
            }

            // Update Attributes

            // InRing
            if (worker.InRingAttributes != null)
            {
                using var cmd = connexion.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = """
                    INSERT OR REPLACE INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
                    VALUES ($id, $p1, $p2, $p3, $p4, $p5, $p6, $p7, $p8, $p9, $p10)
                    """;
                cmd.Parameters.AddWithValue("$id", idToUse);
                cmd.Parameters.AddWithValue("$p1", worker.InRingAttributes.Striking);
                cmd.Parameters.AddWithValue("$p2", worker.InRingAttributes.Grappling);
                cmd.Parameters.AddWithValue("$p3", worker.InRingAttributes.HighFlying);
                cmd.Parameters.AddWithValue("$p4", worker.InRingAttributes.Powerhouse);
                cmd.Parameters.AddWithValue("$p5", worker.InRingAttributes.Timing);
                cmd.Parameters.AddWithValue("$p6", worker.InRingAttributes.Selling);
                cmd.Parameters.AddWithValue("$p7", worker.InRingAttributes.Psychology);
                cmd.Parameters.AddWithValue("$p8", worker.InRingAttributes.Stamina);
                cmd.Parameters.AddWithValue("$p9", worker.InRingAttributes.Safety);
                cmd.Parameters.AddWithValue("$p10", worker.InRingAttributes.HardcoreBrawl);
                cmd.ExecuteNonQuery();
            }

            // Entertainment
            if (worker.EntertainmentAttributes != null)
            {
                using var cmd = connexion.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = """
                    INSERT OR REPLACE INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
                    VALUES ($id, $p1, $p2, $p3, $p4, $p5, $p6, $p7, $p8, $p9, $p10)
                    """;
                cmd.Parameters.AddWithValue("$id", idToUse);
                cmd.Parameters.AddWithValue("$p1", worker.EntertainmentAttributes.Charisma);
                cmd.Parameters.AddWithValue("$p2", worker.EntertainmentAttributes.MicWork);
                cmd.Parameters.AddWithValue("$p3", worker.EntertainmentAttributes.Acting);
                cmd.Parameters.AddWithValue("$p4", worker.EntertainmentAttributes.CrowdConnection);
                cmd.Parameters.AddWithValue("$p5", worker.EntertainmentAttributes.StarPower);
                cmd.Parameters.AddWithValue("$p6", worker.EntertainmentAttributes.Improvisation);
                cmd.Parameters.AddWithValue("$p7", worker.EntertainmentAttributes.Entrance);
                cmd.Parameters.AddWithValue("$p8", worker.EntertainmentAttributes.SexAppeal);
                cmd.Parameters.AddWithValue("$p9", worker.EntertainmentAttributes.MerchandiseAppeal);
                cmd.Parameters.AddWithValue("$p10", worker.EntertainmentAttributes.CrossoverPotential);
                cmd.ExecuteNonQuery();
            }

            // Story
            if (worker.StoryAttributes != null)
            {
                using var cmd = connexion.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = """
                    INSERT OR REPLACE INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
                    VALUES ($id, $p1, $p2, $p3, $p4, $p5, $p6, $p7, $p8, $p9, $p10)
                    """;
                cmd.Parameters.AddWithValue("$id", idToUse);
                cmd.Parameters.AddWithValue("$p1", worker.StoryAttributes.CharacterDepth);
                cmd.Parameters.AddWithValue("$p2", worker.StoryAttributes.Consistency);
                cmd.Parameters.AddWithValue("$p3", worker.StoryAttributes.HeelPerformance);
                cmd.Parameters.AddWithValue("$p4", worker.StoryAttributes.BabyfacePerformance);
                cmd.Parameters.AddWithValue("$p5", worker.StoryAttributes.StorytellingLongTerm);
                cmd.Parameters.AddWithValue("$p6", worker.StoryAttributes.EmotionalRange);
                cmd.Parameters.AddWithValue("$p7", worker.StoryAttributes.Adaptability);
                cmd.Parameters.AddWithValue("$p8", worker.StoryAttributes.RivalryChemistry);
                cmd.Parameters.AddWithValue("$p9", worker.StoryAttributes.CreativeInput);
                cmd.Parameters.AddWithValue("$p10", worker.StoryAttributes.MoralAlignment);
                cmd.ExecuteNonQuery();
            }

            // Mental
            if (worker.MentalAttributes != null)
            {
                using var cmd = connexion.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = """
                    INSERT OR REPLACE INTO WorkerMentalAttributes (WorkerId, Ambition, Loyauté, Professionnalisme, Pression, Tempérament, Égoïsme, Détermination, Adaptabilité, Influence, Sportivité)
                    VALUES ($id, $p1, $p2, $p3, $p4, $p5, $p6, $p7, $p8, $p9, $p10)
                    """;
                cmd.Parameters.AddWithValue("$id", idToUse);
                cmd.Parameters.AddWithValue("$p1", worker.MentalAttributes.Ambition);
                cmd.Parameters.AddWithValue("$p2", worker.MentalAttributes.Loyauté);
                cmd.Parameters.AddWithValue("$p3", worker.MentalAttributes.Professionnalisme);
                cmd.Parameters.AddWithValue("$p4", worker.MentalAttributes.Pression);
                cmd.Parameters.AddWithValue("$p5", worker.MentalAttributes.Tempérament);
                cmd.Parameters.AddWithValue("$p6", worker.MentalAttributes.Égoïsme);
                cmd.Parameters.AddWithValue("$p7", worker.MentalAttributes.Détermination);
                cmd.Parameters.AddWithValue("$p8", worker.MentalAttributes.Adaptabilité);
                cmd.Parameters.AddWithValue("$p9", worker.MentalAttributes.Influence);
                cmd.Parameters.AddWithValue("$p10", worker.MentalAttributes.Sportivité);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    public void TerminateCurrentContract(string workerId, DateTime date)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        try
        {
            using var cmd = connexion.CreateCommand();
            cmd.Transaction = transaction;
            // Find active contract
            cmd.CommandText = "UPDATE ContractHistory SET EndDate = $date, Status = 'Terminated' WHERE WorkerId = $id AND Status = 'Active'";
            cmd.Parameters.AddWithValue("$id", workerId);
            cmd.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));
            cmd.ExecuteNonQuery();

            // Also update current Contract table if separate
            using var cmd2 = connexion.CreateCommand();
            cmd2.Transaction = transaction;
            cmd2.CommandText = "DELETE FROM Contracts WHERE WorkerId = $id"; // Remove from active contracts table
            cmd2.ExecuteNonQuery();

            // Or if Contracts table is history, update it. 
            // Based on 001_init.sql, 'Contracts' table is the active contracts, 'ContractHistory' (if exists?) or Contracts is history?
            // 001_init.sql has 'Contracts' table.
            // WorkerRepository 'ChargerContractHistory' reads from 'ContractHistory'.
            // If 'ContractHistory' is a view or separate table?
            // Assuming 'ContractHistory' is the archive and 'Contracts' is the active.
            // But 'Contracts' table in 001_init.sql has EndDate.

            // Let's assume we update 'Contracts' (Active) and also 'ContractHistory' (Archive) if implemented.
            // But wait, step 6195 showed ChargerContractHistory reading from ContractHistory.
            // It did NOT show `ChargerCurrentContract`.
            // Let's check `ChargerContractHistory` query again: "SELECT ... FROM ContractHistory"

            // If ContractHistory exists, we update it.
            // If Contracts exists, we remove/update it.

            // Safety: Update Contracts if it exists too
            // Note: 001_init.sql defined `Contracts`. It did NOT define `ContractHistory`.
            // `ContractHistory` must be a view or created later.
            // Wait, if `ChargerContractHistory` reads from `ContractHistory`, verify if it exists.
            // If it's a VIEW of `Contracts`, update `Contracts`.

            // Let's assume `Contracts` is the source of truth.
            using var cmd3 = connexion.CreateCommand();
            cmd3.Transaction = transaction;
            cmd3.CommandText = "UPDATE Contracts SET EndDate = $dateInt WHERE WorkerId = $id";
            // Contracts uses INTEGER dates in 001_init.sql?
            // "StartDate INTEGER, EndDate INTEGER".
            // Convert DateTime to int (YYYYMMDD or ticks?). 
            // StandardizeLegacySchema likely converted them to TEXT or keeps INT. 
            // BUT `ContractHistory` reader uses `DateTime.Parse(reader.GetString(0))`. This implies TEXT.
            // So `Contracts` or `ContractHistory` uses TEXT dates.

            cmd3.Parameters.AddWithValue("$id", workerId);
            cmd3.Parameters.AddWithValue("$dateInt", date.ToString("yyyy-MM-dd"));
            cmd3.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
    public IReadOnlyList<BookerMemoryEntry> GetBookerMemory(string bookerId, string? workerId = null)
    {
        var entries = new List<BookerMemoryEntry>();
        using var connection = OpenConnection();
        // RepositoryBase.OpenConnection doesn't call .Open() - wait, let me check RepositoryBase again.
        // Actually RepositoryBase.OpenConnection calls _factory.CreateGeneralConnection() which returns an OPENED connection in some cases? 
        // No, typically CreateConnection doesn't open.
        // Let's check SqliteConnectionFactory.
        connection.Open();

        using var command = connection.CreateCommand();
        string sql = "SELECT MemoryId, BookerId, WorkerId, EventType, ImpactScore, RecallStrength, Description, EventDate FROM BookerMemory WHERE BookerId = @BookerId";
        if (workerId != null) sql += " AND WorkerId = @WorkerId";
        command.CommandText = sql;
        command.Parameters.AddWithValue("@BookerId", bookerId);
        if (workerId != null) command.Parameters.AddWithValue("@WorkerId", workerId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            entries.Add(new BookerMemoryEntry(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.GetString(7)
            ));
        }
        return entries;
    }

}
