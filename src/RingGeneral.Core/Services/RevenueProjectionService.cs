using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service pour calculer les projections de revenus
/// </summary>
public sealed class RevenueProjectionService : IRevenueProjectionService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ITvDealRepository? _tvDealRepository;

    public RevenueProjectionService(
        ICompanyRepository companyRepository,
        ITvDealRepository? tvDealRepository = null)
    {
        _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        _tvDealRepository = tvDealRepository;
    }

    public RevenueProjection ProjectRevenue(string companyId, int startMonth)
    {
        var company = _companyRepository.ChargerEtatCompagnie(companyId);
        if (company == null)
        {
            throw new InvalidOperationException($"Compagnie introuvable: {companyId}");
        }

        var monthlyRevenues = new List<MonthlyRevenue>();

        // Charger les TV deals actifs
        var tvDeals = _tvDealRepository != null
            ? LoadActiveTvDeals(companyId)
            : new List<TvDeal>();

        // Calculer les revenus pour chaque mois (12 mois)
        for (int month = 0; month < 12; month++)
        {
            var currentMonth = startMonth + month;
            
            // Revenus TV (basés sur les deals actifs)
            var tvRevenue = CalculateTvRevenue(tvDeals, currentMonth);

            // Revenus billets (avec saisonnalité)
            var ticketRevenue = CalculateTicketRevenue(company, currentMonth);

            // Revenus merch (basés sur audience moyenne)
            var merchRevenue = CalculateMerchRevenue(company, currentMonth);

            // Revenus sponsors (basés sur prestige)
            var sponsorRevenue = CalculateSponsorRevenue(company, currentMonth);

            var totalRevenue = tvRevenue + ticketRevenue + merchRevenue + sponsorRevenue;

            monthlyRevenues.Add(new MonthlyRevenue(
                currentMonth,
                tvRevenue,
                ticketRevenue,
                merchRevenue,
                sponsorRevenue,
                totalRevenue));
        }

        var totalProjected = monthlyRevenues.Sum(m => m.TotalRevenue);
        var averageMonthly = totalProjected / 12m;

        var trend = new TrendAnalysis(
            growthRate: 0.05m, // Basic growth rate
            trend: "Stable",
            volatility: 0.1m,
            recommendation: "Maintain current strategy");

        return new RevenueProjection(
            companyId: companyId,
            startMonth: startMonth,
            monthlyRevenues: monthlyRevenues,
            totalProjectedRevenue: totalProjected,
            trend: trend);
    }

    private List<TvDeal> LoadActiveTvDeals(string companyId)
    {
        // TODO: Implémenter chargement depuis repository
        // Pour l'instant, retourner liste vide
        return new List<TvDeal>();
    }

    private decimal CalculateTvRevenue(List<TvDeal> deals, int month)
    {
        // Calculer revenus TV basés sur les deals actifs
        // Chaque deal contribue selon sa base revenue + revenue per point
        decimal total = 0m;
        foreach (var deal in deals)
        {
            // Simplification: utiliser base revenue * 4 semaines par mois
            total += (decimal)deal.BaseRevenue * 4m;
        }
        return total;
    }

    private decimal CalculateTicketRevenue(CompanyState company, int month)
    {
        // Saisonnalité: été (mois 6-8) = +20%, hiver (mois 12, 1-2) = -10%
        var seasonalityMultiplier = month switch
        {
            6 or 7 or 8 => 1.2m,  // Été
            12 or 1 or 2 => 0.9m, // Hiver
            _ => 1.0m
        };

        // Base: Prestige * 1000 * capacité venue estimée
        var baseRevenue = company.Prestige * 1000m * 0.6m; // 60% remplissage moyen
        return baseRevenue * seasonalityMultiplier;
    }

    private decimal CalculateMerchRevenue(CompanyState company, int month)
    {
        // Merch basé sur audience moyenne et prestige
        var baseMerch = company.AudienceMoyenne * 5m; // $5 par spectateur moyen
        var prestigeBonus = company.Prestige * 10m;
        return baseMerch + prestigeBonus;
    }

    private decimal CalculateSponsorRevenue(CompanyState company, int month)
    {
        // Sponsors basés sur prestige et reach
        var baseSponsor = company.Prestige * 500m;
        var reachBonus = company.Reach * 200m;
        return baseSponsor + reachBonus;
    }
}
