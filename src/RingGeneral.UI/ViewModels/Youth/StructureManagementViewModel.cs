using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Application.Facades;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using RingGeneral.Core.Interfaces;
using RingGeneral.Data.Repositories;
using System.Linq;

namespace RingGeneral.UI.ViewModels.Youth;

public sealed class StructureManagementViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;
    private readonly YouthFacade _youthFacade;
    private readonly IRegionRepository _regionRepository;

    public ObservableCollection<YouthStructureItemViewModel> Structures { get; } = new();

    // Power User Table Properties
    private string _searchFilter = string.Empty;
    public string SearchFilter
    {
        get => _searchFilter;
        set { this.RaiseAndSetIfChanged(ref _searchFilter, value); FilterStructures(); }
    }

    private ObservableCollection<YouthStructureItemViewModel> _filteredStructures = new();
    public ObservableCollection<YouthStructureItemViewModel> FilteredStructures
    {
        get => _filteredStructures;
        set => this.RaiseAndSetIfChanged(ref _filteredStructures, value);
    }

    public bool IsEmpty => Structures.Count == 0;
    // Creation State
    private bool _isCreating;
    public bool IsCreating
    {
        get => _isCreating;
        set => this.RaiseAndSetIfChanged(ref _isCreating, value);
    }

    private string _newStructureName = string.Empty;
    public string NewStructureName
    {
        get => _newStructureName;
        set => this.RaiseAndSetIfChanged(ref _newStructureName, value);
    }

    private string _selectedType = "DOJO";
    public string SelectedType
    {
        get => _selectedType;
        set => this.RaiseAndSetIfChanged(ref _selectedType, value);
    }

    private int _maxCapacity = 20;
    public int MaxCapacity
    {
        get => _maxCapacity;
        set => this.RaiseAndSetIfChanged(ref _maxCapacity, value);
    }

    private string _selectedPhilosophy = "Balanced";
    public string SelectedPhilosophy
    {
        get => _selectedPhilosophy;
        set => this.RaiseAndSetIfChanged(ref _selectedPhilosophy, value);
    }

    private string _selectedGenderPreference = "BOTH";
    public string SelectedGenderPreference
    {
        get => _selectedGenderPreference;
        set => this.RaiseAndSetIfChanged(ref _selectedGenderPreference, value);
    }

    private string _selectedSpecializationPreference = "NONE";
    public string SelectedSpecializationPreference
    {
        get => _selectedSpecializationPreference;
        set => this.RaiseAndSetIfChanged(ref _selectedSpecializationPreference, value);
    }

    public ObservableCollection<string> AvailableTypes { get; } = new() { "DOJO", "ACADEMY", "PERFORMANCE_CENTER", "INDY_SCHOOL", "DEVELOPMENT" };
    public ObservableCollection<string> AvailablePhilosophies { get; } = new() { "Balanced", "Technical", "Brawler", "High-Flyer", "All-Rounder", "Entertainment" };
    public ObservableCollection<string> AvailableGenderPreferences { get; } = new() { "MALE", "FEMALE", "BOTH" };
    public ObservableCollection<string> AvailableSpecializations { get; } = new() { "NONE", "LUCHADOR", "STRONG STYLE", "TECHNICAL", "BRAWLER", "HIGH-FLYER" };

    public string TypeDescription
    {
        get
        {
            return SelectedType switch
            {
                "DOJO" => "Établissement traditionnel et modeste. Budget faible, se concentre sur les fondamentaux.",
                "ACADEMY" => "École professionnelle avec un équipement correct. Attire les talents locaux.",
                "PERFORMANCE_CENTER" => "Centre à la pointe de la technologie. Budget élevé, idéal pour former les futures stars.",
                "INDY_SCHOOL" => "École indépendante favorisant la créativité et les styles alternatifs.",
                "DEVELOPMENT" => "Structure polyvalente optimisée pour les filiales en développement.",
                _ => "Description non disponible."
            };
        }
    }

    private System.Collections.Generic.List<RegionSelection> _allRegions = new();

    public ObservableCollection<string> AvailableCountries { get; } = new();

    private string? _selectedCountry;
    public string? SelectedCountry
    {
        get => _selectedCountry;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedCountry, value);
            FilterRegions();
        }
    }

    private void FilterRegions()
    {
        AvailableRegions.Clear();
        if (string.IsNullOrEmpty(SelectedCountry) || _allRegions.Count == 0) return;

        var regionsInCountry = _allRegions
            .Where(r => r.CountryName == SelectedCountry)
            .OrderByDescending(r => r.Importance)
            .ThenBy(r => r.RegionName);

        foreach (var r in regionsInCountry)
        {
            AvailableRegions.Add(r);
        }
        SelectedRegion = AvailableRegions.FirstOrDefault();
    }

    public ObservableCollection<RegionSelection> AvailableRegions { get; } = new();

    private RegionSelection? _selectedRegion;
    public RegionSelection? SelectedRegion
    {
        get => _selectedRegion;
        set => this.RaiseAndSetIfChanged(ref _selectedRegion, value);
    }

    public ObservableCollection<WorkerBackstageProfile> AvailableStaff { get; } = new();

    private WorkerBackstageProfile? _selectedStaff;
    public WorkerBackstageProfile? SelectedStaff
    {
        get => _selectedStaff;
        set => this.RaiseAndSetIfChanged(ref _selectedStaff, value);
    }

    public ReactiveCommand<Unit, Unit> StartCreationCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCreationCommand { get; }
    public ReactiveCommand<Unit, Unit> ConfirmCreationCommand { get; }
    public ReactiveCommand<YouthStructureItemViewModel, YouthStructureItemViewModel> SelectStructureCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseStructureCommand { get; }

    private YouthStructureItemViewModel? _selectedListStructure;
    public YouthStructureItemViewModel? SelectedListStructure
    {
        get => _selectedListStructure;
        set => this.RaiseAndSetIfChanged(ref _selectedListStructure, value);
    }

    public string TotalBudgetDisplay => $"{Structures.Sum(s => s.BudgetAnnuel) / 1000:N0} k€";
    public int TotalTraineesCount => Structures.Sum(s => s.TraineeCount);

    public StructureManagementViewModel(
        YouthRepository youthRepository,
        GameRepository gameRepository,
        IRegionRepository regionRepository,
        IWorkerGenerationService generationService)
    {
        _youthRepository = youthRepository;
        _gameRepository = gameRepository;
        _regionRepository = regionRepository;
        _youthFacade = new YouthFacade(youthRepository, generationService, gameRepository);

        this.WhenAnyValue(x => x.SelectedType)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(TypeDescription)));

        StartCreationCommand = ReactiveCommand.Create(StartCreation);
        CancelCreationCommand = ReactiveCommand.Create(() => { IsCreating = false; });
        ConfirmCreationCommand = ReactiveCommand.CreateFromTask(ConfirmCreationAsync);
        SelectStructureCommand = ReactiveCommand.Create<YouthStructureItemViewModel, YouthStructureItemViewModel>(s => s);

        var canClose = this.WhenAnyValue(x => x.SelectedListStructure, selector: s => s != null);
        CloseStructureCommand = ReactiveCommand.Create(CloseStructure, canClose);

        try
        {
            LoadStructures();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading structures: {ex.Message}");
        }
    }


    private void FilterStructures()
    {
        if (string.IsNullOrWhiteSpace(SearchFilter))
        {
            FilteredStructures = new ObservableCollection<YouthStructureItemViewModel>(Structures);
            return;
        }

        var filtered = Structures.Where(s =>
            s.Nom.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
            s.Region.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
            s.Type.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        FilteredStructures = new ObservableCollection<YouthStructureItemViewModel>(filtered);
    }

    private void StartCreation()
    {
        if (IsCreating)
        {
            IsCreating = false;
            return;
        }

        NewStructureName = "Nouveau Centre";
        SelectedType = "DOJO";

        // Load all regions and filter by country
        _allRegions = _regionRepository.GetRegions().ToList();

        AvailableCountries.Clear();
        var countries = _allRegions.Select(r => r.CountryName).Distinct().OrderBy(c => c).ToList();
        foreach (var c in countries)
        {
            AvailableCountries.Add(c);
        }

        // Default Country (Try to find "France" or "USA" or First)
        SelectedCountry = AvailableCountries.FirstOrDefault(c => c == "France")
                          ?? AvailableCountries.FirstOrDefault(c => c == "USA")
                          ?? AvailableCountries.FirstOrDefault();

        IsCreating = true;

        AvailableStaff.Clear();
        try
        {
            using var conn = _gameRepository.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CompanyId FROM Companies LIMIT 1";
            var companyId = cmd.ExecuteScalar()?.ToString();

            if (!string.IsNullOrEmpty(companyId))
            {
                var workers = _youthRepository.ChargerWorkersDisposPourStaff(companyId);
                foreach (var w in workers)
                {
                    AvailableStaff.Add(w);
                }
            }
        }
        catch (Exception ex)
        {
            ApplicationServices.Logger.Error($"Erreur chargement staff dispo : {ex.Message}", ex);
        }
    }

    private async Task ConfirmCreationAsync()
    {
        try
        {
            using var conn = _gameRepository.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CompanyId FROM Companies LIMIT 1";
            var companyId = cmd.ExecuteScalar()?.ToString();

            if (string.IsNullOrEmpty(companyId)) return;

            var structureNom = await _youthFacade.CreateStructureAsync(
                companyId,
                NewStructureName,
                SelectedRegion?.RegionId,
                SelectedType,
                50000,
                MaxCapacity,
                SelectedPhilosophy,
                SelectedGenderPreference,
                SelectedSpecializationPreference
            );

            if (SelectedStaff != null)
            {
                var structures = _youthRepository.ChargerYouthStructures();
                var created = structures.OrderByDescending(s => s.YouthId).FirstOrDefault(s => s.Nom == NewStructureName);
                if (created != null)
                {
                    _youthRepository.AffecterCoachYouth(created.YouthId, SelectedStaff.WorkerId, "ENTRAINEUR_CHEF", 1);
                }
            }

            IsCreating = false;
            LoadStructures();
        }
        catch (Exception ex)
        {
            ApplicationServices.Logger.Error($"Erreur création structure : {ex.Message}", ex);
        }
    }

    private void CloseStructure()
    {
        if (SelectedListStructure == null) return;

        try
        {
            _youthRepository.DeleteStructure(SelectedListStructure.YouthId);
            LoadStructures();
        }
        catch (Exception ex)
        {
            ApplicationServices.Logger.Error($"Erreur suppression structure : {ex.Message}", ex);
        }
    }

    public void LoadStructures()
    {
        Structures.Clear();
        var structures = _youthRepository.ChargerYouthStructures();
        var regions = _regionRepository.GetRegions();
        foreach (var s in structures)
        {
            var regionName = regions.FirstOrDefault(r => r.RegionId == s.Region)?.RegionName ?? "Unknown Region";
            Structures.Add(new YouthStructureItemViewModel(s, _youthRepository, _gameRepository, _youthFacade, this, regionName));
        }
        this.RaisePropertyChanged(nameof(IsEmpty));
        this.RaisePropertyChanged(nameof(TotalBudgetDisplay));
        this.RaisePropertyChanged(nameof(TotalTraineesCount));
        FilterStructures();
    }
}

