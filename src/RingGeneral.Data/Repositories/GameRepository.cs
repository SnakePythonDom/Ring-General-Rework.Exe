using System.Text.Json;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using RingGeneral.Data.Models;
using MatchType = RingGeneral.Core.Models.MatchType;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// GameRepository acts as a Façade orchestrating specialized repositories.
///
/// REFACTORING STATUS: In progress - transitioning from monolithic to façade pattern.
///
/// Methods are being systematically delegated to specialized repositories:
/// - ShowRepository: Show and segment management
/// - CompanyRepository: Company, finance, TV deals
/// - WorkerRepository: Worker data and fatigue
/// - BackstageRepository: Incidents, discipline, morale
/// - ScoutingRepository: Scouting and recruiting
/// - ContractRepository: Contracts and negotiations
/// - SettingsRepository: Game settings
/// - YouthRepository: Youth development and generation
///
/// Methods KEPT in GameRepository:
/// - Cross-domain orchestration (ChargerShowContext, ChargerBookingPlan, AppliquerDelta)
/// - Initialization (Initialiser, EnregistrerMatchTypes, EnregistrerSegmentTemplates)
/// - Legacy methods not yet migrated
///
/// Note: GameRepository no longer implements IScoutingRepository or IContractRepository.
/// Use RepositoryContainer from RepositoryFactory.CreateRepositories() to access specialized repositories directly.
/// </summary>
public sealed class GameRepository
{
    private readonly SqliteConnectionFactory _factory;
    private readonly ShowRepository _showRepository;
    private readonly CompanyRepository _companyRepository;
    private readonly WorkerRepository _workerRepository;
    private readonly BackstageRepository _backstageRepository;
    private readonly ScoutingRepository _scoutingRepository;
    private readonly ContractRepository _contractRepository;
    private readonly SettingsRepository _settingsRepository;
    private readonly YouthRepository _youthRepository;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GameRepository(
        SqliteConnectionFactory factory,
        ShowRepository showRepository,
        CompanyRepository companyRepository,
        WorkerRepository workerRepository,
        BackstageRepository backstageRepository,
        ScoutingRepository scoutingRepository,
        ContractRepository contractRepository,
        SettingsRepository settingsRepository,
        YouthRepository youthRepository)
    {
        _factory = factory;
        _showRepository = showRepository;
        _companyRepository = companyRepository;
        _workerRepository = workerRepository;
        _backstageRepository = backstageRepository;
        _scoutingRepository = scoutingRepository;
        _contractRepository = contractRepository;
        _settingsRepository = settingsRepository;
        _youthRepository = youthRepository;
    }

    /// <summary>
    /// Crée et ouvre une nouvelle connexion à la base de données
    /// </summary>
    /// <returns>Connexion SQLite ouverte</returns>
    public SqliteConnection CreateConnection()
    {
        return _factory.OuvrirConnexion();
    }

