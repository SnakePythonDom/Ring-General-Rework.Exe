using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Services;
using RingGeneral.Data.Repositories;
using RingGeneral.Data.Database;
using RingGeneral.UI.Services.Navigation;

namespace RingGeneral.UI.ViewModels.Company;

/// <summary>
/// ViewModel pour la gestion des filiales
/// </summary>
public sealed class ChildCompaniesViewModel : ViewModelBase
{
    private readonly IChildCompanyExtendedRepository? _childCompanyRepository;
    private readonly ChildCompanyService? _childCompanyService;
    private string _parentCompanyId = string.Empty;

    private readonly IRegionRepository? _regionRepository;
    private readonly INavigationService? _navigationService;

    public ChildCompaniesViewModel(
        IChildCompanyExtendedRepository? childCompanyRepository = null,
        ChildCompanyService? childCompanyService = null,
        IRegionRepository? regionRepository = null,
        INavigationService? navigationService = null)
    {
        _childCompanyRepository = childCompanyRepository;
        _childCompanyService = childCompanyService;
        _regionRepository = regionRepository;
        _navigationService = navigationService;

        AvailableObjectives = Enum.GetValues<ChildCompanyObjective>();
        AvailableRegions = new ObservableCollection<RegionSelection>();

        ChildCompanies = new ObservableCollection<ChildCompanyExtended>();
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshChildCompaniesAsync);

        var canCreate = this.WhenAnyValue(
            x => x.SelectedObjective,
            x => x.NewChildCompanyName,
            x => x.ParentCompanyId,
            (obj, name, parentId) => obj.HasValue
                && !string.IsNullOrWhiteSpace(name)
                && !string.IsNullOrWhiteSpace(parentId));

        CreateChildCompanyCommand = ReactiveCommand.CreateFromTask(CreateChildCompanyAsync, canCreate);

        ManageChildCompanyCommand = ReactiveCommand.Create<string>(ManageChildCompany);

        // Charger les régions si le repository est disponible
        if (_regionRepository != null)
        {
            try
            {
                var regions = _regionRepository.GetRegions();
                foreach (var region in regions)
                {
                    AvailableRegions.Add(region);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Erreur lors du chargement des régions: {ex.Message}", ex);
            }
        }
    }

    public ObservableCollection<ChildCompanyExtended> ChildCompanies { get; }
    public ObservableCollection<RegionSelection> AvailableRegions { get; }

    // Properties for creation
    public IEnumerable<ChildCompanyObjective> AvailableObjectives { get; }

    public ChildCompanyObjective? SelectedObjective
    {
        get => _selectedObjective;
        set => this.RaiseAndSetIfChanged(ref _selectedObjective, value);
    }
    private ChildCompanyObjective? _selectedObjective;

    public string? NewChildCompanyName
    {
        get => _newChildCompanyName;
        set => this.RaiseAndSetIfChanged(ref _newChildCompanyName, value);
    }
    private string? _newChildCompanyName;

    public string? NewChildCompanyAcronym
    {
        get => _newChildCompanyAcronym;
        set => this.RaiseAndSetIfChanged(ref _newChildCompanyAcronym, value);
    }
    private string? _newChildCompanyAcronym;

    public RegionSelection? SelectedRegion
    {
        get => _selectedRegion;
        set => this.RaiseAndSetIfChanged(ref _selectedRegion, value);
    }
    private RegionSelection? _selectedRegion;

    public int NewChildCompanyRosterLimit
    {
        get => _newChildCompanyRosterLimit;
        set => this.RaiseAndSetIfChanged(ref _newChildCompanyRosterLimit, value);
    }
    private int _newChildCompanyRosterLimit = 20;


    // Parent context
    public string ParentCompanyId
    {
        get => _parentCompanyId;
        set => this.RaiseAndSetIfChanged(ref _parentCompanyId, value);
    }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateChildCompanyCommand { get; }
    public ReactiveCommand<string, Unit> ManageChildCompanyCommand { get; }

    public async Task LoadChildCompaniesAsync(string parentCompanyId)
    {
        ParentCompanyId = parentCompanyId;

        if (_childCompanyRepository == null)
        {
            return;
        }

        try
        {
            var childCompanies = await _childCompanyRepository.GetChildCompaniesByParentIdAsync(parentCompanyId);
            ChildCompanies.Clear();
            foreach (var childCompany in childCompanies)
            {
                ChildCompanies.Add(childCompany);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors du chargement des filiales: {ex.Message}", ex);
        }
    }

    private async Task RefreshChildCompaniesAsync()
    {
        if (!string.IsNullOrWhiteSpace(ParentCompanyId))
        {
            await LoadChildCompaniesAsync(ParentCompanyId);
        }
    }

    private async Task CreateChildCompanyAsync()
    {
        if (_childCompanyService == null || !SelectedObjective.HasValue)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(NewChildCompanyName))
        {
            return;
        }

        try
        {
            var childCompanyId = Guid.NewGuid().ToString("N");
            await _childCompanyService.CreateChildCompanyAsync(
                childCompanyId,
                ParentCompanyId,
                SelectedObjective.Value,
                NewChildCompanyName,
                NewChildCompanyAcronym,
                SelectedRegion?.RegionId,
                NewChildCompanyRosterLimit);

            await RefreshChildCompaniesAsync();

            // Reset input fields
            NewChildCompanyName = string.Empty;
            NewChildCompanyAcronym = string.Empty;
            SelectedRegion = null;
            SelectedObjective = null;
            NewChildCompanyRosterLimit = 20;
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors du chargement des filiales: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Phase 2.3 - Gère une filiale (ouvre la vue de gestion détaillée)
    /// </summary>
    private void ManageChildCompany(string childCompanyId)
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo<ChildCompanyDetailViewModel>(childCompanyId);
        }
        else
        {
            Logger.Error("Navigation service not available in ChildCompaniesViewModel");
        }
    }
}
