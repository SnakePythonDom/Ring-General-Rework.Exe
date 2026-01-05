using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class DealRevenueModel
{
    public DealRevenueResult Calculer(TvDeal deal, AudienceDetails audience)
    {
        var audienceCap = deal.AudienceCap <= 0 ? 100 : deal.AudienceCap;
        var audienceUsed = Math.Clamp(audience.Audience, 0, audienceCap);
        var baseRevenue = deal.BaseRevenue + audienceUsed * deal.RevenuePerPoint;
        var penalty = audience.Audience < deal.MinimumAudience ? deal.Penalty : 0;
        var revenue = Math.Max(0, baseRevenue - penalty);

        return new DealRevenueResult(revenue, baseRevenue, penalty, audienceUsed);
    }
}
