using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using System;
using System.Linq;

namespace RingGeneral.Core.Services;

public class RecruitmentScoringService : IRecruitmentScoringService
{
    private readonly IGameRepository _repository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICatchStyleRepository _catchStyleRepository;

    public RecruitmentScoringService(
        IGameRepository repository,
        ICompanyRepository companyRepository,
        ICatchStyleRepository catchStyleRepository)
    {
        _repository = repository;
        _companyRepository = companyRepository;
        _catchStyleRepository = catchStyleRepository;
    }

    public double CalculateScore(string companyId, Worker worker)
    {
        double geoFit = GetGeoFit(companyId, worker);
        double strategicFit = GetStrategicFit(companyId, worker);

        // Weighted average
        return (geoFit * 0.45) + (strategicFit * 0.55);
    }

    public double GetGeoFit(string companyId, Worker worker)
    {
        var company = _companyRepository.ChargerEtatCompagnie(companyId);
        if (company == null) return 50.0;

        // Perfect match: same region
        if (worker.ResidenceCountry == company.Region) return 100.0;

        // Nearby match (rough logic for now: same continent/country prefix)
        if (worker.ResidenceCountry != null && company.Region != null &&
            worker.ResidenceCountry.Split('_')[0] == company.Region.Split('_')[0])
        {
            return 80.0;
        }

        return 40.0; // Distant match
    }

    public double GetStrategicFit(string companyId, Worker worker)
    {
        var company = _companyRepository.ChargerEtatCompagnie(companyId);
        if (company == null) return 50.0;

        // Default fit if no style defined
        if (string.IsNullOrEmpty(company.CatchStyleId))
        {
            // Fallback to simple average
            return (worker.InRingAttributes?.InRingAvg ?? 50 +
                    worker.EntertainmentAttributes?.EntertainmentAvg ?? 50) / 2.0;
        }

        var style = _catchStyleRepository.GetStyleByIdAsync(company.CatchStyleId).Result;
        if (style == null) return 50.0;

        double score = 0;
        double totalWeight = 0;

        // 1. Wrestling Purity -> InRing Attributes
        if (style.WrestlingPurity > 0)
        {
            double weight = style.WrestlingPurity / 100.0;
            score += (worker.InRingAttributes?.InRingAvg ?? 0) * weight;
            totalWeight += weight;
        }

        // 2. Entertainment Focus -> Entertainment Attributes
        if (style.EntertainmentFocus > 0)
        {
            double weight = style.EntertainmentFocus / 100.0;
            score += (worker.EntertainmentAttributes?.EntertainmentAvg ?? 0) * weight;
            totalWeight += weight;
        }

        // 3. Hardcore -> HardcoreBrawl & Safety
        if (style.HardcoreIntensity > 0)
        {
            double weight = style.HardcoreIntensity / 100.0;
            // Use HardcoreBrawl (p10 in repository?) or average of Brawling. 
            // Assuming HardcoreBrawl is available or fallback to Striking+PSy?
            // Let's use InRingAvg as proxy if HardcoreBrawl is not exposed directly on attributes object 
            // (Wait, WorkerInRingAttributes has: Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
            // But DomainModels WorkerSnapshot/Worker might not expose all sub-properties easily?
            // The Worker object passed in has InRingAttributes property which is `WorkerInRingAttributes`.
            // Let's check `Worker.cs` Model to be sure. 
            // Assuming it has HardcoreBrawl.
            double hardcoreStat = worker.InRingAttributes?.HardcoreBrawl ?? 50;
            double safety = worker.InRingAttributes?.Safety ?? 50;

            // High hardcore needs resilience/brawl, but safety is also good?
            // Hardcore style values brawl.
            score += hardcoreStat * weight;
            totalWeight += weight;
        }

        // 4. Lucha -> HighFlying
        if (style.LuchaInfluence > 0)
        {
            double weight = style.LuchaInfluence / 100.0;
            double highFly = worker.InRingAttributes?.HighFlying ?? 0;
            score += highFly * weight;
            totalWeight += weight;
        }

        if (totalWeight <= 0) return 50.0;

        return score / totalWeight;
    }
}
