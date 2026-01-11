using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Enums;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.Services.Navigation;

namespace RingGeneral.UI.ViewModels.CompanyHub;

/// <summary>
/// ViewModel principal du Company Hub avec navigation multi-tabs
/// </summary>
public sealed class CompanyHubViewModel : ViewModelBase, INavigableViewModel
{
    private readonly GameRepository _gameRepository;
    private readonly RingGeneral.Core.Interfaces.IOwnerRepository _ownerRepository;
    private readonly RingGeneral.Core.Interfaces.IBookerRepository _bookerRepository;
    private readonly ICatchStyleRepository _catchStyleRepository;
    private readonly IChildCompanyExtendedRepository _childCompanyRepository;
    private readonly IChildCompanyStaffService _childCompanyStaffService;
    private readonly IChildCompanyStaffRepository _childCompanyStaffRepository;
    private readonly IYouthRepository _youthRepository;

    private int _selectedTabIndex = 0;
    private bool _isViewingRival = false;
    private string? _currentCompanyId;
    private CompanyState? _currentCompany;
    private OwnerSnapshot? _currentOwner;
    private BookerSnapshot? _currentBooker;
    private CatchStyle? _currentStyle;
    private ObservableCollection<CompanyState> _rivalCompanies;
    private ObservableCollection<ChildCompanyExtended> _playerCompanies;
    private ObservableCollection<YouthStructureState> _youthStructures;

    private string _newYouthStructureName = string.Empty;
    private string _newYouthStructureType = "ACADEMY";

    public CompanyHubViewModel(
        GameRepository? gameRepository = null,
        RingGeneral.Core.Interfaces.IOwnerRepository? ownerRepository = null,
        RingGeneral.Core.Interfaces.IBookerRepository? bookerRepository = null,
        ICatchStyleRepository? catchStyleRepository = null,
        IChildCompanyExtendedRepository? childCompanyRepository = null,
        IChildCompanyStaffService? childCompanyStaffService = null,
        IChildCompanyStaffRepository? childCompanyStaffRepository = null,
        IYouthRepository? youthRepository = null)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _bookerRepository = bookerRepository ?? throw new ArgumentNullException(nameof(bookerRepository));
        _catchStyleRepository = catchStyleRepository ?? throw new ArgumentNullException(nameof(catchStyleRepository));
        _childCompanyRepository = childCompanyRepository ?? throw new ArgumentNullException(nameof(childCompanyRepository));
        _childCompanyStaffService = childCompanyStaffService ?? throw new ArgumentNullException(nameof(childCompanyStaffService));
        _childCompanyStaffRepository = childCompanyStaffRepository ?? throw new ArgumentNullException(nameof(childCompanyStaffRepository));
        _youthRepository = youthRepository ?? throw new ArgumentNullException(nameof(youthRepository));

        _rivalCompanies = new ObservableCollection<CompanyState>();
        _playerCompanies = new ObservableCollection<ChildCompanyExtended>();
        _youthStructures = new ObservableCollection<YouthStructureState>();

        // Prédicat pour HireStaffCommand : vérifie budget et absence de Manager
        // Le prédicat sera vérifié dynamiquement dans la commande avec le childCompanyId
        var canHireStaff = this.WhenAnyValue(
            x => x.CurrentCompany)
            .Select(company => company != null && company.Tresorerie > 50000);

        // Commandes
        SwitchToRivalsCommand = ReactiveCommand.Create(SwitchToRivals);
        SwitchToMyCompanyCommand = ReactiveCommand.Create(SwitchToMyCompany);
        HireStaffCommand = ReactiveCommand.CreateFromTask<string>(HireStaffAsync, canHireStaff);
        CreateYouthStructureCommand = ReactiveCommand.CreateFromTask(CreateYouthStructureAsync);
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
    /// Liste des filiales du joueur (ChildCompanies)
    /// </summary>
    public ObservableCollection<ChildCompanyExtended> PlayerCompanies
    {
        get => _playerCompanies;
        private set => this.RaiseAndSetIfChanged(ref _playerCompanies, value);
    }

    /// <summary>
    /// Liste des structures jeunesse
    /// </summary>
    public ObservableCollection<YouthStructureState> YouthStructures
    {
        get => _youthStructures;
        private set => this.RaiseAndSetIfChanged(ref _youthStructures, value);
    }

    public string NewYouthStructureName
    {
        get => _newYouthStructureName;
        set => this.RaiseAndSetIfChanged(ref _newYouthStructureName, value);
    }

    public string NewYouthStructureType
    {
        get => _newYouthStructureType;
        set => this.RaiseAndSetIfChanged(ref _newYouthStructureType, value);
    }

    public ObservableCollection<string> YouthStructureTypes { get; } = new ObservableCollection<string>
    {
        "ACADEMY",
        "DOJO",
        "PERFORMANCE_CENTER",
        "DEVELOPMENT"
    };

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
    public ReactiveCommand<string, Unit> HireStaffCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateYouthStructureCommand { get; }

    #endregion

    #region INavigableViewModel

