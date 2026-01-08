using RingGeneral.Core.Models.Relations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository pour la gestion du népotisme et des décisions biaisées.
/// </summary>
public interface INepotismRepository
{
    // === WorkerRelation Nepotism Extensions ===

    /// <summary>
    /// Met à jour les attributs de népotisme d'une relation.
    /// </summary>
    Task UpdateRelationNepotismAttributesAsync(int relationId, bool isHidden, int biasStrength, string? originEvent, string? lastImpact);

    /// <summary>
    /// Récupère toutes les relations avec biais fort pour un décideur.
    /// </summary>
    Task<List<WorkerRelation>> GetStrongBiasRelationsAsync(string decisionMakerId, int minBiasStrength = 70);

    // === NepotismImpact ===

    /// <summary>
    /// Enregistre un impact de népotisme.
    /// </summary>
    Task SaveNepotismImpactAsync(NepotismImpact impact);

    /// <summary>
    /// Récupère tous les impacts de népotisme pour une relation.
    /// </summary>
    Task<List<NepotismImpact>> GetImpactsByRelationAsync(int relationId);

    /// <summary>
    /// Récupère tous les impacts visibles pour une compagnie.
    /// </summary>
    Task<List<NepotismImpact>> GetVisibleImpactsByCompanyAsync(string companyId);

    /// <summary>
    /// Récupère tous les impacts récents (< 30 jours).
    /// </summary>
    Task<List<NepotismImpact>> GetRecentImpactsAsync(string companyId, int daysBack = 30);

    // === BiasedDecision ===

    /// <summary>
    /// Enregistre une décision (biaisée ou non).
    /// </summary>
    Task SaveBiasedDecisionAsync(BiasedDecision decision);

    /// <summary>
    /// Récupère toutes les décisions pour une entité cible.
    /// </summary>
    Task<List<BiasedDecision>> GetDecisionsByTargetAsync(string targetEntityId);

    /// <summary>
    /// Récupère toutes les décisions biaisées pour un décideur.
    /// </summary>
    Task<List<BiasedDecision>> GetBiasedDecisionsByMakerAsync(string decisionMakerId);

    /// <summary>
    /// Compte le nombre de décisions biaisées récentes pour un décideur.
    /// </summary>
    Task<int> CountRecentBiasedDecisionsAsync(string decisionMakerId, int daysBack = 14);
}
