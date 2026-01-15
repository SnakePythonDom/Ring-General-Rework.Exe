using RingGeneral.Core.Models.Booker;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Interface pour le repository des bookers.
/// Gère les opérations CRUD et requêtes métier pour les bookers, leurs mémoires et historique.
/// </summary>
public interface IBookerRepository
{
    // ====================================================================
    // BOOKER CRUD OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde un nouveau booker dans la base de données
    /// </summary>
    Task SaveBookerAsync(Booker booker);

    /// <summary>
    /// Récupère un booker par son identifiant
    /// </summary>
    Task<Booker?> GetBookerByIdAsync(string bookerId);

    /// <summary>
    /// Récupère tous les bookers actifs d'une compagnie
    /// </summary>
    Task<List<Booker>> GetActiveBookersByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère tous les bookers (actifs et inactifs) d'une compagnie
    /// </summary>
    Task<List<Booker>> GetAllBookersByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère le booker actif avec auto-booking activé pour une compagnie
    /// </summary>
    Task<Booker?> GetAutoBookingBookerAsync(string companyId);

    /// <summary>
    /// Met à jour un booker existant
    /// </summary>
    Task UpdateBookerAsync(Booker booker);

    /// <summary>
    /// Supprime un booker (rare, mais possible)
    /// </summary>
    Task DeleteBookerAsync(string bookerId);

    /// <summary>
    /// Active/désactive l'auto-booking pour un booker
    /// </summary>
    Task ToggleAutoBookingAsync(string bookerId, bool enabled);

    /// <summary>
    /// Change le statut d'emploi d'un booker
    /// </summary>
    Task UpdateEmploymentStatusAsync(string bookerId, string status);

    // ====================================================================
    // BOOKER MEMORY OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde une nouvelle mémoire pour un booker
    /// </summary>
    Task SaveBookerMemoryAsync(BookerMemory memory);

    /// <summary>
    /// Récupère toutes les mémoires d'un booker
    /// </summary>
    Task<List<BookerMemory>> GetBookerMemoriesAsync(string bookerId);

    /// <summary>
    /// Récupère les mémoires fortes (RecallStrength >= 70) d'un booker
    /// </summary>
    Task<List<BookerMemory>> GetStrongMemoriesAsync(string bookerId);

    /// <summary>
    /// Récupère les mémoires récentes (dernières N semaines) d'un booker
    /// </summary>
    Task<List<BookerMemory>> GetRecentMemoriesAsync(string bookerId, int weeksPast = 12);

    /// <summary>
    /// Récupère les mémoires par type d'événement
    /// </summary>
    Task<List<BookerMemory>> GetMemoriesByTypeAsync(string bookerId, string eventType);

    /// <summary>
    /// Met à jour une mémoire existante (pour decay ou renforcement)
    /// </summary>
    Task UpdateBookerMemoryAsync(BookerMemory memory);

    /// <summary>
    /// Supprime les mémoires faibles (RecallStrength < 10) pour cleanup
    /// </summary>
    Task CleanupWeakMemoriesAsync(string bookerId);

    // ====================================================================
    // EMPLOYMENT HISTORY OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde une nouvelle entrée d'historique d'emploi
    /// </summary>
    Task SaveEmploymentHistoryAsync(BookerEmploymentHistory history);

    /// <summary>
    /// Récupère tout l'historique d'emploi d'un booker
    /// </summary>
    Task<List<BookerEmploymentHistory>> GetEmploymentHistoryAsync(string bookerId);

    /// <summary>
    /// Récupère l'emploi actuel d'un booker (EndDate = NULL)
    /// </summary>
    Task<BookerEmploymentHistory?> GetCurrentEmploymentAsync(string bookerId);

    /// <summary>
    /// Met à jour une entrée d'historique (pour terminer l'emploi)
    /// </summary>
    Task UpdateEmploymentHistoryAsync(BookerEmploymentHistory history);

    /// <summary>
    /// Récupère l'historique d'emploi pour une compagnie
    /// </summary>
    Task<List<BookerEmploymentHistory>> GetCompanyEmploymentHistoryAsync(string companyId);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    /// <summary>
    /// Compte le nombre de bookers actifs dans une compagnie
    /// </summary>
    Task<int> CountActiveBookersAsync(string companyId);

    /// <summary>
    /// Vérifie si une compagnie a au moins un booker actif
    /// </summary>
    Task<bool> CompanyHasActiveBookerAsync(string companyId);

    /// <summary>
    /// Récupère tous les bookers avec auto-booking activé (toutes compagnies)
    /// </summary>
    Task<List<Booker>> GetAllAutoBookingBookersAsync();
}
