using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Finance;

/// <summary>
/// ViewModel pour le système financier.
/// Enrichi dans Phase 6.3 avec TV deals, audience et reach.
/// </summary>
public sealed class FinanceViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private decimal _currentBalance = 10_000_000m;
    private decimal _weeklyRevenue;
    private decimal _weeklyExpenses;
    private int _currentWeek = 1;

    public FinanceViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        Transactions = new ObservableCollection<TransactionItemViewModel>();

        // Phase 6.3 - Nouvelles collections
        TvDeals = new ObservableCollection<TvDealViewModel>();
        ReachMap = new ObservableCollection<ReachMapItemViewModel>();
        BroadcastConstraints = new ObservableCollection<string>();
        AudienceHistory = new ObservableCollection<AudienceHistoryItemViewModel>();

        // Phase 6.3 - Commandes
        LoadTvDealsCommand = ReactiveCommand.Create(LoadTvDeals);
        LoadAudienceHistoryCommand = ReactiveCommand.Create<string>(LoadAudienceHistory);
        CalculateReachCommand = ReactiveCommand.Create(CalculateReach);

        LoadFinanceData();
    }

    #region Collections

    public ObservableCollection<TransactionItemViewModel> Transactions { get; }

    /// <summary>
    /// Phase 6.3 - Deals TV actifs
    /// </summary>
    public ObservableCollection<TvDealViewModel> TvDeals { get; }

    /// <summary>
    /// Phase 6.3 - Carte de reach (régions/markets)
    /// </summary>
    public ObservableCollection<ReachMapItemViewModel> ReachMap { get; }

    /// <summary>
    /// Phase 6.3 - Contraintes de diffusion
    /// </summary>
    public ObservableCollection<string> BroadcastConstraints { get; }

    /// <summary>
    /// Phase 6.3 - Historique d'audience
    /// </summary>
    public ObservableCollection<AudienceHistoryItemViewModel> AudienceHistory { get; }

    #endregion

    #region Properties

    public decimal CurrentBalance
    {
        get => _currentBalance;
        set => this.RaiseAndSetIfChanged(ref _currentBalance, value);
    }

    public string CurrentBalanceFormatted => $"${CurrentBalance:N0}";

    public decimal WeeklyRevenue
    {
        get => _weeklyRevenue;
        set => this.RaiseAndSetIfChanged(ref _weeklyRevenue, value);
    }

    public string WeeklyRevenueFormatted => $"+${WeeklyRevenue:N0}";

    public decimal WeeklyExpenses
    {
        get => _weeklyExpenses;
        set => this.RaiseAndSetIfChanged(ref _weeklyExpenses, value);
    }

    public string WeeklyExpensesFormatted => $"-${WeeklyExpenses:N0}";

    public decimal NetIncome => WeeklyRevenue - WeeklyExpenses;

    public string NetIncomeFormatted => $"{(NetIncome >= 0 ? "+" : "")}{NetIncome:N0}";

    public int CurrentWeek
    {
        get => _currentWeek;
        set => this.RaiseAndSetIfChanged(ref _currentWeek, value);
    }

    public int TotalTvDeals => TvDeals.Count;
    public decimal TotalReach => ReachMap.Sum(r => r.Population);

    #endregion

    #region Commands

    /// <summary>
    /// Phase 6.3 - Commande pour charger les deals TV
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadTvDealsCommand { get; }

    /// <summary>
    /// Phase 6.3 - Commande pour charger l'historique d'audience
    /// </summary>
    public ReactiveCommand<string, Unit> LoadAudienceHistoryCommand { get; }

    /// <summary>
    /// Phase 6.3 - Commande pour calculer le reach
    /// </summary>
    public ReactiveCommand<Unit, Unit> CalculateReachCommand { get; }

    #endregion

    #region Public Methods - Phase 6.3

    /// <summary>
    /// Phase 6.3 - Charge les deals TV actifs
    /// </summary>
    public void LoadTvDeals()
    {
        TvDeals.Clear();
        BroadcastConstraints.Clear();

        if (_repository == null)
        {
            LoadPlaceholderTvDeals();
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT DealId, Network, WeeklyPayment, MinShows, MaxShows, StartWeek, EndWeek
                FROM TvDeals
                WHERE IsActive = 1
                ORDER BY WeeklyPayment DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TvDeals.Add(new TvDealViewModel
                {
                    DealId = reader.GetString(0),
                    Network = reader.GetString(1),
                    WeeklyPayment = reader.GetDecimal(2),
                    MinShows = reader.GetInt32(3),
                    MaxShows = reader.GetInt32(4),
                    StartWeek = reader.GetInt32(5),
                    EndWeek = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6)
                });
            }

            this.RaisePropertyChanged(nameof(TotalTvDeals));

            Logger.Info($"{TvDeals.Count} TV deals actifs chargés");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur chargement TV deals : {ex.Message}");
            LoadPlaceholderTvDeals();
        }
    }

    /// <summary>
    /// Phase 6.3 - Charge l'historique d'audience pour un show
    /// </summary>
    public void LoadAudienceHistory(string showId)
    {
        AudienceHistory.Clear();

        if (_repository == null || string.IsNullOrWhiteSpace(showId))
        {
            LoadPlaceholderAudienceHistory();
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Week, Rating, Viewers, SharePercent
                FROM ShowAudienceHistory
                WHERE ShowId = @showId
                ORDER BY Week DESC
                LIMIT 12";

            cmd.Parameters.AddWithValue("@showId", showId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                AudienceHistory.Add(new AudienceHistoryItemViewModel
                {
                    Week = reader.GetInt32(0),
                    Rating = reader.GetDouble(1),
                    Viewers = reader.GetInt32(2),
                    SharePercent = reader.GetDouble(3)
                });
            }

            Logger.Info($"{AudienceHistory.Count} entrées d'audience chargées pour {showId}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur chargement audience history : {ex.Message}");
            LoadPlaceholderAudienceHistory();
        }
    }

    /// <summary>
    /// Phase 6.3 - Calcule le reach potentiel
    /// </summary>
    public void CalculateReach()
    {
        ReachMap.Clear();

        if (_repository == null)
        {
            LoadPlaceholderReach();
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Region, Population, Penetration
                FROM MarketReach
                ORDER BY Population DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ReachMap.Add(new ReachMapItemViewModel
                {
                    Region = reader.GetString(0),
                    Population = reader.GetDecimal(1),
                    Penetration = reader.GetDouble(2)
                });
            }

            this.RaisePropertyChanged(nameof(TotalReach));

            Logger.Info($"Reach calculé : {ReachMap.Count} régions, {TotalReach:N0} pop totale");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur calcul reach : {ex.Message}");
            LoadPlaceholderReach();
        }
    }

    #endregion

    #region Private Methods

    private void LoadFinanceData()
    {
        if (_repository == null)
        {
            LoadPlaceholderData();
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();

            // Charger le trésor actuel et la semaine courante
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT Treasury, CurrentWeek
                    FROM Companies
                    WHERE IsPlayerControlled = 1
                    LIMIT 1";

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    CurrentBalance = reader.GetDecimal(0);
                    CurrentWeek = reader.GetInt32(1);
                }
            }

            // Charger les revenus et dépenses de la semaine courante
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT Revenues, Expenses
                    FROM CompanyBalanceSnapshots
                    WHERE Week = @week
                    ORDER BY CreatedAt DESC
                    LIMIT 1";

                cmd.Parameters.AddWithValue("@week", CurrentWeek);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    WeeklyRevenue = reader.GetDecimal(0);
                    WeeklyExpenses = reader.GetDecimal(1);
                }
            }

            // Charger l'historique des transactions (20 dernières)
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT Category, Amount, Description, Week
                    FROM FinanceTransactions
                    ORDER BY Week DESC, FinanceTransactionId DESC
                    LIMIT 20";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Transactions.Add(new TransactionItemViewModel
                    {
                        Category = reader.GetString(0),
                        Amount = reader.GetDecimal(1),
                        Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Week = reader.GetInt32(3)
                    });
                }
            }

            Logger.Info($"Balance: ${CurrentBalance:N0}, Revenus: ${WeeklyRevenue:N0}, Dépenses: ${WeeklyExpenses:N0}");
        }
        catch (Exception ex)
        {
            Logger.Error($"[FinanceViewModel] Erreur: {ex.Message}");
            LoadPlaceholderData();
        }
    }

    private void LoadPlaceholderData()
    {
        CurrentBalance = 10_000_000m;
        WeeklyRevenue = 500_000m;
        WeeklyExpenses = 350_000m;

        Transactions.Add(new TransactionItemViewModel
        {
            Category = "Ticket Sales",
            Amount = 250_000m,
            Description = "Monday Night Raw - Week 1",
            Week = 1
        });
        Transactions.Add(new TransactionItemViewModel
        {
            Category = "TV Deal",
            Amount = 150_000m,
            Description = "USA Network Payment",
            Week = 1
        });
        Transactions.Add(new TransactionItemViewModel
        {
            Category = "Merchandise",
            Amount = 100_000m,
            Description = "Arena Sales",
            Week = 1
        });
        Transactions.Add(new TransactionItemViewModel
        {
            Category = "Salaries",
            Amount = -200_000m,
            Description = "Weekly Payroll",
            Week = 1
        });
        Transactions.Add(new TransactionItemViewModel
        {
            Category = "Production",
            Amount = -80_000m,
            Description = "Show Production Costs",
            Week = 1
        });
        Transactions.Add(new TransactionItemViewModel
        {
            Category = "Arena Rental",
            Amount = -70_000m,
            Description = "Madison Square Garden",
            Week = 1
        });
    }

    /// <summary>
    /// Phase 6.3 - Charge placeholder TV deals
    /// </summary>
    private void LoadPlaceholderTvDeals()
    {
        TvDeals.Add(new TvDealViewModel
        {
            DealId = "TV001",
            Network = "USA Network",
            WeeklyPayment = 150_000m,
            MinShows = 1,
            MaxShows = 2,
            StartWeek = 1,
            EndWeek = null
        });

        BroadcastConstraints.Add("Minimum 1 show par semaine");
        BroadcastConstraints.Add("Primetime slot requis (19h-22h)");
        BroadcastConstraints.Add("Rating minimum: PG-13");
    }

    /// <summary>
    /// Phase 6.3 - Charge placeholder audience history
    /// </summary>
    private void LoadPlaceholderAudienceHistory()
    {
        AudienceHistory.Add(new AudienceHistoryItemViewModel
        {
            Week = 4,
            Rating = 2.5,
            Viewers = 3_500_000,
            SharePercent = 18.5
        });
        AudienceHistory.Add(new AudienceHistoryItemViewModel
        {
            Week = 3,
            Rating = 2.3,
            Viewers = 3_200_000,
            SharePercent = 17.8
        });
    }

    /// <summary>
    /// Phase 6.3 - Charge placeholder reach
    /// </summary>
    private void LoadPlaceholderReach()
    {
        ReachMap.Add(new ReachMapItemViewModel
        {
            Region = "Northeast",
            Population = 55_000_000m,
            Penetration = 0.65
        });
        ReachMap.Add(new ReachMapItemViewModel
        {
            Region = "South",
            Population = 125_000_000m,
            Penetration = 0.45
        });
    }

    #endregion
}

