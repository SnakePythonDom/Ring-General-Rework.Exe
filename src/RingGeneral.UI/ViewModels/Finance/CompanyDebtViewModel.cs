using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels.Finance;

/// <summary>
/// Phase 2.2 - ViewModel pour afficher une dette dans l'UI
/// </summary>
public sealed class CompanyDebtViewModel : ViewModelBase
{
    private readonly CompanyDebt _debt;
    private readonly IDebtManagementService _debtService;

    public CompanyDebtViewModel(CompanyDebt debt, IDebtManagementService debtService)
    {
        _debt = debt ?? throw new ArgumentNullException(nameof(debt));
        _debtService = debtService ?? throw new ArgumentNullException(nameof(debtService));
    }

    public string DebtId => _debt.DebtId;
    public decimal PrincipalAmount => _debt.PrincipalAmount;
    public string PrincipalAmountFormatted => $"${PrincipalAmount:N0}";
    public decimal InterestRate => _debt.InterestRate;
    public string InterestRateFormatted => $"{InterestRate:F2}%";
    public int TermMonths => _debt.TermMonths;
    public string TermFormatted => $"{TermMonths} mois";
    public decimal MonthlyPayment => _debt.MonthlyPayment;
    public string MonthlyPaymentFormatted => $"${MonthlyPayment:N0}/mois";
    public decimal RemainingBalance => _debt.RemainingBalance;
    public string RemainingBalanceFormatted => $"${RemainingBalance:N0}";
    public decimal MonthlyInterest => _debtService.CalculateMonthlyInterest(_debt);
    public string MonthlyInterestFormatted => $"${MonthlyInterest:N0}/mois";
    public int RemainingMonths => CalculateRemainingMonths();
    public string RemainingMonthsFormatted => $"{RemainingMonths} mois restants";

    private int CalculateRemainingMonths()
    {
        if (MonthlyPayment == 0)
            return 0;

        var monthsElapsed = (DateTime.Now - _debt.StartDate).Days / 30;
        return Math.Max(0, TermMonths - monthsElapsed);
    }
}
