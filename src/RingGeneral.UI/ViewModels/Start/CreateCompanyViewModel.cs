using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.Services.Navigation;

namespace RingGeneral.UI.ViewModels.Start;

/// <summary>
/// ViewModel pour la création d'une nouvelle compagnie personnalisée
/// </summary>
public sealed class CreateCompanyViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private readonly INavigationService _navigationService;

    private string _companyName = string.Empty;
    private string _selectedRegion = "USA";
    private int _startingPrestige = 50;
    private double _startingTreasury = 100000.0;
    private string? _errorMessage;

    public CreateCompanyViewModel(
        GameRepository? repository = null,
        INavigationService? navigationService = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        // Initialiser les régions disponibles
        AvailableRegions = new ObservableCollection<string>
        {
            "USA",
            "Japan",
            "Mexico",
            "UK",
            "Canada",
            "France",
            "Germany",
            "Australia",
            "Brazil",
            "Other"
        };

        // Commandes
        CreateCompanyCommand = ReactiveCommand.Create(CreateCompany);
        CancelCommand = ReactiveCommand.Create(Cancel);
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
    public string SelectedRegion
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
    public ObservableCollection<string> AvailableRegions { get; }

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

        if (string.IsNullOrWhiteSpace(SelectedRegion))
        {
            ErrorMessage = "Veuillez sélectionner une région.";
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();

            // Générer un ID unique
            var companyId = $"COMP_CUSTOM_{Guid.NewGuid():N}".Substring(0, 20);

            // Insérer la nouvelle compagnie
            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Companies (CompanyId, Name, Region, Prestige, Treasury, CurrentWeek, IsPlayerControlled)
                VALUES (@companyId, @name, @region, @prestige, @treasury, @week, 1)";

            insertCmd.Parameters.AddWithValue("@companyId", companyId);
            insertCmd.Parameters.AddWithValue("@name", CompanyName.Trim());
            insertCmd.Parameters.AddWithValue("@region", SelectedRegion);
            insertCmd.Parameters.AddWithValue("@prestige", StartingPrestige);
            insertCmd.Parameters.AddWithValue("@treasury", StartingTreasury);
            insertCmd.Parameters.AddWithValue("@week", 1);

            insertCmd.ExecuteNonQuery();

            System.Console.WriteLine($"[CreateCompanyViewModel] Compagnie créée: {CompanyName} ({companyId})");

            // Créer la sauvegarde
            CreateSaveGame(connection, companyId);

            // Naviguer vers le Shell (tableau de bord)
            _navigationService.NavigateTo<Core.ShellViewModel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de la création: {ex.Message}";
            System.Console.Error.WriteLine($"[CreateCompanyViewModel] Erreur: {ex.Message}");
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

        System.Console.WriteLine("[CreateCompanyViewModel] Sauvegarde créée avec succès");
    }

    /// <summary>
    /// Annule et retourne au sélecteur de compagnies
    /// </summary>
    private void Cancel()
    {
        _navigationService.NavigateTo<CompanySelectorViewModel>();
    }
}