public sealed class TransactionItemViewModel : ViewModelBase
{
    private string _category = string.Empty;
    private decimal _amount;
    private string _description = string.Empty;
    private int _week;

    public string Category
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    public decimal Amount
    {
        get => _amount;
        set => this.RaiseAndSetIfChanged(ref _amount, value);
    }

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public int Week
    {
        get => _week;
        set => this.RaiseAndSetIfChanged(ref _week, value);
    }

    public string AmountFormatted => $"{(Amount >= 0 ? "+" : "")}{Amount:N0}";
    public string AmountColor => Amount >= 0 ? "#10b981" : "#ef4444";
    public string WeekDisplay => $"Week {Week}";
}

/// <summary>
/// Phase 6.3 - ViewModel pour un TV deal
/// </summary>
public sealed class TvDealViewModel : ViewModelBase
{
    private string _dealId = string.Empty;
    private string _network = string.Empty;
    private decimal _weeklyPayment;
    private int _minShows;
    private int _maxShows;
    private int _startWeek;
    private int? _endWeek;

    public string DealId
    {
        get => _dealId;
        set => this.RaiseAndSetIfChanged(ref _dealId, value);
    }

    public string Network
    {
        get => _network;
        set => this.RaiseAndSetIfChanged(ref _network, value);
    }

