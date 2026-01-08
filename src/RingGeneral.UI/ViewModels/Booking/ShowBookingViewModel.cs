using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using RingGeneral.Core.Simulation;
using RingGeneral.Core.Validation;
using RingGeneral.Data.Repositories;
using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;

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
    private ShowContext? _context;
    private string? _showId;

    public ShowBookingViewModel(
        GameRepository repository,
        SegmentTypeCatalog catalog)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _validator = new BookingValidator();
        _builder = new BookingBuilderService();
        _templateService = new TemplateService();

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

        // Commandes
        AddSegmentCommand = ReactiveCommand.Create(AddSegment);
        RemoveSegmentCommand = ReactiveCommand.Create<SegmentViewModel>(RemoveSegment);
        MoveSegmentUpCommand = ReactiveCommand.Create<SegmentViewModel>(MoveSegmentUp);
        MoveSegmentDownCommand = ReactiveCommand.Create<SegmentViewModel>(MoveSegmentDown);
        DuplicateSegmentCommand = ReactiveCommand.Create<SegmentViewModel>(DuplicateSegment);
        SimulateShowCommand = ReactiveCommand.Create(SimulateShow);
        ValidateBookingCommand = ReactiveCommand.Create(ValidateBooking);

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

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> AddSegmentCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> RemoveSegmentCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> MoveSegmentUpCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> MoveSegmentDownCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> DuplicateSegmentCommand { get; }
    public ReactiveCommand<Unit, Unit> SimulateShowCommand { get; }
    public ReactiveCommand<Unit, Unit> ValidateBookingCommand { get; }

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
            var plan = BuildBookingPlan();
            var engine = new SimulationEngine();
            var result = engine.SimulerShow(plan, _context);

            Results.Clear();
            var workerNames = _context.Workers.ToDictionary(w => w.WorkerId, w => w.NomComplet);
            foreach (var segmentResult in result.RapportShow.Segments)
            {
                Results.Add(new SegmentResultViewModel(segmentResult, workerNames));
            }

            UpdateAnalysis(result);

            Logger.Info($"Simulation terminée : Note globale {result.NoteGlobale}/100");
        }
        catch (Exception ex)
        {
            Logger.Error("Erreur lors de la simulation du show", ex);
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
        WhyNote.Clear();
        foreach (var reason in result.WhyNote ?? Array.Empty<string>())
        {
            WhyNote.Add(reason);
        }

        Tips.Clear();
        foreach (var tip in result.Tips ?? Array.Empty<string>())
        {
            Tips.Add(tip);
        }

        BookingGuidelines.Clear();
        if (result.Guidelines is not null)
        {
            foreach (var guideline in result.Guidelines)
            {
                BookingGuidelines.Add(guideline);
            }
        }
    }

    #endregion
}
