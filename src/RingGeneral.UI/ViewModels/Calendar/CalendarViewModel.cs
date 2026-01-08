using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels.Calendar;

/// <summary>
/// ViewModel pour le calendrier et la planification des shows.
/// Enrichi dans Phase 6.3 avec création et gestion de shows.
/// </summary>
public sealed class CalendarViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private ShowContext? _context;
    private int _currentWeek = 1;
    private CalendarEntryItemViewModel? _selectedEntry;
    private ShowScheduleItemViewModel? _selectedShow;

    // Phase 6.3 - Propriétés pour nouveau show
    private string _newShowName = string.Empty;
    private int _newShowWeek = 1;
    private int _newShowDuration = 120;
    private string _newShowLocation = string.Empty;
    private string _newShowType = "TV";

    public CalendarViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        UpcomingShows = new ObservableCollection<ShowScheduleItemViewModel>();
        CalendarEntries = new ObservableCollection<CalendarEntryItemViewModel>();

        // Phase 6.3 - Commandes
        CreateNewShowCommand = ReactiveCommand.Create(CreateNewShow);
        UpdateShowScheduleCommand = ReactiveCommand.Create<ShowScheduleItemViewModel>(UpdateShowSchedule);
        CancelShowCommand = ReactiveCommand.Create<ShowScheduleItemViewModel>(CancelShow);

        LoadCalendarData();
    }

    #region Collections

    public ObservableCollection<ShowScheduleItemViewModel> UpcomingShows { get; }
    public ObservableCollection<CalendarEntryItemViewModel> CalendarEntries { get; }

    #endregion

    #region Properties

    public int CurrentWeek
    {
        get => _currentWeek;
        set => this.RaiseAndSetIfChanged(ref _currentWeek, value);
    }

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

    // Phase 6.3 - Propriétés pour création de show
    public string NewShowName
    {
        get => _newShowName;
        set => this.RaiseAndSetIfChanged(ref _newShowName, value);
    }

    public int NewShowWeek
    {
        get => _newShowWeek;
        set => this.RaiseAndSetIfChanged(ref _newShowWeek, value);
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
    /// Phase 6.3 - Commande pour créer un nouveau show
    /// </summary>
    public ReactiveCommand<Unit, Unit> CreateNewShowCommand { get; }

    /// <summary>
    /// Phase 6.3 - Commande pour mettre à jour le planning d'un show
    /// </summary>
    public ReactiveCommand<ShowScheduleItemViewModel, Unit> UpdateShowScheduleCommand { get; }

    /// <summary>
    /// Phase 6.3 - Commande pour annuler un show
    /// </summary>
    public ReactiveCommand<ShowScheduleItemViewModel, Unit> CancelShowCommand { get; }

    #endregion

    #region Public Methods - Phase 6.3

    /// <summary>
    /// Phase 6.3 - Crée un nouveau show planifié
    /// </summary>
    public void CreateNewShow()
    {
        if (_repository == null || string.IsNullOrWhiteSpace(NewShowName))
        {
            Logger.Warning("Impossible de créer show : nom invalide");
            return;
        }

        try
        {
            var showId = $"SHOW-{Guid.NewGuid():N}".ToUpperInvariant();

            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Shows (ShowId, Name, Week, Location, Duration, ShowType, Status, Broadcast)
                VALUES (@id, @name, @week, @location, @duration, @type, 'ABOOKER', 'TBA')";

            cmd.Parameters.AddWithValue("@id", showId);
            cmd.Parameters.AddWithValue("@name", NewShowName);
            cmd.Parameters.AddWithValue("@week", NewShowWeek);
            cmd.Parameters.AddWithValue("@location", string.IsNullOrWhiteSpace(NewShowLocation) ? "TBA" : NewShowLocation);
            cmd.Parameters.AddWithValue("@duration", NewShowDuration);
            cmd.Parameters.AddWithValue("@type", NewShowType);

            cmd.ExecuteNonQuery();

            UpcomingShows.Add(new ShowScheduleItemViewModel
            {
                ShowId = showId,
                Name = NewShowName,
                Week = NewShowWeek,
                Location = NewShowLocation,
                ShowType = NewShowType,
                Status = "ABOOKER",
                Broadcast = "TBA"
            });

            this.RaisePropertyChanged(nameof(TotalUpcomingShows));

            Logger.Info($"Show créé : {NewShowName} à la semaine {NewShowWeek}");

            // Réinitialiser les champs
            NewShowName = string.Empty;
            NewShowWeek = CurrentWeek + 1;
            NewShowLocation = string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur création show : {ex.Message}");
        }
    }

    /// <summary>
    /// Phase 6.3 - Met à jour le planning d'un show
    /// </summary>
    public void UpdateShowSchedule(ShowScheduleItemViewModel? show)
    {
        if (_repository == null || show == null || NewShowWeek <= 0)
        {
            Logger.Warning("Impossible de modifier planning : paramètres invalides");
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Shows
                SET Week = @week
                WHERE ShowId = @id";

            cmd.Parameters.AddWithValue("@week", NewShowWeek);
            cmd.Parameters.AddWithValue("@id", show.ShowId);

            cmd.ExecuteNonQuery();

            show.Week = NewShowWeek;

            Logger.Info($"Show '{show.Name}' replanifié à la semaine {NewShowWeek}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur mise à jour planning : {ex.Message}");
        }
    }

    /// <summary>
    /// Phase 6.3 - Annule un show planifié
    /// </summary>
    public void CancelShow(ShowScheduleItemViewModel? show)
    {
        if (_repository == null || show == null)
        {
            Logger.Warning("Impossible d'annuler show : paramètre invalide");
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Shows
                SET Status = 'ANNULE'
                WHERE ShowId = @id";

            cmd.Parameters.AddWithValue("@id", show.ShowId);

            cmd.ExecuteNonQuery();

            UpcomingShows.Remove(show);
            this.RaisePropertyChanged(nameof(TotalUpcomingShows));

            Logger.Info($"Show annulé : {show.Name}");
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
        if (_repository == null)
        {
            LoadPlaceholderData();
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();

            // Charger la semaine courante
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT CurrentWeek FROM Companies WHERE IsPlayerControlled = 1 LIMIT 1";
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    CurrentWeek = Convert.ToInt32(result);
                }
            }

            // Charger les shows à venir (prochaines 8 semaines)
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT ShowId, Name, Week, Location, Broadcast, ShowType, Status
                    FROM Shows
                    WHERE Week >= @currentWeek AND Week <= @maxWeek
                    ORDER BY Week ASC";

                cmd.Parameters.AddWithValue("@currentWeek", CurrentWeek);
                cmd.Parameters.AddWithValue("@maxWeek", CurrentWeek + 8);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    UpcomingShows.Add(new ShowScheduleItemViewModel
                    {
                        ShowId = reader.GetString(0),
                        Name = reader.GetString(1),
                        Week = reader.GetInt32(2),
                        Location = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        Broadcast = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        ShowType = reader.IsDBNull(5) ? "TV" : reader.GetString(5),
                        Status = reader.IsDBNull(6) ? "ABOOKER" : reader.GetString(6)
                    });
                }
            }

            // Charger les entrées du calendrier
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT CalendarEntryId, Date, EntryType, Title, Notes
                    FROM CalendarEntries
                    ORDER BY Date ASC
                    LIMIT 30";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    CalendarEntries.Add(new CalendarEntryItemViewModel
                    {
                        EntryId = reader.GetString(0),
                        Date = reader.GetString(1),
                        EntryType = reader.GetString(2),
                        Title = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        Notes = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                    });
                }
            }

            Logger.Info($"Week {CurrentWeek}, {UpcomingShows.Count} shows à venir, {CalendarEntries.Count} entrées");
        }
        catch (Exception ex)
        {
            Logger.Error($"[CalendarViewModel] Erreur: {ex.Message}");
            LoadPlaceholderData();
        }
    }

    private void LoadPlaceholderData()
    {
        CurrentWeek = 1;

        UpcomingShows.Add(new ShowScheduleItemViewModel
        {
            ShowId = "SHOW_RAW_W1",
            Name = "Monday Night Raw",
            Week = 1,
            Location = "Madison Square Garden, New York",
            Broadcast = "USA Network",
            ShowType = "TV",
            Status = "ABOOKER"
        });
        UpcomingShows.Add(new ShowScheduleItemViewModel
        {
            ShowId = "SHOW_SD_W1",
            Name = "Friday Night SmackDown",
            Week = 1,
            Location = "Staples Center, Los Angeles",
            Broadcast = "FOX",
            ShowType = "TV",
            Status = "ABOOKER"
        });
        UpcomingShows.Add(new ShowScheduleItemViewModel
        {
            ShowId = "SHOW_PPV_W4",
            Name = "Royal Rumble",
            Week = 4,
            Location = "Wells Fargo Center, Philadelphia",
            Broadcast = "Pay-Per-View",
            ShowType = "PPV",
            Status = "PLANIFIE"
        });

        CalendarEntries.Add(new CalendarEntryItemViewModel
        {
            EntryId = "CAL_001",
            Date = "2024-01-15",
            EntryType = "Show",
            Title = "Monday Night Raw",
            Notes = "Season premiere"
        });
        CalendarEntries.Add(new CalendarEntryItemViewModel
        {
            EntryId = "CAL_002",
            Date = "2024-01-28",
            EntryType = "PPV",
            Title = "Royal Rumble",
            Notes = "Major PPV event"
        });
    }
}

public sealed class ShowScheduleItemViewModel : ViewModelBase
{
    private string _showId = string.Empty;
    private string _name = string.Empty;
    private int _week;
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

    public int Week
    {
        get => _week;
        set => this.RaiseAndSetIfChanged(ref _week, value);
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

    public string WeekDisplay => $"Week {Week}";
    public string ShowTypeIcon => ShowType switch
    {
        "PPV" => "🏆",
        "TV" => "📺",
        "House" => "🏠",
        _ => "📅"
    };
    public string StatusDisplay => Status switch
    {
        "ABOOKER" => "To Book",
        "PLANIFIE" => "Scheduled",
        "TERMINE" => "Completed",
        _ => Status
    };
    public string StatusColor => Status switch
    {
        "ABOOKER" => "#f59e0b",
        "PLANIFIE" => "#3b82f6",
        "TERMINE" => "#10b981",
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
