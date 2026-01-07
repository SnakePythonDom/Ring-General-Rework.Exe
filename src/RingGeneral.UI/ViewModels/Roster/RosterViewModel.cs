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
            // Données de placeholder si pas de repository
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
            return;
        }

        // TODO: Charger depuis le repository
        // var workers = _repository.ChargerTousLesWorkers();
        // foreach (var w in workers)
        // {
        //     Workers.Add(new WorkerListItemViewModel
        //     {
        //         WorkerId = w.WorkerId,
        //         Name = w.FullName,
        //         Role = w.Role,
        //         Popularity = w.Popularity,
        //         Status = w.Status,
        //         Company = w.CompanyName
        //     });
        // }
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
