using RingGeneral.Core.Models.Owner;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Interface pour le repository des propriétaires (Owners).
/// Gère les opérations CRUD et requêtes métier pour les owners.
/// </summary>
public interface IOwnerRepository
{
    /// <summary>
    /// Sauvegarde un nouveau owner dans la base de données
    /// </summary>
    Task SaveOwnerAsync(Owner owner);

    /// <summary>
    /// Récupère un owner par son identifiant
    /// </summary>
    Task<Owner?> GetOwnerByIdAsync(string ownerId);

    /// <summary>
    /// Récupère l'owner d'une compagnie
    /// </summary>
    Task<Owner?> GetOwnerByCompanyIdAsync(string companyId);

    /// <summary>
    /// Récupère tous les owners
    /// </summary>
    Task<List<Owner>> GetAllOwnersAsync();

    /// <summary>
    /// Récupère les owners par type de vision (Creative, Business, Balanced)
    /// </summary>
    Task<List<Owner>> GetOwnersByVisionTypeAsync(string visionType);

    /// <summary>
    /// Récupère les owners avec tolérance au risque >= minimum
    /// </summary>
    Task<List<Owner>> GetOwnersWithRiskToleranceAboveAsync(int minRiskTolerance);

    /// <summary>
    /// Met à jour un owner existant
    /// </summary>
    Task UpdateOwnerAsync(Owner owner);

    /// <summary>
    /// Supprime un owner (rare, mais possible si compagnie fermée)
    /// </summary>
    Task DeleteOwnerAsync(string ownerId);

    /// <summary>
    /// Compte le nombre d'owners total
    /// </summary>
    Task<int> CountOwnersAsync();

    /// <summary>
    /// Vérifie si une compagnie a déjà un owner
    /// </summary>
    Task<bool> CompanyHasOwnerAsync(string companyId);
}
