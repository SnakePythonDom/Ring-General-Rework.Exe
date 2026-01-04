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
        var initializer = new DbInitializer();
        initializer.CreateDatabaseIfMissing(_factory.DatabasePath);
    }

    public ShowContext? ChargerShowContext(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
        var show = ChargerShow(connexion, showId);
        if (show is null)
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

    private static ShowDefinition? ChargerShow(SqliteConnection connexion, string showId)
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
}
