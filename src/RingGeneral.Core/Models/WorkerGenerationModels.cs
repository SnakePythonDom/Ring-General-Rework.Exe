using System.Collections.Immutable;

namespace RingGeneral.Core.Models;

public enum YouthGenerationMode
{
    Desactivee,
    Realiste,
    Abondante
}

public enum WorldGenerationMode
{
    Desactivee,
    Faible
}

public sealed record WorkerGenerationOptions(
    YouthGenerationMode YouthMode,
    WorldGenerationMode WorldMode,
    int? SemainePivotAnnuelle);

public sealed record GameState(
    int Semaine,
    string CompagnieId,
    string Region,
    WorkerGenerationOptions Options,
    IReadOnlyList<YouthStructureState> YouthStructures,
    GenerationCounters Counters);

public sealed record YouthStructureState(
    string YouthId,
    string Nom,
    string CompagnieId,
    string Region,
    string Type,
    int BudgetAnnuel,
    int CapaciteMax,
    int NiveauEquipements,
    int QualiteCoaching,
    string Philosophie,
    bool Actif,
    int? DerniereGenerationSemaine,
    int TraineesActifs);

public sealed record GenerationCounters(
    int Annee,
    int GlobalTrainees,
    IReadOnlyDictionary<string, int> TraineesParPays,
    IReadOnlyDictionary<string, int> TraineesParCompagnie,
    int GlobalFreeAgents,
    IReadOnlyDictionary<string, int> FreeAgentsParPays)
{
    public int TraineesPourPays(string pays) => TraineesParPays.TryGetValue(pays, out var count) ? count : 0;
    public int TraineesPourCompagnie(string compagnieId) => TraineesParCompagnie.TryGetValue(compagnieId, out var count) ? count : 0;
    public int FreeAgentsPourPays(string pays) => FreeAgentsParPays.TryGetValue(pays, out var count) ? count : 0;
}

public sealed record GeneratedWorker(
    string WorkerId,
    string Prenom,
    string Nom,
    string TypeWorker,
    int Age,
    string Region,
    string? CompagnieId,
    string? YouthId,
    int InRing,
    int Entertainment,
    int Story,
    int Popularite,
    int Fatigue,
    string Blessure,
    int Momentum,
    string RoleTv,
    string Specialite,
    IReadOnlyDictionary<string, int> Attributes);

public sealed record WorkerGenerationNotice(
    string Type,
    string Titre,
    string Contenu,
    string? Lien);

public sealed record WorkerGenerationResultatStructure(
    string YouthId,
    string NomYouth,
    string Region,
    int NombreGeneres,
    string? Raison);

public sealed record WorkerGenerationResultatMonde(
    string Region,
    int NombreGeneres,
    string? Raison);

public sealed record WorkerGenerationReport(
    int Semaine,
    IReadOnlyList<GeneratedWorker> Workers,
    IReadOnlyList<WorkerGenerationResultatStructure> ResultatsStructures,
    IReadOnlyList<WorkerGenerationResultatMonde> ResultatsMonde,
    IReadOnlyList<WorkerGenerationNotice> Notices)
{
    public static WorkerGenerationReport Vide(int semaine)
        => new(
            semaine,
            ImmutableArray<GeneratedWorker>.Empty,
            ImmutableArray<WorkerGenerationResultatStructure>.Empty,
            ImmutableArray<WorkerGenerationResultatMonde>.Empty,
            ImmutableArray<WorkerGenerationNotice>.Empty);
}

public sealed record WorkerGenerationSpec(
    WorkerGenerationFrequencies Frequences,
    WorkerGenerationCaps Caps,
    WorkerGenerationFactors Facteurs,
    WorkerGenerationProfiles Profils,
    WorkerGenerationInitialValues ValeursInitiales,
    WorkerGenerationNames Noms,
    WorkerGenerationModes Modes,
    WorkerGenerationWorld Monde);

public sealed record WorkerGenerationFrequencies(
    WorkerGenerationAnnualFrequency Annuel,
    WorkerGenerationMonthlyFrequency Mensuel);

public sealed record WorkerGenerationAnnualFrequency(
    int SemainePivot,
    int CooldownSemaines);

public sealed record WorkerGenerationMonthlyFrequency(
    bool Actif,
    int CooldownSemaines,
    int IntervalleSemaines);

public sealed record WorkerGenerationCaps(
    WorkerGenerationCapsTrainees Trainees,
    WorkerGenerationCapsFreeAgents FreeAgents);

public sealed record WorkerGenerationCapsTrainees(
    int GlobalAnnuel,
    int ParPaysAnnuel,
    int ParCompagnieAnnuel,
    WorkerGenerationCapsStructure ParStructure);

public sealed record WorkerGenerationCapsStructure(
    int MaxParPeriode,
    int MaxActifs);

public sealed record WorkerGenerationCapsFreeAgents(
    int GlobalAnnuel,
    int ParPaysAnnuel);

public sealed record WorkerGenerationFactors(
    IReadOnlyDictionary<string, WorkerGenerationTypeFactor> TypeYouth,
    WorkerGenerationInfrastructureFactor Infrastructure,
    WorkerGenerationBudgetFactor Budget,
    WorkerGenerationCoachingFactor Coaching,
    IReadOnlyDictionary<string, WorkerGenerationPhilosophyFactor> Philosophie,
    IReadOnlyDictionary<string, WorkerGenerationRegionFactor> Regions);

public sealed record WorkerGenerationTypeFactor(
    double Base,
    int Max);

public sealed record WorkerGenerationInfrastructureFactor(
    double BonusParNiveau);

public sealed record WorkerGenerationBudgetFactor(
    IReadOnlyList<WorkerGenerationBudgetTier> Paliers);

public sealed record WorkerGenerationBudgetTier(
    int Min,
    int Max,
    double Bonus);

public sealed record WorkerGenerationCoachingFactor(
    double BonusParPoint);

public sealed record WorkerGenerationPhilosophyFactor(
    int? BonusAerial,
    int? BonusTechnique,
    int? BonusPromo,
    int? BonusGeneral);

public sealed record WorkerGenerationRegionFactor(
    int? BonusAerial,
    int? BonusTechnique,
    int? BonusPromo);

public sealed record WorkerGenerationProfiles(
    WorkerGenerationProfile Trainee,
    WorkerGenerationProfile FreeAgent);

public sealed record WorkerGenerationProfile(
    int AgeMin,
    int AgeMax,
    WorkerGenerationPotential Potentiel,
    IReadOnlyDictionary<string, double> Specialites);

public sealed record WorkerGenerationPotential(
    int Min,
    int Max,
    string Courbe);

public sealed record WorkerGenerationInitialValues(
    int Popularite,
    int Momentum,
    int Fatigue,
    string Blessure,
    string RoleTrainee,
    string RoleFreeAgent);

public sealed record WorkerGenerationNames(
    IReadOnlyList<string> Prenoms,
    IReadOnlyList<string> Noms);

public sealed record WorkerGenerationModes(
    WorkerGenerationModeSpec Desactive,
    WorkerGenerationModeSpec Realiste,
    WorkerGenerationModeSpec Abondant);

public sealed record WorkerGenerationModeSpec(
    double MultiplicateurQuantite,
    bool? Mensuel = null);

public sealed record WorkerGenerationWorld(
    WorkerGenerationWorldMode Faible);

public sealed record WorkerGenerationWorldMode(
    double ChanceHebdo,
    int MaxParPeriode);
