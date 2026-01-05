using System.Collections.ObjectModel;
using Avalonia.Collections;
using ReactiveUI;
using System.Reactive;
using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Services;
using RingGeneral.Core.Simulation;
using RingGeneral.Core.Validation;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;
using RingGeneral.UI.Services;

namespace RingGeneral.UI.ViewModels;

public sealed class GameSessionViewModel : ViewModelBase
{
    private const string ShowId = "SHOW-001";
    private GameRepository? _repository;
    private readonly BookingValidator _validator = new();
    private readonly IReadOnlyDictionary<string, string> _segmentLabels;
    private readonly HelpContentProvider _helpProvider = new();
    private readonly IReadOnlyDictionary<string, HelpPageEntry> _helpPages;
    private readonly IReadOnlyDictionary<string, HelpPageEntry> _impactPages;
    private readonly TooltipHelper _tooltipHelper;
    private readonly StorylineService _storylineService = new();
    private ShowContext? _context;
    private readonly List<GlobalSearchResultViewModel> _rechercheGlobaleIndex = new();

    public GameSessionViewModel(string? cheminDb = null)
    {
        var cheminFinal = string.IsNullOrWhiteSpace(cheminDb)
            ? Path.Combine(Directory.GetCurrentDirectory(), "ringgeneral.db")
            : cheminDb;
        var factory = new SqliteConnectionFactory($"Data Source={cheminFinal}");
        _repository = new GameRepository(factory);
        _repository.Initialiser();
        _segmentLabels = ChargerSegmentTypes();
        _tooltipHelper = new TooltipHelper(_helpProvider);
        _helpPages = ChargerPages();
        _impactPages = _helpPages
            .Where(page => page.Key.StartsWith("impacts.", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        Segments = new ObservableCollection<SegmentViewModel>();
        Resultats = new ObservableCollection<SegmentResultViewModel>();
        Inbox = new ObservableCollection<InboxItemViewModel>();
        AttributsPrincipaux = new ObservableCollection<AttributeViewModel>();
        PourquoiNote = new ObservableCollection<string>();
        Conseils = new ObservableCollection<string>();
        ImpactPages = new ObservableCollection<ImpactPageViewModel>();
        ShowsAVenir = new ObservableCollection<ShowCalendarItemViewModel>();
        SegmentTypes = new ObservableCollection<SegmentTypeOptionViewModel>();
        WorkersDisponibles = new ObservableCollection<ParticipantViewModel>();
        ConsignesBooking = new ObservableCollection<string>();
        RecapFm = new ObservableCollection<string>();
        NouveauSegmentParticipants = new ObservableCollection<ParticipantViewModel>();
        Storylines = new ObservableCollection<StorylineListItemViewModel>();
        StorylineOptions = new ObservableCollection<StorylineOptionViewModel>();
        StorylineParticipantsEdition = new ObservableCollection<StorylineParticipantViewModel>();
        AidePanel = new HelpPanelViewModel();
        Codex = ChargerCodex();
        TableItems = new ObservableCollection<TableViewItemViewModel>();
        TableItemsView = new DataGridCollectionView(TableItems)
        {
            Filter = FiltrerTableItems
        };
        TableConfiguration = new TableViewConfigurationViewModel();
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
        ChargerShow();
        ChargerInbox();
        ChargerHistoriqueShow();
        ChargerImpactsInitial();
        InitialiserNouveauShow();
        ChargerYouth();
    }

    public ObservableCollection<SegmentViewModel> Segments { get; }
    public ObservableCollection<SegmentResultViewModel> Resultats { get; }
    public ObservableCollection<InboxItemViewModel> Inbox { get; }
    public ObservableCollection<AttributeViewModel> AttributsPrincipaux { get; }
    public ObservableCollection<string> PourquoiNote { get; }
    public ObservableCollection<string> Conseils { get; }
    public ObservableCollection<ImpactPageViewModel> ImpactPages { get; }
    public ObservableCollection<ShowCalendarItemViewModel> ShowsAVenir { get; }
    public ObservableCollection<SegmentTypeOptionViewModel> SegmentTypes { get; }
    public ObservableCollection<ParticipantViewModel> WorkersDisponibles { get; }
    public ObservableCollection<string> ConsignesBooking { get; }
    public ObservableCollection<string> RecapFm { get; }
    public ObservableCollection<ParticipantViewModel> NouveauSegmentParticipants { get; }
    public ObservableCollection<StorylineListItemViewModel> Storylines { get; }
    public ObservableCollection<StorylineOptionViewModel> StorylineOptions { get; }
    public ObservableCollection<StorylineParticipantViewModel> StorylineParticipantsEdition { get; }
    public HelpPanelViewModel AidePanel { get; }
    public CodexViewModel Codex { get; }
    public ObservableCollection<TableViewItemViewModel> TableItems { get; }
    public DataGridCollectionView TableItemsView { get; }
    public TableViewConfigurationViewModel TableConfiguration { get; }
    public ObservableCollection<TableFilterOptionViewModel> TableTypeFilters { get; }
    public ObservableCollection<TableFilterOptionViewModel> TableStatusFilters { get; }
    public ObservableCollection<GlobalSearchResultViewModel> RechercheGlobaleResultats { get; }

    public ReactiveCommand<Unit, Unit> OuvrirRechercheGlobaleCommand { get; }
    public ReactiveCommand<Unit, Unit> FermerRechercheGlobaleCommand { get; }

    public IReadOnlyList<YouthGenerationOptionViewModel> YouthGenerationModes { get; }
    public IReadOnlyList<WorldGenerationOptionViewModel> WorldGenerationModes { get; }
    public IReadOnlyList<StorylinePhaseOptionViewModel> StorylinePhases { get; }
    public IReadOnlyList<StorylineStatusOptionViewModel> StorylineStatuts { get; }

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

    public string? NouveauSegmentStorylineId
    {
        get => _nouveauSegmentStorylineId;
        set => this.RaiseAndSetIfChanged(ref _nouveauSegmentStorylineId, value);
    }
    private string? _nouveauSegmentStorylineId;

    public StorylineListItemViewModel? StorylineSelectionnee
    {
        get => _storylineSelectionnee;
        set
        {
            this.RaiseAndSetIfChanged(ref _storylineSelectionnee, value);
            ChargerStorylineSelection();
        }
    }
    private StorylineListItemViewModel? _storylineSelectionnee;

    public string? StorylineNom
    {
        get => _storylineNom;
        set => this.RaiseAndSetIfChanged(ref _storylineNom, value);
    }
    private string? _storylineNom;

    public string? StorylineResume
    {
        get => _storylineResume;
        set => this.RaiseAndSetIfChanged(ref _storylineResume, value);
    }
    private string? _storylineResume;

    public StorylinePhaseOptionViewModel? StorylinePhaseSelection
    {
        get => _storylinePhaseSelection;
        set => this.RaiseAndSetIfChanged(ref _storylinePhaseSelection, value);
    }
    private StorylinePhaseOptionViewModel? _storylinePhaseSelection;

    public StorylineStatusOptionViewModel? StorylineStatutSelection
    {
        get => _storylineStatutSelection;
        set => this.RaiseAndSetIfChanged(ref _storylineStatutSelection, value);
    }
    private StorylineStatusOptionViewModel? _storylineStatutSelection;

    public string? StorylineParticipantSelectionId
    {
        get => _storylineParticipantSelectionId;
        set => this.RaiseAndSetIfChanged(ref _storylineParticipantSelectionId, value);
    }
    private string? _storylineParticipantSelectionId;

    public string? ResumeShow
    {
        get => _resumeShow;
        private set => this.RaiseAndSetIfChanged(ref _resumeShow, value);
    }
    private string? _resumeShow;

    public SegmentResultViewModel? ResultatSelectionne
    {
        get => _resultatSelectionne;
        set => this.RaiseAndSetIfChanged(ref _resultatSelectionne, value);
    }
    private SegmentResultViewModel? _resultatSelectionne;

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
            AppliquerFiltreTable();
        }
    }
    private string? _tableRecherche;

