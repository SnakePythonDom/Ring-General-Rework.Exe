using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels.Start;

/// <summary>
/// ViewModel pour la sélection de compagnie au démarrage
/// </summary>
public sealed class CompanySelectorViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly GameRepository _repository;
    private CompanyListItem? _selectedCompany;
    private bool _isLoading;

    public CompanySelectorViewModel(INavigationService navigationService, GameRepository repository)
    {
        _navigationService = navigationService;
        _repository = repository;

        Companies = new ObservableCollection<CompanyListItem>();

        // Commandes (async)
        SelectCompanyCommand = ReactiveCommand.CreateFromTask(StartGameWithSelectedCompanyAsync);
        CreateNewCompanyCommand = ReactiveCommand.Create(CreateNewCompany);
        BackCommand = ReactiveCommand.Create(GoBack);

        // Charger les compagnies de manière asynchrone
        _ = LoadCompaniesAsync();
    }

    /// <summary>
    /// Indique si le chargement est en cours
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    /// <summary>
    /// Liste des compagnies disponibles
    /// </summary>
    public ObservableCollection<CompanyListItem> Companies { get; }

    /// <summary>
    /// Compagnie sélectionnée
    /// </summary>
    public CompanyListItem? SelectedCompany
    {
        get => _selectedCompany;
        set => this.RaiseAndSetIfChanged(ref _selectedCompany, value);
    }

    /// <summary>
    /// Commande pour démarrer avec la compagnie sélectionnée
    /// </summary>
    public ReactiveCommand<Unit, Unit> SelectCompanyCommand { get; }

    /// <summary>
    /// Commande pour créer une nouvelle compagnie
    /// </summary>
    public ReactiveCommand<Unit, Unit> CreateNewCompanyCommand { get; }

    /// <summary>
    /// Commande pour retourner au menu principal
    /// </summary>
    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    /// <summary>
    /// Charge la liste des compagnies depuis la base de données (asynchrone)
    /// </summary>
    private async Task LoadCompaniesAsync()
    {
        IsLoading = true;
        System.Console.WriteLine("[CompanySelectorViewModel] Début du chargement des compagnies...");

        try
        {
            // Exécuter la requête DB dans un thread en arrière-plan
            var companies = await Task.Run(() =>
            {
                var result = new List<CompanyListItem>();

                using var connection = _repository.CreateConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT CompanyId, Name, Region, Prestige, Treasury
                    FROM Companies
                    ORDER BY Prestige DESC
                    LIMIT 50";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new CompanyListItem
                    {
                        CompanyId = reader.GetString(0),
                        Name = reader.GetString(1),
                        Region = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),
                        Prestige = reader.IsDBNull(3) ? 50 : reader.GetInt32(3),
                        Treasury = reader.IsDBNull(4) ? 1000000.0 : reader.GetDouble(4)
                    });
                }

                return result;
            });

            // Mettre à jour l'UI sur le thread principal
            Companies.Clear();
            foreach (var company in companies)
            {
                Companies.Add(company);
            }

            System.Console.WriteLine($"[CompanySelectorViewModel] {Companies.Count} compagnies chargées avec succès");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[CompanySelectorViewModel] Erreur lors du chargement: {ex.Message}");
            System.Console.Error.WriteLine($"[CompanySelectorViewModel] Stack trace: {ex.StackTrace}");

            // Ajouter une compagnie par défaut en cas d'erreur
            Companies.Clear();
            Companies.Add(new CompanyListItem
            {
                CompanyId = "COMP_DEFAULT",
                Name = "Default Wrestling Company",
                Region = "USA",
                Prestige = 50,
                Treasury = 1000000.0
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Démarre le jeu avec la compagnie sélectionnée (asynchrone)
    /// </summary>
    private async Task StartGameWithSelectedCompanyAsync()
    {
        if (SelectedCompany == null)
        {
            System.Console.WriteLine("[CompanySelectorViewModel] Aucune compagnie sélectionnée");
            return;
        }

        System.Console.WriteLine($"[CompanySelectorViewModel] Démarrage avec {SelectedCompany.Name}...");
        IsLoading = true;

        try
        {
            // Exécuter les opérations DB dans un thread en arrière-plan
            await Task.Run(() =>
            {
                using var connection = _repository.CreateConnection();

                // Désactiver toutes les sauvegardes existantes
                using (var deactivateCmd = connection.CreateCommand())
                {
                    deactivateCmd.CommandText = "UPDATE SaveGames SET IsActive = 0";
                    deactivateCmd.ExecuteNonQuery();
                }

                // Créer la nouvelle sauvegarde
                using (var insertCmd = connection.CreateCommand())
                {
                    insertCmd.CommandText = @"
                        INSERT INTO SaveGames (SaveName, PlayerCompanyId, CurrentWeek, CurrentDate, IsActive)
                        VALUES (@saveName, @companyId, @week, @date, 1)";

                    insertCmd.Parameters.AddWithValue("@saveName", $"{SelectedCompany.Name} - {DateTime.Now:yyyy-MM-dd HH:mm}");
                    insertCmd.Parameters.AddWithValue("@companyId", SelectedCompany.CompanyId);
                    insertCmd.Parameters.AddWithValue("@week", 1);
                    insertCmd.Parameters.AddWithValue("@date", "2024-01-01");

                    insertCmd.ExecuteNonQuery();
                }

                // Marquer la compagnie comme contrôlée par le joueur
                using (var updateCmd = connection.CreateCommand())
                {
                    updateCmd.CommandText = @"
                        UPDATE Companies
                        SET IsPlayerControlled = 1
                        WHERE CompanyId = @companyId";

                    updateCmd.Parameters.AddWithValue("@companyId", SelectedCompany.CompanyId);
                    updateCmd.ExecuteNonQuery();
                }

                System.Console.WriteLine("[CompanySelectorViewModel] Sauvegarde créée avec succès");
            });

            // Naviguer vers le Shell (tableau de bord) - sur le thread UI
            _navigationService.NavigateTo<Core.ShellViewModel>();
            System.Console.WriteLine("[CompanySelectorViewModel] Navigation vers Shell effectuée");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[CompanySelectorViewModel] Erreur lors de la création: {ex.Message}");
            System.Console.Error.WriteLine($"[CompanySelectorViewModel] Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Ouvre le formulaire de création de compagnie
    /// </summary>
    private void CreateNewCompany()
    {
        System.Console.WriteLine("[CompanySelectorViewModel] Création d'une nouvelle compagnie...");
        _navigationService.NavigateTo<CreateCompanyViewModel>();
    }

    /// <summary>
    /// Retourne au menu principal
    /// </summary>
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}

/// <summary>
/// Item de liste pour une compagnie
/// </summary>
public sealed class CompanyListItem : ViewModelBase
{
    private string _companyId = string.Empty;
    private string _name = string.Empty;
    private string _region = string.Empty;
    private int _prestige;
    private double _treasury;

    public string CompanyId
    {
        get => _companyId;
        set => this.RaiseAndSetIfChanged(ref _companyId, value);
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string Region
    {
        get => _region;
        set => this.RaiseAndSetIfChanged(ref _region, value);
    }

    public int Prestige
    {
        get => _prestige;
        set => this.RaiseAndSetIfChanged(ref _prestige, value);
    }

    public double Treasury
    {
        get => _treasury;
        set => this.RaiseAndSetIfChanged(ref _treasury, value);
    }

    public string TreasuryFormatted => $"${Treasury:N0}";
    public string PrestigeDisplay => $"★ {Prestige}/100";
}
