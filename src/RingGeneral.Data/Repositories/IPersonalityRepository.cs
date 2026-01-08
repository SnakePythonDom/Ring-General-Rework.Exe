using RingGeneral.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository pour la gestion des attributs mentaux et personnalités.
/// </summary>
public interface IPersonalityRepository
{
    // === Mental Attributes ===

    /// <summary>
    /// Récupère les attributs mentaux d'une entité.
    /// </summary>
    Task<MentalAttributes?> GetMentalAttributesAsync(string entityId, string entityType);

    /// <summary>
    /// Sauvegarde ou met à jour les attributs mentaux.
    /// </summary>
    Task SaveMentalAttributesAsync(MentalAttributes attributes);

    /// <summary>
    /// Supprime les attributs mentaux d'une entité.
    /// </summary>
    Task DeleteMentalAttributesAsync(string entityId, string entityType);

    // === Personality ===

    /// <summary>
    /// Récupère la personnalité d'une entité.
    /// </summary>
    Task<Personality?> GetPersonalityAsync(string entityId, string entityType);

    /// <summary>
    /// Sauvegarde ou met à jour la personnalité.
    /// </summary>
    Task SavePersonalityAsync(Personality personality);

    /// <summary>
    /// Enregistre un changement de personnalité dans l'historique.
    /// </summary>
    Task LogPersonalityChangeAsync(PersonalityHistory history);

    /// <summary>
    /// Récupère l'historique des changements de personnalité.
    /// </summary>
    Task<List<PersonalityHistory>> GetPersonalityHistoryAsync(string entityId, string entityType);

    // === Batch Operations ===

    /// <summary>
    /// Récupère tous les attributs mentaux pour une compagnie.
    /// </summary>
    Task<List<MentalAttributes>> GetAllMentalAttributesByCompanyAsync(string companyId);

    /// <summary>
    /// Récupère toutes les personnalités pour une compagnie.
    /// </summary>
    Task<List<Personality>> GetAllPersonalitiesByCompanyAsync(string companyId);

    /// <summary>
    /// Initialise les attributs mentaux et personnalité pour une entité.
    /// Génère aléatoirement si pas existant.
    /// </summary>
    Task InitializePersonalitySystemAsync(string entityId, string entityType);
}
