using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Calendar;

/// <summary>
/// ViewModel pour le calendrier et la planification des shows.
/// Système jour par jour avec shows quotidiens.
/// </summary>
public sealed class CalendarViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private readonly ShowSchedulerService? _showScheduler;
    private readonly ShowRepository? _showRepository;
    private DateOnly _currentDate = DateOnly.FromDateTime(DateTime.Now);
    private CalendarEntryItemViewModel? _selectedEntry;
    private ShowScheduleItemViewModel? _selectedShow;
    private string? _companyId;

    // Collections pour shows par jour
    public Dictionary<DateOnly, List<ShowScheduleItemViewModel>> ShowsParJour { get; } = new();

    // Phase 6.3 - Propriétés pour nouveau show
    private string _newShowName = string.Empty;
    private DateOnly _newShowDate = DateOnly.FromDateTime(DateTime.Now);
    private int _newShowDuration = 120;
    private string _newShowLocation = string.Empty;
    private string _newShowType = "TV";

    public CalendarViewModel(
        GameRepository? repository = null,
        ShowSchedulerService? showScheduler = null,
        ShowRepository? showRepository = null)
    {
        _repository = repository;
        _showScheduler = showScheduler;
        _showRepository = showRepository;

        UpcomingShows = new ObservableCollection<ShowScheduleItemViewModel>();
        CalendarEntries = new ObservableCollection<CalendarEntryItemViewModel>();

        // Commandes
        CreateNewShowCommand = ReactiveCommand.Create(CreateNewShow);
        CreateShowRapideCommand = ReactiveCommand.Create<DateOnly>(CreateShowRapide);
        UpdateShowScheduleCommand = ReactiveCommand.Create<ShowScheduleItemViewModel>(UpdateShowSchedule);
        CancelShowCommand = ReactiveCommand.Create<ShowScheduleItemViewModel>(CancelShow);
        PreviousMonthCommand = ReactiveCommand.Create(NavigateToPreviousMonth);
        NextMonthCommand = ReactiveCommand.Create(NavigateToNextMonth);

        LoadCalendarData();
        BuildCalendarDays();
    }

    #region Collections

    public ObservableCollection<ShowScheduleItemViewModel> UpcomingShows { get; }
    public ObservableCollection<CalendarEntryItemViewModel> CalendarEntries { get; }
    public ObservableCollection<CalendarDayViewModel> CalendarDays { get; } = new();

    #endregion

    #region Properties

    public DateOnly CurrentDate
    {
        get => _currentDate;
        set => this.RaiseAndSetIfChanged(ref _currentDate, value);
    }

    public string CurrentDateFormatted => CurrentDate.ToString("dddd d MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));

    public CalendarEntryItemViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set => this.RaiseAndSetIfChanged(ref _selectedEntry, value);
    }

    public ShowScheduleItemViewModel? SelectedShow
    {
        get => _selectedShow;
        set => this.RaiseAndSetIfChanged(ref _selectedShow, value);
    }

    // Propriétés pour création de show
    public string NewShowName
    {
        get => _newShowName;
        set => this.RaiseAndSetIfChanged(ref _newShowName, value);
    }

    public DateOnly NewShowDate
    {
        get => _newShowDate;
        set => this.RaiseAndSetIfChanged(ref _newShowDate, value);
    }

    public int NewShowDuration
    {
        get => _newShowDuration;
        set => this.RaiseAndSetIfChanged(ref _newShowDuration, value);
    }

    public string NewShowLocation
    {
        get => _newShowLocation;
        set => this.RaiseAndSetIfChanged(ref _newShowLocation, value);
    }

    public string NewShowType
    {
        get => _newShowType;
        set => this.RaiseAndSetIfChanged(ref _newShowType, value);
    }

    public int TotalUpcomingShows => UpcomingShows.Count;

    #endregion

    #region Commands

    /// <summary>
    /// Commande pour créer un nouveau show
    /// </summary>
    public ReactiveCommand<Unit, Unit> CreateNewShowCommand { get; }

    /// <summary>
    /// Commande pour créer un show rapidement depuis le calendrier
    /// </summary>
    public ReactiveCommand<DateOnly, Unit> CreateShowRapideCommand { get; }

    /// <summary>
    /// Phase 6.3 - Commande pour mettre à jour le planning d'un show
    /// </summary>
    public ReactiveCommand<ShowScheduleItemViewModel, Unit> UpdateShowScheduleCommand { get; }

    /// <summary>
    /// Commande pour annuler un show
    /// </summary>
    public ReactiveCommand<ShowScheduleItemViewModel, Unit> CancelShowCommand { get; }

    /// <summary>
    /// Commande pour naviguer au mois précédent
    /// </summary>
    public ReactiveCommand<Unit, Unit> PreviousMonthCommand { get; }

    /// <summary>
    /// Commande pour naviguer au mois suivant
    /// </summary>
    public ReactiveCommand<Unit, Unit> NextMonthCommand { get; }

    #endregion

    #region Public Methods - Phase 6.3

    /// <summary>
    /// Crée un nouveau show planifié avec formulaire complet
    /// </summary>
    public void CreateNewShow()
    {
        if (_showScheduler == null || string.IsNullOrWhiteSpace(_companyId) || string.IsNullOrWhiteSpace(NewShowName))
        {
            Logger.Warning("Impossible de créer show : paramètres invalides");
            return;
        }

        try
        {
            var showType = Enum.TryParse<ShowType>(NewShowType, true, out var type) ? type : ShowType.Tv;
            
            var draft = new ShowScheduleDraft(
                _companyId,
                NewShowName,
                showType,
                NewShowDate,
                NewShowDuration,
                null, // VenueId
                "TBA", // Broadcast
                0m, // TicketPrice
                null); // BrandId

            var result = _showScheduler.CreerShow(draft);

            if (result.Reussite && result.Show != null)
            {
                var viewModel = new ShowScheduleItemViewModel
                {
                    ShowId = result.Show.ShowId,
                    Name = result.Show.Nom,
                    Date = result.Show.Date,
                    Location = NewShowLocation,
                    ShowType = result.Show.Type.ToString(),
                    Status = result.Show.Statut.ToString(),
                    Broadcast = result.Show.Broadcast ?? "TBA"
                };

                UpcomingShows.Add(viewModel);
                
                // Ajouter à ShowsParJour
                if (!ShowsParJour.ContainsKey(result.Show.Date))
                    ShowsParJour[result.Show.Date] = new List<ShowScheduleItemViewModel>();
                ShowsParJour[result.Show.Date].Add(viewModel);

                this.RaisePropertyChanged(nameof(TotalUpcomingShows));

                Logger.Info($"Show créé : {NewShowName} le {NewShowDate:dd/MM/yyyy}");

                // Réinitialiser les champs
                NewShowName = string.Empty;
                NewShowDate = CurrentDate.AddDays(1);
                NewShowLocation = string.Empty;
            }
            else
            {
                Logger.Warning($"Erreur création show : {string.Join(", ", result.Erreurs)}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur création show : {ex.Message}");
        }
    }

    /// <summary>
    /// Crée un show rapidement depuis le calendrier (clic droit sur un jour)
    /// </summary>
    public void CreateShowRapide(DateOnly date)
    {
        if (_showScheduler == null || string.IsNullOrWhiteSpace(_companyId))
        {
            Logger.Warning("Impossible de créer show rapide : services non disponibles");
            return;
        }

        try
        {
            var showType = Enum.TryParse<ShowType>(NewShowType, true, out var type) ? type : ShowType.Tv;
            var result = _showScheduler.CreerShowRapide(_companyId, showType, date);

            if (result.Reussite && result.Show != null)
            {
                var viewModel = new ShowScheduleItemViewModel
                {
                    ShowId = result.Show.ShowId,
                    Name = result.Show.Nom,
                    Date = result.Show.Date,
                    Location = "",
                    ShowType = result.Show.Type.ToString(),
                    Status = result.Show.Statut.ToString(),
                    Broadcast = result.Show.Broadcast ?? "TBA"
                };

                UpcomingShows.Add(viewModel);
                
                // Ajouter à ShowsParJour
                if (!ShowsParJour.ContainsKey(date))
                    ShowsParJour[date] = new List<ShowScheduleItemViewModel>();
                ShowsParJour[date].Add(viewModel);

                this.RaisePropertyChanged(nameof(TotalUpcomingShows));
                Logger.Info($"Show rapide créé : {result.Show.Nom} le {date:dd/MM/yyyy}");
            }
            else
            {
                Logger.Warning($"Erreur création show rapide : {string.Join(", ", result.Erreurs)}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur création show rapide : {ex.Message}");
        }
    }

    /// <summary>
    /// Obtient les shows pour un jour spécifique
    /// </summary>
    public IReadOnlyList<ShowScheduleItemViewModel> GetShowsForDay(DateOnly date)
    {
        return ShowsParJour.TryGetValue(date, out var shows) ? shows : Array.Empty<ShowScheduleItemViewModel>();
    }

    /// <summary>
    /// Construit la grille de jours du calendrier pour le mois actuel
    /// </summary>
    private void BuildCalendarDays()
    {
        CalendarDays.Clear();

        var firstDayOfMonth = new DateOnly(CurrentDate.Year, CurrentDate.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        
        // Trouver le premier lundi du mois (ou avant si le mois commence un autre jour)
        var startDate = firstDayOfMonth;
        while (startDate.DayOfWeek != DayOfWeek.Monday)
        {
            startDate = startDate.AddDays(-1);
        }

        // Créer 42 jours (6 semaines x 7 jours)
        for (int i = 0; i < 42; i++)
        {
            var date = startDate.AddDays(i);
            var isCurrentMonth = date.Month == CurrentDate.Month;
            var shows = GetShowsForDay(date).ToList();

            CalendarDays.Add(new CalendarDayViewModel
            {
                Date = date,
                DayNumber = date.Day,
                IsCurrentMonth = isCurrentMonth,
                IsToday = date == DateOnly.FromDateTime(DateTime.Now),
                Shows = shows
            });
        }
    }

    /// <summary>
    /// Navigue au mois précédent
    /// </summary>
    private void NavigateToPreviousMonth()
    {
        CurrentDate = CurrentDate.AddMonths(-1);
        BuildCalendarDays();
    }

    /// <summary>
    /// Navigue au mois suivant
    /// </summary>
    private void NavigateToNextMonth()
    {
        CurrentDate = CurrentDate.AddMonths(1);
        BuildCalendarDays();
    }

    /// <summary>
    /// Met à jour le planning d'un show
    /// </summary>
    public void UpdateShowSchedule(ShowScheduleItemViewModel? show)
    {
        if (_showScheduler == null || show == null)
        {
            Logger.Warning("Impossible de modifier planning : paramètres invalides");
            return;
        }

        try
        {
            var update = new ShowScheduleUpdate(
                Nom: null,
                Date: NewShowDate,
                RuntimeMinutes: null,
                VenueId: null,
                Broadcast: null,
                TicketPrice: null,
                Statut: null);

            var result = _showScheduler.MettreAJourShow(show.ShowId, update);

            if (result.Reussite && result.Show != null)
            {
                // Retirer de l'ancienne date
                if (ShowsParJour.ContainsKey(show.Date))
                    ShowsParJour[show.Date].Remove(show);

                // Mettre à jour la date
                show.Date = result.Show.Date;

                // Ajouter à la nouvelle date
                if (!ShowsParJour.ContainsKey(result.Show.Date))
                    ShowsParJour[result.Show.Date] = new List<ShowScheduleItemViewModel>();
                ShowsParJour[result.Show.Date].Add(show);

                Logger.Info($"Show '{show.Name}' replanifié au {result.Show.Date:dd/MM/yyyy}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur mise à jour planning : {ex.Message}");
        }
    }

    /// <summary>
    /// Annule un show planifié
    /// </summary>
    public void CancelShow(ShowScheduleItemViewModel? show)
    {
        if (_showScheduler == null || show == null)
        {
            Logger.Warning("Impossible d'annuler show : paramètre invalide");
            return;
        }

        try
        {
            var result = _showScheduler.AnnulerShow(show.ShowId);

            if (result.Reussite)
            {
                // Retirer de ShowsParJour
                if (ShowsParJour.ContainsKey(show.Date))
                    ShowsParJour[show.Date].Remove(show);

                UpcomingShows.Remove(show);
                this.RaisePropertyChanged(nameof(TotalUpcomingShows));

                Logger.Info($"Show annulé : {show.Name}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur annulation show : {ex.Message}");
        }
    }

    #endregion

    #region Private Methods

    private void LoadCalendarData()
    {
        if (_repository == null || _showRepository == null)
        {
            LoadPlaceholderData();
            return;
        }

        try
        {
            // Obtenir la compagnie du joueur
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT CompanyId, CurrentDate 
                FROM SaveGames 
                WHERE IsActive = 1 
                LIMIT 1;
                """;
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                _companyId = reader.GetString(0);
                var dateStr = reader.GetString(1);
                if (DateOnly.TryParse(dateStr, out var date))
                {
                    CurrentDate = date;
                }
            }

            if (string.IsNullOrWhiteSpace(_companyId))
            {
                LoadPlaceholderData();
                return;
            }

            // Charger les shows des 60 prochains jours
            var startDate = CurrentDate;
            var shows = _showRepository.ChargerShowsProchainsJours(_companyId, startDate, 60);

            foreach (var show in shows)
            {
                var viewModel = new ShowScheduleItemViewModel
                {
                    ShowId = show.ShowId,
                    Name = show.Nom,
                    Date = show.Date,
                    Location = show.VenueId ?? "",
                    Broadcast = show.Broadcast ?? "TBA",
                    ShowType = show.Type.ToString(),
                    Status = show.Statut.ToString()
                };

                UpcomingShows.Add(viewModel);

                // Ajouter à ShowsParJour
                if (!ShowsParJour.ContainsKey(show.Date))
                    ShowsParJour[show.Date] = new List<ShowScheduleItemViewModel>();
                ShowsParJour[show.Date].Add(viewModel);
            }

            // Charger les entrées du calendrier
            using (var cmd2 = connection.CreateCommand())
            {
                cmd2.CommandText = """
                    SELECT CalendarEntryId, Date, EntryType, Title, Notes
                    FROM CalendarEntries
                    WHERE CompanyId = $companyId
                    ORDER BY Date ASC
                    LIMIT 60
                    """;
                cmd2.Parameters.AddWithValue("$companyId", _companyId);

                using var reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    CalendarEntries.Add(new CalendarEntryItemViewModel
                    {
                        EntryId = reader2.GetString(0),
                        Date = reader2.GetString(1),
                        EntryType = reader2.GetString(2),
                        Title = reader2.IsDBNull(3) ? string.Empty : reader2.GetString(3),
                        Notes = reader2.IsDBNull(4) ? string.Empty : reader2.GetString(4)
                    });
                }
            }

            Logger.Info($"Date {CurrentDate:dd/MM/yyyy}, {UpcomingShows.Count} shows à venir, {CalendarEntries.Count} entrées");
            
            // Reconstruire le calendrier après chargement
            BuildCalendarDays();
        }
        catch (Exception ex)
        {
            Logger.Error($"[CalendarViewModel] Erreur: {ex.Message}");
            LoadPlaceholderData();
        }
    }

    private void LoadPlaceholderData()
    {
        CurrentDate = DateOnly.FromDateTime(DateTime.Now);
        var today = CurrentDate;

        var show1 = new ShowScheduleItemViewModel
        {
            ShowId = "SHOW_RAW_001",
            Name = "Monday Night Raw",
            Date = today.AddDays(1),
            Location = "Madison Square Garden, New York",
            Broadcast = "USA Network",
            ShowType = "TV",
            Status = "ABOOKER"
        };
        UpcomingShows.Add(show1);
        ShowsParJour[today.AddDays(1)] = new List<ShowScheduleItemViewModel> { show1 };

        var show2 = new ShowScheduleItemViewModel
        {
            ShowId = "SHOW_SD_001",
            Name = "Friday Night SmackDown",
            Date = today.AddDays(5),
            Location = "Staples Center, Los Angeles",
            Broadcast = "FOX",
            ShowType = "TV",
            Status = "ABOOKER"
        };
        UpcomingShows.Add(show2);
        ShowsParJour[today.AddDays(5)] = new List<ShowScheduleItemViewModel> { show2 };

        CalendarEntries.Add(new CalendarEntryItemViewModel
        {
            EntryId = "CAL_001",
            Date = today.AddDays(1).ToString("yyyy-MM-dd"),
            EntryType = "Show",
            Title = "Monday Night Raw",
            Notes = "Season premiere"
        });

        BuildCalendarDays();
    }

    #endregion
}

/// <summary>
/// ViewModel pour un jour du calendrier
/// </summary>
public sealed class CalendarDayViewModel : ViewModelBase
{
    private DateOnly _date;
    private int _dayNumber;
    private bool _isCurrentMonth = true;
    private bool _isToday;
    private List<ShowScheduleItemViewModel> _shows = new();

    public DateOnly Date
    {
        get => _date;
        set => this.RaiseAndSetIfChanged(ref _date, value);
    }

    public int DayNumber
    {
        get => _dayNumber;
        set => this.RaiseAndSetIfChanged(ref _dayNumber, value);
    }

    public bool IsCurrentMonth
    {
        get => _isCurrentMonth;
        set => this.RaiseAndSetIfChanged(ref _isCurrentMonth, value);
    }

    public bool IsToday
    {
        get => _isToday;
        set => this.RaiseAndSetIfChanged(ref _isToday, value);
    }

    public List<ShowScheduleItemViewModel> Shows
    {
        get => _shows;
        set => this.RaiseAndSetIfChanged(ref _shows, value);
    }
}

public sealed class ShowScheduleItemViewModel : ViewModelBase
{
    private string _showId = string.Empty;
    private string _name = string.Empty;
    private DateOnly _date = DateOnly.FromDateTime(DateTime.Now);
    private string _location = string.Empty;
    private string _broadcast = string.Empty;
    private string _showType = "TV";
    private string _status = "ABOOKER";

    public string ShowId
    {
        get => _showId;
        set => this.RaiseAndSetIfChanged(ref _showId, value);
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public DateOnly Date
    {
        get => _date;
        set => this.RaiseAndSetIfChanged(ref _date, value);
    }

    public string Location
    {
        get => _location;
        set => this.RaiseAndSetIfChanged(ref _location, value);
    }

    public string Broadcast
    {
        get => _broadcast;
        set => this.RaiseAndSetIfChanged(ref _broadcast, value);
    }

    public string ShowType
    {
        get => _showType;
        set => this.RaiseAndSetIfChanged(ref _showType, value);
    }

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string DateDisplay => Date.ToString("dd/MM/yyyy");
    public string DateDisplayLong => Date.ToString("dddd d MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
    public string ShowTypeIcon => ShowType switch
    {
        "PPV" => "🏆",
        "Tv" => "📺",
        "TV" => "📺",
        "House" => "🏠",
        "Youth" => "🎓",
        _ => "📅"
    };
    public string StatusDisplay => Status switch
    {
        "ABooker" => "À Booker",
        "ABOOKER" => "À Booker",
        "Booke" => "Booké",
        "Simule" => "Simulé",
        "Annule" => "Annulé",
        _ => Status
    };
    public string StatusColor => Status switch
    {
        "ABooker" or "ABOOKER" => "#f59e0b",
        "Booke" => "#3b82f6",
        "Simule" => "#10b981",
        "Annule" => "#ef4444",
        _ => "#94a3b8"
    };
}

public sealed class CalendarEntryItemViewModel : ViewModelBase
{
    private string _entryId = string.Empty;
    private string _date = string.Empty;
    private string _entryType = string.Empty;
    private string _title = string.Empty;
    private string _notes = string.Empty;

    public string EntryId
    {
        get => _entryId;
        set => this.RaiseAndSetIfChanged(ref _entryId, value);
    }

    public string Date
    {
        get => _date;
        set => this.RaiseAndSetIfChanged(ref _date, value);
    }

    public string EntryType
    {
        get => _entryType;
        set => this.RaiseAndSetIfChanged(ref _entryType, value);
    }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    public string TypeIcon => EntryType switch
    {
        "Show" => "📺",
        "PPV" => "🏆",
        "Contract" => "📝",
        "Injury" => "🏥",
        _ => "📅"
    };
}
