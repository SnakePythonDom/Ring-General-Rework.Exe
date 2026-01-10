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
            networks.Add(new AvailableNetwork
            {
                NetworkId = "NET_LOCAL_1",
                NetworkName = "Local Sports Network",
                Prestige = 25,
                Reach = 5,
                MinimumCompanyPrestige = 0,
                MinimumShowQuality = 50,
                MinimumRosterSize = 10,
                Description = "Réseau local pour débutants"
            });
        }

        if (company.Prestige >= 20)
        {
            networks.Add(new AvailableNetwork
            {
                NetworkId = "NET_REGIONAL_1",
                NetworkName = "Regional Cable Network",
                Prestige = 40,
                Reach = 15,
                MinimumCompanyPrestige = 20,
                MinimumShowQuality = 60,
                MinimumRosterSize = 20,
                Description = "Réseau régional avec bonne couverture"
            });
        }

        // Prestige 30-60: Networks nationaux moyens
        if (company.Prestige >= 30)
        {
            networks.Add(new AvailableNetwork
            {
                NetworkId = "NET_NATIONAL_1",
                NetworkName = "National Sports Channel",
                Prestige = 55,
                Reach = 30,
                MinimumCompanyPrestige = 30,
                MinimumShowQuality = 65,
                MinimumRosterSize = 30,
                Description = "Chaîne sportive nationale"
            });
        }

        if (company.Prestige >= 40)
        {
            networks.Add(new AvailableNetwork
            {
                NetworkId = "NET_CABLE_1",
                NetworkName = "Premium Cable Network",
                Prestige = 65,
                Reach = 40,
                MinimumCompanyPrestige = 40,
                MinimumShowQuality = 70,
                MinimumRosterSize = 35,
                Description = "Réseau câble premium"
            });
        }

        // Prestige 60-80: Networks majeurs
        if (company.Prestige >= 60)
        {
            networks.Add(new AvailableNetwork
            {
                NetworkId = "NET_MAJOR_1",
                NetworkName = "Major Sports Network",
                Prestige = 75,
                Reach = 50,
                MinimumCompanyPrestige = 60,
                MinimumShowQuality = 75,
                MinimumRosterSize = 40,
                Description = "Réseau sportif majeur"
            });
        }

        if (company.Prestige >= 70)
        {
            networks.Add(new AvailableNetwork
            {
                NetworkId = "NET_PRIME_1",
                NetworkName = "Prime Time Network",
                Prestige = 85,
                Reach = 60,
                MinimumCompanyPrestige = 70,
                MinimumShowQuality = 80,
                MinimumRosterSize = 45,
                Description = "Réseau prime time"
            });
        }

        // Prestige 80+: Networks premium
        if (company.Prestige >= 80)
        {
            networks.Add(new AvailableNetwork
            {
                NetworkId = "NET_PREMIUM_1",
                NetworkName = "Elite Sports Network",
                Prestige = 95,
                Reach = 70,
                MinimumCompanyPrestige = 80,
                MinimumShowQuality = 85,
                MinimumRosterSize = 50,
                Description = "Réseau sportif d'élite"
            });
        }

        if (company.Prestige >= 90)
        {
            networks.Add(new AvailableNetwork
            {
                NetworkId = "NET_ELITE_1",
                NetworkName = "Worldwide Sports Network",
                Prestige = 100,
                Reach = 80,
                MinimumCompanyPrestige = 90,
                MinimumShowQuality = 90,
                MinimumRosterSize = 55,
                Description = "Réseau sportif mondial"
            });
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

        return new TvDealOffer
        {
            NetworkId = networkId,
            WeeklyPayment = baseWeeklyPayment,
            BaseRevenue = baseWeeklyPayment * 0.7m, // Base revenue = 70% du paiement hebdomadaire
            RevenuePerPoint = baseWeeklyPayment * 0.01m, // Revenue per point = 1% du paiement hebdomadaire
            ReachBonus = network.Reach,
            AudienceCap = network.Prestige * 1000, // Audience cap basé sur prestige
            MinimumAudience = network.MinimumShowQuality * 100, // Minimum audience
            Constraints = $"Minimum {network.MinimumShowQuality} show quality, {network.MinimumRosterSize} workers minimum"
        };
    }

    public NegotiationResult NegotiateDeal(string networkId, string companyId, TvDealOffer currentOffer, decimal requestedIncreasePercent)
    {
        var company = _companyRepository.ChargerEtatCompagnie(companyId);
        if (company == null)
        {
            return new NegotiationResult
            {
                IsAccepted = false,
                CounterOffer = null,
                Message = "Compagnie introuvable"
            };
        }

        var network = GetAvailableNetworks(companyId).FirstOrDefault(n => n.NetworkId == networkId);
        if (network == null)
        {
            return new NegotiationResult
            {
                IsAccepted = false,
                CounterOffer = null,
                Message = "Network introuvable"
            };
        }

        // Calculer probabilité d'acceptation selon prestige et augmentation demandée
        var acceptanceProbability = requestedIncreasePercent switch
        {
            <= 10m => company.Prestige >= 60 ? 0.70 : 0.40,
            <= 20m => company.Prestige >= 75 ? 0.40 : 0.15,
            <= 30m => company.Prestige >= 85 ? 0.10 : 0.0,
            _ => 0.0
        };

        var randomValue = _random.NextDouble();
        if (randomValue <= acceptanceProbability)
        {
            // Accepté
            var newOffer = new TvDealOffer
            {
                NetworkId = currentOffer.NetworkId,
                WeeklyPayment = currentOffer.WeeklyPayment * (1m + requestedIncreasePercent / 100m),
                BaseRevenue = currentOffer.BaseRevenue * (1m + requestedIncreasePercent / 100m),
                RevenuePerPoint = currentOffer.RevenuePerPoint * (1m + requestedIncreasePercent / 100m),
                ReachBonus = currentOffer.ReachBonus,
                AudienceCap = currentOffer.AudienceCap,
                MinimumAudience = currentOffer.MinimumAudience,
                Constraints = currentOffer.Constraints
            };

            return new NegotiationResult
            {
                IsAccepted = true,
                CounterOffer = newOffer,
                Message = $"Le network accepte votre demande de +{requestedIncreasePercent}%"
            };
        }
        else
        {
            // Refusé avec contre-offre possible
            var counterOfferPercent = requestedIncreasePercent * 0.5m; // Contre-offre à 50% de la demande
            var counterOffer = new TvDealOffer
            {
                NetworkId = currentOffer.NetworkId,
                WeeklyPayment = currentOffer.WeeklyPayment * (1m + counterOfferPercent / 100m),
                BaseRevenue = currentOffer.BaseRevenue * (1m + counterOfferPercent / 100m),
                RevenuePerPoint = currentOffer.RevenuePerPoint * (1m + counterOfferPercent / 100m),
                ReachBonus = currentOffer.ReachBonus,
                AudienceCap = currentOffer.AudienceCap,
                MinimumAudience = currentOffer.MinimumAudience,
                Constraints = currentOffer.Constraints
            };

            return new NegotiationResult
            {
                IsAccepted = false,
                CounterOffer = counterOffer,
                Message = $"Le network refuse mais propose +{counterOfferPercent:F1}%"
            };
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
        var deal = new TvDeal
        {
            TvDealId = dealId,
            CompanyId = companyId,
            NetworkName = network.NetworkName,
            ReachBonus = finalOffer.ReachBonus,
            AudienceCap = finalOffer.AudienceCap,
            MinimumAudience = finalOffer.MinimumAudience,
            BaseRevenue = (double)finalOffer.BaseRevenue,
            RevenuePerPoint = (double)finalOffer.RevenuePerPoint,
            Penalty = 0.0, // Penalty initial
            Constraints = finalOffer.Constraints
        };

        // Enregistrer dans la DB
        _tvDealRepository?.EnregistrerTvDeal(deal, startWeek, endWeek);

        return deal;
    }
}
