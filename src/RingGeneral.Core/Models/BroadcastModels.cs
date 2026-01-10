namespace RingGeneral.Core.Models;

public sealed record TvDeal
{
    public required string TvDealId { get; init; }
    public required string CompanyId { get; init; }
    public required string NetworkName { get; init; }
    public int ReachBonus { get; init; }
    public int AudienceCap { get; init; }
    public int MinimumAudience { get; init; }
    public double BaseRevenue { get; init; }
    public double RevenuePerPoint { get; init; }
    public double Penalty { get; init; }
    public required string Constraints { get; init; }
}

public sealed record AudienceInputs
{
    public int Reach { get; init; }
    public int ShowScore { get; init; }
    public int Stars { get; init; }
    public int Saturation { get; init; }
}

public sealed record AudienceDetails
{
    public int Audience { get; init; }
    public int Reach { get; init; }
    public int ShowScore { get; init; }
    public int Stars { get; init; }
    public int Saturation { get; init; }
    public int ReachContribution { get; init; }
    public int ShowScoreContribution { get; init; }
    public int StarsContribution { get; init; }
    public int SaturationPenalty { get; init; }
}

public sealed record DealRevenueResult
{
    public double Revenue { get; init; }
    public double BaseRevenue { get; init; }
    public double Penalty { get; init; }
    public int AudienceUsed { get; init; }
}

public sealed record AudienceHistoryEntry
{
    public required string ShowId { get; init; }
    public int Week { get; init; }
    public int Audience { get; init; }
    public int Reach { get; init; }
    public int ShowScore { get; init; }
    public int Stars { get; init; }
    public int Saturation { get; init; }
}

/// <summary>
/// Phase 2.1 - Network disponible pour négociation
/// </summary>
public sealed record AvailableNetwork
{
    public required string NetworkId { get; init; }
    public required string NetworkName { get; init; }
    public int Prestige { get; init; }
    public int Reach { get; init; }
    public int MinimumCompanyPrestige { get; init; }
    public int MinimumShowQuality { get; init; }
    public int MinimumRosterSize { get; init; }
    public required string Description { get; init; }
}

/// <summary>
/// Phase 2.1 - Termes d'un deal TV
/// </summary>
public sealed record TvDealTerms
{
    public int DurationYears { get; init; }
    public bool IsExclusive { get; init; }
    public int ShowsPerYear { get; init; }
}

/// <summary>
/// Phase 2.1 - Offre d'un network pour un deal TV
/// </summary>
public sealed record TvDealOffer
{
    public required string NetworkId { get; init; }
    public decimal WeeklyPayment { get; init; }
    public decimal BaseRevenue { get; init; }
    public decimal RevenuePerPoint { get; init; }
    public int ReachBonus { get; init; }
    public int AudienceCap { get; init; }
    public int MinimumAudience { get; init; }
    public required string Constraints { get; init; }
}

/// <summary>
/// Phase 2.1 - Résultat d'une négociation
/// </summary>
public sealed record NegotiationResult
{
    public bool IsAccepted { get; init; }
    public TvDealOffer? CounterOffer { get; init; }
    public required string Message { get; init; }
}
