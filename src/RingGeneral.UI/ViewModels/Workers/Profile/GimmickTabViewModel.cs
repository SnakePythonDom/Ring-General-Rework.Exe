using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Interfaces;

namespace RingGeneral.UI.ViewModels.Workers.Profile;

/// <summary>
/// ViewModel for Gimmick/Push tab.
/// Manages gimmick selection, alignment, push level, specializations, and history.
/// </summary>
public sealed class GimmickTabViewModel : ViewModelBase
{
    private readonly INotesRepository _notesRepository;
    private readonly IGimmickRepository _gimmickRepository;
    private readonly IGimmickService _gimmickService;

    private int _workerId;
    private Worker? _worker;

    // Specializations
    private ObservableCollection<WorkerSpecialization> _specializations = new();
    private WorkerSpecialization? _selectedSpecialization;

    // Gimmicks
    private ObservableCollection<Gimmick> _availableGimmicks = new();
    private ObservableCollection<GimmickCategoryInfo> _categories = new();
    private ObservableCollection<GimmickHistory> _gimmickHistory = new();
    private ObservableCollection<GimmickRecommendation> _recommendations = new();

    private Gimmick? _selectedGimmick;
    private GimmickCategoryInfo? _selectedCategory;
    private GimmickHistory? _currentGimmick;
    private string _searchText = string.Empty;
    private string _selectedAlignment = "Any";
    private bool _isPickerOpen;