    public void OnNavigatedTo(object? parameter)
    {
        // Charger la compagnie du joueur par défaut
        LoadPlayerCompany();
        // Charger les filiales du joueur
        _ = LoadPlayerCompaniesAsync();
        // Charger les structures jeunesse
        _ = LoadYouthStructuresAsync();
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

                // Charger les filiales du joueur
                await LoadPlayerCompaniesAsync();

                // Charger les structures jeunesse
                await LoadYouthStructuresAsync();

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
    /// Charge les filiales du joueur (ChildCompanies)
    /// </summary>
    private async Task LoadPlayerCompaniesAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_currentCompanyId))
            {
                // Récupérer le PlayerCompanyId depuis SaveGames
                using var connection = _gameRepository.CreateConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT PlayerCompanyId
                    FROM SaveGames
                    WHERE IsActive = 1
                    LIMIT 1";

                var result = cmd.ExecuteScalar();
                if (result == null)
                {
                    Logger.Warning("[CompanyHubViewModel] Aucune SaveGame active trouvée");
                    return;
                }

                _currentCompanyId = result.ToString() ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(_currentCompanyId))
            {
                return;
            }

            // Charger les filiales sur un thread de fond
            var companies = await Task.Run(async () =>
            {
                return await _childCompanyRepository.GetChildCompaniesByParentIdAsync(_currentCompanyId);
            });

            // Mettre à jour sur le thread UI
            PlayerCompanies.Clear();
            foreach (var company in companies)
            {
                PlayerCompanies.Add(company);
            }
            Logger.Info($"[CompanyHubViewModel] {PlayerCompanies.Count} filiales chargées");
        }
        catch (Exception ex)
        {
            Logger.Error($"[CompanyHubViewModel] Erreur chargement filiales: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Charge les structures jeunesse
    /// </summary>
    private async Task LoadYouthStructuresAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_currentCompanyId))
            {
                return;
            }

            // Charger les structures (maintenant via l'interface standard)
            var structures = await Task.Run(() =>
            {
                return _youthRepository.ChargerYouthStructures()
                    .Where(y => y.CompanyId == _currentCompanyId)
                    .ToList();
            });

            YouthStructures.Clear();
            foreach (var s in structures)
            {
                YouthStructures.Add(s);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[CompanyHubViewModel] Erreur chargement youth structures: {ex.Message}", ex);
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
            var activeBooker = await _bookerRepository.GetActiveBookerAsync(companyId);
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

    /// <summary>
    /// Crée une nouvelle structure jeunesse
    /// </summary>
    private async Task CreateYouthStructureAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_currentCompanyId))
            {
                Logger.Error("Impossible de créer une structure: CompanyId manquant");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewYouthStructureName))
            {
                // Idéalement afficher une erreur à l'utilisateur
                Logger.Warning("Nom de structure manquant");
                return;
            }

            var id = $"YS-{Guid.NewGuid():N}".ToUpperInvariant();

            // Valeurs par défaut basiques pour une création manuelle
            await _youthRepository.CreateYouthStructureAsync(
                id,
                _currentCompanyId,
                NewYouthStructureName,
                CurrentCompany?.RegionId, // Utilise la région de la compagnie mère
                NewYouthStructureType,
                100_000m, // Budget par défaut
                20,       // Capacité par défaut
                1,        // Niveau équipements
                10,       // Qualité coaching
                "HYBRIDE" // Philosophie
            );

            Logger.Info($"Structure jeunesse créée: {NewYouthStructureName}");

            // Reset form
            NewYouthStructureName = string.Empty;

            // Reload list
            await LoadYouthStructuresAsync();
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur création structure jeunesse: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Recrute un staff et l'assigne comme Manager d'une filiale
    /// </summary>
    private async Task HireStaffAsync(string childCompanyId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(childCompanyId))
            {
                Logger.Error("[CompanyHubViewModel] ChildCompanyId requis pour recruter un Manager");
                return;
            }

            // Vérifier que la filiale n'a pas déjà de Manager
            var activeAssignments = await Task.Run(async () =>
                await _childCompanyStaffRepository.GetActiveStaffAssignmentsAsync(childCompanyId));
            
            var hasManager = activeAssignments.Any(a =>
                a.AssignmentType == StaffAssignmentType.DedicatedRotation &&
                a.TimePercentage >= 1.0 &&
                (a.MissionObjective?.Contains("Manager", StringComparison.OrdinalIgnoreCase) == true || 
                 a.MissionObjective?.Contains("Gestionnaire", StringComparison.OrdinalIgnoreCase) == true));

            if (hasManager)
            {
                Logger.Warning($"[CompanyHubViewModel] La filiale {childCompanyId} a déjà un Manager");
                return;
            }

            // Vérifier le budget
            if (CurrentCompany == null || CurrentCompany.Tresorerie < 50000) // Coût minimum estimé
            {
                Logger.Warning("[CompanyHubViewModel] Budget insuffisant pour recruter un Manager");
                return;
            }

            // TODO: Récupérer le StaffMember à recruter depuis une sélection ou un paramètre
            // Pour l'instant, on génère un ID temporaire pour la démonstration
            // Dans une implémentation complète, il faudrait ouvrir une fenêtre de sélection de staff
            var tempStaffId = Guid.NewGuid().ToString("N");
            
            Logger.Info($"[CompanyHubViewModel] Recrutement Manager pour filiale {childCompanyId}");
            
            // Assigner le staff comme Manager
            var result = await _childCompanyStaffService.AssignStaffToChildCompanyAsync(
                staffId: tempStaffId,
                childCompanyId: childCompanyId,
                assignmentType: StaffAssignmentType.DedicatedRotation,
                timePercentage: 1.0,
                missionObjective: "Manager"
            );

            if (result.Success)
            {
                Logger.Info($"[CompanyHubViewModel] Manager assigné avec succès à la filiale {childCompanyId}");
                // Recharger les filiales après l'assignation
                await LoadPlayerCompaniesAsync();
            }
            else
            {
                Logger.Error($"[CompanyHubViewModel] Erreur lors de l'assignation: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[CompanyHubViewModel] Erreur recrutement Manager: {ex.Message}", ex);
        }
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
