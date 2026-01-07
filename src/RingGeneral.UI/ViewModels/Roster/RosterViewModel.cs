using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.Services.Navigation;
using System.Linq;

namespace RingGeneral.UI.ViewModels.Roster;

/// <summary>
/// ViewModel pour la liste des workers (roster)
/// </summary>
public sealed class RosterViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private readonly INavigationService? _navigationService;
    private string _searchText = string.Empty;
    private WorkerListItemViewModel? _selectedWorker;
    private readonly List<WorkerListItemViewModel> _allWorkers = new List<WorkerListItemViewModel>();

    public RosterViewModel(GameRepository? repository = null, INavigationService? navigationService = null)
    {
        _repository = repository;
        _navigationService = navigationService;

        Workers = new ObservableCollection<WorkerListItemViewModel>();

        // Commande pour naviguer vers les détails d'un worker
        ViewWorkerDetailsCommand = ReactiveCommand.Create<string>(ViewWorkerDetails);

        // Charger les workers
        LoadWorkers();
    }

    /// <summary>
    /// Commande pour afficher les détails d'un worker
    /// </summary>
    public ReactiveCommand<string, Unit> ViewWorkerDetailsCommand { get; }

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
    /// Nombre total de workers affichés
    /// </summary>
    public int TotalWorkers => Workers.Count;

    /// <summary>
    /// Nombre total de workers dans la base
    /// </summary>
    private int _totalWorkersInDatabase;
    public int TotalWorkersInDatabase
    {
        get => _totalWorkersInDatabase;
        private set => this.RaiseAndSetIfChanged(ref _totalWorkersInDatabase, value);
    }

    /// <summary>
    /// Taille de page pour la pagination
    /// </summary>
    private const int PAGE_SIZE = 100;

    /// <summary>
    /// Indique s'il y a plus de workers à charger
    /// </summary>
    public bool HasMoreWorkers => _allWorkers.Count < TotalWorkersInDatabase;

    /// <summary>
    /// Charge les workers depuis le repository (avec pagination)
    /// </summary>
    public void LoadWorkers()
    {
        Workers.Clear();
        _allWorkers.Clear();

        if (_repository == null)
        {
            LoadPlaceholderData();
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();

            // Obtenir le nombre total de workers
            using (var countCmd = connection.CreateCommand())
            {
                countCmd.CommandText = "SELECT COUNT(*) FROM Workers";
                TotalWorkersInDatabase = Convert.ToInt32(countCmd.ExecuteScalar());
            }

            // Charger les premiers PAGE_SIZE workers
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT w.WorkerId, w.FullName, w.TvRole, w.Popularity, w.CompanyId, c.Name as CompanyName
                FROM Workers w
                LEFT JOIN Companies c ON w.CompanyId = c.CompanyId
                ORDER BY w.Popularity DESC
                LIMIT @pageSize";

            cmd.Parameters.AddWithValue("@pageSize", PAGE_SIZE);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var worker = new WorkerListItemViewModel
                {
                    WorkerId = reader.GetString(0),
                    Name = reader.GetString(1),
                    Role = reader.IsDBNull(2) ? "N/A" : reader.GetString(2),
                    Popularity = reader.GetInt32(3),
                    Status = "Actif", // TODO: Calculer depuis blessures/fatigue
                    Company = reader.IsDBNull(5) ? "Free Agent" : reader.GetString(5)
                };

                _allWorkers.Add(worker);
                Workers.Add(worker);
            }

            System.Console.WriteLine($"[RosterViewModel] {Workers.Count}/{TotalWorkersInDatabase} workers chargés (pagination)");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[RosterViewModel] Erreur lors du chargement: {ex.Message}");
            LoadPlaceholderData();
        }
    }

    /// <summary>
    /// Charge plus de workers (pagination)
    /// </summary>
    public void LoadMoreWorkers()
    {
        if (_repository == null || !HasMoreWorkers)
        {
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT w.WorkerId, w.FullName, w.TvRole, w.Popularity, w.CompanyId, c.Name as CompanyName
                FROM Workers w
                LEFT JOIN Companies c ON w.CompanyId = c.CompanyId
                ORDER BY w.Popularity DESC
                LIMIT @pageSize OFFSET @offset";

            cmd.Parameters.AddWithValue("@pageSize", PAGE_SIZE);
            cmd.Parameters.AddWithValue("@offset", _allWorkers.Count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var worker = new WorkerListItemViewModel
                {
                    WorkerId = reader.GetString(0),
                    Name = reader.GetString(1),
                    Role = reader.IsDBNull(2) ? "N/A" : reader.GetString(2),
                    Popularity = reader.GetInt32(3),
                    Status = "Actif",
                    Company = reader.IsDBNull(5) ? "Free Agent" : reader.GetString(5)
                };

                _allWorkers.Add(worker);
                Workers.Add(worker);
            }

            System.Console.WriteLine($"[RosterViewModel] {Workers.Count}/{TotalWorkersInDatabase} workers chargés");
            this.RaisePropertyChanged(nameof(HasMoreWorkers));
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[RosterViewModel] Erreur lors du chargement: {ex.Message}");
        }
    }

    /// <summary>
    /// Charge des données placeholder en cas d'erreur
    /// </summary>
    private void LoadPlaceholderData()
    {
        var worker1 = new WorkerListItemViewModel
        {
            WorkerId = "W001",
            Name = "John Cena",
            Role = "Main Eventer",
            Popularity = 95,
            Status = "Actif",
            Company = "WWE"
        };
        var worker2 = new WorkerListItemViewModel
        {
            WorkerId = "W002",
            Name = "Randy Orton",
            Role = "Main Eventer",
            Popularity = 92,
            Status = "Actif",
            Company = "WWE"
        };
        var worker3 = new WorkerListItemViewModel
        {
            WorkerId = "W003",
            Name = "CM Punk",
            Role = "Upper Midcard",
            Popularity = 88,
            Status = "Blessé",
            Company = "WWE"
        };

        _allWorkers.Add(worker1);
        _allWorkers.Add(worker2);
        _allWorkers.Add(worker3);

        Workers.Add(worker1);
        Workers.Add(worker2);
        Workers.Add(worker3);
    }

    /// <summary>
    /// Navigue vers les détails d'un worker
    /// </summary>
    private void ViewWorkerDetails(string workerId)
    {
        if (_navigationService == null)
        {
            System.Console.WriteLine("[RosterViewModel] NavigationService non disponible");
            return;
        }

        if (string.IsNullOrEmpty(workerId))
        {
            System.Console.WriteLine("[RosterViewModel] WorkerId invalide");
            return;
        }

        System.Console.WriteLine($"[RosterViewModel] Navigation vers WorkerDetail: {workerId}");
        _navigationService.NavigateTo<WorkerDetailViewModel>(workerId);
    }

    /// <summary>
    /// Filtre les workers selon le texte de recherche
    /// </summary>
    private void FilterWorkers()
    {
        Workers.Clear();

        if (string.IsNullOrWhiteSpace(_searchText))
        {
            // Pas de recherche, afficher tous les workers
            foreach (var worker in _allWorkers)
            {
                Workers.Add(worker);
            }
            return;
        }

        // Filtrer par nom, rôle ou compagnie (insensible à la casse)
        var searchLower = _searchText.ToLower();
        var filtered = _allWorkers.Where(w =>
            w.Name.ToLower().Contains(searchLower) ||
            w.Role.ToLower().Contains(searchLower) ||
            w.Company.ToLower().Contains(searchLower)
        );

        foreach (var worker in filtered)
        {
            Workers.Add(worker);
        }

        this.RaisePropertyChanged(nameof(TotalWorkers));
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
