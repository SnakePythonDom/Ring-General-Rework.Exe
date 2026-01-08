using RingGeneral.Core.Models.Morale;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository pour la gestion du moral backstage et de la compagnie.
/// </summary>
public interface IMoraleRepository
{
    // === BackstageMorale ===

    /// <summary>
    /// Récupère le moral d'une entité.
    /// </summary>
    Task<BackstageMorale?> GetBackstageMoraleAsync(string entityId, string companyId);

    /// <summary>
    /// Sauvegarde ou met à jour le moral d'une entité.
    /// </summary>
    Task SaveBackstageMoraleAsync(BackstageMorale morale);

    /// <summary>
    /// Récupère tous les morales faibles (< 40) pour une compagnie.
    /// </summary>
    Task<List<BackstageMorale>> GetLowMoraleEntitiesAsync(string companyId, int threshold = 40);

    /// <summary>
    /// Récupère tous les morales critiques (< 20) pour une compagnie.
    /// </summary>
    Task<List<BackstageMorale>> GetCriticalMoraleEntitiesAsync(string companyId, int threshold = 20);

    /// <summary>
    /// Récupère tous les morales pour une compagnie.
    /// </summary>
    Task<List<BackstageMorale>> GetAllBackstageMoraleAsync(string companyId);

    // === CompanyMorale ===

    /// <summary>
    /// Récupère le moral global de la compagnie.
    /// </summary>
    Task<CompanyMorale?> GetCompanyMoraleAsync(string companyId);

    /// <summary>
    /// Sauvegarde ou met à jour le moral global de la compagnie.
    /// </summary>
    Task SaveCompanyMoraleAsync(CompanyMorale morale);

    /// <summary>
    /// Calcule et sauvegarde le moral de compagnie basé sur les morales individuels.
    /// </summary>
    Task RecalculateCompanyMoraleAsync(string companyId);
}