    public TableFilterOptionViewModel TableSelectedTypeFilter
    {
        get => _tableSelectedTypeFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _tableSelectedTypeFilter, value);
            AppliquerFiltreTable();
        }
    }
    private TableFilterOptionViewModel _tableSelectedTypeFilter;

    public TableFilterOptionViewModel TableSelectedStatusFilter
    {
        get => _tableSelectedStatusFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _tableSelectedStatusFilter, value);
            AppliquerFiltreTable();
        }
    }
    private TableFilterOptionViewModel _tableSelectedStatusFilter;

    public string? TableResultatsResume
    {
        get => _tableResultatsResume;
        private set => this.RaiseAndSetIfChanged(ref _tableResultatsResume, value);
    }
    private string? _tableResultatsResume;

    public TableViewItemViewModel? TableSelection
    {
        get => _tableSelection;
        set => this.RaiseAndSetIfChanged(ref _tableSelection, value);
    }
    private TableViewItemViewModel? _tableSelection;

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
        if (_context is null || _repository is null)
        {
            return;
        }

        var booking = _repository.ChargerBookingPlan(_context);
        var validation = _validator.ValiderBooking(booking);
        ValidationErreurs = validation.EstValide ? null : string.Join("\n", validation.Erreurs);
        ValidationAvertissements = validation.Avertissements.Count == 0 ? null : string.Join("\n", validation.Avertissements);

        if (!validation.EstValide)
        {
            return;
        }

        var seed = HashCode.Combine(_context.Show.ShowId, _context.Show.Semaine);
        var engine = new ShowSimulationEngine(new SeededRandomProvider(seed));
        var resultat = engine.Simuler(_context);
        var participantsNoms = _context.Workers.ToDictionary(worker => worker.WorkerId, worker => worker.NomComplet);
        Resultats.Clear();
        foreach (var segment in resultat.RapportShow.Segments)
        {
            var libelle = _segmentLabels.TryGetValue(segment.TypeSegment, out var label) ? label : segment.TypeSegment;
            Resultats.Add(new SegmentResultViewModel(segment, libelle, participantsNoms));
        }

        ResultatSelectionne = Resultats.FirstOrDefault();
        ResumeShow =
            $"Note {resultat.RapportShow.NoteGlobale} • Audience {resultat.RapportShow.Audience} " +
            $"• Billetterie {resultat.RapportShow.Billetterie:C} • Merch {resultat.RapportShow.Merch:C} • TV {resultat.RapportShow.Tv:C}";
        MettreAJourAnalyseShow(resultat);
        MettreAJourImpacts(resultat);
        MettreAJourRecapFm(resultat);

        var impactApplier = new ImpactApplier(_repository);
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
        if (_context is null)
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
        if (_context is null)
        {
            return;
        }

        var type = string.IsNullOrWhiteSpace(NouveauSegmentTypeId)
            ? SegmentTypes.FirstOrDefault()?.Id ?? "match"
            : NouveauSegmentTypeId;
        var participants = NouveauSegmentParticipants.Select(p => p.WorkerId).ToList();
        var newSegment = new SegmentDefinition(
            $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
            type,
            participants,
            Math.Max(1, NouveauSegmentDuree),
            NouveauSegmentMainEvent,
            string.IsNullOrWhiteSpace(NouveauSegmentStorylineId) ? null : NouveauSegmentStorylineId,
            null,
            60,
            null,
            null);

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
        if (_context is null)
        {
            return;
        }

        var updated = new SegmentDefinition(
            segment.SegmentId,
            segment.TypeSegment,
            segment.Participants.Select(p => p.WorkerId).ToList(),
            Math.Max(1, segment.DureeMinutes),
            segment.EstMainEvent,
            string.IsNullOrWhiteSpace(segment.StorylineId) ? null : segment.StorylineId,
            segment.TitreId,
            segment.Intensite,
            segment.VainqueurId,
            segment.PerdantId);

        _repository.MettreAJourSegment(updated);
        ChargerShow();
    }

    public void CopierSegment(SegmentViewModel segment)
    {
        if (_context is null)
        {
            return;
        }

        var copie = new SegmentDefinition(
            $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
            segment.TypeSegment,
            segment.Participants.Select(p => p.WorkerId).ToList(),
            segment.DureeMinutes,
            segment.EstMainEvent,
            segment.StorylineId,
            segment.TitreId,
            segment.Intensite,
            segment.VainqueurId,
            segment.PerdantId);

        _repository.AjouterSegment(_context.Show.ShowId, copie, Segments.Count + 1);
        ChargerShow();
    }

    public void CreerStoryline()
    {
        if (_context is null || _repository is null)
        {
            return;
        }

        var nom = string.IsNullOrWhiteSpace(StorylineNom) ? "Nouvelle storyline" : StorylineNom.Trim();
        var phase = StorylinePhaseSelection?.Id ?? "BUILD";
        var statut = StorylineStatutSelection?.Id ?? "ACTIVE";
        var participants = StorylineParticipantsEdition
            .Select(participant => new StorylineParticipant(participant.WorkerId, participant.Role))
            .ToList();

        var storylineId = $"ST-{Guid.NewGuid():N}".ToUpperInvariant();
        var storyline = _storylineService.Creer(storylineId, nom, participants);
        storyline = _storylineService.MettreAJour(storyline, phase: phase, statut: statut, resume: StorylineResume);

        _repository.CreerStoryline(_context.Show.CompagnieId, storyline);
        _repository.AjouterStorylineEvent(storyline.StorylineId, "CREATED", _context.Show.Semaine, "Création storyline");
        ReinitialiserStorylineEdition();
        ChargerShow();
    }

    public void MettreAJourStoryline()
    {
        if (_context is null || _repository is null || StorylineSelectionnee is null)
        {
            return;
        }

        var selection = _context.Storylines.FirstOrDefault(storyline => storyline.StorylineId == StorylineSelectionnee.StorylineId);
        if (selection is null)
        {
            return;
        }

        var nom = string.IsNullOrWhiteSpace(StorylineNom) ? selection.Nom : StorylineNom.Trim();
        var phase = StorylinePhaseSelection?.Id ?? selection.Phase;
        var statut = StorylineStatutSelection?.Id ?? selection.Statut;
        var participants = StorylineParticipantsEdition
            .Select(participant => new StorylineParticipant(participant.WorkerId, participant.Role))
            .ToList();

        var updated = _storylineService.MettreAJour(selection, nom, phase, statut, StorylineResume, participants);
        _repository.MettreAJourStoryline(updated);
        _repository.AjouterStorylineEvent(updated.StorylineId, "UPDATED", _context.Show.Semaine, "Mise à jour storyline");
        ChargerShow();
    }

    public void AvancerStoryline()
    {
        if (_context is null || _repository is null || StorylineSelectionnee is null)
        {
            return;
        }

        var selection = _context.Storylines.FirstOrDefault(storyline => storyline.StorylineId == StorylineSelectionnee.StorylineId);
        if (selection is null)
        {
            return;
        }

        var updated = _storylineService.Avancer(selection);
        _repository.MettreAJourStoryline(updated);
        _repository.AjouterStorylineEvent(updated.StorylineId, "ADVANCED", _context.Show.Semaine, $"Phase {updated.Phase}");
        ChargerShow();
    }

    public void SupprimerStoryline()
    {
        if (_repository is null || StorylineSelectionnee is null)
        {
            return;
        }

        _repository.SupprimerStoryline(StorylineSelectionnee.StorylineId);
        ReinitialiserStorylineEdition();
        ChargerShow();
    }

    public void AjouterParticipantStoryline()
    {
        if (StorylineParticipantSelectionId is null || _context is null)
        {
            return;
        }

        if (StorylineParticipantsEdition.Any(p => p.WorkerId == StorylineParticipantSelectionId))
        {
            return;
        }

        var worker = _context.Workers.FirstOrDefault(w => w.WorkerId == StorylineParticipantSelectionId);
        if (worker is null)
        {
            return;
        }

        StorylineParticipantsEdition.Add(new StorylineParticipantViewModel(worker.WorkerId, worker.NomComplet, "principal", worker.Momentum));
        StorylineParticipantSelectionId = null;
    }

    public void RetirerParticipantStoryline(StorylineParticipantViewModel participant)
    {
        StorylineParticipantsEdition.Remove(participant);
    }

    public void DeplacerSegment(SegmentViewModel segment, int delta)
    {
        if (_context is null)
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

        var weekly = new WeeklyLoopService(_repository);
        weekly.PasserSemaineSuivante(ShowId);
        ChargerInbox();
        ChargerShow();
    }

    public void EnregistrerParametresGeneration()
    {
        var youthMode = YouthGenerationSelection?.Mode ?? YouthGenerationMode.Realiste;
        var worldMode = WorldGenerationSelection?.Mode ?? WorldGenerationMode.Desactivee;
        var pivot = SemainePivotAnnuelle > 0 ? SemainePivotAnnuelle : null;
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

    public void OuvrirFicheWorker()
    {
        if (ResultatSelectionne is null || _context is null)
        {
            return;
        }

        var workerId = ResultatSelectionne.ParticipantIds.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(workerId))
        {
            return;
        }

        var worker = _context.Workers.FirstOrDefault(w => w.WorkerId == workerId);
        if (worker is null)
        {
            return;
        }

        RechercheGlobaleVisible = true;
        RechercheGlobaleQuery = worker.NomComplet;
    }

    public void VoirImpacts()
    {
        SelectionnerImpact("impacts.popularite");
    }

    public void VoirFinances()
    {
        SelectionnerImpact("impacts.finances");
    }

    private void ChargerShow()
    {
        if (_repository is null)
        {
            return;
        }

        _context = _repository.ChargerShowContext(ShowId);
        if (_context is null)
        {
            return;
        }

        Segments.Clear();
        WorkersDisponibles.Clear();
        StorylinesDisponibles.Clear();

        foreach (var worker in _context.Workers)
        {
            WorkersDisponibles.Add(new ParticipantViewModel(worker.WorkerId, worker.NomComplet));
        }

        StorylinesDisponibles.Add(new StorylineOptionViewModel(null, "Aucune storyline", 0, "-", "N/A"));
        foreach (var storyline in _context.Storylines)
        {
            var phase = ObtenirLibellePhase(storyline.Phase);
            var statut = ObtenirLibelleStatut(storyline.Status);
            StorylinesDisponibles.Add(new StorylineOptionViewModel(
                storyline.StorylineId,
                $"{storyline.Nom} • Heat {storyline.Heat} • {phase}",
                storyline.Heat,
                phase,
                statut));
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
                _segmentLabels,
                participants,
                segment.StorylineId,
                segment.TitreId,
                segment.Intensite,
                segment.VainqueurId,
                segment.PerdantId));
        }

        ChargerStorylinesView();
        MettreAJourAttributs();
        MettreAJourTableItems();
        MettreAJourIndexRechercheGlobale();
        MettreAJourRechercheGlobale();
        ChargerCalendrier();
        MettreAJourAvertissements();
        InitialiserNouveauShow();
    }

    private void ChargerStorylinesView()
    {
        if (_context is null)
        {
            return;
        }

        var selectionId = StorylineSelectionnee?.StorylineId;

        Storylines.Clear();
        StorylineOptions.Clear();
        StorylineOptions.Add(new StorylineOptionViewModel(null, "Aucune"));

        foreach (var storyline in _context.Storylines)
        {
            StorylineOptions.Add(new StorylineOptionViewModel(storyline.StorylineId, storyline.Nom));

            var participants = storyline.Participants.Select(participant =>
            {
                var worker = _context.Workers.FirstOrDefault(w => w.WorkerId == participant.WorkerId);
                var nom = worker?.NomComplet ?? participant.WorkerId;
                var momentum = worker?.Momentum ?? 0;
                return new StorylineParticipantViewModel(participant.WorkerId, nom, participant.Role, momentum);
            }).ToList();

            Storylines.Add(new StorylineListItemViewModel(
                storyline.StorylineId,
                storyline.Nom,
                storyline.Phase,
                storyline.Heat,
                storyline.Statut,
                storyline.Resume ?? string.Empty,
                participants));
        }

        StorylineSelectionnee = selectionId is null
            ? null
            : Storylines.FirstOrDefault(storyline => storyline.StorylineId == selectionId);

        if (StorylineSelectionnee is null)
        {
            ChargerStorylineSelection();
        }
    }

    private void ChargerStorylineSelection()
    {
        StorylineParticipantsEdition.Clear();

        if (_context is null || StorylineSelectionnee is null)
        {
            StorylineNom = null;
            StorylineResume = null;
            StorylinePhaseSelection = StorylinePhases.FirstOrDefault();
            StorylineStatutSelection = StorylineStatuts.FirstOrDefault();
            StorylineParticipantSelectionId = null;
            return;
        }

        var selection = _context.Storylines.FirstOrDefault(storyline => storyline.StorylineId == StorylineSelectionnee.StorylineId);
        if (selection is null)
        {
            return;
        }

        StorylineNom = selection.Nom;
        StorylineResume = selection.Resume;
        StorylinePhaseSelection = StorylinePhases.FirstOrDefault(phase => phase.Id == selection.Phase) ?? StorylinePhases.FirstOrDefault();
        StorylineStatutSelection = StorylineStatuts.FirstOrDefault(statut => statut.Id == selection.Statut) ?? StorylineStatuts.FirstOrDefault();
        StorylineParticipantSelectionId = null;

        foreach (var participant in selection.Participants)
        {
            var worker = _context.Workers.FirstOrDefault(w => w.WorkerId == participant.WorkerId);
            var nom = worker?.NomComplet ?? participant.WorkerId;
            var momentum = worker?.Momentum ?? 0;
            StorylineParticipantsEdition.Add(new StorylineParticipantViewModel(participant.WorkerId, nom, participant.Role, momentum));
        }
    }

    private void ReinitialiserStorylineEdition()
    {
        StorylineSelectionnee = null;
        StorylineNom = null;
        StorylineResume = null;
        StorylinePhaseSelection = StorylinePhases.FirstOrDefault();
        StorylineStatutSelection = StorylineStatuts.FirstOrDefault();
        StorylineParticipantsEdition.Clear();
        StorylineParticipantSelectionId = null;
    }

    private void ChargerInbox()
    {
        Inbox.Clear();
        if (_repository is null)
        {
            return;
        }

        foreach (var item in _repository.ChargerInbox())
        {
            Inbox.Add(new InboxItemViewModel(item));
        }
    }

    private void ChargerParametresGeneration()
    {
        var options = _repository.ChargerParametresGeneration();
        YouthGenerationSelection = YouthGenerationModes.FirstOrDefault(mode => mode.Mode == options.YouthMode);
        WorldGenerationSelection = WorldGenerationModes.FirstOrDefault(mode => mode.Mode == options.WorldMode);
        SemainePivotAnnuelle = options.SemainePivotAnnuelle ?? 1;
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
                .Where(worker => storyline.Participants.Contains(worker.WorkerId))
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

        if (TableSelectedTypeFilter.Id != "tous")
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

        if (TableSelectedStatusFilter.Id != "tous")
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
            delta.PopulariteWorkersDelta.Select(kv => $"{kv.Key} {kv.Value:+#;-#;0}"));

        AjouterDeltas("impacts.finances",
            delta.Finances.Select(tx => $"{tx.Libelle} {tx.Montant:+#;-#;0}"));

        AjouterDeltas("impacts.fatigue",
            delta.FatigueDelta.Select(kv => $"{kv.Key} +{kv.Value}"),
            delta.Blessures.Select(kv => $"{kv.Key} : {kv.Value}"));

        AjouterDeltas("impacts.storylines",
            delta.StorylineHeatDelta.Select(kv => $"{kv.Key} {kv.Value:+#;-#;0}"));

        AjouterDeltas("impacts.titres",
            delta.TitrePrestigeDelta.Select(kv => $"{kv.Key} {kv.Value:+#;-#;0}"));

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
        foreach (var type in _segmentLabels)
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
        NouveauSegmentStorylineId = StorylineOptions.FirstOrDefault()?.Id;
    }

    private void ChargerCalendrier()
    {
        if (_context is null)
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
        ValidationErreurs = validation.EstValide ? null : string.Join("\n", validation.Erreurs);
        ValidationAvertissements = validation.Avertissements.Count == 0 ? null : string.Join("\n", validation.Avertissements);

        foreach (var segment in Segments)
        {
            var messages = new List<string>();
            if (segment.Participants.Count == 0)
            {
                messages.Add("Ajoutez des participants.");
            }

            if (segment.DureeMinutes <= 0)
            {
                messages.Add("Durée invalide.");
            }

            segment.Avertissements = messages.Count == 0 ? null : string.Join(" ", messages);
        }
    }

    private void MettreAJourRecapFm(ShowSimulationResult resultat)
    {
        RecapFm.Clear();
        RecapFm.Add($"Note show : {resultat.RapportShow.NoteGlobale}");
        RecapFm.Add($"Audience : {resultat.RapportShow.Audience}");
        RecapFm.Add($"Finances : Billetterie {resultat.RapportShow.Billetterie:C} • Merch {resultat.RapportShow.Merch:C} • TV {resultat.RapportShow.Tv:C}");
        RecapFm.Add($"Pop compagnie : {resultat.RapportShow.PopulariteCompagnieDelta:+#;-#;0}");
        RecapFm.Add($"Momentum total : {resultat.Delta.MomentumDelta.Values.Sum():+#;-#;0}");
        RecapFm.Add($"Heat storylines : {resultat.Delta.StorylineHeatDelta.Values.Sum():+#;-#;0}");
        RecapFm.Add($"Fatigue cumulée : {resultat.Delta.FatigueDelta.Values.Sum():+#;-#;0}");
        foreach (var segment in resultat.RapportShow.Segments)
        {
            var libelle = _segmentLabels.TryGetValue(segment.TypeSegment, out var label) ? label : segment.TypeSegment;
            var breakdown = string.Join(" | ", segment.Facteurs.Select(facteur => $"{facteur.Libelle} {facteur.Impact:+#;-#;0}"));
            var impacts = new SegmentResultViewModel(segment, libelle).Impacts;
            RecapFm.Add($"{libelle} • Note {segment.Note} • {breakdown} • {impacts}");
        }
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
        YouthBudgetNouveau = YouthStructureSelection?.BudgetAnnuel ?? 0;
    }

    private void ChargerYouthDetails()
    {
        if (_repository is null || YouthStructureSelection is null)
        {
            return;
        }

        YouthTrainees.Clear();
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

        YouthPrograms.Clear();
        foreach (var programme in _repository.ChargerYouthPrograms(YouthStructureSelection.YouthId))
        {
            YouthPrograms.Add(new YouthProgramViewModel(
                programme.ProgramId,
                programme.Nom,
                programme.DureeSemaines,
                programme.Focus));
        }

        YouthStaffAssignments.Clear();
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
        YouthActionMessage = $"Budget mis à jour: {YouthBudgetNouveau}€.";
    }

    public void AffecterCoachYouth()
    {
        if (_repository is null || YouthStructureSelection is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(YouthCoachWorkerId) || string.IsNullOrWhiteSpace(YouthCoachRole))
        {
            YouthActionMessage = "Renseignez un worker ID et un rôle.";
            return;
        }

        var semaine = _context?.Show.Semaine ?? 1;
        _repository.AffecterCoachYouth(YouthStructureSelection.YouthId, YouthCoachWorkerId.Trim(), YouthCoachRole.Trim(), semaine);
        YouthActionMessage = "Coach affecté à la structure.";
        ChargerYouthDetails();
    }

    public void DiplomerTrainee(string workerId)
    {
        if (_repository is null || string.IsNullOrWhiteSpace(workerId))
        {
            return;
        }

        var semaine = _context?.Show.Semaine ?? 1;
        _repository.DiplomerTrainee(workerId, semaine);
        YouthActionMessage = "Graduation enregistrée.";
        ChargerYouthDetails();
    }

    private static IReadOnlyDictionary<string, string> ChargerSegmentTypes()
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
            return new Dictionary<string, string>();
        }

        var spec = reader.Charger<SegmentTypesSpec>(chemin);
        return spec.Types.ToDictionary(type => type.Id, type => type.Libelle);
    }
}

public sealed record YouthGenerationOptionViewModel(string Libelle, YouthGenerationMode Mode);

public sealed record WorldGenerationOptionViewModel(string Libelle, WorldGenerationMode Mode);
