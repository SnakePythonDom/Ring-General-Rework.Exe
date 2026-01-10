using System.Collections.ObjectModel;

namespace RingGeneral.Core.Models;

/// <summary>
/// Paramètres de configuration pour le système financier
/// </summary>
public sealed class FinanceSettings
{
    public required VenueSettings Venue { get; set; }
    public required BilletterieSettings Billetterie { get; set; }
    public required MerchSettings Merch { get; set; }
    public required TvSettings Tv { get; set; }
    public required ProductionSettings Production { get; set; }
    public required PaieSettings Paie { get; set; }
}

/// <summary>
/// Paramètres pour les venues (salles)
/// </summary>
public sealed class VenueSettings
{
    public int CapaciteBase { get; set; }
    public int CapaciteParReach { get; set; }
    public int CapaciteParPrestige { get; set; }
    public int CapaciteMin { get; set; }
    public int CapaciteMax { get; set; }
}

/// <summary>
/// Paramètres pour la billetterie
/// </summary>
public sealed class BilletterieSettings
{
    public double TauxRemplissageBase { get; set; }
    public double TauxRemplissageParPoint { get; set; }
    public double TauxRemplissageMin { get; set; }
    public double TauxRemplissageMax { get; set; }
    public double PrixBase { get; set; }
    public double PrixParAudience { get; set; }
    public double PrixParPrestige { get; set; }
    public double PrixMin { get; set; }
    public double PrixMax { get; set; }
}

/// <summary>
/// Paramètres pour le merchandising
/// </summary>
public sealed class MerchSettings
{
    public double DepenseParFan { get; set; }
    public double MultiplicateurStars { get; set; }
    public int StarsPrisesEnCompte { get; set; }
}

/// <summary>
/// Paramètres pour les droits TV
/// </summary>
public sealed class TvSettings
{
    public double RevenuBase { get; set; }
    public double RevenuParAudience { get; set; }
}

/// <summary>
/// Paramètres pour les coûts de production
/// </summary>
public sealed class ProductionSettings
{
    public double CoutBase { get; set; }
    public double CoutParMinute { get; set; }
    public double CoutParSpectateur { get; set; }
}

/// <summary>
/// Paramètres pour les paies
/// </summary>
public sealed class PaieSettings
{
    public int SemainesParMois { get; set; }
}

/// <summary>
/// Contexte pour le calcul des finances d'un show
/// </summary>
public sealed class ShowFinanceContext
{
    public required CompanyState Compagnie { get; set; }
    public int Audience { get; set; }
    public int DureeMinutes { get; set; }
    public required IReadOnlyList<int> PopularitesWorkers { get; set; }
    public bool DiffuseTv { get; set; }
}

/// <summary>
/// Résultat du calcul des finances d'un show
/// </summary>
public sealed class FinanceShowResult
{
    public decimal Billetterie { get; }
    public decimal Merch { get; }
    public decimal Tv { get; }
    public decimal CoutsProduction { get; }
    public IReadOnlyList<FinanceTransactionModel> Transactions { get; }

    public FinanceShowResult(decimal billetterie, decimal merch, decimal tv, decimal coutsProduction, IReadOnlyList<FinanceTransactionModel> transactions)
    {
        Billetterie = billetterie;
        Merch = merch;
        Tv = tv;
        CoutsProduction = coutsProduction;
        Transactions = transactions;
    }
}

/// <summary>
/// Contexte pour le calcul des finances hebdomadaires
/// </summary>
public sealed class WeeklyFinanceContext
{
    public int Semaine { get; set; }
    public IReadOnlyList<ContractPayroll> Contrats { get; set; }

    public WeeklyFinanceContext(int semaine, IReadOnlyList<ContractPayroll> contrats)
    {
        Semaine = semaine;
        Contrats = contrats;
    }
}

/// <summary>
/// Résultat du calcul des finances hebdomadaires
/// </summary>
public sealed class FinanceTickResult
{
    public decimal Revenus { get; }
    public decimal Depenses { get; }
    public IReadOnlyList<FinanceTransactionModel> Transactions { get; }

    public FinanceTickResult(decimal revenus, decimal depenses, IReadOnlyList<FinanceTransactionModel> transactions)
    {
        Revenus = revenus;
        Depenses = depenses;
        Transactions = transactions;
    }
}

