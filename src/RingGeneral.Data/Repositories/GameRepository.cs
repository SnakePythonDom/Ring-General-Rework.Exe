using System.Text.Json;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using RingGeneral.Data.Models;

namespace RingGeneral.Data.Repositories;

public sealed class GameRepository : IScoutingRepository
{
    private readonly SqliteConnectionFactory _factory;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
                role_tv TEXT NOT NULL,
                morale INTEGER NOT NULL DEFAULT 60
            );
            CREATE TABLE IF NOT EXISTS Injuries (
                InjuryId INTEGER PRIMARY KEY AUTOINCREMENT,
                WorkerId TEXT NOT NULL,
                Type TEXT NOT NULL,
                Severity INTEGER NOT NULL,
                StartDate INTEGER NOT NULL,
                EndDate INTEGER,
                IsActive INTEGER NOT NULL DEFAULT 1,
                Notes TEXT
            );
            CREATE TABLE IF NOT EXISTS MedicalNotes (
                MedicalNoteId INTEGER PRIMARY KEY AUTOINCREMENT,
                InjuryId INTEGER,
                WorkerId TEXT NOT NULL,
                Note TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            CREATE TABLE IF NOT EXISTS RecoveryPlans (
                RecoveryPlanId INTEGER PRIMARY KEY AUTOINCREMENT,
                InjuryId INTEGER NOT NULL,
                WorkerId TEXT NOT NULL,
                StartDate INTEGER NOT NULL,
                TargetDate INTEGER NOT NULL,
                RecommendedRestWeeks INTEGER NOT NULL,
                RiskLevel TEXT NOT NULL,
                Status TEXT NOT NULL DEFAULT 'EN_COURS',
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
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
                tv_deal_id TEXT,
                lieu TEXT NOT NULL DEFAULT '',
                diffusion TEXT NOT NULL DEFAULT ''
            );
            CREATE TABLE IF NOT EXISTS tv_deals (
                tv_deal_id TEXT PRIMARY KEY,
                company_id TEXT NOT NULL,
                network_name TEXT NOT NULL,
                reach_bonus INTEGER NOT NULL DEFAULT 0,
                audience_cap INTEGER NOT NULL DEFAULT 100,
                audience_min INTEGER NOT NULL DEFAULT 0,
                base_revenue REAL NOT NULL DEFAULT 0,
                revenue_per_point REAL NOT NULL DEFAULT 0,
                penalty REAL NOT NULL DEFAULT 0,
                constraints TEXT NOT NULL DEFAULT ''
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
            CREATE TABLE IF NOT EXISTS ShowSegments (
                ShowSegmentId TEXT PRIMARY KEY,
                ShowId TEXT NOT NULL,
                OrderIndex INTEGER NOT NULL,
                SegmentType TEXT NOT NULL,
                DurationMinutes INTEGER NOT NULL,
                StorylineId TEXT,
                TitleId TEXT,
                IsMainEvent INTEGER NOT NULL DEFAULT 0,
                Intensity INTEGER NOT NULL DEFAULT 50,
                WinnerWorkerId TEXT,
                LoserWorkerId TEXT
            );
            CREATE TABLE IF NOT EXISTS SegmentParticipants (
                ShowSegmentId TEXT NOT NULL,
                WorkerId TEXT NOT NULL,
                Role TEXT,
                PRIMARY KEY (ShowSegmentId, WorkerId)
            );
            CREATE TABLE IF NOT EXISTS SegmentSettings (
                ShowSegmentId TEXT NOT NULL,
                SettingKey TEXT NOT NULL,
                SettingValue TEXT NOT NULL,
                PRIMARY KEY (ShowSegmentId, SettingKey)
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
            CREATE TABLE IF NOT EXISTS FinanceTransactions (
                FinanceTransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
                CompanyId TEXT NOT NULL,
                ShowId TEXT,
                Date TEXT,
                Week INTEGER,
                Category TEXT NOT NULL,
                Amount REAL NOT NULL,
                Description TEXT
            );
            CREATE TABLE IF NOT EXISTS CompanyBalanceSnapshots (
                CompanyBalanceSnapshotId INTEGER PRIMARY KEY AUTOINCREMENT,
                CompanyId TEXT NOT NULL,
                Week INTEGER NOT NULL,
                Balance REAL NOT NULL,
                Revenues REAL NOT NULL,
                Expenses REAL NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            CREATE TABLE IF NOT EXISTS show_history (
                show_id TEXT NOT NULL,
                semaine INTEGER NOT NULL,
                note INTEGER NOT NULL,
                audience INTEGER NOT NULL,
                resume TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS audience_history (
                show_id TEXT NOT NULL,
                semaine INTEGER NOT NULL,
                audience INTEGER NOT NULL,
                reach INTEGER NOT NULL,
                show_score INTEGER NOT NULL,
                stars INTEGER NOT NULL,
                saturation INTEGER NOT NULL
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
            CREATE TABLE IF NOT EXISTS backstage_incidents (
                incident_id TEXT PRIMARY KEY,
                company_id TEXT NOT NULL,
                semaine INTEGER NOT NULL,
                type_id TEXT NOT NULL,
                titre TEXT NOT NULL,
                description TEXT NOT NULL,
                gravite INTEGER NOT NULL,
                workers_json TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS disciplinary_actions (
                action_id TEXT PRIMARY KEY,
                company_id TEXT NOT NULL,
                worker_id TEXT NOT NULL,
                semaine INTEGER NOT NULL,
                type_id TEXT NOT NULL,
                gravite INTEGER NOT NULL,
                morale_delta INTEGER NOT NULL,
                notes TEXT NOT NULL,
                incident_id TEXT
            );
            CREATE TABLE IF NOT EXISTS morale_history (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                worker_id TEXT NOT NULL,
                semaine INTEGER NOT NULL,
                morale_avant INTEGER NOT NULL,
                morale_apres INTEGER NOT NULL,
                delta INTEGER NOT NULL,
                raison TEXT NOT NULL,
                incident_id TEXT,
                action_id TEXT
            );
            CREATE TABLE IF NOT EXISTS contracts (
                contract_id TEXT,
                worker_id TEXT NOT NULL,
                company_id TEXT NOT NULL,
                type TEXT NOT NULL DEFAULT 'exclusif',
                debut_semaine INTEGER NOT NULL DEFAULT 1,
                fin_semaine INTEGER NOT NULL,
                salaire REAL NOT NULL DEFAULT 0,
                bonus_show REAL NOT NULL DEFAULT 0,
                buyout REAL NOT NULL DEFAULT 0,
                non_compete_weeks INTEGER NOT NULL DEFAULT 0,
                auto_renew INTEGER NOT NULL DEFAULT 0,
                exclusif INTEGER NOT NULL DEFAULT 1,
                statut TEXT NOT NULL DEFAULT 'actif',
                created_week INTEGER NOT NULL DEFAULT 1
            );
            CREATE TABLE IF NOT EXISTS contract_offers (
                offer_id TEXT PRIMARY KEY,
                negotiation_id TEXT NOT NULL,
                worker_id TEXT NOT NULL,
                company_id TEXT NOT NULL,
                type TEXT NOT NULL,
                debut_semaine INTEGER NOT NULL,
                fin_semaine INTEGER NOT NULL,
                salaire REAL NOT NULL,
                bonus_show REAL NOT NULL,
                buyout REAL NOT NULL,
                non_compete_weeks INTEGER NOT NULL,
                auto_renew INTEGER NOT NULL,
                exclusif INTEGER NOT NULL,
                statut TEXT NOT NULL,
                created_week INTEGER NOT NULL,
                expiration_week INTEGER NOT NULL,
                parent_offer_id TEXT,
                est_ia INTEGER NOT NULL DEFAULT 0
            );
            CREATE TABLE IF NOT EXISTS contract_clauses (
                clause_id INTEGER PRIMARY KEY AUTOINCREMENT,
                contract_id TEXT,
                offer_id TEXT,
                type TEXT NOT NULL,
                valeur TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS negotiation_state (
                negotiation_id TEXT PRIMARY KEY,
                worker_id TEXT NOT NULL,
                company_id TEXT NOT NULL,
                fin_semaine INTEGER NOT NULL,
                salaire REAL NOT NULL DEFAULT 0,
                pay_frequency TEXT NOT NULL DEFAULT 'Hebdomadaire'
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
                statut TEXT NOT NULL,
                semaine_inscription INTEGER,
                semaine_graduation INTEGER,
                PRIMARY KEY (worker_id, youth_id)
            );
            CREATE TABLE IF NOT EXISTS youth_programs (
                program_id TEXT PRIMARY KEY,
                youth_id TEXT NOT NULL,
                nom TEXT NOT NULL,
                duree_semaines INTEGER NOT NULL,
                focus TEXT
            );
            CREATE TABLE IF NOT EXISTS youth_staff_assignments (
                assignment_id INTEGER PRIMARY KEY AUTOINCREMENT,
                youth_id TEXT NOT NULL,
                worker_id TEXT NOT NULL,
                role TEXT NOT NULL,
                semaine_debut INTEGER
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
            CREATE TABLE IF NOT EXISTS ui_table_settings (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                recherche TEXT,
                filtre_type TEXT,
                filtre_statut TEXT,
                colonnes_visibles TEXT,
                colonnes_ordre TEXT,
                tri_colonnes TEXT
            );
            CREATE TABLE IF NOT EXISTS popularity_regionale (
                entity_type TEXT NOT NULL,
                entity_id TEXT NOT NULL,
                region TEXT NOT NULL,
                valeur INTEGER NOT NULL,
                UNIQUE(entity_type, entity_id, region)
            );
            CREATE TABLE IF NOT EXISTS match_types (
                match_type_id TEXT PRIMARY KEY,
                nom TEXT NOT NULL,
                description TEXT,
                actif INTEGER NOT NULL,
                ordre INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS segment_templates (
                template_id TEXT PRIMARY KEY,
                nom TEXT NOT NULL,
                type_segment TEXT NOT NULL,
                duree INTEGER NOT NULL,
                main_event INTEGER NOT NULL,
                intensite INTEGER NOT NULL,
                match_type_id TEXT
            );
            CREATE INDEX IF NOT EXISTS idx_workers_company ON workers(company_id);
            CREATE INDEX IF NOT EXISTS idx_workers_popularite ON workers(popularite);
            CREATE INDEX IF NOT EXISTS idx_contracts_enddate ON contracts(fin_semaine);
            CREATE INDEX IF NOT EXISTS idx_contracts_company ON contracts(company_id);
            CREATE INDEX IF NOT EXISTS idx_contracts_worker ON contracts(worker_id);
            CREATE INDEX IF NOT EXISTS idx_contracts_company_end ON contracts(company_id, fin_semaine);
            CREATE INDEX IF NOT EXISTS idx_contract_offers_company ON contract_offers(company_id);
            CREATE INDEX IF NOT EXISTS idx_contract_offers_worker ON contract_offers(worker_id);
            CREATE INDEX IF NOT EXISTS idx_contract_offers_expiration ON contract_offers(expiration_week);
            CREATE INDEX IF NOT EXISTS idx_titles_company ON titles(company_id);
            CREATE INDEX IF NOT EXISTS idx_youth_company ON youth_structures(company_id);
            CREATE INDEX IF NOT EXISTS idx_youth_region ON youth_structures(region);
            CREATE INDEX IF NOT EXISTS idx_youth_trainees_youth ON youth_trainees(youth_id);
            CREATE INDEX IF NOT EXISTS idx_youth_programs_youth ON youth_programs(youth_id);
            CREATE INDEX IF NOT EXISTS idx_youth_staff_youth ON youth_staff_assignments(youth_id);
            CREATE INDEX IF NOT EXISTS idx_worker_attributes_worker ON worker_attributes(worker_id);
            CREATE INDEX IF NOT EXISTS idx_generation_events_semaine ON worker_generation_events(semaine);
            CREATE INDEX IF NOT EXISTS idx_match_types_actif ON match_types(actif);
            CREATE INDEX IF NOT EXISTS idx_segment_templates_type ON segment_templates(type_segment);
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

        InitialiserBibliotheque(connexion);
    }

    public ShowContext? ChargerShowContext(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
        AssurerColonnesSupplementaires(connexion);

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
        var deal = ChargerTvDeal(connexion, show.DealTvId);

        return new ShowContext(show, compagnie, workers, titres, storylines, segments, chimies, deal);
    }

    private static void AjouterColonneSiAbsente(SqliteConnection connexion, string table, string colonne, string type)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (reader.GetString(1).Equals(colonne, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        using var alterCommand = connexion.CreateCommand();
        alterCommand.CommandText = $"ALTER TABLE {table} ADD COLUMN {colonne} {type};";
        alterCommand.ExecuteNonQuery();
    }

    private static void AssurerColonnesSupplementaires(SqliteConnection connexion)
    {
        AjouterColonneSiAbsente(connexion, "workers", "company_id", "TEXT");
        AjouterColonneSiAbsente(connexion, "workers", "type_worker", "TEXT");
        AjouterColonneSiAbsente(connexion, "titles", "company_id", "TEXT");
        AjouterColonneSiAbsente(connexion, "shows", "lieu", "TEXT");
        AjouterColonneSiAbsente(connexion, "shows", "diffusion", "TEXT");
    }

    private static void AjouterColonneSiAbsente(SqliteConnection connexion, string table, string colonne, string type)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), colonne, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        using var alter = connexion.CreateCommand();
        alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {colonne} {type};";
        alter.ExecuteNonQuery();
    }

    private static void AssurerColonnesSupplementaires(SqliteConnection connexion)
    {
        AjouterColonneSiAbsente(connexion, "workers", "company_id", "TEXT");
        AjouterColonneSiAbsente(connexion, "workers", "type_worker", "TEXT");
        AjouterColonneSiAbsente(connexion, "titles", "company_id", "TEXT");
        AjouterColonneSiAbsente(connexion, "shows", "lieu", "TEXT");
        AjouterColonneSiAbsente(connexion, "shows", "diffusion", "TEXT");
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

    public IReadOnlyList<ShowDefinition> ChargerShowsAVenir(string compagnieId, int semaineActuelle)
    {
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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

    public void AjouterSegment(string showId, SegmentDefinition segment, int ordre)
    {
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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

    public void MettreAJourOrdreSegments(string showId, IReadOnlyList<string> segmentIds)
    {
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();

        var semaine = ChargerSemaineShow(connexion, rapport.ShowId) ?? 0;
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

    public void AppliquerDelta(string showId, GameStateDelta delta)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();
        var regionId = ChargerRegionShow(connexion, showId);
        var semaineShow = ChargerSemaineShow(connexion, showId);

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

            if (!string.Equals(blessure, "AUCUNE", StringComparison.OrdinalIgnoreCase))
            {
                using var injuryCommand = connexion.CreateCommand();
                injuryCommand.Transaction = transaction;
                injuryCommand.CommandText = """
                    INSERT INTO Injuries (WorkerId, Type, Severity, StartDate, IsActive, Notes)
                    VALUES ($workerId, $type, $severity, $startDate, 1, $notes);
                    """;
                injuryCommand.Parameters.AddWithValue("$workerId", workerId);
                injuryCommand.Parameters.AddWithValue("$type", blessure);
                injuryCommand.Parameters.AddWithValue("$severity", DeterminerSeveriteBlessure(blessure));
                injuryCommand.Parameters.AddWithValue("$startDate", semaineShow);
                injuryCommand.Parameters.AddWithValue("$notes", $"Blessure lors du show {showId}");
                injuryCommand.ExecuteNonQuery();
            }
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

    private static int ChargerSemaineShow(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Week FROM Shows WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        var resultat = command.ExecuteScalar();
        return resultat is null or DBNull ? 0 : Convert.ToInt32(resultat);
    }

    private static int DeterminerSeveriteBlessure(string blessure)
    {
        return blessure.ToUpperInvariant() switch
        {
            "LEGERE" => 1,
            "MOYENNE" => 2,
            "GRAVE" => 3,
            _ => 1
        };
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

    public IReadOnlyList<BackstageWorker> ChargerBackstageRoster(string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT WorkerId, Name FROM Workers WHERE CompanyId = $companyId;";
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var roster = new List<BackstageWorker>();
        while (reader.Read())
        {
            roster.Add(new BackstageWorker(reader.GetString(0), reader.GetString(1)));
        }

        return roster;
    }

    public IReadOnlyDictionary<string, int> ChargerMorales(string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT WorkerId, Morale FROM Workers WHERE CompanyId = $companyId;";
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
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Morale FROM Workers WHERE WorkerId = $workerId;";
        command.Parameters.AddWithValue("$workerId", workerId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void EnregistrerBackstageIncident(BackstageIncident incident)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO BackstageIncidents (BackstageIncidentId, CompanyId, Week, TypeId, Title, Description, Severity, WorkersJson)
            VALUES ($id, $companyId, $week, $typeId, $title, $description, $severity, $workersJson);
            """;
        command.Parameters.AddWithValue("$id", incident.IncidentId);
        command.Parameters.AddWithValue("$companyId", incident.CompanyId);
        command.Parameters.AddWithValue("$week", incident.Week);
        command.Parameters.AddWithValue("$typeId", incident.TypeId);
        command.Parameters.AddWithValue("$title", incident.Titre);
        command.Parameters.AddWithValue("$description", incident.Description);
        command.Parameters.AddWithValue("$severity", incident.Gravite);
        command.Parameters.AddWithValue("$workersJson", JsonSerializer.Serialize(incident.Workers));
        command.ExecuteNonQuery();
    }

    public void EnregistrerDisciplinaryAction(DisciplinaryAction action)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO DisciplinaryActions (DisciplinaryActionId, CompanyId, WorkerId, Week, TypeId, Severity, MoraleDelta, Notes, IncidentId)
            VALUES ($id, $companyId, $workerId, $week, $typeId, $severity, $moraleDelta, $notes, $incidentId);
            """;
        command.Parameters.AddWithValue("$id", action.ActionId);
        command.Parameters.AddWithValue("$companyId", action.CompanyId);
        command.Parameters.AddWithValue("$workerId", action.WorkerId);
        command.Parameters.AddWithValue("$week", action.Week);
        command.Parameters.AddWithValue("$typeId", action.TypeId);
        command.Parameters.AddWithValue("$severity", action.Gravite);
        command.Parameters.AddWithValue("$moraleDelta", action.MoraleDelta);
        command.Parameters.AddWithValue("$notes", action.Notes);
        command.Parameters.AddWithValue("$incidentId", (object?)action.IncidentId ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<MoraleHistoryEntry> AppliquerMoraleImpacts(IReadOnlyList<BackstageMoraleImpact> impacts, int week)
    {
        if (impacts.Count == 0)
        {
            return Array.Empty<MoraleHistoryEntry>();
        }

        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();
        var historiques = new List<MoraleHistoryEntry>();

        foreach (var impact in impacts)
        {
            using var selectCommand = connexion.CreateCommand();
            selectCommand.Transaction = transaction;
            selectCommand.CommandText = "SELECT Morale FROM Workers WHERE WorkerId = $workerId;";
            selectCommand.Parameters.AddWithValue("$workerId", impact.WorkerId);
            var moraleAvant = Convert.ToInt32(selectCommand.ExecuteScalar());
            var moraleApres = Math.Clamp(moraleAvant + impact.Delta, 0, 100);

            using var updateCommand = connexion.CreateCommand();
            updateCommand.Transaction = transaction;
            updateCommand.CommandText = "UPDATE Workers SET Morale = $morale WHERE WorkerId = $workerId;";
            updateCommand.Parameters.AddWithValue("$morale", moraleApres);
            updateCommand.Parameters.AddWithValue("$workerId", impact.WorkerId);
            updateCommand.ExecuteNonQuery();

            using var historyCommand = connexion.CreateCommand();
            historyCommand.Transaction = transaction;
            historyCommand.CommandText = """
                INSERT INTO MoraleHistory (WorkerId, Week, MoraleBefore, MoraleAfter, Delta, Reason, IncidentId, ActionId)
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
                moraleAvant,
                moraleApres,
                impact.Delta,
                impact.Raison,
                impact.IncidentId,
                impact.ActionId));
        }

        transaction.Commit();
        return historiques;
    }

    public string ChargerCompagnieIdPourShow(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT CompanyId FROM Shows WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        return Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
    }

    public IReadOnlyList<ShowHistoryEntry> ChargerHistoriqueShow(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
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

    public CompanyState? ChargerEtatCompagnie(string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        if (TableExiste(connexion, "Companies"))
        {
            return ChargerCompagnie(connexion, companyId);
        }

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

    public IReadOnlyList<ContractPayroll> ChargerPaieContrats(string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        if (TableExiste(connexion, "Contracts"))
        {
            return ChargerPaieContratsUpper(connexion, companyId);
        }

        return ChargerPaieContratsLower(connexion, companyId);
    }

    public double AppliquerTransactionsFinancieres(
        string companyId,
        int semaine,
        IReadOnlyList<FinanceTransaction> transactions)
    {
        if (transactions.Count == 0)
        {
            var compagnie = ChargerEtatCompagnie(companyId);
            return compagnie?.Tresorerie ?? 0;
        }

        using var connexion = _factory.OuvrirConnexion();
        using var dbTransaction = connexion.BeginTransaction();

        foreach (var transactionFin in transactions)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = dbTransaction;
            command.CommandText = """
                INSERT INTO FinanceTransactions (CompanyId, ShowId, Date, Week, Category, Amount, Description)
                VALUES ($companyId, NULL, $date, $week, $category, $amount, $description);
                """;
            command.Parameters.AddWithValue("$companyId", companyId);
            command.Parameters.AddWithValue("$date", DateTimeOffset.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("$week", semaine);
            command.Parameters.AddWithValue("$category", transactionFin.Type);
            command.Parameters.AddWithValue("$amount", transactionFin.Montant);
            command.Parameters.AddWithValue("$description", transactionFin.Libelle);
            command.ExecuteNonQuery();

            using var treasuryCommand = connexion.CreateCommand();
            treasuryCommand.Transaction = dbTransaction;
            treasuryCommand.CommandText = "UPDATE Companies SET Treasury = Treasury + $amount WHERE CompanyId = $companyId;";
            treasuryCommand.Parameters.AddWithValue("$amount", transactionFin.Montant);
            treasuryCommand.Parameters.AddWithValue("$companyId", companyId);
            treasuryCommand.ExecuteNonQuery();
        }

        using var balanceCommand = connexion.CreateCommand();
        balanceCommand.Transaction = dbTransaction;
        balanceCommand.CommandText = "SELECT Treasury FROM Companies WHERE CompanyId = $companyId;";
        balanceCommand.Parameters.AddWithValue("$companyId", companyId);
        var tresorerie = Convert.ToDouble(balanceCommand.ExecuteScalar());

        dbTransaction.Commit();
        return tresorerie;
    }

    public void EnregistrerSnapshotFinance(string companyId, int semaine)
    {
        using var connexion = _factory.OuvrirConnexion();
        if (!TableExiste(connexion, "CompanyBalanceSnapshots") || !TableExiste(connexion, "FinanceTransactions"))
        {
            return;
        }

        using var transaction = connexion.BeginTransaction();

        using var totalsCommand = connexion.CreateCommand();
        totalsCommand.Transaction = transaction;
        totalsCommand.CommandText = """
            SELECT
                COALESCE(SUM(CASE WHEN Amount > 0 THEN Amount ELSE 0 END), 0),
                COALESCE(SUM(CASE WHEN Amount < 0 THEN -Amount ELSE 0 END), 0)
            FROM FinanceTransactions
            WHERE CompanyId = $companyId AND Week = $week;
            """;
        totalsCommand.Parameters.AddWithValue("$companyId", companyId);
        totalsCommand.Parameters.AddWithValue("$week", semaine);
        using var reader = totalsCommand.ExecuteReader();
        var revenus = 0.0;
        var depenses = 0.0;
        if (reader.Read())
        {
            revenus = reader.GetDouble(0);
            depenses = reader.GetDouble(1);
        }

        using var balanceCommand = connexion.CreateCommand();
        balanceCommand.Transaction = transaction;
        balanceCommand.CommandText = "SELECT Treasury FROM Companies WHERE CompanyId = $companyId;";
        balanceCommand.Parameters.AddWithValue("$companyId", companyId);
        var balance = Convert.ToDouble(balanceCommand.ExecuteScalar());

        using var insertCommand = connexion.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = """
            INSERT INTO CompanyBalanceSnapshots (CompanyId, Week, Balance, Revenues, Expenses)
            VALUES ($companyId, $week, $balance, $revenus, $depenses);
            """;
        insertCommand.Parameters.AddWithValue("$companyId", companyId);
        insertCommand.Parameters.AddWithValue("$week", semaine);
        insertCommand.Parameters.AddWithValue("$balance", balance);
        insertCommand.Parameters.AddWithValue("$revenus", revenus);
        insertCommand.Parameters.AddWithValue("$depenses", depenses);
        insertCommand.ExecuteNonQuery();

        transaction.Commit();
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

    public ScoutingTarget? ChargerCibleScouting(string workerId)
    {
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM scout_reports WHERE worker_id = $workerId AND semaine = $semaine;";
        command.Parameters.AddWithValue("$workerId", workerId);
        command.Parameters.AddWithValue("$semaine", semaine);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public void AjouterScoutReport(ScoutReport report)
    {
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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

    private IReadOnlyList<ScoutMission> ChargerMissions(string clause)
    {
        using var connexion = _factory.OuvrirConnexion();
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
        command.CommandText = "SELECT WorkerId, COALESCE(EndDate, fin_semaine) FROM Contracts;";
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

    public int ChargerSemaineShow(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Week FROM Shows WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int ChargerFatigueWorker(string workerId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Fatigue FROM Workers WHERE WorkerId = $workerId;";
        command.Parameters.AddWithValue("$workerId", workerId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void RecupererFatigueHebdo()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE Workers SET Fatigue = MAX(0, Fatigue - 12);";
        command.ExecuteNonQuery();
    }

    public void AjouterOffre(ContractOffer offre, IReadOnlyList<ContractClause> clauses)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();
        using var command = connexion.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO contract_offers (
                offer_id, negotiation_id, worker_id, company_id, type, debut_semaine, fin_semaine,
                salaire, bonus_show, buyout, non_compete_weeks, auto_renew, exclusif, statut,
                created_week, expiration_week, parent_offer_id, est_ia
            )
            VALUES (
                $offerId, $negotiationId, $workerId, $companyId, $type, $debutSemaine, $finSemaine,
                $salaire, $bonusShow, $buyout, $nonCompete, $autoRenew, $exclusif, $statut,
                $createdWeek, $expirationWeek, $parentOfferId, $estIa
            );
            """;
        command.Parameters.AddWithValue("$offerId", offre.OfferId);
        command.Parameters.AddWithValue("$negotiationId", offre.NegociationId);
        command.Parameters.AddWithValue("$workerId", offre.WorkerId);
        command.Parameters.AddWithValue("$companyId", offre.CompanyId);
        command.Parameters.AddWithValue("$type", offre.TypeContrat);
        command.Parameters.AddWithValue("$debutSemaine", offre.StartWeek);
        command.Parameters.AddWithValue("$finSemaine", offre.EndWeek);
        command.Parameters.AddWithValue("$salaire", offre.SalaireHebdo);
        command.Parameters.AddWithValue("$bonusShow", offre.BonusShow);
        command.Parameters.AddWithValue("$buyout", offre.Buyout);
        command.Parameters.AddWithValue("$nonCompete", offre.NonCompeteWeeks);
        command.Parameters.AddWithValue("$autoRenew", offre.RenouvellementAuto ? 1 : 0);
        command.Parameters.AddWithValue("$exclusif", offre.EstExclusif ? 1 : 0);
        command.Parameters.AddWithValue("$statut", offre.Statut);
        command.Parameters.AddWithValue("$createdWeek", offre.CreatedWeek);
        command.Parameters.AddWithValue("$expirationWeek", offre.ExpirationWeek);
        command.Parameters.AddWithValue("$parentOfferId", (object?)offre.ParentOfferId ?? DBNull.Value);
        command.Parameters.AddWithValue("$estIa", offre.EstIa ? 1 : 0);
        command.ExecuteNonQuery();

        InsererClauses(transaction, clauses, null, offre.OfferId);
        transaction.Commit();
    }

    public ContractOffer? ChargerOffre(string offerId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT offer_id, negotiation_id, worker_id, company_id, type, debut_semaine, fin_semaine,
                   salaire, bonus_show, buyout, non_compete_weeks, auto_renew, exclusif, statut,
                   created_week, expiration_week, parent_offer_id, est_ia
            FROM contract_offers
            WHERE offer_id = $offerId;
            """;
        command.Parameters.AddWithValue("$offerId", offerId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ContractOffer(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetInt32(5),
            reader.GetInt32(6),
            LireDecimal(reader, 7),
            LireDecimal(reader, 8),
            LireDecimal(reader, 9),
            reader.GetInt32(10),
            reader.GetInt32(11) == 1,
            reader.GetInt32(12) == 1,
            reader.GetString(13),
            reader.GetInt32(14),
            reader.GetInt32(15),
            reader.IsDBNull(16) ? null : reader.GetString(16),
            reader.GetInt32(17) == 1);
    }

    public IReadOnlyList<ContractOffer> ChargerOffres(string companyId, int offset, int limit)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT offer_id, negotiation_id, worker_id, company_id, type, debut_semaine, fin_semaine,
                   salaire, bonus_show, buyout, non_compete_weeks, auto_renew, exclusif, statut,
                   created_week, expiration_week, parent_offer_id, est_ia
            FROM contract_offers
            WHERE company_id = $companyId
            ORDER BY created_week DESC
            LIMIT $limit OFFSET $offset;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        command.Parameters.AddWithValue("$limit", limit);
        command.Parameters.AddWithValue("$offset", offset);
        using var reader = command.ExecuteReader();
        var offres = new List<ContractOffer>();
        while (reader.Read())
        {
            offres.Add(new ContractOffer(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                LireDecimal(reader, 7),
                LireDecimal(reader, 8),
                LireDecimal(reader, 9),
                reader.GetInt32(10),
                reader.GetInt32(11) == 1,
                reader.GetInt32(12) == 1,
                reader.GetString(13),
                reader.GetInt32(14),
                reader.GetInt32(15),
                reader.IsDBNull(16) ? null : reader.GetString(16),
                reader.GetInt32(17) == 1));
        }

        return offres;
    }

    public IReadOnlyList<ContractOffer> ChargerOffresExpirant(int semaine)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT offer_id, negotiation_id, worker_id, company_id, type, debut_semaine, fin_semaine,
                   salaire, bonus_show, buyout, non_compete_weeks, auto_renew, exclusif, statut,
                   created_week, expiration_week, parent_offer_id, est_ia
            FROM contract_offers
            WHERE expiration_week <= $semaine
              AND statut IN ('proposee', 'contre');
            """;
        command.Parameters.AddWithValue("$semaine", semaine);
        using var reader = command.ExecuteReader();
        var offres = new List<ContractOffer>();
        while (reader.Read())
        {
            offres.Add(new ContractOffer(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                LireDecimal(reader, 7),
                LireDecimal(reader, 8),
                LireDecimal(reader, 9),
                reader.GetInt32(10),
                reader.GetInt32(11) == 1,
                reader.GetInt32(12) == 1,
                reader.GetString(13),
                reader.GetInt32(14),
                reader.GetInt32(15),
                reader.IsDBNull(16) ? null : reader.GetString(16),
                reader.GetInt32(17) == 1));
        }

        return offres;
    }

    public IReadOnlyList<ContractClause> ChargerClausesPourOffre(string offerId)
    {
        return ChargerClauses("offer_id", offerId);
    }

    public void MettreAJourStatutOffre(string offerId, string statut)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE contract_offers SET statut = $statut WHERE offer_id = $offerId;";
        command.Parameters.AddWithValue("$statut", statut);
        command.Parameters.AddWithValue("$offerId", offerId);
        command.ExecuteNonQuery();
    }

    public void AjouterContratActif(ActiveContract contrat, IReadOnlyList<ContractClause> clauses)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();
        using var command = connexion.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO contracts (
                contract_id, worker_id, company_id, type, debut_semaine, fin_semaine, salaire, bonus_show,
                buyout, non_compete_weeks, auto_renew, exclusif, statut, created_week
            )
            VALUES (
                $contractId, $workerId, $companyId, $type, $debutSemaine, $finSemaine, $salaire, $bonusShow,
                $buyout, $nonCompete, $autoRenew, $exclusif, $statut, $createdWeek
            );
            """;
        command.Parameters.AddWithValue("$contractId", contrat.ContractId);
        command.Parameters.AddWithValue("$workerId", contrat.WorkerId);
        command.Parameters.AddWithValue("$companyId", contrat.CompanyId);
        command.Parameters.AddWithValue("$type", contrat.TypeContrat);
        command.Parameters.AddWithValue("$debutSemaine", contrat.StartWeek);
        command.Parameters.AddWithValue("$finSemaine", contrat.EndWeek);
        command.Parameters.AddWithValue("$salaire", contrat.SalaireHebdo);
        command.Parameters.AddWithValue("$bonusShow", contrat.BonusShow);
        command.Parameters.AddWithValue("$buyout", contrat.Buyout);
        command.Parameters.AddWithValue("$nonCompete", contrat.NonCompeteWeeks);
        command.Parameters.AddWithValue("$autoRenew", contrat.RenouvellementAuto ? 1 : 0);
        command.Parameters.AddWithValue("$exclusif", contrat.EstExclusif ? 1 : 0);
        command.Parameters.AddWithValue("$statut", contrat.Statut);
        command.Parameters.AddWithValue("$createdWeek", contrat.CreatedWeek);
        command.ExecuteNonQuery();

        InsererClauses(transaction, clauses, contrat.ContractId, null);
        transaction.Commit();
    }

    public ActiveContract? ChargerContratActif(string contractId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT contract_id, worker_id, company_id, type, debut_semaine, fin_semaine, salaire, bonus_show,
                   buyout, non_compete_weeks, auto_renew, exclusif, statut, created_week
            FROM contracts
            WHERE contract_id = $contractId;
            """;
        command.Parameters.AddWithValue("$contractId", contractId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ActiveContract(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetInt32(4),
            reader.GetInt32(5),
            LireDecimal(reader, 6),
            LireDecimal(reader, 7),
            LireDecimal(reader, 8),
            reader.GetInt32(9),
            reader.GetInt32(10) == 1,
            reader.GetInt32(11) == 1,
            reader.GetString(12),
            reader.GetInt32(13));
    }

    public ActiveContract? ChargerContratActif(string workerId, string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT contract_id, worker_id, company_id, type, debut_semaine, fin_semaine, salaire, bonus_show,
                   buyout, non_compete_weeks, auto_renew, exclusif, statut, created_week
            FROM contracts
            WHERE worker_id = $workerId
              AND company_id = $companyId
              AND statut = 'actif'
            ORDER BY fin_semaine DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ActiveContract(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetInt32(4),
            reader.GetInt32(5),
            LireDecimal(reader, 6),
            LireDecimal(reader, 7),
            LireDecimal(reader, 8),
            reader.GetInt32(9),
            reader.GetInt32(10) == 1,
            reader.GetInt32(11) == 1,
            reader.GetString(12),
            reader.GetInt32(13));
    }

    public IReadOnlyList<ContractClause> ChargerClausesPourContrat(string contractId)
    {
        return ChargerClauses("contract_id", contractId);
    }

    public void ResilierContrat(string contractId, int finSemaine)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE contracts
            SET fin_semaine = $finSemaine,
                statut = 'libere'
            WHERE contract_id = $contractId;
            """;
        command.Parameters.AddWithValue("$finSemaine", finSemaine);
        command.Parameters.AddWithValue("$contractId", contractId);
        command.ExecuteNonQuery();
    }

    public void EnregistrerNegociation(ContractNegotiationState negociation)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO negotiation_state (negotiation_id, worker_id, company_id, statut, last_offer_id, updated_week)
            VALUES ($negociationId, $workerId, $companyId, $statut, $lastOfferId, $updatedWeek)
            ON CONFLICT(negotiation_id) DO UPDATE SET
                worker_id = excluded.worker_id,
                company_id = excluded.company_id,
                statut = excluded.statut,
                last_offer_id = excluded.last_offer_id,
                updated_week = excluded.updated_week;
            """;
        command.Parameters.AddWithValue("$negociationId", negociation.NegociationId);
        command.Parameters.AddWithValue("$workerId", negociation.WorkerId);
        command.Parameters.AddWithValue("$companyId", negociation.CompanyId);
        command.Parameters.AddWithValue("$statut", negociation.Statut);
        command.Parameters.AddWithValue("$lastOfferId", (object?)negociation.DerniereOffreId ?? DBNull.Value);
        command.Parameters.AddWithValue("$updatedWeek", negociation.DerniereMiseAJourSemaine);
        command.ExecuteNonQuery();
    }

    public ContractNegotiationState? ChargerNegociation(string negociationId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT negotiation_id, worker_id, company_id, statut, last_offer_id, updated_week
            FROM negotiation_state
            WHERE negotiation_id = $negociationId;
            """;
        command.Parameters.AddWithValue("$negociationId", negociationId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ContractNegotiationState(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.GetInt32(5));
    }

    public ContractNegotiationState? ChargerNegociationPourWorker(string workerId, string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT negotiation_id, worker_id, company_id, statut, last_offer_id, updated_week
            FROM negotiation_state
            WHERE worker_id = $workerId AND company_id = $companyId
            ORDER BY updated_week DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ContractNegotiationState(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.GetInt32(5));
    }

    private IReadOnlyList<ContractClause> ChargerClauses(string colonne, string id)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = $"SELECT type, valeur FROM contract_clauses WHERE {colonne} = $id;";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        var clauses = new List<ContractClause>();
        while (reader.Read())
        {
            clauses.Add(new ContractClause(reader.GetString(0), reader.GetString(1)));
        }

        return clauses;
    }

    private static void InsererClauses(SqliteTransaction transaction, IReadOnlyList<ContractClause> clauses, string? contractId, string? offerId)
    {
        foreach (var clause in clauses)
        {
            using var command = transaction.Connection!.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO contract_clauses (contract_id, offer_id, type, valeur)
                VALUES ($contractId, $offerId, $type, $valeur);
                """;
            command.Parameters.AddWithValue("$contractId", (object?)contractId ?? DBNull.Value);
            command.Parameters.AddWithValue("$offerId", (object?)offerId ?? DBNull.Value);
            command.Parameters.AddWithValue("$type", clause.Type);
            command.Parameters.AddWithValue("$valeur", clause.Valeur);
            command.ExecuteNonQuery();
        }
    }

    private static decimal LireDecimal(SqliteDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? 0m : Convert.ToDecimal(reader.GetDouble(index));
    }

    private static T? LireJson<T>(SqliteDataReader reader, int index)
    {
        if (reader.IsDBNull(index))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(reader.GetString(index), _jsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static string? SerializeJson<T>(T value)
    {
        return value is null ? null : JsonSerializer.Serialize(value, _jsonOptions);
    }

    public ShowDefinition? ChargerShowDefinition(string showId)
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

    public TableUiSettings ChargerTableUiSettings()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT recherche, filtre_type, filtre_statut, colonnes_visibles, colonnes_ordre, tri_colonnes
            FROM ui_table_settings
            WHERE id = 1;
            """;
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return new TableUiSettings(null, null, null, new Dictionary<string, bool>(), new List<string>(), new List<TableSortSetting>());
        }

        var colonnesVisibles = LireJson<Dictionary<string, bool>>(reader, 3) ?? new Dictionary<string, bool>();
        var colonnesOrdre = LireJson<List<string>>(reader, 4) ?? new List<string>();
        var tris = LireJson<List<TableSortSetting>>(reader, 5) ?? new List<TableSortSetting>();
        return new TableUiSettings(
            reader.IsDBNull(0) ? null : reader.GetString(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            colonnesVisibles,
            colonnesOrdre,
            tris);
    }

    public void SauvegarderTableUiSettings(TableUiSettings settings)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO ui_table_settings (
                id,
                recherche,
                filtre_type,
                filtre_statut,
                colonnes_visibles,
                colonnes_ordre,
                tri_colonnes
            )
            VALUES (1, $recherche, $filtreType, $filtreStatut, $colonnesVisibles, $colonnesOrdre, $triColonnes)
            ON CONFLICT(id) DO UPDATE SET
                recherche = excluded.recherche,
                filtre_type = excluded.filtre_type,
                filtre_statut = excluded.filtre_statut,
                colonnes_visibles = excluded.colonnes_visibles,
                colonnes_ordre = excluded.colonnes_ordre,
                tri_colonnes = excluded.tri_colonnes;
            """;
        command.Parameters.AddWithValue("$recherche", (object?)settings.Recherche ?? DBNull.Value);
        command.Parameters.AddWithValue("$filtreType", (object?)settings.FiltreType ?? DBNull.Value);
        command.Parameters.AddWithValue("$filtreStatut", (object?)settings.FiltreStatut ?? DBNull.Value);
        command.Parameters.AddWithValue("$colonnesVisibles", SerializeJson(settings.ColonnesVisibles));
        command.Parameters.AddWithValue("$colonnesOrdre", SerializeJson(settings.ColonnesOrdre));
        command.Parameters.AddWithValue("$triColonnes", SerializeJson(settings.Tris));
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

    public IReadOnlyList<YouthStructureState> ChargerYouthStructures()
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE youth_structures SET budget_annuel = $budget WHERE youth_id = $youthId;";
        command.Parameters.AddWithValue("$budget", nouveauBudget);
        command.Parameters.AddWithValue("$youthId", youthId);
        command.ExecuteNonQuery();
    }

    public void AffecterCoachYouth(string youthId, string workerId, string role, int semaine)
    {
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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
        using var connexion = _factory.OuvrirConnexion();
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

        using var connexion = _factory.OuvrirConnexion();
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

    public IReadOnlyList<YouthTraineeProgressionState> ChargerTraineesPourProgression()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT yt.worker_id,
                   yt.youth_id,
                   w.in_ring,
                   w.entertainment,
                   w.story,
                   yt.statut
            FROM youth_trainees yt
            INNER JOIN workers w ON w.worker_id = yt.worker_id
            WHERE yt.statut = 'EN_FORMATION';
            """;
        using var reader = command.ExecuteReader();
        var trainees = new List<YouthTraineeProgressionState>();
        while (reader.Read())
        {
            trainees.Add(new YouthTraineeProgressionState(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetString(5)));
        }

        return trainees;
    }

    public void AppliquerProgressionYouth(YouthProgressionReport report)
    {
        if (report.Updates.Count == 0)
        {
            return;
        }

        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();

        foreach (var update in report.Updates)
        {
            using var workerCommand = connexion.CreateCommand();
            workerCommand.Transaction = transaction;
            workerCommand.CommandText = """
                UPDATE workers
                SET in_ring = $inRing,
                    entertainment = $entertainment,
                    story = $story,
                    type_worker = CASE WHEN $gradue = 1 THEN 'CATCHEUR' ELSE type_worker END
                WHERE worker_id = $workerId;
                """;
            workerCommand.Parameters.AddWithValue("$inRing", update.NouveauInRing);
            workerCommand.Parameters.AddWithValue("$entertainment", update.NouveauEntertainment);
            workerCommand.Parameters.AddWithValue("$story", update.NouveauStory);
            workerCommand.Parameters.AddWithValue("$gradue", update.EstGradue ? 1 : 0);
            workerCommand.Parameters.AddWithValue("$workerId", update.WorkerId);
            workerCommand.ExecuteNonQuery();

            UpsertWorkerAttribute(connexion, transaction, update.WorkerId, "in_ring", update.NouveauInRing);
            UpsertWorkerAttribute(connexion, transaction, update.WorkerId, "entertainment", update.NouveauEntertainment);
            UpsertWorkerAttribute(connexion, transaction, update.WorkerId, "story", update.NouveauStory);

            if (update.EstGradue)
            {
                using var gradCommand = connexion.CreateCommand();
                gradCommand.Transaction = transaction;
                gradCommand.CommandText = """
                    UPDATE youth_trainees
                    SET statut = 'GRADUE',
                        fin_semaine = $semaine
                    WHERE worker_id = $workerId
                      AND youth_id = $youthId;
                    """;
                gradCommand.Parameters.AddWithValue("$semaine", report.Semaine);
                gradCommand.Parameters.AddWithValue("$workerId", update.WorkerId);
                gradCommand.Parameters.AddWithValue("$youthId", update.YouthId);
                gradCommand.ExecuteNonQuery();
            }
        }

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

    private static ShowDefinition? ChargerShow(SqliteConnection connexion, string showId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT show_id, nom, semaine, region, duree, compagnie_id, tv_deal_id, lieu, diffusion
            FROM shows
            WHERE show_id = $showId;
            """;
        command.Parameters.AddWithValue("$showId", showId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

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

        return new ShowDefinition(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetInt32(2),
            reader.GetString(3),
            reader.GetInt32(4),
            reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            lieu,
            diffusion);
    }

    private static CompanyState? ChargerCompagnie(SqliteConnection connexion, string companyId)
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

    private static IReadOnlyList<ContractPayroll> ChargerPaieContratsUpper(SqliteConnection connexion, string companyId)
    {
        var hasSalary = ColonneExiste(connexion, "Contracts", "Salary");
        var hasFrequency = ColonneExiste(connexion, "Contracts", "PayFrequency");
        var salaryColumn = hasSalary ? "Contracts.Salary" : "0";
        var frequencyColumn = hasFrequency ? "Contracts.PayFrequency" : "'Hebdomadaire'";

        using var command = connexion.CreateCommand();
        command.CommandText = $"""
            SELECT Contracts.WorkerId,
                   COALESCE(Workers.Name, Contracts.WorkerId),
                   {salaryColumn},
                   {frequencyColumn}
            FROM Contracts
            LEFT JOIN Workers ON Workers.WorkerId = Contracts.WorkerId
            WHERE Contracts.CompanyId = $companyId;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var contrats = new List<ContractPayroll>();
        while (reader.Read())
        {
            var salaire = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
            var frequence = reader.IsDBNull(3) ? PayrollFrequency.Hebdomadaire : ConvertFrequence(reader.GetString(3));
            contrats.Add(new ContractPayroll(
                reader.GetString(0),
                reader.GetString(1),
                salaire,
                frequence));
        }

        return contrats;
    }

    private static IReadOnlyList<ContractPayroll> ChargerPaieContratsLower(SqliteConnection connexion, string companyId)
    {
        var hasSalary = ColonneExiste(connexion, "contracts", "salaire");
        var hasFrequency = ColonneExiste(connexion, "contracts", "pay_frequency");
        var salaryColumn = hasSalary ? "c.salaire" : "0";
        var frequencyColumn = hasFrequency ? "c.pay_frequency" : "'Hebdomadaire'";

        using var command = connexion.CreateCommand();
        command.CommandText = $"""
            SELECT c.worker_id,
                   TRIM(COALESCE(w.prenom, '') || ' ' || COALESCE(w.nom, c.worker_id)),
                   {salaryColumn},
                   {frequencyColumn}
            FROM contracts c
            LEFT JOIN workers w ON w.worker_id = c.worker_id
            WHERE c.company_id = $companyId;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var contrats = new List<ContractPayroll>();
        while (reader.Read())
        {
            var salaire = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
            var frequence = reader.IsDBNull(3) ? PayrollFrequency.Hebdomadaire : ConvertFrequence(reader.GetString(3));
            contrats.Add(new ContractPayroll(
                reader.GetString(0),
                reader.GetString(1),
                salaire,
                frequence));
        }

        return contrats;
    }

    private static PayrollFrequency ConvertFrequence(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
        {
            return PayrollFrequency.Hebdomadaire;
        }

        return valeur.Trim().ToLowerInvariant() switch
        {
            "mensuelle" => PayrollFrequency.Mensuelle,
            "mensuel" => PayrollFrequency.Mensuelle,
            "monthly" => PayrollFrequency.Mensuelle,
            _ => PayrollFrequency.Hebdomadaire
        };
    }

    private static bool TableExiste(SqliteConnection connexion, string table)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $table;";
        command.Parameters.AddWithValue("$table", table);
        return command.ExecuteScalar() is not null;
    }

    private static bool ColonneExiste(SqliteConnection connexion, string table, string colonne)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), colonne, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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

    private static List<WorkerSnapshot> ChargerWorkers(SqliteConnection connexion, IReadOnlyList<string> workerIds)
    {
        if (workerIds.Count == 0)
        {
            return new List<WorkerSnapshot>();
        }

        using var command = connexion.CreateCommand();
        var placeholders = workerIds.Select((id, index) => $"$id{index}").ToList();
        command.CommandText = $"""
            SELECT WorkerId, Name, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv, Morale
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
            var nomComplet = $"{reader.GetString(2)} {reader.GetString(1)}".Trim();
            workers.Add(new WorkerSnapshot(
                reader.GetString(0),
                nomComplet,
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetString(7),
                reader.GetInt32(8),
                reader.GetString(9),
                reader.GetInt32(10)));
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
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                ChargerStorylineParticipants(connexion, storylineId)));
        }

        return storylines;
    }

    private static void MettreAJourStorylineParticipants(
        SqliteConnection connexion,
        SqliteTransaction transaction,
        string storylineId,
        IReadOnlyList<StorylineParticipant> participants)
    {
        using var deleteCommand = connexion.CreateCommand();
        deleteCommand.Transaction = transaction;
        deleteCommand.CommandText = "DELETE FROM StorylineParticipants WHERE StorylineId = $storylineId;";
        deleteCommand.Parameters.AddWithValue("$storylineId", storylineId);
        deleteCommand.ExecuteNonQuery();

        foreach (var participant in participants)
        {
            using var insertCommand = connexion.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO StorylineParticipants (StorylineId, WorkerId, Role)
                VALUES ($storylineId, $workerId, $role);
                """;
            insertCommand.Parameters.AddWithValue("$storylineId", storylineId);
            insertCommand.Parameters.AddWithValue("$workerId", participant.WorkerId);
            insertCommand.Parameters.AddWithValue("$role", participant.Role);
            insertCommand.ExecuteNonQuery();
        }
    }

    private static List<StorylineParticipant> ChargerStorylineParticipants(SqliteConnection connexion, string storylineId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_id FROM storyline_participants WHERE storyline_id = $storylineId;";
        command.Parameters.AddWithValue("$storylineId", storylineId);
        using var reader = command.ExecuteReader();
        var participants = new List<StorylineParticipant>();
        while (reader.Read())
        {
            participants.Add(new StorylineParticipant(
                reader.GetString(0),
                reader.IsDBNull(1) ? "principal" : reader.GetString(1)));
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

    public IReadOnlyList<MatchTypeDefinition> ChargerMatchTypes()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT MatchTypeId, Libelle, Description, Participants, DureeParDefaut
            FROM MatchTypes
            ORDER BY Libelle ASC;
            """;
        using var reader = command.ExecuteReader();
        var types = new List<MatchTypeDefinition>();
        while (reader.Read())
        {
            types.Add(new MatchTypeDefinition(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetInt32(4)));
        }

        return types;
    }

    public void EnregistrerMatchTypes(IReadOnlyList<MatchTypeDefinition> types)
    {
        if (types.Count == 0)
        {
            return;
        }

        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();
        foreach (var type in types)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO MatchTypes (MatchTypeId, Libelle, Description, Participants, DureeParDefaut)
                VALUES ($id, $libelle, $description, $participants, $duree)
                ON CONFLICT(MatchTypeId)
                DO UPDATE SET Libelle = $libelle,
                              Description = $description,
                              Participants = $participants,
                              DureeParDefaut = $duree;
                """;
            command.Parameters.AddWithValue("$id", type.MatchTypeId);
            command.Parameters.AddWithValue("$libelle", type.Libelle);
            command.Parameters.AddWithValue("$description", (object?)type.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$participants", (object?)type.Participants ?? DBNull.Value);
            command.Parameters.AddWithValue("$duree", (object?)type.DureeParDefaut ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public IReadOnlyList<SegmentTemplateDefinition> ChargerSegmentTemplates()
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT TemplateId, Libelle, Description, SegmentsJson
            FROM SegmentTemplates
            ORDER BY Libelle ASC;
            """;
        using var reader = command.ExecuteReader();
        var templates = new List<SegmentTemplateDefinition>();
        while (reader.Read())
        {
            var segmentsJson = reader.GetString(3);
            var segments = JsonSerializer.Deserialize<IReadOnlyList<SegmentTemplateSegmentDefinition>>(segmentsJson, _jsonOptions)
                ?? Array.Empty<SegmentTemplateSegmentDefinition>();
            templates.Add(new SegmentTemplateDefinition(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                segments));
        }

        return templates;
    }

    public void EnregistrerSegmentTemplates(IReadOnlyList<SegmentTemplateDefinition> templates)
    {
        if (templates.Count == 0)
        {
            return;
        }

        using var connexion = _factory.OuvrirConnexion();
        using var transaction = connexion.BeginTransaction();
        foreach (var template in templates)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO SegmentTemplates (TemplateId, Libelle, Description, SegmentsJson)
                VALUES ($id, $libelle, $description, $segments)
                ON CONFLICT(TemplateId)
                DO UPDATE SET Libelle = $libelle,
                              Description = $description,
                              SegmentsJson = $segments;
                """;
            command.Parameters.AddWithValue("$id", template.TemplateId);
            command.Parameters.AddWithValue("$libelle", template.Libelle);
            command.Parameters.AddWithValue("$description", (object?)template.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$segments", JsonSerializer.Serialize(template.Segments, _jsonOptions));
            command.ExecuteNonQuery();
        }

        transaction.Commit();
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

        using var youthProgrammeCommand = connexion.CreateCommand();
        youthProgrammeCommand.Transaction = transaction;
        youthProgrammeCommand.CommandText = """
            INSERT INTO youth_programs (programme_id, youth_id, nom, type, duree_semaines, focus_attributs, actif)
            VALUES ('PROG-001', 'YOUTH-001', 'Fondamentaux', 'fundamentaux', 12, '["in_ring","story"]', 1);
            """;
        youthProgrammeCommand.ExecuteNonQuery();

        using var workersCommand = connexion.CreateCommand();
        workersCommand.Transaction = transaction;
        workersCommand.CommandText = """
            INSERT INTO workers (worker_id, nom, prenom, company_id, in_ring, entertainment, story, popularite, fatigue, blessure, momentum, role_tv, type_worker, morale)
            VALUES
            ('W-001', 'Dubois', 'Alex', 'COMP-001', 70, 62, 58, 55, 12, 'AUCUNE', 4, 'MAIN_EVENT', 'CATCHEUR', 62),
            ('W-002', 'Martin', 'Leo', 'COMP-001', 64, 70, 65, 52, 18, 'AUCUNE', 2, 'UPPER_MID', 'CATCHEUR', 58),
            ('W-003', 'Petit', 'Sarah', 'COMP-001', 68, 60, 72, 49, 20, 'AUCUNE', 1, 'MID', 'CATCHEUR', 55),
            ('W-004', 'Roche', 'Maya', 'COMP-001', 58, 74, 66, 46, 15, 'AUCUNE', 0, 'MID', 'CATCHEUR', 60);
            """;
        workersCommand.ExecuteNonQuery();

        using var youthProgramCommand = connexion.CreateCommand();
        youthProgramCommand.Transaction = transaction;
        youthProgramCommand.CommandText = """
            INSERT INTO youth_programs (program_id, youth_id, nom, duree_semaines, focus)
            VALUES ('YP-001', 'YOUTH-001', 'Fondamentaux', 12, 'in_ring');
            """;
        youthProgramCommand.ExecuteNonQuery();

        using var youthStaffCommand = connexion.CreateCommand();
        youthStaffCommand.Transaction = transaction;
        youthStaffCommand.CommandText = """
            INSERT INTO youth_staff_assignments (youth_id, worker_id, role, semaine_debut)
            VALUES ('YOUTH-001', 'W-004', 'Coach technique', 1);
            """;
        youthStaffCommand.ExecuteNonQuery();

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
            INSERT INTO shows (show_id, nom, semaine, region, duree, compagnie_id, tv_deal_id, lieu, diffusion)
            VALUES ('SHOW-001', 'Weekly Clash', 1, 'FR', 120, 'COMP-001', 'TV-001', 'Paris', 'Ring General TV');
            """;
        showCommand.ExecuteNonQuery();

        using var dealCommand = connexion.CreateCommand();
        dealCommand.Transaction = transaction;
        dealCommand.CommandText = """
            INSERT INTO tv_deals (tv_deal_id, company_id, network_name, reach_bonus, audience_cap, audience_min, base_revenue, revenue_per_point, penalty, constraints)
            VALUES ('TV-001', 'COMP-001', 'RG Network', 12, 85, 40, 4500, 55, 1500, 'Show principal en prime time');
            """;
        dealCommand.ExecuteNonQuery();

        using var segmentCommand = connexion.CreateCommand();
        segmentCommand.Transaction = transaction;
        segmentCommand.CommandText = """
            INSERT INTO ShowSegments (ShowSegmentId, ShowId, OrderIndex, SegmentType, DurationMinutes, StorylineId, TitleId, IsMainEvent, Intensity, WinnerWorkerId, LoserWorkerId)
            VALUES
            ('SEG-001', 'SHOW-001', 1, 'promo', 8, 'S-001', NULL, 0, 40, NULL, NULL),
            ('SEG-002', 'SHOW-001', 2, 'match', 12, NULL, NULL, 0, 60, 'W-003', 'W-004'),
            ('SEG-003', 'SHOW-001', 3, 'match', 18, 'S-001', 'T-001', 1, 75, 'W-001', 'W-002');
            """;
        segmentCommand.ExecuteNonQuery();

        using var participantCommand = connexion.CreateCommand();
        participantCommand.Transaction = transaction;
        participantCommand.CommandText = """
            INSERT INTO SegmentParticipants (ShowSegmentId, WorkerId)
            VALUES
            ('SEG-001', 'W-001'), ('SEG-001', 'W-002'),
            ('SEG-002', 'W-003'), ('SEG-002', 'W-004'),
            ('SEG-003', 'W-001'), ('SEG-003', 'W-002');
            """;
        participantCommand.ExecuteNonQuery();

        using var settingsCommand = connexion.CreateCommand();
        settingsCommand.Transaction = transaction;
        settingsCommand.CommandText = """
            INSERT INTO SegmentSettings (ShowSegmentId, SettingKey, SettingValue)
            VALUES
            ('SEG-001', 'storyHeavy', 'OUI'),
            ('SEG-002', 'typeMatch', 'Singles'),
            ('SEG-003', 'typeMatch', 'Singles');
            """;
        settingsCommand.ExecuteNonQuery();

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
            INSERT INTO contracts (worker_id, company_id, fin_semaine, salaire, pay_frequency)
            VALUES
            ('W-001', 'COMP-001', 30, 1200, 'Hebdomadaire'),
            ('W-002', 'COMP-001', 12, 850, 'Hebdomadaire'),
            ('W-003', 'COMP-001', 6, 700, 'Mensuelle'),
            ('W-004', 'COMP-001', 20, 600, 'Hebdomadaire');
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

    private static void InitialiserBibliotheque(SqliteConnection connexion)
    {
        using var matchCountCommand = connexion.CreateCommand();
        matchCountCommand.CommandText = "SELECT COUNT(1) FROM match_types";
        var matchCount = Convert.ToInt32(matchCountCommand.ExecuteScalar());

        using var templateCountCommand = connexion.CreateCommand();
        templateCountCommand.CommandText = "SELECT COUNT(1) FROM segment_templates";
        var templateCount = Convert.ToInt32(templateCountCommand.ExecuteScalar());

        if (matchCount > 0 && templateCount > 0)
        {
            return;
        }

        using var transaction = connexion.BeginTransaction();

        if (matchCount == 0)
        {
            using var matchCommand = connexion.CreateCommand();
            matchCommand.Transaction = transaction;
            matchCommand.CommandText = """
                INSERT INTO match_types (match_type_id, nom, description, actif, ordre)
                VALUES
                ('singles', 'Singles', '1 contre 1', 1, 1),
                ('tag', 'Tag team', '2 contre 2', 1, 2),
                ('triple-threat', 'Triple threat', '3 participants, sans élimination', 1, 3),
                ('fatal-four-way', 'Fatal four-way', '4 participants, vainqueur direct', 0, 4);
                """;
            matchCommand.ExecuteNonQuery();
        }

        if (templateCount == 0)
        {
            using var templateCommand = connexion.CreateCommand();
            templateCommand.Transaction = transaction;
            templateCommand.CommandText = """
                INSERT INTO segment_templates (template_id, nom, type_segment, duree, main_event, intensite, match_type_id)
                VALUES
                ('tpl-open-promo', 'Ouverture promo', 'promo', 6, 0, 35, NULL),
                ('tpl-angle-backstage', 'Angle backstage', 'angle_backstage', 4, 0, 30, NULL),
                ('tpl-match-simple', 'Match simple', 'match', 10, 0, 60, 'singles'),
                ('tpl-main-event', 'Main event classique', 'match', 18, 1, 75, 'singles');
                """;
            templateCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
