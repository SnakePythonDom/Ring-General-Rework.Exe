using RingGeneral.Core.Models.Crisis;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le moteur de communications backstage.
/// Gère les 4 types de communications pour résoudre les crises.
/// </summary>
public interface ICommunicationEngine
{
    /// <summary>
    /// Calcule la chance de succès d'une communication avant de l'envoyer
    /// </summary>
    /// <param name="communicationType">Type de communication</param>
    /// <param name="tone">Ton utilisé</param>
    /// <param name="crisis">Crise cible (NULL si préventif)</param>
    /// <param name="initiatorInfluence">Influence de l'initiateur (0-100)</param>
    /// <returns>Chance de succès (0-100)</returns>
    int CalculateSuccessChance(
        string communicationType,
        string tone,
        Crisis? crisis,
        int initiatorInfluence);

    /// <summary>
    /// Crée une communication
    /// </summary>
    /// <param name="companyId">Identifiant de la compagnie</param>
    /// <param name="crisisId">Identifiant de la crise (NULL si préventif)</param>
    /// <param name="communicationType">Type de communication</param>
    /// <param name="initiatorId">ID de l'initiateur</param>
    /// <param name="targetId">ID de la cible (NULL si public)</param>
    /// <param name="message">Message</param>
    /// <param name="tone">Ton</param>
    /// <returns>Communication créée</returns>
    Communication CreateCommunication(
        string companyId,
        int? crisisId,
        string communicationType,
        string initiatorId,
        string? targetId,
        string message,
        string tone);

    /// <summary>
    /// Exécute une communication et génère son résultat
    /// </summary>
    /// <param name="communicationId">ID de la communication</param>
    /// <returns>Résultat de la communication</returns>
    CommunicationOutcome ExecuteCommunication(int communicationId);

    /// <summary>
    /// Applique les impacts d'un résultat de communication
    /// </summary>
    /// <param name="outcome">Résultat à appliquer</param>
    /// <param name="crisisId">ID de la crise (si applicable)</param>
    void ApplyOutcomeEffects(CommunicationOutcome outcome, int? crisisId);

    /// <summary>
    /// Recommande le meilleur type de communication pour une crise
    /// </summary>
    /// <param name="crisis">La crise</param>
    /// <returns>Type de communication recommandé</returns>
    string RecommendCommunicationType(Crisis crisis);

    /// <summary>
    /// Recommande le meilleur ton pour une crise
    /// </summary>
    /// <param name="crisis">La crise</param>
    /// <param name="communicationType">Type de communication</param>
    /// <returns>Ton recommandé</returns>
    string RecommendTone(Crisis crisis, string communicationType);

    /// <summary>
    /// Calcule le taux de succès des communications pour une compagnie
    /// </summary>
    /// <param name="companyId">ID de la compagnie</param>
    /// <returns>Taux de succès (0-100)</returns>
    double GetCommunicationSuccessRate(string companyId);
}
