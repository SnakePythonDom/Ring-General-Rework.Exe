using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service pour gérer la négociation des contrats TV
/// </summary>
public sealed class TvDealNegotiationService : ITvDealNegotiationService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ITvDealRepository? _tvDealRepository;
    private readonly System.Random _random = new();

    public TvDealNegotiationService(
        ICompanyRepository companyRepository,
        ITvDealRepository? tvDealRepository = null)
    {
        _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        _tvDealRepository = tvDealRepository;
    }

    public IReadOnlyList<AvailableNetwork> GetAvailableNetworks(string companyId)
    {
        var company = _companyRepository.ChargerEtatCompagnie(companyId);
        if (company == null)
        {
            return Array.Empty<AvailableNetwork>();
        }

        var networks = new List<AvailableNetwork>();

        // Networks disponibles selon le prestige de la compagnie
        // Prestige 0-30: Networks locaux/régionaux
        if (company.Prestige >= 0)
        {
            networks.Add(new AvailableNetwork(
                "NET_LOCAL_1",
                "Local Sports Network",
                25,
                5,
                0,
                50,
                10,
                "Réseau local pour débutants"));
        }

        if (company.Prestige >= 20)
        {
            networks.Add(new AvailableNetwork(
                "NET_REGIONAL_1",
                "Regional Cable Network",
                40,
                15,
                20,
                60,
                20,
                "Réseau régional avec bonne couverture"));
        }

        // Prestige 30-60: Networks nationaux moyens
        if (company.Prestige >= 30)
        {
            networks.Add(new AvailableNetwork(
                "NET_NATIONAL_1",
                "National Sports Channel",
                55,
                30,
                30,
                65,
                30,
                "Chaîne sportive nationale"));
        }

        if (company.Prestige >= 40)
        {
            networks.Add(new AvailableNetwork(
                "NET_CABLE_1",
                "Premium Cable Network",
                65,
                40,
                40,
                70,
                35,
                "Réseau câble premium"));
        }

        // Prestige 60-80: Networks majeurs
        if (company.Prestige >= 60)
        {
            networks.Add(new AvailableNetwork(
                "NET_MAJOR_1",
                "Major Sports Network",
                75,
                50,
                60,
                75,
                40,
                "Réseau sportif majeur"));
        }

        if (company.Prestige >= 70)
        {
            networks.Add(new AvailableNetwork(
                "NET_PRIME_1",
                "Prime Time Network",
                85,
                60,
                70,
                80,
                45,
                "Réseau prime time"));
        }

        // Prestige 80+: Networks premium
        if (company.Prestige >= 80)
        {
            networks.Add(new AvailableNetwork(
                "NET_PREMIUM_1",
                "Elite Sports Network",
                95,
                70,
                80,
                85,
                50,
                "Réseau sportif d'élite"));
        }

        if (company.Prestige >= 90)
        {
            networks.Add(new AvailableNetwork(
                "NET_ELITE_1",
                "Worldwide Sports Network",
                100,
                80,
                90,
                90,
                55,
                "Réseau sportif mondial"));
        }

        return networks.Where(n => n.MinimumCompanyPrestige <= company.Prestige).ToList();
    }

    public TvDealOffer CalculateInitialOffer(string networkId, string companyId, TvDealTerms terms)
    {
        var company = _companyRepository.ChargerEtatCompagnie(companyId);
        if (company == null)
        {
            throw new InvalidOperationException($"Compagnie introuvable: {companyId}");
        }

        var network = GetAvailableNetworks(companyId).FirstOrDefault(n => n.NetworkId == networkId);
        if (network == null)
        {
            throw new InvalidOperationException($"Network introuvable: {networkId}");
        }

        // Calculer la qualité moyenne des shows récents
        // TODO: Implémenter ChargerShowsRecents dans ShowRepository ou utiliser une autre méthode
        // Pour l'instant, utiliser une valeur par défaut basée sur le prestige
        var averageShowQuality = Math.Max(50, company.Prestige);

        // Formule: Network Prestige * 10k + Company Prestige * 5k + Show Quality * 2k
        var baseWeeklyPayment = (network.Prestige * 10_000m) + (company.Prestige * 5_000m) + (averageShowQuality * 2_000m);

        // Modifier selon les termes
        if (terms.IsExclusive)
        {
            baseWeeklyPayment *= 1.3m; // +30% pour exclusivité
        }

        // Modifier selon nombre de shows/an
        var showsMultiplier = terms.ShowsPerYear switch
        {
            12 => 0.8m,   // 1 show/mois
            24 => 1.0m,   // 2 shows/mois
            52 => 1.2m,   // 1 show/semaine
            104 => 1.4m,  // 2 shows/semaine
            _ => 1.0m
        };
        baseWeeklyPayment *= showsMultiplier;

        // Modifier selon durée
        var durationMultiplier = terms.DurationYears switch
        {
            1 => 0.9m,
            2 => 1.0m,
            3 => 1.1m,
            4 => 1.15m,
            5 => 1.2m,
            _ => 1.0m
        };
        baseWeeklyPayment *= durationMultiplier;

        return new TvDealOffer(
            networkId,
            baseWeeklyPayment,
            baseWeeklyPayment * 0.7m, // Base revenue = 70% du paiement hebdomadaire
            baseWeeklyPayment * 0.01m, // Revenue per point = 1% du paiement hebdomadaire
            network.Reach,
            network.Prestige * 1000, // Audience cap basé sur prestige
            network.MinimumShowQuality * 100, // Minimum audience
            $"Minimum {network.MinimumShowQuality} show quality, {network.MinimumRosterSize} workers minimum");
    }

    public NegotiationResult NegotiateDeal(string networkId, string companyId, TvDealOffer currentOffer, decimal requestedIncreasePercent)
    {
        var company = _companyRepository.ChargerEtatCompagnie(companyId);
        if (company == null)
        {
            return new NegotiationResult(false, null, "Compagnie introuvable");
        }

        var network = GetAvailableNetworks(companyId).FirstOrDefault(n => n.NetworkId == networkId);
        if (network == null)
        {
            return new NegotiationResult(false, null, "Network introuvable");
        }

        // Calculer probabilité d'acceptation selon prestige et augmentation demandée
        var acceptanceProbability = requestedIncreasePercent switch
        {
            <= 10m => company.Prestige >= 60 ? 0.70 : 0.40,
            <= 20m => company.Prestige >= 75 ? 0.40 : 0.15,
            <= 30m => company.Prestige >= 85 ? 0.10 : 0.0m,
            _ => 0.0m
        };

        var randomValue = _random.NextDouble();
        if (randomValue <= acceptanceProbability)
        {
            // Accepté
            var newOffer = new TvDealOffer(
                currentOffer.NetworkId,
                currentOffer.WeeklyPayment * (1m + requestedIncreasePercent / 100m),
                currentOffer.BaseRevenue * (1m + requestedIncreasePercent / 100m),
                currentOffer.RevenuePerPoint * (1m + requestedIncreasePercent / 100m),
                currentOffer.ReachBonus,
                currentOffer.AudienceCap,
                currentOffer.MinimumAudience,
                currentOffer.Constraints);

            return new NegotiationResult(true, newOffer, $"Le network accepte votre demande de +{requestedIncreasePercent}%");
        }
        else
        {
            // Refusé avec contre-offre possible
            var counterOfferPercent = requestedIncreasePercent * 0.5m; // Contre-offre à 50% de la demande
            var counterOffer = new TvDealOffer(
                currentOffer.NetworkId,
                currentOffer.WeeklyPayment * (1m + counterOfferPercent / 100m),
                currentOffer.BaseRevenue * (1m + counterOfferPercent / 100m),
                currentOffer.RevenuePerPoint * (1m + counterOfferPercent / 100m),
                currentOffer.ReachBonus,
                currentOffer.AudienceCap,
                currentOffer.MinimumAudience,
                currentOffer.Constraints);

            return new NegotiationResult(false, counterOffer, $"Le network refuse mais propose +{counterOfferPercent:F1}%");
        }
    }

    public TvDeal SignDeal(string networkId, string companyId, TvDealOffer finalOffer, TvDealTerms terms)
    {
        var network = GetAvailableNetworks(companyId).FirstOrDefault(n => n.NetworkId == networkId);
        if (network == null)
        {
            throw new InvalidOperationException($"Network introuvable: {networkId}");
        }

        var dealId = $"TVDEAL-{Guid.NewGuid():N}".ToUpperInvariant();
        var startWeek = 1; // TODO: Récupérer la semaine actuelle depuis GameState
        var endWeek = startWeek + (terms.DurationYears * 52);

        // Créer le TvDeal
        var deal = new TvDeal(
            dealId,
            companyId,
            network.NetworkName,
            finalOffer.ReachBonus,
            finalOffer.AudienceCap,
            finalOffer.MinimumAudience,
            (double)finalOffer.BaseRevenue,
            (double)finalOffer.RevenuePerPoint,
            0.0, // Penalty initial
            finalOffer.Constraints);

        // Enregistrer dans la DB
        _tvDealRepository?.EnregistrerTvDeal(deal, startWeek, endWeek);

        return deal;
    }
}
