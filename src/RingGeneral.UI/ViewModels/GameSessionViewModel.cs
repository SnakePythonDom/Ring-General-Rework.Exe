using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Collections;
using ReactiveUI;
using System.Reactive;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Medical;
using RingGeneral.Core.Random;
using RingGeneral.Core.Services;
using RingGeneral.Core.Simulation;
using RingGeneral.Core.Validation;
using RingGeneral.Data.Database;
using RingGeneral.Data.Models;
using RingGeneral.Data.Repositories;
using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;
using RingGeneral.UI.Services;
using LogLevel = RingGeneral.Core.Services.LogLevel;

namespace RingGeneral.UI.ViewModels;

public sealed class GameSessionViewModel : ViewModelBase
{
    private const string ShowId = "SHOW-001";
    private GameRepository? _repository;
    private IScoutingRepository? _scoutingRepository;
    private readonly MedicalRepository? _medicalRepository;
    private readonly InjuryService? _injuryService;
    private readonly ILoggingService _logger;
    private readonly BookingValidator _validator = new();
    private readonly IReadOnlyDictionary<string, string> _segmentLabels = new Dictionary<string, string>();
    private readonly TemplateService _templateService = new();
    private readonly SegmentTypeCatalog _segmentCatalog;
    private readonly BookingBuilderService _bookingBuilder = new();
    private readonly HelpContentProvider _helpProvider = new();
    private readonly IReadOnlyDictionary<string, HelpPageEntry> _helpPages;
    private readonly IReadOnlyDictionary<string, HelpPageEntry> _impactPages;
    private readonly TooltipHelper _tooltipHelper;
    private readonly StorylineService _storylineService = new();
    private ShowContext? _context;
    private readonly List<GlobalSearchResultViewModel> _rechercheGlobaleIndex = new();
    private readonly List<TableSortSetting> _tableSortSettings = new();
    private bool _suspendTablePreferences;

