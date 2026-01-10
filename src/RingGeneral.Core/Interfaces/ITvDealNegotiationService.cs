using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service pour gérer la négociation des contrats TV
/// </summary>
public interface ITvDealNegotiationService
{
    /// <summary>
    /// Récupère la liste des networks disponibles pour une compagnie
    /// </summary>
    IReadOnlyList<AvailableNetwork> GetAvailableNetworks(string companyId);

    /// <summary>
    /// Calcule l'offre initiale d'un network pour une compagnie
    /// </summary>
    TvDealOffer CalculateInitialOffer(string networkId, string companyId, TvDealTerms terms);

    /// <summary>
    /// Négocie un deal avec un network
    /// </summary>
    NegotiationResult NegotiateDeal(string networkId, string companyId, TvDealOffer currentOffer, decimal requestedIncreasePercent);

    /// <summary>
    /// Signe un deal TV
    /// </summary>
    TvDeal SignDeal(string networkId, string companyId, TvDealOffer finalOffer, TvDealTerms terms);
}
