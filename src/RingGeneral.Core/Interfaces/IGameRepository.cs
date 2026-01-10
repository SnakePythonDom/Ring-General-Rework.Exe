using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour accéder aux méthodes du GameRepository nécessaires au système quotidien
/// Permet d'éviter la dépendance Core -> Data
/// </summary>
public interface IGameRepository
{
    /// <summary>
    /// Incrémente le jour actuel et retourne le nouveau jour
    /// </summary>
    int IncrementerJour(string companyId);

    /// <summary>
    /// Récupère la date actuelle du jeu pour une compagnie
    /// </summary>
    DateTime GetCurrentDate(string companyId);

    /// <summary>
    /// Charge les contrats hybrides actifs pour une compagnie
    /// </summary>
    IReadOnlyList<HybridContract> ChargerContratsHybrides(string companyId);

    /// <summary>
    /// Met à jour la date de dernier paiement mensuel pour un contrat
    /// </summary>
    void MettreAJourDatePaiement(string contractId, DateTime paymentDate);

    /// <summary>
    /// Met à jour la date de dernière apparition payée pour un contrat
    /// </summary>
    void MettreAJourDateApparition(string contractId, DateTime appearanceDate);

    /// <summary>
    /// Applique des transactions financières avec une date (pour système quotidien)
    /// </summary>
    double AppliquerTransactionsFinancieres(
        string companyId,
        DateTime date,
        IReadOnlyList<FinanceTransaction> transactions);
}
