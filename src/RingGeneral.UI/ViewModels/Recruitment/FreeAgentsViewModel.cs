using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Recruitment;
using RingGeneral.UI.Services.Messaging;

namespace RingGeneral.UI.ViewModels.Recruitment;

public class RecruitAgentEvent
{
    public FreeAgentCandidate Agent { get; }
    public RecruitAgentEvent(FreeAgentCandidate agent) => Agent = agent;
}

public sealed class FreeAgentsViewModel : ViewModelBase
{
    private readonly IFreeAgentRepository _repository;
    private readonly IGameRepository _gameRepository;
    private readonly IRecruitmentScoringService _scoringService;
    private readonly IEventAggregator _eventAggregator;
    private readonly string _companyId;

    private FreeAgentCandidate? _selectedAgent;
    private string _searchText = string.Empty;
    private bool _showWrestlers = true;
    private bool _showStaff = true;
    private string _selectedRegion = "All";

    public FreeAgentsViewModel(
        string companyId,
        IFreeAgentRepository repository,
        IGameRepository gameRepository,
        IRecruitmentScoringService scoringService,
        IEventAggregator eventAggregator)
    {
        _companyId = companyId;
        _repository = repository;
        _gameRepository = gameRepository;
        _scoringService = scoringService;
        _eventAggregator = eventAggregator;

        FreeAgents = new ObservableCollection<FreeAgentCandidate>();
        Regions = new ObservableCollection<string> { "All", "North America", "Europe", "Japan", "Mexico", "United Kingdom" };

        RefreshCommand = ReactiveCommand.CreateFromTask(LoadMarketAsync);
        RecruitCommand = ReactiveCommand.CreateFromTask<FreeAgentCandidate>(RecruitAgentAsync);

        // Auto-refresh when filters change
        this.WhenAnyValue(x => x.SearchText, x => x.ShowWrestlers, x => x.ShowStaff, x => x.SelectedRegion)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ => await LoadMarketAsync());
    }

    public ObservableCollection<FreeAgentCandidate> FreeAgents { get; }
    public ObservableCollection<string> Regions { get; }

    public FreeAgentCandidate? SelectedAgent
    {
        get => _selectedAgent;
        set => this.RaiseAndSetIfChanged(ref _selectedAgent, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public bool ShowWrestlers
    {
        get => _showWrestlers;
        set => this.RaiseAndSetIfChanged(ref _showWrestlers, value);
    }

    public bool ShowStaff
    {
        get => _showStaff;
        set => this.RaiseAndSetIfChanged(ref _showStaff, value);
    }

    public string SelectedRegion
    {
        get => _selectedRegion;
        set => this.RaiseAndSetIfChanged(ref _selectedRegion, value);
    }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<FreeAgentCandidate, Unit> RecruitCommand { get; }

    public async Task LoadMarketAsync()
    {
        try
        {
            var filter = new FreeAgentFilter
            {
                SearchText = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                Type = (_showWrestlers && _showStaff) ? null :
                       _showWrestlers ? FreeAgentType.Wrestler :
                       _showStaff ? FreeAgentType.Staff : (FreeAgentType?)null,
                Region = _selectedRegion == "All" ? null : _selectedRegion
            };

            var data = await _repository.GetFreeAgentMarketAsync(filter);

            FreeAgents.Clear();
            foreach (var agent in data)
            {
                var worker = _gameRepository.GetWorker(agent.Id);
                if (worker != null)
                {
                    agent.GeoFitScore = _scoringService.GetGeoFit(_companyId, worker);
                    agent.StrategicFitScore = _scoringService.GetStrategicFit(_companyId, worker);
                }

                FreeAgents.Add(agent);
            }
        }
        catch (Exception ex)
        {
            // Simple logging
            System.Diagnostics.Debug.WriteLine($"Error loading agent market: {ex.Message}");
        }
    }

    private async Task RecruitAgentAsync(FreeAgentCandidate agent)
    {
        // Simple logic: publish event to open dialog
        _eventAggregator.Publish(new RecruitAgentEvent(agent));
        await Task.CompletedTask;
    }
}
