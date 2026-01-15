using System;
using System.Linq;
using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Models;
using RingGeneral.Core.Interfaces;
using System.Reactive;

namespace RingGeneral.UI.ViewModels.Youth;

public class YouthFinancialsViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly string _structureId;
    private YouthStructureState? _state;

    public string AnnualBudgetDisplay => $"{_state?.BudgetAnnuel ?? 0:N0} €";

    private string _remainingBudgetDisplay = string.Empty;
    public string RemainingBudgetDisplay
    {
        get => _remainingBudgetDisplay;
        set => this.RaiseAndSetIfChanged(ref _remainingBudgetDisplay, value);
    }

    private double _budgetUsagePercent;
    public double BudgetUsagePercent
    {
        get => _budgetUsagePercent;
        set => this.RaiseAndSetIfChanged(ref _budgetUsagePercent, value);
    }

    public string BudgetUsagePercentDisplay => $"{_budgetUsagePercent * 100:N0}% Used";

    public ObservableCollection<ExpenseItemViewModel> Expenses { get; } = new();

    public ReactiveCommand<Unit, Unit> InjectFundsCommand { get; }

    public YouthFinancialsViewModel(YouthRepository youthRepository, string structureId)
    {
        _youthRepository = youthRepository;
        _structureId = structureId;

        InjectFundsCommand = ReactiveCommand.CreateFromTask(InjectFundsAsync);

        LoadData();
    }

    private async System.Threading.Tasks.Task InjectFundsAsync()
    {
        if (_state == null) return;

        await System.Threading.Tasks.Task.Delay(100); // Simulate network/db latency

        // 1. Calculate new budget
        var newBudget = _state.BudgetAnnuel + 50_000;

        // 2. Persist to Repository
        // Note: Ideally we should use _youthFacade or expose UpdateBudget in _youthRepository.
        // For now, we'll try to use a direct update if available, or just update local state if persistence isn't ready in this scope.
        // But the user asked for "exactly", implying it should work.
        // Let's assume _youthRepository has UpdateBudget or similar. If not, I should add it.
        // Checking StructureManagementViewModel, it uses _youthFacade.UpdateBudgetAsync. 
        // Since I don't have _youthFacade here, I should probably pass it or add it.
        // Limit update to repository
        _youthRepository.ChangerBudgetYouth(_structureId, newBudget);

        // 3. Update Local State
        LoadData();
    }

    public void LoadData()
    {
        var structures = _youthRepository.ChargerYouthStructures();
        _state = structures.FirstOrDefault(s => s.YouthId == _structureId);

        if (_state == null) return;

        Expenses.Clear();

        // 1. Calculate Staff Wages
        var staff = _youthRepository.ChargerYouthStaffAssignments(_structureId);
        var staffWages = staff.Sum(s => EstimateWage(s.Role));
        var annualStaffWages = staffWages * 12;

        Expenses.Add(new ExpenseItemViewModel("Staff Wages", annualStaffWages, _state.BudgetAnnuel));

        // 2. Facility Upkeep
        var monthlyUpkeep = _state.NiveauEquipements * 2000; // 2k per level
        var annualUpkeep = monthlyUpkeep * 12;
        Expenses.Add(new ExpenseItemViewModel("Facility Upkeep", annualUpkeep, _state.BudgetAnnuel));

        // 3. Equipment Upgrades (Mock - roughly 5-10% of budget if level is high)
        var equipUpgradeCost = _state.NiveauEquipements > 1 ? 10000 : 0;
        Expenses.Add(new ExpenseItemViewModel("Equipment Upg.", equipUpgradeCost, _state.BudgetAnnuel));

        // 4. Scouting Trips (Mock)
        var scoutingCost = 5000;
        Expenses.Add(new ExpenseItemViewModel("Scouting Trips", scoutingCost, _state.BudgetAnnuel));

        // Totals
        var totalExpenses = annualStaffWages + annualUpkeep + equipUpgradeCost + scoutingCost;
        var remaining = _state.BudgetAnnuel - totalExpenses;

        RemainingBudgetDisplay = $"{remaining:N0} €";
        BudgetUsagePercent = (double)totalExpenses / _state.BudgetAnnuel;
        if (BudgetUsagePercent > 1) BudgetUsagePercent = 1;

        this.RaisePropertyChanged(nameof(AnnualBudgetDisplay));
    }

    private int EstimateWage(string role)
    {
        return role switch
        {
            "ENTRAINEUR_CHEF" => 4500,
            "PHYSIO" => 2200,
            "SCOUT" => 1800,
            "TRAINER" => 3000,
            _ => 2000
        };
    }
}

public class ExpenseItemViewModel : ViewModelBase
{
    public string Category { get; }
    public string Cost { get; }
    public string PercentDisplay { get; }
    public double PercentValue { get; } // 0 to 1 for progress bar

    public ExpenseItemViewModel(string category, int cost, int totalBudget)
    {
        Category = category;
        Cost = $"{cost:N0} €";

        double pct = (double)cost / totalBudget;
        PercentValue = pct;
        PercentDisplay = $"{pct * 100:N0}%";
    }
}
