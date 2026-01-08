using RingGeneral.Core.Models.Relations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository de népotisme (Phase 4).
/// Gère les relations, impacts et décisions biaisées.
/// </summary>
public interface INepotismRepository
{
    Task<List<Relation>> GetStrongBiasRelationsAsync(string entityId, int minBiasStrength);
    Task<List<NepotismImpact>> GetVisibleImpactsByCompanyAsync(string companyId);
    Task<List<BiasedDecision>> GetBiasedDecisionsByMakerAsync(string decisionMakerId);
    Task SaveNepotismImpactAsync(NepotismImpact impact);
    Task SaveBiasedDecisionAsync(BiasedDecision decision);
}
