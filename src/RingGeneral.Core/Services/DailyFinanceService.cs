using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

/// <summary>
/// Gère les finances quotidiennes avec DEUX FLUX FINANCIERS DISTINCTS :
/// 1. Paiement mensuel garanti (fixe) - déclenché uniquement le dernier jour du mois
/// 2. Frais d'apparition (per-appearance) - déclenché immédiatement après chaque show
/// </summary>
public sealed class DailyFinanceService : IDailyFinanceService
{
    private readonly IGameRepository _repository;

    public DailyFinanceService(IGameRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// FLUX 1 : Traite le paiement mensuel garanti (dernier jour du mois uniquement)
    /// Ce paiement est indépendant de l'activité du worker (il est garanti même s'il ne lutte pas)
    /// </summary>
    public void ProcessMonthlyPayroll(string companyId, DateTime currentDate)
    {
        var contracts = _repository.ChargerContratsHybrides(companyId);
        var transactions = new List<FinanceTransaction>();

        foreach (var contract in contracts)
        {
            // Ignorer les contrats sans salaire mensuel garanti
            if (contract.MonthlyWage <= 0)
            {
                continue; // Contrat par apparition uniquement, pas de paiement mensuel
            }

            // Vérifier si déjà payé ce mois-ci (éviter doubles prélèvements)
            if (contract.LastPaymentDate.HasValue &&
                contract.LastPaymentDate.Value.Year == currentDate.Year &&
                contract.LastPaymentDate.Value.Month == currentDate.Month)
            {
                continue; // Déjà payé ce mois-ci
            }

            // Déduire le salaire mensuel garanti (indépendant de l'activité)
            transactions.Add(new FinanceTransaction(
                "paie_mensuelle",
                -contract.MonthlyWage,
                $"Salaire mensuel garanti - {contract.WorkerId}"));

            // Mettre à jour LastPaymentDate pour éviter double paiement
            _repository.MettreAJourDatePaiement(contract.ContractId, currentDate);
        }

        // Appliquer les transactions
        if (transactions.Count > 0)
        {
            _repository.AppliquerTransactionsFinancieres(companyId, currentDate, transactions);
        }
    }

    /// <summary>
    /// FLUX 2 : Traite les frais d'apparition (appelé immédiatement après un show)
    /// Ce paiement est déclenché uniquement quand le worker participe à un show
    /// </summary>
    public void ProcessAppearanceFees(string companyId, IReadOnlyList<string> workerIds, DateTime showDate)
    {
        var contracts = _repository.ChargerContratsHybrides(companyId);
        var transactions = new List<FinanceTransaction>();

        foreach (var workerId in workerIds)
        {
            var contract = contracts.FirstOrDefault(c => c.WorkerId == workerId);

            // Ignorer si pas de contrat ou pas de frais d'apparition
            if (contract is null || contract.AppearanceFee <= 0)
            {
                continue; // Contrat fixe uniquement, pas de frais d'apparition
            }

            // Vérifier si déjà payé pour cette date exacte (éviter doubles paiements)
            // Note: On compare la date complète, pas juste le jour, pour éviter conflits
            if (contract.LastAppearanceDate.HasValue &&
                contract.LastAppearanceDate.Value.Date == showDate.Date)
            {
                continue; // Déjà payé pour ce show
            }

            // Déduire les frais d'apparition (paiement immédiat)
            transactions.Add(new FinanceTransaction(
                "frais_apparition",
                -contract.AppearanceFee,
                $"Frais d'apparition - {workerId} (Show {showDate:yyyy-MM-dd})"));

            // Mettre à jour LastAppearanceDate pour éviter double paiement
            _repository.MettreAJourDateApparition(contract.ContractId, showDate);
        }

        // Appliquer les transactions immédiatement
        if (transactions.Count > 0)
        {
            _repository.AppliquerTransactionsFinancieres(companyId, showDate, transactions);
        }
    }
}
