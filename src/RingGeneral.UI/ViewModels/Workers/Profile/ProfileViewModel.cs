using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Workers.Profile;

/// <summary>
/// Main ViewModel for Worker ProfileView.
/// Manages all 6 tabs and worker data.
/// </summary>
public sealed class ProfileViewModel : ViewModelBase
{
    private readonly IWorkerAttributesRepository _attributesRepository;
    private readonly IRelationsRepository _relationsRepository;
    private readonly INotesRepository _notesRepository;

    private Worker? _currentWorker;
    private ViewModelBase? _selectedTabViewModel;
    private int _selectedTabIndex;

    public ProfileViewModel(
        IWorkerAttributesRepository attributesRepository,
        IRelationsRepository relationsRepository,
        INotesRepository notesRepository)
    {
        _attributesRepository = attributesRepository;
        _relationsRepository = relationsRepository;
        _notesRepository = notesRepository;

        // Initialize tab ViewModels
        AttributesTab = new AttributesTabViewModel(_attributesRepository);
        ContractsTab = new ContractsTabViewModel(_notesRepository);
        GimmickTab = new GimmickTabViewModel(_notesRepository);
        RelationsTab = new RelationsTabViewModel(_relationsRepository);
        HistoryTab = new HistoryTabViewModel(_notesRepository);
        NotesTab = new NotesTabViewModel(_notesRepository);

        // Tab collection for UI binding
        Tabs = new ObservableCollection<TabItem>
        {
            new TabItem("Attributs", AttributesTab),
            new TabItem("Contrats", ContractsTab),
            new TabItem("Gimmick", GimmickTab),
            new TabItem("Relations", RelationsTab),
            new TabItem("Historique", HistoryTab),
            new TabItem("Notes", NotesTab)
        };

        // Select first tab by default
        SelectedTabIndex = 0;

        // Commands
        RefreshCommand = ReactiveCommand.Create(Refresh);
        CloseProfileCommand = ReactiveCommand.Create(CloseProfile);
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    /// <summary>
    /// Current worker being displayed
    /// </summary>
    public Worker? CurrentWorker
    {
        get => _currentWorker;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentWorker, value);
            OnWorkerChanged(value);
        }
    }

    /// <summary>
    /// Selected tab index (0-5)
    /// </summary>
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
            if (value >= 0 && value < Tabs.Count)
            {
                SelectedTabViewModel = Tabs[value].ViewModel;
            }
        }
    }

    /// <summary>
    /// Currently selected tab ViewModel
    /// </summary>
    public ViewModelBase? SelectedTabViewModel
    {
        get => _selectedTabViewModel;
        private set => this.RaiseAndSetIfChanged(ref _selectedTabViewModel, value);
    }

    /// <summary>
    /// Tab collection for UI
    /// </summary>
    public ObservableCollection<TabItem> Tabs { get; }

    // ====================================================================
    // TAB VIEWMODELS
    // ====================================================================

    public AttributesTabViewModel AttributesTab { get; }
    public ContractsTabViewModel ContractsTab { get; }
    public GimmickTabViewModel GimmickTab { get; }
    public RelationsTabViewModel RelationsTab { get; }
    public HistoryTabViewModel HistoryTab { get; }
    public NotesTabViewModel NotesTab { get; }

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseProfileCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    /// <summary>
    /// Load a worker profile
    /// </summary>
    public void LoadWorker(Worker worker)
    {
        CurrentWorker = worker;
    }

    /// <summary>
    /// Load a worker by ID
    /// </summary>
    public void LoadWorkerById(int workerId)
    {
        // TODO: Load worker from repository
        // For now, this is a placeholder
        System.Console.WriteLine($"[ProfileViewModel] Loading worker ID: {workerId}");
    }

    /// <summary>
    /// Navigate to a specific tab
    /// </summary>
    public void NavigateToTab(int tabIndex)
    {
        if (tabIndex >= 0 && tabIndex < Tabs.Count)
        {
            SelectedTabIndex = tabIndex;
        }
    }

    // ====================================================================
    // PRIVATE METHODS
    // ====================================================================

    private void OnWorkerChanged(Worker? worker)
    {
        if (worker == null) return;

        // Load data for all tabs
        AttributesTab.LoadWorker(worker.Id);
        ContractsTab.LoadWorker(worker.Id);
        GimmickTab.LoadWorker(worker.Id);
        RelationsTab.LoadWorker(worker.Id);
        HistoryTab.LoadWorker(worker.Id);
        NotesTab.LoadWorker(worker.Id);
    }

    private void Refresh()
    {
        if (CurrentWorker != null)
        {
            // Reload current worker data
            OnWorkerChanged(CurrentWorker);
        }
    }

    private void CloseProfile()
    {
        // Close profile view (handled by navigation service)
        System.Console.WriteLine("[ProfileViewModel] Closing profile");
    }

    // ====================================================================
    // HELPER CLASS
    // ====================================================================

    public sealed class TabItem
    {
        public TabItem(string header, ViewModelBase viewModel)
        {
            Header = header;
            ViewModel = viewModel;
        }

        public string Header { get; }
        public ViewModelBase ViewModel { get; }
    }
}
