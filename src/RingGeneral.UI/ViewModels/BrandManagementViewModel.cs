using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Services;

namespace RingGeneral.UI.ViewModels;

/// <summary>
/// ViewModel pour l'affichage d'un brand dans la liste.
/// </summary>
public sealed class BrandListItemViewModel : ViewModelBase
{
    private int _prestige;
    private int _averageAudience;
    private bool _isActive;

    public BrandListItemViewModel(
        string brandId,
        string name,
        string objective,
        int prestige,
        int priority,
        double budgetPerShow,
        int averageAudience,
        bool isActive,
        DateTime createdAt)
    {
        BrandId = brandId;
        Name = name;
        Objective = objective;
        _prestige = prestige;
        Priority = priority;
        BudgetPerShow = budgetPerShow;
        _averageAudience = averageAudience;
        _isActive = isActive;
        CreatedAt = createdAt;

        UpdateDerivedProperties();
    }

    public string BrandId { get; }
    public string Name { get; }
    public string Objective { get; }
    public int Priority { get; }
    public double BudgetPerShow { get; }
    public DateTime CreatedAt { get; }

    public int Prestige
    {
        get => _prestige;
        set
        {
            this.RaiseAndSetIfChanged(ref _prestige, value);
            UpdateDerivedProperties();
        }
    }

    public int AverageAudience
    {
        get => _averageAudience;
        set
        {
            this.RaiseAndSetIfChanged(ref _averageAudience, value);
            UpdateDerivedProperties();
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    private string? _prestigeLabel;
    public string? PrestigeLabel
    {
        get => _prestigeLabel;
        private set => this.RaiseAndSetIfChanged(ref _prestigeLabel, value);
    }

    private string? _statusLabel;
    public string? StatusLabel
    {
        get => _statusLabel;
        private set => this.RaiseAndSetIfChanged(ref _statusLabel, value);
    }

    private int _healthScore;
    public int HealthScore
    {
        get => _healthScore;
        private set => this.RaiseAndSetIfChanged(ref _healthScore, value);
    }

    private string? _summary;
    public string? Summary
    {
        get => _summary;
        private set => this.RaiseAndSetIfChanged(ref _summary, value);
    }

    private void UpdateDerivedProperties()
    {
        PrestigeLabel = Prestige >= 70 ? "Élevé" :
                       Prestige >= 40 ? "Moyen" : "Faible";

        StatusLabel = IsActive ? "✓ Actif" : "Inactif";

        HealthScore = CalculateHealthScore();

        Summary = $"{Objective} • Prestige: {PrestigeLabel} ({Prestige}/100) • Budget: ${BudgetPerShow:N0}/show";
    }

    private int CalculateHealthScore()
    {
        return (int)(Prestige * 0.4 +
                    (AverageAudience / 100.0) * 0.4 +
                    (BudgetPerShow / 500.0) * 0.2);
    }
}

/// <summary>
/// ViewModel pour la hiérarchie de la compagnie.
/// </summary>
public sealed class CompanyHierarchyViewModel : ViewModelBase
{
    private string _hierarchyType;
    private int _activeBrandCount;

    public CompanyHierarchyViewModel(
        string companyId,
        string hierarchyType,
        string? headBookerId,
        string? headBookerName,
        int activeBrandCount)
    {
        CompanyId = companyId;
        _hierarchyType = hierarchyType;
        HeadBookerId = headBookerId;
        HeadBookerName = headBookerName;
        _activeBrandCount = activeBrandCount;

        UpdateStructureDescription();
    }

    public string CompanyId { get; }
    public string? HeadBookerId { get; }
    public string? HeadBookerName { get; }

    public string HierarchyType
    {
        get => _hierarchyType;
        set
        {
            this.RaiseAndSetIfChanged(ref _hierarchyType, value);
            UpdateStructureDescription();
        }
    }

    public int ActiveBrandCount
    {
        get => _activeBrandCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _activeBrandCount, value);
            UpdateStructureDescription();
        }
    }

    private string? _structureDescription;
    public string? StructureDescription
    {
        get => _structureDescription;
        private set => this.RaiseAndSetIfChanged(ref _structureDescription, value);
    }

    private void UpdateStructureDescription()
    {
        if (HierarchyType == "MonoBrand")
        {
            StructureDescription = $"Mono-Brand • Owner → Booker → Staff";
        }
        else
        {
            var headBookerLabel = string.IsNullOrWhiteSpace(HeadBookerName) ? "Non assigné" : HeadBookerName;
            StructureDescription = $"Multi-Brand ({ActiveBrandCount} brands) • Owner → Head Booker ({headBookerLabel}) → Brand Bookers → Staff";
        }
    }
}

/// <summary>
/// Option d'objectif de brand pour les sélecteurs.
/// </summary>
public sealed class BrandObjectiveOptionViewModel
{
    public BrandObjectiveOptionViewModel(BrandObjective objective, string description)
    {
        Objective = objective;
        ObjectiveName = objective.ToString();
        Description = description;
    }

    public BrandObjective Objective { get; }
    public string ObjectiveName { get; }
    public string Description { get; }