    public decimal WeeklyPayment
    {
        get => _weeklyPayment;
        set => this.RaiseAndSetIfChanged(ref _weeklyPayment, value);
    }

    public int MinShows
    {
        get => _minShows;
        set => this.RaiseAndSetIfChanged(ref _minShows, value);
    }

    public int MaxShows
    {
        get => _maxShows;
        set => this.RaiseAndSetIfChanged(ref _maxShows, value);
    }

    public int StartWeek
    {
        get => _startWeek;
        set => this.RaiseAndSetIfChanged(ref _startWeek, value);
    }

    public int? EndWeek
    {
        get => _endWeek;
        set => this.RaiseAndSetIfChanged(ref _endWeek, value);
    }

    public string PaymentDisplay => $"${WeeklyPayment:N0}/semaine";
    public string DurationDisplay => EndWeek.HasValue ? $"S{StartWeek}-S{EndWeek}" : $"S{StartWeek}+";
    public string ShowsDisplay => $"{MinShows}-{MaxShows} shows/semaine";
}

/// <summary>
/// Phase 6.3 - ViewModel pour un item de reach map
/// </summary>
public sealed class ReachMapItemViewModel : ViewModelBase
{
    private string _region = string.Empty;
    private decimal _population;
    private double _penetration;

    public string Region
    {
        get => _region;
        set => this.RaiseAndSetIfChanged(ref _region, value);
    }

