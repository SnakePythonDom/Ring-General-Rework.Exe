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

    public void MettreAJourOrdreSegment(string segmentId, int ordre)
        => _showRepository.MettreAJourOrdreSegment(segmentId, ordre);

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

}