    public string DisplayText => $"{ObjectiveName} • {Description}";
}

/// <summary>
/// ViewModel pour les conflits détectés entre brands.
/// </summary>
public sealed class BrandConflictViewModel
{
    public BrandConflictViewModel(string conflictType, string description, string severity)
    {
        ConflictType = conflictType;
        Description = description;
        Severity = severity;
        SeverityIcon = severity switch
        {
            "High" => "⚠️",
            "Medium" => "⚡",
            _ => "ℹ️"
        };
    }

    public string ConflictType { get; }
    public string Description { get; }
    public string Severity { get; }
    public string SeverityIcon { get; }

    public string DisplayText => $"{SeverityIcon} {Description}";
}

/// <summary>
/// ViewModel principal pour la gestion des brands.
/// </summary>
public sealed class BrandManagementViewModel : ViewModelBase
{
    private readonly IBrandRepository _brandRepository;
    private readonly BrandManagementService _brandService;
    private readonly string _companyId;

    private CompanyHierarchyViewModel? _hierarchy;
    private BrandListItemViewModel? _selectedBrand;
    private bool _canCreateBrand;

    public BrandManagementViewModel(
        string companyId,
        IBrandRepository brandRepository,
        BrandManagementService brandService)
    {
        _companyId = companyId;
        _brandRepository = brandRepository;
        _brandService = brandService;

        Brands = new ObservableCollection<BrandListItemViewModel>();
        AvailableObjectives = new ObservableCollection<BrandObjectiveOptionViewModel>();
        DetectedConflicts = new ObservableCollection<BrandConflictViewModel>();

        CreateBrandCommand = ReactiveCommand.CreateFromTask<(string Name, BrandObjective Objective, int Priority, double Budget)>(CreateBrandAsync);
        CloseBrandCommand = ReactiveCommand.CreateFromTask<string>(CloseBrandAsync);
        UpdateBrandCommand = ReactiveCommand.CreateFromTask<BrandListItemViewModel>(UpdateBrandAsync);
        SwitchToMultiBrandCommand = ReactiveCommand.CreateFromTask(SwitchToMultiBrandHierarchyAsync);
        RefreshDataCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);

        LoadAvailableObjectives();
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    public ObservableCollection<BrandListItemViewModel> Brands { get; }
    public ObservableCollection<BrandObjectiveOptionViewModel> AvailableObjectives { get; }
    public ObservableCollection<BrandConflictViewModel> DetectedConflicts { get; }

    public CompanyHierarchyViewModel? Hierarchy
    {
        get => _hierarchy;
        private set => this.RaiseAndSetIfChanged(ref _hierarchy, value);
    }

    public BrandListItemViewModel? SelectedBrand
    {
        get => _selectedBrand;
        set => this.RaiseAndSetIfChanged(ref _selectedBrand, value);
    }

    public bool CanCreateBrand
    {
        get => _canCreateBrand;
        private set => this.RaiseAndSetIfChanged(ref _canCreateBrand, value);
    }

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<(string Name, BrandObjective Objective, int Priority, double Budget), Unit> CreateBrandCommand { get; }
    public ReactiveCommand<string, Unit> CloseBrandCommand { get; }
    public ReactiveCommand<BrandListItemViewModel, Unit> UpdateBrandCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToMultiBrandCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    public async Task LoadDataAsync()
    {
        try
        {
            Logger.Info("Loading brand management data...");

            // Charger la hiérarchie
            var hierarchyModel = await _brandRepository.GetHierarchyByCompanyIdAsync(_companyId);
            if (hierarchyModel != null)
            {
                Hierarchy = MapHierarchyToViewModel(hierarchyModel);
                CanCreateBrand = hierarchyModel.Type == CompanyHierarchyType.MultiBrand ||
                                (hierarchyModel.Type == CompanyHierarchyType.MonoBrand && Brands.Count == 0);
            }

            // Charger les brands
            var brandModels = await _brandRepository.GetBrandsByCompanyIdAsync(_companyId);
            Brands.Clear();
            foreach (var brand in brandModels.OrderBy(b => b.Priority))
            {
                Brands.Add(MapBrandToViewModel(brand));
            }

            // Détecter les conflits
            await DetectConflictsAsync(brandModels);

            Logger.Info($"Loaded {Brands.Count} brands, hierarchy: {Hierarchy?.HierarchyType ?? "Unknown"}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error loading brand data: {ex.Message}", ex);
        }
    }

    // ====================================================================
    // PRIVATE COMMAND HANDLERS
    // ====================================================================

    private async Task CreateBrandAsync((string Name, BrandObjective Objective, int Priority, double Budget) parameters)
    {
        if (!CanCreateBrand)
        {
            Logger.Warning("Cannot create brand: hierarchy does not allow it");
            return;
        }

        try
        {
            Logger.Info($"Creating new brand: {parameters.Name}...");

            var brand = _brandService.CreateBrand(
                _companyId,
                parameters.Name,
                parameters.Objective,
                parameters.Priority,
                parameters.Budget);

            await _brandRepository.SaveBrandAsync(brand);

            await LoadDataAsync();

            Logger.Info($"Brand created: {parameters.Name} ({parameters.Objective})");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error creating brand: {ex.Message}", ex);
        }
    }

