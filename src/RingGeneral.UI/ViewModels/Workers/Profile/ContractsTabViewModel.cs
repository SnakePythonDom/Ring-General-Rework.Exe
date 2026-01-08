using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Workers.Profile;

/// <summary>
/// ViewModel for Contracts tab.
/// Manages contract history and current contract.
/// </summary>
public sealed class ContractsTabViewModel : ViewModelBase
{
    private readonly INotesRepository _repository;

    private int _workerId;
    private ContractHistory? _currentContract;
    private ObservableCollection<ContractHistory> _contractHistory = new();
    private ContractHistory? _selectedContract;

    public ContractsTabViewModel(INotesRepository repository)
    {
        _repository = repository;

        // Commands
        AddContractCommand = ReactiveCommand.Create(AddContract);
        EditContractCommand = ReactiveCommand.Create(EditContract);
        ExpireContractCommand = ReactiveCommand.Create(ExpireContract);
        TerminateContractCommand = ReactiveCommand.Create(TerminateContract);
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
    /// Current active contract
    /// </summary>
    public ContractHistory? CurrentContract
    {
        get => _currentContract;
        private set => this.RaiseAndSetIfChanged(ref _currentContract, value);
    }

    /// <summary>
    /// Contract history (all contracts)
    /// </summary>
    public ObservableCollection<ContractHistory> ContractHistory
    {
        get => _contractHistory;
        private set => this.RaiseAndSetIfChanged(ref _contractHistory, value);
    }

    /// <summary>
    /// Selected contract in the list
    /// </summary>
    public ContractHistory? SelectedContract
    {
        get => _selectedContract;
        set => this.RaiseAndSetIfChanged(ref _selectedContract, value);
    }

    /// <summary>
    /// Is worker under contract?
    /// </summary>
    public bool HasActiveContract => CurrentContract != null;

    /// <summary>
    /// Is contract expiring soon?
    /// </summary>
    public bool IsExpiringSoon => CurrentContract?.IsExpiringSoon ?? false;

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> AddContractCommand { get; }
    public ReactiveCommand<Unit, Unit> EditContractCommand { get; }
    public ReactiveCommand<Unit, Unit> ExpireContractCommand { get; }
    public ReactiveCommand<Unit, Unit> TerminateContractCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    public void LoadWorker(int workerId)
    {
        WorkerId = workerId;

        // Load current contract
        CurrentContract = _repository.GetActiveContract(workerId);

        // Load contract history
        var contracts = _repository.GetContractHistory(workerId);
        ContractHistory = new ObservableCollection<ContractHistory>(contracts);

        this.RaisePropertyChanged(nameof(HasActiveContract));
        this.RaisePropertyChanged(nameof(IsExpiringSoon));
    }

    // ====================================================================
    // PRIVATE METHODS
    // ====================================================================

    private void AddContract()
    {
        System.Console.WriteLine("[ContractsTab] Add contract dialog");
        // TODO: Show add contract dialog
    }

    private void EditContract()
    {
        if (SelectedContract == null) return;
        System.Console.WriteLine($"[ContractsTab] Edit contract {SelectedContract.Id}");
        // TODO: Show edit contract dialog
    }

    private void ExpireContract()
    {
        if (SelectedContract == null) return;
        _repository.ExpireContract(SelectedContract.Id);
        LoadWorker(WorkerId);
    }

    private void TerminateContract()
    {
        if (SelectedContract == null) return;
        _repository.TerminateContract(SelectedContract.Id);
        LoadWorker(WorkerId);
    }
}
