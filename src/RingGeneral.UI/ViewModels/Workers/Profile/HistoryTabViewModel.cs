using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Workers.Profile;

/// <summary>
/// ViewModel for History tab.
/// Manages match history and title reigns.
/// </summary>
public sealed class HistoryTabViewModel : ViewModelBase
{
    private readonly INotesRepository _repository;

    private int _workerId;
    private ObservableCollection<MatchHistoryItem> _matchHistory = new();
    private ObservableCollection<TitleReign> _titleReigns = new();
    private MatchHistoryItem? _selectedMatch;
    private TitleReign? _selectedReign;
    private int _totalMatches;
    private int _totalWins;
    private int _totalLosses;
    private int _totalDraws;
    private double _winPercentage;

    public HistoryTabViewModel(INotesRepository repository)
    {
        _repository = repository;

        // Commands
        ViewMatchDetailsCommand = ReactiveCommand.Create(ViewMatchDetails);
        ViewReignDetailsCommand = ReactiveCommand.Create(ViewReignDetails);
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    public int WorkerId
    {
        get => _workerId;
        private set => this.RaiseAndSetIfChanged(ref _workerId, value);
    }

    /// <summary>
    /// Match history
    /// </summary>
    public ObservableCollection<MatchHistoryItem> MatchHistory
    {
        get => _matchHistory;
        private set => this.RaiseAndSetIfChanged(ref _matchHistory, value);
    }

    /// <summary>
    /// Title reigns (current and historical)
    /// </summary>
    public ObservableCollection<TitleReign> TitleReigns
    {
        get => _titleReigns;
        private set => this.RaiseAndSetIfChanged(ref _titleReigns, value);
    }

    /// <summary>
    /// Selected match
    /// </summary>
    public MatchHistoryItem? SelectedMatch
    {
        get => _selectedMatch;
        set => this.RaiseAndSetIfChanged(ref _selectedMatch, value);
    }

    /// <summary>
    /// Selected title reign
    /// </summary>
    public TitleReign? SelectedReign
    {
        get => _selectedReign;
        set => this.RaiseAndSetIfChanged(ref _selectedReign, value);
    }

    // ====================================================================
    // MATCH STATISTICS
    // ====================================================================

    public int TotalMatches
    {
        get => _totalMatches;
        private set => this.RaiseAndSetIfChanged(ref _totalMatches, value);
    }

    public int TotalWins
    {
        get => _totalWins;
        private set => this.RaiseAndSetIfChanged(ref _totalWins, value);
    }

    public int TotalLosses
    {
        get => _totalLosses;
        private set => this.RaiseAndSetIfChanged(ref _totalLosses, value);
    }

    public int TotalDraws
    {
        get => _totalDraws;
        private set => this.RaiseAndSetIfChanged(ref _totalDraws, value);
    }

    public double WinPercentage
    {
        get => _winPercentage;
        private set => this.RaiseAndSetIfChanged(ref _winPercentage, value);
    }

    /// <summary>
    /// Current title reigns (championships currently held)
    /// </summary>
    public ObservableCollection<TitleReign> CurrentTitleReigns =>
        new(TitleReigns.Where(r => r.IsCurrentChampion));

    /// <summary>
    /// Is worker currently a champion?
    /// </summary>
    public bool IsChampion => CurrentTitleReigns.Count > 0;

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> ViewMatchDetailsCommand { get; }
    public ReactiveCommand<Unit, Unit> ViewReignDetailsCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    public void LoadWorker(int workerId)
    {
        WorkerId = workerId;

        // Load match history
        var matches = _repository.GetMatchHistory(workerId);
        MatchHistory = new ObservableCollection<MatchHistoryItem>(matches);

        // Load match statistics
        var (totalMatches, wins, losses, draws, winPct) = _repository.GetMatchStats(workerId);
        TotalMatches = totalMatches;
        TotalWins = wins;
        TotalLosses = losses;
        TotalDraws = draws;
        WinPercentage = winPct;

        // Load title reigns
        var reigns = _repository.GetTitleReigns(workerId);
        TitleReigns = new ObservableCollection<TitleReign>(reigns);

        this.RaisePropertyChanged(nameof(CurrentTitleReigns));
        this.RaisePropertyChanged(nameof(IsChampion));
    }

    // ====================================================================
    // PRIVATE METHODS
    // ====================================================================

    private void ViewMatchDetails()
    {
        if (SelectedMatch == null) return;
        System.Console.WriteLine($"[HistoryTab] View match {SelectedMatch.Id}");
        // TODO: Show match details dialog
    }

    private void ViewReignDetails()
    {
        if (SelectedReign == null) return;
        System.Console.WriteLine($"[HistoryTab] View reign {SelectedReign.Id}");
        // TODO: Show reign details dialog
    }
}