    public GameSessionViewModel(string? cheminDb = null, ServiceContainer? services = null)
    {
        // Initialize logger from service container or use default
        _logger = services?.IsRegistered<ILoggingService>() == true
            ? services.Resolve<ILoggingService>()
            : new ConsoleLoggingService(LogLevel.Info);

        var cheminFinal = string.IsNullOrWhiteSpace(cheminDb)
            ? Path.Combine(Directory.GetCurrentDirectory(), "ringgeneral.db")
            : cheminDb;

        try
        {
            _logger.Info($"Initializing GameSession with database: {cheminFinal}");

            // Apply database migrations first to ensure all tables exist
            var initializer = new DbInitializer();
            initializer.CreateDatabaseIfMissing(cheminFinal);

            var factory = new SqliteConnectionFactory($"Data Source={cheminFinal}");
            var repositories = RepositoryFactory.CreateRepositories(factory);
            _repository = repositories.GameRepository;
            _scoutingRepository = repositories.ScoutingRepository;
            _medicalRepository = new MedicalRepository(factory);
            _injuryService = new InjuryService(new MedicalRecommendations());

            _logger.Info("GameSession initialized successfully");
        }
        catch (Exception ex)
        {
            // En cas d'échec d'initialisation, l'application continue en mode lecture seule
            // L'utilisateur sera notifié via l'interface qu'aucune sauvegarde n'est chargée
            _logger.Fatal("Failed to initialize database", ex);
            _repository = null;
            _scoutingRepository = null;
        }
        _segmentCatalog = ChargerSegmentTypes();
        _segmentLabels = _segmentCatalog.Labels;
        _tooltipHelper = new TooltipHelper(_helpProvider);
        _helpPages = ChargerPages();
        _impactPages = _helpPages
            .Where(page => page.Key.StartsWith("impacts.", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        Segments = new ObservableCollection<SegmentViewModel>();
        ValidationIssues = new ObservableCollection<BookingIssueViewModel>();
        Resultats = new ObservableCollection<SegmentResultViewModel>();
        Inbox = new ObservableCollection<InboxItemViewModel>();
        AttributsPrincipaux = new ObservableCollection<AttributeViewModel>();
        PourquoiNote = new ObservableCollection<string>();
        Conseils = new ObservableCollection<string>();
        ImpactPages = new ObservableCollection<ImpactPageViewModel>();
        ShowsAVenir = new ObservableCollection<ShowCalendarItemViewModel>();
        SegmentTypes = new ObservableCollection<SegmentTypeOptionViewModel>();
        SegmentTemplates = new ObservableCollection<SegmentTemplateViewModel>();
        MatchTypes = new ObservableCollection<MatchTypeViewModel>();
        WorkersDisponibles = new ObservableCollection<ParticipantViewModel>();
        StorylinesDisponibles = new ObservableCollection<StorylineOptionViewModel>();
        TitresDisponibles = new ObservableCollection<TitleOptionViewModel>();
        DealsTv = new ObservableCollection<TvDealViewModel>();
        ReachMap = new ObservableCollection<ReachMapItemViewModel>();
        ContraintesDiffusion = new ObservableCollection<string>();
        AudienceHistorique = new ObservableCollection<AudienceHistoryItemViewModel>();
        ConsignesBooking = new ObservableCollection<string>();
        RecapFm = new ObservableCollection<string>();
        HistoriqueShow = new ObservableCollection<ShowHistoryViewModel>();
        NouveauSegmentParticipants = new ObservableCollection<ParticipantViewModel>();
        AidePanel = new HelpPanelViewModel();
        Codex = ChargerCodex();
        YouthStructures = new ObservableCollection<YouthStructureViewModel>();
        YouthTrainees = new ObservableCollection<YouthTraineeViewModel>();
        YouthPrograms = new ObservableCollection<YouthProgramViewModel>();
        YouthStaffAssignments = new ObservableCollection<YouthStaffAssignmentViewModel>();
        _suspendTablePreferences = true;
        TableItems = new ObservableCollection<TableViewItemViewModel>();
        TableItemsView = new DataGridCollectionView(TableItems)
        {
            Filter = FiltrerTableItems
        };
        TableConfiguration = new TableViewConfigurationViewModel();
        TableColumns = new ObservableCollection<TableColumnOrderViewModel>
        {
            new("Nom", "Nom"),
            new("Type", "Type"),
            new("Compagnie", "Compagnie"),
            new("Role", "Rôle"),
            new("Statut", "Statut"),
            new("Popularite", "Popularité"),
            new("Momentum", "Momentum"),
            new("Note", "Note")
        };
        TableTypeFilters = new ObservableCollection<TableFilterOptionViewModel>
        {
            new("tous", "Tous"),
            new("worker", "Workers"),
            new("company", "Compagnies"),
            new("title", "Titres"),
            new("storyline", "Storylines")
        };
        TableStatusFilters = new ObservableCollection<TableFilterOptionViewModel>
        {
            new("tous", "Tous"),
            new("actif", "Actif"),
            new("repos", "En repos"),
            new("blesse", "Blessé"),
            new("vacant", "Vacant"),
            new("en-cours", "En cours"),
            new("suspendue", "Suspendue"),
            new("terminee", "Terminée")
        };
        TableSelectedTypeFilter = TableTypeFilters[0];
        TableSelectedStatusFilter = TableStatusFilters[0];
        ChargerPreferencesTable();
        _suspendTablePreferences = false;
        TableConfiguration.PropertyChanged += (_, _) => SauvegarderPreferencesTable();
        TableColumns.CollectionChanged += (_, _) => SauvegarderPreferencesTable();
        RechercheGlobaleResultats = new ObservableCollection<GlobalSearchResultViewModel>();
        OuvrirRechercheGlobaleCommand = ReactiveCommand.Create(OuvrirRechercheGlobale);
        FermerRechercheGlobaleCommand = ReactiveCommand.Create(FermerRechercheGlobale);
        YouthGenerationModes = new[]
        {
            new YouthGenerationOptionViewModel("Désactivée", YouthGenerationMode.Desactivee),
            new YouthGenerationOptionViewModel("Réaliste", YouthGenerationMode.Realiste),
            new YouthGenerationOptionViewModel("Abondante", YouthGenerationMode.Abondante)
        };
        WorldGenerationModes = new[]
        {
            new WorldGenerationOptionViewModel("Désactivée", WorldGenerationMode.Desactivee),
            new WorldGenerationOptionViewModel("Faible", WorldGenerationMode.Faible)
        };
        StorylinePhases = new[]
        {
            new StorylinePhaseOptionViewModel("BUILD", "Build"),
            new StorylinePhaseOptionViewModel("PEAK", "Peak"),
            new StorylinePhaseOptionViewModel("BLOWOFF", "Blowoff")
        };
        StorylineStatuts = new[]
        {
            new StorylineStatusOptionViewModel("ACTIVE", "Active"),
            new StorylineStatusOptionViewModel("SUSPENDUE", "Suspendue"),
            new StorylineStatusOptionViewModel("TERMINEE", "Terminée")
        };

        InitialiserSegmentTypes();
        InitialiserConsignesBooking();
        InitialiserBibliotheque();
        ChargerShow();
        ChargerBibliotheque();
        ChargerInbox();
        ChargerHistoriqueShow();
        ChargerImpactsInitial();
        InitialiserNouveauShow();
        ChargerYouth();
    }

    public ObservableCollection<SegmentViewModel> Segments { get; }
    public ObservableCollection<BookingIssueViewModel> ValidationIssues { get; }
    public ObservableCollection<SegmentResultViewModel> Resultats { get; }
    public ObservableCollection<InboxItemViewModel> Inbox { get; }
    public ObservableCollection<AttributeViewModel> AttributsPrincipaux { get; }
    public ObservableCollection<string> PourquoiNote { get; }
    public ObservableCollection<string> Conseils { get; }
    public ObservableCollection<ImpactPageViewModel> ImpactPages { get; }
    public ObservableCollection<ShowCalendarItemViewModel> ShowsAVenir { get; }
    public ObservableCollection<SegmentTypeOptionViewModel> SegmentTypes { get; }
    public ObservableCollection<SegmentTemplateViewModel> SegmentTemplates { get; }
    public ObservableCollection<MatchTypeViewModel> MatchTypes { get; }
    public ObservableCollection<ParticipantViewModel> WorkersDisponibles { get; }
    public ObservableCollection<StorylineOptionViewModel> StorylinesDisponibles { get; }
    public ObservableCollection<TitleOptionViewModel> TitresDisponibles { get; }
    public ObservableCollection<TvDealViewModel> DealsTv { get; }
    public ObservableCollection<ReachMapItemViewModel> ReachMap { get; }
    public ObservableCollection<string> ContraintesDiffusion { get; }
    public ObservableCollection<AudienceHistoryItemViewModel> AudienceHistorique { get; }
    public ObservableCollection<string> ConsignesBooking { get; }
    public ObservableCollection<string> RecapFm { get; }
    public ObservableCollection<ShowHistoryViewModel> HistoriqueShow { get; }
    public ObservableCollection<ParticipantViewModel> NouveauSegmentParticipants { get; }
    public HelpPanelViewModel AidePanel { get; }
    public CodexViewModel Codex { get; }
    public ObservableCollection<YouthStructureViewModel> YouthStructures { get; }
    public ObservableCollection<YouthTraineeViewModel> YouthTrainees { get; }
    public ObservableCollection<YouthProgramViewModel> YouthPrograms { get; }
    public ObservableCollection<YouthStaffAssignmentViewModel> YouthStaffAssignments { get; }
    public ObservableCollection<TableViewItemViewModel> TableItems { get; }
    public DataGridCollectionView TableItemsView { get; }
    public TableViewConfigurationViewModel TableConfiguration { get; }
    public ObservableCollection<TableColumnOrderViewModel> TableColumns { get; }
    public ObservableCollection<TableFilterOptionViewModel> TableTypeFilters { get; }
    public ObservableCollection<TableFilterOptionViewModel> TableStatusFilters { get; }
    public ObservableCollection<GlobalSearchResultViewModel> RechercheGlobaleResultats { get; }

    public ReactiveCommand<Unit, Unit> OuvrirRechercheGlobaleCommand { get; }
    public ReactiveCommand<Unit, Unit> FermerRechercheGlobaleCommand { get; }

    public IReadOnlyList<YouthGenerationOptionViewModel> YouthGenerationModes { get; }
    public IReadOnlyList<WorldGenerationOptionViewModel> WorldGenerationModes { get; }
    public IReadOnlyList<StorylinePhaseOptionViewModel> StorylinePhases { get; }
    public IReadOnlyList<StorylineStatusOptionViewModel> StorylineStatuts { get; }

    public SegmentViewModel? SegmentSelectionne
    {
        get => _segmentSelectionne;
        set => this.RaiseAndSetIfChanged(ref _segmentSelectionne, value);
    }
    private SegmentViewModel? _segmentSelectionne;

    public YouthGenerationOptionViewModel? YouthGenerationSelection
    {
        get => _youthGenerationSelection;
        set => this.RaiseAndSetIfChanged(ref _youthGenerationSelection, value);
    }
    private YouthGenerationOptionViewModel? _youthGenerationSelection;

    public WorldGenerationOptionViewModel? WorldGenerationSelection
    {
        get => _worldGenerationSelection;
        set => this.RaiseAndSetIfChanged(ref _worldGenerationSelection, value);
    }
    private WorldGenerationOptionViewModel? _worldGenerationSelection;

    public int SemainePivotAnnuelle
    {
        get => _semainePivotAnnuelle;
        set => this.RaiseAndSetIfChanged(ref _semainePivotAnnuelle, value);
    }
    private int _semainePivotAnnuelle = 1;

    public string? ParametresGenerationMessage
    {
        get => _parametresGenerationMessage;
        private set => this.RaiseAndSetIfChanged(ref _parametresGenerationMessage, value);
    }
    private string? _parametresGenerationMessage;

    public string? NouveauSegmentStorylineId
    {
        get => _nouveauSegmentStorylineId;
        set => this.RaiseAndSetIfChanged(ref _nouveauSegmentStorylineId, value);
    }
    private string? _nouveauSegmentStorylineId;

    public YouthStructureViewModel? YouthStructureSelection
    {
        get => _youthStructureSelection;
        set
        {
            this.RaiseAndSetIfChanged(ref _youthStructureSelection, value);
            ChargerYouthDetails();
        }
    }
    private YouthStructureViewModel? _youthStructureSelection;

    public int YouthBudgetNouveau
    {
        get => _youthBudgetNouveau;
        set => this.RaiseAndSetIfChanged(ref _youthBudgetNouveau, value);
    }
    private int _youthBudgetNouveau;

    public string? YouthCoachWorkerId
    {
        get => _youthCoachWorkerId;
        set => this.RaiseAndSetIfChanged(ref _youthCoachWorkerId, value);
    }
    private string? _youthCoachWorkerId;

    public string? YouthCoachRole
    {
        get => _youthCoachRole;
        set => this.RaiseAndSetIfChanged(ref _youthCoachRole, value);
    }
    private string? _youthCoachRole = "Coach technique";

    public string? YouthActionMessage
    {
        get => _youthActionMessage;
        private set => this.RaiseAndSetIfChanged(ref _youthActionMessage, value);
    }
    private string? _youthActionMessage;

    public IReadOnlyDictionary<string, string> Tooltips => _tooltipHelper.Tooltips;

    public string? ValidationErreurs
    {
        get => _validationErreurs;
        private set => this.RaiseAndSetIfChanged(ref _validationErreurs, value);
    }
    private string? _validationErreurs;

    public string? ValidationAvertissements
    {
        get => _validationAvertissements;
        private set => this.RaiseAndSetIfChanged(ref _validationAvertissements, value);
    }
    private string? _validationAvertissements;

    public string? NouveauShowNom
    {
        get => _nouveauShowNom;
        set => this.RaiseAndSetIfChanged(ref _nouveauShowNom, value);
    }
    private string? _nouveauShowNom;

    public int NouveauShowSemaine
    {
        get => _nouveauShowSemaine;
        set => this.RaiseAndSetIfChanged(ref _nouveauShowSemaine, value);
    }
    private int _nouveauShowSemaine;

    public int NouveauShowDuree
    {
        get => _nouveauShowDuree;
        set => this.RaiseAndSetIfChanged(ref _nouveauShowDuree, value);
    }
    private int _nouveauShowDuree;

    public string? NouveauShowLieu
    {
        get => _nouveauShowLieu;
        set => this.RaiseAndSetIfChanged(ref _nouveauShowLieu, value);
    }
    private string? _nouveauShowLieu;

    public string? NouveauShowDiffusion
    {
        get => _nouveauShowDiffusion;
        set => this.RaiseAndSetIfChanged(ref _nouveauShowDiffusion, value);
    }
    private string? _nouveauShowDiffusion;

    public string? NouveauSegmentTypeId
    {
        get => _nouveauSegmentTypeId;
        set => this.RaiseAndSetIfChanged(ref _nouveauSegmentTypeId, value);
    }
    private string? _nouveauSegmentTypeId;

    public int NouveauSegmentDuree
    {
        get => _nouveauSegmentDuree;
        set => this.RaiseAndSetIfChanged(ref _nouveauSegmentDuree, value);
    }
    private int _nouveauSegmentDuree = 8;

    public bool NouveauSegmentMainEvent
    {
        get => _nouveauSegmentMainEvent;
        set => this.RaiseAndSetIfChanged(ref _nouveauSegmentMainEvent, value);
    }
    private bool _nouveauSegmentMainEvent;

    public string? NouveauSegmentParticipantId
    {
        get => _nouveauSegmentParticipantId;
        set => this.RaiseAndSetIfChanged(ref _nouveauSegmentParticipantId, value);
    }
    private string? _nouveauSegmentParticipantId;

    public SegmentTemplateViewModel? TemplateSelectionnee
    {
        get => _templateSelectionnee;
        set => this.RaiseAndSetIfChanged(ref _templateSelectionnee, value);
    }
    private SegmentTemplateViewModel? _templateSelectionnee;

    public string? ResumeShow
    {
        get => _resumeShow;
        private set => this.RaiseAndSetIfChanged(ref _resumeShow, value);
    }
    private string? _resumeShow;

    public string? AudienceResume
    {
        get => _audienceResume;
        private set => this.RaiseAndSetIfChanged(ref _audienceResume, value);
    }
    private string? _audienceResume;

    public string? DetailsSimulation
    {
        get => _detailsSimulation;
        private set => this.RaiseAndSetIfChanged(ref _detailsSimulation, value);
    }
    private string? _detailsSimulation;

    public bool DetailsSimulationVisible
    {
        get => _detailsSimulationVisible;
        set => this.RaiseAndSetIfChanged(ref _detailsSimulationVisible, value);
    }
    private bool _detailsSimulationVisible;

    public string? TableRecherche
    {
        get => _tableRecherche;
        set
        {
            this.RaiseAndSetIfChanged(ref _tableRecherche, value);
            if (!_suspendTablePreferences)
            {
                AppliquerFiltreTable();
                SauvegarderPreferencesTable();
            }
        }
    }
    private string? _tableRecherche;

    public TableFilterOptionViewModel TableSelectedTypeFilter
    {
        get => _tableSelectedTypeFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _tableSelectedTypeFilter, value);
            if (!_suspendTablePreferences)
            {
                AppliquerFiltreTable();
                SauvegarderPreferencesTable();
            }
        }
    }
    private TableFilterOptionViewModel _tableSelectedTypeFilter;

