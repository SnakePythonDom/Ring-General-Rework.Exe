using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class TvDealViewModel
{
    public TvDealViewModel(TvDeal deal)
    {
        Diffuseur = deal.NetworkName;
        ReachBonus = $"+{deal.ReachBonus}";
        AudienceCap = deal.AudienceCap.ToString();
        RevenuBase = deal.BaseRevenue.ToString("C");
        RevenuParPoint = deal.RevenuePerPoint.ToString("C");
        Contraintes = deal.Constraints;
    }

    public string Diffuseur { get; }
    public string ReachBonus { get; }
    public string AudienceCap { get; }
    public string RevenuBase { get; }
    public string RevenuParPoint { get; }
    public string Contraintes { get; }
}
