using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour accéder aux données des TV Deals
/// </summary>
public interface ITvDealRepository
{
    /// <summary>
    /// Enregistre un nouveau TV Deal
    /// </summary>
    void EnregistrerTvDeal(TvDeal deal, int startWeek, int endWeek);
}