/// <summary>
/// Transaction financière pour le moteur de finance
/// </summary>
public sealed class FinanceTransactionModel
{
    public string Id { get; }
    public decimal Amount { get; }
    public string Description { get; }

    public FinanceTransactionModel(string id, decimal amount, string description)
    {
        Id = id;
        Amount = amount;
        Description = description;
    }
}

/// <summary>
/// Contrat de paie pour un worker
/// </summary>
public sealed class ContractPayroll
{
    public required PayrollFrequency Frequence { get; set; }
    public decimal Salaire { get; set; }
}

/// <summary>
/// Fréquence de paiement
/// </summary>
public enum PayrollFrequency
{
    Hebdomadaire,
    Mensuelle
}

/// <summary>
/// Allocation budgétaire par département
/// </summary>
public sealed class BudgetAllocation
{
    public string CompanyId { get; }
    public decimal TotalBudget { get; }
    public decimal TalentAllocation { get; }
    public decimal ProductionAllocation { get; }
    public decimal YouthDevAllocation { get; }
    public decimal MarketingAllocation { get; }
    public decimal MedicalAllocation { get; }

    public BudgetAllocation(string companyId, decimal totalBudget, decimal talentAllocation, decimal productionAllocation, decimal youthDevAllocation, decimal marketingAllocation, decimal medicalAllocation)
    {
        CompanyId = companyId;
        TotalBudget = totalBudget;
        TalentAllocation = talentAllocation;
        ProductionAllocation = productionAllocation;
        YouthDevAllocation = youthDevAllocation;
        MarketingAllocation = marketingAllocation;
        MedicalAllocation = medicalAllocation;
    }
}

/// <summary>
/// Impact d'une allocation budgétaire
/// </summary>
public sealed class AllocationImpact
{
    public string Department { get; }
    public decimal Amount { get; }
    public string Description { get; }
    public double Effect { get; }

    public AllocationImpact(string department, decimal amount, string description, double effect)
    {
        Department = department;
        Amount = amount;
        Description = description;
        Effect = effect;
    }
}

/// <summary>
/// Dette de compagnie
/// </summary>
public sealed record CompanyDebt(
    string DebtId,
    string CompanyId,
    decimal PrincipalAmount,
    decimal InterestRate,
    int TermMonths,
    DateTime StartDate,
    decimal MonthlyPayment,
    decimal RemainingBalance);

/// <summary>
/// Projection de revenus sur 12 mois
/// </summary>
public sealed class RevenueProjection
{
    public string CompanyId { get; }
    public int StartMonth { get; }
    public IReadOnlyList<MonthlyRevenue> MonthlyRevenues { get; }
    public decimal TotalProjectedRevenue { get; }
    public TrendAnalysis Trend { get; }

    public RevenueProjection(string companyId, int startMonth, IReadOnlyList<MonthlyRevenue> monthlyRevenues, decimal totalProjectedRevenue, TrendAnalysis trend)
    {
        CompanyId = companyId;
        StartMonth = startMonth;
        MonthlyRevenues = monthlyRevenues;
        TotalProjectedRevenue = totalProjectedRevenue;
        Trend = trend;
    }
}

/// <summary>
/// Revenus mensuels projetés
/// </summary>
public sealed class MonthlyRevenue
{
    public int Month { get; }
    public decimal TicketSales { get; }
    public decimal Merchandise { get; }
    public decimal TvDeals { get; }
    public decimal Sponsors { get; }
    public decimal TotalRevenue { get; }

    public MonthlyRevenue(int month, decimal ticketSales, decimal merchandise, decimal tvDeals, decimal sponsors, decimal totalRevenue)
    {
        Month = month;
        TicketSales = ticketSales;
        Merchandise = merchandise;
        TvDeals = tvDeals;
        Sponsors = sponsors;
        TotalRevenue = totalRevenue;
    }
}

/// <summary>
/// Analyse de tendance
/// </summary>
public sealed class TrendAnalysis
{
    public decimal GrowthRate { get; }
    public string Trend { get; } // "Increasing", "Stable", "Decreasing"
    public decimal Volatility { get; }
    public string Recommendation { get; }

    public TrendAnalysis(decimal growthRate, string trend, decimal volatility, string recommendation)
    {
        GrowthRate = growthRate;
        Trend = trend;
        Volatility = volatility;
        Recommendation = recommendation;
    }
}