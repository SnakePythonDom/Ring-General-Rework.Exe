using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Calendar;

/// <summary>
/// ViewModel pour le calendrier et la planification des shows
/// </summary>
public sealed class CalendarViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private int _currentWeek = 1;
    private CalendarEntryItemViewModel? _selectedEntry;

    public CalendarViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        UpcomingShows = new ObservableCollection<ShowScheduleItemViewModel>();
        CalendarEntries = new ObservableCollection<CalendarEntryItemViewModel>();

        LoadCalendarData();
    }

    public ObservableCollection<ShowScheduleItemViewModel> UpcomingShows { get; }
    public ObservableCollection<CalendarEntryItemViewModel> CalendarEntries { get; }

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

            System.Console.WriteLine($"[CalendarViewModel] Week {CurrentWeek}, {UpcomingShows.Count} shows à venir, {CalendarEntries.Count} entrées");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[CalendarViewModel] Erreur: {ex.Message}");
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
