using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.ViewModels.Contracts;

namespace RingGeneral.UI.ViewModels.Youth;

public sealed class YouthTraineeManagementViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;
    private readonly string? _structureId;

    public ObservableCollection<YouthTraineeItemViewModel> Trainees { get; } = new();

    private ContractEditorViewModel? _currentContractEditor;
    public ContractEditorViewModel? CurrentContractEditor
    {
        get => _currentContractEditor;
        set => this.RaiseAndSetIfChanged(ref _currentContractEditor, value);
    }

    public ReactiveCommand<Unit, Unit> PromoteSelectedCommand { get; }
    public ReactiveCommand<Unit, Unit> CutSelectedCommand { get; }

    private Action<string>? _openProfileAction;

    public YouthTraineeManagementViewModel(YouthRepository youthRepository, GameRepository gameRepository, string? structureId = null, Action<string>? openProfile = null)
    {
        _youthRepository = youthRepository;
        _gameRepository = gameRepository;
        _structureId = structureId;
        _openProfileAction = openProfile;

        PromoteSelectedCommand = ReactiveCommand.Create(PromoteSelected);
        CutSelectedCommand = ReactiveCommand.Create(CutSelected);

        try
        {
            LoadTrainees();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading trainees: {ex.Message}");
        }
    }

    public void LoadTrainees()
    {
        Trainees.Clear();
        var trainees = _youthRepository.ChargerYouthTraineesPourProgression();

        if (!string.IsNullOrEmpty(_structureId))
        {
            trainees = trainees.Where(t => t.YouthId == _structureId).ToList();
        }

        var structures = _youthRepository.ChargerYouthStructures().ToDictionary(s => s.YouthId, s => s.Nom);

        foreach (var t in trainees)
        {
            var structureName = structures.TryGetValue(t.YouthId, out var sName) ? sName : t.YouthId;
            // Pass current week as 100 for now to calc TimeInDev
            Trainees.Add(new YouthTraineeItemViewModel(t, structureName, PromoteTrainee, ReleaseTrainee, 100, _openProfileAction));
        }
    }

    private void PromoteTrainee(string workerId)
    {
        // 1. Fetch Trainee Data (Mocking Worker object for now since we only have State)
        var trainee = Trainees.FirstOrDefault(t => t.WorkerId == workerId);
        if (trainee == null) return;

        // Create a transient Worker object for the contract editor
        // Important: Force Type to Wrestler as we are promoting them to the active roster
        var worker = new Worker
        {
            Name = trainee.Name,
            Type = WorkerType.Wrestler, // FORCE WRESTLER MODE
            PushLevel = PushLevel.LowerMid, // Default starting push
            Age = 20 // Default/Mock
        };

        // 2. Launch Editor
        CurrentContractEditor = new ContractEditorViewModel(worker, () =>
        {
            // On Save:
            try
            {
                // Execute actual promotion
                // Pass workerId and current week (placeholder 1)
                _youthRepository.DiplomerTrainee(workerId, 1);

                // TODO: Apply the contract terms (Salary, Push) to the newly created Wrestler record
                // This would require a dedicated method in Repository to update the worker after promotion
                // e.g. _workerRepository.UpdateContract(workerId, editor.Salary, editor.Push ...);

                LoadTrainees();
                CurrentContractEditor = null; // Close editor
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error promoting: {ex.Message}");
            }
        }, _youthRepository);

        // Handle Cancel (if ViewModel has a way to signal cancel, or just set null)
        // We can hook into CancelCommand if we exposed it or passed a check
        CurrentContractEditor.CancelCommand.Subscribe(_ => CurrentContractEditor = null);
    }

    private void ReleaseTrainee(string workerId)
    {
        try
        {
            _youthRepository.LicencierTrainee(workerId, 1);
            LoadTrainees();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error releasing trainee: {ex.Message}");
        }
    }

    private void PromoteSelected()
    {
        // Bulk promote simply bypasses the editor for now, or we could queue them
        // For simplicity/safety, we only support detailed editor for single promote
        // Or we could loop and auto-graduate. Let's auto-graduate for bulk.
        var selected = Trainees.Where(t => t.IsSelected).ToList();
        foreach (var t in selected)
        {
            try { _youthRepository.DiplomerTrainee(t.WorkerId, 1); } catch { }
        }
        LoadTrainees();
    }

    private void CutSelected()
    {
        var selected = Trainees.Where(t => t.IsSelected).ToList();
        foreach (var t in selected)
        {
            try { _youthRepository.LicencierTrainee(t.WorkerId, 1); } catch { }
        }
        LoadTrainees();
    }
}

public sealed class YouthTraineeItemViewModel : ViewModelBase
{
    public string WorkerId { get; }
    public string Name { get; }
    public string StructureName { get; }
    public int InRing { get; }
    public int Entertainment { get; }
    public int Story { get; }
    public string Statut { get; }
    public int SemaineInscription { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public int Average => (InRing + Entertainment + Story) / 3;

    // Calculated / Mock Columns
    public string OvrDisplay => Average.ToString();
    public string AgeDisplay => "19"; // Mock
    public string StyleDisplay => InRing > Entertainment ? "Technical" : "Entertainer";
    public string PotentialDisplay => Average > 60 ? "A" : (Average > 40 ? "B" : "C");
    public string MoraleDisplay => "Happy"; // Mock
    public string TimeInDevDisplay => $"{(100 - SemaineInscription)} wks"; // using 100 as current week placeholder

    public System.Windows.Input.ICommand PromoteCommand { get; }
    public System.Windows.Input.ICommand ReleaseCommand { get; }
    public System.Windows.Input.ICommand ViewProfileCommand { get; }

    public YouthTraineeItemViewModel(
        YouthTraineeProgressionState state,
        string structureName,
        Action<string> promoteAction,
        Action<string> releaseAction,
        int currentWeek,
        Action<string>? openProfileAction = null)
    {
        WorkerId = state.WorkerId;
        Name = state.Nom;
        StructureName = structureName;
        InRing = state.InRing;
        Entertainment = state.Entertainment;
        Story = state.Story;
        Statut = state.Statut;
        SemaineInscription = state.SemaineInscription;

        PromoteCommand = ReactiveCommand.Create(() => promoteAction(WorkerId));
        ReleaseCommand = ReactiveCommand.Create(() => releaseAction(WorkerId));
        ViewProfileCommand = ReactiveCommand.Create(() =>
        {
            System.Diagnostics.Debug.WriteLine($"[YouthTraineeItemViewModel] ViewProfileCommand executed for WorkerId: {WorkerId}");
            if (openProfileAction == null)
            {
                System.Diagnostics.Debug.WriteLine($"[YouthTraineeItemViewModel] openProfileAction is NULL!");
            }
            openProfileAction?.Invoke(WorkerId);
        });
    }
}
