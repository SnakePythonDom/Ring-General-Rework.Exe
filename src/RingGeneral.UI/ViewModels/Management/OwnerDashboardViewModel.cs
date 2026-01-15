using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Owner;

namespace RingGeneral.UI.ViewModels.Management;

public class OwnerDashboardViewModel : ViewModelBase
{
    private readonly IOwnerRepository _ownerRepository;
    private readonly IGameRepository _gameRepository;
    private readonly string _companyId;
    private string _ownerMood = "Neutre";

    public OwnerDashboardViewModel(
        IOwnerRepository ownerRepository,
        IGameRepository gameRepository,
        string companyId)
    {
        _ownerRepository = ownerRepository;
        _gameRepository = gameRepository;
        _companyId = companyId;

        ActiveGoals = new ObservableCollection<OwnerGoal>();
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadGoalsAsync);
    }

    public ObservableCollection<OwnerGoal> ActiveGoals { get; }

    public string OwnerMood
    {
        get => _ownerMood;
        set => this.RaiseAndSetIfChanged(ref _ownerMood, value);
    }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    public async Task LoadGoalsAsync()
    {
        ActiveGoals.Clear();

        // Check if player controls this company
        // Assuming GameRepository or CompanyState has this info readily available or we check via OwnerId
        // The user says "Player is the Owner". So if IsPlayerControlled, there is no "Owner" entity above them.

        // We need to fetch CompanyState to check IsPlayerControlled
        // Assuming we can get it via _gameRepository or similar.
        // For now, let's assume we can check if ownerId is null or specific value.

        var ownerId = _gameRepository.ObtenirOwnerId(_companyId);

        if (string.IsNullOrEmpty(ownerId))
        {
            OwnerMood = "Vous êtes le Propriétaire";
            return;
        }

        var goals = await _ownerRepository.GetGoalsAsync(ownerId);
        foreach (var goal in goals)
        {
            ActiveGoals.Add(goal);
        }

        // Simple mood logic based on failed/met goals
        // In a real implementation this would be more complex
        OwnerMood = ActiveGoals.Any() ? "Satisfait" : "Aucune Directive";
    }
}
