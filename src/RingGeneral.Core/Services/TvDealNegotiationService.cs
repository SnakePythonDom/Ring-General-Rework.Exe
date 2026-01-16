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
    private readonly IGameRepository _gameRepository;
    private readonly System.Random _random = new();

    public TvDealNegotiationService(
        ICompanyRepository companyRepository,
        IGameRepository gameRepository,
        ITvDealRepository? tvDealRepository = null)
    {
        _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
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
                Description = "Réseau local pour débutants",
                BannedContent = new List<string> { "Hardcore", "Blood" },
                PreferredContent = new List<string> { "Realism" }
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
                Description = "Réseau régional avec bonne couverture",
                BannedContent = new List<string> { "Hardcore" },
                PreferredContent = new List<string> { "Entertainment" }
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
                Description = "Chaîne sportive nationale",
                BannedContent = new List<string> { "Comedy", "Gimmick" },
                PreferredContent = new List<string> { "Technical", "Competition" }
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
                Description = "Réseau câble premium sans censure",
                BannedContent = new List<string> { "Comedy" },
                PreferredContent = new List<string> { "Edgy", "Hardcore", "Blood" }
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
                Description = "Réseau sportif majeur",
                BannedContent = new List<string> { "Hardcore", "Blood" },
                PreferredContent = new List<string> { "Mainstream", "StarPower" }
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
                Description = "Réseau prime time",
                BannedContent = new List<string> { "Blood", "Hardcore", "Edgy" },
                PreferredContent = new List<string> { "Drama", "Storyline", "Entertainment" }
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
                Description = "Réseau sportif d'élite",
                PreferredContent = new List<string> { "Technical", "PureWrestling" }
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
                Description = "Réseau sportif mondial",
                Distribution = DistributionType.International,
                SupportedRegionIds = new List<string> { "USA_EAST", "USA_WEST", "UK", "JAPAN", "MEXICO" },
                PreferredContent = new List<string> { "Spectacle", "MainEvent", "StarPower" }
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

        // Modifier selon le slot
        var slotMultiplier = terms.Slot switch
        {
            BroadcastSlot.PrimeTime => 1.4m,
            BroadcastSlot.Daytime => 0.8m,
            BroadcastSlot.LateNight => 1.0m,
            BroadcastSlot.Graveyard => 0.5m,
            _ => 1.0m
        };
        baseWeeklyPayment *= slotMultiplier;

        // Modifier selon les régions cibles (Reach cumulé)
        var regionsCount = terms.TargetRegionIds.Count;
        if (regionsCount > 1)
        {
            baseWeeklyPayment *= (1.0m + (regionsCount - 1) * 0.2m); // +20% par région supplémentaire
        }

        // Production requirement basé sur le network et le slot
        var prodRequirement = network.Prestige / 2 + (terms.Slot == BroadcastSlot.PrimeTime ? 20 : 0);

        return new TvDealOffer
        {
            NetworkId = networkId,
            WeeklyPayment = baseWeeklyPayment,
            BaseRevenue = baseWeeklyPayment * 0.7m,
            RevenuePerPoint = baseWeeklyPayment * 0.01m,
            ReachBonus = network.Reach,
            AudienceCap = network.Prestige * 1000,
            MinimumAudience = network.MinimumShowQuality * 100,
            Constraints = $"Min Quality: {network.MinimumShowQuality}, Production: {prodRequirement}%"
        };
    }

    // Dans un vrai scénario, la patience serait stockée en DB ou en session.
    // Ici on simule une réduction basée sur l'agressivité de la demande.
    public NegotiationResult NegotiateDeal(string networkId, string companyId, TvDealOffer currentOffer, decimal requestedIncreasePercent)
    {
        var company = _companyRepository.ChargerEtatCompagnie(companyId);
        if (company == null) return new NegotiationResult { IsAccepted = false, Message = "Compagnie introuvable" };

        var network = GetAvailableNetworks(companyId).FirstOrDefault(n => n.NetworkId == networkId);
        if (network == null) return new NegotiationResult { IsAccepted = false, Message = "Network introuvable" };

        // Calcul de la perte de patience (Agressivité)
        // Demander +10% est standard. Demander +30% est risqué.
        int patienceLoss = (int)(requestedIncreasePercent * 2.5m);
        // Simulation d'une patience restante (devrait être persistée)
        int currentPatience = 100 - (int)(_random.Next(10, 30)); // Valeur aléatoire pour la démo

        int remainingPatience = currentPatience - patienceLoss;

        if (remainingPatience <= 0)
        {
            return new NegotiationResult
            {
                IsAccepted = false,
                IsWalkAway = true,
                Message = "Le network a perdu patience et s'est retiré de la table des négociations.",
                RemainingPatience = 0
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
            var newOffer = currentOffer with
            {
                WeeklyPayment = currentOffer.WeeklyPayment * (1m + requestedIncreasePercent / 100m),
                BaseRevenue = currentOffer.BaseRevenue * (1m + requestedIncreasePercent / 100m),
                RevenuePerPoint = currentOffer.RevenuePerPoint * (1m + requestedIncreasePercent / 100m)
            };

            return new NegotiationResult
            {
                IsAccepted = true,
                CounterOffer = newOffer,
                Message = $"Le network accepte votre demande de +{requestedIncreasePercent}%",
                RemainingPatience = remainingPatience
            };
        }
        else
        {
            var counterOfferPercent = requestedIncreasePercent * 0.5m;
            var counterOffer = currentOffer with
            {
                WeeklyPayment = currentOffer.WeeklyPayment * (1m + counterOfferPercent / 100m),
                BaseRevenue = currentOffer.BaseRevenue * (1m + counterOfferPercent / 100m),
                RevenuePerPoint = currentOffer.RevenuePerPoint * (1m + counterOfferPercent / 100m)
            };

            return new NegotiationResult
            {
                IsAccepted = false,
                CounterOffer = counterOffer,
                Message = $"Le network refuse mais propose +{counterOfferPercent:F1}%",
                RemainingPatience = remainingPatience
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
            Penalty = 0.0,
            Constraints = finalOffer.Constraints,
            Slot = terms.Slot,
            Distribution = network.Distribution,
            ProductionRequirement = (network.Prestige / 2) + (terms.Slot == BroadcastSlot.PrimeTime ? 20 : 0),
            TargetRegionIds = terms.TargetRegionIds
        };

        // Enregistrer dans la DB
        _tvDealRepository?.EnregistrerTvDeal(deal, startWeek, endWeek);

        return deal;
    }
}
