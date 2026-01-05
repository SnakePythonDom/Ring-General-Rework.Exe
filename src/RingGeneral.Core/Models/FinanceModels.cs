namespace RingGeneral.Core.Models;

public enum PayrollFrequency
{
    Hebdomadaire,
    Mensuelle
}

public sealed record ContractPayroll(
    string WorkerId,
    string Nom,
    double Salaire,
    PayrollFrequency Frequence);

public sealed record ShowFinanceContext(
    CompanyState Compagnie,
    int Audience,
    int DureeMinutes,
    IReadOnlyList<int> PopularitesWorkers,
    bool DiffuseTv);

public sealed record WeeklyFinanceContext(
    string CompagnieId,
    int Semaine,
    double Tresorerie,
    IReadOnlyList<ContractPayroll> Contrats);

public sealed record TicketingSettings(
    double PrixBase,
    double PrixParAudience,
    double PrixParPrestige,
    double PrixMin,
    double PrixMax,
    double TauxRemplissageBase,
    double TauxRemplissageParPoint,
    double TauxRemplissageMin,
    double TauxRemplissageMax);

public sealed record VenueSettings(
    int CapaciteBase,
    int CapaciteMin,
    int CapaciteMax,
    int CapaciteParReach,
    int CapaciteParPrestige);

public sealed record MerchSettings(
    double DepenseParFan,
    double MultiplicateurStars,
    int StarsPrisesEnCompte);

public sealed record TvSettings(
    double RevenuBase,
    double RevenuParAudience);

public sealed record ProductionSettings(
    double CoutBase,
    double CoutParMinute,
    double CoutParSpectateur);

public sealed record PayrollSettings(
    int SemainesParMois);

public sealed record FinanceSettings(
    TicketingSettings Billetterie,
    VenueSettings Venue,
    MerchSettings Merch,
    TvSettings Tv,
    ProductionSettings Production,
    PayrollSettings Paie)
{
    public static FinanceSettings V1() => new(
        new TicketingSettings(
            PrixBase: 18,
            PrixParAudience: 0.22,
            PrixParPrestige: 0.15,
            PrixMin: 8,
            PrixMax: 45,
            TauxRemplissageBase: 0.55,
            TauxRemplissageParPoint: 0.006,
            TauxRemplissageMin: 0.3,
            TauxRemplissageMax: 0.95),
        new VenueSettings(
            CapaciteBase: 900,
            CapaciteMin: 400,
            CapaciteMax: 9000,
            CapaciteParReach: 120,
            CapaciteParPrestige: 18),
        new MerchSettings(
            DepenseParFan: 6.5,
            MultiplicateurStars: 0.6,
            StarsPrisesEnCompte: 3),
        new TvSettings(
            RevenuBase: 3200,
            RevenuParAudience: 35),
        new ProductionSettings(
            CoutBase: 1200,
            CoutParMinute: 35,
            CoutParSpectateur: 1.1),
        new PayrollSettings(
            SemainesParMois: 4));
}

public sealed record FinanceShowResult(
    double Billetterie,
    double Merch,
    double Tv,
    double CoutProduction,
    IReadOnlyList<FinanceTransaction> Transactions);

public sealed record FinanceTickResult(
    double TotalRevenus,
    double TotalDepenses,
    IReadOnlyList<FinanceTransaction> Transactions);
