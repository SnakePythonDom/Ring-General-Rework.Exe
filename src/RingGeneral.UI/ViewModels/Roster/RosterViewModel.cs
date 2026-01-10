using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.Services.Navigation;
using Avalonia.Threading;

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
        LoadWorkersCommand = ReactiveCommand.Create(LoadWorkers);
        LoadMoreWorkersCommand = ReactiveCommand.Create(LoadMoreWorkers);

        // Ne pas charger les données dans le constructeur
        // Le chargement sera déclenché par la vue quand elle sera prête (événement Loaded)
    }

    /// <summary>
    /// Commande pour afficher les détails d'un worker
    /// </summary>
    public ReactiveCommand<string, Unit> ViewWorkerDetailsCommand { get; }

    /// <summary>
    /// Commande pour charger les workers
    /// </summary>
    public ReactiveCommand<Unit, Task> LoadWorkersCommand { get; }

    /// <summary>
    /// Commande pour charger plus de workers
    /// </summary>
    public ReactiveCommand<Unit, Task> LoadMoreWorkersCommand { get; }

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
    public async Task LoadWorkers()
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
            // Effectuer le travail de base de données sur un thread background
            var result = await Task.Run(() =>
            {
                using var connection = _repository.CreateConnection();

                // Vérifier si la table existe
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Workers'";
                    var tableExists = checkCmd.ExecuteScalar() != null;
                    Logger.Info($"Table Workers existe: {tableExists}");
                }

                // Obtenir le nombre total de workers
                int totalCount = 0;
                try
                {
                    using (var countCmd = connection.CreateCommand())
                    {
                        countCmd.CommandText = "SELECT COUNT(*) FROM Workers";
                        var countResult = countCmd.ExecuteScalar();
                        totalCount = countResult != null ? Convert.ToInt32(countResult) : 0;
                    }
                    Logger.Info($"Nombre total de workers dans la base: {totalCount}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Erreur lors du comptage des workers: {ex.Message}", ex);
                }

                var workers = new List<WorkerListItemViewModel>();

                // Charger les premiers PAGE_SIZE workers
                using var cmd = connection.CreateCommand();
                
                // #region agent log
                var logPath = Path.Combine(AppContext.BaseDirectory, ".cursor", "debug.log");
                File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"C\",\"location\":\"RosterViewModel.cs:137\",\"message\":\"Before query execution\",\"data\":{{\"queryPreview\":\"SELECT w.WorkerId, w.FullName...\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
                // #endregion
                
                cmd.CommandText = @"
                    SELECT w.WorkerId, 
                           COALESCE(w.Name, w.FirstName || ' ' || w.LastName, w.RingName, 'Unknown') as FullName,
                           w.RoleTv, w.Popularity, w.InRing, w.Entertainment, w.Story,
                           w.Momentum, w.Fatigue, 
                           COALESCE((SELECT Morale FROM WorkerAttributes WHERE WorkerId = w.WorkerId), 50) as Morale,
                           w.CompanyId, c.Name as CompanyName
                    FROM Workers w
                    LEFT JOIN Companies c ON w.CompanyId = c.CompanyId
                    ORDER BY w.Popularity DESC
                    LIMIT @pageSize";

                cmd.Parameters.AddWithValue("@pageSize", PAGE_SIZE);

                using var reader = cmd.ExecuteReader();
                
                // #region agent log
                File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"C\",\"location\":\"RosterViewModel.cs:154\",\"message\":\"Reader created\",\"data\":{{\"fieldCount\":{reader.FieldCount}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
                // #endregion
                int rowCount = 0;
                while (reader.Read())
                {
                    try
                    {
                        var worker = new WorkerListItemViewModel
                        {
                            WorkerId = reader.GetString(0),
                            Name = reader.GetString(1),
                            Role = reader.IsDBNull(2) ? "N/A" : (reader.GetString(2) ?? "N/A"),
                            Popularity = reader.GetInt32(3),
                            InRing = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                            Entertainment = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            Story = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                            Momentum = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                            Fatigue = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                            Morale = reader.IsDBNull(9) ? 50 : reader.GetInt32(9),
                            Status = "Actif", // TODO: Calculer depuis blessures/fatigue
                            Company = reader.IsDBNull(11) ? "Free Agent" : (reader.GetString(11) ?? "Free Agent")
                        };

                        workers.Add(worker);
                        rowCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Erreur lors de la lecture d'une ligne de worker: {ex.Message}", ex);
                    }
                }
                Logger.Info($"{rowCount} workers lus depuis la base de données");

                return (totalCount, workers);
            });

            // Mettre à jour les propriétés et collections sur le thread UI
            // S'assurer qu'on est sur le thread UI avant de modifier les collections
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                TotalWorkersInDatabase = result.totalCount;

                // Vider les collections avant d'ajouter les nouveaux workers
                _allWorkers.Clear();
                
                // Ajouter tous les workers à la liste interne d'abord
                foreach (var worker in result.workers)
                {
                    _allWorkers.Add(worker);
                }
                
                // Vider et remplir la collection observable en une seule opération
                // pour éviter les notifications multiples
                Workers.Clear();
                
                // Ajouter tous les workers à la collection observable
                foreach (var worker in _allWorkers)
                {
                    Workers.Add(worker);
                }

                // Forcer la notification de changement pour s'assurer que l'UI se met à jour
                this.RaisePropertyChanged(nameof(HasMoreWorkers));
                this.RaisePropertyChanged(nameof(TotalWorkers));
                this.RaisePropertyChanged(nameof(Workers));

                Logger.Info($"{Workers.Count}/{TotalWorkersInDatabase} workers chargés (pagination)");
            });
        }
        catch (Exception ex)
        {
            // #region agent log
            var logPath = Path.Combine(AppContext.BaseDirectory, ".cursor", "debug.log");
            File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"C\",\"location\":\"RosterViewModel.cs:209\",\"message\":\"Exception in LoadWorkers\",\"data\":{{\"exceptionType\":\"{ex.GetType().Name}\",\"message\":\"{ex.Message.Replace("\"", "\\\"")}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
            // #endregion
            
            Logger.Error($"Erreur lors du chargement des workers: {ex.Message}", ex);
            System.Console.Error.WriteLine($"[RosterViewModel] Erreur lors du chargement: {ex.Message}");
            System.Console.Error.WriteLine($"[RosterViewModel] StackTrace: {ex.StackTrace}");
            LoadPlaceholderData();
        }
    }

    /// <summary>
    /// Charge plus de workers (pagination)
    /// </summary>
    public async Task LoadMoreWorkers()
    {
        if (_repository == null || !HasMoreWorkers)
        {
            return;
        }

        try
        {
            // Charger plus de données depuis un thread background
            var newWorkers = await Task.Run(() =>
            {
                using var connection = _repository.CreateConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT w.WorkerId, 
                           COALESCE(w.Name, w.FirstName || ' ' || w.LastName, w.RingName, 'Unknown') as FullName,
                           w.RoleTv, w.Popularity, w.InRing, w.Entertainment, w.Story,
                           w.Momentum, w.Fatigue,
                           COALESCE((SELECT Morale FROM WorkerAttributes WHERE WorkerId = w.WorkerId), 50) as Morale,
                           w.CompanyId, c.Name as CompanyName
                    FROM Workers w
                    LEFT JOIN Companies c ON w.CompanyId = c.CompanyId
                    ORDER BY w.Popularity DESC
                    LIMIT @pageSize OFFSET @offset";

                cmd.Parameters.AddWithValue("@pageSize", PAGE_SIZE);
                cmd.Parameters.AddWithValue("@offset", _allWorkers.Count);

                var workers = new List<WorkerListItemViewModel>();
                using var reader = cmd.ExecuteReader();
                int rowCount = 0;
                while (reader.Read())
                {
                    try
                    {
                        var worker = new WorkerListItemViewModel
                        {
                            WorkerId = reader.GetString(0),
                            Name = reader.GetString(1),
                            Role = reader.IsDBNull(2) ? "N/A" : (reader.GetString(2) ?? "N/A"),
                            Popularity = reader.GetInt32(3),
                            InRing = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                            Entertainment = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            Story = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                            Momentum = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                            Fatigue = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                            Morale = reader.IsDBNull(9) ? 50 : reader.GetInt32(9),
                            Status = "Actif",
                            Company = reader.IsDBNull(11) ? "Free Agent" : (reader.GetString(11) ?? "Free Agent")
                        };

                        workers.Add(worker);
                        rowCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Erreur lors de la lecture d'une ligne de worker (LoadMore): {ex.Message}", ex);
                    }
                }
                Logger.Info($"{rowCount} workers supplémentaires lus");

                return workers;
            });

            // Ajouter les nouveaux workers sur le thread UI
            foreach (var worker in newWorkers)
            {
                _allWorkers.Add(worker);
                Workers.Add(worker);
            }

            this.RaisePropertyChanged(nameof(HasMoreWorkers));
            this.RaisePropertyChanged(nameof(TotalWorkers));

            Logger.Info($"{Workers.Count}/{TotalWorkersInDatabase} workers chargés");
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
        Logger.Warning("Chargement de données placeholder - la base de données n'est pas disponible ou la table Workers n'existe pas");
        
        Workers.Clear();
        _allWorkers.Clear();
        
        var worker1 = new WorkerListItemViewModel
        {
            WorkerId = "W001",
            Name = "John Cena",
            Role = "Main Eventer",
            Popularity = 95,
            InRing = 85,
            Entertainment = 92,
            Story = 88,
            Momentum = 78,
            Fatigue = 25,
            Morale = 85,
            Status = "Actif",
            Company = "WWE"
        };
        var worker2 = new WorkerListItemViewModel
        {
            WorkerId = "W002",
            Name = "Randy Orton",
            Role = "Main Eventer",
            Popularity = 92,
            InRing = 84,
            Entertainment = 88,
            Story = 86,
            Momentum = 72,
            Fatigue = 30,
            Morale = 80,
            Status = "Actif",
            Company = "WWE"
        };
        var worker3 = new WorkerListItemViewModel
        {
            WorkerId = "W003",
            Name = "CM Punk",
            Role = "Upper Midcard",
            Popularity = 88,
            InRing = 86,
            Entertainment = 90,
            Story = 87,
            Momentum = 65,
            Fatigue = 40,
            Morale = 70,
            Status = "Blessé",
            Company = "WWE"
        };

        _allWorkers.Add(worker1);
        _allWorkers.Add(worker2);
        _allWorkers.Add(worker3);

        Workers.Add(worker1);
        Workers.Add(worker2);
        Workers.Add(worker3);
        
        TotalWorkersInDatabase = 3;
        
        // Forcer les notifications de changement
        this.RaisePropertyChanged(nameof(TotalWorkers));
        this.RaisePropertyChanged(nameof(HasMoreWorkers));
        this.RaisePropertyChanged(nameof(Workers));
        
        Logger.Info($"Données placeholder chargées: {Workers.Count} workers");
    }

    /// <summary>
    /// Navigue vers les détails d'un worker
    /// </summary>
    private void ViewWorkerDetails(string workerId)
    {
        if (_navigationService == null)
        {
            Logger.Info("NavigationService non disponible");
            return;
        }

        if (string.IsNullOrEmpty(workerId))
        {
            Logger.Info("WorkerId invalide");
            return;
        }

        Logger.Info($"Navigation vers WorkerDetail: {workerId}");
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
    private int _inRing;
    private int _entertainment;
    private int _story;
    private int _momentum;
    private int _fatigue;
    private int _morale;
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

    public int InRing
    {
        get => _inRing;
        set
        {
            this.RaiseAndSetIfChanged(ref _inRing, value);
            this.RaisePropertyChanged(nameof(Overall));
        }
    }

    public int Entertainment
    {
        get => _entertainment;
        set
        {
            this.RaiseAndSetIfChanged(ref _entertainment, value);
            this.RaisePropertyChanged(nameof(Overall));
        }
    }

    public int Story
    {
        get => _story;
        set
        {
            this.RaiseAndSetIfChanged(ref _story, value);
            this.RaisePropertyChanged(nameof(Overall));
        }
    }

    public int Momentum
    {
        get => _momentum;
        set => this.RaiseAndSetIfChanged(ref _momentum, value);
    }

    public int Fatigue
    {
        get => _fatigue;
        set => this.RaiseAndSetIfChanged(ref _fatigue, value);
    }

    public int Morale
    {
        get => _morale;
        set => this.RaiseAndSetIfChanged(ref _morale, value);
    }

    public int Overall => (InRing + Entertainment + Story) / 3;

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string StatusBackground => Status switch
    {
        "Blessé" => "#7f1d1d",
        "Suspendu" => "#7c2d12",
        _ => "#065f46"
    };

    public string StatusForeground => Status switch
    {
        "Blessé" => "#fecaca",
        "Suspendu" => "#fde68a",
        _ => "#a7f3d0"
    };

    public string Company
    {
        get => _company;
        set => this.RaiseAndSetIfChanged(ref _company, value);
    }
}
