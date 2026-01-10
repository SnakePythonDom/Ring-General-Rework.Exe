using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Models.Roster;
using System;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service pour gérer les fédérations de niche
/// </summary>
public class NicheFederationService
{
    private readonly INicheFederationRepository _nicheRepository;
    private readonly IRosterAnalysisRepository _rosterAnalysisRepository;

    public NicheFederationService(
        INicheFederationRepository nicheRepository,
        IRosterAnalysisRepository rosterAnalysisRepository)
    {
        _nicheRepository = nicheRepository ?? throw new ArgumentNullException(nameof(nicheRepository));
        _rosterAnalysisRepository = rosterAnalysisRepository ?? throw new ArgumentNullException(nameof(rosterAnalysisRepository));
    }

    /// <summary>
    /// Détermine si une fédération peut devenir une niche
    /// </summary>
    public async Task<bool> CanBecomeNicheAsync(string companyId, NicheType type)
    {
        var dna = await _rosterAnalysisRepository.GetRosterDNAByCompanyIdAsync(companyId);
        if (dna == null) return false;

        // Vérifier que l'ADN correspond au type de niche
        var stylePercentage = type switch
        {
            NicheType.Hardcore => dna.HardcorePercentage,
            NicheType.Technical => dna.TechnicalPercentage,
            NicheType.Lucha => dna.LuchaPercentage,
            NicheType.Entertainment => dna.EntertainmentPercentage,
            NicheType.StrongStyle => dna.StrongStylePercentage,
            _ => 0
        };

        // Peut devenir niche si le style correspondant représente au moins 60% du roster
        return stylePercentage >= 60 && dna.CoherenceScore >= 70;
    }

    /// <summary>
    /// Établit une fédération comme niche
    /// </summary>
    public async Task<NicheFederationProfile> EstablishNicheAsync(string companyId, NicheType nicheType)
    {
        if (!await CanBecomeNicheAsync(companyId, nicheType))
        {
            throw new InvalidOperationException($"La compagnie {companyId} ne peut pas devenir une niche de type {nicheType}");
        }

        var profileId = Guid.NewGuid().ToString("N");
        var profile = new NicheFederationProfile
        {
            ProfileId = profileId,
            CompanyId = companyId,
            IsNicheFederation = true,
            NicheType = nicheType,
            CaptiveAudiencePercentage = 70 + new System.Random().Next(20), // 70-90%
            TvDependencyReduction = 40 + new System.Random().Next(30), // 40-70%
            MerchandiseMultiplier = 1.3 + (new System.Random().NextDouble() * 0.4), // 1.3-1.7
            TicketSalesStability = 60 + new System.Random().Next(30), // 60-90%
            TalentSalaryReduction = 20 + new System.Random().Next(20), // 20-40%
            TalentLoyaltyBonus = 30 + new System.Random().Next(30), // 30-60%
            HasGrowthCeiling = true,
            MaxSize = CompanySize.Regional, // Par défaut, plafond à Regional
            EstablishedAt = DateTime.Now,
            CeasedAt = null
        };

        await _nicheRepository.SaveNicheFederationProfileAsync(profile);
        return profile;
    }

    /// <summary>
    /// Abandonne le statut de niche
    /// </summary>
    public async Task AbandonNicheAsync(string companyId)
    {
        var profile = await _nicheRepository.GetNicheFederationProfileByCompanyIdAsync(companyId);
        if (profile == null || !profile.IsNicheFederation) return;

        var updatedProfile = new NicheFederationProfile
        {
            ProfileId = profile.ProfileId,
            CompanyId = profile.CompanyId,
            IsNicheFederation = false,
            NicheType = profile.NicheType,
            CaptiveAudiencePercentage = profile.CaptiveAudiencePercentage,
            TvDependencyReduction = profile.TvDependencyReduction,
            MerchandiseMultiplier = profile.MerchandiseMultiplier,
            TicketSalesStability = profile.TicketSalesStability,
            TalentSalaryReduction = profile.TalentSalaryReduction,
            TalentLoyaltyBonus = profile.TalentLoyaltyBonus,
            HasGrowthCeiling = profile.HasGrowthCeiling,
            MaxSize = profile.MaxSize,
            EstablishedAt = profile.EstablishedAt,
            CeasedAt = DateTime.Now
        };

        await _nicheRepository.UpdateNicheFederationProfileAsync(updatedProfile);
    }
}
