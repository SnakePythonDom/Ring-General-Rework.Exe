using RingGeneral.Core.Models.Crisis;
using System.Collections.Generic;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le moteur de gestion de crises.
/// Détecte, escalade, et résout les crises backstage.
/// </summary>
public interface ICrisisEngine
{
    /// <summary>
    /// Détecte si des signaux faibles indiquent une crise potentielle
    /// </summary>
    /// <param name="companyId">Identifiant de la compagnie</param>
    /// <param name="companyMoraleScore">Score de moral de la compagnie (0-100)</param>
    /// <param name="activeRumorsCount">Nombre de rumeurs actives</param>
    /// <returns>True si une crise doit être créée</returns>
    bool ShouldTriggerCrisis(string companyId, int companyMoraleScore, int activeRumorsCount);

    /// <summary>
    /// Crée une nouvelle crise basée sur le contexte
    /// </summary>
    /// <param name="companyId">Identifiant de la compagnie</param>
    /// <param name="triggerReason">Raison déclenchante</param>
    /// <param name="severity">Sévérité initiale (1-5)</param>
    /// <returns>Crise créée</returns>
    Crisis CreateCrisis(string companyId, string triggerReason, int severity);

    /// <summary>
    /// Fait progresser toutes les crises actives (appelé chaque semaine)
    /// </summary>
    /// <param name="companyId">Identifiant de la compagnie</param>
    void ProgressCrises(string companyId);

    /// <summary>
    /// Escalade une crise au stage suivant si le score d'escalade est élevé
    /// </summary>
    /// <param name="crisisId">Identifiant de la crise</param>
    /// <returns>Crise mise à jour</returns>
    Crisis? EscalateCrisis(int crisisId);

    /// <summary>
    /// Tente de résoudre une crise via intervention
    /// </summary>
    /// <param name="crisisId">Identifiant de la crise</param>
    /// <param name="interventionQuality">Qualité de l'intervention (0-100)</param>
    /// <returns>True si résolue</returns>
    bool AttemptResolution(int crisisId, int interventionQuality);

    /// <summary>
    /// Calcule l'impact d'une crise sur le moral de la compagnie
    /// </summary>
    /// <param name="crisis">La crise</param>
    /// <returns>Impact négatif sur le moral (-50 à 0)</returns>
    int CalculateMoraleImpact(Crisis crisis);

    /// <summary>
    /// Détermine si une crise devrait être ignorée (dissipée naturellement)
    /// </summary>
    /// <param name="crisis">La crise</param>
    /// <returns>True si doit être ignorée</returns>
    bool ShouldIgnoreCrisis(Crisis crisis);

    /// <summary>
    /// Récupère toutes les crises actives d'une compagnie
    /// </summary>
    /// <param name="companyId">Identifiant de la compagnie</param>
    /// <returns>Liste des crises actives</returns>
    List<Crisis> GetActiveCrises(string companyId);

    /// <summary>
    /// Récupère les crises critiques nécessitant attention immédiate
    /// </summary>
    /// <param name="companyId">Identifiant de la compagnie</param>
    /// <returns>Liste des crises critiques</returns>
    List<Crisis> GetCriticalCrises(string companyId);
}
