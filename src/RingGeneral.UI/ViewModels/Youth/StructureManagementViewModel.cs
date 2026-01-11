using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Youth;

public sealed class StructureManagementViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;

    public ObservableCollection<YouthStructureItemViewModel> Structures { get; } = new();

    public StructureManagementViewModel(YouthRepository youthRepository, GameRepository gameRepository)
    {
        _youthRepository = youthRepository;
        _gameRepository = gameRepository;

        LoadStructures();
    }

    public void LoadStructures()
    {
        Structures.Clear();
        var structures = _youthRepository.ChargerYouthStructures();
        foreach (var s in structures)
        {
            Structures.Add(new YouthStructureItemViewModel(s, _youthRepository, _gameRepository, this));
        }
    }
}

public sealed class YouthStructureItemViewModel : ViewModelBase
{
    private readonly YouthStructureState _state;
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;
    private readonly StructureManagementViewModel _parent;

    private int _budgetAnnuel;
    private int _niveauEquipements;
    private int _qualiteCoaching;

    public string YouthId => _state.YouthId;
    public string Nom => _state.Nom;
    public string Region => _state.Region;
    public string Type => _state.Type;
    public string Philosophie => _state.Philosophie;
    public int CapaciteMax => _state.CapaciteMax; // ou _state.Capacite ?

    public int BudgetAnnuel
    {
        get => _budgetAnnuel;
        set => this.RaiseAndSetIfChanged(ref _budgetAnnuel, value);
    }

    public int NiveauEquipements
    {
        get => _niveauEquipements;
        set => this.RaiseAndSetIfChanged(ref _niveauEquipements, value);
    }

    public int QualiteCoaching
    {
        get => _qualiteCoaching;
        set => this.RaiseAndSetIfChanged(ref _qualiteCoaching, value);
    }

    public string EquipementsDisplay => $"{NiveauEquipements}/5";
    public string CoachingDisplay => $"{QualiteCoaching}/100";
    public string BudgetDisplay => $"{BudgetAnnuel:N0} €/an";

    public ReactiveCommand<Unit, Unit> UpgradeEquipmentCommand { get; }
    public ReactiveCommand<Unit, Unit> IncreaseBudgetCommand { get; }

    public YouthStructureItemViewModel(YouthStructureState state, YouthRepository youthRepo, GameRepository gameRepo, StructureManagementViewModel parent)
    {
        _state = state;
        _youthRepository = youthRepo;
        _gameRepository = gameRepo;
        _parent = parent;

        _budgetAnnuel = state.BudgetAnnuel;
        _niveauEquipements = state.NiveauEquipements;
        _qualiteCoaching = state.QualiteCoaching;

        UpgradeEquipmentCommand = ReactiveCommand.Create(UpgradeEquipment);
        IncreaseBudgetCommand = ReactiveCommand.Create(IncreaseBudget);
    }

    private void UpgradeEquipment()
    {
        if (NiveauEquipements >= 5) return;

        // Coût : 50k * niveau actuel
        var cout = 50_000 * NiveauEquipements;

        // TODO: Vérifier fonds compagnie via GameRepository
        // Pour l'instant on suppose infini ou géré ailleurs

        // Update DB
        _youthRepository.AmeliorerEquipements(YouthId);

        // Simuler update
        NiveauEquipements++;
        this.RaisePropertyChanged(nameof(EquipementsDisplay));

        // Refresh parent ?
    }

    private void IncreaseBudget()
    {
        // Increase by 10k
        var nouveauBudget = BudgetAnnuel + 10_000;
        _youthRepository.ChangerBudgetYouth(YouthId, nouveauBudget);
        BudgetAnnuel = nouveauBudget;
        this.RaisePropertyChanged(nameof(BudgetDisplay));
    }
}
