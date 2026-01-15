using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Interfaces;

namespace RingGeneral.UI.ViewModels.Dialogs;

/// <summary>
/// ViewModel for gimmick picker dialog.
/// Allows browsing, filtering, and selecting gimmicks.
/// </summary>
public sealed class GimmickPickerViewModel : ViewModelBase
{
    private readonly IGimmickRepository _gimmickRepository;
    private readonly IGimmickService _gimmickService;

    private Worker? _worker;
    private ObservableCollection<Gimmick> _gimmicks = new();
    private ObservableCollection<GimmickCategoryInfo> _categories = new();
    private ObservableCollection<GimmickRecommendation> _recommendations = new();

    private Gimmick? _selectedGimmick;
    private GimmickCategoryInfo? _selectedCategory;
    private string _searchText = string.Empty;
    private string _selectedAlignment = "Any";
    private string _selectedTier = "Any";
    private bool _showOnlyMatchingStyle;
    private string _customGimmickName = string.Empty;

    public GimmickPickerViewModel(IGimmickRepository gimmickRepository, IGimmickService gimmickService)
    {
        _gimmickRepository = gimmickRepository;
        _gimmickService = gimmickService;

        // Load categories
        var categories = _gimmickRepository.GetAllCategories();
        Categories = new ObservableCollection<GimmickCategoryInfo>(categories);

        // Commands
        SelectGimmickCommand = ReactiveCommand.Create(SelectGimmick);
        CancelCommand = ReactiveCommand.Create(Cancel);
        ApplyFiltersCommand = ReactiveCommand.Create(ApplyFilters);
        ClearFiltersCommand = ReactiveCommand.Create(ClearFilters);
        UseCustomGimmickCommand = ReactiveCommand.Create(UseCustomGimmick);

        // React to search text changes with debounce
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ApplyFilters());

        // React to category changes
        this.WhenAnyValue(x => x.SelectedCategory)
            .Subscribe(_ => ApplyFilters());

        // React to alignment changes
        this.WhenAnyValue(x => x.SelectedAlignment)
            .Subscribe(_ => ApplyFilters());
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    public Worker? Worker
    {
        get => _worker;
        set
        {
            this.RaiseAndSetIfChanged(ref _worker, value);
            LoadRecommendations();
        }
    }

    public ObservableCollection<Gimmick> Gimmicks
    {
        get => _gimmicks;
        private set => this.RaiseAndSetIfChanged(ref _gimmicks, value);
    }

