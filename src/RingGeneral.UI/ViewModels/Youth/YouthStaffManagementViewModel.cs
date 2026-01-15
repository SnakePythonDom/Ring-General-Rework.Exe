using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Youth;

public sealed class YouthStaffManagementViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;
    private readonly string? _structureId;

    public YouthStaffItemViewModel? HeadCoach { get; private set; }
    public ObservableCollection<YouthStaffItemViewModel> SupportStaff { get; } = new();

    public ReactiveCommand<Unit, Unit> HireStaffCommand { get; }
    public ReactiveCommand<Unit, Unit> AutoAssignCommand { get; }

    private readonly Action<string>? _openProfile;

    public YouthStaffManagementViewModel(YouthRepository youthRepository, GameRepository gameRepository, string? structureId = null, Action<string>? openProfile = null)
    {
        _youthRepository = youthRepository;
        _gameRepository = gameRepository;
        _structureId = structureId;
        _openProfile = openProfile;

        HireStaffCommand = ReactiveCommand.Create(() => { /* TODO: Launch hire dialog */ });
        AutoAssignCommand = ReactiveCommand.Create(() => { /* TODO: Auto assign logic */ });

        try
        {
            LoadAssignments();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading staff assignments: {ex.Message}");
        }
    }

    public void LoadAssignments()
    {
        SupportStaff.Clear();
        HeadCoach = null;

        IEnumerable<YouthStructureState> structures;
        if (!string.IsNullOrEmpty(_structureId))
        {
            var structure = _youthRepository.ChargerYouthStructures().FirstOrDefault(s => s.YouthId == _structureId);
            structures = structure != null ? new List<YouthStructureState> { structure } : new List<YouthStructureState>();
        }
        else
        {
            structures = _youthRepository.ChargerYouthStructures();
        }

        foreach (var s in structures)
        {
            var staff = _youthRepository.ChargerYouthStaffAssignments(s.YouthId);
            foreach (var assignment in staff)
            {
                var vm = new YouthStaffItemViewModel(assignment, s.Nom, _openProfile);

                if (assignment.Role == "ENTRAINEUR_CHEF")
                {
                    HeadCoach = vm;
                }
                else
                {
                    SupportStaff.Add(vm);
                }
            }
        }

        this.RaisePropertyChanged(nameof(HeadCoach));
    }
}

public sealed class YouthStaffItemViewModel : ViewModelBase
{
    public string WorkerId { get; }
    public string Name { get; }
    public string StructureName { get; }
    public string Role { get; }
    public int? SemaineDebut { get; }

    // Mock data for UI alignment
    public string Speciality => "Technical";
    public int Teaching => 85;
    public int Motivation => 60;
    public int Admin => 40;
    public string WageDisplay => "2,200 €";
    public string SkillGrade => "78 (B)";

    public ReactiveCommand<Unit, Unit> ViewProfileCommand { get; }

    public YouthStaffItemViewModel(YouthStaffAssignmentInfo info, string structureName, Action<string>? openProfile = null)
    {
        WorkerId = info.WorkerId;
        Name = info.Nom;
        StructureName = structureName;
        Role = info.Role;
        SemaineDebut = info.SemaineDebut;

        ViewProfileCommand = ReactiveCommand.Create(() =>
        {
            openProfile?.Invoke(WorkerId);
        });
    }
}
