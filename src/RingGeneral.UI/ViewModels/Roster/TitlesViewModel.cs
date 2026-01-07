using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;
using System.Linq;

namespace RingGeneral.UI.ViewModels.Roster;

/// <summary>
/// ViewModel pour la gestion des titres (championships)
/// </summary>
public sealed class TitlesViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private TitleListItemViewModel? _selectedTitle;
    private string _searchText = string.Empty;
    private readonly List<TitleListItemViewModel> _allTitles = new List<TitleListItemViewModel>();

    public TitlesViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        Titles = new ObservableCollection<TitleListItemViewModel>();
        TitleHistory = new ObservableCollection<TitleReignHistoryItem>();

        LoadTitles();
    }

    /// <summary>
    /// Liste des titres
    /// </summary>
    public ObservableCollection<TitleListItemViewModel> Titles { get; }

    /// <summary>
    /// Historique du titre sélectionné
    /// </summary>
    public ObservableCollection<TitleReignHistoryItem> TitleHistory { get; }

    /// <summary>
    /// Titre sélectionné
    /// </summary>
    public TitleListItemViewModel? SelectedTitle
    {
        get => _selectedTitle;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTitle, value);
            LoadTitleHistory(value?.TitleId);
        }
    }

    /// <summary>
    /// Texte de recherche
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterTitles();
        }
    }

    /// <summary>
    /// Nombre total de titres
    /// </summary>
    public int TotalTitles => Titles.Count;

    /// <summary>
    /// Nombre de titres vacants
    /// </summary>
    public int VacantTitles => Titles.Count(t => t.IsVacant);

    /// <summary>
    /// Charge la liste des titres
    /// </summary>
    private void LoadTitles()
    {
        Titles.Clear();
        _allTitles.Clear();

        if (_repository == null)
        {
            LoadPlaceholderTitles();
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    t.TitleId,
                    t.Name,
                    t.Prestige,
                    t.CurrentChampionId,
                    w.FullName as ChampionName,
                    COALESCE(tr.DefenseCount, 0) as DefenseCount
                FROM Titles t
                LEFT JOIN Workers w ON t.CurrentChampionId = w.WorkerId
                LEFT JOIN TitleReigns tr ON t.TitleId = tr.TitleId AND tr.IsActive = 1
                ORDER BY t.Prestige DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var championId = reader.IsDBNull(3) ? null : reader.GetString(3);
                var championName = reader.IsDBNull(4) ? "VACANT" : reader.GetString(4);
                var isVacant = string.IsNullOrEmpty(championId);

                var title = new TitleListItemViewModel
                {
                    TitleId = reader.GetString(0),
                    Name = reader.GetString(1),
                    Prestige = reader.GetInt32(2),
                    CurrentChampion = championName,
                    ReignDays = 0, // TODO: Calculer depuis StartWeek
                    ReignCount = reader.GetInt32(5),
                    IsVacant = isVacant
                };

                _allTitles.Add(title);
                Titles.Add(title);
            }

            System.Console.WriteLine($"[TitlesViewModel] {Titles.Count} titres chargés depuis la DB");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[TitlesViewModel] Erreur lors du chargement: {ex.Message}");
            LoadPlaceholderTitles();
        }
    }

    /// <summary>
    /// Charge des données placeholder
    /// </summary>
    private void LoadPlaceholderTitles()
    {
        var title1 = new TitleListItemViewModel
        {
            TitleId = "T001",
            Name = "WWE Championship",
            Prestige = 95,
            CurrentChampion = "John Cena",
            ReignDays = 278,
            ReignCount = 1,
            IsVacant = false
        };
        var title2 = new TitleListItemViewModel
        {
            TitleId = "T002",
            Name = "World Heavyweight Championship",
            Prestige = 92,
            CurrentChampion = "Randy Orton",
            ReignDays = 145,
            ReignCount = 3,
            IsVacant = false
        };
        var title3 = new TitleListItemViewModel
        {
            TitleId = "T003",
            Name = "Intercontinental Championship",
            Prestige = 78,
            CurrentChampion = "VACANT",
            ReignDays = 0,
            ReignCount = 0,
            IsVacant = true
        };

        _allTitles.Add(title1);
        _allTitles.Add(title2);
        _allTitles.Add(title3);

        Titles.Add(title1);
        Titles.Add(title2);
        Titles.Add(title3);
    }

    /// <summary>
    /// Charge l'historique d'un titre
    /// </summary>
    private void LoadTitleHistory(string? titleId)
    {
        TitleHistory.Clear();

        if (string.IsNullOrEmpty(titleId))
            return;

        // Données placeholder
        if (titleId == "T001")
        {
            TitleHistory.Add(new TitleReignHistoryItem
            {
                Champion = "John Cena",
                StartWeek = 1,
                EndWeek = null,
                ReignDays = 278,
                DefenseCount = 12,
                Status = "CURRENT"
            });
            TitleHistory.Add(new TitleReignHistoryItem
            {
                Champion = "The Rock",
                StartWeek = -15,
                EndWeek = 1,
                ReignDays = 112,
                DefenseCount = 5,
                Status = "ENDED"
            });
        }

        // TODO: Charger l'historique réel depuis la DB
        // var history = _repository.ChargerHistoriqueTitre(titleId);
    }

    /// <summary>
    /// Filtre les titres selon le texte de recherche
    /// </summary>
    private void FilterTitles()
    {
        Titles.Clear();

        if (string.IsNullOrWhiteSpace(_searchText))
        {
            // Pas de recherche, afficher tous les titres
            foreach (var title in _allTitles)
            {
                Titles.Add(title);
            }
            return;
        }

        // Filtrer par nom de titre ou nom du champion (insensible à la casse)
        var searchLower = _searchText.ToLower();
        var filtered = _allTitles.Where(t =>
            t.Name.ToLower().Contains(searchLower) ||
            t.CurrentChampion.ToLower().Contains(searchLower)
        );

        foreach (var title in filtered)
        {
            Titles.Add(title);
        }

        this.RaisePropertyChanged(nameof(TotalTitles));
        this.RaisePropertyChanged(nameof(VacantTitles));
    }
}

