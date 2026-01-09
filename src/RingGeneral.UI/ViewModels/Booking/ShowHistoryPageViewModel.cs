using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Booking;

/// <summary>
/// ViewModel pour afficher l'historique des shows passés
/// Permet de consulter les performances, notes, et statistiques des shows
/// </summary>
public sealed class ShowHistoryPageViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private ShowHistoryEntryViewModel? _selectedEntry;
    private string _selectedShow = "Tous";
    private int _selectedWeekStart = 1;
    private int _selectedWeekEnd = 52;

    public ShowHistoryPageViewModel(GameRepository repository)
    {
        _repository = repository;

        // Collections
        HistoryEntries = new ObservableCollection<ShowHistoryEntryViewModel>();
        Shows = new ObservableCollection<string> { "Tous", "Monday Night Raw", "SmackDown", "PPV" };
        WeekOptions = new ObservableCollection<int>(Enumerable.Range(1, 52));

        // Commands
        ViewDetailsCommand = ReactiveCommand.Create<ShowHistoryEntryViewModel>(ViewDetails);
        ExportCommand = ReactiveCommand.Create(ExportHistory);
        FilterCommand = ReactiveCommand.Create(ApplyFilter);
        RefreshCommand = ReactiveCommand.Create(LoadHistory);

        // Initialisation
        LoadHistory();
    }

    // ========== COLLECTIONS ==========

    public ObservableCollection<ShowHistoryEntryViewModel> HistoryEntries { get; }
    public ObservableCollection<string> Shows { get; }
    public ObservableCollection<int> WeekOptions { get; }

    // ========== PROPRIÉTÉS ==========

    public ShowHistoryEntryViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set => this.RaiseAndSetIfChanged(ref _selectedEntry, value);
    }

    public string SelectedShow
    {
        get => _selectedShow;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedShow, value);
            ApplyFilter();
        }
    }

    public int SelectedWeekStart
    {
        get => _selectedWeekStart;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedWeekStart, value);
            ApplyFilter();
        }
    }

    public int SelectedWeekEnd
    {
        get => _selectedWeekEnd;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedWeekEnd, value);
            ApplyFilter();
        }
    }

    // ========== STATISTIQUES ==========

    public int TotalShows => HistoryEntries.Count;
    public double AverageRating => HistoryEntries.Count > 0
        ? HistoryEntries.Average(e => e.Rating)
        : 0.0;
    public int AverageAttendance => HistoryEntries.Count > 0
        ? (int)HistoryEntries.Average(e => e.Attendance)
        : 0;
    public string BestShow => HistoryEntries
        .OrderByDescending(e => e.Rating)
        .FirstOrDefault()?.ShowName ?? "N/A";

    // ========== COMMANDS ==========

    public ReactiveCommand<ShowHistoryEntryViewModel, Unit> ViewDetailsCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    // ========== MÉTHODES PUBLIQUES ==========

    public void LoadHistory()
    {
        // TODO: Charger depuis le repository
        HistoryEntries.Clear();

        // Données de test
        LoadTestData();

        UpdateStatistics();
    }

    // ========== MÉTHODES PRIVÉES ==========

    private void ViewDetails(ShowHistoryEntryViewModel entry)
    {
        // TODO: Ouvrir panneau de détails ou nouvelle vue
        System.Diagnostics.Debug.WriteLine($"Viewing details for: {entry.ShowName} Week {entry.Week}");
    }

    private void ExportHistory()
    {
        // TODO: Exporter l'historique en CSV ou JSON
        System.Diagnostics.Debug.WriteLine($"Exporting history ({HistoryEntries.Count} entries)");
    }

    private void ApplyFilter()
    {
        // TODO: Implémenter filtre réel avec requête repository
        LoadHistory();
    }

    private void UpdateStatistics()
    {
        this.RaisePropertyChanged(nameof(TotalShows));
        this.RaisePropertyChanged(nameof(AverageRating));
        this.RaisePropertyChanged(nameof(AverageAttendance));
        this.RaisePropertyChanged(nameof(BestShow));
    }

    private void LoadTestData()
    {
        // Historique de test
        HistoryEntries.Add(new ShowHistoryEntryViewModel(
            "SH001",
            "Monday Night Raw",
            "SHOW001",
            24,
            85,
            15000,
            "Excellent show avec main event mémorable",
            DateTime.Now.AddDays(-7)
        ));

        HistoryEntries.Add(new ShowHistoryEntryViewModel(
            "SH002",
            "SmackDown",
            "SHOW002",
            24,
            78,
            12000,
            "Bon show, quelques segments faibles",
            DateTime.Now.AddDays(-10)
        ));

        HistoryEntries.Add(new ShowHistoryEntryViewModel(
            "SH003",
            "Monday Night Raw",
            "SHOW001",
            23,
            92,
            16500,
            "Show exceptionnel, meilleur de l'année",
            DateTime.Now.AddDays(-14)
        ));

        HistoryEntries.Add(new ShowHistoryEntryViewModel(
            "SH004",
            "PPV - WrestleMania",
            "SHOW003",
            23,
            95,
            75000,
            "WrestleMania historique",
            DateTime.Now.AddDays(-21)
        ));

        HistoryEntries.Add(new ShowHistoryEntryViewModel(
            "SH005",
            "SmackDown",
            "SHOW002",
            23,
            81,
            13000,
            "Show correct avec bonne storyline progression",
            DateTime.Now.AddDays(-17)
        ));

        HistoryEntries.Add(new ShowHistoryEntryViewModel(
            "SH006",
            "Monday Night Raw",
            "SHOW001",
            22,
            76,
            14000,
            "Show moyen, manque d'intensité",
            DateTime.Now.AddDays(-21)
        ));
    }
}

// ========== VIEWMODEL HELPER ==========

/// <summary>
/// ViewModel représentant une entrée d'historique de show
/// </summary>
public sealed class ShowHistoryEntryViewModel : ViewModelBase
{
    public ShowHistoryEntryViewModel(
        string historyId,
        string showName,
        string showId,
        int week,
        int rating,
        int attendance,
        string summary,
        DateTime date)
    {
        HistoryId = historyId;
        ShowName = showName;
        ShowId = showId;
        Week = week;
        Rating = rating;
        Attendance = attendance;
        Summary = summary;
        Date = date;
    }

    public string HistoryId { get; }
    public string ShowName { get; }
    public string ShowId { get; }
    public int Week { get; }
    public int Rating { get; }
    public int Attendance { get; }
    public string Summary { get; }
    public DateTime Date { get; }

    public string RatingText => $"{Rating}/100";
    public string AttendanceText => $"{Attendance:N0} spectateurs";
    public string DateText => Date.ToString("dd/MM/yyyy");
    public string DisplayLine => $"Semaine {Week} • {RatingText} • {AttendanceText} • {Summary}";

    public string RatingClass => Rating switch
    {
        >= 90 => "Excellent",
        >= 80 => "Très bon",
        >= 70 => "Bon",
        >= 60 => "Moyen",
        _ => "Faible"
    };
}
