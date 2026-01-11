using ReactiveUI;
using RingGeneral.Core.Models.Staff;

namespace RingGeneral.UI.ViewModels.CompanyHub;

public class StaffItemViewModel : ViewModelBase
{
    public string StaffId { get; }
    public string Name { get; }
    public string Role { get; }
    public string Department { get; }
    public string Specialty { get; }
    public int SkillScore { get; }
    public int CompatibilityScore { get; }
    public string ExtraInfo { get; }
    
    public object RawStaff { get; }
    public StaffMember BaseStaff { get; }

    public StaffItemViewModel(StaffMember baseStaff, object rawStaff)
    {
        BaseStaff = baseStaff;
        RawStaff = rawStaff;
        
        StaffId = baseStaff.StaffId;
        Name = baseStaff.Name;
        Role = baseStaff.Role.ToString();
        Department = baseStaff.Department.ToString();
        SkillScore = baseStaff.SkillScore;
        
        if (rawStaff is CreativeStaff creative)
        {
            Specialty = creative.PreferredStyle.ToString();
            CompatibilityScore = creative.BookerCompatibilityScore;
            ExtraInfo = $"Style: {creative.PreferredStyle}";
        }
        else if (rawStaff is StructuralStaff structural)
        {
            Specialty = structural.ExpertiseDomain;
            CompatibilityScore = structural.EfficiencyScore;
            ExtraInfo = $"Expertise: {structural.ExpertiseDomain}";
        }
        else if (rawStaff is Trainer trainer)
        {
            Specialty = trainer.TrainingSpecialization;
            CompatibilityScore = trainer.TrainingEfficiency;
            ExtraInfo = $"Spé: {trainer.TrainingSpecialization}, Élèves: {trainer.CurrentStudents}/{trainer.MaxStudentCapacity}";
        }
        else
        {
            Specialty = "Général";
            CompatibilityScore = 50;
            ExtraInfo = "";
        }
    }
}
