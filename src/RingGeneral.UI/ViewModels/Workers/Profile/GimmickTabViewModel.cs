using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Workers.Profile;

/// <summary>
/// ViewModel for Gimmick/Push tab.
/// Manages gimmick, alignment, push level, and specializations.
/// </summary>
public sealed class GimmickTabViewModel : ViewModelBase
{
    private readonly INotesRepository _repository;

    private int _workerId;
    private ObservableCollection<WorkerSpecialization> _specializations = new();
    private WorkerSpecialization? _selectedSpecialization;

    public GimmickTabViewModel(INotesRepository repository)
    {
        _repository = repository;

        // Commands
        AddSpecializationCommand = ReactiveCommand.Create(AddSpecialization);
        RemoveSpecializationCommand = ReactiveCommand.Create(RemoveSpecialization);
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
    /// Worker specializations (Brawler, Technical, etc.)
    /// </summary>
    public ObservableCollection<WorkerSpecialization> Specializations
    {
        get => _specializations;
        private set => this.RaiseAndSetIfChanged(ref _specializations, value);
    }

    /// <summary>
    /// Selected specialization
    /// </summary>
    public WorkerSpecialization? SelectedSpecialization
    {
        get => _selectedSpecialization;
        set => this.RaiseAndSetIfChanged(ref _selectedSpecialization, value);
    }

    /// <summary>
    /// Primary specialization (Level 1)
    /// </summary>
    public WorkerSpecialization? PrimarySpecialization =>
        Specializations.FirstOrDefault(s => s.Level == 1);

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> AddSpecializationCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveSpecializationCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    public void LoadWorker(int workerId)
    {
        WorkerId = workerId;

        // Load specializations
        var specs = _repository.GetSpecializations(workerId);
        Specializations = new ObservableCollection<WorkerSpecialization>(specs);

        this.RaisePropertyChanged(nameof(PrimarySpecialization));
    }

    // ====================================================================
    // PRIVATE METHODS
    // ====================================================================

    private void AddSpecialization()
    {
        System.Console.WriteLine("[GimmickTab] Add specialization dialog");
        // TODO: Show add specialization dialog
    }

    private void RemoveSpecialization()
    {
        if (SelectedSpecialization == null) return;
        _repository.DeleteSpecialization(SelectedSpecialization.Id);
        LoadWorker(WorkerId);
    }
}