    public ObservableCollection<GimmickCategoryInfo> Categories
    {
        get => _categories;
        private set => this.RaiseAndSetIfChanged(ref _categories, value);
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
            this.RaisePropertyChanged(nameof(CanSelect));
            this.RaisePropertyChanged(nameof(GimmickPotential));
            this.RaisePropertyChanged(nameof(AcceptanceChance));
            this.RaisePropertyChanged(nameof(AcceptanceMessage));
            this.RaisePropertyChanged(nameof(GimmickDetails));
        }
    }

    public GimmickCategoryInfo? SelectedCategory
    {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
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

    public string SelectedTier
    {
        get => _selectedTier;
        set => this.RaiseAndSetIfChanged(ref _selectedTier, value);
    }

    public bool ShowOnlyMatchingStyle
    {
        get => _showOnlyMatchingStyle;
        set
        {
            this.RaiseAndSetIfChanged(ref _showOnlyMatchingStyle, value);
            ApplyFilters();
        }
    }

    public string CustomGimmickName
    {
        get => _customGimmickName;
        set => this.RaiseAndSetIfChanged(ref _customGimmickName, value);
    }

    // ====================================================================
    // CALCULATED PROPERTIES
    // ====================================================================

    public bool CanSelect => SelectedGimmick != null;

    public int GimmickPotential
    {
        get
        {
            if (SelectedGimmick == null || Worker == null)
                return 0;
            return _gimmickService.CalculateGimmickPotential(Worker, SelectedGimmick);
        }
    }

    public int AcceptanceChance
    {
        get
        {
            if (SelectedGimmick == null || Worker == null)
                return 0;
            return _gimmickService.CheckGimmickAcceptance(Worker, SelectedGimmick).AcceptanceChance;
        }
    }

    public string AcceptanceMessage
    {
        get
        {
            if (SelectedGimmick == null || Worker == null)
                return string.Empty;
            return _gimmickService.CheckGimmickAcceptance(Worker, SelectedGimmick).Message;
        }
    }

    public string GimmickDetails
    {
        get
        {
            if (SelectedGimmick == null)
                return string.Empty;

            var g = SelectedGimmick;
            return $"Category: {g.Category}\n" +
                   $"Alignment: {g.PreferredAlignment}\n" +
                   $"Tier: {g.PopularityTier}\n" +
                   $"Entertainment: {(g.EntertainmentModifier >= 0 ? "+" : "")}{g.EntertainmentModifier}\n" +
                   $"Crowd Reaction: {(g.CrowdReactionModifier >= 0 ? "+" : "")}{g.CrowdReactionModifier}";
        }
    }

    public int TotalGimmicks => _gimmickRepository.GetTotalGimmickCount();
    public int FilteredCount => Gimmicks.Count;

    public string[] AlignmentOptions => new[] { "Any", "Face", "Heel", "Tweener" };
    public string[] TierOptions => new[] { "Any", "MainEvent", "UpperMid", "MidCard", "LowerMid", "Jobber" };

    // ====================================================================
    // RESULT
    // ====================================================================

    public Gimmick? ResultGimmick { get; private set; }
    public string? ResultCustomName { get; private set; }
    public bool DialogResult { get; private set; }

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> SelectGimmickCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }
    public ReactiveCommand<Unit, Unit> UseCustomGimmickCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    public void Initialize(Worker worker)
    {
        Worker = worker;
        ApplyFilters();
    }

    // ====================================================================
    // PRIVATE METHODS
    // ====================================================================

    private void LoadRecommendations()
    {
        if (Worker == null)
            return;

        var recs = _gimmickService.GetRecommendations(Worker.Id, 5);
        Recommendations = new ObservableCollection<GimmickRecommendation>(recs);
    }

    private void ApplyFilters()
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
            gimmicks = _gimmickRepository.GetAllGimmicks();
        }

        // Apply alignment filter
        if (SelectedAlignment != "Any")
        {
            gimmicks = gimmicks.Where(g =>
                g.PreferredAlignment == SelectedAlignment ||
                g.PreferredAlignment == "Any");
        }

        // Apply tier filter
        if (SelectedTier != "Any")
        {
            gimmicks = gimmicks.Where(g => g.PopularityTier == SelectedTier);
        }

        // Show only matching style
        if (ShowOnlyMatchingStyle && Worker?.PrimarySpecialization != null)
        {
            string workerCategory = MapSpecializationToCategory(Worker.PrimarySpecialization.Specialization.ToString());
            gimmicks = gimmicks.Where(g => g.Category == workerCategory);
        }

        Gimmicks = new ObservableCollection<Gimmick>(gimmicks.Take(100));
        this.RaisePropertyChanged(nameof(FilteredCount));
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedCategory = null;
        SelectedAlignment = "Any";
        SelectedTier = "Any";
        ShowOnlyMatchingStyle = false;
        ApplyFilters();
    }

    private void SelectGimmick()
    {
        if (SelectedGimmick == null)
            return;

        ResultGimmick = SelectedGimmick;
        DialogResult = true;
    }

    private void Cancel()
    {
        ResultGimmick = null;
        DialogResult = false;
    }

    private void UseCustomGimmick()
    {
        if (string.IsNullOrWhiteSpace(CustomGimmickName))
            return;

        ResultCustomName = CustomGimmickName;
        DialogResult = true;
    }

    private string MapSpecializationToCategory(string specialization)
    {
        return specialization switch
        {
            "Power" => "POWER",
            "Technical" => "TECHNICAL",
            "HighFlyer" => "HIGHFLYER",
            "Brawler" => "BRAWLER",
            "Showman" => "SHOWMAN",
            "Hardcore" => "HARDCORE",
            "AllRounder" => "ALLROUNDER",
            _ => "ALLROUNDER"
        };
    }
}
