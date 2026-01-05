using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Models;

public sealed record YouthSpec(
    YouthStructureSpec Structures,
    YouthProgressionSpec Progression,
    YouthGraduationSpec Graduation);

public sealed record YouthStructureSpec(
    YouthStructureDefaults Defaults,
    IReadOnlyDictionary<string, YouthStructureDefaults> Types);

public sealed record YouthStructureDefaults(
    int BudgetAnnuel,
    int CapaciteMax,
    int NiveauInfrastructure,
    int QualiteStaff,
    string Philosophie);

public sealed record YouthProgressionSpec(
    double GainBase,
    double BonusInfrastructureParNiveau,
    double BonusCoachingParPoint,
    IReadOnlyList<YouthProgressionBudgetTier> BonusBudget,
    double GainMax,
    IReadOnlyDictionary<string, double> Distribution,
    IReadOnlyDictionary<string, YouthProgressionPhilosophyBonus> Philosophie);

public sealed record YouthProgressionBudgetTier(
    int Min,
    int Max,
    double Bonus);

public sealed record YouthProgressionPhilosophyBonus(
    double InRing,
    double Entertainment,
    double Story);

public sealed record YouthGraduationSpec(
    int SeuilGlobal,
    int SeuilInRing,
    int SeuilEntertainment,
    int SeuilStory);

public sealed record YouthTraineeProgressionState(
    string WorkerId,
    string YouthId,
    int InRing,
    int Entertainment,
    int Story,
    string Statut);

public sealed record YouthProgressionUpdate(
    string WorkerId,
    string YouthId,
    int NouveauInRing,
    int NouveauEntertainment,
    int NouveauStory,
    int DeltaInRing,
    int DeltaEntertainment,
    int DeltaStory,
    bool EstGradue);

public sealed record YouthProgressionReport(
    int Semaine,
    IReadOnlyList<YouthProgressionUpdate> Updates)
{
    public IReadOnlyList<YouthProgressionUpdate> Graduations => Updates.Where(update => update.EstGradue).ToList();
}
