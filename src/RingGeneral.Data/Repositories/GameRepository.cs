using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class GameRepository
{
    private readonly SqliteConnectionFactory _factory;

    public GameRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public void Initialiser()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var commande = connexion.CreateCommand();
        commande.CommandText = """
            CREATE TABLE IF NOT EXISTS companies (
                company_id TEXT PRIMARY KEY,
                nom TEXT NOT NULL,
                region TEXT NOT NULL,
                prestige INTEGER NOT NULL,
                tresorerie REAL NOT NULL,
                audience_moyenne INTEGER NOT NULL,
                reach INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS workers (
                worker_id TEXT PRIMARY KEY,
                nom TEXT NOT NULL,
                prenom TEXT NOT NULL,
                company_id TEXT,
                in_ring INTEGER NOT NULL,
                entertainment INTEGER NOT NULL,
                story INTEGER NOT NULL,
                popularite INTEGER NOT NULL,
                fatigue INTEGER NOT NULL,
                blessure TEXT NOT NULL,
                momentum INTEGER NOT NULL,
                role_tv TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS titles (
                title_id TEXT PRIMARY KEY,
                nom TEXT NOT NULL,
                prestige INTEGER NOT NULL,
                detenteur_id TEXT,
                company_id TEXT
            );
            CREATE TABLE IF NOT EXISTS storylines (
                storyline_id TEXT PRIMARY KEY,
                nom TEXT NOT NULL,
                heat INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS storyline_participants (
                storyline_id TEXT NOT NULL,
                worker_id TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS shows (
                show_id TEXT PRIMARY KEY,
                nom TEXT NOT NULL,
                semaine INTEGER NOT NULL,
                region TEXT NOT NULL,
                duree INTEGER NOT NULL,
                compagnie_id TEXT NOT NULL,
                tv_deal_id TEXT
            );
            CREATE TABLE IF NOT EXISTS segments (
                segment_id TEXT PRIMARY KEY,
                show_id TEXT NOT NULL,
                ordre INTEGER NOT NULL,
                type TEXT NOT NULL,
                duree INTEGER NOT NULL,
                participants_json TEXT NOT NULL,
                storyline_id TEXT,
                title_id TEXT,
                main_event INTEGER NOT NULL,
                intensite INTEGER NOT NULL,
                vainqueur_id TEXT,
                perdant_id TEXT
            );
            CREATE TABLE IF NOT EXISTS chimies (
                worker_a TEXT NOT NULL,
                worker_b TEXT NOT NULL,
                valeur INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS finances (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                show_id TEXT NOT NULL,
                type TEXT NOT NULL,
                montant REAL NOT NULL,
                libelle TEXT NOT NULL,
                semaine INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS show_history (
                show_id TEXT NOT NULL,
                semaine INTEGER NOT NULL,
                note INTEGER NOT NULL,
                audience INTEGER NOT NULL,
                resume TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS segment_history (
                segment_id TEXT NOT NULL,
                show_id TEXT NOT NULL,
                semaine INTEGER NOT NULL,
                note INTEGER NOT NULL,
                resume TEXT NOT NULL,
                details_json TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS inbox_items (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                type TEXT NOT NULL,
                titre TEXT NOT NULL,
                contenu TEXT NOT NULL,
                semaine INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS contracts (
                worker_id TEXT NOT NULL,
                company_id TEXT NOT NULL,
                fin_semaine INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS youth_structures (
                youth_id TEXT PRIMARY KEY,
                company_id TEXT NOT NULL,
                nom TEXT NOT NULL,
                type TEXT NOT NULL,
                region TEXT NOT NULL,
                budget_annuel INTEGER NOT NULL,
                capacite_max INTEGER NOT NULL,
                niveau_equipements INTEGER NOT NULL,
                qualite_coaching INTEGER NOT NULL,
                philosophie TEXT NOT NULL,
                actif INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS youth_trainees (
                worker_id TEXT NOT NULL,
                youth_id TEXT NOT NULL,
                statut TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS youth_generation_state (
                youth_id TEXT PRIMARY KEY,
                derniere_generation_semaine INTEGER
            );
            CREATE TABLE IF NOT EXISTS worker_attributes (
                worker_id TEXT NOT NULL,
                attribut_id TEXT NOT NULL,
                valeur INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS worker_generation_counters (
                annee INTEGER NOT NULL,
                scope_type TEXT NOT NULL,
                scope_id TEXT NOT NULL,
                worker_type TEXT NOT NULL,
                count INTEGER NOT NULL,
                PRIMARY KEY (annee, scope_type, scope_id, worker_type)
            );
            CREATE TABLE IF NOT EXISTS worker_generation_events (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                worker_id TEXT NOT NULL,
                worker_type TEXT NOT NULL,
                semaine INTEGER NOT NULL,
                youth_id TEXT,
                region TEXT NOT NULL,
                company_id TEXT
            );
            CREATE TABLE IF NOT EXISTS game_settings (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                youth_generation_mode TEXT NOT NULL,
                world_generation_mode TEXT NOT NULL,
                semaine_pivot_annuelle INTEGER
            );
            CREATE TABLE IF NOT EXISTS popularity_regionale (
                entity_type TEXT NOT NULL,
                entity_id TEXT NOT NULL,
                region TEXT NOT NULL,
                valeur INTEGER NOT NULL,
                UNIQUE(entity_type, entity_id, region)
            );
            CREATE INDEX IF NOT EXISTS idx_workers_company ON workers(company_id);
            CREATE INDEX IF NOT EXISTS idx_workers_popularite ON workers(popularite);
            CREATE INDEX IF NOT EXISTS idx_contracts_enddate ON contracts(fin_semaine);
            CREATE INDEX IF NOT EXISTS idx_contracts_company ON contracts(company_id);
            CREATE INDEX IF NOT EXISTS idx_titles_company ON titles(company_id);
            CREATE INDEX IF NOT EXISTS idx_youth_company ON youth_structures(company_id);
            CREATE INDEX IF NOT EXISTS idx_youth_region ON youth_structures(region);
            CREATE INDEX IF NOT EXISTS idx_youth_trainees_youth ON youth_trainees(youth_id);
            CREATE INDEX IF NOT EXISTS idx_worker_attributes_worker ON worker_attributes(worker_id);
            CREATE INDEX IF NOT EXISTS idx_generation_events_semaine ON worker_generation_events(semaine);
            """;
        commande.ExecuteNonQuery();

        AssurerColonnesSupplementaires(connexion);

        using var countCommand = connexion.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(1) FROM companies";
        var count = Convert.ToInt32(countCommand.ExecuteScalar());
        if (count == 0)
        {
            SeedDatabase(connexion);
        }
    }

    public ShowContext? ChargerShowContext(string showId)
    {
        AjouterColonneSiAbsente(connexion, "workers", "company_id", "TEXT");
        AjouterColonneSiAbsente(connexion, "workers", "type_worker", "TEXT");
        AjouterColonneSiAbsente(connexion, "titles", "company_id", "TEXT");
    }

    private static void AjouterColonneSiAbsente(SqliteConnection connexion, string table, string colonne, string type)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            return null;
        }

        var compagnie = ChargerCompagnie(connexion, show.CompagnieId);
        if (compagnie is null)
        {
            return null;
        }

        var segments = ChargerSegments(connexion, showId);
        var participantsIds = segments.SelectMany(segment => segment.Participants).Distinct().ToList();
        var workers = ChargerWorkers(connexion, participantsIds);
        var titres = ChargerTitres(connexion);
        var storylines = ChargerStorylines(connexion);
        var chimies = ChargerChimies(connexion);

        return new ShowContext(show, compagnie, workers, titres, storylines, segments, chimies);
    }

    public BookingPlan ChargerBookingPlan(ShowContext context)
    {
        var segments = context.Segments.Select(segment => new SegmentSimulationContext(
            segment.SegmentId,
            segment.TypeSegment,
            segment.Participants,
            segment.DureeMinutes,
            segment.EstMainEvent,
            segment.StorylineId,
            segment.TitreId,
            segment.Intensite,
            segment.VainqueurId,
            segment.PerdantId,
            context.Workers.Where(worker => segment.Participants.Contains(worker.WorkerId)).ToList()))
            .ToList();

        var etat = context.Workers.ToDictionary(
            worker => worker.WorkerId,
            worker => new WorkerHealth(worker.Fatigue, worker.Blessure));

        return new BookingPlan(context.Show.ShowId, segments, context.Show.DureeMinutes, etat);
    }

    public void EnregistrerRapport(ShowReport rapport)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();

        using var showCommand = connexion.CreateCommand();
        showCommand.Transaction = transaction;
        showCommand.CommandText = """
            INSERT INTO ShowHistory (ShowId, Week, Note, Audience, Summary)
            VALUES ($showId, (SELECT Week FROM Shows WHERE ShowId = $showId), $note, $audience, $resume);
            """;
        showCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
        showCommand.Parameters.AddWithValue("$note", rapport.NoteGlobale);
        showCommand.Parameters.AddWithValue("$audience", rapport.Audience);
        showCommand.Parameters.AddWithValue("$resume", string.Join(" | ", rapport.PointsCles));
        showCommand.ExecuteNonQuery();

        foreach (var segment in rapport.Segments)
        {
            var details = string.Join(", ",
                segment.Facteurs.Select(facteur => $"{facteur.Libelle} {facteur.Impact:+#;-#;0}"));

            using var segmentCommand = connexion.CreateCommand();
            segmentCommand.Transaction = transaction;
            segmentCommand.CommandText = """
                INSERT INTO SegmentResults (ShowSegmentId, ShowId, Week, Note, Summary, Details)
                VALUES ($segmentId, $showId, (SELECT Week FROM Shows WHERE ShowId = $showId), $note, $resume, $details);
                """;
            segmentCommand.Parameters.AddWithValue("$segmentId", segment.SegmentId);
            segmentCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
            segmentCommand.Parameters.AddWithValue("$note", segment.Note);
            segmentCommand.Parameters.AddWithValue("$resume", $"Segment {segment.TypeSegment} - Note {segment.Note}");
            segmentCommand.Parameters.AddWithValue("$details", details);
            segmentCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public void AppliquerDelta(string showId, GameStateDelta delta)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();
        var regionId = ChargerRegionShow(connexion, showId);

        foreach (var (workerId, fatigueDelta) in delta.FatigueDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE Workers
                SET Fatigue = MAX(0, MIN(100, Fatigue + $delta))
                WHERE WorkerId = $workerId;
                """;
            command.Parameters.AddWithValue("$delta", fatigueDelta);
            command.Parameters.AddWithValue("$workerId", workerId);
            command.ExecuteNonQuery();
        }

        foreach (var (workerId, blessure) in delta.Blessures)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE Workers SET InjuryStatus = $blessure WHERE WorkerId = $workerId;";
            command.Parameters.AddWithValue("$blessure", blessure);
            command.Parameters.AddWithValue("$workerId", workerId);
            command.ExecuteNonQuery();
        }

        foreach (var (workerId, momentum) in delta.MomentumDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE Workers SET Momentum = Momentum + $delta WHERE WorkerId = $workerId;";
            command.Parameters.AddWithValue("$delta", momentum);
            command.Parameters.AddWithValue("$workerId", workerId);
            command.ExecuteNonQuery();
        }

        foreach (var (workerId, popularite) in delta.PopulariteWorkersDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE Workers SET Popularity = MAX(0, MIN(100, Popularity + $delta)) WHERE WorkerId = $workerId;";
            command.Parameters.AddWithValue("$delta", popularite);
            command.Parameters.AddWithValue("$workerId", workerId);
            command.ExecuteNonQuery();

            using var popCommand = connexion.CreateCommand();
            popCommand.Transaction = transaction;
            popCommand.CommandText = """
                INSERT INTO WorkerPopularityByRegion (WorkerId, RegionId, Popularity)
                VALUES ($workerId, $region, 50)
                ON CONFLICT(WorkerId, RegionId)
                DO UPDATE SET Popularity = MAX(0, MIN(100, Popularity + $delta));
                """;
            popCommand.Parameters.AddWithValue("$workerId", workerId);
            popCommand.Parameters.AddWithValue("$region", regionId);
            popCommand.Parameters.AddWithValue("$delta", popularite);
            popCommand.ExecuteNonQuery();
        }

        foreach (var (companyId, deltaPop) in delta.PopulariteCompagnieDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE Companies SET Prestige = MAX(0, MIN(100, Prestige + $delta)) WHERE CompanyId = $companyId;";
            command.Parameters.AddWithValue("$delta", deltaPop);
            command.Parameters.AddWithValue("$companyId", companyId);
            command.ExecuteNonQuery();
        }

        foreach (var (storylineId, deltaHeat) in delta.StorylineHeatDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE Storylines SET Heat = MAX(0, MIN(100, Heat + $delta)) WHERE StorylineId = $storylineId;";
            command.Parameters.AddWithValue("$delta", deltaHeat);
            command.Parameters.AddWithValue("$storylineId", storylineId);
            command.ExecuteNonQuery();
        }

        foreach (var (titreId, deltaPrestige) in delta.TitrePrestigeDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE Titles SET Prestige = MAX(0, MIN(100, Prestige + $delta)) WHERE TitleId = $titleId;";
            command.Parameters.AddWithValue("$delta", deltaPrestige);
            command.Parameters.AddWithValue("$titleId", titreId);
            command.ExecuteNonQuery();
        }

        foreach (var transactionFin in delta.Finances)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO FinanceTransactions (CompanyId, ShowId, Week, Category, Amount, Description)
                VALUES ((SELECT CompanyId FROM Shows WHERE ShowId = $showId), $showId, (SELECT Week FROM Shows WHERE ShowId = $showId), $type, $montant, $libelle);
                """;
            command.Parameters.AddWithValue("$showId", showId);
            command.Parameters.AddWithValue("$type", transactionFin.Type);
            command.Parameters.AddWithValue("$montant", transactionFin.Montant);
            command.Parameters.AddWithValue("$libelle", transactionFin.Libelle);
            command.ExecuteNonQuery();

            using var treasuryCommand = connexion.CreateCommand();
            treasuryCommand.Transaction = transaction;
            treasuryCommand.CommandText = "UPDATE Companies SET Treasury = Treasury + $montant WHERE CompanyId = (SELECT CompanyId FROM Shows WHERE ShowId = $showId);";
            treasuryCommand.Parameters.AddWithValue("$montant", transactionFin.Montant);
            treasuryCommand.Parameters.AddWithValue("$showId", showId);
            treasuryCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private static string ChargerRegionShow(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT RegionId FROM Shows WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        return Convert.ToString(command.ExecuteScalar()) ?? "";
    }

    public IReadOnlyList<InboxItem> ChargerInbox()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Type, Title, Content, Week FROM InboxItems ORDER BY Week DESC, InboxItemId DESC;";
        using var reader = command.ExecuteReader();
        var items = new List<InboxItem>();
        while (reader.Read())
        {
            items.Add(new InboxItem(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3)));
        }

        return items;
    }

    public string ChargerCompagnieIdPourShow(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT CompanyId FROM Shows WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        return Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
    }

    public IReadOnlyList<CompanyState> ChargerCompagnies()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT CompanyId, Name, RegionId, Prestige, Treasury, AverageAudience, Reach
            FROM Companies;
            """;
        using var reader = command.ExecuteReader();
        var compagnies = new List<CompanyState>();
        while (reader.Read())
        {
            compagnies.Add(new CompanyState(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetDouble(4),
                reader.GetInt32(5),
                reader.GetInt32(6)));
        }

        return compagnies;
    }

    public void AppliquerImpactCompagnie(string compagnieId, int deltaPrestige, double deltaTresorerie)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE Companies
            SET Prestige = MAX(0, MIN(100, Prestige + $deltaPrestige)),
                Treasury = Treasury + $deltaTresorerie
            WHERE CompanyId = $companyId;
            """;
        command.Parameters.AddWithValue("$deltaPrestige", deltaPrestige);
        command.Parameters.AddWithValue("$deltaTresorerie", deltaTresorerie);
        command.Parameters.AddWithValue("$companyId", compagnieId);
        command.ExecuteNonQuery();
    }

    public void AjouterInboxItem(InboxItem item)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "INSERT INTO InboxItems (Type, Title, Content, Week) VALUES ($type, $titre, $contenu, $semaine);";
        command.Parameters.AddWithValue("$type", item.Type);
        command.Parameters.AddWithValue("$titre", item.Titre);
        command.Parameters.AddWithValue("$contenu", item.Contenu);
        command.Parameters.AddWithValue("$semaine", item.Semaine);
        command.ExecuteNonQuery();
    }

    public int IncrementerSemaine(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE Shows SET Week = Week + 1 WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        command.ExecuteNonQuery();

        using var weekCommand = connexion.CreateCommand();
        weekCommand.CommandText = "SELECT Week FROM Shows WHERE ShowId = $showId;";
        weekCommand.Parameters.AddWithValue("$showId", showId);
        return Convert.ToInt32(weekCommand.ExecuteScalar());
    }

    public IReadOnlyList<(string WorkerId, int FinSemaine)> ChargerContracts()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT WorkerId, EndDate FROM Contracts;";
        using var reader = command.ExecuteReader();
        var contracts = new List<(string, int)>();
        while (reader.Read())
        {
            contracts.Add((reader.GetString(0), reader.GetInt32(1)));
        }

        return contracts;
    }

    public IReadOnlyDictionary<string, string> ChargerNomsWorkers()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT WorkerId, Name FROM Workers;";
        using var reader = command.ExecuteReader();
        var noms = new Dictionary<string, string>();
        while (reader.Read())
        {
            noms[reader.GetString(0)] = reader.GetString(1);
        }

        return noms;
    }

    public void RecupererFatigueHebdo()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE Workers SET Fatigue = MAX(0, Fatigue - 12);";
        command.ExecuteNonQuery();
    }

    public ShowDefinition ChargerShowDefinition(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
        return ChargerShow(connexion, showId);
    }

    public WorkerGenerationOptions ChargerParametresGeneration()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT youth_generation_mode, world_generation_mode, semaine_pivot_annuelle FROM game_settings WHERE id = 1;";
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var youthMode = Enum.TryParse<YouthGenerationMode>(reader.GetString(0), out var ym)
                ? ym
                : YouthGenerationMode.Realiste;
            var worldMode = Enum.TryParse<WorldGenerationMode>(reader.GetString(1), out var wm)
                ? wm
                : WorldGenerationMode.Desactivee;
            var pivot = reader.IsDBNull(2) ? null : reader.GetInt32(2);
            return new WorkerGenerationOptions(youthMode, worldMode, pivot);
        }

        return new WorkerGenerationOptions(YouthGenerationMode.Realiste, WorldGenerationMode.Desactivee, null);
    }

    public void SauvegarderParametresGeneration(WorkerGenerationOptions options)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO game_settings (id, youth_generation_mode, world_generation_mode, semaine_pivot_annuelle)
            VALUES (1, $youthMode, $worldMode, $pivot)
            ON CONFLICT(id) DO UPDATE SET
                youth_generation_mode = excluded.youth_generation_mode,
                world_generation_mode = excluded.world_generation_mode,
                semaine_pivot_annuelle = excluded.semaine_pivot_annuelle;
            """;
        command.Parameters.AddWithValue("$youthMode", options.YouthMode.ToString());
        command.Parameters.AddWithValue("$worldMode", options.WorldMode.ToString());
        command.Parameters.AddWithValue("$pivot", options.SemainePivotAnnuelle.HasValue ? options.SemainePivotAnnuelle.Value : DBNull.Value);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<YouthStructureState> ChargerYouthStructuresPourGeneration()
    {
        using var connexion = _factory.OuvrirConnexion();
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

    public GenerationCounters ChargerGenerationCounters(int annee)
    {
        using var connexion = _factory.OuvrirConnexion();
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

        using var connexion = _factory.OuvrirConnexion();
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
                    INSERT INTO youth_trainees (worker_id, youth_id, statut)
                    VALUES ($workerId, $youthId, 'EN_FORMATION');
                    """;
                youthCommand.Parameters.AddWithValue("$workerId", worker.WorkerId);
                youthCommand.Parameters.AddWithValue("$youthId", worker.YouthId);
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

    private static ShowDefinition ChargerShow(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ShowId, Name, Week, RegionId, DurationMinutes, CompanyId, TvDealId
            FROM Shows
            WHERE ShowId = $showId;
            """;
        command.Parameters.AddWithValue("$showId", showId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ShowDefinition(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetInt32(2),
            reader.GetString(3),
            reader.GetInt32(4),
            reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6));
    }

    private static CompanyState? ChargerCompagnie(SqliteConnection connexion, string companyId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT CompanyId, Name, RegionId, Prestige, Treasury, AverageAudience, Reach
            FROM Companies
            WHERE CompanyId = $companyId;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new CompanyState(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetDouble(4),
            reader.GetInt32(5),
            reader.GetInt32(6));
    }

    private List<SegmentDefinition> ChargerSegments(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ShowSegmentId, SegmentType, DurationMinutes, StorylineId, TitleId, IsMainEvent, Intensity, WinnerWorkerId, LoserWorkerId
            FROM ShowSegments
            WHERE ShowId = $showId
            ORDER BY OrderIndex ASC;
            """;
        command.Parameters.AddWithValue("$showId", showId);
        using var reader = command.ExecuteReader();
        var segments = new List<SegmentDefinition>();
        while (reader.Read())
        {
            segments.Add(new SegmentDefinition(
                reader.GetString(0),
                reader.GetString(1),
                Array.Empty<string>(),
                reader.GetInt32(2),
                reader.GetInt32(5) == 1,
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.IsDBNull(8) ? null : reader.GetString(8)));
        }

        var participantsMap = ChargerParticipants(connexion, segments.Select(segment => segment.SegmentId).ToList());
        return segments
            .Select(segment => segment with { Participants = participantsMap.GetValueOrDefault(segment.SegmentId, new List<string>()) })
            .ToList();
    }

    private static Dictionary<string, List<string>> ChargerParticipants(SqliteConnection connexion, IReadOnlyList<string> segmentIds)
    {
        var participants = segmentIds.ToDictionary(id => id, _ => new List<string>());
        if (segmentIds.Count == 0)
        {
            return participants;
        }

        using var command = connexion.CreateCommand();
        var placeholders = segmentIds.Select((id, index) => $"$id{index}").ToList();
        command.CommandText = $"SELECT ShowSegmentId, WorkerId FROM SegmentParticipants WHERE ShowSegmentId IN ({string.Join(", ", placeholders)});";
        for (var i = 0; i < segmentIds.Count; i++)
        {
            command.Parameters.AddWithValue(placeholders[i], segmentIds[i]);
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var segmentId = reader.GetString(0);
            var workerId = reader.GetString(1);
            if (participants.TryGetValue(segmentId, out var list))
            {
                list.Add(workerId);
            }
        }

        return participants;
    }

    private static List<WorkerSnapshot> ChargerWorkers(SqliteConnection connexion, IReadOnlyList<string> workerIds)
    {
        if (workerIds.Count == 0)
        {
            return new List<WorkerSnapshot>();
        }

        using var command = connexion.CreateCommand();
        var placeholders = workerIds.Select((id, index) => $"$id{index}").ToList();
        command.CommandText = $"""
            SELECT WorkerId, Name, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv
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
                reader.GetString(9)));
        }

        return workers;
    }

    private static List<TitleInfo> ChargerTitres(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT TitleId, Name, Prestige, HolderWorkerId FROM Titles;";
        using var reader = command.ExecuteReader();
        var titres = new List<TitleInfo>();
        while (reader.Read())
        {
            titres.Add(new TitleInfo(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetString(3)));
        }

        return titres;
    }

    private static List<StorylineInfo> ChargerStorylines(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT StorylineId, Name, Heat FROM Storylines;";
        using var reader = command.ExecuteReader();
        var storylines = new List<StorylineInfo>();
        while (reader.Read())
        {
            var storylineId = reader.GetString(0);
            storylines.Add(new StorylineInfo(
                storylineId,
                reader.GetString(1),
                reader.GetInt32(2),
                ChargerStorylineParticipants(connexion, storylineId)));
        }

        return storylines;
    }

    private static List<string> ChargerStorylineParticipants(SqliteConnection connexion, string storylineId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT WorkerId FROM StorylineParticipants WHERE StorylineId = $storylineId;";
        command.Parameters.AddWithValue("$storylineId", storylineId);
        using var reader = command.ExecuteReader();
        var participants = new List<string>();
        while (reader.Read())
        {
            participants.Add(reader.GetString(0));
        }

        return participants;
    }

    private static Dictionary<string, int> ChargerChimies(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT WorkerA, WorkerB, Value FROM Chimies;";
        using var reader = command.ExecuteReader();
        var chimies = new Dictionary<string, int>();
        while (reader.Read())
        {
            chimies[$"{reader.GetString(0)}|{reader.GetString(1)}"] = reader.GetInt32(2);
        }

        return chimies;
    }

    private void SeedDatabase(SqliteConnection connexion)
    {
        using var transaction = connexion.BeginTransaction();

        using var companyCommand = connexion.CreateCommand();
        companyCommand.Transaction = transaction;
        companyCommand.CommandText = """
            INSERT INTO companies (company_id, nom, region, prestige, tresorerie, audience_moyenne, reach)
            VALUES ('COMP-001', 'Ring General', 'FR', 55, 25000, 48, 5);
            """;
        companyCommand.ExecuteNonQuery();

        using var youthCommand = connexion.CreateCommand();
        youthCommand.Transaction = transaction;
        youthCommand.CommandText = """
            INSERT INTO youth_structures (youth_id, company_id, nom, type, region, budget_annuel, capacite_max, niveau_equipements, qualite_coaching, philosophie, actif)
            VALUES ('YOUTH-001', 'COMP-001', 'Ring General Academy', 'ACADEMY', 'FR', 85000, 24, 3, 12, 'HYBRIDE', 1);
            """;
        youthCommand.ExecuteNonQuery();

        using var workersCommand = connexion.CreateCommand();
        workersCommand.Transaction = transaction;
        workersCommand.CommandText = """
            INSERT INTO workers (worker_id, nom, prenom, company_id, in_ring, entertainment, story, popularite, fatigue, blessure, momentum, role_tv, type_worker)
            VALUES
            ('W-001', 'Dubois', 'Alex', 'COMP-001', 70, 62, 58, 55, 12, 'AUCUNE', 4, 'MAIN_EVENT', 'CATCHEUR'),
            ('W-002', 'Martin', 'Leo', 'COMP-001', 64, 70, 65, 52, 18, 'AUCUNE', 2, 'UPPER_MID', 'CATCHEUR'),
            ('W-003', 'Petit', 'Sarah', 'COMP-001', 68, 60, 72, 49, 20, 'AUCUNE', 1, 'MID', 'CATCHEUR'),
            ('W-004', 'Roche', 'Maya', 'COMP-001', 58, 74, 66, 46, 15, 'AUCUNE', 0, 'MID', 'CATCHEUR');
            """;
        workersCommand.ExecuteNonQuery();

        using var titleCommand = connexion.CreateCommand();
        titleCommand.Transaction = transaction;
        titleCommand.CommandText = """
            INSERT INTO titles (title_id, nom, prestige, detenteur_id, company_id)
            VALUES ('T-001', 'Championnat Principal', 60, 'W-001', 'COMP-001');
            """;
        titleCommand.ExecuteNonQuery();

        using var storylineCommand = connexion.CreateCommand();
        storylineCommand.Transaction = transaction;
        storylineCommand.CommandText = """
            INSERT INTO storylines (storyline_id, nom, heat)
            VALUES ('S-001', 'La course au titre', 52);
            """;
        storylineCommand.ExecuteNonQuery();

        using var storylineParticipants = connexion.CreateCommand();
        storylineParticipants.Transaction = transaction;
        storylineParticipants.CommandText = """
            INSERT INTO storyline_participants (storyline_id, worker_id)
            VALUES ('S-001', 'W-001'), ('S-001', 'W-002');
            """;
        storylineParticipants.ExecuteNonQuery();

        using var showCommand = connexion.CreateCommand();
        showCommand.Transaction = transaction;
        showCommand.CommandText = """
            INSERT INTO shows (show_id, nom, semaine, region, duree, compagnie_id, tv_deal_id)
            VALUES ('SHOW-001', 'Weekly Clash', 1, 'FR', 120, 'COMP-001', 'TV-001');
            """;
        showCommand.ExecuteNonQuery();

        using var segmentCommand = connexion.CreateCommand();
        segmentCommand.Transaction = transaction;
        segmentCommand.CommandText = """
            INSERT INTO segments (segment_id, show_id, ordre, type, duree, participants_json, storyline_id, title_id, main_event, intensite, vainqueur_id, perdant_id)
            VALUES
            ('SEG-001', 'SHOW-001', 1, 'promo', 8, '["W-001","W-002"]', 'S-001', NULL, 0, 40, NULL, NULL),
            ('SEG-002', 'SHOW-001', 2, 'match', 12, '["W-003","W-004"]', NULL, NULL, 0, 60, 'W-003', 'W-004'),
            ('SEG-003', 'SHOW-001', 3, 'match', 18, '["W-001","W-002"]', 'S-001', 'T-001', 1, 75, 'W-001', 'W-002');
            """;
        segmentCommand.ExecuteNonQuery();

        using var chimieCommand = connexion.CreateCommand();
        chimieCommand.Transaction = transaction;
        chimieCommand.CommandText = """
            INSERT INTO chimies (worker_a, worker_b, valeur)
            VALUES ('W-001', 'W-002', 6);
            """;
        chimieCommand.ExecuteNonQuery();

        using var contractCommand = connexion.CreateCommand();
        contractCommand.Transaction = transaction;
        contractCommand.CommandText = """
            INSERT INTO contracts (worker_id, company_id, fin_semaine)
            VALUES
            ('W-001', 'COMP-001', 30),
            ('W-002', 'COMP-001', 12),
            ('W-003', 'COMP-001', 6),
            ('W-004', 'COMP-001', 20);
            """;
        contractCommand.ExecuteNonQuery();

        using var popularityCommand = connexion.CreateCommand();
        popularityCommand.Transaction = transaction;
        popularityCommand.CommandText = """
            INSERT INTO popularity_regionale (entity_type, entity_id, region, valeur)
            VALUES
            ('company', 'COMP-001', 'FR', 50),
            ('worker', 'W-001', 'FR', 55),
            ('worker', 'W-002', 'FR', 52),
            ('worker', 'W-003', 'FR', 49),
            ('worker', 'W-004', 'FR', 46);
            """;
        popularityCommand.ExecuteNonQuery();

        using var settingsCommand = connexion.CreateCommand();
        settingsCommand.Transaction = transaction;
        settingsCommand.CommandText = """
            INSERT INTO game_settings (id, youth_generation_mode, world_generation_mode, semaine_pivot_annuelle)
            VALUES (1, 'Realiste', 'Desactivee', 1);
            """;
        settingsCommand.ExecuteNonQuery();

        transaction.Commit();
    }
}
