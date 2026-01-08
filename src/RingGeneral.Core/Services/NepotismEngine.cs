using RingGeneral.Core.Models.Relations;
using RingGeneral.Data.Repositories;
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
    private readonly INepotismRepository? _repository;

    public NepotismEngine(INepotismRepository? repository = null)
    {
        _repository = repository;
    }

    public bool IsDecisionBiased(string decisionMakerId, string targetEntityId, string decisionType)
    {
        if (_repository == null)
            return false;

        // Récupérer les relations avec biais fort pour le décideur
        var strongBiasRelations = _repository.GetStrongBiasRelationsAsync(decisionMakerId, minBiasStrength: 40).Result;

        // Vérifier si le targetEntity est impliqué dans une de ces relations
        if (!int.TryParse(targetEntityId, out int targetId))
            return false;

        return strongBiasRelations.Any(r => r.InvolvesWorker(targetId));
    }

    public int CalculateSanctionDelay(string targetEntityId, string offense)
    {
        if (_repository == null)
            return 0;

        // Récupérer les relations avec biais fort pour le worker
        var strongBiasRelations = _repository.GetStrongBiasRelationsAsync(targetEntityId, minBiasStrength: 40).Result;

        if (!strongBiasRelations.Any())
            return 0;

        // Calculer le délai basé sur le BiasStrength le plus élevé
        var maxBias = strongBiasRelations.Max(r => r.BiasStrength);

        // Formule: BiasStrength 40-69 → 1-2 semaines, 70-89 → 3-4 semaines, 90-100 → 5-6 semaines
        if (maxBias >= 90)
            return 6;
        else if (maxBias >= 70)
            return 4;
        else if (maxBias >= 40)
            return 2;

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
        if (_repository == null)
            return;

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

        // Persister dans la base de données via repository
        _repository.SaveNepotismImpactAsync(impact).Wait();
    }

    public void LogDecision(
        string decisionType,
        string targetEntityId,
        string decisionMakerId,
        bool isBiased,
        string? biasReason = null,
        string? justification = null)
    {
        if (_repository == null)
            return;

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

        // Persister dans la base de données via repository
        _repository.SaveBiasedDecisionAsync(decision).Wait();
    }

    // ====================================================================
    // HELPER METHODS (pour usage futur)
    // ====================================================================

    /// <summary>
    /// Récupère tous les impacts visibles (pour UI)
    /// </summary>
    public List<NepotismImpact> GetVisibleImpacts(string companyId)
    {
        if (_repository == null)
            return new List<NepotismImpact>();

        return _repository.GetVisibleImpactsByCompanyAsync(companyId).Result;
    }

    /// <summary>
    /// Récupère les décisions biaisées récentes (pour génération de rumeurs)
    /// </summary>
    public List<BiasedDecision> GetRecentBiasedDecisions(string decisionMakerId, int days = 14)
    {
        if (_repository == null)
            return new List<BiasedDecision>();

        return _repository.GetBiasedDecisionsByMakerAsync(decisionMakerId).Result
            .Where(d => (DateTime.Now - d.CreatedAt).TotalDays <= days)
            .ToList();
    }
}
