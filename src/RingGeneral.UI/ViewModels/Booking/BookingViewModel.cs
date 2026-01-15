using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Validation;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Services;
using RingGeneral.UI.Services.Messaging;

namespace RingGeneral.UI.ViewModels.Booking;

/// <summary>
/// ViewModel pour la gestion du booking de shows
/// Extrait de GameSessionViewModel (lignes 196-829)
/// </summary>
public sealed class BookingViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private readonly BookingValidator _validator;
    private readonly SegmentTypeCatalog _segmentCatalog;
    private readonly IEventAggregator _eventAggregator;
    private readonly BookingBuilderService _bookingBuilder;
    private readonly TemplateService _templateService;
    private readonly IBookingControlService _bookingControlService;
    private readonly IShowDayOrchestrator _showDayOrchestrator;

    private SegmentViewModel? _selectedSegment;
    private string? _validationErrors;
    private string? _validationWarnings;
    private BookingControlLevel _controlLevel = BookingControlLevel.CoBooker;
    private List<SegmentDefinition> _currentBooking = new();

    // Overlay properties
    public bool IsWorkerSelectionVisible
    {
        get => _isWorkerSelectionVisible;
        set => this.RaiseAndSetIfChanged(ref _isWorkerSelectionVisible, value);
    }
    private bool _isWorkerSelectionVisible;

    public WorkerSelectionViewModel? WorkerSelectionContent
    {
        get => _workerSelectionContent;
        set => this.RaiseAndSetIfChanged(ref _workerSelectionContent, value);
    }
    private WorkerSelectionViewModel? _workerSelectionContent;

    public BookingViewModel(
        GameRepository repository,
        BookingValidator validator,
        SegmentTypeCatalog segmentCatalog,
        IEventAggregator eventAggregator,
        BookingBuilderService bookingBuilder,
        TemplateService templateService,
        IBookingControlService bookingControlService,
        IShowDayOrchestrator showDayOrchestrator)
    {
        _repository = repository;
        _validator = validator;
        _segmentCatalog = segmentCatalog;
        _eventAggregator = eventAggregator;
        _bookingBuilder = bookingBuilder;
        _templateService = templateService;
        _bookingControlService = bookingControlService;
        _showDayOrchestrator = showDayOrchestrator;

        // Subscribe to selection requests
        _eventAggregator.GetEvent<RequestWorkerSelectionEvent>()
            .Subscribe(OnWorkerSelectionRequested);

        // Collections
        Segments = new ObservableCollection<SegmentViewModel>();
        ValidationIssues = new ObservableCollection<BookingIssueViewModel>();
        WorkersAvailable = new ObservableCollection<ParticipantViewModel>();
        StorylinesAvailable = new ObservableCollection<StorylineOptionViewModel>();
        TitlesAvailable = new ObservableCollection<TitleOptionViewModel>();
        SegmentTypes = new ObservableCollection<SegmentTypeOptionViewModel>();
        ControlLevels = new ObservableCollection<BookingControlLevel>
        {
            BookingControlLevel.Spectator,
            BookingControlLevel.Producer,
            BookingControlLevel.CoBooker,
            BookingControlLevel.Dictator
        };

        // Charger le niveau de contrôle
        LoadBookingControlLevel();

        // Commands
        AddSegmentCommand = ReactiveCommand.Create(AddSegment);
        DeleteSegmentCommand = ReactiveCommand.Create<SegmentViewModel>(DeleteSegment);
        MoveSegmentUpCommand = ReactiveCommand.Create<SegmentViewModel>(segment => MoveSegment(segment, -1));
        MoveSegmentDownCommand = ReactiveCommand.Create<SegmentViewModel>(segment => MoveSegment(segment, 1));
        SaveSegmentCommand = ReactiveCommand.Create<SegmentViewModel>(SaveSegment);
        CopySegmentCommand = ReactiveCommand.Create<SegmentViewModel>(CopySegment);
        ApplyTemplateCommand = ReactiveCommand.Create<SegmentTemplateViewModel>(ApplyTemplate);
        ValidateBookingCommand = ReactiveCommand.Create(ValidateBooking);

        // Initialisation
        InitializeSegmentTypes();
    }

    private void OnWorkerSelectionRequested(RequestWorkerSelectionEvent evt)
    {
        var selectionVm = new WorkerSelectionViewModel(WorkersAvailable);
        selectionVm.OnSelectionConfirmed = (worker) =>
        {
            if (worker != null)
            {
                evt.Requester.AddParticipant(worker);
            }
            IsWorkerSelectionVisible = false;
            WorkerSelectionContent = null;
        };

        selectionVm.CancelCommand.Subscribe(_ =>
        {
            IsWorkerSelectionVisible = false;
            WorkerSelectionContent = null;
        });

        WorkerSelectionContent = selectionVm;
        IsWorkerSelectionVisible = true;
    }

    // ========== COLLECTIONS ==========

    public ObservableCollection<SegmentViewModel> Segments { get; }
    public ObservableCollection<BookingIssueViewModel> ValidationIssues { get; }
    public ObservableCollection<ParticipantViewModel> WorkersAvailable { get; }
    public ObservableCollection<StorylineOptionViewModel> StorylinesAvailable { get; }
    public ObservableCollection<TitleOptionViewModel> TitlesAvailable { get; }
    public ObservableCollection<SegmentTypeOptionViewModel> SegmentTypes { get; }
    public ObservableCollection<BookingControlLevel> ControlLevels { get; }

    // ========== PROPRIÉTÉS ==========

    public SegmentViewModel? SelectedSegment
    {
        get => _selectedSegment;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSegment, value);
            // Publier l'événement de sélection pour le context panel
            if (value != null)
            {
                _eventAggregator.Publish(new SegmentSelectedEvent(value));
            }
        }
    }

    public string? ValidationErrors
    {
        get => _validationErrors;
        private set => this.RaiseAndSetIfChanged(ref _validationErrors, value);
    }

    public string? ValidationWarnings
    {
        get => _validationWarnings;
        private set => this.RaiseAndSetIfChanged(ref _validationWarnings, value);
    }

    private int _totalDuration;
    public int TotalDuration
    {
        get => _totalDuration;
        private set => this.RaiseAndSetIfChanged(ref _totalDuration, value);
    }

    public int ShowDuration { get; set; } = 120;

    private string _durationSummary = "0/120 min";
    public string DurationSummary
    {
        get => _durationSummary;
        private set => this.RaiseAndSetIfChanged(ref _durationSummary, value);
    }

    public bool IsBookingValid => ValidationIssues.Count == 0;

    /// <summary>
    /// Niveau de contrôle du booking
    /// </summary>
    public BookingControlLevel ControlLevel
    {
        get => _controlLevel;
        set
        {
            if (_controlLevel != value)
            {
                _controlLevel = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(ControlLevelDescription));
                this.RaisePropertyChanged(nameof(CanAutoBook));
                SaveBookingControlLevel();
            }
        }
    }

    /// <summary>
    /// Description du niveau de contrôle
    /// </summary>
    public string ControlLevelDescription => ControlLevel switch
    {
        BookingControlLevel.Spectator => "👁️ IA contrôle 100% des décisions",
        BookingControlLevel.Producer => "🎬 IA propose, vous validez",
        BookingControlLevel.CoBooker => "🤝 Vous gérez titres majeurs, IA développe midcard",
        BookingControlLevel.Dictator => "👑 Contrôle total, pas d'intervention IA",
        _ => "Niveau non défini"
    };

    /// <summary>
    /// Indique si l'auto-booking est disponible selon le niveau
    /// </summary>
    public bool CanAutoBook => ControlLevel != BookingControlLevel.Dictator;

    // ========== COMMANDS ==========

    public ReactiveCommand<Unit, Unit> AddSegmentCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> DeleteSegmentCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> MoveSegmentUpCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> MoveSegmentDownCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> SaveSegmentCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> CopySegmentCommand { get; }
    public ReactiveCommand<SegmentTemplateViewModel, Unit> ApplyTemplateCommand { get; }
    public ReactiveCommand<Unit, Unit> ValidateBookingCommand { get; }

    public void LoadShow(string showId)
    {
        // Chargement du show réel
        try
        {
            LoadContextData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading show: {ex}");
            LoadTestData();
        }
    }

    private void LoadContextData()
    {
        WorkersAvailable.Clear();
        using (var conn = _repository.CreateConnection())
        {
            conn.Open();
            try
            {
                // Using direct SQL to bypass RepositoryFactory dependency issues
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "SELECT WorkerId, NomComplet FROM Workers ORDER BY NomComplet";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetString(0);
                            var nom = reader.GetString(1);
                            WorkersAvailable.Add(new ParticipantViewModel(id, nom));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle error silently as fallback
                System.Diagnostics.Debug.WriteLine($"Error loading workers: {ex.Message}");
            }
        }

        if (WorkersAvailable.Count == 0)
        {
            LoadTestData();
        }
    }

    // ========== MÉTHODES PRIVÉES ==========

    private void AddSegment()
    {
        var newDef = new SegmentDefinition(
            $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
            "match",
            new List<string>(),
            10,
            false,
            null,
            null,
            50,
            null,
            null,
            new Dictionary<string, string>()
        );

        _currentBooking = _bookingBuilder.AjouterSegment(_currentBooking, newDef).ToList();
        RefreshSegmentsFromDefinitions();

        SelectedSegment = Segments.LastOrDefault();
        ValidateBooking();
    }

    private void DeleteSegment(SegmentViewModel segment)
    {
        if (segment == null) return;

        _currentBooking = _bookingBuilder.SupprimerSegment(_currentBooking, segment.SegmentId).ToList();
        RefreshSegmentsFromDefinitions();

        SelectedSegment = Segments.FirstOrDefault();
        ValidateBooking();
    }

    private void MoveSegment(SegmentViewModel segment, int delta)
    {
        if (segment == null) return;

        _currentBooking = _bookingBuilder.DeplacerSegment(_currentBooking, segment.SegmentId, delta).ToList();
        RefreshSegmentsFromDefinitions();

        // Resélectionner le segment déplacé
        SelectedSegment = Segments.FirstOrDefault(s => s.SegmentId == segment.SegmentId);
        ValidateBooking();
    }

    private void SaveSegment(SegmentViewModel segment)
    {
        // Mise à jour de la définition dans la liste interne
        // SegmentViewModel est déjà mis à jour via bindings, 
        // mais on doit synchroniser _currentBooking si on modifie des propriétés structurelles
        ValidateBooking();
    }

    private void CopySegment(SegmentViewModel segment)
    {
        if (segment == null) return;

        // Conversion ViewModel -> Definition pour duplication
        var def = new SegmentDefinition(
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
            segment.ConstruireSettings()
        );

        var copy = _bookingBuilder.DupliquerSegment(def);
        _currentBooking = _bookingBuilder.AjouterSegment(_currentBooking, copy).ToList();
        RefreshSegmentsFromDefinitions();
        ValidateBooking();
    }

    private void ApplyTemplate(SegmentTemplateViewModel template)
    {
        // TODO: Utiliser TemplateService
    }

    private void ValidateBooking()
    {
        ValidationIssues.Clear();

        // Mca: Transformer les ViewModels en contexts de simulation pour le validator
        var simulationContexts = Segments.Select(s => new SegmentSimulationContext(
            s.SegmentId,
            s.TypeSegment,
            s.Participants.Select(p => p.WorkerId).ToList(),
            s.DureeMinutes,
            s.EstMainEvent,
            s.StorylineId,
            s.TitreId,
            s.Intensite,
            s.VainqueurId,
            s.PerdantId,
            null // ParticipantsDetails
        )).ToList();

        var plan = new BookingPlan(
            "CURRENT_SHOW",
            simulationContexts,
            ShowDuration,
            null
        );

        var result = _validator.ValiderBooking(plan);

        foreach (var error in result.Erreurs)
        {
            ValidationIssues.Add(new BookingIssueViewModel("error", error, ValidationSeverity.Erreur, null, "Fix"));
        }
        foreach (var warning in result.Avertissements)
        {
            ValidationIssues.Add(new BookingIssueViewModel("warning", warning, ValidationSeverity.Avertissement, null, "Fix"));
        }

        var errors = ValidationIssues.Where(i => i.Severity == ValidationSeverity.Erreur).ToList();
        var warnings = ValidationIssues.Where(i => i.Severity == ValidationSeverity.Avertissement).ToList();

        ValidationErrors = errors.Count > 0 ? string.Join("\n", errors.Select(e => e.Message)) : null;
        ValidationWarnings = warnings.Count > 0 ? string.Join("\n", warnings.Select(w => w.Message)) : null;

        UpdateCalculatedProperties();
        this.RaisePropertyChanged(nameof(IsBookingValid));
    }

    private void RefreshSegmentsFromDefinitions()
    {
        Segments.Clear();
        foreach (var def in _currentBooking)
        {
            // Mca : mapping Definition -> ViewModel
            // On a besoin des participants objets, pas juste IDs
            var participants = WorkersAvailable
                .Where(w => def.Participants.Contains(w.WorkerId))
                .ToList();

            var vm = new SegmentViewModel(
                def.SegmentId,
                def.TypeSegment,
                def.DureeMinutes,
                def.EstMainEvent,
                _segmentCatalog,
                _eventAggregator,
                participants,
                def.StorylineId,
                def.TitreId,
                def.Intensite,
                def.VainqueurId,
                def.PerdantId,
                def.Settings
            );
            Segments.Add(vm);
        }
        UpdateCalculatedProperties();
    }

    private void InitializeSegmentTypes()
    {
        SegmentTypes.Clear();
        SegmentTypes.Add(new SegmentTypeOptionViewModel("match", "Match"));
        SegmentTypes.Add(new SegmentTypeOptionViewModel("promo", "Promo"));
        SegmentTypes.Add(new SegmentTypeOptionViewModel("angle", "Angle"));
        SegmentTypes.Add(new SegmentTypeOptionViewModel("interview", "Interview"));
    }

    private void LoadTestData()
    {
        // Données de test pour visualisation si DB vide
        if (WorkersAvailable.Count == 0)
        {
            WorkersAvailable.Add(new ParticipantViewModel("W001", "John Cena"));
            WorkersAvailable.Add(new ParticipantViewModel("W002", "Randy Orton"));
            WorkersAvailable.Add(new ParticipantViewModel("W003", "The Rock"));

            StorylinesAvailable.Add(new StorylineOptionViewModel(string.Empty, "Aucune storyline"));
            StorylinesAvailable.Add(new StorylineOptionViewModel("ST001", "Rivalité Title"));
            StorylinesAvailable.Add(new StorylineOptionViewModel("ST002", "Legacy Rising"));

            TitlesAvailable.Add(new TitleOptionViewModel(string.Empty, "Aucun titre"));
            TitlesAvailable.Add(new TitleOptionViewModel("T001", "World Title"));
        }

        if (_currentBooking.Count == 0)
        {
            var mainEvent = new SegmentDefinition(
               "SEG001", "match", new List<string> { "W001", "W002" }, 15, true, "ST001", "T001", 85, null, null, new Dictionary<string, string>());
            _currentBooking.Add(mainEvent);
            RefreshSegmentsFromDefinitions();
            ValidateBooking();
        }
    }

    private void UpdateCalculatedProperties()
    {
        TotalDuration = Segments.Sum(s => s.DureeMinutes);
        DurationSummary = $"{TotalDuration}/{ShowDuration} min";
    }

    private void LoadBookingControlLevel()
    {
        // Utilisation du service BookingControl
        _controlLevel = _bookingControlService.GetControlLevel();
        this.RaisePropertyChanged(nameof(ControlLevel));
        this.RaisePropertyChanged(nameof(ControlLevelDescription));
        this.RaisePropertyChanged(nameof(CanAutoBook));
    }

    private void SaveBookingControlLevel()
    {
        _bookingControlService.SetControlLevel(_controlLevel);
    }
}

// ========== ÉVÉNEMENTS ==========

/// <summary>
/// Événement publié quand un segment est sélectionné
/// </summary>
public record SegmentSelectedEvent(SegmentViewModel Segment);
