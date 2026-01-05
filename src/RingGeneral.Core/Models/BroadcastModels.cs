namespace RingGeneral.Core.Models;

public sealed record TvDealDefinition(
    string TvDealId,
    string CompagnieId,
    string Nom,
    int Reach,
    int AudienceCap,
    double RevenueBase,
    double RevenueParPoint,
    IReadOnlyList<string> Contraintes);

public sealed record AudienceResult(
    int Audience,
    int ReachScore,
    int ShowScore,
    int Stars,
    int SaturationImpact,
    int AudienceCap);

public sealed record DealRevenueResult(
    double RevenueBase,
    double RevenueVariable,
    double RevenueTotale,
    int AudienceEffective,
    int AudienceCap,
    IReadOnlyList<string> Contraintes);
