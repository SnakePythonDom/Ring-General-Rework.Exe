using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service pour calculer les projections de revenus
/// </summary>
public interface IRevenueProjectionService
{
    /// <summary>
    /// Calcule les revenus projetés sur 12 mois
    /// </summary>
    RevenueProjection ProjectRevenue(string companyId, int startMonth);
}
