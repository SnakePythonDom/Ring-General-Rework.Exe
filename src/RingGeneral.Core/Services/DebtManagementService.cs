using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

/// <summary>
/// Phase 2.2 - Service pour gérer la dette de la compagnie
/// </summary>
public sealed class DebtManagementService : IDebtManagementService
{
    private readonly ICompanyRepository _companyRepository;

    public DebtManagementService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
    }

    /// <summary>
    /// Calcule les intérêts mensuels pour une dette
    /// </summary>
    public decimal CalculateMonthlyInterest(CompanyDebt debt)
    {
        return debt.RemainingBalance * (debt.InterestRate / 100m) / 12m;
    }

    /// <summary>
    /// Calcule le paiement mensuel total (principal + intérêts)
    /// </summary>
    public decimal CalculateTotalMonthlyPayment(CompanyDebt debt)
    {
        // Formule d'amortissement : P * [r(1+r)^n] / [(1+r)^n - 1]
        // où P = principal, r = taux mensuel, n = nombre de mois
        var monthlyRate = debt.InterestRate / 100m / 12m;
        var months = debt.TermMonths;
        
        if (monthlyRate == 0)
            return debt.PrincipalAmount / months;

        var factor = (decimal)Math.Pow((double)(1 + monthlyRate), months);
        return debt.PrincipalAmount * (monthlyRate * factor) / (factor - 1);
    }

    /// <summary>
    /// Applique un paiement mensuel et met à jour le solde restant
    /// </summary>
    public CompanyDebt ApplyMonthlyPayment(CompanyDebt debt)
    {
        var interest = CalculateMonthlyInterest(debt);
        var principalPayment = debt.MonthlyPayment - interest;
        var newBalance = Math.Max(0, debt.RemainingBalance - principalPayment);

        return debt with
        {
            RemainingBalance = newBalance
        };
    }

    /// <summary>
    /// Crée une nouvelle dette
    /// </summary>
    public CompanyDebt CreateDebt(
        string companyId,
        decimal principalAmount,
        decimal interestRate,
        int termMonths)
    {
        var debtId = Guid.NewGuid().ToString("N");
        var startDate = DateTime.Now;
        
        var debt = new CompanyDebt(
            debtId,
            companyId,
            principalAmount,
            interestRate,
            termMonths,
            startDate,
            0m, // Sera calculé
            principalAmount);

        var monthlyPayment = CalculateTotalMonthlyPayment(debt);
        return debt with { MonthlyPayment = monthlyPayment };
    }

    /// <summary>
    /// Récupère toutes les dettes actives d'une compagnie
    /// </summary>
    public List<CompanyDebt> GetActiveDebts(string companyId)
    {
        // TODO: Implémenter avec ICompanyRepository quand méthode disponible
        // Pour l'instant, retourner liste vide
        return new List<CompanyDebt>();
    }

    /// <summary>
    /// Calcule le total des paiements mensuels pour toutes les dettes
    /// </summary>
    public decimal CalculateTotalMonthlyDebtPayments(string companyId)
    {
        var debts = GetActiveDebts(companyId);
        return debts.Sum(d => d.MonthlyPayment);
    }
}
