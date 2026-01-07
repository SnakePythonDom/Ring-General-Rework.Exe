using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Youth;

/// <summary>
/// ViewModel pour le système youth development
/// </summary>
public sealed class YouthViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private decimal _budget = 100_000m;
    private int _totalTrainees;
    private string? _coachName;

    public YouthViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        Trainees = new ObservableCollection<TraineeItemViewModel>();

        LoadYouthData();
    }

    public ObservableCollection<TraineeItemViewModel> Trainees { get; }

    public decimal Budget
    {
        get => _budget;
        set => this.RaiseAndSetIfChanged(ref _budget, value);
    }

    public string BudgetFormatted => $"${Budget:N0}";

    public int TotalTrainees
    {
        get => _totalTrainees;
        set => this.RaiseAndSetIfChanged(ref _totalTrainees, value);
    }

    public string? CoachName
    {
        get => _coachName;
        set => this.RaiseAndSetIfChanged(ref _coachName, value);
    }

    private void LoadYouthData()
    {
        if (_repository == null)
        {
            LoadPlaceholderData();
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();

            // Charger le budget youth
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT YouthBudget FROM Companies WHERE IsPlayerControlled = 1 LIMIT 1";
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    Budget = Convert.ToDecimal(result);
                }
            }

            // Charger les trainees
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Youth";
                TotalTrainees = Convert.ToInt32(cmd.ExecuteScalar());
            }

            System.Console.WriteLine($"[YouthViewModel] {TotalTrainees} trainees, Budget: ${Budget:N0}");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[YouthViewModel] Erreur: {ex.Message}");
            LoadPlaceholderData();
        }
    }

    private void LoadPlaceholderData()
    {
        Trainees.Add(new TraineeItemViewModel
        {
            Name = "John Morrison Jr.",
            Age = 19,
            Potential = 75,
            Progress = 45
        });
        Trainees.Add(new TraineeItemViewModel
        {
            Name = "Sarah Phoenix",
            Age = 21,
            Potential = 82,
            Progress = 68
        });
        Trainees.Add(new TraineeItemViewModel
        {
            Name = "Mike Thunder",
            Age = 20,
            Potential = 70,
            Progress = 52
        });
    }
}

public sealed class TraineeItemViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private int _age;
    private int _potential;
    private int _progress;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int Age
    {
        get => _age;
        set => this.RaiseAndSetIfChanged(ref _age, value);
    }

    public int Potential
    {
        get => _potential;
        set => this.RaiseAndSetIfChanged(ref _potential, value);
    }

    public int Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public string AgeDisplay => $"{Age} ans";
    public string ProgressDisplay => $"{Progress}%";
}