    public TableFilterOptionViewModel TableSelectedStatusFilter
    {
        get => _tableSelectedStatusFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _tableSelectedStatusFilter, value);
            if (!_suspendTablePreferences)
            {
                AppliquerFiltreTable();
                SauvegarderPreferencesTable();
            }
        }
    }
    private TableFilterOptionViewModel _tableSelectedStatusFilter;

    public string? TableResultatsResume
    {
        get => _tableResultatsResume;
        private set => this.RaiseAndSetIfChanged(ref _tableResultatsResume, value);
    }
    private string? _tableResultatsResume;

    public string? TableTriResume
    {
        get => _tableTriResume;
        private set => this.RaiseAndSetIfChanged(ref _tableTriResume, value);
    }
    private string? _tableTriResume;

    public IReadOnlyList<TableSortSetting> TableSortSettings => _tableSortSettings;

    public TableViewItemViewModel? TableSelection
    {
        get => _tableSelection;
        set
        {
            this.RaiseAndSetIfChanged(ref _tableSelection, value);
            this.RaisePropertyChanged(nameof(TableSelectionDisponible));
            this.RaisePropertyChanged(nameof(TableSelectionContexte));
        }
    }
    private TableViewItemViewModel? _tableSelection;

    public bool TableSelectionDisponible => TableSelection is not null;

    public string TableSelectionContexte => TableSelection is null
        ? "Aucune fiche sélectionnée"
        : $"{TableSelection.Nom} · {TableSelection.Type}";

    public bool RechercheGlobaleVisible
    {
        get => _rechercheGlobaleVisible;
        private set => this.RaiseAndSetIfChanged(ref _rechercheGlobaleVisible, value);
    }
    private bool _rechercheGlobaleVisible;

    public string? RechercheGlobaleQuery
    {
        get => _rechercheGlobaleQuery;
        set
        {
            this.RaiseAndSetIfChanged(ref _rechercheGlobaleQuery, value);
            MettreAJourRechercheGlobale();
        }
    }
    private string? _rechercheGlobaleQuery;

    public bool RechercheGlobaleAucunResultat
    {
        get => _rechercheGlobaleAucunResultat;
        private set => this.RaiseAndSetIfChanged(ref _rechercheGlobaleAucunResultat, value);
    }
    private bool _rechercheGlobaleAucunResultat;

    public bool AideOuverte
    {
        get => _aideOuverte;
        private set => this.RaiseAndSetIfChanged(ref _aideOuverte, value);
    }
    private bool _aideOuverte;

    public ImpactPageViewModel? ImpactSelectionnee
    {
        get => _impactSelectionnee;
        private set => this.RaiseAndSetIfChanged(ref _impactSelectionnee, value);
    }
    private ImpactPageViewModel? _impactSelectionnee;

    public SegmentResultViewModel? ResultatSelectionne
    {
        get => _resultatSelectionne;
        set => this.RaiseAndSetIfChanged(ref _resultatSelectionne, value);
    }
    private SegmentResultViewModel? _resultatSelectionne;

    public void OuvrirRechercheGlobale()
    {
        RechercheGlobaleVisible = true;
        RechercheGlobaleQuery ??= string.Empty;
        MettreAJourRechercheGlobale();
    }

    public void FermerRechercheGlobale()
    {
        RechercheGlobaleVisible = false;
    }

    public void SimulerShow()
    {
        if (_context is null || _repository is null || _medicalRepository is null || _injuryService is null)
        {
            return;
        }

        var booking = _repository.ChargerBookingPlan(_context);
        var validation = _validator.ValiderBooking(booking);
        MettreAJourValidation(validation);

        if (!validation.EstValide)
        {
            return;
        }

        var seed = HashCode.Combine(_context.Show.ShowId, _context.Show.Semaine);
        var engine = new ShowSimulationEngine(new SeededRandomProvider(seed));
        var resultat = engine.Simuler(_context);
        Resultats.Clear();
        var workerNames = ConstruireNomsWorkers();
        foreach (var segment in resultat.RapportShow.Segments)
        {
            var libelle = _segmentCatalog.Labels.TryGetValue(segment.TypeSegment, out var label) ? label : segment.TypeSegment;
            Resultats.Add(new SegmentResultViewModel(segment, workerNames, libelle));
        }
        ResultatSelectionne = Resultats.FirstOrDefault();

        ResumeShow = $"Note {resultat.RapportShow.NoteGlobale} • Audience {resultat.RapportShow.Audience} • Billetterie {resultat.RapportShow.Billetterie:C} • TV {resultat.RapportShow.Tv:C}";
        AudienceResume =
            $"Audience {resultat.RapportShow.Audience} • Reach {resultat.RapportShow.AudienceDetails.Reach} • Stars {resultat.RapportShow.AudienceDetails.Stars} • Saturation {resultat.RapportShow.AudienceDetails.Saturation}";
        MettreAJourAnalyseShow(resultat);
        MettreAJourImpacts(resultat);
        MettreAJourRecapFm(resultat);

        var impactApplier = new ImpactApplier(_repository, _medicalRepository, _injuryService);
        var segmentResults = resultat.RapportShow.Segments
            .Select(segment => new SegmentResult(segment.SegmentId, segment.Note, $"Segment {segment.TypeSegment}", segment))
            .ToList();
        var impactContext = new ImpactContext(
            _context.Show.ShowId,
            segmentResults,
            _context.Storylines.Select(storyline => storyline.StorylineId).ToList(),
            resultat.RapportShow,
            resultat.Delta);
        impactApplier.AppliquerImpacts(impactContext);

        ChargerShow();
        ChargerHistoriqueShow();
    }

    public void CreerShow()
    {
        if (_repository is null || _context is null)
        {
            return;
        }

        var nom = string.IsNullOrWhiteSpace(NouveauShowNom) ? "Nouveau show" : NouveauShowNom.Trim();
        var lieu = string.IsNullOrWhiteSpace(NouveauShowLieu) ? _context.Show.Region : NouveauShowLieu.Trim();
        var diffusion = string.IsNullOrWhiteSpace(NouveauShowDiffusion) ? "À définir" : NouveauShowDiffusion.Trim();
        var show = new ShowDefinition(
            $"SHOW-{Guid.NewGuid():N}".ToUpperInvariant(),
            nom,
            NouveauShowSemaine,
            _context.Show.Region,
            NouveauShowDuree,
            _context.Show.CompagnieId,
            _context.Show.DealTvId,
            lieu,
            diffusion);

        _repository.CreerShow(show);
        ChargerCalendrier();
        NouveauShowNom = null;
        NouveauShowLieu = null;
        NouveauShowDiffusion = null;
    }

    public void AjouterSegment()
    {
        if (_repository is null || _context is null)
        {
            return;
        }

        var type = string.IsNullOrWhiteSpace(NouveauSegmentTypeId)
            ? SegmentTypes.FirstOrDefault()?.Id ?? "match"
            : NouveauSegmentTypeId;
        var participants = NouveauSegmentParticipants.Select(p => p.WorkerId).ToList();
        var settings = ObtenirSettingsParDefaut(type);
        var intensite = CalculerIntensite(settings, 60);
        var newSegment = new SegmentDefinition(
            $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
            type,
            participants,
            Math.Max(1, NouveauSegmentDuree),
            NouveauSegmentMainEvent,
            string.IsNullOrWhiteSpace(NouveauSegmentStorylineId) ? null : NouveauSegmentStorylineId,
            null,
            intensite,
            null,
            null,
            settings);

        _repository.AjouterSegment(_context.Show.ShowId, newSegment, Segments.Count + 1);
        NouveauSegmentParticipants.Clear();
        NouveauSegmentTypeId = type;
        NouveauSegmentDuree = 8;
        NouveauSegmentMainEvent = false;
        NouveauSegmentParticipantId = null;
        NouveauSegmentStorylineId = null;
        ChargerShow();
    }

    public void EnregistrerSegment(SegmentViewModel segment)
    {
        if (_repository is null || _context is null)
        {
            return;
        }

        var settings = segment.ConstruireSettings();
        var intensite = CalculerIntensite(settings, segment.Intensite);
        var storylineId = string.IsNullOrWhiteSpace(segment.StorylineId) ? null : segment.StorylineId;
        var titreId = string.IsNullOrWhiteSpace(segment.TitreId) ? null : segment.TitreId;
        var updated = new SegmentDefinition(
            segment.SegmentId,
            segment.TypeSegment,
            segment.Participants.Select(p => p.WorkerId).ToList(),
            Math.Max(1, segment.DureeMinutes),
            segment.EstMainEvent,
            storylineId,
            titreId,
            intensite,
            segment.VainqueurId,
            segment.PerdantId,
            settings);

        _repository.MettreAJourSegment(updated);
        ChargerShow();
    }

    public void CopierSegment(SegmentViewModel segment)
    {
        if (_repository is null || _context is null)
        {
            return;
        }

        var copieSegment = new SegmentDefinition(
            segment.SegmentId,
            segment.TypeSegment,
            segment.Participants.Select(p => p.WorkerId).ToList(),
            segment.DureeMinutes,
            segment.EstMainEvent,
            string.IsNullOrWhiteSpace(segment.StorylineId) ? null : segment.StorylineId,
            string.IsNullOrWhiteSpace(segment.TitreId) ? null : segment.TitreId,
            segment.Intensite,
            segment.VainqueurId,
            segment.PerdantId,
            segment.ConstruireSettings());

        var copie = _bookingBuilder.DupliquerSegment(copieSegment);

        _repository.AjouterSegment(_context.Show.ShowId, copie, Segments.Count + 1);
        ChargerShow();
    }

    public void DupliquerMatch(SegmentViewModel segment)
    {
        if (_repository is null || _context is null)
        {
            return;
        }

        var copieSegment = new SegmentDefinition(
            segment.SegmentId,
            segment.TypeSegment,
            segment.Participants.Select(p => p.WorkerId).ToList(),
            segment.DureeMinutes,
            segment.EstMainEvent,
            string.IsNullOrWhiteSpace(segment.StorylineId) ? null : segment.StorylineId,
            string.IsNullOrWhiteSpace(segment.TitreId) ? null : segment.TitreId,
            segment.Intensite,
            segment.VainqueurId,
            segment.PerdantId,
            segment.ConstruireSettings());

        var copie = _bookingBuilder.DupliquerMatch(copieSegment);

        _repository.AjouterSegment(_context.Show.ShowId, copie, Segments.Count + 1);
        ChargerShow();
    }

    public void SupprimerSegment(SegmentViewModel segment)
    {
        if (_repository is null || _context is null)
        {
            return;
        }

        Segments.Remove(segment);
        _repository.MettreAJourOrdreSegments(_context.Show.ShowId, Segments.Select(s => s.SegmentId).ToList());
        ChargerShow();
    }

    public void AppliquerTemplateSelectionnee()
    {
        if (TemplateSelectionnee is null)
        {
            return;
        }

        AppliquerTemplate(TemplateSelectionnee);
    }

    public void AppliquerTemplate(SegmentTemplateViewModel template)
    {
        if (_repository is null || _context is null)
        {
            return;
        }

        var segmentTemplate = new SegmentTemplate(
            template.TemplateId,
            template.Nom,
            template.TypeSegment,
            template.DureeMinutes,
            template.EstMainEvent,
            template.Intensite,
            template.MatchTypeId);

        var segment = _templateService.AppliquerTemplate(segmentTemplate);
        _repository.AjouterSegment(_context.Show.ShowId, segment, Segments.Count + 1);

        ChargerShow();
    }

    public void DeplacerSegment(SegmentViewModel segment, int delta)
    {
        if (_repository is null || _context is null)
        {
            return;
        }

        var index = Segments.IndexOf(segment);
        var target = index + delta;
        if (index < 0 || target < 0 || target >= Segments.Count)
        {
            return;
        }

        Segments.Move(index, target);
        _repository.MettreAJourOrdreSegments(_context.Show.ShowId, Segments.Select(s => s.SegmentId).ToList());
        MettreAJourAvertissements();
    }

    public void AjouterParticipant(SegmentViewModel segment)
    {
        if (segment.ParticipantSelectionneeId is null)
        {
            return;
        }

        if (segment.Participants.Any(p => p.WorkerId == segment.ParticipantSelectionneeId))
        {
            return;
        }

        var worker = WorkersDisponibles.FirstOrDefault(p => p.WorkerId == segment.ParticipantSelectionneeId);
        if (worker is null)
        {
            return;
        }

        segment.Participants.Add(new ParticipantViewModel(worker.WorkerId, worker.Nom));
        segment.ParticipantSelectionneeId = null;
        MettreAJourAvertissements();
    }

    public void RetirerParticipant(SegmentViewModel segment, ParticipantViewModel participant)
    {
        segment.Participants.Remove(participant);
        MettreAJourAvertissements();
    }

    public void AjouterParticipantNouveauSegment()
    {
        if (NouveauSegmentParticipantId is null)
        {
            return;
        }

        if (NouveauSegmentParticipants.Any(p => p.WorkerId == NouveauSegmentParticipantId))
        {
            return;
        }

        var worker = WorkersDisponibles.FirstOrDefault(p => p.WorkerId == NouveauSegmentParticipantId);
        if (worker is null)
        {
            return;
        }

        NouveauSegmentParticipants.Add(new ParticipantViewModel(worker.WorkerId, worker.Nom));
        NouveauSegmentParticipantId = null;
    }

    public void RetirerParticipantNouveauSegment(ParticipantViewModel participant)
    {
        NouveauSegmentParticipants.Remove(participant);
    }

    public void PasserSemaineSuivante()
    {
        if (_repository is null)
        {
            return;
        }

        var weekly = new WeeklyLoopService(_repository, _scoutingRepository!);
        weekly.PasserSemaineSuivante(ShowId);
        ChargerInbox();
        ChargerShow();
    }

    public void CorrigerIssue(BookingIssueViewModel issue)
    {
        if (_context is null || _repository is null)
        {
            return;
        }

        switch (issue.Code)
        {
            case "booking.empty":
                AjouterSegment();
                break;
            case "booking.duration.exceed":
                CorrigerDureeTotale();
                break;
            case "booking.main-event.missing":
                MarquerMainEvent();
                break;
            case "segment.duration.invalid":
                CorrigerDureeSegment(issue.SegmentId);
                break;
            default:
                SelectionnerSegment(issue.SegmentId);
                break;
        }
    }

    public void EnregistrerParametresGeneration()
    {
        if (_repository is null)
        {
            ParametresGenerationMessage = "Impossible d'enregistrer : aucune sauvegarde chargée.";
            return;
        }

        var youthMode = YouthGenerationSelection?.Mode ?? YouthGenerationMode.Realiste;
        var worldMode = WorldGenerationSelection?.Mode ?? WorldGenerationMode.Desactivee;
        var pivot = SemainePivotAnnuelle > 0 ? SemainePivotAnnuelle : (int?)null;
        _repository.SauvegarderParametresGeneration(new WorkerGenerationOptions(youthMode, worldMode, pivot));
        ParametresGenerationMessage = "Paramètres de génération enregistrés.";
    }

    public void OuvrirAide(string pageId)
    {
        if (_helpPages.TryGetValue(pageId, out var page))
        {
            AidePanel.Titre = page.Titre;
            AidePanel.Resume = page.Resume;
            AidePanel.Sections.Clear();
            foreach (var section in page.Sections)
            {
                AidePanel.Sections.Add(new HelpSectionViewModel(section.Titre, section.Contenu));
            }

            AidePanel.ErreursFrequentes.Clear();
            foreach (var erreur in page.ErreursFrequentes)
            {
                AidePanel.ErreursFrequentes.Add(erreur);
            }

            AideOuverte = true;
        }
    }

    public void FermerAide()
    {
        AideOuverte = false;
    }

    public void SelectionnerImpact(string pageId)
    {
        ImpactSelectionnee = ImpactPages.FirstOrDefault(page =>
            page.Id.Equals(pageId, StringComparison.OrdinalIgnoreCase));
    }

    public void OuvrirFicheWorker(string? workerId)
    {
        if (string.IsNullOrWhiteSpace(workerId) || _context is null)
        {
            return;
        }

        var worker = _context.Workers.FirstOrDefault(w => w.WorkerId == workerId);
        if (worker is null)
        {
            return;
        }

        OuvrirRechercheGlobale();
        RechercheGlobaleQuery = worker.NomComplet;
    }

    private void ChargerShow()
    {
        if (_repository is not null)
        {
            _context = _repository.ChargerShowContext(ShowId);
        }

        if (_context is null)
        {
            return;
        }

        Segments.Clear();
        WorkersDisponibles.Clear();
        StorylinesDisponibles.Clear();
        TitresDisponibles.Clear();

        foreach (var worker in _context.Workers)
        {
            WorkersDisponibles.Add(new ParticipantViewModel(worker.WorkerId, worker.NomComplet));
        }

        StorylinesDisponibles.Add(new StorylineOptionViewModel(string.Empty, "Aucune storyline"));
        foreach (var storyline in _context.Storylines)
        {
            StorylinesDisponibles.Add(new StorylineOptionViewModel(storyline.StorylineId, storyline.Nom));
        }

        TitresDisponibles.Add(new TitleOptionViewModel(string.Empty, "Aucun titre"));
        foreach (var titre in _context.Titres)
        {
            TitresDisponibles.Add(new TitleOptionViewModel(titre.TitreId, titre.Nom));
        }

        foreach (var segment in _context.Segments)
        {
            var participants = _context.Workers.Where(worker => segment.Participants.Contains(worker.WorkerId))
                .Select(worker => new ParticipantViewModel(worker.WorkerId, worker.NomComplet))
                .ToList();
            Segments.Add(new SegmentViewModel(
                segment.SegmentId,
                segment.TypeSegment,
                segment.DureeMinutes,
                segment.EstMainEvent,
                _segmentCatalog,
                participants,
                segment.StorylineId,
                segment.TitreId,
                segment.Intensite,
                segment.VainqueurId,
                segment.PerdantId,
                segment.Settings));
        }

        var selectionId = SegmentSelectionne?.SegmentId;
        SegmentSelectionne = selectionId is null
            ? Segments.FirstOrDefault()
            : Segments.FirstOrDefault(segment => segment.SegmentId == selectionId) ?? Segments.FirstOrDefault();

        MettreAJourAttributs();
        MettreAJourIndexRechercheGlobale();
        ChargerCalendrier();
        ChargerHistoriqueShow();
        MettreAJourAvertissements();
        InitialiserNouveauShow();
        ChargerDiffusion();
        MettreAJourAudienceHistorique();
    }

    private void ChargerDiffusion()
    {
        DealsTv.Clear();
        ReachMap.Clear();
        ContraintesDiffusion.Clear();

        if (_repository is null || _context is null)
        {
            return;
        }

        var deals = _repository.ChargerTvDeals(_context.Show.CompagnieId);
        foreach (var deal in deals)
        {
            DealsTv.Add(new TvDealViewModel(deal));
            if (!string.IsNullOrWhiteSpace(deal.Constraints))
            {
                ContraintesDiffusion.Add(deal.Constraints);
            }
        }

        ReachMap.Add(new ReachMapItemViewModel(_context.Compagnie.Region, _context.Compagnie.Reach, "Base locale"));
        if (deals.Count > 0)
        {
            var totalReach = Math.Clamp(
                _context.Compagnie.Reach + deals.Sum(deal => deal.ReachBonus),
                0,
                100);
            ReachMap.Add(new ReachMapItemViewModel("Couverture totale", totalReach, "Avec bonus deals TV"));
        }
    }

    private void MettreAJourAudienceHistorique()
    {
        AudienceHistorique.Clear();
        if (_repository is null || _context is null)
        {
            return;
        }

        var historique = _repository.ChargerAudienceHistorique(_context.Show.ShowId)
            .OrderByDescending(entry => entry.Week)
            .ToList();

        AudienceHistoryEntry? precedent = null;
        foreach (var entree in historique)
        {
            var item = new AudienceHistoryItemViewModel(entree, precedent?.Audience);
            AudienceHistorique.Add(item);
            precedent = entree;
        }
    }

    private void SelectionnerSegment(string? segmentId)
    {
        if (string.IsNullOrWhiteSpace(segmentId))
        {
            return;
        }

        SegmentSelectionne = Segments.FirstOrDefault(segment => segment.SegmentId == segmentId);
    }

    private void CorrigerDureeSegment(string? segmentId)
    {
        if (string.IsNullOrWhiteSpace(segmentId))
        {
            return;
        }

        var segment = Segments.FirstOrDefault(s => s.SegmentId == segmentId);
        if (segment is null)
        {
            return;
        }

        segment.DureeMinutes = Math.Max(1, segment.DureeMinutes);
        EnregistrerSegment(segment);
        SegmentSelectionne = segment;
    }

    private void CorrigerDureeTotale()
    {
        if (_context is null || Segments.Count == 0)
        {
            return;
        }

        var total = Segments.Sum(segment => segment.DureeMinutes);
        var excedent = total - _context.Show.DureeMinutes;
        if (excedent <= 0)
        {
            return;
        }

        var segmentCible = Segments.OrderByDescending(segment => segment.DureeMinutes).First();
        segmentCible.DureeMinutes = Math.Max(1, segmentCible.DureeMinutes - excedent);
        EnregistrerSegment(segmentCible);
        SegmentSelectionne = segmentCible;
    }

    private void MarquerMainEvent()
    {
        if (Segments.Count == 0)
        {
            return;
        }

        foreach (var segment in Segments)
        {
            segment.EstMainEvent = false;
        }

        var segmentCible = Segments.Last();
        segmentCible.EstMainEvent = true;
        EnregistrerSegment(segmentCible);
        SegmentSelectionne = segmentCible;
    }

    private void ChargerBibliotheque()
    {
        if (_repository is null)
        {
            return;
        }

        SegmentTemplates.Clear();
        MatchTypes.Clear();

        var matchTypes = _repository.ChargerMatchTypes();
        var matchMap = matchTypes.ToDictionary(type => type.MatchTypeId, type => type.Nom);
        foreach (var matchType in matchTypes)
        {
            var vm = new MatchTypeViewModel(matchType.MatchTypeId, matchType.Nom, matchType.Description, matchType.EstActif, matchType.Ordre);
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MatchTypeViewModel.EstActif))
                {
                    _repository.MettreAJourMatchType(vm.VersModele());
                }
            };
            MatchTypes.Add(vm);
        }

        foreach (var template in _repository.ChargerSegmentTemplates())
        {
            var label = _segmentLabels.TryGetValue(template.TypeSegment, out var libelle) ? libelle : template.TypeSegment;
            matchMap.TryGetValue(template.MatchTypeId ?? string.Empty, out var matchNom);
            SegmentTemplates.Add(new SegmentTemplateViewModel(
                template.TemplateId,
                template.Nom,
                template.TypeSegment,
                label,
                template.DureeMinutes,
                template.EstMainEvent,
                template.Intensite,
                template.MatchTypeId,
                matchNom));
        }

        TemplateSelectionnee ??= SegmentTemplates.FirstOrDefault();
    }

    private void ChargerInbox()
    {
        if (_repository is null)
        {
            return;
        }

        Inbox.Clear();

        foreach (var item in _repository.ChargerInbox())
        {
            Inbox.Add(new InboxItemViewModel(item));
        }
    }

    private void ChargerHistoriqueShow()
    {
        if (_repository is null || _context is null)
        {
            return;
        }

        HistoriqueShow.Clear();

        foreach (var entry in _repository.ChargerHistoriqueShow(_context.Show.ShowId))
        {
            HistoriqueShow.Add(new ShowHistoryViewModel(entry));
        }
    }

    private void ChargerParametresGeneration()
    {
        var options = _repository.ChargerParametresGeneration();
        YouthGenerationSelection = YouthGenerationModes.FirstOrDefault(mode => mode.Mode == options.YouthMode);
        WorldGenerationSelection = WorldGenerationModes.FirstOrDefault(mode => mode.Mode == options.WorldMode);
        SemainePivotAnnuelle = options.SemainePivotAnnuelle ?? 1;
    }

    private void ChargerYouth()
    {
        if (_repository is null)
        {
            return;
        }

        YouthStructures.Clear();

        foreach (var structure in _repository.ChargerYouthStructures())
        {
            YouthStructures.Add(new YouthStructureViewModel(
                structure.YouthId,
                structure.Nom,
                structure.Region,
                structure.Type,
                structure.BudgetAnnuel,
                structure.CapaciteMax,
                structure.NiveauEquipements,
                structure.QualiteCoaching,
                structure.Philosophie,
                structure.Actif,
                structure.TraineesActifs));
        }

        YouthStructureSelection ??= YouthStructures.FirstOrDefault();
        ChargerYouthDetails();
    }

    private void ChargerYouthDetails()
    {
        YouthTrainees.Clear();
        YouthPrograms.Clear();
        YouthStaffAssignments.Clear();

        if (_repository is null || YouthStructureSelection is null)
        {
            return;
        }

        foreach (var trainee in _repository.ChargerYouthTrainees(YouthStructureSelection.YouthId))
        {
            YouthTrainees.Add(new YouthTraineeViewModel(
                trainee.WorkerId,
                trainee.Nom,
                trainee.InRing,
                trainee.Entertainment,
                trainee.Story,
                trainee.Statut));
        }

        foreach (var programme in _repository.ChargerYouthPrograms(YouthStructureSelection.YouthId))
        {
            YouthPrograms.Add(new YouthProgramViewModel(
                programme.ProgramId,
                programme.Nom,
                programme.DureeSemaines,
                programme.Focus));
        }

        foreach (var staff in _repository.ChargerYouthStaffAssignments(YouthStructureSelection.YouthId))
        {
            YouthStaffAssignments.Add(new YouthStaffAssignmentViewModel(
                staff.AssignmentId,
                staff.WorkerId,
                staff.Nom,
                staff.Role,
                staff.SemaineDebut));
        }
    }

    public void ChangerBudgetYouth()
    {
        if (_repository is null || YouthStructureSelection is null)
        {
            return;
        }

        _repository.ChangerBudgetYouth(YouthStructureSelection.YouthId, YouthBudgetNouveau);
        YouthStructureSelection.BudgetAnnuel = YouthBudgetNouveau;
        YouthActionMessage = "Budget mis à jour.";
        ChargerYouth();
    }

    public void AffecterCoachYouth()
    {
        if (_repository is null || YouthStructureSelection is null)
        {
            YouthActionMessage = "Impossible d'affecter : aucune sauvegarde chargée.";
            return;
        }

        if (string.IsNullOrWhiteSpace(YouthCoachWorkerId) || string.IsNullOrWhiteSpace(YouthCoachRole))
        {
            return;
        }

        _repository.AffecterCoachYouth(YouthStructureSelection.YouthId, YouthCoachWorkerId, YouthCoachRole, _context?.Show.Semaine ?? 1);
        YouthCoachWorkerId = null;
        YouthCoachRole = null;
        YouthActionMessage = "Coach affecté.";
        ChargerYouthDetails();
    }

    public void DiplomerTrainee(string? workerId)
    {
        if (_repository is null || string.IsNullOrWhiteSpace(workerId))
        {
            if (_repository is null)
            {
                YouthActionMessage = "Impossible de diplômer : aucune sauvegarde chargée.";
            }
            return;
        }

        _repository.DiplomerTrainee(workerId, _context?.Show.Semaine ?? 1);
        YouthActionMessage = "Trainee diplômé.";
        ChargerYouthDetails();
    }

    private void MettreAJourAttributs()
    {
        AttributsPrincipaux.Clear();
        if (_context is null || _context.Workers.Count == 0)
        {
            return;
        }

        var worker = _context.Workers[0];
        AttributsPrincipaux.Add(new AttributeViewModel("In-Ring", worker.InRing, _tooltipHelper.Obtenir("attr.inring")));
        AttributsPrincipaux.Add(new AttributeViewModel("Entertainment", worker.Entertainment, _tooltipHelper.Obtenir("attr.entertainment")));
        AttributsPrincipaux.Add(new AttributeViewModel("Story", worker.Story, _tooltipHelper.Obtenir("attr.story")));
        AttributsPrincipaux.Add(new AttributeViewModel("Popularité", worker.Popularite, _tooltipHelper.Obtenir("attr.popularite")));
        AttributsPrincipaux.Add(new AttributeViewModel("Fatigue", worker.Fatigue, _tooltipHelper.Obtenir("attr.fatigue")));
        AttributsPrincipaux.Add(new AttributeViewModel("Momentum", worker.Momentum, _tooltipHelper.Obtenir("attr.momentum")));
    }

    private void MettreAJourTableItems()
    {
        TableItems.Clear();
        if (_context is null)
        {
            return;
        }

        TableItems.Add(new TableViewItemViewModel(
            _context.Compagnie.CompagnieId,
            _context.Compagnie.Nom,
            "Compagnie",
            _context.Compagnie.Region,
            "Promotion",
            "Actif",
            _context.Compagnie.Prestige,
            0,
            _context.Compagnie.AudienceMoyenne,
            $"Prestige {_context.Compagnie.Prestige}",
            new[] { _context.Compagnie.Region }));

        foreach (var worker in _context.Workers)
        {
            var statut = string.IsNullOrWhiteSpace(worker.Blessure) ? "Actif" : "Blessé";
            var note = (int)Math.Round((worker.InRing + worker.Entertainment + worker.Story) / 3.0);
            TableItems.Add(new TableViewItemViewModel(
                worker.WorkerId,
                worker.NomComplet,
                "Worker",
                _context.Compagnie.Nom,
                worker.RoleTv,
                statut,
                worker.Popularite,
                worker.Momentum,
                note,
                $"{worker.RoleTv} • Popularité {worker.Popularite}",
                new[] { worker.RoleTv }));
        }

        foreach (var titre in _context.Titres)
        {
            var detenteur = _context.Workers.FirstOrDefault(worker => worker.WorkerId == titre.DetenteurId);
            var detenteurNom = detenteur?.NomComplet ?? "Vacant";
            var statut = detenteur is null ? "Vacant" : "Défendu";
            TableItems.Add(new TableViewItemViewModel(
                titre.TitreId,
                titre.Nom,
                "Titre",
                _context.Compagnie.Nom,
                detenteurNom,
                statut,
                titre.Prestige,
                detenteur?.Momentum ?? 0,
                titre.Prestige,
                $"Détenteur {detenteurNom}",
                new[] { statut }));
        }

        foreach (var storyline in _context.Storylines)
        {
            var participants = _context.Workers
                .Where(worker => storyline.Participants.Any(participant => participant.WorkerId == worker.WorkerId))
                .ToList();
            var nomsParticipants = participants.Select(worker => worker.NomComplet).ToList();
            var momentum = participants.Count == 0 ? 0 : (int)Math.Round(participants.Average(worker => worker.Momentum));
            var phase = ObtenirLibellePhase(storyline.Phase);
            var statut = ObtenirLibelleStatut(storyline.Status);
            TableItems.Add(new TableViewItemViewModel(
                storyline.StorylineId,
                storyline.Nom,
                "Storyline",
                _context.Compagnie.Nom,
                string.Join(", ", nomsParticipants),
                $"{phase} • {statut}",
                storyline.Heat,
                momentum,
                storyline.Heat,
                $"Phase {phase} • {statut}",
                new[] { phase, statut }));
        }

        MettreAJourResumeTable();
    }

    private static string ObtenirLibellePhase(StorylinePhase phase)
        => phase switch
        {
            StorylinePhase.Setup => "Lancement",
            StorylinePhase.Rising => "Montée",
            StorylinePhase.Climax => "Climax",
            StorylinePhase.Fallout => "Retombées",
            _ => phase.ToString()
        };

    private static string ObtenirLibelleStatut(StorylineStatus status)
        => status switch
        {
            StorylineStatus.Active => "En cours",
            StorylineStatus.Suspended => "Suspendue",
            StorylineStatus.Completed => "Terminée",
            _ => status.ToString()
        };

    private bool FiltrerTableItems(object? item)
    {
        if (item is not TableViewItemViewModel tableItem)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(TableRecherche))
        {
            var recherche = TableRecherche.Trim();
            if (!tableItem.Nom.Contains(recherche, StringComparison.OrdinalIgnoreCase) &&
                !tableItem.Role.Contains(recherche, StringComparison.OrdinalIgnoreCase) &&
                !tableItem.Statut.Contains(recherche, StringComparison.OrdinalIgnoreCase) &&
                !tableItem.Type.Contains(recherche, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (TableSelectedTypeFilter?.Id is not null && TableSelectedTypeFilter.Id != "tous")
        {
            var itemTypeId = tableItem.Type.ToLowerInvariant() switch
            {
                "worker" => "worker",
                "compagnie" => "company",
                "titre" => "title",
                "storyline" => "storyline",
                _ => tableItem.Type.ToLowerInvariant()
            };

            if (!itemTypeId.Equals(TableSelectedTypeFilter.Id, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (TableSelectedStatusFilter?.Id is not null && TableSelectedStatusFilter.Id != "tous")
        {
            var statutLower = tableItem.Statut.ToLowerInvariant();
            var statutId = statutLower switch
            {
                _ when statutLower.Contains("suspendue") => "suspendue",
                _ when statutLower.Contains("terminée") => "terminee",
                _ when statutLower.Contains("en cours") => "en-cours",
                _ when statutLower == "actif" => "actif",
                _ when statutLower == "en repos" => "repos",
                _ when statutLower == "blessé" => "blesse",
                _ when statutLower == "vacant" => "vacant",
                _ when statutLower == "défendu" => "en-cours",
                _ => statutLower
            };

            if (!statutId.Equals(TableSelectedStatusFilter.Id, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private void AppliquerFiltreTable()
    {
        TableItemsView.Refresh();
        MettreAJourResumeTable();
    }

    private void MettreAJourResumeTable()
    {
        TableResultatsResume = $"Résultats : {TableItemsView.Count} / {TableItems.Count}";
    }

    public void MonterColonne(TableColumnOrderViewModel colonne)
    {
        var index = TableColumns.IndexOf(colonne);
        if (index <= 0)
        {
            return;
        }

        TableColumns.Move(index, index - 1);
        SauvegarderPreferencesTable();
    }

    public void DescendreColonne(TableColumnOrderViewModel colonne)
    {
        var index = TableColumns.IndexOf(colonne);
        if (index < 0 || index >= TableColumns.Count - 1)
        {
            return;
        }

        TableColumns.Move(index, index + 1);
        SauvegarderPreferencesTable();
    }

    public void MettreAJourTriTable(string colonneId)
    {
        var existant = _tableSortSettings.FirstOrDefault(sort => sort.ColumnId.Equals(colonneId, StringComparison.OrdinalIgnoreCase));
        if (existant is null)
        {
            _tableSortSettings.Add(new TableSortSetting(colonneId, TableSortDirection.Ascending));
        }
        else if (existant.Direction == TableSortDirection.Ascending)
        {
            _tableSortSettings[_tableSortSettings.IndexOf(existant)] = existant with { Direction = TableSortDirection.Descending };
        }
        else
        {
            _tableSortSettings.Remove(existant);
        }

        AppliquerTriTable();
        SauvegarderPreferencesTable();
    }

    public void ReinitialiserTriTable()
    {
        if (_tableSortSettings.Count == 0)
        {
            return;
        }

        _tableSortSettings.Clear();
        AppliquerTriTable();
        SauvegarderPreferencesTable();
    }

    private void AppliquerTriTable()
    {
        TableItemsView.Refresh();

        TableTriResume = _tableSortSettings.Count == 0
            ? "Tri : aucun"
            : $"Tri : {string.Join(" · ", _tableSortSettings.Select(tri => $"{tri.ColumnId} {(tri.Direction == TableSortDirection.Ascending ? "↑" : "↓")}"))}";
    }

    private void ChargerPreferencesTable()
    {
        if (_repository is null)
        {
            MettreAJourResumeTable();
            return;
        }

        _suspendTablePreferences = true;
        var settings = _repository.ChargerTableUiSettings();
        TableRecherche = settings.Recherche;
        TableSelectedTypeFilter = TableTypeFilters.FirstOrDefault(filter => filter.Id.Equals(settings.FiltreType, StringComparison.OrdinalIgnoreCase))
            ?? TableTypeFilters[0];
        TableSelectedStatusFilter = TableStatusFilters.FirstOrDefault(filter => filter.Id.Equals(settings.FiltreStatut, StringComparison.OrdinalIgnoreCase))
            ?? TableStatusFilters[0];

        AppliquerColonnesVisibles(settings.ColonnesVisibles);
        AppliquerOrdreColonnes(settings.ColonnesOrdre);
        _tableSortSettings.Clear();
        _tableSortSettings.AddRange(settings.Tris);
        AppliquerTriTable();
        _suspendTablePreferences = false;
    }

    private void AppliquerColonnesVisibles(IReadOnlyDictionary<string, bool> colonnesVisibles)
    {
        if (colonnesVisibles.Count == 0)
        {
            return;
        }

        if (colonnesVisibles.TryGetValue("Type", out var afficherType))
        {
            TableConfiguration.AfficherType = afficherType;
        }

        if (colonnesVisibles.TryGetValue("Compagnie", out var afficherCompagnie))
        {
            TableConfiguration.AfficherCompagnie = afficherCompagnie;
        }

        if (colonnesVisibles.TryGetValue("Role", out var afficherRole))
        {
            TableConfiguration.AfficherRole = afficherRole;
        }

        if (colonnesVisibles.TryGetValue("Statut", out var afficherStatut))
        {
            TableConfiguration.AfficherStatut = afficherStatut;
        }

        if (colonnesVisibles.TryGetValue("Popularite", out var afficherPopularite))
        {
            TableConfiguration.AfficherPopularite = afficherPopularite;
        }

        if (colonnesVisibles.TryGetValue("Momentum", out var afficherMomentum))
        {
            TableConfiguration.AfficherMomentum = afficherMomentum;
        }

        if (colonnesVisibles.TryGetValue("Note", out var afficherNote))
        {
            TableConfiguration.AfficherNote = afficherNote;
        }
    }

    private void AppliquerOrdreColonnes(IReadOnlyList<string> ordre)
    {
        if (ordre.Count == 0)
        {
            return;
        }

        var mapping = TableColumns.ToDictionary(colonne => colonne.Id, StringComparer.OrdinalIgnoreCase);
        var nouvelOrdre = ordre
            .Select(id => mapping.TryGetValue(id, out var item) ? item : null)
            .Where(item => item is not null)
            .Cast<TableColumnOrderViewModel>()
            .ToList();
        foreach (var colonne in TableColumns)
        {
            if (!nouvelOrdre.Contains(colonne))
            {
                nouvelOrdre.Add(colonne);
            }
        }

        TableColumns.Clear();
        foreach (var colonne in nouvelOrdre)
        {
            TableColumns.Add(colonne);
        }
    }

    private void SauvegarderPreferencesTable()
    {
        if (_repository is null || _suspendTablePreferences || TableSelectedTypeFilter is null || TableSelectedStatusFilter is null)
        {
            return;
        }

        var colonnesVisibles = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            ["Type"] = TableConfiguration.AfficherType,
            ["Compagnie"] = TableConfiguration.AfficherCompagnie,
            ["Role"] = TableConfiguration.AfficherRole,
            ["Statut"] = TableConfiguration.AfficherStatut,
            ["Popularite"] = TableConfiguration.AfficherPopularite,
            ["Momentum"] = TableConfiguration.AfficherMomentum,
            ["Note"] = TableConfiguration.AfficherNote
        };
        var settings = new TableUiSettings(
            TableRecherche,
            TableSelectedTypeFilter.Id,
            TableSelectedStatusFilter.Id,
            colonnesVisibles,
            TableColumns.Select(colonne => colonne.Id).ToList(),
            _tableSortSettings.ToList());
        _repository.SauvegarderTableUiSettings(settings);
    }

    private void MettreAJourIndexRechercheGlobale()
    {
        _rechercheGlobaleIndex.Clear();
        if (_context is null)
        {
            return;
        }

        foreach (var worker in _context.Workers)
        {
            _rechercheGlobaleIndex.Add(new GlobalSearchResultViewModel(
                "Worker",
                worker.NomComplet,
                $"{worker.RoleTv} • Popularité {worker.Popularite}",
                string.IsNullOrWhiteSpace(worker.Blessure) ? "Actif" : "Blessé"));
        }

        _rechercheGlobaleIndex.Add(new GlobalSearchResultViewModel(
            "Compagnie",
            _context.Compagnie.Nom,
            $"{_context.Compagnie.Region} • Prestige {_context.Compagnie.Prestige}",
            "Promotion"));

        foreach (var titre in _context.Titres)
        {
            var detenteur = _context.Workers.FirstOrDefault(worker => worker.WorkerId == titre.DetenteurId)?.NomComplet ?? "Vacant";
            _rechercheGlobaleIndex.Add(new GlobalSearchResultViewModel(
                "Titre",
                titre.Nom,
                $"Détenteur {detenteur}",
                $"Prestige {titre.Prestige}"));
        }

        foreach (var storyline in _context.Storylines)
        {
            var participants = _context.Workers
                .Where(worker => storyline.Participants.Any(participant => participant.WorkerId == worker.WorkerId))
                .Select(worker => worker.NomComplet)
                .Take(3);
            var phase = ObtenirLibellePhase(storyline.Phase);
            _rechercheGlobaleIndex.Add(new GlobalSearchResultViewModel(
                "Storyline",
                storyline.Nom,
                $"Participants {string.Join(", ", participants)} • Phase {phase}",
                $"Heat {storyline.Heat}"));
        }
    }

    private void MettreAJourRechercheGlobale()
    {
        RechercheGlobaleResultats.Clear();
        var recherche = RechercheGlobaleQuery?.Trim();

        var resultats = string.IsNullOrWhiteSpace(recherche)
            ? _rechercheGlobaleIndex
            : _rechercheGlobaleIndex.Where(resultat =>
                resultat.Titre.Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
                resultat.SousTitre.Contains(recherche, StringComparison.OrdinalIgnoreCase));

        var liste = resultats.Take(12).ToList();
        foreach (var resultat in liste)
        {
            RechercheGlobaleResultats.Add(resultat);
        }

        RechercheGlobaleAucunResultat = liste.Count == 0;
    }

    private void MettreAJourAnalyseShow(ShowSimulationResult resultat)
    {
        PourquoiNote.Clear();
        Conseils.Clear();

        var facteurs = resultat.RapportShow.Segments
            .SelectMany(segment => segment.Facteurs)
            .OrderByDescending(facteur => Math.Abs(facteur.Impact))
            .Take(5)
            .Select(facteur => $"{facteur.Libelle} {facteur.Impact:+#;-#;0}")
            .ToList();

        foreach (var facteur in facteurs)
        {
            PourquoiNote.Add(facteur);
        }

        var conseils = GenererConseils(resultat);
        foreach (var conseil in conseils)
        {
            Conseils.Add(conseil);
        }

        DetailsSimulation =
            "La note combine performance in-ring, divertissement, heat du public et rythme global. " +
            "Les bonus/malus de chimie, d'intensité et de pacing sont appliqués avant les impacts.";
    }

    private void ChargerImpactsInitial()
    {
        ImpactPages.Clear();
        foreach (var page in _impactPages.Values)
        {
            var pourquoi = page.Sections.FirstOrDefault(section => section.Titre.StartsWith("Pourquoi", StringComparison.OrdinalIgnoreCase))?.Contenu ?? string.Empty;
            var comment = page.Sections.FirstOrDefault(section => section.Titre.StartsWith("Comment", StringComparison.OrdinalIgnoreCase))?.Contenu ?? string.Empty;
            ImpactPages.Add(new ImpactPageViewModel(page.Id, page.Titre, pourquoi, comment));
        }

        ImpactSelectionnee = ImpactPages.FirstOrDefault();
    }

    private void MettreAJourImpacts(ShowSimulationResult resultat)
    {
        var delta = resultat.Delta;
        if (delta is null)
        {
            return;
        }

        foreach (var page in ImpactPages)
        {
            page.Deltas.Clear();
        }

        AjouterDeltas("impacts.popularite",
            delta.PopulariteCompagnieDelta.Select(kv => $"Compagnie {kv.Value:+#;-#;0}"),
            delta.PopulariteWorkersDelta.Select(kv => $"{NommerWorker(kv.Key)} {kv.Value:+#;-#;0}"));

        AjouterDeltas("impacts.finances",
            delta.Finances.Select(tx => $"{tx.Libelle} {tx.Montant:+#;-#;0}"));

        AjouterDeltas("impacts.fatigue",
            delta.FatigueDelta.Select(kv => $"{NommerWorker(kv.Key)} +{kv.Value}"),
            delta.Blessures.Select(kv => $"{NommerWorker(kv.Key)} : {kv.Value}"));

        AjouterDeltas("impacts.storylines",
            delta.StorylineHeatDelta.Select(kv => $"{NommerStoryline(kv.Key)} {kv.Value:+#;-#;0}"));

        AjouterDeltas("impacts.titres",
            delta.TitrePrestigeDelta.Select(kv => $"{NommerTitre(kv.Key)} {kv.Value:+#;-#;0}"));

        foreach (var page in ImpactPages)
        {
            if (page.Deltas.Count == 0)
            {
                page.Deltas.Add("Aucun changement récent.");
            }
        }
    }

    private void AjouterDeltas(string pageId, params IEnumerable<string>[] lignes)
    {
        var page = ImpactPages.FirstOrDefault(p => p.Id.Equals(pageId, StringComparison.OrdinalIgnoreCase));
        if (page is null)
        {
            return;
        }

        foreach (var ligne in lignes.SelectMany(ligne => ligne))
        {
            page.Deltas.Add(ligne);
        }
    }

    private IReadOnlyList<string> GenererConseils(ShowSimulationResult resultat)
    {
        var conseils = new List<string>();
        if (resultat.RapportShow.NoteGlobale < 60)
        {
            conseils.Add("Renforcez le main event et limitez les segments faibles.");
        }

        if (resultat.RapportShow.Segments.Any(segment => segment.PacingPenalty < 0))
        {
            conseils.Add("Alternez match et promo pour éviter les pénalités de rythme.");
        }

        if (resultat.RapportShow.Segments.Any(segment => segment.Impact.FatigueDelta.Values.Any(delta => delta > 6)))
        {
            conseils.Add("Réduisez la durée des matchs les plus fatigants.");
        }

        if (resultat.RapportShow.Segments.All(segment => segment.Impact.StorylineHeatDelta.Count == 0))
        {
            conseils.Add("Liez davantage de segments aux storylines actives.");
        }

        if (conseils.Count == 0)
        {
            conseils.Add("Continuez sur ce rythme et consolidez vos points forts.");
        }

        return conseils;
    }

    private CodexViewModel ChargerCodex()
    {
        var glossaire = _helpProvider.ChargerGlossaire();
        var systemes = _helpProvider.ChargerSystemes();
        var tutoriel = _helpProvider.ChargerTutorial();

        var articles = new List<CodexArticleViewModel>();
        articles.AddRange(glossaire.Entrees.Select(entree =>
            new CodexArticleViewModel(entree.Id, entree.Terme, "Glossaire", entree.Definition, entree.Liens)));

        articles.AddRange(systemes.Systemes.Select(systeme =>
            new CodexArticleViewModel(systeme.Id, systeme.Titre, "Système",
                $"{systeme.Resume}\n- {string.Join("\n- ", systeme.Points)}", systeme.Liens)));

        foreach (var etape in tutoriel.Etapes)
        {
            articles.Add(new CodexArticleViewModel(etape.Id, etape.Titre, "Guide",
                $"{etape.Contenu}\nObjectif : {etape.Objectif}", Array.Empty<string>()));
        }

        return new CodexViewModel(articles);
    }

    private IReadOnlyDictionary<string, HelpPageEntry> ChargerPages()
    {
        var spec = _helpProvider.ChargerPages();
        return spec.Pages.ToDictionary(page => page.Id, page => page, StringComparer.OrdinalIgnoreCase);
    }

    private void InitialiserSegmentTypes()
    {
        SegmentTypes.Clear();
        foreach (var type in _segmentCatalog.Labels)
        {
            SegmentTypes.Add(new SegmentTypeOptionViewModel(type.Key, type.Value));
        }
    }

    private void InitialiserConsignesBooking()
    {
        ConsignesBooking.Clear();
        ConsignesBooking.Add("Durée totale des segments ≤ durée du show.");
        ConsignesBooking.Add("Un main event est requis.");
        ConsignesBooking.Add("Maximum 2 promos par show.");
        ConsignesBooking.Add("Évitez d'utiliser un même participant sur trop de segments.");
    }

    private void InitialiserBibliotheque()
    {
        if (_repository is null)
        {
            System.Diagnostics.Debug.WriteLine("InitialiserBibliotheque abandonnée : Repository est null.");
            Console.WriteLine("AVERTISSEMENT: InitialiserBibliotheque ne peut pas s'exécuter car la base de données n'est pas chargée.");
            return;
        }

        var matchTypes = _repository.ChargerMatchTypes().ToList();
        if (matchTypes.Count == 0)
        {
            var matchTypeDefinitions = ChargerMatchTypesSpec().ToList();
            if (matchTypeDefinitions.Count > 0)
            {
                _repository.EnregistrerMatchTypes(matchTypeDefinitions);
                matchTypes = _repository.ChargerMatchTypes().ToList();
            }
        }

        MatchTypes.Clear();
        foreach (var matchType in matchTypes)
        {
            MatchTypes.Add(new MatchTypeViewModel(matchType.MatchTypeId, matchType.Nom, matchType.Description, matchType.EstActif, matchType.Ordre));
        }

        var templates = _repository.ChargerSegmentTemplates().ToList();
        if (templates.Count == 0)
        {
            var templateDefinitions = ChargerSegmentTemplatesSpec().ToList();
            if (templateDefinitions.Count > 0)
            {
                _repository.EnregistrerSegmentTemplates(templateDefinitions);
                templates = _repository.ChargerSegmentTemplates().ToList();
            }
        }

        SegmentTemplates.Clear();
        var matchTypeLabels = matchTypes.ToDictionary(type => type.MatchTypeId, type => type.Nom);
        foreach (var template in templates)
        {
            var typeLabel = _segmentLabels.TryGetValue(template.TypeSegment, out var libelle) ? libelle : template.TypeSegment;
            matchTypeLabels.TryGetValue(template.MatchTypeId ?? string.Empty, out var matchNom);
            SegmentTemplates.Add(new SegmentTemplateViewModel(
                template.TemplateId,
                template.Nom,
                template.TypeSegment,
                typeLabel,
                template.DureeMinutes,
                template.EstMainEvent,
                template.Intensite,
                template.MatchTypeId,
                matchNom));
        }

        TemplateSelectionnee = SegmentTemplates.FirstOrDefault();
    }

    private void InitialiserNouveauShow()
    {
        if (_context is null)
        {
            return;
        }

        NouveauShowNom = $"{_context.Show.Nom} spécial";
        NouveauShowSemaine = _context.Show.Semaine + 1;
        NouveauShowDuree = _context.Show.DureeMinutes;
        NouveauShowLieu = _context.Show.Lieu;
        NouveauShowDiffusion = _context.Show.Diffusion;
        NouveauSegmentTypeId = SegmentTypes.FirstOrDefault()?.Id;
        NouveauSegmentStorylineId = StorylinesDisponibles.FirstOrDefault()?.Id;
    }

    private IReadOnlyDictionary<string, string> ObtenirSettingsParDefaut(string typeSegment)
    {
        var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var consigneId in _segmentCatalog.ObtenirConsignesPourType(typeSegment))
        {
            var option = _segmentCatalog.ObtenirOptionsConsigne(consigneId).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(option))
            {
                settings[consigneId] = option;
            }
        }

        return settings;
    }

    private static int CalculerIntensite(IReadOnlyDictionary<string, string> settings, int valeurParDefaut)
    {
        if (!settings.TryGetValue("intensite", out var intensite))
        {
            return valeurParDefaut;
        }

        return intensite.ToUpperInvariant() switch
        {
            "FAIBLE" => 35,
            "MOYENNE" => 60,
            "FORTE" => 85,
            _ => valeurParDefaut
        };
    }

    private void ChargerCalendrier()
    {
        if (_repository is null || _context is null)
        {
            return;
        }

        ShowsAVenir.Clear();
        foreach (var show in _repository.ChargerShowsAVenir(_context.Show.CompagnieId, _context.Show.Semaine))
        {
            ShowsAVenir.Add(new ShowCalendarItemViewModel(
                show.ShowId,
                show.Nom,
                show.Semaine,
                show.DureeMinutes,
                show.Lieu,
                show.Diffusion));
        }
    }

    private void MettreAJourAvertissements()
    {
        if (_context is null)
        {
            return;
        }

        var segments = Segments.Select(segment => new SegmentSimulationContext(
            segment.SegmentId,
            segment.TypeSegment,
            segment.Participants.Select(p => p.WorkerId).ToList(),
            segment.DureeMinutes,
            segment.EstMainEvent,
            segment.StorylineId,
            segment.TitreId,
            segment.Intensite,
            segment.VainqueurId,
            segment.PerdantId,
            _context.Workers.Where(worker => segment.Participants.Any(p => p.WorkerId == worker.WorkerId)).ToList()))
            .ToList();

        var etat = _context.Workers.ToDictionary(
            worker => worker.WorkerId,
            worker => new WorkerHealth(worker.Fatigue, worker.Blessure));

        var plan = new BookingPlan(_context.Show.ShowId, segments, _context.Show.DureeMinutes, etat);
        var validation = _validator.ValiderBooking(plan);
        MettreAJourValidation(validation);
    }

    private void MettreAJourValidation(ValidationResult validation)
    {
        ValidationIssues.Clear();
        foreach (var issue in validation.Issues)
        {
            var action = issue.Code switch
            {
                "booking.empty" => "Ajouter",
                "booking.duration.exceed" => "Réduire",
                "booking.main-event.missing" => "Marquer",
                "segment.duration.invalid" => "Corriger",
                "segment.participants.empty" => "Sélectionner",
                "segment.participant.injured" => "Voir",
                "segment.participant.fatigue" => "Voir",
                _ => "Corriger"
            };

            ValidationIssues.Add(new BookingIssueViewModel(
                issue.Code,
                issue.Message,
                issue.Severite,
                issue.SegmentId,
                action));
        }

        ValidationErreurs = validation.Erreurs.Count == 0 ? null : string.Join("\n", validation.Erreurs);
        ValidationAvertissements = validation.Avertissements.Count == 0 ? null : string.Join("\n", validation.Avertissements);

        var messagesParSegment = validation.Issues
            .Where(issue => !string.IsNullOrWhiteSpace(issue.SegmentId))
            .GroupBy(issue => issue.SegmentId!)
            .ToDictionary(
                groupe => groupe.Key,
                groupe => string.Join(" ", groupe.Select(issue => issue.Message)));

        foreach (var segment in Segments)
        {
            segment.Avertissements = messagesParSegment.TryGetValue(segment.SegmentId, out var message)
                ? message
                : null;
        }
    }

    private void MettreAJourRecapFm(ShowSimulationResult resultat)
    {
        RecapFm.Clear();
        var rapport = resultat.RapportShow;
        var delta = resultat.Delta;
        var totalFinances = rapport.Billetterie + rapport.Merch + rapport.Tv;
        var workerNames = ConstruireNomsWorkers();

        RecapFm.Add($"Note show {rapport.NoteGlobale} • Audience {rapport.Audience}");
        RecapFm.Add($"Finances • Billetterie {rapport.Billetterie:C} • Merch {rapport.Merch:C} • TV {rapport.Tv:C} • Total {totalFinances:C}");

        if (delta is not null)
        {
            var popCompagnie = delta.PopulariteCompagnieDelta.Values.Sum();
            RecapFm.Add($"Δ Popularité • Compagnie {popCompagnie:+#;-#;0} • Workers {FormatterDelta(delta.PopulariteWorkersDelta, NommerWorker)}");
            RecapFm.Add($"Δ Momentum • {FormatterDelta(delta.MomentumDelta, NommerWorker)}");
            RecapFm.Add($"Δ Heat • {FormatterDelta(delta.StorylineHeatDelta, NommerStoryline)}");
            RecapFm.Add($"Δ Fatigue • {FormatterDelta(delta.FatigueDelta, NommerWorker)}");
        }

        foreach (var segment in rapport.Segments)
        {
            var libelle = _segmentCatalog.Labels.TryGetValue(segment.TypeSegment, out var label) ? label : segment.TypeSegment;
            var breakdown = string.Join(" | ", segment.Facteurs.Select(facteur => $"{facteur.Libelle} {facteur.Impact:+#;-#;0}"));
            var impacts = new SegmentResultViewModel(segment, workerNames, libelle).Impacts;
            RecapFm.Add($"{libelle} • Note {segment.Note} • {breakdown} • {impacts}");
        }
    }

    private static SegmentTypeCatalog ChargerSegmentTypes()
    {
        var reader = new SpecsReader();
        var chemins = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "specs", "booking", "segment-types.fr.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "specs", "booking", "segment-types.fr.json")
        };

        var chemin = chemins.FirstOrDefault(File.Exists);
        if (chemin is null)
        {
            return new SegmentTypeCatalog(
                new Dictionary<string, string>(),
                new Dictionary<string, IReadOnlyList<string>>(),
                new Dictionary<string, IReadOnlyList<string>>(),
                new Dictionary<string, string>());
        }

        var spec = reader.Charger<SegmentTypesSpec>(chemin);
        var labels = spec.Types.ToDictionary(type => type.Id, type => type.Libelle);
        var consignesParType = spec.Types
            .Where(type => type.Consignes is not null)
            .ToDictionary(type => type.Id, type => (IReadOnlyList<string>)type.Consignes!);

        var consigneOptions = spec.Consignes ?? new Dictionary<string, IReadOnlyList<string>>();
        var consigneLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["intensite"] = "Intensité",
            ["storyHeavy"] = "Storyline importante",
            ["finish"] = "Finish",
            ["risqueBotch"] = "Risque de botch",
            ["ton"] = "Ton",
            ["typeMatch"] = "Type de match"
        };

        return new SegmentTypeCatalog(labels, consignesParType, consigneOptions, consigneLabels);
    }

    private static IReadOnlyList<MatchTypeDefinition> ChargerMatchTypesSpec()
    {
        var reader = new SpecsReader();
        var chemins = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "specs", "library", "match-types.fr.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "specs", "library", "match-types.fr.json")
        };

        var chemin = chemins.FirstOrDefault(File.Exists);
        if (chemin is null)
        {
            return Array.Empty<MatchTypeDefinition>();
        }

        var spec = reader.Charger<MatchTypesSpec>(chemin);
        return spec.Types.Select(type => new MatchTypeDefinition(
            type.Id,
            type.Libelle,
            type.Description,
            type.Participants,
            type.DureeParDefaut)).ToList();
    }

    private static IReadOnlyList<SegmentTemplateDefinition> ChargerSegmentTemplatesSpec()
    {
        var reader = new SpecsReader();
        var chemins = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "specs", "library", "segments.fr.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "specs", "library", "segments.fr.json")
        };

        var chemin = chemins.FirstOrDefault(File.Exists);
        if (chemin is null)
        {
            return Array.Empty<SegmentTemplateDefinition>();
        }

        var spec = reader.Charger<LibrarySegmentsSpec>(chemin);
        return spec.Templates.Select(template => new SegmentTemplateDefinition(
            template.Id,
            template.Libelle,
            template.Description,
            template.Segments.Select(segment => new SegmentTemplateSegmentDefinition(
                segment.TypeSegment,
                segment.Duree,
                segment.MainEvent,
                segment.AutoParticipants,
                segment.MatchTypeId)).ToList())).ToList();
    }

    private IReadOnlyDictionary<string, string> ConstruireNomsWorkers()
    {
        if (_context is null)
        {
            return new Dictionary<string, string>();
        }

        return _context.Workers.ToDictionary(worker => worker.WorkerId, worker => worker.NomComplet);
    }

    private string NommerWorker(string workerId)
    {
        if (_context is null)
        {
            return workerId;
        }

        return _context.Workers.FirstOrDefault(worker => worker.WorkerId == workerId)?.NomComplet ?? workerId;
    }

    private string NommerStoryline(string storylineId)
    {
        if (_context is null)
        {
            return storylineId;
        }

        return _context.Storylines.FirstOrDefault(storyline => storyline.StorylineId == storylineId)?.Nom ?? storylineId;
    }

    private string NommerTitre(string titreId)
    {
        if (_context is null)
        {
            return titreId;
        }

        return _context.Titres.FirstOrDefault(titre => titre.TitreId == titreId)?.Nom ?? titreId;
    }

    private static string FormatterDelta(IReadOnlyDictionary<string, int> deltas, Func<string, string> nommer)
    {
        if (deltas.Count == 0)
        {
            return "—";
        }

        return string.Join(", ", deltas.Select(kv => $"{nommer(kv.Key)} {kv.Value:+#;-#;0}"));
    }
}

public sealed record YouthGenerationOptionViewModel(string Libelle, YouthGenerationMode Mode);

public sealed record WorldGenerationOptionViewModel(string Libelle, WorldGenerationMode Mode);

public sealed record TitleOptionViewModel(string TitreId, string Nom);