/// <summary>
/// ViewModel pour un titre dans la liste
/// </summary>
public sealed class TitleListItemViewModel : ViewModelBase
{
    private string _titleId = string.Empty;
    private string _name = string.Empty;
    private int _prestige;
    private string _currentChampion = string.Empty;
    private int _reignDays;
    private int _reignCount;
    private bool _isVacant;

    public string TitleId
    {
        get => _titleId;
        set => this.RaiseAndSetIfChanged(ref _titleId, value);
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int Prestige
    {
        get => _prestige;
        set => this.RaiseAndSetIfChanged(ref _prestige, value);
    }

    public string CurrentChampion
    {
        get => _currentChampion;
        set => this.RaiseAndSetIfChanged(ref _currentChampion, value);
    }

    public int ReignDays
    {
        get => _reignDays;
        set
        {
            this.RaiseAndSetIfChanged(ref _reignDays, value);
            this.RaisePropertyChanged(nameof(ReignDaysDisplay));
        }
    }

    public int ReignCount
    {
        get => _reignCount;
        set => this.RaiseAndSetIfChanged(ref _reignCount, value);
    }

    public bool IsVacant
    {
        get => _isVacant;
        set => this.RaiseAndSetIfChanged(ref _isVacant, value);
    }

    public string ReignDaysDisplay => $"{ReignDays} jours";
}

/// <summary>
/// Item d'historique de règne
/// </summary>
public sealed class TitleReignHistoryItem : ViewModelBase
{
    private string _champion = string.Empty;
    private int? _startWeek;
    private int? _endWeek;
    private int _reignDays;
    private int _defenseCount;
    private string _status = string.Empty;

    public string Champion
    {
        get => _champion;
        set => this.RaiseAndSetIfChanged(ref _champion, value);
    }

    public int? StartWeek
    {
        get => _startWeek;
        set => this.RaiseAndSetIfChanged(ref _startWeek, value);
    }

    public int? EndWeek
    {
        get => _endWeek;
        set => this.RaiseAndSetIfChanged(ref _endWeek, value);
    }

    public int ReignDays
    {
        get => _reignDays;
        set => this.RaiseAndSetIfChanged(ref _reignDays, value);
    }

    public int DefenseCount
    {
        get => _defenseCount;
        set => this.RaiseAndSetIfChanged(ref _defenseCount, value);
    }

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string PeriodDisplay => EndWeek.HasValue
        ? $"S{StartWeek} → S{EndWeek}"
        : $"S{StartWeek} → Actuel";
}