public sealed class YouthStructureItemViewModel : ViewModelBase
{
    private readonly YouthStructureState _state;
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;
    private readonly YouthFacade _youthFacade;
    private readonly StructureManagementViewModel _parent;

    private readonly string _regionName;
    private int _budgetAnnuel;
    private int _niveauEquipements;
    private int _qualiteCoaching;

    private string _philosophie;
    private string _trainingIntensity;

    public string YouthId => _state.YouthId;
    public string Nom => _state.Nom;
    public string Region => _regionName;
    public string Type => _state.Type;
    public string Philosophie
    {
        get => _philosophie;
        set
        {
            this.RaiseAndSetIfChanged(ref _philosophie, value);
            this.RaisePropertyChanged(nameof(PhilosophyBonus));
        }
    }
    public int CapaciteMax => _state.CapaciteMax;
    public int TraineeCount => _state.TraineesActifs;
    public string TraineesDisplay => $"{TraineeCount} / {CapaciteMax}";
    public string Status
    {
        get
        {
            if (TraineeCount >= CapaciteMax) return "FULL";
            return "OK";
        }
    }

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

    public string EquipementsDisplay
    {
        get
        {
            var stars = new string('★', NiveauEquipements) + new string('☆', 5 - NiveauEquipements);
            return stars;
        }
    }
    public string CoachingDisplay => $"{QualiteCoaching}%";
    public string BudgetDisplay => $"{BudgetAnnuel / 1000:N0} k€";