    /// <summary>
    /// Initialise la base de données avec les tables nécessaires.
    /// </summary>
    /// <remarks>
    /// TODO: DETTE TECHNIQUE - DUPLICATION DE SCHÉMA
    ///
    /// Il existe actuellement deux systèmes de création de tables:
    /// 1. Cette méthode (GameRepository.Initialiser) - crée des tables snake_case (workers, companies, etc.)
    /// 2. DbInitializer.ApplyMigrations() - crée des tables PascalCase (Workers, Companies, etc.)
    ///
    /// Les deux ensembles coexistent, ce qui peut causer confusion et bugs silencieux.
    ///
    /// SOLUTION RECOMMANDÉE (Phase 1):
    /// 1. Supprimer ces CREATE TABLE et utiliser uniquement DbInitializer
    /// 2. Mettre à jour toutes les requêtes SQL pour utiliser les noms PascalCase
    /// 3. Ajouter une migration de transition pour les anciennes bases
    ///
    /// Voir: docs/PLAN_ACTION_FR.md - Tâche 0.3
    /// </remarks>
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
                worker_id TEXT NOT NULL,
                role TEXT NOT NULL DEFAULT 'principal'
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
            CREATE TABLE IF NOT EXISTS InboxItems (
                InboxItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                Type TEXT NOT NULL,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL,
                Week INTEGER NOT NULL
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
                statut TEXT NOT NULL DEFAULT 'en_cours',
                last_offer_id TEXT,
                updated_week INTEGER NOT NULL DEFAULT 1
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
            CREATE TABLE IF NOT EXISTS scout_reports (
                report_id TEXT PRIMARY KEY,
                worker_id TEXT NOT NULL,
                nom TEXT NOT NULL,
                region TEXT,
                potentiel INTEGER NOT NULL DEFAULT 0,
                in_ring INTEGER NOT NULL DEFAULT 0,
                entertainment INTEGER NOT NULL DEFAULT 0,
                story INTEGER NOT NULL DEFAULT 0,
                resume TEXT,
                notes TEXT,
                semaine INTEGER NOT NULL,
                source TEXT
            );
            CREATE TABLE IF NOT EXISTS scout_missions (
                mission_id TEXT PRIMARY KEY,
                titre TEXT NOT NULL,
                region TEXT,
                focus TEXT,
                progression INTEGER NOT NULL DEFAULT 0,
                objectif INTEGER NOT NULL DEFAULT 100,
                statut TEXT NOT NULL DEFAULT 'active',
                semaine_debut INTEGER NOT NULL,
                semaine_maj INTEGER NOT NULL
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

    public IReadOnlyList<TvDeal> ChargerTvDeals(string companyId)
        => _companyRepository.ChargerTvDeals(companyId);

    private static TvDeal? ChargerTvDeal(SqliteConnection connexion, string? dealId)
    {
        if (string.IsNullOrWhiteSpace(dealId))
        {
            return null;
        }

        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT tv_deal_id, company_id, network_name, reach_bonus, audience_cap, audience_min,
                   base_revenue, revenue_per_point, penalty, constraints
            FROM tv_deals
            WHERE tv_deal_id = $dealId;
            """;
        command.Parameters.AddWithValue("$dealId", dealId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new TvDeal(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetInt32(4),
            reader.GetInt32(5),
            reader.GetDouble(6),
            reader.GetDouble(7),
            reader.GetDouble(8),
            reader.GetString(9));
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
        => _showRepository.ChargerShowsAVenir(compagnieId, semaineActuelle);

    public void CreerShow(ShowDefinition show)
        => _showRepository.CreerShow(show);

    public void AjouterSegment(string showId, SegmentDefinition segment, int ordre)
        => _showRepository.AjouterSegment(showId, segment, ordre);

    public void MettreAJourSegment(SegmentDefinition segment)
        => _showRepository.MettreAJourSegment(segment);

    public void SupprimerSegment(string segmentId)
        => _showRepository.SupprimerSegment(segmentId);

    public void MettreAJourOrdreSegments(string showId, IReadOnlyList<string> segmentIds)
        => _showRepository.MettreAJourOrdreSegments(showId, segmentIds);

    public IReadOnlyList<SegmentTemplate> ChargerSegmentTemplates()
        => _showRepository.ChargerSegmentTemplates();

    public IReadOnlyList<MatchType> ChargerMatchTypes()
        => _showRepository.ChargerMatchTypes();

    public void MettreAJourMatchType(MatchType matchType)
        => _showRepository.MettreAJourMatchType(matchType);

    public void EnregistrerRapport(ShowReport rapport)
        => _showRepository.EnregistrerRapport(rapport);

    public IReadOnlyList<AudienceHistoryEntry> ChargerAudienceHistorique(string showId)
        => _showRepository.ChargerAudienceHistorique(showId);

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
        => _companyRepository.ChargerInbox();

    public IReadOnlyList<WorkerBackstageProfile> ChargerBackstageRoster(string companyId)
        => _workerRepository.ChargerBackstageRoster(companyId);

    public IReadOnlyDictionary<string, int> ChargerMorales(string companyId)
        => _workerRepository.ChargerMorales(companyId);

    public int ChargerMorale(string workerId)
        => _workerRepository.ChargerMorale(workerId);

    public void EnregistrerBackstageIncident(BackstageIncident incident)
        => _backstageRepository.EnregistrerBackstageIncident(incident);

    public void EnregistrerDisciplinaryAction(DisciplinaryAction action)
        => _backstageRepository.EnregistrerDisciplinaryAction(action);

    public IReadOnlyList<MoraleHistoryEntry> AppliquerMoraleImpacts(IReadOnlyList<BackstageMoraleImpact> impacts, int week)
        => _backstageRepository.AppliquerMoraleImpacts(impacts, week);

    public string ChargerCompagnieIdPourShow(string showId)
        => _companyRepository.ChargerCompagnieIdPourShow(showId);

    public IReadOnlyList<ShowHistoryEntry> ChargerHistoriqueShow(string showId)
        => _showRepository.ChargerHistoriqueShow(showId);

    public IReadOnlyList<CompanyState> ChargerCompagnies()
        => _companyRepository.ChargerCompagnies();

    public CompanyState? ChargerEtatCompagnie(string companyId)
        => _companyRepository.ChargerEtatCompagnie(companyId);

    public IReadOnlyList<ContractPayroll> ChargerPaieContrats(string companyId)
        => _companyRepository.ChargerPaieContrats(companyId);

    public double AppliquerTransactionsFinancieres(
        string companyId,
        int semaine,
        IReadOnlyList<FinanceTransaction> transactions)
        => _companyRepository.AppliquerTransactionsFinancieres(companyId, semaine, transactions);

    public void EnregistrerSnapshotFinance(string companyId, int semaine)
        => _companyRepository.EnregistrerSnapshotFinance(companyId, semaine);

    public void AppliquerImpactCompagnie(string compagnieId, int deltaPrestige, double deltaTresorerie)
        => _companyRepository.AppliquerImpactCompagnie(compagnieId, deltaPrestige, deltaTresorerie);

    public void AjouterInboxItem(InboxItem item)
        => _companyRepository.AjouterInboxItem(item);

    public ScoutingTarget? ChargerCibleScouting(string workerId)
        => _scoutingRepository.ChargerCibleScouting(workerId);

    public IReadOnlyList<ScoutingTarget> ChargerCiblesScouting(int limite)
        => _scoutingRepository.ChargerCiblesScouting(limite);

    public bool RapportExiste(string workerId, int semaine)
        => _scoutingRepository.RapportExiste(workerId, semaine);

    public void AjouterScoutReport(ScoutReport report)
        => _scoutingRepository.AjouterScoutReport(report);

    public IReadOnlyList<ScoutReport> ChargerScoutReports()
        => _scoutingRepository.ChargerScoutReports();

    public void AjouterShortlist(ShortlistEntry entry)
        => _scoutingRepository.AjouterShortlist(entry);

    public IReadOnlyList<ShortlistEntry> ChargerShortlist()
        => _scoutingRepository.ChargerShortlist();

    public void AjouterMission(ScoutMission mission)
        => _scoutingRepository.AjouterMission(mission);

    public IReadOnlyList<ScoutMission> ChargerMissionsActives()
        => _scoutingRepository.ChargerMissionsActives();

    public IReadOnlyList<ScoutMission> ChargerScoutMissions()
        => _scoutingRepository.ChargerScoutMissions();

    public void MettreAJourMissionProgress(string missionId, int progression, string statut, int semaineMaj)
        => _scoutingRepository.MettreAJourMissionProgress(missionId, progression, statut, semaineMaj);

    public int IncrementerSemaine(string showId)
        => _showRepository.IncrementerSemaine(showId);

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
        => _workerRepository.ChargerNomsWorkers();

    public int ChargerSemaineShow(string showId)
        => _showRepository.ChargerSemaineShow(showId);

    public int ChargerFatigueWorker(string workerId)
        => _workerRepository.ChargerFatigueWorker(workerId);

    public void RecupererFatigueHebdo()
        => _workerRepository.RecupererFatigueHebdo();

    public void AjouterOffre(ContractOffer offre, IReadOnlyList<ContractClause> clauses)
        => _contractRepository.AjouterOffre(offre, clauses);

    public ContractOffer? ChargerOffre(string offerId)
        => _contractRepository.ChargerOffre(offerId);

    public IReadOnlyList<ContractOffer> ChargerOffres(string companyId, int offset, int limit)
        => _contractRepository.ChargerOffres(companyId, offset, limit);

    public IReadOnlyList<ContractOffer> ChargerOffresExpirant(int semaine)
        => _contractRepository.ChargerOffresExpirant(semaine);

    public IReadOnlyList<ContractClause> ChargerClausesPourOffre(string offerId)
        => _contractRepository.ChargerClausesPourOffre(offerId);

    public void MettreAJourStatutOffre(string offerId, string statut)
        => _contractRepository.MettreAJourStatutOffre(offerId, statut);

    public void AjouterContratActif(ActiveContract contrat, IReadOnlyList<ContractClause> clauses)
        => _contractRepository.AjouterContratActif(contrat, clauses);

    public ActiveContract? ChargerContratActif(string contractId)
        => _contractRepository.ChargerContratActif(contractId);

    public ActiveContract? ChargerContratActif(string workerId, string companyId)
        => _contractRepository.ChargerContratActif(workerId, companyId);

    public IReadOnlyList<ContractClause> ChargerClausesPourContrat(string contractId)
        => _contractRepository.ChargerClausesPourContrat(contractId);

    public void ResilierContrat(string contractId, int finSemaine)
        => _contractRepository.ResilierContrat(contractId, finSemaine);

    public void EnregistrerNegociation(ContractNegotiationState negociation)
        => _contractRepository.EnregistrerNegociation(negociation);

    public ContractNegotiationState? ChargerNegociation(string negociationId)
        => _contractRepository.ChargerNegociation(negociationId);

    public ContractNegotiationState? ChargerNegociationPourWorker(string workerId, string companyId)
        => _contractRepository.ChargerNegociationPourWorker(workerId, companyId);

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
        => _showRepository.ChargerShowDefinition(showId);

    public WorkerGenerationOptions ChargerParametresGeneration()
        => _settingsRepository.ChargerParametresGeneration();

    public void SauvegarderParametresGeneration(WorkerGenerationOptions options)
        => _settingsRepository.SauvegarderParametresGeneration(options);

    public TableUiSettings ChargerTableUiSettings()
        => _settingsRepository.ChargerTableUiSettings();

    public void SauvegarderTableUiSettings(TableUiSettings settings)
        => _settingsRepository.SauvegarderTableUiSettings(settings);

    public IReadOnlyList<YouthStructureState> ChargerYouthStructuresPourGeneration()
        => _youthRepository.ChargerYouthStructuresPourGeneration();

    public IReadOnlyList<YouthStructureState> ChargerYouthStructures()
        => _youthRepository.ChargerYouthStructures();

    public IReadOnlyList<YouthTraineeInfo> ChargerYouthTrainees(string youthId)
        => _youthRepository.ChargerYouthTrainees(youthId);

    public IReadOnlyList<YouthProgramInfo> ChargerYouthPrograms(string youthId)
        => _youthRepository.ChargerYouthPrograms(youthId);

    public IReadOnlyList<YouthStaffAssignmentInfo> ChargerYouthStaffAssignments(string youthId)
        => _youthRepository.ChargerYouthStaffAssignments(youthId);

    public void ChangerBudgetYouth(string youthId, int nouveauBudget)
        => _youthRepository.ChangerBudgetYouth(youthId, nouveauBudget);

    public void AffecterCoachYouth(string youthId, string workerId, string role, int semaine)
        => _youthRepository.AffecterCoachYouth(youthId, workerId, role, semaine);

    public void DiplomerTrainee(string workerId, int semaine)
        => _youthRepository.DiplomerTrainee(workerId, semaine);

    public IReadOnlyList<YouthTraineeProgressionState> ChargerYouthTraineesPourProgression()
        => _youthRepository.ChargerYouthTraineesPourProgression();

    public void EnregistrerProgressionTrainees(YouthProgressionReport report)
        => _youthRepository.EnregistrerProgressionTrainees(report);

    public GenerationCounters ChargerGenerationCounters(int annee)
        => _youthRepository.ChargerGenerationCounters(annee);

    public void EnregistrerGeneration(WorkerGenerationReport report)
        => _youthRepository.EnregistrerGeneration(report);

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

    private static List<WorkerSnapshot> ChargerWorkers(SqliteConnection connexion, IReadOnlyList<string> workerIds)
    {
        if (workerIds.Count == 0)
        {
            return new List<WorkerSnapshot>();
        }

        using var command = connexion.CreateCommand();
        var placeholders = workerIds.Select((id, index) => $"$id{index}").ToList();
        command.CommandText = $"""
            SELECT worker_id, nom || ' ' || prenom, in_ring, entertainment, story, popularite, fatigue, blessure, momentum, role_tv, morale
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
            var nomComplet = reader.GetString(1);
            workers.Add(new WorkerSnapshot(
                reader.GetString(0),
                nomComplet,
                reader.GetInt32(2),
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
                StorylinePhase.Setup,
                reader.GetInt32(2),
                StorylineStatus.Active,
                null,
                ChargerStorylineParticipants(connexion, storylineId)));
        }

        return storylines;
    }

    private static List<StorylineParticipant> ChargerStorylineParticipants(SqliteConnection connexion, string storylineId)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_id, role FROM storyline_participants WHERE storyline_id = $storylineId;";
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
            INSERT INTO youth_programs (program_id, youth_id, nom, duree_semaines, focus)
            VALUES ('PROG-001', 'YOUTH-001', 'Fondamentaux', 12, 'in_ring,story');
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
            INSERT INTO contracts (worker_id, company_id, fin_semaine, salaire)
            VALUES
            ('W-001', 'COMP-001', 30, 1200),
            ('W-002', 'COMP-001', 12, 850),
            ('W-003', 'COMP-001', 6, 700),
            ('W-004', 'COMP-001', 20, 600);
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

        using var gameSettingsCommand = connexion.CreateCommand();
        gameSettingsCommand.Transaction = transaction;
        gameSettingsCommand.CommandText = """
            INSERT INTO game_settings (id, youth_generation_mode, world_generation_mode, semaine_pivot_annuelle)
            VALUES (1, 'Realiste', 'Desactivee', 1);
            """;
        gameSettingsCommand.ExecuteNonQuery();

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
