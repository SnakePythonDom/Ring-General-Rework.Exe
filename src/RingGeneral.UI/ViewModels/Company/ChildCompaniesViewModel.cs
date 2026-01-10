using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Services;

namespace RingGeneral.UI.ViewModels.Company;

/// <summary>
/// ViewModel pour la gestion des filiales
/// </summary>
public sealed class ChildCompaniesViewModel : ViewModelBase
{
    private readonly IChildCompanyExtendedRepository? _childCompanyRepository;
    private readonly ChildCompanyService? _childCompanyService;
    private string _parentCompanyId = string.Empty;

    public ChildCompaniesViewModel(
        IChildCompanyExtendedRepository? childCompanyRepository = null,
        ChildCompanyService? childCompanyService = null)
    {
        _childCompanyRepository = childCompanyRepository;
        _childCompanyService = childCompanyService;

        ChildCompanies = new ObservableCollection<ChildCompanyExtended>();
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshChildCompaniesAsync);
        CreateChildCompanyCommand = ReactiveCommand.CreateFromTask<ChildCompanyObjective>(CreateChildCompanyAsync);
    }

    /// <summary>
    /// Commande pour rafraîchir la liste des filiales
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    /// <summary>
    /// Commande pour créer une nouvelle filiale
    /// </summary>
    public ReactiveCommand<ChildCompanyObjective, Unit> CreateChildCompanyCommand { get; }

    /// <summary>
    /// Liste des filiales
    /// </summary>
    public ObservableCollection<ChildCompanyExtended> ChildCompanies { get; }

    public string ParentCompanyId
    {
        get => _parentCompanyId;
        set => this.RaiseAndSetIfChanged(ref _parentCompanyId, value);
    }

    /// <summary>
    /// Charge les filiales pour une compagnie mère
    /// </summary>
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

    /// <summary>
    /// Rafraîchit la liste des filiales
    /// </summary>
    private async Task RefreshChildCompaniesAsync()
    {
        if (!string.IsNullOrWhiteSpace(ParentCompanyId))
        {
            await LoadChildCompaniesAsync(ParentCompanyId);
        }
    }

    /// <summary>
    /// Crée une nouvelle filiale
    /// </summary>
    private async Task CreateChildCompanyAsync(ChildCompanyObjective objective)
    {
        if (_childCompanyService == null)
        {
            return;
        }

        try
        {
            var childCompanyId = Guid.NewGuid().ToString("N");
            await _childCompanyService.CreateChildCompanyAsync(childCompanyId, ParentCompanyId, objective);
            await RefreshChildCompaniesAsync();
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors de la création de la filiale: {ex.Message}", ex);
        }
    }
}
