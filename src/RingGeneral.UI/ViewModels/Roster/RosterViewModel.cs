using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Data.Repositories;
using System.Linq;

namespace RingGeneral.UI.ViewModels.Roster;

/// <summary>
/// ViewModel pour la liste des workers (roster)
/// </summary>
public sealed class RosterViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private string _searchText = string.Empty;
    private WorkerListItemViewModel? _selectedWorker;

    public RosterViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        Workers = new ObservableCollection<WorkerListItemViewModel>();

        // Charger les workers
        LoadWorkers();
    }

    /// <summary>
    /// Liste des workers
    /// </summary>
    public ObservableCollection<WorkerListItemViewModel> Workers { get; }

    /// <summary>
    /// Texte de recherche
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterWorkers();
        }
    }

    /// <summary>
    /// Worker sélectionné
    /// </summary>
    public WorkerListItemViewModel? SelectedWorker
    {
        get => _selectedWorker;
        set => this.RaiseAndSetIfChanged(ref _selectedWorker, value);
    }

    /// <summary>
    /// Nombre total de workers
    /// </summary>
    public int TotalWorkers => Workers.Count;

    /// <summary>
    /// Charge les workers depuis le repository
    /// </summary>
    public void LoadWorkers()
    {
        Workers.Clear();

        if (_repository == null)
        {
            LoadPlaceholderData();
            return;
        }

        try
        {
            // Charger tous les workers depuis la DB
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT w.WorkerId, w.FullName, w.TvRole, w.Popularity, w.CompanyId, c.Name as CompanyName
                FROM Workers w
                LEFT JOIN Companies c ON w.CompanyId = c.CompanyId
                ORDER BY w.Popularity DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Workers.Add(new WorkerListItemViewModel
                {
                    WorkerId = reader.GetString(0),
                    Name = reader.GetString(1),
                    Role = reader.IsDBNull(2) ? "N/A" : reader.GetString(2),
                    Popularity = reader.GetInt32(3),
                    Status = "Actif", // TODO: Calculer depuis blessures/fatigue
                    Company = reader.IsDBNull(5) ? "Free Agent" : reader.GetString(5)
                });
            }

            System.Console.WriteLine($"[RosterViewModel] {Workers.Count} workers chargés depuis la DB");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[RosterViewModel] Erreur lors du chargement: {ex.Message}");
            LoadPlaceholderData();
        }
    }

    /// <summary>
    /// Charge des données placeholder en cas d'erreur
    /// </summary>
    private void LoadPlaceholderData()
    {
        Workers.Add(new WorkerListItemViewModel
        {
            WorkerId = "W001",
            Name = "John Cena",
            Role = "Main Eventer",
            Popularity = 95,
            Status = "Actif",
            Company = "WWE"
        });
        Workers.Add(new WorkerListItemViewModel
        {
            WorkerId = "W002",
            Name = "Randy Orton",
            Role = "Main Eventer",
            Popularity = 92,
            Status = "Actif",
            Company = "WWE"
        });
        Workers.Add(new WorkerListItemViewModel
        {
            WorkerId = "W003",
            Name = "CM Punk",
            Role = "Upper Midcard",
            Popularity = 88,
            Status = "Blessé",
            Company = "WWE"
        });
    }

    /// <summary>
    /// Filtre les workers selon le texte de recherche
    /// </summary>
    private void FilterWorkers()
    {
        // TODO: Implémenter le filtrage
    }
}

/// <summary>
/// ViewModel pour un item de la liste de workers
/// </summary>
public sealed class WorkerListItemViewModel : ViewModelBase
{
    private string _workerId = string.Empty;
    private string _name = string.Empty;
    private string _role = string.Empty;
    private int _popularity;
    private string _status = string.Empty;
    private string _company = string.Empty;

    public string WorkerId
    {
        get => _workerId;
        set => this.RaiseAndSetIfChanged(ref _workerId, value);
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string Role
    {
        get => _role;
        set => this.RaiseAndSetIfChanged(ref _role, value);
    }

    public int Popularity
    {
        get => _popularity;
        set => this.RaiseAndSetIfChanged(ref _popularity, value);
    }

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string Company
    {
        get => _company;
        set => this.RaiseAndSetIfChanged(ref _company, value);
    }
}