    public decimal Population
    {
        get => _population;
        set => this.RaiseAndSetIfChanged(ref _population, value);
    }

    public double Penetration
    {
        get => _penetration;
        set => this.RaiseAndSetIfChanged(ref _penetration, value);
    }

    public string PopulationDisplay => $"{Population:N0}";
    public string PenetrationDisplay => $"{Penetration:P0}";
    public decimal EffectiveReach => Population * (decimal)Penetration;
}

/// <summary>
/// Phase 6.3 - ViewModel pour un item d'historique d'audience
/// </summary>
public sealed class AudienceHistoryItemViewModel : ViewModelBase
{
    private int _week;
    private double _rating;
    private int _viewers;
    private double _sharePercent;

    public int Week
    {
        get => _week;
        set => this.RaiseAndSetIfChanged(ref _week, value);
    }

    public double Rating
    {
        get => _rating;
        set => this.RaiseAndSetIfChanged(ref _rating, value);
    }

    public int Viewers
    {
        get => _viewers;
        set => this.RaiseAndSetIfChanged(ref _viewers, value);
    }

    public double SharePercent
    {
        get => _sharePercent;
        set => this.RaiseAndSetIfChanged(ref _sharePercent, value);
    }

    public string WeekDisplay => $"Week {Week}";
    public string RatingDisplay => $"{Rating:F1}";
    public string ViewersDisplay => $"{Viewers:N0}";
    public string ShareDisplay => $"{SharePercent:F1}%";
}
