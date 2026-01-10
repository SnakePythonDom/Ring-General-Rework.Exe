using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Validation;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Data.Repositories;
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
    private readonly SettingsRepository? _settingsRepository;
    private SegmentViewModel? _selectedSegment;
    private string? _validationErrors;
    private string? _validationWarnings;
    private BookingControlLevel _controlLevel = BookingControlLevel.CoBooker;

    public BookingViewModel(
        GameRepository repository,
        BookingValidator validator,
        SegmentTypeCatalog segmentCatalog,
        IEventAggregator eventAggregator,
        SettingsRepository? settingsRepository = null)
    {
        _repository = repository;
        _validator = validator;
        _segmentCatalog = segmentCatalog;
        _eventAggregator = eventAggregator;
        _settingsRepository = settingsRepository;

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

        // Charger le niveau de contrôle depuis les settings
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
        UpdateCalculatedProperties();
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

    public int ShowDuration { get; set; } = 120; // TODO: Charger depuis le contexte

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

    // ========== MÉTHODES PUBLIQUES ==========

    public void LoadShow(string showId)
    {
        // TODO: Charger le show depuis le repository
        // Logique extraite de GameSessionViewModel.ChargerShow()
        System.Diagnostics.Debug.WriteLine($"Loading show {showId}");

        // Pour l'instant, données de test
        LoadTestData();
    }

    // ========== MÉTHODES PRIVÉES ==========

    private void AddSegment()
    {
        // Logique extraite de GameSessionViewModel.AjouterSegment() (ligne 652)
        var newSegment = new SegmentViewModel(
            $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
            "match",
            8,
            false,
            _segmentCatalog,
            new List<ParticipantViewModel>(),
            null,
            null,
            60,
            null,
            null,
            new Dictionary<string, string>()
        );

        Segments.Add(newSegment);
        UpdateCalculatedProperties();
        SelectedSegment = newSegment;
        ValidateBooking();
    }

    private void DeleteSegment(SegmentViewModel segment)
    {
        // Logique extraite de GameSessionViewModel.SupprimerSegment() (ligne 768)
        Segments.Remove(segment);
        UpdateCalculatedProperties();
        if (SelectedSegment == segment)
        {
            SelectedSegment = Segments.FirstOrDefault();
        }
        ValidateBooking();
    }

    private void MoveSegment(SegmentViewModel segment, int delta)
    {
        // Logique extraite de GameSessionViewModel.DeplacerSegment() (ligne 812)
        var index = Segments.IndexOf(segment);
        var targetIndex = index + delta;

        if (index < 0 || targetIndex < 0 || targetIndex >= Segments.Count)
        {
            return;
        }

        Segments.Move(index, targetIndex);
        ValidateBooking();
    }

    private void SaveSegment(SegmentViewModel segment)
    {
        // Logique extraite de GameSessionViewModel.EnregistrerSegment() (ligne 688)
        // TODO: Sauvegarder dans le repository
        ValidateBooking();
    }

    private void CopySegment(SegmentViewModel segment)
    {
        // Logique extraite de GameSessionViewModel.CopierSegment() (ligne 716)
        var copy = new SegmentViewModel(
            $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
            segment.TypeSegment,
            segment.DureeMinutes,
            segment.EstMainEvent,
            _segmentCatalog,
            segment.Participants.ToList(),
            segment.StorylineId,
            segment.TitreId,
            segment.Intensite,
            segment.VainqueurId,
            segment.PerdantId,
            segment.ConstruireSettings()
        );

        Segments.Add(copy);
        ValidateBooking();
    }

    private void ApplyTemplate(SegmentTemplateViewModel template)
    {
        // Logique extraite de GameSessionViewModel.AppliquerTemplate() (ligne 790)
        // TODO: Créer le segment depuis le template
        System.Diagnostics.Debug.WriteLine($"Applying template {template.Nom}");
    }

    private void ValidateBooking()
    {
        // Logique extraite de GameSessionViewModel.MettreAJourAvertissements() (ligne 2126)
        ValidationIssues.Clear();

        // Validation basique
        if (Segments.Count == 0)
        {
            ValidationIssues.Add(new BookingIssueViewModel(
                "booking.empty",
                "Le booking est vide. Ajoutez au moins un segment.",
                ValidationSeverity.Avertissement,
                null,
                "Ajouter"
            ));
        }

        var hasMainEvent = Segments.Any(s => s.EstMainEvent);
        if (!hasMainEvent && Segments.Count > 0)
        {
            ValidationIssues.Add(new BookingIssueViewModel(
                "booking.main-event.missing",
                "Aucun main event défini.",
                ValidationSeverity.Avertissement,
                null,
                "Marquer"
            ));
        }

        if (TotalDuration > ShowDuration)
        {
            ValidationIssues.Add(new BookingIssueViewModel(
                "booking.duration.exceed",
                $"La durée totale ({TotalDuration} min) dépasse la durée du show ({ShowDuration} min).",
                ValidationSeverity.Erreur,
                null,
                "Réduire"
            ));
        }

        // Mettre à jour les résumés
        var errors = ValidationIssues.Where(i => i.Severity == ValidationSeverity.Erreur).ToList();
        var warnings = ValidationIssues.Where(i => i.Severity == ValidationSeverity.Avertissement).ToList();

        ValidationErrors = errors.Count > 0 ? string.Join("\n", errors.Select(e => e.Message)) : null;
        ValidationWarnings = warnings.Count > 0 ? string.Join("\n", warnings.Select(w => w.Message)) : null;

        this.RaisePropertyChanged(nameof(IsBookingValid));
        this.RaisePropertyChanged(nameof(TotalDuration));
        this.RaisePropertyChanged(nameof(DurationSummary));
    }

    private void InitializeSegmentTypes()
    {
        // Logique extraite de GameSessionViewModel.InitialiserSegmentTypes() (ligne 1983)
        SegmentTypes.Clear();
        SegmentTypes.Add(new SegmentTypeOptionViewModel("match", "Match"));
        SegmentTypes.Add(new SegmentTypeOptionViewModel("promo", "Promo"));
        SegmentTypes.Add(new SegmentTypeOptionViewModel("angle", "Angle"));
        SegmentTypes.Add(new SegmentTypeOptionViewModel("interview", "Interview"));
    }

    private void LoadTestData()
    {
        // Données de test pour visualisation
        WorkersAvailable.Add(new ParticipantViewModel("W001", "John Cena"));
        WorkersAvailable.Add(new ParticipantViewModel("W002", "Randy Orton"));
        WorkersAvailable.Add(new ParticipantViewModel("W003", "The Rock"));

        StorylinesAvailable.Add(new StorylineOptionViewModel(string.Empty, "Aucune storyline"));
        StorylinesAvailable.Add(new StorylineOptionViewModel("ST001", "Rivalité Title"));
        StorylinesAvailable.Add(new StorylineOptionViewModel("ST002", "Legacy Rising"));

        TitlesAvailable.Add(new TitleOptionViewModel(string.Empty, "Aucun titre"));
        TitlesAvailable.Add(new TitleOptionViewModel("T001", "World Title"));

        // Ajouter des segments de test
        var mainEvent = new SegmentViewModel(
            "SEG001",
            "match",
            15,
            true,
            _segmentCatalog,
            new List<ParticipantViewModel>
            {
                new ParticipantViewModel("W001", "John Cena"),
                new ParticipantViewModel("W002", "Randy Orton")
            },
            "ST001",
            "T001",
            85,
            null,
            null,
            new Dictionary<string, string>()
        );

        var promo = new SegmentViewModel(
            "SEG002",
            "promo",
            8,
            false,
            _segmentCatalog,
            new List<ParticipantViewModel>
            {
                new ParticipantViewModel("W003", "The Rock")
            },
            null,
            null,
            0,
            null,
            null,
            new Dictionary<string, string>()
        );

        Segments.Add(mainEvent);
        Segments.Add(promo);
        UpdateCalculatedProperties();
        SelectedSegment = mainEvent;

        ValidateBooking();
    }

    private void UpdateCalculatedProperties()
    {
        TotalDuration = Segments.Sum(s => s.DureeMinutes);
        DurationSummary = $"{TotalDuration}/{ShowDuration} min";
    }

    private void LoadBookingControlLevel()
    {
        if (_settingsRepository == null) return;

        try
        {
            var levelString = _settingsRepository.ChargerBookingControlLevel();
            if (Enum.TryParse<BookingControlLevel>(levelString, out var level))
            {
                _controlLevel = level;
                this.RaisePropertyChanged(nameof(ControlLevel));
                this.RaisePropertyChanged(nameof(ControlLevelDescription));
                this.RaisePropertyChanged(nameof(CanAutoBook));
            }
        }
        catch
        {
            // Utiliser la valeur par défaut si le chargement échoue
        }
    }

    private void SaveBookingControlLevel()
    {
        if (_settingsRepository == null) return;

        try
        {
            _settingsRepository.SauvegarderBookingControlLevel(ControlLevel.ToString());
        }
        catch
        {
            // Ignorer les erreurs de sauvegarde
        }
    }
}

// ========== ÉVÉNEMENTS ==========

/// <summary>
/// Événement publié quand un segment est sélectionné
/// </summary>
public record SegmentSelectedEvent(SegmentViewModel Segment);
