using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Core.Services;
using RingGeneral.Core.Validation;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Booking;

/// <summary>
/// ViewModel pour la gestion complète du booking d'un show.
/// Responsable des segments, validation, simulation et templates.
/// Extrait depuis GameSessionViewModel (Phase 6.2).
/// </summary>
public sealed class ShowBookingViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private readonly BookingValidator _validator;
    private readonly SegmentTypeCatalog _catalog;
    private readonly BookingBuilderService _builder;
    private readonly TemplateService _templateService;
    private readonly IBookerAIEngine? _bookerAIEngine;
    private readonly IBookingControlService? _bookingControlService;
    private readonly SettingsRepository? _settingsRepository;
    private ShowContext? _context;
    private string? _showId;
    private BookingControlLevel _controlLevel = BookingControlLevel.CoBooker; // Phase 1.2

    public ShowBookingViewModel(
        GameRepository repository,
        SegmentTypeCatalog catalog,
        IBookerAIEngine? bookerAIEngine = null,
        IBookingControlService? bookingControlService = null,
        SettingsRepository? settingsRepository = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _bookerAIEngine = bookerAIEngine;
        _bookingControlService = bookingControlService;
        _settingsRepository = settingsRepository;
        _validator = new BookingValidator();
        _builder = new BookingBuilderService();
        _templateService = new TemplateService();
        
        // Phase 1.2 - Charger le niveau de contrôle depuis GameState
        LoadBookingControlLevel();

        // Collections
        Segments = new ObservableCollection<SegmentViewModel>();
        ValidationIssues = new ObservableCollection<BookingIssueViewModel>();
        Results = new ObservableCollection<SegmentResultViewModel>();
        SegmentTypes = new ObservableCollection<SegmentTypeOptionViewModel>();
        Templates = new ObservableCollection<SegmentTemplateViewModel>();
        MatchTypes = new ObservableCollection<MatchTypeViewModel>();
        WhyNote = new ObservableCollection<string>();
        Tips = new ObservableCollection<string>();
        BookingGuidelines = new ObservableCollection<string>();
        
        // Phase 1.2 - Collection des niveaux de contrôle disponibles
        ControlLevels = new ObservableCollection<BookingControlLevel>
        {
            BookingControlLevel.Spectator,
            BookingControlLevel.Producer,
            BookingControlLevel.CoBooker,
            BookingControlLevel.Dictator
        };

        // Commandes
        AddSegmentCommand = ReactiveCommand.Create(AddSegment);
        RemoveSegmentCommand = ReactiveCommand.Create<SegmentViewModel>(RemoveSegment);
        MoveSegmentUpCommand = ReactiveCommand.Create<SegmentViewModel>(MoveSegmentUp);
        MoveSegmentDownCommand = ReactiveCommand.Create<SegmentViewModel>(MoveSegmentDown);
        DuplicateSegmentCommand = ReactiveCommand.Create<SegmentViewModel>(DuplicateSegment);
        SimulateShowCommand = ReactiveCommand.Create(SimulateShow);
        ValidateBookingCommand = ReactiveCommand.Create(ValidateBooking);
        AutoBookCommand = ReactiveCommand.Create(GenerateAutoBooking);

        LoadSegmentTypes();
        LoadMatchTypes();
    }

    #region Collections

    public ObservableCollection<SegmentViewModel> Segments { get; }
    public ObservableCollection<BookingIssueViewModel> ValidationIssues { get; }
    public ObservableCollection<SegmentResultViewModel> Results { get; }
    public ObservableCollection<SegmentTypeOptionViewModel> SegmentTypes { get; }
    public ObservableCollection<SegmentTemplateViewModel> Templates { get; }
    public ObservableCollection<MatchTypeViewModel> MatchTypes { get; }
    public ObservableCollection<string> WhyNote { get; }
    public ObservableCollection<string> Tips { get; }
    public ObservableCollection<string> BookingGuidelines { get; }

    /// <summary>
    /// Phase 1.2 - Niveaux de contrôle disponibles
    /// </summary>
    public ObservableCollection<BookingControlLevel> ControlLevels { get; }

    #endregion

    #region Properties

    private SegmentViewModel? _selectedSegment;
    public SegmentViewModel? SelectedSegment
    {
        get => _selectedSegment;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSegment, value);
            this.RaisePropertyChanged(nameof(HasSelectedSegment));
        }
    }

    public bool HasSelectedSegment => SelectedSegment is not null;

    private SegmentResultViewModel? _selectedResult;
    public SegmentResultViewModel? SelectedResult
    {
        get => _selectedResult;
        set => this.RaiseAndSetIfChanged(ref _selectedResult, value);
    }

    private string? _validationErrors;
    public string? ValidationErrors
    {
        get => _validationErrors;
        private set => this.RaiseAndSetIfChanged(ref _validationErrors, value);
    }

    private string? _validationWarnings;
    public string? ValidationWarnings
    {
        get => _validationWarnings;
        private set => this.RaiseAndSetIfChanged(ref _validationWarnings, value);
    }

    public int TotalDuration => Segments.Sum(s => s.DureeMinutes);
    public int SegmentCount => Segments.Count;
    public bool HasSegments => Segments.Any();
    public bool CanSimulate => HasSegments && string.IsNullOrWhiteSpace(ValidationErrors);

    /// <summary>
    /// Phase 1.2 - Niveau de contrôle du booking
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
                this.RaisePropertyChanged(nameof(CanAutoBook));
                this.RaisePropertyChanged(nameof(ControlLevelDescription));
                // Phase 1.2 - Sauvegarder le niveau de contrôle
                SaveBookingControlLevel();
            }
        }
    }

    /// <summary>
    /// Phase 1.2 - Description du niveau de contrôle
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
    /// Phase 1.2 - Indique si l'auto-booking est disponible selon le niveau
    /// </summary>
    public bool CanAutoBook => ControlLevel != BookingControlLevel.Dictator;

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> AddSegmentCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> RemoveSegmentCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> MoveSegmentUpCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> MoveSegmentDownCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> DuplicateSegmentCommand { get; }
    public ReactiveCommand<Unit, Unit> SimulateShowCommand { get; }
    public ReactiveCommand<Unit, Unit> ValidateBookingCommand { get; }
    public ReactiveCommand<Unit, Unit> AutoBookCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Charge le booking depuis le contexte du show.
    /// </summary>
    public void LoadBooking(ShowContext context, string showId)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _showId = showId ?? throw new ArgumentNullException(nameof(showId));

        Segments.Clear();
        foreach (var segment in context.Segments)
        {
            Segments.Add(new SegmentViewModel(segment));
        }

        LoadTemplates();
        ValidateBooking();

        Logger.Info($"Booking chargé : {Segments.Count} segments, durée totale {TotalDuration} min");

        this.RaisePropertyChanged(nameof(TotalDuration));
        this.RaisePropertyChanged(nameof(SegmentCount));
        this.RaisePropertyChanged(nameof(HasSegments));
    }

    /// <summary>
    /// Ajoute un nouveau segment au booking.
    /// </summary>
    public void AddSegment()
    {
        if (_context is null || string.IsNullOrWhiteSpace(_showId))
        {
            Logger.Warning("Impossible d'ajouter un segment : contexte non chargé");
            return;
        }

        var newSegment = new SegmentDefinition(
            $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
            "promo",
            new List<string>(),
            10,
            false,
            null,
            null,
            0,
            null,
            null,
            new Dictionary<string, string>());

        _repository.AjouterSegment(_showId, newSegment, Segments.Count + 1);
        Segments.Add(new SegmentViewModel(newSegment));

        SelectedSegment = Segments.Last();
        ValidateBooking();

        this.RaisePropertyChanged(nameof(TotalDuration));
        this.RaisePropertyChanged(nameof(SegmentCount));
        this.RaisePropertyChanged(nameof(HasSegments));

        Logger.Debug($"Segment ajouté : {newSegment.SegmentId}");
    }

    /// <summary>
    /// Supprime un segment du booking.
    /// </summary>
    public void RemoveSegment(SegmentViewModel? segment)
    {
        if (segment is null || _context is null)
        {
            return;
        }

        _repository.SupprimerSegment(segment.SegmentId);
        Segments.Remove(segment);

        if (SelectedSegment == segment)
        {
            SelectedSegment = Segments.FirstOrDefault();
        }

        ValidateBooking();

        this.RaisePropertyChanged(nameof(TotalDuration));
        this.RaisePropertyChanged(nameof(SegmentCount));
        this.RaisePropertyChanged(nameof(HasSegments));

        Logger.Debug($"Segment supprimé : {segment.SegmentId}");
    }

    /// <summary>
    /// Déplace un segment vers le haut.
    /// </summary>
    public void MoveSegmentUp(SegmentViewModel? segment)
    {
        if (segment is null) return;

        var index = Segments.IndexOf(segment);
        if (index <= 0) return;

        Segments.Move(index, index - 1);
        SaveSegmentOrder();

        Logger.Debug($"Segment déplacé vers le haut : {segment.SegmentId}");
    }

    /// <summary>
    /// Déplace un segment vers le bas.
    /// </summary>
    public void MoveSegmentDown(SegmentViewModel? segment)
    {
        if (segment is null) return;

        var index = Segments.IndexOf(segment);
        if (index < 0 || index >= Segments.Count - 1) return;

        Segments.Move(index, index + 1);
        SaveSegmentOrder();

        Logger.Debug($"Segment déplacé vers le bas : {segment.SegmentId}");
    }

    /// <summary>
    /// Duplique un segment.
    /// </summary>
    public void DuplicateSegment(SegmentViewModel? segment)
    {
        if (segment is null || _context is null || string.IsNullOrWhiteSpace(_showId))
        {
            return;
        }

        var duplicated = new SegmentDefinition(
            $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
            segment.TypeSegment,
            segment.Participants.Select(p => p.WorkerId).ToList(),
            segment.DureeMinutes,
            segment.EstMainEvent,
            segment.StorylineId,
            segment.TitreId,
            segment.Intensite,
            segment.VainqueurId,
            segment.PerdantId,
            new Dictionary<string, string>(segment.ConstruireSettings()));

        var index = Segments.IndexOf(segment);
        _repository.AjouterSegment(_showId, duplicated, index + 2);
        Segments.Insert(index + 1, new SegmentViewModel(duplicated));

        this.RaisePropertyChanged(nameof(TotalDuration));
        this.RaisePropertyChanged(nameof(SegmentCount));

        Logger.Debug($"Segment dupliqué : {segment.SegmentId} → {duplicated.SegmentId}");
    }

    /// <summary>
    /// Valide le booking complet.
    /// </summary>
    public void ValidateBooking()
    {
        if (_context is null || string.IsNullOrWhiteSpace(_showId))
        {
            return;
        }

        try
        {
            var plan = BuildBookingPlan();
            var result = _validator.ValiderBooking(plan);

            ValidationIssues.Clear();
            foreach (var issue in result.Issues)
            {
                ValidationIssues.Add(new BookingIssueViewModel(
                    issue.Code,
                    issue.Message,
                    issue.Severite,
                    issue.SegmentId,
                    issue.Severite == ValidationSeverity.Erreur ? "Corriger" : "Ignorer"));
            }

            var errors = ValidationIssues.Where(i => i.Severity == ValidationSeverity.Erreur).ToList();
            var warnings = ValidationIssues.Where(i => i.Severity == ValidationSeverity.Avertissement).ToList();

            ValidationErrors = errors.Any()
                ? $"{errors.Count} erreur(s)"
                : null;

            ValidationWarnings = warnings.Any()
                ? $"{warnings.Count} avertissement(s)"
                : null;

            this.RaisePropertyChanged(nameof(CanSimulate));

            Logger.Debug($"Validation : {errors.Count} erreurs, {warnings.Count} avertissements");
        }
        catch (Exception ex)
        {
            Logger.Error("Erreur lors de la validation du booking", ex);
        }
    }

    /// <summary>
    /// Simule le show et génère les résultats.
    /// </summary>
    public void SimulateShow()
    {
        if (_context is null || string.IsNullOrWhiteSpace(_showId))
        {
            Logger.Warning("Impossible de simuler : contexte non chargé");
            return;
        }

        if (!CanSimulate)
        {
            Logger.Warning("Impossible de simuler : erreurs de validation présentes");
            return;
        }

        Logger.Info("Simulation du show...");

        try
        {
            var orchestrator = new ShowDayOrchestrator(null, null);
            var result = orchestrator.SimulerShow(_context);

            Results.Clear();
            var workerNames = _context.Workers.ToDictionary(w => w.WorkerId, w => w.NomComplet);
            foreach (var segmentResult in result.RapportShow.Segments)
            {
                Results.Add(new SegmentResultViewModel(segmentResult, workerNames));
            }

            UpdateAnalysis(result);

            Logger.Info($"Simulation terminée : Note globale {result.RapportShow.NoteGlobale}/100");
        }
        catch (Exception ex)
        {
            Logger.Error("Erreur lors de la simulation du show", ex);
        }
    }

    /// <summary>
    /// Génère un booking automatique en utilisant le BookerAIEngine.
    /// Le Booker complète les slots vides selon ses préférences et mémoires.
    /// </summary>
    public void GenerateAutoBooking()
    {
        if (_context is null || string.IsNullOrWhiteSpace(_showId))
        {
            Logger.Warning("Impossible de générer un auto-booking : contexte non chargé");
            return;
        }

        if (_bookerAIEngine is null)
        {
            Logger.Warning("BookerAIEngine non disponible pour l'auto-booking");
            return;
        }

        Logger.Info("Génération du booking automatique...");

        try
        {
            // Récupérer le booker de la compagnie
            // Note: Cette méthode devrait être disponible dans GameRepository
            // Pour l'instant, on utilise un ID de booker par défaut ou on demande au joueur de configurer un booker
            var bookerId = "BOOKER-DEFAULT"; // TODO: Récupérer depuis la configuration de la compagnie

            if (string.IsNullOrWhiteSpace(bookerId))
            {
                Logger.Warning("Aucun booker trouvé pour cette compagnie");
                return;
            }

            // Préparer les contraintes par défaut
            var constraints = new AutoBookingConstraints
            {
                ForbidInjuredWorkers = true,
                MaxFatigueLevel = 80,
                MinSegments = 4,
                MaxSegments = 8,
                ForbidMultipleAppearances = true,
                PrioritizeActiveStorylines = true,
                UseTitles = true,
                RequireMainEvent = true,
                TargetDuration = _context.Show.DureeMinutes
            };

            // Récupérer les segments existants
            var existingSegments = Segments
                .Select(s => new SegmentDefinition(
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
                    s.ConstruireSettings()
                ))
                .ToList();

            // Phase 1.2 - Générer les nouveaux segments selon le niveau de contrôle
            List<SegmentDefinition> generatedSegments;
            if (_bookingControlService != null)
            {
                generatedSegments = _bookingControlService.GenerateShowWithControlLevel(
                    ControlLevel,
                    bookerId,
                    _context,
                    existingSegments,
                    constraints);
            }
            else if (_bookerAIEngine != null)
            {
                // Fallback sur BookerAIEngine si BookingControlService non disponible
                generatedSegments = _bookerAIEngine.GenerateAutoBooking(
                    bookerId,
                    _context,
                    existingSegments,
                    constraints);
            }
            else
            {
                Logger.Warning("Aucun service de booking disponible");
                return;
            }

            // Ajouter les segments générés à la liste
            foreach (var segment in generatedSegments)
            {
                _repository.AjouterSegment(_showId, segment, Segments.Count + 1);
                Segments.Add(new SegmentViewModel(segment));
            }

            // Valider le booking
            ValidateBooking();

            // Rafraîchir les propriétés
            this.RaisePropertyChanged(nameof(TotalDuration));
            this.RaisePropertyChanged(nameof(SegmentCount));
            this.RaisePropertyChanged(nameof(HasSegments));

            Logger.Info($"Auto-booking généré : {generatedSegments.Count} segments ajoutés");
        }
        catch (Exception ex)
        {
            Logger.Error("Erreur lors de la génération du booking automatique", ex);
        }
    }

    /// <summary>
    /// Phase 1.2 - Charge le niveau de contrôle depuis GameState
    /// </summary>
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
                this.RaisePropertyChanged(nameof(CanAutoBook));
                this.RaisePropertyChanged(nameof(ControlLevelDescription));
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur chargement niveau contrôle: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase 1.2 - Sauvegarde le niveau de contrôle dans GameState
    /// </summary>
    private void SaveBookingControlLevel()
    {
        if (_settingsRepository == null) return;

        try
        {
            _settingsRepository.SauvegarderBookingControlLevel(ControlLevel.ToString());
            Logger.Debug($"Niveau de contrôle sauvegardé: {ControlLevel}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur sauvegarde niveau contrôle: {ex.Message}");
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Construit un BookingPlan à partir des segments actuels.
    /// </summary>
    private BookingPlan BuildBookingPlan()
    {
        if (string.IsNullOrWhiteSpace(_showId))
        {
            throw new InvalidOperationException("ShowId non défini");
        }

        var segmentContexts = Segments.Select(seg => new SegmentSimulationContext(
            seg.SegmentId,
            seg.TypeSegment,
            seg.Participants.Select(p => p.WorkerId).ToList(),
            seg.DureeMinutes,
            seg.EstMainEvent,
            seg.StorylineId,
            seg.TitreId,
            seg.Intensite,
            seg.VainqueurId,
            seg.PerdantId
        )).ToList();

        return new BookingPlan(_showId, segmentContexts);
    }

    private void LoadSegmentTypes()
    {
        SegmentTypes.Clear();
        foreach (var type in _catalog.Labels)
        {
            SegmentTypes.Add(new SegmentTypeOptionViewModel(type.Key, type.Value));
        }
    }

    private void LoadTemplates()
    {
        Templates.Clear();
        try
        {
            var templates = _templateService.LoadTemplates();
            foreach (var template in templates)
            {
                Templates.Add(new SegmentTemplateViewModel(template));
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Impossible de charger les templates : {ex.Message}");
        }
    }

    private void LoadMatchTypes()
    {
        MatchTypes.Clear();
        MatchTypes.Add(new MatchTypeViewModel("Singles", "Simple", "Match 1 contre 1", true, 1));
        MatchTypes.Add(new MatchTypeViewModel("Tag", "Tag Team", "Match par équipes de 2", true, 2));
        MatchTypes.Add(new MatchTypeViewModel("Triple", "Triple Threat", "Match à 3 combattants", true, 3));
        MatchTypes.Add(new MatchTypeViewModel("Fatal4", "Fatal 4-Way", "Match à 4 combattants", true, 4));
        MatchTypes.Add(new MatchTypeViewModel("Battle", "Battle Royal", "Bataille royale multi-participants", true, 5));
        MatchTypes.Add(new MatchTypeViewModel("Ladder", "Ladder Match", "Match avec échelles", true, 6));
        MatchTypes.Add(new MatchTypeViewModel("Cage", "Cage Match", "Match en cage", true, 7));
    }

    private void SaveSegmentOrder()
    {
        if (_context is null || string.IsNullOrWhiteSpace(_showId)) return;

        for (int i = 0; i < Segments.Count; i++)
        {
            try
            {
                _repository.MettreAJourOrdreSegment(Segments[i].SegmentId, i + 1);
            }
            catch (Exception ex)
            {
                Logger.Error($"Erreur mise à jour ordre segment {Segments[i].SegmentId}", ex);
            }
        }
    }

    private void UpdateAnalysis(ShowSimulationResult result)
    {
        // TODO: Les propriétés WhyNote, Tips, Guidelines n'existent pas encore sur ShowSimulationResult
        // Pour l'instant, on vide les collections
        WhyNote.Clear();
        Tips.Clear();
        BookingGuidelines.Clear();

        // Placeholder: Ajouter des analyses basiques basées sur la note globale
        var noteGlobale = result.RapportShow.NoteGlobale;
        if (noteGlobale >= 80)
        {
            WhyNote.Add("Excellent show avec une note globale élevée");
        }
        else if (noteGlobale >= 60)
        {
            WhyNote.Add("Show correct avec des points à améliorer");
        }
        else
        {
            WhyNote.Add("Show décevant nécessitant des ajustements");
        }
    }

    #endregion
}
