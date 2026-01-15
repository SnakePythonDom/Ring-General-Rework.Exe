using RingGeneral.Core.Models.Relations;

namespace RingGeneral.Core.Services;

/// <summary>
/// Moteur de détection et gestion du népotisme.
/// Détecte les décisions biaisées par les relations et enregistre les impacts.
/// </summary>
public interface INepotismEngine
{
    /// <summary>
    /// Vérifie si une décision est biaisée par une relation.
    /// </summary>
    /// <param name="decisionMakerId">ID du décideur (Owner, Booker, etc.)</param>
    /// <param name="targetEntityId">ID de l'entité ciblée</param>
    /// <param name="decisionType">Type de décision (Push, Sanction, etc.)</param>
    /// <returns>True si la décision est biaisée</returns>
    bool IsDecisionBiased(string decisionMakerId, string targetEntityId, string decisionType);

    /// <summary>
    /// Calcule le délai de sanction basé sur les relations.
    /// Ex: Worker fils de l'Owner → sanction retardée de 4 semaines
    /// </summary>
    /// <param name="targetEntityId">ID de l'entité à sanctionner</param>
    /// <param name="offense">Type d'offense</param>
    /// <returns>Nombre de semaines de délai</returns>
    int CalculateSanctionDelay(string targetEntityId, string offense);

    /// <summary>
    /// Détermine si un push est justifié ou biaisé.
    /// </summary>
    /// <param name="workerId">ID du worker</param>
    /// <param name="currentPopularity">Popularité actuelle</param>
    /// <param>
/// <param name="currentSkill">Skill actuel</param>
    /// <returns>True si le push est justifié</returns>
    bool IsPushJustified(string workerId, int currentPopularity, int currentSkill);

    /// <summary>
    /// Enregistre un impact de népotisme.
    /// </summary>
    /// <param name="relationId">ID de la relation concernée</param>
    /// <param name="impactType">Type d'impact (Push, Protection, etc.)</param>
    /// <param name="targetEntityId">ID de l'entité affectée</param>
    /// <param name="decisionMakerId">ID du décideur</param>
    /// <param name="severity">Sévérité (1-5)</param>
    /// <param name="description">Description de l'impact</param>
    void LogNepotismImpact(
        int relationId,
        string impactType,
        string targetEntityId,
        string decisionMakerId,
        int severity,
        string? description = null);

    /// <summary>
    /// Enregistre une décision (biaisée ou non).
    /// </summary>
    void LogDecision(
        string decisionType,
        string targetEntityId,
        string decisionMakerId,
        bool isBiased,
        string? biasReason = null,
        string? justification = null);
}
