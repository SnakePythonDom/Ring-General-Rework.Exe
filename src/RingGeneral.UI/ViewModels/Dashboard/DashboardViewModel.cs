using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Services;
using RingGeneral.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace RingGeneral.UI.ViewModels.Dashboard;

/// <summary>
/// ViewModel pour la page d'accueil (Dashboard)
/// Affiche un récapitulatif de l'état actuel de la compagnie
/// </summary>
public sealed class DashboardViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private readonly IShowDayOrchestrator? _showDayOrchestrator;
    private readonly ITimeOrchestratorService? _timeOrchestrator;
    private readonly IShowSchedulerStore? _showSchedulerStore;
    private readonly IMoraleEngine? _moraleEngine;
    private readonly ICrisisEngine? _crisisEngine;
    private string _companyName = "Ma Compagnie";
    private string _companyId = string.Empty;
    private int _currentDay = 1;
    private DateTime _currentDate = new DateTime(2024, 1, 1);
    private int _totalWorkers;
    private int _activeStorylines;
    private int _upcomingShows;
    private decimal _currentBudget;
    private string _latestNews = "Bienvenue dans Ring General !";
    private bool _hasUpcomingShow;
    private string _upcomingShowName = string.Empty;
    private int _moraleScore = 70;
    private string _moraleTrend = "Stable";
    private string _moraleLabel = "Good";
    private string _trendIcon = "➡️";
    private int _activeCrisesCount = 0;
    private int _criticalCrisesCount = 0;
    private bool _hasCriticalCrises = false;

    public DashboardViewModel(
        GameRepository? repository = null,
        IShowSchedulerStore? showSchedulerStore = null,
        IShowDayOrchestrator? showDayOrchestrator = null,
        ITimeOrchestratorService? timeOrchestrator = null,
        IMoraleEngine? moraleEngine = null,
        ICrisisEngine? crisisEngine = null)
    {
        _repository = repository;
        _showSchedulerStore = showSchedulerStore;
        _showDayOrchestrator = showDayOrchestrator;
        _timeOrchestrator = timeOrchestrator;
        _moraleEngine = moraleEngine;
        _crisisEngine = crisisEngine;

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

        // Commandes
        ContinueCommand = ReactiveCommand.Create(OnContinue);
        PrepareShowCommand = ReactiveCommand.Create(OnPrepareShow);

        // Charger les données au démarrage
        LoadDashboardData();
    }

    /// <summary>
    /// Commande pour continuer (avancer d'un jour)
    /// </summary>
    public ReactiveCommand<Unit, Unit> ContinueCommand { get; }

    /// <summary>
    /// Commande pour préparer le show
    /// </summary>
    public ReactiveCommand<Unit, Unit> PrepareShowCommand { get; }

    /// <summary>
    /// Nom de la compagnie
    /// </summary>
    public string CompanyName
    {
        get => _companyName;
        set => this.RaiseAndSetIfChanged(ref _companyName, value);
    }

    /// <summary>
    /// Jour actuel
    /// </summary>
    public int CurrentDay
    {
        get => _currentDay;
        set => this.RaiseAndSetIfChanged(ref _currentDay, value);
    }

    /// <summary>
    /// Date actuelle du jeu
    /// </summary>
    public DateTime CurrentDate
    {
        get => _currentDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentDate, value);
            this.RaisePropertyChanged(nameof(CurrentDateFormatted));
        }
    }

    /// <summary>
    /// Date formatée pour l'affichage (ex: "Lundi 12 Janvier 2026")
    /// </summary>
    public string CurrentDateFormatted => CurrentDate.ToString("dddd d MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));

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
    /// Indique si un show est prévu cette semaine
    /// </summary>
    public bool HasUpcomingShow
    {
        get => _hasUpcomingShow;
        set => this.RaiseAndSetIfChanged(ref _hasUpcomingShow, value);
    }

    /// <summary>
    /// Nom du show à venir (si existant)
    /// </summary>
    public string UpcomingShowName
    {
        get => _upcomingShowName;
        set => this.RaiseAndSetIfChanged(ref _upcomingShowName, value);
    }

    /// <summary>
    /// Score de moral de la compagnie (0-100)
    /// </summary>
    public int MoraleScore
    {
        get => _moraleScore;
        set => this.RaiseAndSetIfChanged(ref _moraleScore, value);
    }

    /// <summary>
    /// Tendance du moral (Improving, Stable, Declining)
    /// </summary>
    public string MoraleTrend
    {
        get => _moraleTrend;
        set => this.RaiseAndSetIfChanged(ref _moraleTrend, value);
    }

    /// <summary>
    /// Label descriptif du moral (Excellent, Good, Average, Low, Critical)
    /// </summary>
    public string MoraleLabel
    {
        get => _moraleLabel;
        set => this.RaiseAndSetIfChanged(ref _moraleLabel, value);
    }

    /// <summary>
    /// Icône de tendance (📈, ➡️, 📉)
    /// </summary>
    public string TrendIcon
    {
        get => _trendIcon;
        set => this.RaiseAndSetIfChanged(ref _trendIcon, value);
    }

    /// <summary>
    /// Nombre de crises actives
    /// </summary>
    public int ActiveCrisesCount
    {
        get => _activeCrisesCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _activeCrisesCount, value);
            this.RaisePropertyChanged(nameof(HasCrises));
        }
    }

    /// <summary>
    /// Nombre de crises critiques
    /// </summary>
    public int CriticalCrisesCount
    {
        get => _criticalCrisesCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _criticalCrisesCount, value);
            this.RaisePropertyChanged(nameof(HasCriticalCrises));
        }
    }

    /// <summary>
    /// Indique s'il y a des crises actives
    /// </summary>
    public bool HasCrises => ActiveCrisesCount > 0;

    /// <summary>
    /// Indique s'il y a des crises critiques
    /// </summary>
    public bool HasCriticalCrises
    {
        get => _hasCriticalCrises;
        set => this.RaiseAndSetIfChanged(ref _hasCriticalCrises, value);
    }

    /// <summary>
    /// Message d'alerte pour les crises critiques
    /// </summary>
    public string CrisisAlertMessage => CriticalCrisesCount > 1
        ? $"{CriticalCrisesCount} crises critiques nécessitent votre attention immédiate !"
        : "Une crise critique nécessite votre attention immédiate !";

    /// <summary>
    /// Label du bouton principal (dynamique)
    /// </summary>
    public string MainButtonLabel => HasUpcomingShow ? "📺 Préparer le Show" : "⏭️ Jour Suivant";

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

            // Charger le budget de la compagnie principale et la date actuelle
            try
            {
                // Essayer d'abord de charger depuis SaveGames (source de vérité)
                string? companyIdFromSave = null;
                try
                {
                    using var saveCmd = connection.CreateCommand();
                    saveCmd.CommandText = """
                        SELECT PlayerCompanyId, CurrentWeek, CurrentDate 
                        FROM SaveGames 
                        WHERE IsActive = 1 
                        LIMIT 1;
                        """;
                    using var saveReader = saveCmd.ExecuteReader();
                    if (saveReader.Read())
                    {
                        companyIdFromSave = saveReader.GetString(0);
                        CurrentDay = saveReader.GetInt32(1);
                        var dateStr = saveReader.GetString(2);
                        if (DateTime.TryParse(dateStr, out var date))
                        {
                            CurrentDate = date;
                        }
                    }
                }
                catch
                {
                    // SaveGames peut ne pas exister ou ne pas avoir de sauvegarde active
                }

                // Charger les informations de la compagnie
                using var cmd = connection.CreateCommand();
                
                // Vérifier si la colonne IsPlayerControlled existe
                bool hasIsPlayerControlled = ColumnExists(connection, "Companies", "IsPlayerControlled");
                
                if (hasIsPlayerControlled)
                {
                    // Utiliser IsPlayerControlled si disponible
                    cmd.CommandText = "SELECT CompanyId, Name, Treasury FROM Companies WHERE IsPlayerControlled = 1 LIMIT 1";
                }
                else if (!string.IsNullOrEmpty(companyIdFromSave))
                {
                    // Utiliser la compagnie depuis SaveGames
                    cmd.CommandText = "SELECT CompanyId, Name, Treasury FROM Companies WHERE CompanyId = $companyId LIMIT 1";
                    cmd.Parameters.AddWithValue("$companyId", companyIdFromSave);
                }
                else
                {
                    // Fallback : prendre la première compagnie
                    cmd.CommandText = "SELECT CompanyId, Name, Treasury FROM Companies LIMIT 1";
                }
                
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    _companyId = reader.GetString(0);
                    CompanyName = reader.GetString(1);
                    CurrentBudget = (decimal)reader.GetDouble(2);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[DashboardViewModel] Erreur chargement compagnie: {ex.Message}");
            }

            // Détecter si un show est prévu cette semaine
            DetectUpcomingShow();

            // Charger les données de moral
            LoadMoraleData();

            // Charger les données de crises
            LoadCrisisData();

            // Mettre à jour l'activité récente
            UpdateRecentActivity();
        }
        catch (Exception ex)
        {
            Logger.Error($"[DashboardViewModel] Erreur lors du chargement: {ex.Message}");
            LatestNews = $"⚠️ Erreur de chargement: {ex.Message}";
        }
    }

    /// <summary>
    /// Vérifie si une colonne existe dans une table
    /// </summary>
    private static bool ColumnExists(SqliteConnection connection, string tableName, string columnName)
    {
        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({tableName});";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private void UpdateRecentActivity()
    {
        var activityUpdates = new List<string>
        {
            "✅ Données chargées avec succès",
            $"🤼 {TotalWorkers} workers dans le roster",
            "🏆 Titres et storylines actives"
        };

        if (HasCriticalCrises)
        {
            activityUpdates.Add($"⚠️ {CriticalCrisesCount} crise(s) critique(s) !");
        }

        if (HasUpcomingShow)
        {
            activityUpdates.Add($"📺 Show à préparer: {UpcomingShowName}");
        }

        RecentActivity.Clear();
        foreach (var activity in activityUpdates)
        {
            RecentActivity.Add(activity);
        }

        Logger.Info($"Dashboard chargé: {TotalWorkers} workers, Budget: ${CurrentBudget:N0}");
    }

    /// <summary>
    /// Détecte si un show est prévu aujourd'hui
    /// </summary>
    private void DetectUpcomingShow()
    {
        if (_showDayOrchestrator is null || string.IsNullOrEmpty(_companyId))
        {
            HasUpcomingShow = false;
            return;
        }

        // Utiliser CurrentDay pour la détection (ShowDayOrchestrator doit être adapté si nécessaire)
        // Pour l'instant, on utilise CurrentDay directement
        var detection = _showDayOrchestrator.DetecterShowAVenir(_companyId, CurrentDay);
        HasUpcomingShow = detection.ShowDetecte;
        UpcomingShowName = detection.Show?.Nom ?? string.Empty;

        // Notifier le changement du label du bouton
        this.RaisePropertyChanged(nameof(MainButtonLabel));
    }

    /// <summary>
    /// Charge les données de moral de la compagnie
    /// </summary>
    private void LoadMoraleData()
    {
        if (_moraleEngine is null || string.IsNullOrEmpty(_companyId))
        {
            // Valeurs par défaut
            MoraleScore = 70;
            MoraleTrend = "Stable";
            MoraleLabel = "Good";
            TrendIcon = "➡️";
            return;
        }

        try
        {
            var companyMorale = _moraleEngine.CalculateCompanyMorale(_companyId);

            if (companyMorale != null)
            {
                MoraleScore = companyMorale.GlobalMoraleScore;
                MoraleTrend = companyMorale.Trend;

                // Déterminer le label basé sur le score
                MoraleLabel = MoraleScore switch
                {
                    >= 80 => "Excellent",
                    >= 60 => "Good",
                    >= 40 => "Average",
                    >= 20 => "Low",
                    _ => "Critical"
                };

                // Déterminer l'icône de tendance
                TrendIcon = companyMorale.Trend switch
                {
                    "Improving" => "📈",
                    "Declining" => "📉",
                    _ => "➡️"
                };

                System.Console.WriteLine($"[DashboardViewModel] Moral chargé: {MoraleScore} ({MoraleLabel}), Tendance: {MoraleTrend}");
            }
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[DashboardViewModel] Erreur chargement moral: {ex.Message}");
            // Garder les valeurs par défaut en cas d'erreur
        }
    }

    /// <summary>
    /// Charge les données de crises de la compagnie
    /// </summary>
    private void LoadCrisisData()
    {
        if (_crisisEngine is null || string.IsNullOrEmpty(_companyId))
        {
            // Valeurs par défaut
            ActiveCrisesCount = 0;
            CriticalCrisesCount = 0;
            HasCriticalCrises = false;
            return;
        }

        try
        {
            var activeCrises = _crisisEngine.GetActiveCrises(_companyId);
            var criticalCrises = _crisisEngine.GetCriticalCrises(_companyId);

            ActiveCrisesCount = activeCrises.Count;
            CriticalCrisesCount = criticalCrises.Count;
            HasCriticalCrises = CriticalCrisesCount > 0;

            System.Console.WriteLine($"[DashboardViewModel] Crises chargées: {ActiveCrisesCount} actives, {CriticalCrisesCount} critiques");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[DashboardViewModel] Erreur chargement crises: {ex.Message}");
            // Garder les valeurs par défaut en cas d'erreur
        }
    }

    /// <summary>
    /// Action du bouton "Jour Suivant" (avancer d'un jour OU simuler le show)
    /// </summary>
    private void OnContinue()
    {
        if (_repository is null || _timeOrchestrator is null || string.IsNullOrEmpty(_companyId))
        {
            Logger.Warning("TimeOrchestratorService non disponible ou compagnie non chargée");
            return;
        }

        try
        {
            // Détecter si un show est prévu aujourd'hui
            if (HasUpcomingShow && _showDayOrchestrator is not null)
            {
                // Simuler le show
                OnPrepareShow();
            }
            else
            {
                // Avancer d'un jour avec TimeOrchestratorService
                var result = _timeOrchestrator.PasserJourSuivant(_companyId);

                // Mettre à jour les propriétés
                CurrentDay = result.Day;
                CurrentDate = result.CurrentDate;

                // Afficher les événements dans le Daily Log
                var activityUpdates = new List<string> { $"⏭️ Jour {CurrentDay} terminé - {CurrentDateFormatted}" };
                foreach (var evt in result.Events)
                {
                    activityUpdates.Add($"📅 {evt}");
                }

                foreach (var update in activityUpdates)
                {
                    RecentActivity.Insert(0, update);
                }

                Logger.Info($"Jour {CurrentDay} terminé ({CurrentDateFormatted})");
            }

            // Recharger les données
            LoadDashboardData();
        }
        catch (Exception ex)
        {
            Logger.Error($"[DashboardViewModel] Erreur lors du passage au jour suivant: {ex.Message}", ex);
            RecentActivity.Insert(0, $"⚠️ Erreur: {ex.Message}");
        }
    }

    /// <summary>
    /// Action du bouton "Préparer le Show" - Exécute le flux Show Day complet
    /// </summary>
    private void OnPrepareShow()
    {
        if (!HasUpcomingShow || _showDayOrchestrator is null || _repository is null)
        {
            return;
        }

        try
        {
            // Récupérer le show à simuler (utiliser CurrentDay au lieu de CurrentWeek)
            var detection = _showDayOrchestrator.DetecterShowAVenir(_companyId, CurrentDay);
            if (!detection.ShowDetecte || detection.Show is null)
            {
                RecentActivity.Insert(0, "⚠️ Aucun show détecté à simuler");
                return;
            }

            var showId = detection.Show.ShowId;
            var activityUpdates = new List<string> { $"🎬 Simulation du show: {detection.Show.Nom}" };
            Logger.Info($"Début simulation show: {detection.Show.Nom} ({showId})");

            // Exécuter le flux complet Show Day
            var resultat = _showDayOrchestrator.ExecuterFluxComplet(showId, _companyId);

            if (resultat.Succes)
            {
                activityUpdates.Add("✅ Show simulé avec succès !");
                if (resultat.Rapport is not null)
                {
                    activityUpdates.Add($"📊 Note: {resultat.Rapport.NoteGlobale}/100");
                    activityUpdates.Add($"👥 Audience: {resultat.Rapport.Audience}");
                    activityUpdates.Add($"💰 Revenus: ${resultat.Rapport.Billetterie + resultat.Rapport.Merch + resultat.Rapport.Tv:N2}");
                }

                foreach (var changement in resultat.Changements.Take(5))
                {
                    activityUpdates.Add(changement);
                }

                Logger.Info($"Show simulé avec succès: {detection.Show.Nom}");
            }
            else
            {
                activityUpdates.Add("⚠️ Erreurs lors de la simulation:");
                foreach (var erreur in resultat.Erreurs)
                {
                    activityUpdates.Add($"  - {erreur}");
                    Logger.Error($"[DashboardViewModel] Erreur simulation: {erreur}");
                }
            }

            // Mettre à jour l'activité récente
            foreach (var update in activityUpdates)
            {
                RecentActivity.Insert(0, update);
            }

            // Recharger les données
            LoadDashboardData();
        }
        catch (Exception ex)
        {
            Logger.Error($"[DashboardViewModel] Erreur lors de la simulation du show: {ex.Message}");
            RecentActivity.Insert(0, $"⚠️ Erreur critique: {ex.Message}");
        }
    }
}
