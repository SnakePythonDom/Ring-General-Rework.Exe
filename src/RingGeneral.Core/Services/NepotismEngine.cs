using RingGeneral.Core.Models.Relations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

/// <summary>
/// Implémentation du moteur de népotisme.
/// Détecte les décisions biaisées et enregistre les impacts pour tracking.
/// </summary>
public class NepotismEngine : INepotismEngine
{
    // Pour l'instant, stockage en mémoire (sera remplacé par repository)
    private readonly List<NepotismImpact> _impacts = new();
    private readonly List<BiasedDecision> _decisions = new();

    public bool IsDecisionBiased(string decisionMakerId, string targetEntityId, string decisionType)
    {
        // TODO: Récupérer les relations depuis le repository
        // Pour l'instant, retourne false (implémentation minimale)
        return false;
    }

    public int CalculateSanctionDelay(string targetEntityId, string offense)
    {
        // TODO: Récupérer les relations du targetEntity
        // Pour l'instant, retourne 0 (pas de délai)
        return 0;
    }

    public bool IsPushJustified(string workerId, int currentPopularity, int currentSkill)
    {
        // Un push est justifié si:
        // - Popularity >= 70 OU
        // - Skill >= 75 OU
        // - Les deux sont au-dessus de la moyenne (50)

        if (currentPopularity >= 70) return true;
        if (currentSkill >= 75) return true;
        if (currentPopularity >= 50 && currentSkill >= 50) return true;

        return false; // Push NON justifié (probablement népotisme)
    }

    public void LogNepotismImpact(
        int relationId,
        string impactType,
        string targetEntityId,
        string decisionMakerId,
        int severity,
        string? description = null)
    {
        var impact = new NepotismImpact
        {
            RelationId = relationId,
            ImpactType = impactType,
            TargetEntityId = targetEntityId,
            DecisionMakerId = decisionMakerId,
            Severity = Math.Clamp(severity, 1, 5),
            IsVisible = severity >= 3, // Visible si sévérité >= 3
            Description = description,
            CreatedAt = DateTime.Now
        };

        _impacts.Add(impact);

        // TODO: Persister dans la base de données via repository
    }

    public void LogDecision(
        string decisionType,
        string targetEntityId,
        string decisionMakerId,
        bool isBiased,
        string? biasReason = null,
        string? justification = null)
    {
        var decision = new BiasedDecision
        {
            DecisionType = decisionType,
            TargetEntityId = targetEntityId,
            DecisionMakerId = decisionMakerId,
            IsBiased = isBiased,
            BiasReason = biasReason,
            Justification = justification,
            CreatedAt = DateTime.Now
        };

        _decisions.Add(decision);

        // TODO: Persister dans la base de données via repository
    }

    // ====================================================================
    // HELPER METHODS (pour usage futur)
    // ====================================================================

    /// <summary>
    /// Récupère tous les impacts visibles (pour UI)
    /// </summary>
    public List<NepotismImpact> GetVisibleImpacts()
    {
        return _impacts.Where(i => i.IsVisible).OrderByDescending(i => i.CreatedAt).ToList();
    }

    /// <summary>
    /// Récupère les décisions biaisées récentes (pour génération de rumeurs)
    /// </summary>
    public List<BiasedDecision> GetRecentBiasedDecisions(int days = 30)
    {
        var cutoffDate = DateTime.Now.AddDays(-days);
        return _decisions
            .Where(d => d.IsBiased && d.CreatedAt >= cutoffDate)
            .OrderByDescending(d => d.CreatedAt)
            .ToList();
    }
}
