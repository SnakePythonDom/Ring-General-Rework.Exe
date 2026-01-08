using System.Reactive;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.UI.ViewModels.Dashboard;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Start;

/// <summary>
/// ViewModel pour l'écran de démarrage (Menu Principal)
/// </summary>
public sealed class StartViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly GameRepository _repository;

    public StartViewModel(INavigationService navigationService, GameRepository repository)
    {
        _navigationService = navigationService;
        _repository = repository;

        // Commandes
        NewGameCommand = ReactiveCommand.Create(StartNewGame);
        LoadGameCommand = ReactiveCommand.Create(LoadExistingGame);
        ExitCommand = ReactiveCommand.Create(ExitApplication);

        // Vérifier s'il existe une sauvegarde active
        CheckForActiveSave();
    }

    /// <summary>
    /// Commande pour démarrer une nouvelle partie
    /// </summary>
    public ReactiveCommand<Unit, Unit> NewGameCommand { get; }

    /// <summary>
    /// Commande pour charger une partie existante
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadGameCommand { get; }

    /// <summary>
    /// Commande pour quitter l'application
    /// </summary>
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }

    /// <summary>
    /// Indique s'il existe une sauvegarde active
    /// </summary>
    private bool _hasActiveSave;
    public bool HasActiveSave
    {
        get => _hasActiveSave;
        private set => this.RaiseAndSetIfChanged(ref _hasActiveSave, value);
    }

    /// <summary>
    /// Nom de la sauvegarde active
    /// </summary>
    private string _activeSaveName = string.Empty;
    public string ActiveSaveName
    {
        get => _activeSaveName;
        private set => this.RaiseAndSetIfChanged(ref _activeSaveName, value);
    }

    /// <summary>
    /// Vérifie s'il existe une sauvegarde active
    /// </summary>
    private void CheckForActiveSave()
    {
        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT SaveName
                FROM SaveGames
                WHERE IsActive = 1
                LIMIT 1";

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                HasActiveSave = true;
                ActiveSaveName = reader.GetString(0);
            }
            else
            {
                HasActiveSave = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[StartViewModel] Erreur lors de la vérification de sauvegarde: {ex.Message}");
            HasActiveSave = false;
        }
    }

    /// <summary>
    /// Démarre une nouvelle partie
    /// </summary>
    private void StartNewGame()
    {
        Logger.Info("Démarrage d'une nouvelle partie...");

        // Naviguer vers le sélecteur de compagnie
        _navigationService.NavigateTo<CompanySelectorViewModel>();
    }

    /// <summary>
    /// Charge une partie existante
    /// </summary>
    private void LoadExistingGame()
    {
        Logger.Info("Chargement de la partie existante...");

        // TODO: Pour l'instant, on charge directement la sauvegarde active
        // Plus tard: afficher une liste de sauvegardes
        if (HasActiveSave)
        {
            // Naviguer vers le Dashboard (tableau de bord)
            _navigationService.NavigateTo<DashboardViewModel>();
        }
    }

    /// <summary>
    /// Quitte l'application
    /// </summary>
    private void ExitApplication()
    {
        Logger.Info("Fermeture de l'application...");
        System.Environment.Exit(0);
    }
}
