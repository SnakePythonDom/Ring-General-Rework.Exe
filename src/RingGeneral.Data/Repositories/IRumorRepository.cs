using RingGeneral.Core.Models.Morale;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository pour la gestion des rumeurs backstage.
/// </summary>
public interface IRumorRepository
{
    /// <summary>
    /// Récupère une rumeur par ID.
    /// </summary>
    Task<Rumor?> GetRumorByIdAsync(int rumorId);

    /// <summary>
    /// Sauvegarde une nouvelle rumeur.
    /// </summary>
    Task SaveRumorAsync(Rumor rumor);

    /// <summary>
    /// Met à jour une rumeur existante.
    /// </summary>
    Task UpdateRumorAsync(Rumor rumor);

    /// <summary>
    /// Récupère toutes les rumeurs actives pour une compagnie.
    /// </summary>
    Task<List<Rumor>> GetActiveRumorsAsync(string companyId);

    /// <summary>
    /// Récupère toutes les rumeurs (actives et inactives) pour une compagnie.
    /// </summary>
    Task<List<Rumor>> GetAllRumorsByCompanyAsync(string companyId);

    /// <summary>
    /// Récupère les rumeurs par stage.
    /// </summary>
    Task<List<Rumor>> GetRumorsByStageAsync(string companyId, string stage);

    /// <summary>
    /// Récupère les rumeurs répandues (AmplificationScore >= 70).
    /// </summary>
    Task<List<Rumor>> GetWidespreadRumorsAsync(string companyId);

    /// <summary>
    /// Supprime les rumeurs résolues ou ignorées plus anciennes que X jours.
    /// </summary>
    Task CleanupOldRumorsAsync(string companyId, int daysToKeep = 90);
}
