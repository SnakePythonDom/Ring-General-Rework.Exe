using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Services;
using RingGeneral.Core.Interfaces;

namespace RingGeneral.UI.ViewModels.Dashboard;

/// <summary>
/// ViewModel pour la page d'accueil (Dashboard)
/// Affiche un récapitulatif de l'état actuel de la compagnie
/// </summary>
public sealed class DashboardViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private readonly ShowDayOrchestrator? _showDayOrchestrator;
    private readonly IShowSchedulerStore? _showSchedulerStore;
    private readonly IMoraleEngine? _moraleEngine;
    private readonly ICrisisEngine? _crisisEngine;
    private string _companyName = "Ma Compagnie";
    private string _companyId = string.Empty;
    private int _currentWeek = 1;
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
        ShowDayOrchestrator? showDayOrchestrator = null,
        IMoraleEngine? moraleEngine = null,
        ICrisisEngine? crisisEngine = null)
    {
        _repository = repository;
        _showSchedulerStore = showSchedulerStore;
        _showDayOrchestrator = showDayOrchestrator;
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
    /// Commande pour continuer (avancer d'une semaine)
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
    public string MainButtonLabel => HasUpcomingShow ? "📺 Préparer le Show" : "▶️ Continuer";

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
                cmd.CommandText = "SELECT CompanyId, Name, Treasury, CurrentWeek FROM Companies WHERE IsPlayerControlled = 1 LIMIT 1";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    _companyId = reader.GetString(0);
                    CompanyName = reader.GetString(1);
                    CurrentBudget = (decimal)reader.GetDouble(2);
                    CurrentWeek = reader.GetInt32(3);
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
            RecentActivity.Clear();
            RecentActivity.Add($"✅ Données chargées avec succès");
            RecentActivity.Add($"🤼 {TotalWorkers} workers dans le roster");
            RecentActivity.Add($"🏆 Titres et storylines actives");

            if (HasCriticalCrises)
            {
                RecentActivity.Add($"⚠️ {CriticalCrisesCount} crise(s) critique(s) !");
            }

            if (HasUpcomingShow)
            {
                RecentActivity.Add($"📺 Show à préparer: {UpcomingShowName}");
            }

            Logger.Info($"Dashboard chargé: {TotalWorkers} workers, Budget: ${CurrentBudget:N0}");
        }
        catch (Exception ex)
        {
            Logger.Error($"[DashboardViewModel] Erreur lors du chargement: {ex.Message}");
            LatestNews = $"⚠️ Erreur de chargement: {ex.Message}";
        }
    }

    /// <summary>
    /// Détecte si un show est prévu à la semaine actuelle
    /// </summary>
    private void DetectUpcomingShow()
    {
        if (_showDayOrchestrator is null || string.IsNullOrEmpty(_companyId))
        {
            HasUpcomingShow = false;
            return;
        }

        var detection = _showDayOrchestrator.DetecterShowAVenir(_companyId, CurrentWeek);
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
    /// Action du bouton "Continuer" (avancer d'une semaine OU simuler le show)
    /// </summary>
    private void OnContinue()
    {
        if (_repository is null)
        {
            return;
        }

        try
        {
            // Détecter si un show est prévu cette semaine
            if (HasUpcomingShow && _showDayOrchestrator is not null)
            {
                // Simuler le show
                OnPrepareShow();
            }
            else
            {
                // Avancer d'une semaine normale
                // TODO: Intégrer WeeklyLoopService
                CurrentWeek++;
                RecentActivity.Insert(0, $"⏭️ Passage à la semaine {CurrentWeek}");
                Logger.Info($"Avancé à la semaine {CurrentWeek}");
            }

            // Recharger les données
            LoadDashboardData();
        }
        catch (Exception ex)
        {
            Logger.Error($"[DashboardViewModel] Erreur lors de OnContinue: {ex.Message}");
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
            // Récupérer le show à simuler
            var detection = _showDayOrchestrator.DetecterShowAVenir(_companyId, CurrentWeek);
            if (!detection.ShowDetecte || detection.Show is null)
            {
                RecentActivity.Insert(0, "⚠️ Aucun show détecté à simuler");
                return;
            }

            var showId = detection.Show.ShowId;
            RecentActivity.Insert(0, $"🎬 Simulation du show: {detection.Show.Nom}");
            Logger.Info($"Début simulation show: {detection.Show.Nom} ({showId})");

            // Exécuter le flux complet Show Day
            var resultat = _showDayOrchestrator.ExecuterFluxComplet(showId, _companyId);

            if (resultat.Succes)
            {
                RecentActivity.Insert(0, $"✅ Show simulé avec succès !");
                if (resultat.Rapport is not null)
                {
                    RecentActivity.Insert(0, $"📊 Note: {resultat.Rapport.NoteGlobale}/100");
                    RecentActivity.Insert(0, $"👥 Audience: {resultat.Rapport.Audience}");
                    RecentActivity.Insert(0, $"💰 Revenus: ${resultat.Rapport.Billetterie + resultat.Rapport.Merch + resultat.Rapport.Tv:N2}");
                }

                foreach (var changement in resultat.Changements.Take(5))
                {
                    RecentActivity.Insert(0, changement);
                }

                Logger.Info($"Show simulé avec succès: {detection.Show.Nom}");
            }
            else
            {
                RecentActivity.Insert(0, $"⚠️ Erreurs lors de la simulation:");
                foreach (var erreur in resultat.Erreurs)
                {
                    RecentActivity.Insert(0, $"  - {erreur}");
                    Logger.Error($"[DashboardViewModel] Erreur simulation: {erreur}");
                }
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
