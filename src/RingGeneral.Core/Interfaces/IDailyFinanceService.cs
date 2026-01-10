namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service pour la gestion des finances quotidiennes avec deux flux distincts :
/// 1. Paiement mensuel garanti (fixe) - déclenché fin du mois
/// 2. Frais d'apparition (per-appearance) - déclenché après chaque show
/// </summary>
public interface IDailyFinanceService
{
    /// <summary>
    /// FLUX 1 : Traite le paiement mensuel garanti (dernier jour du mois uniquement)
    /// Ce paiement est indépendant de l'activité du worker
    /// </summary>
    /// <param name="companyId">ID de la compagnie</param>
    /// <param name="currentDate">Date actuelle (doit être le dernier jour du mois)</param>
    void ProcessMonthlyPayroll(string companyId, DateTime currentDate);

    /// <summary>
    /// FLUX 2 : Traite les frais d'apparition (appelé immédiatement après un show)
    /// Ce paiement est déclenché uniquement quand le worker participe à un show
    /// </summary>
    /// <param name="companyId">ID de la compagnie</param>
    /// <param name="workerIds">Liste des workers qui ont participé au show</param>
    /// <param name="showDate">Date exacte du show (pour éviter doubles paiements)</param>
    void ProcessAppearanceFees(string companyId, IReadOnlyList<string> workerIds, DateTime showDate);
}
