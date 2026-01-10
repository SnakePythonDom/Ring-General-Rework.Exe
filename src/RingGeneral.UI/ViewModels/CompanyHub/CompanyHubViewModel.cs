using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.Services.Navigation;

namespace RingGeneral.UI.ViewModels.CompanyHub;

/// <summary>
/// ViewModel principal du Company Hub avec navigation multi-tabs
/// </summary>
public sealed class CompanyHubViewModel : ViewModelBase, INavigableViewModel
{
    private readonly GameRepository _gameRepository;
    private readonly IOwnerRepository _ownerRepository;
    private readonly IBookerRepository _bookerRepository;
    private readonly ICatchStyleRepository _catchStyleRepository;

    private int _selectedTabIndex = 0;
    private bool _isViewingRival = false;
    private string? _currentCompanyId;
    private CompanyState? _currentCompany;
    private OwnerSnapshot? _currentOwner;
    private BookerSnapshot? _currentBooker;
    private CatchStyle? _currentStyle;
    private ObservableCollection<CompanyState> _rivalCompanies;

    public CompanyHubViewModel(
        GameRepository? gameRepository = null,
        IOwnerRepository? ownerRepository = null,
        IBookerRepository? bookerRepository = null,
        ICatchStyleRepository? catchStyleRepository = null)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _bookerRepository = bookerRepository ?? throw new ArgumentNullException(nameof(bookerRepository));
        _catchStyleRepository = catchStyleRepository ?? throw new ArgumentNullException(nameof(catchStyleRepository));

        _rivalCompanies = new ObservableCollection<CompanyState>();

