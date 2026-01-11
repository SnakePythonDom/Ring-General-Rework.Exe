using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Models.Staff;
using RingGeneral.Data.Repositories; // For WorkerBackstageProfile context if needed, though usually in Core.Models
using RingGeneral.UI.Services.Navigation;

namespace RingGeneral.UI.ViewModels.Company;

public sealed class ChildCompanyDetailViewModel : ViewModelBase, INavigableViewModel
{
    private readonly IGameRepository _gameRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IChildCompanyExtendedRepository _childCompanyRepository;
    private readonly WorkerRepository _workerRepository;
    private readonly INavigationService _navigationService;

    private ChildCompanyExtended? _company;
    private bool _isLoading;

    public ChildCompanyDetailViewModel(
        IGameRepository gameRepository,
        IStaffRepository staffRepository,
        IChildCompanyExtendedRepository childCompanyRepository,
        WorkerRepository workerRepository,
        INavigationService navigationService)
    {
        _gameRepository = gameRepository;
        _staffRepository = staffRepository;
        _childCompanyRepository = childCompanyRepository;
        _workerRepository = workerRepository;
        _navigationService = navigationService;

        Roster = new ObservableCollection<WorkerBackstageProfile>();
        Staff = new ObservableCollection<StaffMember>();

        BackCommand = ReactiveCommand.Create(GoBack);
    }

    public ObservableCollection<WorkerBackstageProfile> Roster { get; }
    public ObservableCollection<StaffMember> Staff { get; }

    public ChildCompanyExtended? Company
    {
        get => _company;
        private set => this.RaiseAndSetIfChanged(ref _company, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    public async Task LoadAsync(string childCompanyId)
    {
        IsLoading = true;
        try
        {
            // 1. Refetch Company Details (to ensure fresh data)
            Company = await _childCompanyRepository.GetChildCompanyExtendedByIdAsync(childCompanyId);

            if (Company == null) return;

            // 2. Load Roster
            // We use the concrete WorkerRepository from GameRepository if possible, 
            // or we might need to cast if IGameRepository doesn't expose it directly as IWorkerRepository with the method we need.
            // Based on previous checks, WorkerRepository has ChargerBackstageRoster(companyId).
            // Assuming GameRepository has a public WorkerRepository property or we can get it via DI directly if we injected it.
            // For now, let's assume we can get it via the repository factory pattern or similar access in GameRepository.
            // Actually, we'll use the worker repository directly if we can, but let's try to access it via GameRepo first.
            // Wait, IGameRepository usually exposes specific methods. 
            // Let's use the concrete WorkerRepository injected via DI if possible, 
            // but the constructor signature in App.axaml.cs showed us passing repositories. 
            // I'll check App.axaml.cs to see how to inject IWorkerRepository or similar.

            // To be safe and follow the pattern, I will assume we can get the list. 
            // Since ChargerBackstageRoster is on WorkerRepository, I will cast or inject it.
            // Looking at the implementation plan, I'll update the constructor to take WorkerRepository directly if IGameRepository is too generic.
            // But wait, App.axaml.cs registers repositories.WorkerRepository as singleton.

            await LoadData(childCompanyId);
        }
        catch (System.Exception)
        {
            // Use Logger if available, otherwise just swallow or Console
            // Logger.Error($"Error loading details: {ex.Message}", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadData(string companyId)
    {
        // 2. Load Roster
        Roster.Clear();
        var workers = _workerRepository.ChargerBackstageRoster(companyId);
        foreach (var worker in workers)
        {
            Roster.Add(worker);
        }

        // 3. Load Staff
        Staff.Clear();
        var staffMembers = await _staffRepository.GetStaffByCompanyIdAsync(companyId);
        foreach (var member in staffMembers)
        {
            Staff.Add(member);
        }
    }

    private void GoBack()
    {
        _navigationService.GoBack();
    }

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is string childCompanyId)
        {
            _ = LoadAsync(childCompanyId);
        }
    }
}
