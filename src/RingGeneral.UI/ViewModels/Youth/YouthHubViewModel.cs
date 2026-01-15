using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using RingGeneral.Core.Interfaces;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Services;


namespace RingGeneral.UI.ViewModels.Youth;

public enum YouthViewMode
{
    List,
    Detail
}

public sealed class YouthHubViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;
    private readonly IRegionRepository _regionRepository;

    private StructureManagementViewModel _structuresVM;
    private LoanManagementViewModel _loansVM;
    private YouthTraineeManagementViewModel _traineesVM;
    private YouthStaffManagementViewModel _staffVM;

    private YouthViewMode _currentMode = YouthViewMode.List;
    public YouthViewMode CurrentMode
    {
        get => _currentMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentMode, value);
            this.RaisePropertyChanged(nameof(IsListMode));
            this.RaisePropertyChanged(nameof(IsDetailMode));
        }
    }

    public bool IsListMode => CurrentMode == YouthViewMode.List;
    public bool IsDetailMode => CurrentMode == YouthViewMode.Detail;

    private YouthDetailViewModel? _detailVM;
    public YouthDetailViewModel? DetailVM
    {
        get => _detailVM;
        private set => this.RaiseAndSetIfChanged(ref _detailVM, value);
    }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }

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

    // Profile Overlay
    private Common.WorkerProfileViewModel? _profileVM;
    public Common.WorkerProfileViewModel? ProfileVM
    {
        get => _profileVM;
        set
        {
            this.RaiseAndSetIfChanged(ref _profileVM, value);
            this.RaisePropertyChanged(nameof(IsProfileOpen));
        }
    }

    public bool IsProfileOpen => ProfileVM != null;

    public void OpenProfile(string workerId)
    {
        System.Diagnostics.Debug.WriteLine($"[YouthHubViewModel] Request to open profile for ID: '{workerId}'");

        // Use new String ID overload
        var worker = _gameRepository.GetWorker(workerId);
        if (worker != null)
        {
            var names = _gameRepository.ChargerNomsWorkers();
            var vm = new Common.WorkerProfileViewModel(worker, _gameRepository, names);

            // Debug logs
            System.Diagnostics.Debug.WriteLine($"[YouthHubViewModel] Profile VM created for {worker.Name}. CloseCommand subscribed.");

            vm.CloseCommand.Subscribe(_ =>
            {
                ProfileVM = null;
                // IsProfileOpen = false; // This is handled by RaiseAndSetIfChanged in ProfileVM setter
            });

            ProfileVM = vm;
            // IsProfileOpen = true; // This is handled by RaiseAndSetIfChanged in ProfileVM setter
            // this.RaisePropertyChanged(nameof(IsProfileOpen)); // This is handled by RaiseAndSetIfChanged in ProfileVM setter
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[YouthHubViewModel] Worker not found for ID: {workerId}");
        }
    }

    public YouthHubViewModel(
        GameRepository gameRepository,
        YouthRepository youthRepository,
        IRegionRepository regionRepository,
        IWorkerGenerationService generationService)
    {
        try
        {
            _gameRepository = gameRepository;
            _youthRepository = youthRepository;
            _regionRepository = regionRepository;

            // Initialize sub-VMs
            _structuresVM = new StructureManagementViewModel(_youthRepository, _gameRepository, _regionRepository, generationService);
            _loansVM = new LoanManagementViewModel(_gameRepository, _youthRepository);

            // Pass OpenProfile action to children
            // Note: Since Trainees are also in Detail view, we need to pass it there too.
            // For the global Trainee tab (if used):
            _traineesVM = new YouthTraineeManagementViewModel(_youthRepository, _gameRepository, null, OpenProfile);

            _staffVM = new YouthStaffManagementViewModel(_youthRepository, _gameRepository);

            BackCommand = ReactiveCommand.Create(() =>
            {
                CurrentMode = YouthViewMode.List;
                DetailVM = null;
            });

            _structuresVM.SelectStructureCommand.Subscribe(structure =>
            {
                if (structure != null)
                {
                    // Pass OpenProfile to DetailVM
                    DetailVM = new YouthDetailViewModel(structure.YouthId, structure, _youthRepository, _gameRepository, BackCommand, OpenProfile);
                    CurrentMode = YouthViewMode.Detail;
                }
            });
        }
        catch (Exception ex)
        {
            ApplicationServices.Logger.Error($"CRITICAL: Error initializing YouthHubViewModel: {ex.Message}", ex);
            // Re-throw to ensure the crash is still visible but now we have logs
            throw;
        }
    }
}
