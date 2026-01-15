namespace RingGeneral.Core.Models;

public enum WorldSimLod
{
    Detail = 0,
    Resume = 1,
    Statistique = 2
}

public sealed record WorldSimSettings(
    int NbCompagniesLod0,
    int BudgetMsParTick,
    int FrequenceLod1Semaines,
    int FrequenceLod2Semaines,
    int Seed)
{
    public static WorldSimSettings ParDefaut => new(10, 120, 1, 4, 42);
}

public sealed record WorldSimCompanyPlan(
    string CompanyId,
    WorldSimLod NiveauDetail);

public sealed record WorldSimPlan(
    int Semaine,
    IReadOnlyList<WorldSimCompanyPlan> Compagnies);

public sealed record WorldSimCompanyImpact(
    string CompanyId,
    int DeltaPrestige,
    double DeltaTresorerie,
    string Resume);

public sealed record WorldSimTickResult(
    WorldSimPlan Plan,
    IReadOnlyList<WorldSimCompanyImpact> Impacts,
    TimeSpan TempsExecution,
    bool BudgetDepasse);
