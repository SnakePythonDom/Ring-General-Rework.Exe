using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Services;

namespace RingGeneral.UI.ViewModels.Roster;

/// <summary>
/// ViewModel pour la gestion des titres (championships).
/// Enrichi dans Phase 6.3 avec intégration booking.
/// </summary>
public sealed class TitlesViewModel : ViewModelBase
{
    private readonly ITitleRepository? _titleRepository;
    private readonly ContenderService? _contenderService;
    private readonly GameRepository? _gameRepository;
    private TitleListItemViewModel? _selectedTitle;
    private string _searchText = string.Empty;
    private readonly List<TitleListItemViewModel> _allTitles = new List<TitleListItemViewModel>();

    // Temporaire: ID Compagnie par défaut (sera injecté via context plus tard)
    private const string CurrentCompanyId = "C001";

    public TitlesViewModel(
        ITitleRepository? titleRepository = null,
        ContenderService? contenderService = null,
        GameRepository? gameRepository = null)
    {
        _titleRepository = titleRepository;
        _contenderService = contenderService;
        _gameRepository = gameRepository;

        Titles = new ObservableCollection<TitleListItemViewModel>();
        TitleHistory = new ObservableCollection<TitleReignHistoryItem>();

        // Phase 6.3 - Collection pour booking
        AvailableForBooking = new ObservableCollection<TitleOptionViewModel>();

        // Phase 6.3 - Commandes
        LoadAvailableTitlesCommand = ReactiveCommand.Create(LoadAvailableTitles);
        AssignToSegmentCommand = ReactiveCommand.Create<string>(AssignToSegment);
        GetVacantTitlesCommand = ReactiveCommand.Create(GetVacantTitles);
        GetDefendedTitlesCommand = ReactiveCommand.Create(GetDefendedTitles);
        VacateTitleCommand = ReactiveCommand.Create(() => Logger.Info("Action: Rendre Vacant"));
        NewChampionCommand = ReactiveCommand.Create(() => Logger.Info("Action: Nouveau Champion"));
        DesignBeltCommand = ReactiveCommand.Create(() => Logger.Info("Action: Design Ceinture"));

        LoadTitles();
    }

    #region Collections

    /// <summary>
    /// Liste des titres
    /// </summary>
    public ObservableCollection<TitleListItemViewModel> Titles { get; }

    /// <summary>
    /// Historique du titre sélectionné
    /// </summary>
    public ObservableCollection<TitleReignHistoryItem> TitleHistory { get; }

    /// <summary>
    /// Phase 6.3 - Titres disponibles pour assignment booking
    /// </summary>
    public ObservableCollection<TitleOptionViewModel> AvailableForBooking { get; }

    #endregion

    #region Properties

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
            if (value != null && _contenderService != null)
            {
                // Exemple: Charger les contenders
                // _contenderService.MettreAJourClassement(value.TitleId);
            }
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

    #endregion

    #region Commands

    /// <summary>
    /// Phase 6.3 - Commande pour charger les titres disponibles pour booking
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadAvailableTitlesCommand { get; }

    /// <summary>
    /// Phase 6.3 - Commande pour assigner à un segment
    /// </summary>
    public ReactiveCommand<string, Unit> AssignToSegmentCommand { get; }

    /// <summary>
    /// Phase 6.3 - Commande pour obtenir les titres vacants
    /// </summary>
    public ReactiveCommand<Unit, Unit> GetVacantTitlesCommand { get; }

    /// <summary>
    /// Phase 6.3 - Commande pour obtenir les titres défendus
    /// </summary>
    public ReactiveCommand<Unit, Unit> GetDefendedTitlesCommand { get; }

    /// <summary>
    /// Rendre le titre vacant
    /// </summary>
    public ReactiveCommand<Unit, Unit> VacateTitleCommand { get; }

    /// <summary>
    /// Assigner un nouveau champion
    /// </summary>
    public ReactiveCommand<Unit, Unit> NewChampionCommand { get; }

    /// <summary>
    /// Changer le design de la ceinture
    /// </summary>
    public ReactiveCommand<Unit, Unit> DesignBeltCommand { get; }

    #endregion

    #region Public Methods - Phase 6.3

    /// <summary>
    /// Phase 6.3 - Charge les titres disponibles pour assignment booking
    /// </summary>
    public void LoadAvailableTitles()
    {
        AvailableForBooking.Clear();

        if (_titleRepository == null)
        {
            LoadPlaceholderBookingTitles();
            return;
        }

        try
        {
            var overviewList = _titleRepository.ChargerTousLesTitres(CurrentCompanyId);
            foreach (var title in overviewList)
            {
                AvailableForBooking.Add(new TitleOptionViewModel
                {
                    TitleId = title.TitleId,
                    Name = title.Name,
                    Prestige = title.Prestige,
                    CurrentChampion = title.ChampionName,
                    IsVacant = title.IsVacant
                });
            }

            Logger.Info($"{AvailableForBooking.Count} titres disponibles pour booking");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur chargement titres booking : {ex.Message}");
            LoadPlaceholderBookingTitles();
        }
    }

    /// <summary>
    /// Phase 6.3 - Assigne un titre à un segment (défense titre)
    /// </summary>
    public void AssignToSegment(string segmentId)
    {
        if (_gameRepository == null || string.IsNullOrWhiteSpace(segmentId) || SelectedTitle == null)
        {
            Logger.Warning("Impossible d'assigner titre : paramètres invalides");
            return;
        }

        try
        {
            using var connection = (SqliteConnection)_gameRepository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Segments
                SET TitleId = @titleId
                WHERE SegmentId = @segmentId";

            cmd.Parameters.AddWithValue("@titleId", SelectedTitle.TitleId);
            cmd.Parameters.AddWithValue("@segmentId", segmentId);

            cmd.ExecuteNonQuery();

            Logger.Info($"Titre '{SelectedTitle.Name}' assigné au segment {segmentId}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur assignment titre : {ex.Message}");
        }
    }

