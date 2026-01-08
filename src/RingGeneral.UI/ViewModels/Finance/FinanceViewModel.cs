using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Finance;

/// <summary>
/// ViewModel pour le système financier
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

        LoadFinanceData();
    }

    public ObservableCollection<TransactionItemViewModel> Transactions { get; }

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
