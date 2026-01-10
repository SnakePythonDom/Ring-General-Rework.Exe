using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Core.Services;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Company;

/// <summary>
/// ViewModel pour gérer le contrôle de booking des child companies
/// </summary>
public sealed class ChildCompanyBookingViewModel : ViewModelBase
{
    private readonly ChildCompanyBookingService _bookingService;
    private readonly ShowRepository? _showRepository;
    private readonly GameRepository? _gameRepository;
    private ChildCompanyBookingItemViewModel? _selectedChildCompany;

    public ChildCompanyBookingViewModel(
        ChildCompanyBookingService bookingService,
        ShowRepository? showRepository = null,
        GameRepository? gameRepository = null)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _showRepository = showRepository;
        _gameRepository = gameRepository;

        ChildCompanies = new ObservableCollection<ChildCompanyBookingItemViewModel>();
        UpcomingShows = new ObservableCollection<ShowScheduleItemViewModel>();

        // Commandes
        SetControlLevelCommand = ReactiveCommand.Create<BookingControlLevel>(SetControlLevel);
        ToggleAutoScheduleCommand = ReactiveCommand.Create(ToggleAutoSchedule);
        ViewShowsCommand = ReactiveCommand.Create(ViewShows);
        RefreshCommand = ReactiveCommand.Create(LoadChildCompanies);