    /// <summary>
    /// Phase 6.3 - Retourne les titres vacants uniquement
    /// </summary>
    public void GetVacantTitles()
    {
        var vacantTitles = AvailableForBooking
            .Where(t => t.IsVacant)
            .ToList();

        Logger.Info($"{vacantTitles.Count} titres vacants");
    }

    /// <summary>
    /// Phase 6.3 - Retourne les titres avec détenteur
    /// </summary>
    public void GetDefendedTitles()
    {
        var defendedTitles = AvailableForBooking
            .Where(t => !t.IsVacant)
            .ToList();

        Logger.Info($"{defendedTitles.Count} titres défendus");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Charge la liste des titres
    /// </summary>
    private void LoadTitles()
    {
        Titles.Clear();
        _allTitles.Clear();

        if (_titleRepository == null)
        {
            LoadPlaceholderTitles();
            return;
        }

        try
        {
            var overviewList = _titleRepository.ChargerTousLesTitres(CurrentCompanyId);
            foreach (var title in overviewList)
            {
                var vm = new TitleListItemViewModel
                {
                    TitleId = title.TitleId,
                    Name = title.Name,
                    Prestige = title.Prestige,
                    CurrentChampion = title.ChampionName,
                    ReignDays = 0, // TODO: Calculer depuis StartWeek via TitleRepository si nécessaire
                    ReignCount = title.DefenseCount,
                    IsVacant = title.IsVacant,
                    Gender = title.Gender ?? "M",
                    Type = title.Type ?? "S",
                    Trend = title.Trend ?? "S",
                };

                if (!string.IsNullOrEmpty(title.BookerIdeas))
                {
                    foreach (var idea in title.BookerIdeas.Split('|', StringSplitOptions.RemoveEmptyEntries))
                    {
                        vm.BookerIdeas.Add(idea.Trim());
                    }
                }

                _allTitles.Add(vm);
                Titles.Add(vm);
            }

            Logger.Info($"{Titles.Count} titres chargés depuis la DB");
        }
        catch (Exception ex)
        {
            Logger.Error($"[TitlesViewModel] Erreur lors du chargement: {ex.Message}");
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

        // TODO: Charger l'historique réel depuis la DB via ITitleRepository ou TitleService
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

    /// <summary>
    /// Phase 6.3 - Charge placeholder titres pour booking
    /// </summary>
    private void LoadPlaceholderBookingTitles()
    {
        AvailableForBooking.Add(new TitleOptionViewModel
        {
            TitleId = "T001",
            Name = "WWE Championship",
            Prestige = 95,
            CurrentChampion = "John Cena",
            IsVacant = false
        });
        AvailableForBooking.Add(new TitleOptionViewModel
        {
            TitleId = "T003",
            Name = "Intercontinental Championship",
            Prestige = 78,
            CurrentChampion = "VACANT",
            IsVacant = true
        });
    }

    #endregion
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

    private string _gender = "M";
    public string Gender
    {
        get => _gender;
        set
        {
            this.RaiseAndSetIfChanged(ref _gender, value);
            this.RaisePropertyChanged(nameof(DisplayGender));
        }
    }

    private string _type = "S";
    public string Type
    {
        get => _type;
        set
        {
            this.RaiseAndSetIfChanged(ref _type, value);
            this.RaisePropertyChanged(nameof(DisplayType));
        }
    }

    private string _trend = "S";
    public string Trend
    {
        get => _trend;
        set
        {
            this.RaiseAndSetIfChanged(ref _trend, value);
            this.RaisePropertyChanged(nameof(IsTrendUp));
            this.RaisePropertyChanged(nameof(IsTrendDown));
            this.RaisePropertyChanged(nameof(IsTrendStable));
        }
    }

    private ObservableCollection<string> _bookerIdeas = new();
    public ObservableCollection<string> BookerIdeas
    {
        get => _bookerIdeas;
        set => this.RaiseAndSetIfChanged(ref _bookerIdeas, value);
    }

    public bool IsTrendUp => Trend == "U";
    public bool IsTrendDown => Trend == "D";
    public bool IsTrendStable => Trend == "S" || string.IsNullOrEmpty(Trend);

    public string DisplayGender => Gender switch
    {
        "M" => "Male",
        "F" => "Female",
        "I" => "Inter",
        _ => "Male"
    };

    public string DisplayType => Type switch
    {
        "S" => "Single",
        "T" => "Tag",
        "Tr" => "Trio",
        _ => "Single"
    };

    public string ReignDaysDisplay => $"{ReignDays}j";
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

/// <summary>
/// Phase 6.3 - ViewModel pour un titre disponible pour booking
/// </summary>
public sealed class TitleOptionViewModel : ViewModelBase
{
    private string _titleId = string.Empty;
    private string _name = string.Empty;
    private int _prestige;
    private string _currentChampion = string.Empty;
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

    public bool IsVacant
    {
        get => _isVacant;
        set => this.RaiseAndSetIfChanged(ref _isVacant, value);
    }

    public string Display => IsVacant
        ? $"{Name} (VACANT - Prestige: {Prestige})"
        : $"{Name} ({CurrentChampion} - Prestige: {Prestige})";

    public string StatusDisplay => IsVacant ? "VACANT" : $"Détenteur: {CurrentChampion}";
}
