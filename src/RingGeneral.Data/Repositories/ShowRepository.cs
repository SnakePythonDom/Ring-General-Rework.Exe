using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using MatchType = RingGeneral.Core.Models.MatchType;

namespace RingGeneral.Data.Repositories;

public sealed class ShowRepository : RepositoryBase
{
    public ShowRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public IReadOnlyList<ShowDefinition> ChargerShowsAVenir(string compagnieId, int semaineActuelle)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT show_id, nom, semaine, region, duree, compagnie_id, tv_deal_id, lieu, diffusion
            FROM shows
            WHERE compagnie_id = $compagnieId
              AND semaine >= $semaine
            ORDER BY semaine ASC;
            """;
        command.Parameters.AddWithValue("$compagnieId", compagnieId);
        command.Parameters.AddWithValue("$semaine", semaineActuelle);
        using var reader = command.ExecuteReader();
        var shows = new List<ShowDefinition>();
        while (reader.Read())
        {
            var lieu = reader.IsDBNull(7) ? reader.GetString(3) : reader.GetString(7);
            if (string.IsNullOrWhiteSpace(lieu))
            {
                lieu = reader.GetString(3);
            }

            var diffusion = reader.IsDBNull(8) ? "Non défini" : reader.GetString(8);
            if (string.IsNullOrWhiteSpace(diffusion))
            {
                diffusion = "Non défini";
            }

            shows.Add(new ShowDefinition(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                lieu,
                diffusion));
        }

        return shows;
    }

    public void CreerShow(ShowDefinition show)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO shows (show_id, nom, semaine, region, duree, compagnie_id, tv_deal_id, lieu, diffusion)
            VALUES ($showId, $nom, $semaine, $region, $duree, $compagnieId, $tvDealId, $lieu, $diffusion);
            """;
        command.Parameters.AddWithValue("$showId", show.ShowId);
        command.Parameters.AddWithValue("$nom", show.Nom);
        command.Parameters.AddWithValue("$semaine", show.Semaine);
        command.Parameters.AddWithValue("$region", show.Region);
        command.Parameters.AddWithValue("$duree", show.DureeMinutes);
        command.Parameters.AddWithValue("$compagnieId", show.CompagnieId);
        command.Parameters.AddWithValue("$tvDealId", (object?)show.DealTvId ?? DBNull.Value);
        command.Parameters.AddWithValue("$lieu", show.Lieu);
        command.Parameters.AddWithValue("$diffusion", show.Diffusion);
        command.ExecuteNonQuery();
    }

    public ShowDefinition? ChargerShowDefinition(string showId)
    {
        using var connexion = OpenConnection();
        return SharedQueries.ChargerShow(connexion, showId);
    }

    public void AjouterSegment(string showId, SegmentDefinition segment, int ordre)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        using var command = connexion.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO ShowSegments (ShowSegmentId, ShowId, OrderIndex, SegmentType, DurationMinutes, StorylineId, TitleId, IsMainEvent, Intensity, WinnerWorkerId, LoserWorkerId)
            VALUES ($segmentId, $showId, $ordre, $type, $duree, $storylineId, $titleId, $mainEvent, $intensite, $vainqueurId, $perdantId);
            """;
        command.Parameters.AddWithValue("$segmentId", segment.SegmentId);
        command.Parameters.AddWithValue("$showId", showId);
        command.Parameters.AddWithValue("$ordre", ordre);
        command.Parameters.AddWithValue("$type", segment.TypeSegment);
        command.Parameters.AddWithValue("$duree", segment.DureeMinutes);
        command.Parameters.AddWithValue("$storylineId", (object?)segment.StorylineId ?? DBNull.Value);
        command.Parameters.AddWithValue("$titleId", (object?)segment.TitreId ?? DBNull.Value);
        command.Parameters.AddWithValue("$mainEvent", segment.EstMainEvent ? 1 : 0);
        command.Parameters.AddWithValue("$intensite", segment.Intensite);
        command.Parameters.AddWithValue("$vainqueurId", (object?)segment.VainqueurId ?? DBNull.Value);
        command.Parameters.AddWithValue("$perdantId", (object?)segment.PerdantId ?? DBNull.Value);
        command.ExecuteNonQuery();

        SauvegarderParticipants(connexion, transaction, segment.SegmentId, segment.Participants);
        SauvegarderSettings(connexion, transaction, segment.SegmentId, segment.Settings);

        transaction.Commit();
    }

    public void MettreAJourSegment(SegmentDefinition segment)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        using var command = connexion.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE ShowSegments
            SET SegmentType = $type,
                DurationMinutes = $duree,
                StorylineId = $storylineId,
                TitleId = $titleId,
                IsMainEvent = $mainEvent,
                Intensity = $intensite,
                WinnerWorkerId = $vainqueurId,
                LoserWorkerId = $perdantId
            WHERE ShowSegmentId = $segmentId;
            """;
        command.Parameters.AddWithValue("$segmentId", segment.SegmentId);
        command.Parameters.AddWithValue("$type", segment.TypeSegment);
        command.Parameters.AddWithValue("$duree", segment.DureeMinutes);
        command.Parameters.AddWithValue("$storylineId", (object?)segment.StorylineId ?? DBNull.Value);
        command.Parameters.AddWithValue("$titleId", (object?)segment.TitreId ?? DBNull.Value);
        command.Parameters.AddWithValue("$mainEvent", segment.EstMainEvent ? 1 : 0);
        command.Parameters.AddWithValue("$intensite", segment.Intensite);
        command.Parameters.AddWithValue("$vainqueurId", (object?)segment.VainqueurId ?? DBNull.Value);
        command.Parameters.AddWithValue("$perdantId", (object?)segment.PerdantId ?? DBNull.Value);
        command.ExecuteNonQuery();

        SupprimerParticipants(connexion, transaction, segment.SegmentId);
        SauvegarderParticipants(connexion, transaction, segment.SegmentId, segment.Participants);
        SupprimerSettings(connexion, transaction, segment.SegmentId);
        SauvegarderSettings(connexion, transaction, segment.SegmentId, segment.Settings);

        transaction.Commit();
    }

    public void SupprimerSegment(string segmentId)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();

        SupprimerParticipants(connexion, transaction, segmentId);
        SupprimerSettings(connexion, transaction, segmentId);

        using var command = connexion.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "DELETE FROM ShowSegments WHERE ShowSegmentId = $segmentId;";
        command.Parameters.AddWithValue("$segmentId", segmentId);
        command.ExecuteNonQuery();

        transaction.Commit();
    }

    public void MettreAJourOrdreSegments(string showId, IReadOnlyList<string> segmentIds)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        for (var i = 0; i < segmentIds.Count; i++)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE ShowSegments
                SET OrderIndex = $ordre
                WHERE ShowSegmentId = $segmentId AND ShowId = $showId;
                """;
            command.Parameters.AddWithValue("$ordre", i + 1);
            command.Parameters.AddWithValue("$segmentId", segmentIds[i]);
            command.Parameters.AddWithValue("$showId", showId);
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public IReadOnlyList<SegmentTemplate> ChargerSegmentTemplates()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT template_id, nom, type_segment, duree, main_event, intensite, match_type_id
            FROM segment_templates
            ORDER BY nom ASC;
            """;
        using var reader = command.ExecuteReader();
        var templates = new List<SegmentTemplate>();
        while (reader.Read())
        {
            templates.Add(new SegmentTemplate(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetInt32(4) == 1,
                reader.GetInt32(5),
                reader.IsDBNull(6) ? null : reader.GetString(6)));
        }

        return templates;
    }

    public IReadOnlyList<MatchType> ChargerMatchTypes()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT match_type_id, nom, description, actif, ordre
            FROM match_types
            ORDER BY ordre ASC, nom ASC;
            """;
        using var reader = command.ExecuteReader();
        var types = new List<MatchType>();
        while (reader.Read())
        {
            types.Add(new MatchType(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.GetInt32(3) == 1,
                reader.GetInt32(4)));
        }

        return types;
    }

    public void MettreAJourMatchType(MatchType matchType)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE match_types
            SET nom = $nom,
                description = $description,
                actif = $actif,
                ordre = $ordre
            WHERE match_type_id = $id;
            """;
        command.Parameters.AddWithValue("$id", matchType.MatchTypeId);
        command.Parameters.AddWithValue("$nom", matchType.Nom);
        command.Parameters.AddWithValue("$description", (object?)matchType.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$actif", matchType.EstActif ? 1 : 0);
        command.Parameters.AddWithValue("$ordre", matchType.Ordre);
        command.ExecuteNonQuery();
    }

    public void EnregistrerRapport(ShowReport rapport)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();

        var semaine = ChargerSemaineShow(connexion, rapport.ShowId);
        var resume = string.Join(" | ", rapport.PointsCles);

        if (TableExiste(connexion, "show_history"))
        {
            using var showCommand = connexion.CreateCommand();
            showCommand.Transaction = transaction;
            showCommand.CommandText = """
                INSERT INTO show_history (show_id, semaine, note, audience, resume)
                VALUES ($showId, $semaine, $note, $audience, $resume);
                """;
            showCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
            showCommand.Parameters.AddWithValue("$semaine", semaine);
            showCommand.Parameters.AddWithValue("$note", rapport.NoteGlobale);
            showCommand.Parameters.AddWithValue("$audience", rapport.Audience);
            showCommand.Parameters.AddWithValue("$resume", resume);
            showCommand.ExecuteNonQuery();
        }

        if (TableExiste(connexion, "ShowHistory"))
        {
            using var showCommand = connexion.CreateCommand();
            showCommand.Transaction = transaction;
            showCommand.CommandText = """
                INSERT INTO ShowHistory (ShowId, Week, Note, Audience, Summary)
                VALUES ($showId, $semaine, $note, $audience, $resume);
                """;
            showCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
            showCommand.Parameters.AddWithValue("$semaine", semaine);
            showCommand.Parameters.AddWithValue("$note", rapport.NoteGlobale);
            showCommand.Parameters.AddWithValue("$audience", rapport.Audience);
            showCommand.Parameters.AddWithValue("$resume", resume);
            showCommand.ExecuteNonQuery();
        }

        if (TableExiste(connexion, "audience_history"))
        {
            using var audienceCommand = connexion.CreateCommand();
            audienceCommand.Transaction = transaction;
            audienceCommand.CommandText = """
                INSERT INTO audience_history (show_id, semaine, audience, reach, show_score, stars, saturation)
                VALUES ($showId, $semaine, $audience, $reach, $score, $stars, $saturation);
                """;
            audienceCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
            audienceCommand.Parameters.AddWithValue("$semaine", semaine);
            audienceCommand.Parameters.AddWithValue("$audience", rapport.AudienceDetails.Audience);
            audienceCommand.Parameters.AddWithValue("$reach", rapport.AudienceDetails.Reach);
            audienceCommand.Parameters.AddWithValue("$score", rapport.AudienceDetails.ShowScore);
            audienceCommand.Parameters.AddWithValue("$stars", rapport.AudienceDetails.Stars);
            audienceCommand.Parameters.AddWithValue("$saturation", rapport.AudienceDetails.Saturation);
            audienceCommand.ExecuteNonQuery();
        }

        if (TableExiste(connexion, "AudienceHistory"))
        {
            using var audienceCommand = connexion.CreateCommand();
            audienceCommand.Transaction = transaction;
            audienceCommand.CommandText = """
                INSERT INTO AudienceHistory (ShowId, Week, Audience, Reach, ShowScore, Stars, Saturation)
                VALUES ($showId, $semaine, $audience, $reach, $score, $stars, $saturation);
                """;
            audienceCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
            audienceCommand.Parameters.AddWithValue("$semaine", semaine);
            audienceCommand.Parameters.AddWithValue("$audience", rapport.AudienceDetails.Audience);
            audienceCommand.Parameters.AddWithValue("$reach", rapport.AudienceDetails.Reach);
            audienceCommand.Parameters.AddWithValue("$score", rapport.AudienceDetails.ShowScore);
            audienceCommand.Parameters.AddWithValue("$stars", rapport.AudienceDetails.Stars);
            audienceCommand.Parameters.AddWithValue("$saturation", rapport.AudienceDetails.Saturation);
            audienceCommand.ExecuteNonQuery();
        }

        using var showResultCommand = connexion.CreateCommand();
        showResultCommand.Transaction = transaction;
        showResultCommand.CommandText = """
            INSERT INTO ShowResults (ShowId, Week, Note, Audience, Billetterie, Merch, Tv, Summary)
            VALUES ($showId, (SELECT Week FROM Shows WHERE ShowId = $showId), $note, $audience, $billetterie, $merch, $tv, $resume);
            """;
        showResultCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
        showResultCommand.Parameters.AddWithValue("$note", rapport.NoteGlobale);
        showResultCommand.Parameters.AddWithValue("$audience", rapport.Audience);
        showResultCommand.Parameters.AddWithValue("$billetterie", rapport.Billetterie);
        showResultCommand.Parameters.AddWithValue("$merch", rapport.Merch);
        showResultCommand.Parameters.AddWithValue("$tv", rapport.Tv);
        showResultCommand.Parameters.AddWithValue("$resume", string.Join(" | ", rapport.PointsCles));
        showResultCommand.ExecuteNonQuery();

        foreach (var segment in rapport.Segments)
        {
            var details = string.Join(", ",
                segment.Facteurs.Select(facteur => $"{facteur.Libelle} {facteur.Impact:+#;-#;0}"));

            if (TableExiste(connexion, "segment_history"))
            {
                using var segmentCommand = connexion.CreateCommand();
                segmentCommand.Transaction = transaction;
                segmentCommand.CommandText = """
                    INSERT INTO segment_history (segment_id, show_id, semaine, note, resume, details_json)
                    VALUES ($segmentId, $showId, $semaine, $note, $resume, $details);
                    """;
                segmentCommand.Parameters.AddWithValue("$segmentId", segment.SegmentId);
                segmentCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
                segmentCommand.Parameters.AddWithValue("$semaine", semaine);
                segmentCommand.Parameters.AddWithValue("$note", segment.Note);
                segmentCommand.Parameters.AddWithValue("$resume", $"Segment {segment.TypeSegment} - Note {segment.Note}");
                segmentCommand.Parameters.AddWithValue("$details", details);
                segmentCommand.ExecuteNonQuery();
            }

            if (TableExiste(connexion, "SegmentResults"))
            {
                using var segmentCommand = connexion.CreateCommand();
                segmentCommand.Transaction = transaction;
                segmentCommand.CommandText = """
                    INSERT INTO SegmentResults (ShowSegmentId, ShowId, Week, Note, Summary, Details)
                    VALUES ($segmentId, $showId, $semaine, $note, $resume, $details);
                    """;
                segmentCommand.Parameters.AddWithValue("$segmentId", segment.SegmentId);
                segmentCommand.Parameters.AddWithValue("$showId", rapport.ShowId);
                segmentCommand.Parameters.AddWithValue("$semaine", semaine);
                segmentCommand.Parameters.AddWithValue("$note", segment.Note);
                segmentCommand.Parameters.AddWithValue("$resume", $"Segment {segment.TypeSegment} - Note {segment.Note}");
                segmentCommand.Parameters.AddWithValue("$details", details);
                segmentCommand.ExecuteNonQuery();
            }
        }

        transaction.Commit();
    }

    public IReadOnlyList<AudienceHistoryEntry> ChargerAudienceHistorique(string showId)
    {
        using var connexion = OpenConnection();
        if (TableExiste(connexion, "audience_history"))
        {
            return ChargerAudienceHistoriqueLower(connexion, showId);
        }

        return ChargerAudienceHistoriqueUpper(connexion, showId);
    }

    public IReadOnlyList<ShowHistoryEntry> ChargerHistoriqueShow(string showId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ShowId, Week, Note, Audience, Summary, CreatedAt
            FROM ShowHistory
            WHERE ShowId = $showId
            ORDER BY Week DESC, CreatedAt DESC;
            """;
        command.Parameters.AddWithValue("$showId", showId);
        using var reader = command.ExecuteReader();
        var historique = new List<ShowHistoryEntry>();
        while (reader.Read())
        {
            historique.Add(new ShowHistoryEntry(
                reader.GetString(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5)));
        }

        return historique;
    }

    public int IncrementerSemaine(string showId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE Shows SET Week = Week + 1 WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        command.ExecuteNonQuery();

        using var weekCommand = connexion.CreateCommand();
        weekCommand.CommandText = "SELECT Week FROM Shows WHERE ShowId = $showId;";
        weekCommand.Parameters.AddWithValue("$showId", showId);
        return Convert.ToInt32(weekCommand.ExecuteScalar());
    }

    public int ChargerSemaineShow(string showId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Week FROM Shows WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // === Helpers privés (Catégorie B - Show domain) ===

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
        var settingsMap = ChargerSettings(connexion, segments.Select(segment => segment.SegmentId).ToList());
        return segments
            .Select(segment => segment with
            {
                Participants = participantsMap.GetValueOrDefault(segment.SegmentId, new List<string>()),
                Settings = settingsMap.GetValueOrDefault(segment.SegmentId, new Dictionary<string, string>())
            })
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

    private static Dictionary<string, Dictionary<string, string>> ChargerSettings(
        SqliteConnection connexion,
        IReadOnlyList<string> segmentIds)
    {
        var settings = segmentIds.ToDictionary(id => id, _ => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        if (segmentIds.Count == 0)
        {
            return settings;
        }

        using var command = connexion.CreateCommand();
        var placeholders = segmentIds.Select((id, index) => $"$id{index}").ToList();
        command.CommandText = $"SELECT ShowSegmentId, SettingKey, SettingValue FROM SegmentSettings WHERE ShowSegmentId IN ({string.Join(", ", placeholders)});";
        for (var i = 0; i < segmentIds.Count; i++)
        {
            command.Parameters.AddWithValue(placeholders[i], segmentIds[i]);
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var segmentId = reader.GetString(0);
            var key = reader.GetString(1);
            var value = reader.GetString(2);
            if (settings.TryGetValue(segmentId, out var map))
            {
                map[key] = value;
            }
        }

        return settings;
    }

    private static void SupprimerParticipants(SqliteConnection connexion, SqliteTransaction transaction, string segmentId)
    {
        using var command = connexion.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "DELETE FROM SegmentParticipants WHERE ShowSegmentId = $segmentId;";
        command.Parameters.AddWithValue("$segmentId", segmentId);
        command.ExecuteNonQuery();
    }

    private static void SauvegarderParticipants(
        SqliteConnection connexion,
        SqliteTransaction transaction,
        string segmentId,
        IReadOnlyList<string> participants)
    {
        foreach (var participantId in participants.Distinct())
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO SegmentParticipants (ShowSegmentId, WorkerId)
                VALUES ($segmentId, $workerId);
                """;
            command.Parameters.AddWithValue("$segmentId", segmentId);
            command.Parameters.AddWithValue("$workerId", participantId);
            command.ExecuteNonQuery();
        }
    }

    private static void SupprimerSettings(SqliteConnection connexion, SqliteTransaction transaction, string segmentId)
    {
        using var command = connexion.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "DELETE FROM SegmentSettings WHERE ShowSegmentId = $segmentId;";
        command.Parameters.AddWithValue("$segmentId", segmentId);
        command.ExecuteNonQuery();
    }

    private static void SauvegarderSettings(
        SqliteConnection connexion,
        SqliteTransaction transaction,
        string segmentId,
        IReadOnlyDictionary<string, string>? settings)
    {
        if (settings is null)
        {
            return;
        }

        foreach (var (key, value) in settings)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO SegmentSettings (ShowSegmentId, SettingKey, SettingValue)
                VALUES ($segmentId, $key, $value);
                """;
            command.Parameters.AddWithValue("$segmentId", segmentId);
            command.Parameters.AddWithValue("$key", key);
            command.Parameters.AddWithValue("$value", value);
            command.ExecuteNonQuery();
        }
    }

    private static IReadOnlyList<AudienceHistoryEntry> ChargerAudienceHistoriqueLower(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT show_id, semaine, audience, reach, show_score, stars, saturation
            FROM audience_history
            WHERE show_id = $showId
            ORDER BY semaine DESC;
            """;
        command.Parameters.AddWithValue("$showId", showId);
        using var reader = command.ExecuteReader();
        var entries = new List<AudienceHistoryEntry>();
        while (reader.Read())
        {
            entries.Add(new AudienceHistoryEntry(
                reader.GetString(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6)));
        }

        return entries;
    }

    private static IReadOnlyList<AudienceHistoryEntry> ChargerAudienceHistoriqueUpper(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ShowId, Week, Audience, Reach, ShowScore, Stars, Saturation
            FROM AudienceHistory
            WHERE ShowId = $showId
            ORDER BY Week DESC;
            """;
        command.Parameters.AddWithValue("$showId", showId);
        using var reader = command.ExecuteReader();
        var entries = new List<AudienceHistoryEntry>();
        while (reader.Read())
        {
            entries.Add(new AudienceHistoryEntry(
                reader.GetString(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6)));
        }

        return entries;
    }

    private static string ChargerRegionShow(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT RegionId FROM Shows WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        return Convert.ToString(command.ExecuteScalar()) ?? "";
    }

    private static int ChargerSemaineShow(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Week FROM Shows WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        var resultat = command.ExecuteScalar();
        return resultat is null or DBNull ? 0 : Convert.ToInt32(resultat);
    }
}
