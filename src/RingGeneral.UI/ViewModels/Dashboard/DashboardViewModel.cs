using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;

namespace RingGeneral.UI.ViewModels.Dashboard;

/// <summary>
/// ViewModel pour la page d'accueil (Dashboard)
/// Affiche un récapitulatif de l'état actuel de la compagnie
/// </summary>
public sealed class DashboardViewModel : ViewModelBase
{
    private string _companyName = "Ma Compagnie";
    private int _currentWeek = 1;
    private int _totalWorkers;
    private int _activeStorylines;
    private int _upcomingShows;
    private decimal _currentBudget;
    private string _latestNews = "Bienvenue dans Ring General !";

    public DashboardViewModel()
    {
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
        // TODO: Implémenter le chargement depuis GameRepository
        // Pour l'instant, affiche des données de placeholder
    }
}
