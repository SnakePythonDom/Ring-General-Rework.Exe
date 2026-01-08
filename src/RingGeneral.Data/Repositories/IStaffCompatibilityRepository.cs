using RingGeneral.Core.Models.Staff;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Interface pour le repository des compatibilités staff-booker.
/// Gère le cache des compatibilités calculées.
/// </summary>
public interface IStaffCompatibilityRepository
{
    // ====================================================================
    // STAFF COMPATIBILITY CRUD OPERATIONS
    // ====================================================================

    /// <summary>
    /// Sauvegarde une nouvelle compatibilité
    /// </summary>
    Task SaveCompatibilityAsync(StaffCompatibility compatibility);

    /// <summary>
    /// Récupère une compatibilité par son identifiant
    /// </summary>
    Task<StaffCompatibility?> GetCompatibilityByIdAsync(string compatibilityId);

    /// <summary>
    /// Récupère la compatibilité entre un staff et un booker
    /// </summary>
    Task<StaffCompatibility?> GetCompatibilityAsync(string staffId, string bookerId);

    /// <summary>
    /// Récupère toutes les compatibilités d'un staff
    /// </summary>
    Task<List<StaffCompatibility>> GetCompatibilitiesByStaffIdAsync(string staffId);

    /// <summary>
    /// Récupère toutes les compatibilités d'un booker
    /// </summary>
    Task<List<StaffCompatibility>> GetCompatibilitiesByBookerIdAsync(string bookerId);

    /// <summary>
    /// Met à jour une compatibilité existante
    /// </summary>
    Task UpdateCompatibilityAsync(StaffCompatibility compatibility);

    /// <summary>
    /// Supprime une compatibilité
    /// </summary>
    Task DeleteCompatibilityAsync(string compatibilityId);

    /// <summary>
    /// Incrémente les compteurs de collaboration
    /// </summary>
    Task IncrementCollaborationAsync(string compatibilityId, bool successful);

    /// <summary>
    /// Incrémente l'historique de conflits
    /// </summary>
    Task IncrementConflictHistoryAsync(string compatibilityId);

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    /// <summary>
    /// Récupère les compatibilités dangereuses (score <= 30)
    /// </summary>
    Task<List<StaffCompatibility>> GetDangerousCompatibilitiesAsync(string companyId);

    /// <summary>
    /// Récupère les excellentes compatibilités (score >= 80)
    /// </summary>
    Task<List<StaffCompatibility>> GetExcellentCompatibilitiesAsync(string companyId);

    /// <summary>
    /// Récupère les compatibilités nécessitant recalcul (> 30 jours)
    /// </summary>
    Task<List<StaffCompatibility>> GetCompatibilitiesNeedingRecalculationAsync();

    /// <summary>
    /// Vérifie si une compatibilité existe déjà
    /// </summary>
    Task<bool> CompatibilityExistsAsync(string staffId, string bookerId);

    /// <summary>
    /// Calcule le score moyen de compatibilité pour un booker
    /// </summary>
    Task<double> CalculateAverageCompatibilityScoreAsync(string bookerId);
}
