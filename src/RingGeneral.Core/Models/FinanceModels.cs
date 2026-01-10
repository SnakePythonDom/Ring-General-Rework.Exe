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
    public decimal Billetterie { get; set; }
    public decimal Merch { get; set; }
    public decimal Tv { get; set; }
    public decimal CoutsProduction { get; set; }
    public required IReadOnlyList<FinanceTransaction> Transactions { get; set; }

    public FinanceShowResult(decimal billetterie, decimal merch, decimal tv, decimal coutsProduction, IReadOnlyList<FinanceTransaction> transactions)
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
    public required IReadOnlyList<ContractPayroll> Contrats { get; set; }

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
    public decimal Revenus { get; set; }
    public decimal Depenses { get; set; }
    public required IReadOnlyList<FinanceTransaction> Transactions { get; set; }

    public FinanceTickResult(decimal revenus, decimal depenses, IReadOnlyList<FinanceTransaction> transactions)
    {
        Revenus = revenus;
        Depenses = depenses;
        Transactions = transactions;
    }
}

/// <summary>
/// Transaction financière
/// </summary>
public sealed class FinanceTransaction
{
    public required string Id { get; set; }
    public decimal Amount { get; set; }
    public required string Description { get; set; }

    public FinanceTransaction(string id, decimal amount, string description)
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