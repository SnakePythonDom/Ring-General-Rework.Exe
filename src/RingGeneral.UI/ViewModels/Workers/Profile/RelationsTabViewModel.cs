using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models.Relations;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Workers.Profile;

/// <summary>
/// ViewModel for Relations tab.
/// Manages worker relations and faction memberships.
/// </summary>
public sealed class RelationsTabViewModel : ViewModelBase
{
    private readonly IRelationsRepository _repository;

    private int _workerId;
    private ObservableCollection<WorkerRelation> _relations = new();
    private ObservableCollection<FactionMember> _factionMemberships = new();
    private WorkerRelation? _selectedRelation;
    private FactionMember? _selectedFaction;

    public RelationsTabViewModel(IRelationsRepository repository)
    {
        _repository = repository;

        // Commands
        AddRelationCommand = ReactiveCommand.Create(AddRelation);
        EditRelationCommand = ReactiveCommand.Create(EditRelation);
        DeleteRelationCommand = ReactiveCommand.Create(DeleteRelation);
        AddToFactionCommand = ReactiveCommand.Create(AddToFaction);
        RemoveFromFactionCommand = ReactiveCommand.Create(RemoveFromFaction);
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    public int WorkerId
    {
        get => _workerId;
        private set => this.RaiseAndSetIfChanged(ref _workerId, value);
    }

    /// <summary>
    /// All relations for this worker
    /// </summary>
    public ObservableCollection<WorkerRelation> Relations
    {
        get => _relations;
        private set => this.RaiseAndSetIfChanged(ref _relations, value);
    }

    /// <summary>
    /// Faction memberships (current and historical)
    /// </summary>
    public ObservableCollection<FactionMember> FactionMemberships
    {
        get => _factionMemberships;
        private set => this.RaiseAndSetIfChanged(ref _factionMemberships, value);
    }

    /// <summary>
    /// Selected relation
    /// </summary>
    public WorkerRelation? SelectedRelation
    {
        get => _selectedRelation;
        set => this.RaiseAndSetIfChanged(ref _selectedRelation, value);
    }

    /// <summary>
    /// Selected faction membership
    /// </summary>
    public FactionMember? SelectedFaction
    {
        get => _selectedFaction;
        set => this.RaiseAndSetIfChanged(ref _selectedFaction, value);
    }

    /// <summary>
    /// Current active faction memberships
    /// </summary>
    public ObservableCollection<FactionMember> CurrentFactions =>
        new(FactionMemberships.Where(f => f.IsActiveMember));

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> AddRelationCommand { get; }
    public ReactiveCommand<Unit, Unit> EditRelationCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteRelationCommand { get; }
    public ReactiveCommand<Unit, Unit> AddToFactionCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveFromFactionCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    public void LoadWorker(int workerId)
    {
        WorkerId = workerId;

        // Load relations
        var relations = _repository.GetRelationsForWorker(workerId);
        Relations = new ObservableCollection<WorkerRelation>(relations);

        // Load faction memberships
        var factions = _repository.GetWorkerFactionHistory(workerId);
        FactionMemberships = new ObservableCollection<FactionMember>(factions);

        this.RaisePropertyChanged(nameof(CurrentFactions));
    }

    // ====================================================================
    // PRIVATE METHODS
    // ====================================================================

    private void AddRelation()
    {
        Logger.Info("Add relation dialog");
        // TODO: Show add relation dialog
    }

    private void EditRelation()
    {
        if (SelectedRelation == null) return;
        Logger.Info($"[RelationsTab] Edit relation {SelectedRelation.Id}");
        // TODO: Show edit relation dialog
    }

    private void DeleteRelation()
    {
        if (SelectedRelation == null) return;
        _repository.DeleteRelation(SelectedRelation.Id);
        LoadWorker(WorkerId);
    }

    private void AddToFaction()
    {
        Logger.Info("Add to faction dialog");
        // TODO: Show add to faction dialog
    }

    private void RemoveFromFaction()
    {
        if (SelectedFaction == null || !SelectedFaction.IsActiveMember) return;

        // TODO: Get current week/year from game state
        int currentWeek = 1;
        int currentYear = DateTime.Now.Year;

        _repository.RemoveFactionMember(SelectedFaction.Id, currentWeek, currentYear);
        LoadWorker(WorkerId);
    }
}
