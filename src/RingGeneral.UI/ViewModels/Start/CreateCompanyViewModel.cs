using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Owner;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.ViewModels.Dashboard;

namespace RingGeneral.UI.ViewModels.Start;

/// <summary>
/// ViewModel pour la création d'une nouvelle compagnie personnalisée avec gouvernance complète
/// </summary>
public sealed class CreateCompanyViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private readonly INavigationService _navigationService;
    private readonly IOwnerRepository _ownerRepository;
    private readonly IBookerRepository _bookerRepository;
    private readonly ICatchStyleRepository _catchStyleRepository;
    private readonly IRegionRepository _regionRepository;

    private string _companyName = string.Empty;
    private RegionInfo? _selectedRegion;
    private CatchStyle? _selectedCatchStyle;
    private int _startingPrestige = 50;
    private double _startingTreasury = 100000.0;
    private int _foundedYear = 2024;
    private string? _errorMessage;
    private readonly int _baseStartingPrestige = 50;
    private readonly double _baseStartingTreasury = 100000.0;

    public CreateCompanyViewModel(
        GameRepository? repository = null,
        INavigationService? navigationService = null,
        IOwnerRepository? ownerRepository = null,
        IBookerRepository? bookerRepository = null,
        ICatchStyleRepository? catchStyleRepository = null,
        IRegionRepository? regionRepository = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _bookerRepository = bookerRepository ?? throw new ArgumentNullException(nameof(bookerRepository));
        _catchStyleRepository = catchStyleRepository ?? throw new ArgumentNullException(nameof(catchStyleRepository));
        _regionRepository = regionRepository ?? throw new ArgumentNullException(nameof(regionRepository));

        // Initialiser les données de sélection
        AvailableRegions = new ObservableCollection<RegionInfo>();
        AvailableCatchStyles = new ObservableCollection<CatchStyle>();

        // Commandes
        var canCreateCompany = this.WhenAnyValue(
            vm => vm.CompanyName,
            vm => vm.SelectedRegion,
            vm => vm.FoundedYear,
            (name, region, year) => !string.IsNullOrWhiteSpace(name)
                                    && region != null
                                    && year is >= 1950 and <= 2100);

        ContinueCommand = ReactiveCommand.Create(CreateCompany, canCreateCompany);
        CreateCompanyCommand = ContinueCommand;
        CancelCommand = ReactiveCommand.Create(Cancel);

        this.WhenAnyValue(vm => vm.SelectedCatchStyle)
            .Subscribe(ApplyStyleModifiers);

        LoadRegionsFromDatabase();
        LoadCatchStylesFromDatabase();
    }

    /// <summary>
    /// Charge les régions depuis la base de données
    /// </summary>
    private void LoadRegionsFromDatabase()
    {
        try
        {
            var regions = _regionRepository.GetRegions();
            foreach (var region in regions)
            {
                AvailableRegions.Add(new RegionInfo(region.RegionId, region.RegionName, region.CountryName));
            }

            // Sélectionner la première région par défaut
            if (AvailableRegions.Count > 0)
            {
                SelectedRegion = AvailableRegions.FirstOrDefault(r => r.CountryName.Contains("United States"))
                              ?? AvailableRegions[0];
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[CreateCompanyViewModel] Erreur lors du chargement des régions: {ex.Message}");

            // Ajouter une région par défaut en cas d'erreur
            AvailableRegions.Add(new RegionInfo("REGION_DEFAULT", "Global", "World"));
            SelectedRegion = AvailableRegions[0];
        }
    }

    /// <summary>
    /// Charge les styles de catch depuis la base de données
    /// </summary>
    private async void LoadCatchStylesFromDatabase()
    {
        try
        {
            var styles = await _catchStyleRepository.GetAllActiveStylesAsync();
            foreach (var style in styles)
            {
                AvailableCatchStyles.Add(style);
            }

            // Sélectionner "Hybrid" par défaut (le plus équilibré)
            SelectedCatchStyle = AvailableCatchStyles.FirstOrDefault(s => s.CatchStyleId == "STYLE_HYBRID")
                              ?? AvailableCatchStyles.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Logger.Error($"[CreateCompanyViewModel] Erreur lors du chargement des styles: {ex.Message}");

            // Créer un style par défaut en cas d'erreur (fallback)
            var defaultStyle = new CatchStyle(
                "STYLE_HYBRID",
                "Hybrid Wrestling",
                "Style équilibré par défaut",
                60, 60, 20, 30, 30,  // Characteristics
                65, 65, 60, 65,      // Fan Expectations
                1.0, 1.0,            // Multipliers
                "🌐", "#607D8B", true);
            AvailableCatchStyles.Add(defaultStyle);
            SelectedCatchStyle = defaultStyle;
        }
    }

    /// <summary>
    /// Nom de la compagnie
    /// </summary>
    public string CompanyName
    {
        get => _companyName;
        set => this.RaiseAndSetIfChanged(ref _companyName, value);
    }

    /// <summary>
    /// Région sélectionnée
    /// </summary>
    public RegionInfo? SelectedRegion
    {
        get => _selectedRegion;
        set => this.RaiseAndSetIfChanged(ref _selectedRegion, value);
    }

    /// <summary>
    /// Style de catch sélectionné
    /// </summary>
    public CatchStyle? SelectedCatchStyle
    {
        get => _selectedCatchStyle;
        set => this.RaiseAndSetIfChanged(ref _selectedCatchStyle, value);
    }

    /// <summary>
    /// Prestige de départ (0-100)
    /// </summary>
    public int StartingPrestige
    {
        get => _startingPrestige;
        set => this.RaiseAndSetIfChanged(ref _startingPrestige, Math.Clamp(value, 0, 100));
    }

    /// <summary>
    /// Trésorerie de départ
    /// </summary>
    public double StartingTreasury
    {
        get => _startingTreasury;
        set => this.RaiseAndSetIfChanged(ref _startingTreasury, Math.Max(0, value));
    }

    /// <summary>
    /// Année de fondation (1950-2100)
    /// </summary>
    public int FoundedYear
    {
        get => _foundedYear;
        set => this.RaiseAndSetIfChanged(ref _foundedYear, Math.Clamp(value, 1950, 2100));
    }

    /// <summary>
    /// Message d'erreur en cas de validation échouée
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    /// <summary>
    /// Liste des régions disponibles
    /// </summary>
    public ObservableCollection<RegionInfo> AvailableRegions { get; }

    /// <summary>
    /// Liste des styles de catch disponibles
    /// </summary>
    public ObservableCollection<CatchStyle> AvailableCatchStyles { get; }

    /// <summary>
    /// Commande pour créer la compagnie
    /// </summary>
    public ReactiveCommand<Unit, Unit> CreateCompanyCommand { get; }

    /// <summary>
    /// Commande pour continuer la création
    /// </summary>
    public ReactiveCommand<Unit, Unit> ContinueCommand { get; }

    /// <summary>
    /// Commande pour annuler et retourner
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public void ApplyDefaultTemplate()
    {
        CompanyName = "Nouvelle Compagnie";
        FoundedYear = 2024;
        StartingPrestige = _baseStartingPrestige;
        StartingTreasury = _baseStartingTreasury;

        if (AvailableRegions.Count > 0 && SelectedRegion == null)
        {
            SelectedRegion = AvailableRegions[0];
        }

        if (AvailableCatchStyles.Count > 0 && SelectedCatchStyle == null)
        {
            SelectedCatchStyle = AvailableCatchStyles[0];
        }
    }

    private void ApplyStyleModifiers(CatchStyle? style)
    {
        var prestigeMultiplier = 1.0;
        var treasuryMultiplier = 1.0;

        if (style?.Name == "Hardcore")
        {
            prestigeMultiplier = 0.8;
            treasuryMultiplier = 1.2;
        }
        else if (style?.Name == "Pure Wrestling")
        {
            prestigeMultiplier = 1.2;
            treasuryMultiplier = 0.9;
        }

        var adjustedPrestige = (int)Math.Round(_baseStartingPrestige * prestigeMultiplier);
        StartingPrestige = Math.Clamp(adjustedPrestige, 0, 100);
        StartingTreasury = Math.Max(0, _baseStartingTreasury * treasuryMultiplier);
    }

    /// <summary>
    /// Crée la nouvelle compagnie dans la base de données
    /// </summary>
    private void CreateCompany()
    {
        ErrorMessage = null;

        // Validation
        if (string.IsNullOrWhiteSpace(CompanyName))
        {
            ErrorMessage = "Le nom de la compagnie est requis.";
            return;
        }

        if (CompanyName.Length < 3)
        {
            ErrorMessage = "Le nom de la compagnie doit contenir au moins 3 caractères.";
            return;
        }

        if (SelectedRegion == null)
        {
            ErrorMessage = "Veuillez sélectionner une région.";
            return;
        }

        if (SelectedCatchStyle == null)
        {
            ErrorMessage = "Veuillez sélectionner un style de catch.";
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();

            // Générer un ID unique
            var companyId = $"COMP_CUSTOM_{Guid.NewGuid():N}".Substring(0, 20);

            // Récupérer le CountryId de la région sélectionnée
            string? countryId = null;
            using (var countryCmd = connection.CreateCommand())
            {
                countryCmd.CommandText = "SELECT CountryId FROM Regions WHERE RegionId = @regionId";
                countryCmd.Parameters.AddWithValue("@regionId", SelectedRegion.RegionId);
                countryId = countryCmd.ExecuteScalar() as string;
            }

            if (countryId == null)
            {
                ErrorMessage = "Erreur: Impossible de déterminer le pays de la région sélectionnée.";
                return;
            }

            // Insérer la nouvelle compagnie avec tous les champs d'identité et de gouvernance
            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Companies (
                    CompanyId, Name, CountryId, RegionId, Prestige, Treasury,
                    FoundedYear, CompanySize, CurrentEra, CatchStyleId, IsPlayerControlled, MonthlyBurnRate
                ) VALUES (
                    @companyId, @name, @countryId, @regionId, @prestige, @treasury,
                    @foundedYear, @companySize, @currentEra, @catchStyleId, @isPlayerControlled, @burnRate
                )";

            insertCmd.Parameters.AddWithValue("@companyId", companyId);
            insertCmd.Parameters.AddWithValue("@name", CompanyName.Trim());
            insertCmd.Parameters.AddWithValue("@countryId", countryId!);
            insertCmd.Parameters.AddWithValue("@regionId", SelectedRegion.RegionId);
            insertCmd.Parameters.AddWithValue("@prestige", StartingPrestige);
            insertCmd.Parameters.AddWithValue("@treasury", StartingTreasury);
            insertCmd.Parameters.AddWithValue("@foundedYear", FoundedYear);
            insertCmd.Parameters.AddWithValue("@companySize", "Local"); // Taille initiale
            insertCmd.Parameters.AddWithValue("@currentEra", "Foundation Era");
            insertCmd.Parameters.AddWithValue("@catchStyleId", SelectedCatchStyle.CatchStyleId);
            insertCmd.Parameters.AddWithValue("@isPlayerControlled", 1); // C'est la compagnie du joueur
            insertCmd.Parameters.AddWithValue("@burnRate", 5000.0); // Burn rate initial modéré

            insertCmd.ExecuteNonQuery();

            Logger.Info($"Compagnie créée: {CompanyName} ({companyId})");

            // Créer l'Owner (contrôleur stratégique)
            var ownerId = $"OWN_{Guid.NewGuid():N}".Substring(0, 16);
            CreateDefaultOwner(companyId, ownerId).Wait();
            Logger.Info($"Owner créé: {ownerId}");

            // Créer le Booker (directeur créatif)
            var bookerId = $"BOOK_{Guid.NewGuid():N}".Substring(0, 16);
            CreateDefaultBooker(companyId, bookerId).Wait();
            Logger.Info($"Booker créé: {bookerId}");

            // Créer la sauvegarde
            CreateSaveGame(connection, companyId);

            // Naviguer vers le Dashboard (tableau de bord)
            _navigationService.NavigateTo<DashboardViewModel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de la création: {ex.Message}";
            Logger.Error($"[CreateCompanyViewModel] Erreur: {ex.Message}");
            Logger.Error($"[CreateCompanyViewModel] Stack: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Crée une nouvelle sauvegarde pour la compagnie créée
    /// </summary>
    private void CreateSaveGame(SqliteConnection connection, string companyId)
    {
        // Désactiver toutes les sauvegardes existantes
        using (var deactivateCmd = connection.CreateCommand())
        {
            deactivateCmd.CommandText = "UPDATE SaveGames SET IsActive = 0";
            deactivateCmd.ExecuteNonQuery();
        }

        // Créer la nouvelle sauvegarde
        using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO SaveGames (SaveName, PlayerCompanyId, CurrentWeek, CurrentDate, IsActive)
            VALUES (@saveName, @companyId, @week, @date, 1)";

        insertCmd.Parameters.AddWithValue("@saveName", $"{CompanyName} - {DateTime.Now:yyyy-MM-dd HH:mm}");
        insertCmd.Parameters.AddWithValue("@companyId", companyId);
        insertCmd.Parameters.AddWithValue("@week", 1);
        insertCmd.Parameters.AddWithValue("@date", $"{FoundedYear}-01-01");

        insertCmd.ExecuteNonQuery();

        Logger.Info("Sauvegarde créée avec succès");
    }

    /// <summary>
    /// Crée un Owner par défaut aligné avec le style de catch choisi
    /// </summary>
    private async System.Threading.Tasks.Task CreateDefaultOwner(string companyId, string ownerId)
    {
        // Mapper le CatchStyle vers PreferredProductType de l'Owner
        var productType = SelectedCatchStyle!.Name switch
        {
            "Pure Wrestling" or "Strong Style" => "Technical",
            "Sports Entertainment" or "Family-Friendly" => "Entertainment",
            "Hardcore Wrestling" => "Hardcore",
            "Lucha Libre" => "Entertainment",
            _ => "Entertainment"
        };

        var owner = new Owner
        {
            OwnerId = ownerId,
            CompanyId = companyId,
            Name = "Owner",  // Le joueur pourra personnaliser plus tard
            VisionType = "Balanced",
            RiskTolerance = 50,
            PreferredProductType = productType,
            ShowFrequencyPreference = "Weekly",
            TalentDevelopmentFocus = 50,
            FinancialPriority = 50,
            FanSatisfactionPriority = 50,
            CreatedAt = DateTime.Now
        };

        await _ownerRepository.SaveOwnerAsync(owner);
    }

    /// <summary>
    /// Crée un Booker par défaut équilibré
    /// </summary>
    private async System.Threading.Tasks.Task CreateDefaultBooker(string companyId, string bookerId)
    {
        var booker = new RingGeneral.Core.Models.Booker.Booker
        {
            BookerId = bookerId,
            CompanyId = companyId,
            Name = "Head Booker",  // Le joueur pourra personnaliser plus tard
            CreativityScore = 60,
            LogicScore = 70,
            BiasResistance = 60,
            PreferredStyle = "Flexible",
            LikesUnderdog = true,
            LikesVeteran = false,
            LikesFastRise = false,
            LikesSlowBurn = true,
            IsAutoBookingEnabled = false,  // Désactivé par défaut (le joueur book manuellement)
            EmploymentStatus = "Active",
            HireDate = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        await _bookerRepository.SaveBookerAsync(booker);
    }

    /// <summary>
    /// Annule et retourne au menu principal
    /// </summary>
    private void Cancel()
    {
        _navigationService.NavigateTo<StartViewModel>();
    }
}

/// <summary>
/// Informations sur une région pour la sélection
/// </summary>
public sealed record RegionInfo(string RegionId, string RegionName, string CountryName)
{
    public override string ToString() => $"{RegionName}, {CountryName}";
}
