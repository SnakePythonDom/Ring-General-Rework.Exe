namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service pour la mise à jour quotidienne des statistiques des workers
/// (fatigue, blessures, récupération)
/// </summary>
public interface IDailyStatUpdateService
{
    /// <summary>
    /// Met à jour les statistiques quotidiennes pour tous les workers d'une compagnie
    /// </summary>
    /// <param name="companyId">ID de la compagnie</param>
    /// <param name="currentDay">Jour actuel</param>
    void UpdateDailyStats(string companyId, int currentDay);
}
