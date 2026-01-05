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
