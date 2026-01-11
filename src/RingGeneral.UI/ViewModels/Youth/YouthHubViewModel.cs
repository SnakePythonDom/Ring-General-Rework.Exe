using ReactiveUI;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Youth;

public sealed class YouthHubViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;

    private StructureManagementViewModel _structuresVM;
    private LoanManagementViewModel _loansVM;
    private YouthTraineeManagementViewModel _traineesVM;
    private YouthStaffManagementViewModel _staffVM;

    public StructureManagementViewModel StructuresVM
    {
        get => _structuresVM;
        set => this.RaiseAndSetIfChanged(ref _structuresVM, value);
    }

    public LoanManagementViewModel LoansVM
    {
        get => _loansVM;
        set => this.RaiseAndSetIfChanged(ref _loansVM, value);
    }

    public YouthTraineeManagementViewModel TraineesVM
    {
        get => _traineesVM;
        set => this.RaiseAndSetIfChanged(ref _traineesVM, value);
    }

    public YouthStaffManagementViewModel StaffVM
    {
        get => _staffVM;
        set => this.RaiseAndSetIfChanged(ref _staffVM, value);
    }

    // TODO: Add Staff and Trainee VMs

    public YouthHubViewModel(GameRepository gameRepository, YouthRepository youthRepository)
    {
        _gameRepository = gameRepository;
        _youthRepository = youthRepository;

        // Initialize sub-VMs
        _structuresVM = new StructureManagementViewModel(_youthRepository, _gameRepository);
        _loansVM = new LoanManagementViewModel(_gameRepository, _youthRepository);
        _traineesVM = new YouthTraineeManagementViewModel(_youthRepository, _gameRepository);
        _staffVM = new YouthStaffManagementViewModel(_youthRepository, _gameRepository);
    }
}
