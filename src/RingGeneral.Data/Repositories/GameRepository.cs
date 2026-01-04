using System.Text.Json;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class GameRepository
{
    private readonly SqliteConnectionFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

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
                detenteur_id TEXT
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
            CREATE TABLE IF NOT EXISTS popularity_regionale (
                entity_type TEXT NOT NULL,
                entity_id TEXT NOT NULL,
                region TEXT NOT NULL,
                valeur INTEGER NOT NULL,
                UNIQUE(entity_type, entity_id, region)
            );
            """;
        commande.ExecuteNonQuery();

        using var countCommand = connexion.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(1) FROM companies";
        var count = Convert.ToInt32(countCommand.ExecuteScalar());
        if (count == 0)
        {
            SeedDatabase(connexion);
        }
    }

    public ShowContext ChargerShowContext(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
        var show = ChargerShow(connexion, showId);
        var compagnie = ChargerCompagnie(connexion, show.CompagnieId);
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
            INSERT INTO show_history (show_id, semaine, note, audience, resume)
            VALUES ($showId, (SELECT semaine FROM shows WHERE show_id = $showId), $note, $audience, $resume);
            """;
        showCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
        showCommand.Parameters.AddWithValue("$note", rapport.NoteGlobale);
        showCommand.Parameters.AddWithValue("$audience", rapport.Audience);
        showCommand.Parameters.AddWithValue("$resume", string.Join(" | ", rapport.PointsCles));
        showCommand.ExecuteNonQuery();

        foreach (var segment in rapport.Segments)
        {
            using var segmentCommand = connexion.CreateCommand();
            segmentCommand.Transaction = transaction;
            segmentCommand.CommandText = """
                INSERT INTO segment_history (segment_id, show_id, semaine, note, resume, details_json)
                VALUES ($segmentId, $showId, (SELECT semaine FROM shows WHERE show_id = $showId), $note, $resume, $details);
                """;
            segmentCommand.Parameters.AddWithValue("$segmentId", segment.SegmentId);
            segmentCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
            segmentCommand.Parameters.AddWithValue("$note", segment.Note);
            segmentCommand.Parameters.AddWithValue("$resume", $"Segment {segment.TypeSegment} - Note {segment.Note}");
            segmentCommand.Parameters.AddWithValue("$details", JsonSerializer.Serialize(segment, _jsonOptions));
            segmentCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public void AppliquerDelta(string showId, GameStateDelta delta)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();
        var region = ChargerRegionShow(connexion, showId);

        foreach (var (workerId, fatigueDelta) in delta.FatigueDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE workers
                SET fatigue = MAX(0, MIN(100, fatigue + $delta))
                WHERE worker_id = $workerId;
                """;
            command.Parameters.AddWithValue("$delta", fatigueDelta);
            command.Parameters.AddWithValue("$workerId", workerId);
            command.ExecuteNonQuery();
        }

        foreach (var (workerId, blessure) in delta.Blessures)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE workers SET blessure = $blessure WHERE worker_id = $workerId;";
            command.Parameters.AddWithValue("$blessure", blessure);
            command.Parameters.AddWithValue("$workerId", workerId);
            command.ExecuteNonQuery();
        }

        foreach (var (workerId, momentum) in delta.MomentumDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE workers SET momentum = momentum + $delta WHERE worker_id = $workerId;";
            command.Parameters.AddWithValue("$delta", momentum);
            command.Parameters.AddWithValue("$workerId", workerId);
            command.ExecuteNonQuery();
        }

        foreach (var (workerId, popularite) in delta.PopulariteWorkersDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE workers SET popularite = MAX(0, MIN(100, popularite + $delta)) WHERE worker_id = $workerId;";
            command.Parameters.AddWithValue("$delta", popularite);
            command.Parameters.AddWithValue("$workerId", workerId);
            command.ExecuteNonQuery();

            using var popCommand = connexion.CreateCommand();
            popCommand.Transaction = transaction;
            popCommand.CommandText = """
                INSERT INTO popularity_regionale (entity_type, entity_id, region, valeur)
                VALUES ('worker', $workerId, $region, 50)
                ON CONFLICT(entity_type, entity_id, region)
                DO UPDATE SET valeur = MAX(0, MIN(100, valeur + $delta));
                """;
            popCommand.Parameters.AddWithValue("$workerId", workerId);
            popCommand.Parameters.AddWithValue("$region", region);
            popCommand.Parameters.AddWithValue("$delta", popularite);
            popCommand.ExecuteNonQuery();
        }

        foreach (var (companyId, deltaPop) in delta.PopulariteCompagnieDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE companies SET prestige = MAX(0, MIN(100, prestige + $delta)) WHERE company_id = $companyId;";
            command.Parameters.AddWithValue("$delta", deltaPop);
            command.Parameters.AddWithValue("$companyId", companyId);
            command.ExecuteNonQuery();

            using var popCommand = connexion.CreateCommand();
            popCommand.Transaction = transaction;
            popCommand.CommandText = """
                INSERT INTO popularity_regionale (entity_type, entity_id, region, valeur)
                VALUES ('company', $companyId, $region, 50)
                ON CONFLICT(entity_type, entity_id, region)
                DO UPDATE SET valeur = MAX(0, MIN(100, valeur + $delta));
                """;
            popCommand.Parameters.AddWithValue("$companyId", companyId);
            popCommand.Parameters.AddWithValue("$region", region);
            popCommand.Parameters.AddWithValue("$delta", deltaPop);
            popCommand.ExecuteNonQuery();
        }

        foreach (var (storylineId, deltaHeat) in delta.StorylineHeatDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE storylines SET heat = MAX(0, MIN(100, heat + $delta)) WHERE storyline_id = $storylineId;";
            command.Parameters.AddWithValue("$delta", deltaHeat);
            command.Parameters.AddWithValue("$storylineId", storylineId);
            command.ExecuteNonQuery();
        }

        foreach (var (titreId, deltaPrestige) in delta.TitrePrestigeDelta)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE titles SET prestige = MAX(0, MIN(100, prestige + $delta)) WHERE title_id = $titleId;";
            command.Parameters.AddWithValue("$delta", deltaPrestige);
            command.Parameters.AddWithValue("$titleId", titreId);
            command.ExecuteNonQuery();
        }

        foreach (var transactionFin in delta.Finances)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO finances (show_id, type, montant, libelle, semaine)
                VALUES ($showId, $type, $montant, $libelle, (SELECT semaine FROM shows WHERE show_id = $showId));
                """;
            command.Parameters.AddWithValue("$showId", showId);
            command.Parameters.AddWithValue("$type", transactionFin.Type);
            command.Parameters.AddWithValue("$montant", transactionFin.Montant);
            command.Parameters.AddWithValue("$libelle", transactionFin.Libelle);
            command.ExecuteNonQuery();

            using var treasuryCommand = connexion.CreateCommand();
            treasuryCommand.Transaction = transaction;
            treasuryCommand.CommandText = "UPDATE companies SET tresorerie = tresorerie + $montant WHERE company_id = (SELECT compagnie_id FROM shows WHERE show_id = $showId);";
            treasuryCommand.Parameters.AddWithValue("$montant", transactionFin.Montant);
            treasuryCommand.Parameters.AddWithValue("$showId", showId);
            treasuryCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private static string ChargerRegionShow(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT region FROM shows WHERE show_id = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        return Convert.ToString(command.ExecuteScalar()) ?? "FR";
    }

    public IReadOnlyList<InboxItem> ChargerInbox()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT type, titre, contenu, semaine FROM inbox_items ORDER BY semaine DESC, id DESC;";
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

    public void AjouterInboxItem(InboxItem item)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "INSERT INTO inbox_items (type, titre, contenu, semaine) VALUES ($type, $titre, $contenu, $semaine);";
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
        command.CommandText = "UPDATE shows SET semaine = semaine + 1 WHERE show_id = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        command.ExecuteNonQuery();

        using var weekCommand = connexion.CreateCommand();
        weekCommand.CommandText = "SELECT semaine FROM shows WHERE show_id = $showId;";
        weekCommand.Parameters.AddWithValue("$showId", showId);
        return Convert.ToInt32(weekCommand.ExecuteScalar());
    }

    public IReadOnlyList<(string WorkerId, int FinSemaine)> ChargerContracts()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_id, fin_semaine FROM contracts;";
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
        command.CommandText = "SELECT worker_id, prenom || ' ' || nom FROM workers;";
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
        command.CommandText = "UPDATE workers SET fatigue = MAX(0, fatigue - 12);";
        command.ExecuteNonQuery();
    }

    private static ShowDefinition ChargerShow(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT show_id, nom, semaine, region, duree, compagnie_id, tv_deal_id
            FROM shows
            WHERE show_id = $showId;
            """;
        command.Parameters.AddWithValue("$showId", showId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidOperationException($"Show introuvable ({showId}).");
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

    private static CompanyState ChargerCompagnie(SqliteConnection connexion, string companyId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT company_id, nom, region, prestige, tresorerie, audience_moyenne, reach
            FROM companies
            WHERE company_id = $companyId;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidOperationException($"Compagnie introuvable ({companyId}).");
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
            SELECT segment_id, type, duree, participants_json, storyline_id, title_id, main_event, intensite, vainqueur_id, perdant_id
            FROM segments
            WHERE show_id = $showId
            ORDER BY ordre ASC;
            """;
        command.Parameters.AddWithValue("$showId", showId);
        using var reader = command.ExecuteReader();
        var segments = new List<SegmentDefinition>();
        while (reader.Read())
        {
            var participants = JsonSerializer.Deserialize<List<string>>(reader.GetString(3), _jsonOptions) ?? new List<string>();
            segments.Add(new SegmentDefinition(
                reader.GetString(0),
                reader.GetString(1),
                participants,
                reader.GetInt32(2),
                reader.GetInt32(6) == 1,
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.GetInt32(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetString(9)));
        }

        return segments;
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
            SELECT worker_id, prenom || ' ' || nom, in_ring, entertainment, story, popularite, fatigue, blessure, momentum, role_tv
            FROM workers
            WHERE worker_id IN ({string.Join(", ", placeholders)});
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
        command.CommandText = "SELECT title_id, nom, prestige, detenteur_id FROM titles;";
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
        command.CommandText = "SELECT storyline_id, nom, heat FROM storylines;";
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
        command.CommandText = "SELECT worker_id FROM storyline_participants WHERE storyline_id = $storylineId;";
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
        command.CommandText = "SELECT worker_a, worker_b, valeur FROM chimies;";
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

        using var workersCommand = connexion.CreateCommand();
        workersCommand.Transaction = transaction;
        workersCommand.CommandText = """
            INSERT INTO workers (worker_id, nom, prenom, in_ring, entertainment, story, popularite, fatigue, blessure, momentum, role_tv)
            VALUES
            ('W-001', 'Dubois', 'Alex', 70, 62, 58, 55, 12, 'AUCUNE', 4, 'MAIN_EVENT'),
            ('W-002', 'Martin', 'Leo', 64, 70, 65, 52, 18, 'AUCUNE', 2, 'UPPER_MID'),
            ('W-003', 'Petit', 'Sarah', 68, 60, 72, 49, 20, 'AUCUNE', 1, 'MID'),
            ('W-004', 'Roche', 'Maya', 58, 74, 66, 46, 15, 'AUCUNE', 0, 'MID');
            """;
        workersCommand.ExecuteNonQuery();

        using var titleCommand = connexion.CreateCommand();
        titleCommand.Transaction = transaction;
        titleCommand.CommandText = """
            INSERT INTO titles (title_id, nom, prestige, detenteur_id)
            VALUES ('T-001', 'Championnat Principal', 60, 'W-001');
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

        transaction.Commit();
    }
}
