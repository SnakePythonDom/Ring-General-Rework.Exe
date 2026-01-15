using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Relations;

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

    /// <summary>
    /// Vérifie si une compagnie est contrôlée par le joueur
    /// </summary>
    bool EstCompagnieJoueur(string companyId);

    /// <summary>
    /// Charge la semaine associée à un show spécifique
    /// </summary>
    int ChargerSemaineShow(string showId);

    /// <summary>
    /// Obtient l'ID de l'owner d'une compagnie
    /// </summary>
    string? ObtenirOwnerId(string companyId);

    /// <summary>
    /// Crée une connexion à la base de données (Utiliser avec précaution)
    /// </summary>
    System.Data.Common.DbConnection CreateConnection();

    /// <summary>
    /// Charge les noms des workers (ID -> Nom)
    /// </summary>
    System.Collections.Generic.IReadOnlyDictionary<string, string> ChargerNomsWorkers();

    /// <summary>
    /// Met à jour les données d'un worker (incluant attributs, relations, etc.)
    /// </summary>
    void UpdateWorker(Worker worker);
    /// <summary>
    /// Récupère un worker par son ID string (pour compatibilité ID string)
    /// </summary>
    Worker? GetWorker(string id);

    /// <summary>
    /// Met à jour les stats quotidiennes pour tous les workers d'une compagnie
    /// </summary>
    void MettreAJourStatsQuotidiennes(string companyId);

    /// <summary>
    /// Accès au repository des relations.
    /// </summary>
    IRelationsRepository RelationsRepository { get; }
    IPersonalityEngine PersonalityEngine { get; }
    IOwnerDecisionEngine OwnerDecisionEngine { get; }
    IBookerAIEngine BookerAIEngine { get; }

    /// <summary>
    /// Ajoute ou met à jour une relation entre deux workers
    /// </summary>
    void AddOrUpdateRelation(WorkerRelation relation);

    /// <summary>
    /// Termine le contrat actuel d'un worker (Licenciement)
    /// </summary>
    void TerminateCurrentContract(string workerId, DateTime date);

    /// <summary>
    /// Récupère la mémoire d'un booker (événements marquants).
    /// </summary>
    IReadOnlyList<BookerMemoryEntry> GetBookerMemory(string bookerId, string? workerId = null);

    /// <summary>
    /// Récupère toutes les relations d'un worker.
    /// </summary>
    IReadOnlyList<WorkerRelation> GetWorkerRelations(string workerId);
}
