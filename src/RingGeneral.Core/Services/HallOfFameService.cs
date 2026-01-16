using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

public class HallOfFameService : IHallOfFameService
{
    private readonly IAlumniRepository _alumniRepository;
    private readonly ITitleHistoryRepository _titleHistoryRepository;

    // Threshold for automatic eligibility
    private const int ELIGIBILITY_THRESHOLD = 100;

    public HallOfFameService(IAlumniRepository alumniRepository, ITitleHistoryRepository titleHistoryRepository)
    {
        _alumniRepository = alumniRepository;
        _titleHistoryRepository = titleHistoryRepository;
    }

    public async Task<bool> IsEligibleForHallOfFameAsync(Worker worker)
    {
        if (worker.IsHallOfFame) return false; // Already inducted
        if (worker.IsActive) return false; // Must be retired (usually)

        var result = await CalculateHallOfFameScoreAsync(worker);
        return result.Score >= ELIGIBILITY_THRESHOLD;
    }

    public async Task<(int Score, List<string> Reasons)> CalculateHallOfFameScoreAsync(Worker worker)
    {
        int score = 0;
        var reasons = new List<string>();

        // 1. Base Legacy Score (from attributes/career peak)
        score += worker.LegacyScore;
        if (worker.LegacyScore > 0)
        {
            reasons.Add($"Legacy Base: +{worker.LegacyScore}");
        }

        // 2. Title Reigns
        var titleReigns = await _titleHistoryRepository.GetWorkerTitleHistoryAsync(worker.WorkerId);

        // Count world titles (assuming we can identify them, logic placeholder)
        // For now, just count distinct titles held * arbitrary value
        int titlesCount = titleReigns.Select(tr => tr.TitleId).Distinct().Count();
        int titlePoints = titlesCount * 10;
        score += titlePoints;
        if (titlePoints > 0)
        {
            reasons.Add($"Championships ({titlesCount}): +{titlePoints}");
        }

        // Long reigns bonus
        int longReigns = titleReigns.Count(tr => tr.DaysHeldCalculated > 180);
        int longReignPoints = longReigns * 5;
        score += longReignPoints;
        if (longReignPoints > 0)
        {
            reasons.Add($"Historic Reigns (>180d): +{longReignPoints}");
        }

        // 3. Career Longevity
        int longevityPoints = worker.Experience * 2;
        score += longevityPoints;
        reasons.Add($"Career Longevity ({worker.Experience}y): +{longevityPoints}");

        // 4. Main Event status bonus
        if (worker.PushLevel == PushLevel.MainEvent)
        {
            score += 20;
            reasons.Add("Main Event Status: +20");
        }

        return (score, reasons);
    }

    public async Task<List<Worker>> InductClassAsync(string companyId, int maxInductees = 3)
    {
        var alumni = await _alumniRepository.GetCompanyAlumniAsync(companyId);
        var eligibleCandidates = new List<(Worker Worker, int Score)>();

        foreach (var worker in alumni)
        {
            if (worker.IsHallOfFame) continue;

            var (score, _) = await CalculateHallOfFameScoreAsync(worker);
            if (score >= ELIGIBILITY_THRESHOLD)
            {
                eligibleCandidates.Add((worker, score));
            }
        }

        var inductees = eligibleCandidates
            .OrderByDescending(x => x.Score)
            .Take(maxInductees)
            .Select(x => x.Worker)
            .ToList();

        foreach (var inductee in inductees)
        {
            await _alumniRepository.InductIntoHallOfFameAsync(inductee.WorkerId);
            inductee.IsHallOfFame = true;
        }

        return inductees;
    }
}
