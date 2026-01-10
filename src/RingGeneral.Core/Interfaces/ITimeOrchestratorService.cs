using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour l'orchestrateur du passage du temps.
/// Gère le passage du temps jour par jour avec toutes les mises à jour associées.
/// </summary>
public interface ITimeOrchestratorService
{
    /// <summary>
    /// Fait passer au jour suivant pour une compagnie
    /// Gère les statistiques quotidiennes, événements et shows
    /// </summary>
    /// <param name="companyId">ID de la compagnie</param>
    /// <returns>Résultat du passage du jour</returns>
    DailyTickResult PasserJourSuivant(string companyId);
}