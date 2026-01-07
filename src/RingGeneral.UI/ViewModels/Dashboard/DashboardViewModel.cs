using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Dashboard;

/// <summary>
/// ViewModel pour la page d'accueil (Dashboard)
/// Affiche un récapitulatif de l'état actuel de la compagnie
/// </summary>
public sealed class DashboardViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private string _companyName = "Ma Compagnie";
    private int _currentWeek = 1;
    private int _totalWorkers;
    private int _activeStorylines;
    private int _upcomingShows;
    private decimal _currentBudget;
    private string _latestNews = "Bienvenue dans Ring General !";

    public DashboardViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        // Données par défaut (seront remplacées par les vraies données)
        TotalWorkers = 0;
        ActiveStorylines = 0;
        UpcomingShows = 0;
        CurrentBudget = 1_000_000m;

        RecentActivity = new ObservableCollection<string>
        {
            "🎯 Application démarrée avec succès",
            "📊 Données en cours de chargement...",
            "⚠️ Veuillez importer une base de données ou créer une nouvelle partie"
        };

        // Charger les données au démarrage
        LoadDashboardData();
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
    /// Semaine actuelle
    /// </summary>
    public int CurrentWeek
    {
        get => _currentWeek;
        set => this.RaiseAndSetIfChanged(ref _currentWeek, value);
    }

    /// <summary>
    /// Nombre total de workers
    /// </summary>
    public int TotalWorkers
    {
        get => _totalWorkers;
        set => this.RaiseAndSetIfChanged(ref _totalWorkers, value);
    }

    /// <summary>
    /// Nombre de storylines actives
    /// </summary>
    public int ActiveStorylines
    {
        get => _activeStorylines;
        set => this.RaiseAndSetIfChanged(ref _activeStorylines, value);
    }

    /// <summary>
    /// Nombre de shows à venir
    /// </summary>
    public int UpcomingShows
    {
        get => _upcomingShows;
        set => this.RaiseAndSetIfChanged(ref _upcomingShows, value);
    }

    /// <summary>
    /// Budget actuel
    /// </summary>
    public decimal CurrentBudget
    {
        get => _currentBudget;
        set => this.RaiseAndSetIfChanged(ref _currentBudget, value);
    }

    /// <summary>
    /// Budget formaté
    /// </summary>
    public string CurrentBudgetFormatted => $"${CurrentBudget:N0}";

    /// <summary>
    /// Dernière news
    /// </summary>
    public string LatestNews
    {
        get => _latestNews;
        set => this.RaiseAndSetIfChanged(ref _latestNews, value);
    }

    /// <summary>
    /// Activité récente
    /// </summary>
    public ObservableCollection<string> RecentActivity { get; }

    /// <summary>
    /// Charge les données du dashboard depuis le repository
    /// </summary>
    public void LoadDashboardData()
    {
        if (_repository == null)
        {
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();

            // Charger le nombre de workers
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Workers";
                TotalWorkers = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Charger le nombre de storylines actives (si la table existe)
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Storylines WHERE IsActive = 1";
                ActiveStorylines = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch
            {
                ActiveStorylines = 0;
            }

            // Charger le nombre de shows à venir
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Shows";
                UpcomingShows = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch
            {
                UpcomingShows = 0;
            }

            // Charger le budget de la compagnie principale
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT Name, Treasury, CurrentWeek FROM Companies WHERE IsPlayerControlled = 1 LIMIT 1";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    CompanyName = reader.GetString(0);
                    CurrentBudget = (decimal)reader.GetDouble(1);
                    CurrentWeek = reader.GetInt32(2);
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine($"[DashboardViewModel] Erreur chargement compagnie: {ex.Message}");
            }

            // Mettre à jour l'activité récente
            RecentActivity.Clear();
            RecentActivity.Add($"✅ Données chargées avec succès");
            RecentActivity.Add($"🤼 {TotalWorkers} workers dans le roster");
            RecentActivity.Add($"🏆 Titres et storylines actives");

            System.Console.WriteLine($"[DashboardViewModel] Dashboard chargé: {TotalWorkers} workers, Budget: ${CurrentBudget:N0}");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[DashboardViewModel] Erreur lors du chargement: {ex.Message}");
            LatestNews = $"⚠️ Erreur de chargement: {ex.Message}";
        }
    }
}
