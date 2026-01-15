using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Phase 2.2 - Interface pour la gestion de la dette
/// </summary>
public interface IDebtManagementService
{
    decimal CalculateMonthlyInterest(CompanyDebt debt);
    decimal CalculateTotalMonthlyPayment(CompanyDebt debt);
    CompanyDebt ApplyMonthlyPayment(CompanyDebt debt);
    CompanyDebt CreateDebt(string companyId, decimal principalAmount, decimal interestRate, int termMonths);
    List<CompanyDebt> GetActiveDebts(string companyId);
    decimal CalculateTotalMonthlyDebtPayments(string companyId);
}
