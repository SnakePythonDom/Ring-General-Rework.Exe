using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Youth;

public sealed class YouthStaffManagementViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;

    public ObservableCollection<YouthStaffItemViewModel> Assignments { get; } = new();

    public YouthStaffManagementViewModel(YouthRepository youthRepository, GameRepository gameRepository)
    {
        _youthRepository = youthRepository;
        _gameRepository = gameRepository;
        LoadAssignments();
    }

    public void LoadAssignments()
    {
        Assignments.Clear();
        var structures = _youthRepository.ChargerYouthStructures();

        foreach (var s in structures)
        {
            var staff = _youthRepository.ChargerYouthStaffAssignments(s.YouthId);
            foreach (var assignment in staff)
            {
                Assignments.Add(new YouthStaffItemViewModel(assignment, s.Nom));
            }
        }
    }
}

public sealed class YouthStaffItemViewModel : ViewModelBase
{
    public string WorkerId { get; }
    public string Name { get; }
    public string StructureName { get; }
    public string Role { get; }
    public int? SemaineDebut { get; }

    public YouthStaffItemViewModel(YouthStaffAssignmentInfo info, string structureName)
    {
        WorkerId = info.WorkerId;
        Name = info.Nom;
        StructureName = structureName;
        Role = info.Role;
        SemaineDebut = info.SemaineDebut;
    }
}
