using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.ViewModels.Dashboard;

namespace RingGeneral.UI.ViewModels.Start;

/// <summary>
/// ViewModel pour la création d'une nouvelle compagnie personnalisée
/// </summary>
public sealed class CreateCompanyViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private readonly INavigationService _navigationService;

    private string _companyName = string.Empty;
    private RegionInfo? _selectedRegion;
    private int _startingPrestige = 50;
    private double _startingTreasury = 100000.0;
    private string? _errorMessage;

    public CreateCompanyViewModel(
        GameRepository? repository = null,
        INavigationService? navigationService = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        // Initialiser les régions disponibles depuis la base de données
        AvailableRegions = new ObservableCollection<RegionInfo>();
        LoadRegionsFromDatabase();

        // Commandes
        CreateCompanyCommand = ReactiveCommand.Create(CreateCompany);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    /// <summary>
    /// Charge les régions depuis la base de données
    /// </summary>
    private void LoadRegionsFromDatabase()
    {
        try
        {
            using var connection = _repository.CreateConnection();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT r.RegionId, r.Name, c.Name as CountryName
                FROM Regions r
                INNER JOIN Countries c ON c.CountryId = r.CountryId
                ORDER BY c.Name, r.Name
                LIMIT 500";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var regionId = reader.GetString(0);
                var regionName = reader.GetString(1);
                var countryName = reader.GetString(2);

                AvailableRegions.Add(new RegionInfo(regionId, regionName, countryName));
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
    /// Commande pour créer la compagnie
    /// </summary>
    public ReactiveCommand<Unit, Unit> CreateCompanyCommand { get; }

    /// <summary>
    /// Commande pour annuler et retourner
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

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

            // Insérer la nouvelle compagnie
            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury)
                VALUES (@companyId, @name, @countryId, @regionId, @prestige, @treasury)";

            insertCmd.Parameters.AddWithValue("@companyId", companyId);
            insertCmd.Parameters.AddWithValue("@name", CompanyName.Trim());
            insertCmd.Parameters.AddWithValue("@countryId", countryId);
            insertCmd.Parameters.AddWithValue("@regionId", SelectedRegion.RegionId);
            insertCmd.Parameters.AddWithValue("@prestige", StartingPrestige);
            insertCmd.Parameters.AddWithValue("@treasury", StartingTreasury);

            insertCmd.ExecuteNonQuery();

            Logger.Info($"Compagnie créée: {CompanyName} ({companyId})");

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
        insertCmd.Parameters.AddWithValue("@date", "2024-01-01");

        insertCmd.ExecuteNonQuery();

        Logger.Info("Sauvegarde créée avec succès");
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