    public GimmickTabViewModel(
        INotesRepository notesRepository,
        IGimmickRepository gimmickRepository,
        IGimmickService gimmickService)
    {
        _notesRepository = notesRepository;
        _gimmickRepository = gimmickRepository;
        _gimmickService = gimmickService;

        // Initialize categories
        Categories = new ObservableCollection<GimmickCategoryInfo>(_gimmickRepository.GetAllCategories());

        // Commands
        AddSpecializationCommand = ReactiveCommand.Create(AddSpecialization);
        RemoveSpecializationCommand = ReactiveCommand.Create(RemoveSpecialization);
        OpenGimmickPickerCommand = ReactiveCommand.Create(OpenGimmickPicker);
        CloseGimmickPickerCommand = ReactiveCommand.Create(CloseGimmickPicker);
        AssignGimmickCommand = ReactiveCommand.Create(AssignSelectedGimmick);
        AssignCustomGimmickCommand = ReactiveCommand.Create<string>(AssignCustomGimmick);
        RefreshRecommendationsCommand = ReactiveCommand.Create(RefreshRecommendations);

        // React to category changes
        this.WhenAnyValue(x => x.SelectedCategory)
            .Subscribe(_ => FilterGimmicks());

        // React to alignment filter changes
        this.WhenAnyValue(x => x.SelectedAlignment)
            .Subscribe(_ => FilterGimmicks());

        // React to search text changes
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => FilterGimmicks());
    }

    // ====================================================================
    // WORKER PROPERTIES
    // ====================================================================

    public int WorkerId
    {
        get => _workerId;
        private set => this.RaiseAndSetIfChanged(ref _workerId, value);
    }

    public Worker? Worker
    {
        get => _worker;
        private set => this.RaiseAndSetIfChanged(ref _worker, value);
    }

    // ====================================================================
    // SPECIALIZATION PROPERTIES
    // ====================================================================

    public ObservableCollection<WorkerSpecialization> Specializations
    {
        get => _specializations;
        private set => this.RaiseAndSetIfChanged(ref _specializations, value);
    }

    public WorkerSpecialization? SelectedSpecialization
    {
        get => _selectedSpecialization;
        set => this.RaiseAndSetIfChanged(ref _selectedSpecialization, value);
    }

    public WorkerSpecialization? PrimarySpecialization =>
        Specializations.FirstOrDefault(s => s.Level == 1);

    // ====================================================================
    // GIMMICK PROPERTIES
    // ====================================================================

    public ObservableCollection<Gimmick> AvailableGimmicks
    {
        get => _availableGimmicks;
        private set => this.RaiseAndSetIfChanged(ref _availableGimmicks, value);
    }

    public ObservableCollection<GimmickCategoryInfo> Categories
    {
        get => _categories;
        private set => this.RaiseAndSetIfChanged(ref _categories, value);
    }

    public ObservableCollection<GimmickHistory> GimmickHistory
    {
        get => _gimmickHistory;
        private set => this.RaiseAndSetIfChanged(ref _gimmickHistory, value);
    }

    public ObservableCollection<GimmickRecommendation> Recommendations
    {
        get => _recommendations;
        private set => this.RaiseAndSetIfChanged(ref _recommendations, value);
    }

    public Gimmick? SelectedGimmick
    {
        get => _selectedGimmick;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedGimmick, value);
            this.RaisePropertyChanged(nameof(CanAssignGimmick));
            this.RaisePropertyChanged(nameof(SelectedGimmickPotential));
            this.RaisePropertyChanged(nameof(SelectedGimmickAcceptance));
        }
    }

    public GimmickCategoryInfo? SelectedCategory
    {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    public GimmickHistory? CurrentGimmick
    {
        get => _currentGimmick;
        private set => this.RaiseAndSetIfChanged(ref _currentGimmick, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public string SelectedAlignment
    {
        get => _selectedAlignment;
        set => this.RaiseAndSetIfChanged(ref _selectedAlignment, value);
    }

    public bool IsPickerOpen
    {
        get => _isPickerOpen;
        set => this.RaiseAndSetIfChanged(ref _isPickerOpen, value);
    }

    // ====================================================================
    // CALCULATED PROPERTIES
    // ====================================================================

    public bool CanAssignGimmick => SelectedGimmick != null && Worker != null;

    public int SelectedGimmickPotential
    {
        get
        {
            if (SelectedGimmick == null || Worker == null)
                return 0;
            return _gimmickService.CalculateGimmickPotential(Worker, SelectedGimmick);
        }
    }

    public GimmickAcceptanceResult? SelectedGimmickAcceptance
    {
        get
        {
            if (SelectedGimmick == null || Worker == null)
                return null;
            return _gimmickService.CheckGimmickAcceptance(Worker, SelectedGimmick);
        }
    }

    public string CurrentGimmickName => CurrentGimmick?.GimmickName ?? "No Gimmick";
    public int CurrentGimmickDuration => CurrentGimmick?.DurationInWeeks ?? 0;
    public string CurrentGimmickGrade => CurrentGimmick?.SuccessGrade ?? "-";

    public int GimmickEntertainmentBonus =>
        Worker != null ? _gimmickService.CalculateGimmickEntertainmentBonus(Worker) : 0;

    public int GimmickCrowdBonus =>
        Worker != null ? _gimmickService.CalculateGimmickCrowdBonus(Worker, false) : 0;

    public int TotalGimmickCount => _gimmickRepository.GetTotalGimmickCount();

    public string[] AlignmentOptions => new[] { "Any", "Face", "Heel", "Tweener" };

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> AddSpecializationCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveSpecializationCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenGimmickPickerCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseGimmickPickerCommand { get; }
    public ReactiveCommand<Unit, Unit> AssignGimmickCommand { get; }
    public ReactiveCommand<string, Unit> AssignCustomGimmickCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshRecommendationsCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    public void LoadWorker(int workerId, Worker? worker = null)
    {
        WorkerId = workerId;
        Worker = worker;

        // Load specializations
        var specs = _notesRepository.GetSpecializations(workerId);
        Specializations = new ObservableCollection<WorkerSpecialization>(specs);

        // Load current gimmick
        CurrentGimmick = _gimmickRepository.GetCurrentGimmick(workerId);

        // Load gimmick history
        var history = _gimmickRepository.GetWorkerGimmickHistory(workerId);
        GimmickHistory = new ObservableCollection<GimmickHistory>(history);

        // Load recommendations
        RefreshRecommendations();

        // Notify UI
        this.RaisePropertyChanged(nameof(PrimarySpecialization));
        this.RaisePropertyChanged(nameof(CurrentGimmickName));
        this.RaisePropertyChanged(nameof(CurrentGimmickDuration));
        this.RaisePropertyChanged(nameof(CurrentGimmickGrade));
        this.RaisePropertyChanged(nameof(GimmickEntertainmentBonus));
        this.RaisePropertyChanged(nameof(GimmickCrowdBonus));
    }

    // ====================================================================
    // PRIVATE METHODS
    // ====================================================================

    private void FilterGimmicks()
    {
        IEnumerable<Gimmick> gimmicks;

        // Search by text first
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            gimmicks = _gimmickRepository.SearchGimmicks(SearchText);
        }
        // Filter by category
        else if (SelectedCategory != null)
        {
            gimmicks = _gimmickRepository.GetGimmicksByCategory(SelectedCategory.CategoryId);
        }
        else
        {
            gimmicks = _gimmickRepository.GetAllGimmicks().Take(100); // Limit for performance
        }

        // Apply alignment filter
        if (SelectedAlignment != "Any")
        {
            gimmicks = gimmicks.Where(g =>
                g.PreferredAlignment == SelectedAlignment ||
                g.PreferredAlignment == "Any");
        }

        AvailableGimmicks = new ObservableCollection<Gimmick>(gimmicks.Take(100));
    }

    private void OpenGimmickPicker()
    {
        IsPickerOpen = true;
        FilterGimmicks(); // Load initial gimmicks
    }

    private void CloseGimmickPicker()
    {
        IsPickerOpen = false;
        SelectedGimmick = null;
        SearchText = string.Empty;
        SelectedCategory = null;
    }

    private void AssignSelectedGimmick()
    {
        if (SelectedGimmick == null || Worker == null)
            return;

        var result = _gimmickService.AssignGimmick(
            WorkerId,
            SelectedGimmick.GimmickId,
            "Player Assignment"
        );

        if (result.Success)
        {
            Logger.Info(result.Message);
            CloseGimmickPicker();
            LoadWorker(WorkerId, Worker); // Refresh
        }
        else
        {
            Logger.Warning(result.Message);
            // Could show dialog with option to force assign
        }
    }

    private void AssignCustomGimmick(string gimmickName)
    {
        if (string.IsNullOrWhiteSpace(gimmickName) || Worker == null)
            return;

        var result = _gimmickService.AssignCustomGimmick(
            WorkerId,
            gimmickName,
            "Custom Player Assignment"
        );

        if (result.Success)
        {
            Logger.Info(result.Message);
            LoadWorker(WorkerId, Worker); // Refresh
        }
        else
        {
            Logger.Warning(result.Message);
        }
    }

    private void RefreshRecommendations()
    {
        if (WorkerId <= 0)
            return;

        var recs = _gimmickService.GetRecommendations(WorkerId, 5);
        Recommendations = new ObservableCollection<GimmickRecommendation>(recs);
    }

    private void AddSpecialization()
    {
        Logger.Info("Add specialization dialog");
        // TODO: Show add specialization dialog
    }

    private void RemoveSpecialization()
    {
        if (SelectedSpecialization == null) return;
        _notesRepository.DeleteSpecialization(SelectedSpecialization.Id);
        LoadWorker(WorkerId, Worker);
    }
}
