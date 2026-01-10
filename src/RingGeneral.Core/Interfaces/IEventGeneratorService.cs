namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service pour la génération d'événements aléatoires quotidiens
/// (rumeurs, incidents, nouvelles, etc.)
/// </summary>
public interface IEventGeneratorService
{
    /// <summary>
    /// Génère des événements aléatoires pour le jour donné
    /// </summary>
    /// <param name="companyId">ID de la compagnie</param>
    /// <param name="currentDay">Jour actuel</param>
    /// <returns>Liste des événements générés (messages pour l'inbox)</returns>
    IReadOnlyList<string> GenerateDailyEvents(string companyId, int currentDay);
}
