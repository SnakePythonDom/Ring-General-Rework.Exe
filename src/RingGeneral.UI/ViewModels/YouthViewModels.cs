using ReactiveUI;

namespace RingGeneral.UI.ViewModels;

public sealed class YouthStructureViewModel : ViewModelBase
{
    public YouthStructureViewModel(
        string youthId,
        string nom,
        string region,
        string type,
        int budgetAnnuel,
        int capaciteMax,
        int niveauEquipements,
        int qualiteCoaching,
        string philosophie,
        bool actif,
        int traineesActifs)
    {
        YouthId = youthId;
        Nom = nom;
        Region = region;
        Type = type;
        _budgetAnnuel = budgetAnnuel;
        CapaciteMax = capaciteMax;
        NiveauEquipements = niveauEquipements;
        QualiteCoaching = qualiteCoaching;
        Philosophie = philosophie;
        Actif = actif;
        TraineesActifs = traineesActifs;
    }

    public string YouthId { get; }
    public string Nom { get; }
    public string Region { get; }
    public string Type { get; }
    public int CapaciteMax { get; }
    public int NiveauEquipements { get; }
    public int QualiteCoaching { get; }
    public string Philosophie { get; }
    public bool Actif { get; }
    public int TraineesActifs { get; }

    public int BudgetAnnuel
    {
        get => _budgetAnnuel;
        set => this.RaiseAndSetIfChanged(ref _budgetAnnuel, value);
    }
    private int _budgetAnnuel;
}

public sealed record YouthTraineeViewModel(
    string WorkerId,
    string Nom,
    int InRing,
    int Entertainment,
    int Story,
    string Statut);

public sealed record YouthProgramViewModel(
    string ProgramId,
    string Nom,
    int DureeSemaines,
    string? Focus);

public sealed record YouthStaffAssignmentViewModel(
    int AssignmentId,
    string WorkerId,
    string Nom,
    string Role,
    int? SemaineDebut);