    private async Task CloseBrandAsync(string brandId)
    {
        try
        {
            Logger.Info($"Closing brand: {brandId}...");

            await _brandRepository.DeleteBrandAsync(brandId);

            await LoadDataAsync();

            Logger.Info("Brand closed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error closing brand: {ex.Message}", ex);
        }
    }

    private async Task UpdateBrandAsync(BrandListItemViewModel brandViewModel)
    {
        try
        {
            Logger.Info($"Updating brand: {brandViewModel.Name}...");

            var brandModel = await _brandRepository.GetBrandByIdAsync(brandViewModel.BrandId);
            if (brandModel != null)
            {
                var updatedBrand = brandModel with
                {
                    Prestige = brandViewModel.Prestige,
                    IsActive = brandViewModel.IsActive
                };

                await _brandRepository.UpdateBrandAsync(updatedBrand);

                Logger.Info("Brand updated successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error updating brand: {ex.Message}", ex);
        }
    }

    private async Task SwitchToMultiBrandHierarchyAsync()
    {
        if (Hierarchy == null || Hierarchy.HierarchyType == "MultiBrand")
        {
            Logger.Warning("Already in MultiBrand hierarchy or no hierarchy found");
            return;
        }

        try
        {
            Logger.Info("Switching to MultiBrand hierarchy...");

            var hierarchyModel = await _brandRepository.GetHierarchyByCompanyIdAsync(_companyId);
            if (hierarchyModel != null)
            {
                var updatedHierarchy = hierarchyModel with
                {
                    Type = CompanyHierarchyType.MultiBrand,
                    // Note: HeadBookerId doit être assigné séparément par l'utilisateur
                };

                await _brandRepository.UpdateHierarchyAsync(updatedHierarchy);

                await LoadDataAsync();

                Logger.Info("Switched to MultiBrand hierarchy - assign Head Booker to complete setup");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error switching hierarchy: {ex.Message}", ex);
        }
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private void LoadAvailableObjectives()
    {
        var objectives = new Dictionary<BrandObjective, string>
        {
            [BrandObjective.Flagship] = "Brand principal, prestige élevé",
            [BrandObjective.Development] = "Développement jeunes talents",
            [BrandObjective.Experimental] = "Expérimentation créative",
            [BrandObjective.Mainstream] = "Produit grand public",
            [BrandObjective.Prestige] = "Focus sur qualité technique",
            [BrandObjective.Regional] = "Ancrage régional",
            [BrandObjective.Women] = "Division féminine",
            [BrandObjective.Touring] = "Brand itinérant"
        };

        foreach (var kvp in objectives)
        {
            AvailableObjectives.Add(new BrandObjectiveOptionViewModel(kvp.Key, kvp.Value));
        }
    }

    private async Task DetectConflictsAsync(List<Brand> brands)
    {
        DetectedConflicts.Clear();

        if (!brands.Any())
            return;

        var conflicts = _brandService.DetectBrandConflicts(brands, Hierarchy != null
            ? await _brandRepository.GetHierarchyByCompanyIdAsync(_companyId) ?? new CompanyHierarchy
            {
                HierarchyId = "",
                CompanyId = _companyId,
                Type = CompanyHierarchyType.MonoBrand,
                OwnerId = "",
                HeadBookerId = null,
                ActiveBrandCount = brands.Count,
                CreatedAt = DateTime.Now
            }
            : new CompanyHierarchy
            {
                HierarchyId = "",
                CompanyId = _companyId,
                Type = CompanyHierarchyType.MonoBrand,
                OwnerId = "",
                HeadBookerId = null,
                ActiveBrandCount = brands.Count,
                CreatedAt = DateTime.Now
            });

        foreach (var conflict in conflicts)
        {
            var severity = DetermineConflictSeverity(conflict);
            DetectedConflicts.Add(new BrandConflictViewModel("Configuration", conflict, severity));
        }
    }

    private static string DetermineConflictSeverity(string conflict)
    {
        if (conflict.Contains("Multiple flagships") || conflict.Contains("No flagship"))
            return "High";

        if (conflict.Contains("Same air day"))
            return "Medium";

        return "Low";
    }

    private static BrandListItemViewModel MapBrandToViewModel(Brand brand)
    {
        return new BrandListItemViewModel(
            brand.BrandId,
            brand.Name,
            brand.Objective.ToString(),
            brand.Prestige,
            brand.Priority,
            brand.BudgetPerShow,
            brand.AverageAudience,
            brand.IsActive,
            brand.CreatedAt);
    }

    private static CompanyHierarchyViewModel MapHierarchyToViewModel(CompanyHierarchy hierarchy)
    {
        return new CompanyHierarchyViewModel(
            hierarchy.CompanyId,
            hierarchy.Type.ToString(),
            hierarchy.HeadBookerId,
            null, // TODO: Récupérer le nom du Head Booker
            hierarchy.ActiveBrandCount);
    }
}
