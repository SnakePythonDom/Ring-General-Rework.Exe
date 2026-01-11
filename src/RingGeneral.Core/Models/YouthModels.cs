namespace RingGeneral.Core.Models;

public sealed record YouthSpec(
    YouthSpecMeta Meta,
    YouthStructureSpec Structures,
    YouthProgressionSpec Progression,
    YouthGraduationSpec Graduation);

public sealed record YouthSpecMeta(
    string Langue,
    string Modele,
    string? SourceDeVerite);

public sealed record YouthStructureSpec(
    IReadOnlyList<string> Types,
    YouthStructureInfrastructureSpec Infrastructure,
    YouthStructureBudgetSpec Budget,
    YouthStructureStaffSpec Staff,
    IReadOnlyDictionary<string, YouthPhilosophyFocus> Philosophie);

public sealed record YouthStructureInfrastructureSpec(
    int Min,
    int Max);

public sealed record YouthStructureBudgetSpec(
    int Min,
    int Max,
    IReadOnlyList<YouthStructureBudgetTier> Paliers);

public sealed record YouthStructureBudgetTier(
    int Min,
    int Max,
    double BonusChance);

public sealed record YouthStructureStaffSpec(
    IReadOnlyList<string> Roles,
    int MaxAffectationsParCoach);

public sealed record YouthPhilosophyFocus(
    double InRing,
    double Entertainment,
    double Story);

public sealed record YouthProgressionSpec(
    double ChanceBase,
    double BonusInfrastructureParNiveau,
    double BonusCoachingParPoint,
    int MaxGainParSemaine,
    int CapAttribut);

public sealed record YouthGraduationSpec(
    int MinSemaines,
    int SeuilMoyen,
    int SeuilInRing,
    int SeuilEntertainment,
    int SeuilStory);

public sealed record YouthTraineeProgressionState(
    string WorkerId,
    string Nom,
    string YouthId,
    string Philosophie,
    int NiveauEquipements,
    int BudgetAnnuel,
    int QualiteCoaching,
    string Statut,
    int SemaineInscription,
    int InRing,
    int Entertainment,
    int Story);

public sealed record YouthTraineeProgressionResult(
    string WorkerId,
    string YouthId,
    string Nom,
    int InRing,
    int Entertainment,
    int Story,
    int DeltaInRing,
    int DeltaEntertainment,
    int DeltaStory,
    bool Diplome);

public sealed record YouthProgressionReport(
    int Semaine,
    IReadOnlyList<YouthTraineeProgressionResult> Resultats);

public sealed record YouthTraineeInfo(
    string WorkerId,
    string Nom,
    string YouthId,
    int InRing,
    int Entertainment,
    int Story,
    string Statut);

public sealed record YouthProgramInfo(
    string ProgramId,
    string YouthId,
    string Nom,
    int DureeSemaines,
    string? Focus);

public sealed record YouthStaffAssignmentInfo(
    int AssignmentId,
    string YouthId,
    string WorkerId,
    string Nom,
    string Role,
    int? SemaineDebut);

/// <summary>
/// Represents a Youth Structure (Training Center/Dojo) in the persistent database.
/// Based on YouthStructureState blueprint but adapted for Entity usage.
/// </summary>
public class YouthStructure
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string YouthId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the structure
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Owner company ID
    /// </summary>
    public string CompanyId { get; set; } = string.Empty;

    /// <summary>
    /// Geographical region
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Structure type (Dojo, Performance Center, etc.)
    /// </summary>
    public string Type { get; set; } = "Dojo";

    /// <summary>
    /// Annual budget in currency
    /// </summary>
    public int BudgetAnnual { get; set; }

    /// <summary>
    /// Maximum trainee capacity
    /// </summary>
    public int MaxCapacity { get; set; }

    /// <summary>
    /// Equipment level (0-100)
    /// </summary>
    public int EquipmentLevel { get; set; }

    /// <summary>
    /// Coaching quality (0-100)
    /// </summary>
    public int CoachingQuality { get; set; }

    /// <summary>
    /// Training philosophy (Technical, Brawler, etc.)
    /// </summary>
    public string Philosophy { get; set; } = "Balanced";

    /// <summary>
    /// Is the structure currently active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Week number of the last graduation event
    /// </summary>
    public int? LastGraduationWeek { get; set; }

    /// <summary>
    /// Structure Level (1-5)
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// Number of active trainees
    /// </summary>
    public int ActiveTraineesCount { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