        // Commandes
        SwitchToRivalsCommand = ReactiveCommand.Create(SwitchToRivals);
        SwitchToMyCompanyCommand = ReactiveCommand.Create(SwitchToMyCompany);
    }

    #region Properties

    /// <summary>
    /// Index de l'onglet actif (0=Profile, 1=Staff, 2=Roster, etc.)
    /// </summary>
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
    }

    /// <summary>
    /// Est-on en train de visualiser une compagnie rivale ?
    /// </summary>
    public bool IsViewingRival
    {
        get => _isViewingRival;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _isViewingRival, value))
            {
                this.RaisePropertyChanged(nameof(IsViewingMyCompany));
                this.RaisePropertyChanged(nameof(SwitchButtonText));
                this.RaisePropertyChanged(nameof(CompanyDisplayTitle));
                this.RaisePropertyChanged(nameof(CompanyLogoText));
                this.RaisePropertyChanged(nameof(CompanyLogoBackground));
            }
        }
    }

    /// <summary>
    /// Est-on en train de visualiser sa compagnie ?
    /// </summary>
    public bool IsViewingMyCompany
    {
        get => !IsViewingRival;
        set
        {
            if (value)
            {
                IsViewingRival = false;
            }
        }
    }

    /// <summary>
    /// Compagnie actuellement affichée
    /// </summary>
    public CompanyState? CurrentCompany
    {
        get => _currentCompany;
        set => this.RaiseAndSetIfChanged(ref _currentCompany, value);
    }

    /// <summary>
    /// Owner de la compagnie actuelle
    /// </summary>
    public OwnerSnapshot? CurrentOwner
    {
        get => _currentOwner;
        set => this.RaiseAndSetIfChanged(ref _currentOwner, value);
    }

    /// <summary>
    /// Booker actif de la compagnie actuelle
    /// </summary>
    public BookerSnapshot? CurrentBooker
    {
        get => _currentBooker;
        set => this.RaiseAndSetIfChanged(ref _currentBooker, value);
    }

    /// <summary>
    /// Style de catch de la compagnie actuelle
    /// </summary>
    public CatchStyle? CurrentStyle
    {
        get => _currentStyle;
        set => this.RaiseAndSetIfChanged(ref _currentStyle, value);
    }

    /// <summary>
    /// Liste des compagnies rivales
    /// </summary>
    public ObservableCollection<CompanyState> RivalCompanies
    {
        get => _rivalCompanies;
        set => this.RaiseAndSetIfChanged(ref _rivalCompanies, value);
    }

    /// <summary>
    /// Texte du bouton de switch
    /// </summary>
    public string SwitchButtonText => IsViewingRival ? "📊 Ma Compagnie" : "👁️ Voir Rivales";

    /// <summary>
    /// Libellé de date affiché dans le header.
    /// </summary>
    public string CurrentDateLabel { get; set; } = "Lun, Sem 1, Jan 2026";

    /// <summary>
    /// Nom de compagnie ou libellé alternatif pour l'affichage.
    /// </summary>
    public string CompanyDisplayTitle => IsViewingRival ? "Analyse Rivaux" : CurrentCompany?.Nom ?? "Ma Compagnie";

    /// <summary>
    /// Texte du logo (initiales).
    /// </summary>
    public string CompanyLogoText => IsViewingRival ? "RG" : GetCompanyInitials(CurrentCompany?.Nom);

    /// <summary>
    /// Couleur du logo selon le mode.
    /// </summary>
    public string CompanyLogoBackground => IsViewingRival ? "#ef4444" : "#3b82f6";

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> SwitchToRivalsCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToMyCompanyCommand { get; }

    #endregion

    #region INavigableViewModel

    public void OnNavigatedTo(object? parameter)
    {
        // Charger la compagnie du joueur par défaut
        LoadPlayerCompany();
    }

    #endregion

    #region Data Loading

    /// <summary>
    /// Charge la compagnie du joueur
    /// </summary>
    private async void LoadPlayerCompany()
    {
        try
        {
            using var connection = _gameRepository.CreateConnection();
            using var cmd = connection.CreateCommand();

            // Charger la compagnie du joueur depuis la SaveGame active
            cmd.CommandText = @"
                SELECT c.CompanyId, c.Name, c.RegionId, c.Prestige, c.Treasury,
                       c.AverageAudience, c.Reach, c.FoundedYear, c.CompanySize,
                       c.CurrentEra, c.CatchStyleId, c.IsPlayerControlled, c.MonthlyBurnRate
                FROM Companies c
                INNER JOIN SaveGames sg ON c.CompanyId = sg.PlayerCompanyId
                WHERE sg.IsActive = 1
                LIMIT 1";

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                _currentCompanyId = reader.GetString(0);

                CurrentCompany = new CompanyState(
                    reader.GetString(0),  // CompagnieId
                    reader.GetString(1),  // Nom
                    reader.GetString(2),  // Region
                    reader.GetInt32(3),   // Prestige
                    reader.GetDouble(4),  // Tresorerie
                    reader.GetInt32(5),   // AudienceMoyenne
                    reader.GetInt32(6),   // Reach
                    reader.GetInt32(7),   // FoundedYear
                    reader.GetString(8),  // CompanySize
                    reader.GetString(9),  // CurrentEra
                    reader.IsDBNull(10) ? null : reader.GetString(10),  // CatchStyleId
                    reader.GetInt32(11) == 1,  // IsPlayerControlled
                    reader.GetDouble(12)  // MonthlyBurnRate
                );

                // Charger Owner, Booker, Style
                await LoadGovernanceData(_currentCompanyId);

                IsViewingRival = false;
                Logger.Info($"Company Hub chargé pour: {CurrentCompany.Nom}");
            }
            else
            {
                Logger.Error("[CompanyHubViewModel] Aucune compagnie joueur trouvée");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[CompanyHubViewModel] Erreur chargement compagnie: {ex.Message}");
        }
    }

    /// <summary>
    /// Charge les données de gouvernance (Owner, Booker, Style)
    /// </summary>
    private async System.Threading.Tasks.Task LoadGovernanceData(string companyId)
    {
        try
        {
            // Charger Owner
            var owner = await _ownerRepository.GetOwnerByCompanyIdAsync(companyId);
            if (owner != null)
            {
                CurrentOwner = new OwnerSnapshot(
                    owner.OwnerId,
                    owner.CompanyId,
                    owner.Name,
                    owner.VisionType,
                    owner.RiskTolerance,
                    owner.PreferredProductType,
                    owner.ShowFrequencyPreference,
                    owner.TalentDevelopmentFocus,
                    owner.FinancialPriority,
                    owner.FanSatisfactionPriority
                );
            }

            // Charger Booker actif
            var bookers = await _bookerRepository.GetActiveBookersByCompanyIdAsync(companyId);
            var activeBooker = bookers.FirstOrDefault();
            if (activeBooker != null)
            {
                CurrentBooker = new BookerSnapshot(
                    activeBooker.BookerId,
                    activeBooker.CompanyId,
                    activeBooker.Name,
                    activeBooker.CreativityScore,
                    activeBooker.LogicScore,
                    activeBooker.BiasResistance,
                    activeBooker.PreferredStyle,
                    activeBooker.LikesUnderdog,
                    activeBooker.LikesVeteran,
                    activeBooker.LikesFastRise,
                    activeBooker.LikesSlowBurn,
                    activeBooker.IsAutoBookingEnabled,
                    activeBooker.EmploymentStatus,
                    activeBooker.HireDate.ToString("yyyy-MM-dd")
                );
            }

            // Charger Style
            if (CurrentCompany?.CatchStyleId != null)
            {
                CurrentStyle = await _catchStyleRepository.GetStyleByIdAsync(CurrentCompany.CatchStyleId);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[CompanyHubViewModel] Erreur chargement gouvernance: {ex.Message}");
        }
    }

    #endregion

    #region Actions

    /// <summary>
    /// Switcher vers la vue des compagnies rivales
    /// </summary>
    private void SwitchToRivals()
    {
        // TODO: Charger la liste des compagnies rivales
        IsViewingRival = true;
    }

    /// <summary>
    /// Revenir à la vue de sa propre compagnie
    /// </summary>
    private void SwitchToMyCompany()
    {
        LoadPlayerCompany();
        IsViewingRival = false;
    }

    #endregion

    private static string GetCompanyInitials(string? companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            return "RG";
        }

        var parts = companyName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0].Length > 3 ? parts[0][..3].ToUpperInvariant() : parts[0].ToUpperInvariant();
        }

        var initials = string.Concat(parts.Select(part => part[0]));
        return initials.ToUpperInvariant();
    }
}
