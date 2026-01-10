using RingGeneral.Core.Models.Booker;
using System.Collections.Generic;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le moteur AI de booking.
/// Permet aux bookers de prendre des décisions autonomes basées sur leurs préférences et mémoires.
/// </summary>
public interface IBookerAIEngine
{
    /// <summary>
    /// Propose un main event pour un show basé sur les préférences du booker
    /// </summary>
    /// <param name="bookerId">Identifiant du booker</param>
    /// <param name="availableWorkers">Liste des workers disponibles</param>
    /// <param name="showImportance">Importance du show (0-100)</param>
    /// <returns>Paire de WorkerId pour le main event suggéré</returns>
    (string Worker1Id, string Worker2Id)? ProposeMainEvent(
        string bookerId,
        List<string> availableWorkers,
        int showImportance);

    /// <summary>
    /// Évalue la qualité d'un match après qu'il ait eu lieu
    /// </summary>
    /// <param name="bookerId">Identifiant du booker</param>
    /// <param name="matchRating">Note du match (0-100)</param>
    /// <param name="fanReaction">Réaction des fans (-100 à +100)</param>
    /// <param name="worker1Id">Premier participant</param>
    /// <param name="worker2Id">Deuxième participant</param>
    /// <returns>Score d'évaluation global (0-100)</returns>
    int EvaluateMatchQuality(
        string bookerId,
        int matchRating,
        int fanReaction,
        string worker1Id,
        string worker2Id);

    /// <summary>
    /// Crée une mémoire pour le booker basée sur le résultat d'un match
    /// </summary>
    /// <param name="bookerId">Identifiant du booker</param>
    /// <param name="matchQuality">Qualité du match (0-100)</param>
    /// <param name="matchDescription">Description du match</param>
    void CreateMemoryFromMatch(string bookerId, int matchQuality, string matchDescription);

    /// <summary>
    /// Détermine si le booker recommande un push pour un worker
    /// </summary>
    /// <param name="bookerId">Identifiant du booker</param>
    /// <param name="workerId">Identifiant du worker</param>
    /// <param name="workerPopularity">Popularité actuelle (0-100)</param>
    /// <param name="workerSkill">Compétence technique (0-100)</param>
    /// <returns>True si le push est recommandé</returns>
    bool ShouldPushWorker(
        string bookerId,
        string workerId,
        int workerPopularity,
        int workerSkill);

    /// <summary>
    /// Applique le decay naturel aux mémoires d'un booker
    /// Réduit RecallStrength de toutes les mémoires
    /// </summary>
    /// <param name="bookerId">Identifiant du booker</param>
    /// <param name="weeksPassed">Nombre de semaines écoulées</param>
    void ApplyMemoryDecay(string bookerId, int weeksPassed = 1);

    /// <summary>
    /// Récupère les mémoires influentes pour une décision
    /// (mémoires fortes et récentes)</summary>
    /// <param name="bookerId">Identifiant du booker</param>
    /// <returns>Liste des mémoires influentes</returns>
    List<BookerMemory> GetInfluentialMemories(string bookerId);

    /// <summary>
    /// Calcule le score de cohérence du booker basé sur ses mémoires
    /// Un booker cohérent a des mémoires alignées avec ses préférences
    /// </summary>
    /// <param name="bookerId">Identifiant du booker</param>
    /// <returns>Score de cohérence (0-100)</returns>
    int CalculateBookerConsistency(string bookerId);

    /// <summary>
    /// Génère un booking automatique complet pour un show
    /// Utilise les préférences du booker, ses mémoires et les contraintes de l'owner
    /// </summary>
    /// <param name="bookerId">Identifiant du booker</param>
    /// <param name="showContext">Contexte du show à booker</param>
    /// <param name="existingSegments">Segments déjà créés par le joueur (optionnel)</param>
    /// <param name="constraints">Contraintes imposées par l'Owner (optionnel)</param>
    /// <returns>Liste de segments générés automatiquement</returns>
    List<Models.SegmentDefinition> GenerateAutoBooking(
        string bookerId,
        Models.ShowContext showContext,
        List<Models.SegmentDefinition>? existingSegments = null,
        AutoBookingConstraints? constraints = null);

    /// <summary>
    /// Obtient un rapport sur l'humeur du roster
    /// </summary>
    /// <param name="companyId">Identifiant de la compagnie</param>
    /// <returns>Rapport sur l'humeur du roster</returns>
    RingGeneral.Core.Models.Decisions.RosterMoodReport GetRosterMoodReport(string companyId);

    /// <summary>
    /// Détermine si le booker devrait s'adapter à une tendance
    /// </summary>
    /// <param name="companyId">Identifiant de la compagnie</param>
    /// <param name="trend">Tendance à évaluer</param>
    /// <returns>True si adaptation recommandée</returns>
    bool ShouldAdaptToTrend(string companyId, RingGeneral.Core.Models.Trends.Trend trend);

    /// <summary>
    /// Calcule le risque d'adaptation à une tendance
    /// </summary>
    /// <param name="companyId">Identifiant de la compagnie</param>
    /// <param name="trend">Tendance à évaluer</param>
    /// <returns>Score de risque (0-100)</returns>
    double CalculateAdaptationRisk(string companyId, RingGeneral.Core.Models.Trends.Trend trend);

    /// <summary>
    /// Évalue la stratégie à long terme basée sur l'archétype créatif du booker
    /// </summary>
    /// <param name="bookerId">Identifiant du booker</param>
    /// <param name="context">Contexte du show actuel</param>
    /// <returns>Description de la stratégie recommandée</returns>
    string EvaluateLongTermStrategy(string bookerId, RingGeneral.Core.Models.ShowContext context);
}