    public string CapacityStatus => TraineeCount >= CapaciteMax ? "FULL" : (TraineeCount >= CapaciteMax * 0.9 ? "CROWDED" : "HEALTHY");
    public string UpkeepCostDisplay => $"{1000 * NiveauEquipements:N0} €/mo"; // Dummy calculation
    public string PhilosophyBonus => Philosophie switch
    {
        "Technical" => "(Submission Bonus +10%)",
        "Brawler" => "(Strike Power Bonus +10%)",
        "High-Flyer" => "(Agility Bonus +10%)",
        "Entertainment" => "(Charisma Bonus +15%)",
        "Balanced" => "(All Stats +2%)",
        _ => "(Standard Progression)"
    };
    public string TrainingIntensity
    {
        get => _trainingIntensity;
        set => this.RaiseAndSetIfChanged(ref _trainingIntensity, value);
    }
    public string RegionFocus => $"{_regionName} - Scouting Range: 500km"; // Placeholder

    public ReactiveCommand<Unit, Unit> UpgradeEquipmentCommand { get; }
    public ReactiveCommand<Unit, Unit> IncreaseBudgetCommand { get; }

    public YouthStructureItemViewModel(YouthStructureState state, YouthRepository youthRepo, GameRepository gameRepo, YouthFacade youthFacade, StructureManagementViewModel parent, string regionName)
    {
        _state = state;
        _youthRepository = youthRepo;
        _gameRepository = gameRepo;
        _youthFacade = youthFacade;
        _parent = parent;
        _regionName = regionName;

        _budgetAnnuel = state.BudgetAnnuel;
        _niveauEquipements = state.NiveauEquipements;
        _qualiteCoaching = state.QualiteCoaching;
        _philosophie = state.Philosophie;
        _trainingIntensity = "HIGH"; // Default

        UpgradeEquipmentCommand = ReactiveCommand.CreateFromTask(UpgradeEquipmentAsync);
        IncreaseBudgetCommand = ReactiveCommand.CreateFromTask(IncreaseBudgetAsync);
    }

    private async Task UpgradeEquipmentAsync()
    {
        var success = await _youthFacade.UpgradeEquipmentAsync(YouthId, NiveauEquipements);

        if (success)
        {
            NiveauEquipements++;
            this.RaisePropertyChanged(nameof(EquipementsDisplay));
        }
    }

    private async Task IncreaseBudgetAsync()
    {
        var nouveauBudget = BudgetAnnuel + 10_000;
        await _youthFacade.UpdateBudgetAsync(YouthId, nouveauBudget);
        BudgetAnnuel = nouveauBudget;
        this.RaisePropertyChanged(nameof(BudgetDisplay));
    }
}