        LoadChildCompanies();
    }

    #region Collections

    public ObservableCollection<ChildCompanyBookingItemViewModel> ChildCompanies { get; }
    public ObservableCollection<ShowScheduleItemViewModel> UpcomingShows { get; }

    #endregion

    #region Properties

    public ChildCompanyBookingItemViewModel? SelectedChildCompany
    {
        get => _selectedChildCompany;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedChildCompany, value);
            if (value != null)
            {
                LoadUpcomingShows(value.ChildCompanyId);
                this.RaisePropertyChanged(nameof(CurrentControlLevel));
                this.RaisePropertyChanged(nameof(AutoScheduleEnabled));
            }
        }
    }

    private BookingControlLevel _currentControlLevel = BookingControlLevel.CoBooker;
    private bool _autoScheduleEnabled;

    public BookingControlLevel CurrentControlLevel
    {
        get => SelectedChildCompany?.ControlLevel ?? BookingControlLevel.CoBooker;
        set
        {
            if (SelectedChildCompany != null && SelectedChildCompany.ControlLevel != value)
            {
                SetControlLevel(value);
            }
            this.RaiseAndSetIfChanged(ref _currentControlLevel, value);
        }
    }

    public bool AutoScheduleEnabled
    {
        get => SelectedChildCompany?.AutoScheduleShows ?? false;
        set
        {
            if (SelectedChildCompany != null && SelectedChildCompany.AutoScheduleShows != value)
            {
                ToggleAutoSchedule();
            }
            this.RaiseAndSetIfChanged(ref _autoScheduleEnabled, value);
        }
    }

    public ObservableCollection<BookingControlLevel> ControlLevels { get; } = new()
    {
        BookingControlLevel.Spectator,
        BookingControlLevel.Producer,
        BookingControlLevel.CoBooker,
        BookingControlLevel.Dictator
    };

    #endregion

    #region Commands

    public ReactiveCommand<BookingControlLevel, Unit> SetControlLevelCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleAutoScheduleCommand { get; }
    public ReactiveCommand<Unit, Unit> ViewShowsCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    #endregion

    #region Methods

    private void LoadChildCompanies()
    {
        if (_gameRepository == null)
            return;

        try
        {
            // Obtenir la compagnie du joueur
            string? parentCompanyId = null;
            using var connection = _gameRepository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT PlayerCompanyId 
                FROM SaveGames 
                WHERE IsActive = 1 
                LIMIT 1;
                """;
            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                parentCompanyId = result.ToString();
            }

            if (string.IsNullOrWhiteSpace(parentCompanyId))
                return;

            // Charger les child companies
            cmd.CommandText = """
                SELECT ChildCompanyId, Name, Level
                FROM ChildCompanies
                WHERE ParentCompanyId = $parentId;
                """;
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("$parentId", parentCompanyId);

            ChildCompanies.Clear();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var childCompanyId = reader.GetString(0);
                var controlLevel = _bookingService.GetBookingControlLevel(childCompanyId);
                var autoSchedule = _bookingService.GetAutoScheduleShows(childCompanyId);

                ChildCompanies.Add(new ChildCompanyBookingItemViewModel
                {
                    ChildCompanyId = childCompanyId,
                    Name = reader.GetString(1),
                    Level = reader.IsDBNull(2) ? "Development" : reader.GetString(2),
                    ControlLevel = controlLevel,
                    AutoScheduleShows = autoSchedule
                });
            }

            Logger.Info($"{ChildCompanies.Count} child companies chargées");
        }
        catch (Exception ex)
        {
            Logger.Error($"[ChildCompanyBookingViewModel] Erreur chargement: {ex.Message}");
        }
    }

    private void SetControlLevel(BookingControlLevel level)
    {
        if (SelectedChildCompany == null)
            return;

        try
        {
            _bookingService.SetBookingControlLevel(SelectedChildCompany.ChildCompanyId, level);
            SelectedChildCompany.ControlLevel = level;
            this.RaisePropertyChanged(nameof(CurrentControlLevel));
            Logger.Info($"Niveau de contrôle changé à {level} pour {SelectedChildCompany.Name}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur changement niveau contrôle: {ex.Message}");
        }
    }

    private void ToggleAutoSchedule()
    {
        if (SelectedChildCompany == null)
            return;

        try
        {
            var newValue = !SelectedChildCompany.AutoScheduleShows;
            _bookingService.SetAutoScheduleShows(SelectedChildCompany.ChildCompanyId, newValue);
            SelectedChildCompany.AutoScheduleShows = newValue;
            this.RaisePropertyChanged(nameof(AutoScheduleEnabled));
            Logger.Info($"Planification automatique {(newValue ? "activée" : "désactivée")} pour {SelectedChildCompany.Name}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur toggle planification auto: {ex.Message}");
        }
    }

    private void ViewShows()
    {
        if (SelectedChildCompany == null || _showRepository == null)
            return;

        try
        {
            var startDate = DateOnly.FromDateTime(DateTime.Now);
            var shows = _showRepository.ChargerShowsProchainsJours(SelectedChildCompany.ChildCompanyId, startDate, 60);

            UpcomingShows.Clear();
            foreach (var show in shows)
            {
                UpcomingShows.Add(new ShowScheduleItemViewModel
                {
                    ShowId = show.ShowId,
                    Name = show.Nom,
                    Date = show.Date,
                    Location = show.VenueId ?? "",
                    Broadcast = show.Broadcast ?? "TBA",
                    ShowType = show.Type.ToString(),
                    Status = show.Statut.ToString()
                });
            }

            Logger.Info($"{UpcomingShows.Count} shows chargés pour {SelectedChildCompany.Name}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur chargement shows: {ex.Message}");
        }
    }

    private void LoadUpcomingShows(string childCompanyId)
    {
        ViewShows();
    }

    #endregion
}

public sealed class ChildCompanyBookingItemViewModel : ViewModelBase
{
    private string _childCompanyId = string.Empty;
    private string _name = string.Empty;
    private string _level = "Development";
    private BookingControlLevel _controlLevel = BookingControlLevel.CoBooker;
    private bool _autoScheduleShows;

    public string ChildCompanyId
    {
        get => _childCompanyId;
        set => this.RaiseAndSetIfChanged(ref _childCompanyId, value);
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string Level
    {
        get => _level;
        set => this.RaiseAndSetIfChanged(ref _level, value);
    }

    public BookingControlLevel ControlLevel
    {
        get => _controlLevel;
        set => this.RaiseAndSetIfChanged(ref _controlLevel, value);
    }

    public bool AutoScheduleShows
    {
        get => _autoScheduleShows;
        set => this.RaiseAndSetIfChanged(ref _autoScheduleShows, value);
    }

    public string ControlLevelDisplay => ControlLevel switch
    {
        BookingControlLevel.Spectator => "👁️ Spectator",
        BookingControlLevel.Producer => "🎬 Producer",
        BookingControlLevel.CoBooker => "🤝 CoBooker",
        BookingControlLevel.Dictator => "👑 Dictator",
        _ => ControlLevel.ToString()
    };

    public string ControlLevelDescription => ControlLevel switch
    {
        BookingControlLevel.Spectator => "IA contrôle 100% des décisions",
        BookingControlLevel.Producer => "IA propose, vous validez",
        BookingControlLevel.CoBooker => "Vous gérez titres majeurs, IA développe midcard",
        BookingControlLevel.Dictator => "Contrôle total, pas d'intervention IA",
        _ => ""
    };
}
