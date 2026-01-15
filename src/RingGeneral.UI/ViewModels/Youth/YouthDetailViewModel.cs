using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Data.Repositories;
using System.Reactive;
using System.Threading.Tasks;

namespace RingGeneral.UI.ViewModels.Youth;

public sealed class YouthDetailViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;
    private readonly string _structureId;

    private YouthStructureItemViewModel _structure;
    public YouthStructureItemViewModel Structure
    {
        get => _structure;
        set => this.RaiseAndSetIfChanged(ref _structure, value);
    }

    public YouthStaffManagementViewModel StaffVM { get; }
    public YouthTraineeManagementViewModel TraineesVM { get; }
    public YouthAlumniViewModel AlumniVM { get; }
    public YouthFinancialsViewModel FinancialsVM { get; }

    public ObservableCollection<string> Alerts { get; } = new();

    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    public YouthDetailViewModel(
        string structureId,
        YouthStructureItemViewModel structureItem,
        YouthRepository youthRepository,
        GameRepository gameRepository,
        ReactiveCommand<Unit, Unit> backCommand,
        Action<string> openProfile)
    {
        _structureId = structureId;
        _structure = structureItem;
        _youthRepository = youthRepository;
        _gameRepository = gameRepository;
        BackCommand = backCommand;

        // Initialize sub-VMs scoped to this structure
        StaffVM = new YouthStaffManagementViewModel(_youthRepository, _gameRepository, _structureId, openProfile);
        TraineesVM = new YouthTraineeManagementViewModel(_youthRepository, _gameRepository, _structureId, openProfile);
        AlumniVM = new YouthAlumniViewModel(_youthRepository, _structureId);
        FinancialsVM = new YouthFinancialsViewModel(_youthRepository, _structureId);

        // Dummy Alerts for mockup
        Alerts.Add("[!] 2 Trainees are ready for graduation (Skill > 70)");
        Alerts.Add("[i] New equipment \"Wrestling Mats\" installed yesterday");
        Alerts.Add("[i] Monthly budget deducted: -12,500 €");
    }

    private void LoadData()
    {
        // Data loading is handled by sub-VMs upon instantiation
    }
}
