using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Contracts;

public sealed class ContractEditorViewModel : ViewModelBase
{
    private readonly Worker _worker;
    private readonly YouthRepository? _youthRepository; // Optional, for promotion logic
    private readonly Action _onSave;

    // polymorphism helpers
    public bool IsWrestler => _worker.IsWrestler;
    public bool IsStaff => _worker.IsStaff;
    public bool IsTrainee => _worker.IsTrainee;

    // Header Info
    public string HeaderTitle
    {
        get
        {
            if (IsWrestler) return "CONTRACT & PUSH MANAGEMENT";
            if (IsStaff) return "STAFF AGREEMENT";
            return "DEVELOPMENTAL DEAL";
        }
    }

    public string WorkerName => _worker.Name;

    // == COMMON TERMS ==
    private decimal _salary;
    public decimal Salary
    {
        get => _salary;
        set => this.RaiseAndSetIfChanged(ref _salary, value);
    }

    private int _durationMonths;
    public int DurationMonths
    {
        get => _durationMonths;
        set => this.RaiseAndSetIfChanged(ref _durationMonths, value);
    }

    // == WRESTLER SPECIFIC ==
    public ObservableCollection<PushLevel> AvailablePushLevels { get; }
    private PushLevel _selectedPush;
    public PushLevel SelectedPush
    {
        get => _selectedPush;
        set => this.RaiseAndSetIfChanged(ref _selectedPush, value);
    }

    // == STAFF SPECIFIC ==
    // Using string for roles for now, or could iterate an enum if we had one for staff roles
    public ObservableCollection<string> AvailableRoles { get; }
    private string _selectedRole;
    public string SelectedRole
    {
        get => _selectedRole;
        set => this.RaiseAndSetIfChanged(ref _selectedRole, value);
    }

    // == TRAINEE SPECIFIC ==
    // Maybe Weight Class target?
    public ObservableCollection<string> WeightClasses { get; } = new ObservableCollection<string> { "Lightweight", "Middleweight", "Heavyweight" };
    private string _targetWeightClass;
    public string TargetWeightClass
    {
        get => _targetWeightClass;
        set => this.RaiseAndSetIfChanged(ref _targetWeightClass, value);
    }

    // == CLAUSES ==
    public ObservableCollection<ContractClauseItem> Clauses { get; } = new();

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ContractEditorViewModel(Worker worker, Action onSave, YouthRepository? youthRepository = null)
    {
        _worker = worker;
        _onSave = onSave;
        _youthRepository = youthRepository;

        // Initialize Defaults
        _salary = 1000;
        _durationMonths = 12;

        AvailablePushLevels = new ObservableCollection<PushLevel>(Enum.GetValues<PushLevel>());
        _selectedPush = worker.PushLevel;

        AvailableRoles = new ObservableCollection<string> { "Referee", "Manager", "Announcer", "Road Agent", "Medic" };
        _selectedRole = "Referee"; // Default

        _targetWeightClass = "Middleweight";

        InitializeClauses();

        SaveCommand = ReactiveCommand.Create(Save);
        CancelCommand = ReactiveCommand.Create(() => { /* Close logic handled by parent */ });
    }

    private void InitializeClauses()
    {
        Clauses.Clear();
        if (IsWrestler)
        {
            Clauses.Add(new ContractClauseItem("Creative Control", false));
            Clauses.Add(new ContractClauseItem("Merch Cut (25%)", true));
            Clauses.Add(new ContractClauseItem("No-Compete Clause", false));
            Clauses.Add(new ContractClauseItem("Win Bonus (15%)", false));
            Clauses.Add(new ContractClauseItem("PPV Main Event Bonus", true));
        }
        else if (IsStaff)
        {
            Clauses.Add(new ContractClauseItem("Travel Expenses", true));
            Clauses.Add(new ContractClauseItem("Creative Input", false));
            Clauses.Add(new ContractClauseItem("TV Production Credit", true));
        }
        else // Trainee
        {
            Clauses.Add(new ContractClauseItem("Dormitory Provided", true));
            Clauses.Add(new ContractClauseItem("Medical Insurance", true));
            Clauses.Add(new ContractClauseItem("Call-Up Guarantee", false));
        }
    }

    private void Save()
    {
        // Apply changes to worker logic
        // In a real app we would create a Contract object and append to history
        // For now we just update properties if needed

        if (IsWrestler)
        {
            _worker.PushLevel = SelectedPush;
        }

        // If promoting trainee -> wrestler (special case handled by caller usually, but we can do logic here)

        _onSave?.Invoke();
    }
}

public class ContractClauseItem : ViewModelBase
{
    public string Name { get; }

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public ContractClauseItem(string name, bool isChecked)
    {
        Name = name;
        IsChecked = isChecked;
    }
}
