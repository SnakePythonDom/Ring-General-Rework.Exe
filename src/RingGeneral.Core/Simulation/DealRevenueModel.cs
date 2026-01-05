using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class DealRevenueModel
{
    private readonly IReadOnlyDictionary<string, TvDealDefinition> _deals;

    public DealRevenueModel(IReadOnlyDictionary<string, TvDealDefinition>? deals = null)
    {
        _deals = deals ?? ChargerDealsParDefaut();
    }

    public TvDealDefinition? TrouverDeal(string? dealId)
    {
        if (string.IsNullOrWhiteSpace(dealId))
        {
            return null;
        }

        return _deals.TryGetValue(dealId, out var deal) ? deal : null;
    }

    public DealRevenueResult Calculer(TvDealDefinition deal, int audience)
    {
        var audienceEffective = deal.AudienceCap > 0 ? Math.Min(audience, deal.AudienceCap) : audience;
        var revenueVariable = audienceEffective * deal.RevenueParPoint;
        var revenueTotale = deal.RevenueBase + revenueVariable;

        return new DealRevenueResult(
            deal.RevenueBase,
            revenueVariable,
            revenueTotale,
            audienceEffective,
            deal.AudienceCap,
            deal.Contraintes);
    }

    private static IReadOnlyDictionary<string, TvDealDefinition> ChargerDealsParDefaut()
    {
        return new Dictionary<string, TvDealDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["TV-001"] = new(
                "TV-001",
                "COMP-001",
                "Ring General TV",
                12,
                95,
                5000,
                35,
                new[] { "Prime time", "Exclusivité régionale" }),
            ["TV-BASE"] = new(
                "TV-BASE",
                "GLOBAL",
                "Local Sports",
                6,
                85,
                2500,
                20,
                new[] { "Diffusion locale" })
        };
    }
}
