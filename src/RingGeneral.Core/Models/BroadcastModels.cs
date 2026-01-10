namespace RingGeneral.Core.Models;

public sealed record TvDeal(
    string TvDealId,
    string CompanyId,
    string NetworkName,
    int ReachBonus,
    int AudienceCap,
    int MinimumAudience,
    double BaseRevenue,
    double RevenuePerPoint,
    double Penalty,
    string Constraints);

public sealed record AudienceInputs(
    int Reach,
    int ShowScore,
    int Stars,
    int Saturation);

public sealed record AudienceDetails(
    int Audience,
    int Reach,
    int ShowScore,
    int Stars,
    int Saturation,
    int ReachContribution,
    int ShowScoreContribution,
    int StarsContribution,
    int SaturationPenalty);

public sealed record DealRevenueResult(
    double Revenue,
    double BaseRevenue,
    double Penalty,
    int AudienceUsed);

public sealed record AudienceHistoryEntry(
    string ShowId,
    int Week,
    int Audience,
    int Reach,
    int ShowScore,
    int Stars,
    int Saturation);

/// <summary>
/// Phase 2.1 - Network disponible pour négociation
/// </summary>
public sealed record AvailableNetwork(
    string NetworkId,
    string NetworkName,
    int Prestige,
    int Reach,
    int MinimumCompanyPrestige,
    int MinimumShowQuality,
    int MinimumRosterSize,
    string Description);

/// <summary>
/// Phase 2.1 - Termes d'un deal TV
/// </summary>
public sealed record TvDealTerms(
    int DurationYears,
    bool IsExclusive,
    int ShowsPerYear);

/// <summary>
/// Phase 2.1 - Offre d'un network pour un deal TV
/// </summary>
public sealed record TvDealOffer(
    string NetworkId,
    decimal WeeklyPayment,
    decimal BaseRevenue,
    decimal RevenuePerPoint,
    int ReachBonus,
    int AudienceCap,
    int MinimumAudience,
    string Constraints);

/// <summary>
/// Phase 2.1 - Résultat d'une négociation
/// </summary>
public sealed record NegotiationResult(
    bool IsAccepted,
    TvDealOffer? CounterOffer,
    string Message);

/// <summary>
/// Phase 2.1 - Network disponible pour négociation
/// </summary>
public sealed record AvailableNetwork(
    string NetworkId,
    string NetworkName,
    int Prestige,
    int Reach,
    int MinimumCompanyPrestige,
    int MinimumShowQuality,
    int MinimumRosterSize,
    string Description);

/// <summary>
/// Phase 2.1 - Termes d'un deal TV
/// </summary>
public sealed record TvDealTerms(
    int DurationYears,
    bool IsExclusive,
    int ShowsPerYear);

/// <summary>
/// Phase 2.1 - Offre d'un network pour un deal TV
/// </summary>
public sealed record TvDealOffer(
    string NetworkId,
    decimal WeeklyPayment,
    decimal BaseRevenue,
    decimal RevenuePerPoint,
    int ReachBonus,
    int AudienceCap,
    int MinimumAudience,
    string Constraints);

/// <summary>
/// Phase 2.1 - Résultat d'une négociation
/// </summary>
public sealed record NegotiationResult(
    bool IsAccepted,
    TvDealOffer? CounterOffer,
    string Message);
