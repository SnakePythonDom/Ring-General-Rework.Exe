using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour l'orchestrateur du flux "Show Day" (Match Day).
/// Gère la détection d'événements, la simulation et l'application des impacts.
/// </summary>
public interface IShowDayOrchestrator
{
    /// <summary>
    /// Détecte si un show est prévu pour ce jour
    /// </summary>
    /// <param name="companyId">ID de la compagnie</param>
    /// <param name="currentDay">Jour actuel</param>
    /// <returns>Résultat de la détection</returns>
    ShowDayDetectionResult DetecterShowAVenir(string companyId, int currentDay);

    /// <summary>
    /// Simule un show complet basé sur son contexte
    /// </summary>
    /// <param name="context">Contexte du show à simuler</param>
    /// <returns>Résultat de la simulation</returns>
    ShowSimulationResult SimulerShow(ShowContext context);

    /// <summary>
    /// Génère automatiquement un booking pour une compagnie AI
    /// </summary>
    /// <param name="bookerId">ID du booker</param>
    /// <param name="context">Contexte du show</param>
    /// <param name="isPlayerCompany">Si c'est la compagnie du joueur</param>
    /// <returns>Liste des segments générés</returns>
    List<SegmentDefinition> GenerateAICompanyBooking(string bookerId, ShowContext context, bool isPlayerCompany);

    /// <summary>
    /// Finalise un show après simulation en appliquant tous les impacts
    /// </summary>
    /// <param name="resultat">Résultat de la simulation</param>
    /// <param name="context">Contexte du show</param>
    /// <returns>Résultat de la finalisation</returns>
    ShowDayFinalizationResult FinaliserShow(ShowSimulationResult resultat, ShowContext context);

    /// <summary>
    /// Exécute le flux complet "Show Day" pour un show
    /// </summary>
    /// <param name="showId">ID du show</param>
    /// <param name="companyId">ID de la compagnie</param>
    /// <returns>Résultat complet du flux</returns>
    ShowDayFluxCompletResult ExecuterFluxComplet(string showId, string companyId);
}