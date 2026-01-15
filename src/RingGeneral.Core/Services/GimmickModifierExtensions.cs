using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

/// <summary>
/// Extension methods for applying gimmick modifiers to match/segment calculations
/// </summary>
public static class GimmickModifierExtensions
{
    /// <summary>
    /// Calculate total gimmick modifier for a segment/match
    /// </summary>
    public static GimmickModifierResult CalculateSegmentGimmickModifiers(
        this IGimmickService gimmickService,
        IEnumerable<Worker> participants,
        bool isMainEvent,
        bool isHeelTerritory = false)
    {
        int totalEntertainmentBonus = 0;
        int totalCrowdBonus = 0;
        int synergyBonus = 0;
        var bonusBreakdown = new List<string>();

        var workerList = participants.ToList();

        // Calculate individual gimmick bonuses
        foreach (var worker in workerList)
        {
            int entertainment = gimmickService.CalculateGimmickEntertainmentBonus(worker);
            int crowd = gimmickService.CalculateGimmickCrowdBonus(worker, isHeelTerritory);

            totalEntertainmentBonus += entertainment;
            totalCrowdBonus += crowd;

            if (entertainment != 0 || crowd != 0)
            {
                bonusBreakdown.Add($"{worker.Name}: Ent {(entertainment >= 0 ? "+" : "")}{entertainment}, Crowd {(crowd >= 0 ? "+" : "")}{crowd}");
            }
        }

        // Calculate synergy between participants (pairs)
        if (workerList.Count >= 2)
        {
            for (int i = 0; i < workerList.Count - 1; i++)
            {
                for (int j = i + 1; j < workerList.Count; j++)
                {
                    int synergy = gimmickService.CalculateGimmickSynergy(workerList[i], workerList[j]);
                    synergyBonus += synergy;

                    if (synergy > 0)
                    {
                        bonusBreakdown.Add($"{workerList[i].Name} vs {workerList[j].Name}: Synergy +{synergy}");
                    }
                }
            }
        }

        // Main event bonus amplification
        if (isMainEvent)
        {
            totalEntertainmentBonus = (int)(totalEntertainmentBonus * 1.2);
            totalCrowdBonus = (int)(totalCrowdBonus * 1.2);
            synergyBonus = (int)(synergyBonus * 1.2);
        }

        return new GimmickModifierResult
        {
            EntertainmentBonus = totalEntertainmentBonus,
            CrowdBonus = totalCrowdBonus,
            SynergyBonus = synergyBonus,
            TotalBonus = totalEntertainmentBonus + totalCrowdBonus + synergyBonus,
            BonusBreakdown = bonusBreakdown
        };
    }

    /// <summary>
    /// Apply gimmick modifiers to a base segment rating
    /// </summary>
    public static int ApplyGimmickModifiers(int baseRating, GimmickModifierResult modifiers)
    {
        // Each point of modifier affects rating by 0.5
        int adjustment = modifiers.TotalBonus / 2;

        // Clamp final rating between 10 and 100
        return Math.Clamp(baseRating + adjustment, 10, 100);
    }

    /// <summary>
    /// Calculate gimmick compatibility score for a storyline
    /// </summary>
    public static int CalculateStorylineGimmickCompatibility(
        this IGimmickService gimmickService,
        IEnumerable<Worker> protagonists,
        IEnumerable<Worker> antagonists)
    {
        int compatibility = 0;

        var heroes = protagonists.ToList();
        var villains = antagonists.ToList();

        // Face vs Heel archetype bonus
        int faceVsHeel = 0;
        foreach (var hero in heroes)
        {
            foreach (var villain in villains)
            {
                faceVsHeel += gimmickService.CalculateGimmickSynergy(hero, villain);
            }
        }
        compatibility += faceVsHeel;

        // Same-side synergy (faces working together, heels working together)
        foreach (var heroA in heroes)
        {
            foreach (var heroB in heroes.Where(h => h.Id != heroA.Id))
            {
                compatibility += gimmickService.CalculateGimmickSynergy(heroA, heroB) / 2;
            }
        }

        return compatibility;
    }

    /// <summary>
    /// Check if a gimmick turn would be beneficial (Face to Heel or vice versa)
    /// </summary>
    public static GimmickTurnAnalysis AnalyzePotentialTurn(
        this IGimmickService gimmickService,
        Worker worker,
        IGimmickRepository gimmickRepository)
    {
        var currentAlignment = worker.Alignment;
        var oppositeAlignment = currentAlignment == Alignment.Face ? "Heel" : "Face";

        // Get compatible gimmicks for the opposite alignment
        var newGimmicks = gimmickRepository.GetGimmicksByAlignment(oppositeAlignment)
            .Where(g => g.IsCompatibleWithPushLevel(worker.PushLevel))
            .Select(g => new GimmickRecommendation
            {
                Gimmick = g,
                Score = gimmickService.CalculateGimmickPotential(worker, g),
                Reason = $"Good fit for {oppositeAlignment} turn"
            })
            .OrderByDescending(r => r.Score)
            .Take(5)
            .ToList();

        // Calculate turn impact
        int crowdReactionPotential = 0;
        int freshnessFactor = 0;

        var currentGimmick = gimmickRepository.GetCurrentGimmick(worker.Id);
        if (currentGimmick != null)
        {
            // Long-running gimmicks benefit more from turns
            freshnessFactor = Math.Min(currentGimmick.DurationInWeeks / 8, 15);

            // Successful gimmicks have bigger turn impact
            if (currentGimmick.SuccessRating > 70)
                crowdReactionPotential = 20;
            else if (currentGimmick.SuccessRating < 40)
                crowdReactionPotential = 5; // Stale gimmick, turn might help
        }

        return new GimmickTurnAnalysis
        {
            RecommendedNewGimmicks = newGimmicks,
            TurnImpactScore = crowdReactionPotential + freshnessFactor,
            IsTurnRecommended = freshnessFactor > 10 || currentGimmick?.SuccessRating < 40,
            Reason = freshnessFactor > 10
                ? "Gimmick has been running long enough for an impactful turn"
                : currentGimmick?.SuccessRating < 40
                    ? "Current gimmick isn't connecting, turn could help"
                    : "Current gimmick is working well, consider carefully"
        };
    }
}

/// <summary>
/// Result of gimmick modifier calculation for a segment
/// </summary>
public class GimmickModifierResult
{
    public int EntertainmentBonus { get; set; }
    public int CrowdBonus { get; set; }
    public int SynergyBonus { get; set; }
    public int TotalBonus { get; set; }
    public List<string> BonusBreakdown { get; set; } = new();

    public string GetSummary()
    {
        if (TotalBonus == 0)
            return "No gimmick effects";

        return $"Gimmick Modifier: {(TotalBonus >= 0 ? "+" : "")}{TotalBonus} " +
               $"(Ent: {EntertainmentBonus}, Crowd: {CrowdBonus}, Synergy: {SynergyBonus})";
    }
}

/// <summary>
/// Analysis of potential alignment turn
/// </summary>
public class GimmickTurnAnalysis
{
    public List<GimmickRecommendation> RecommendedNewGimmicks { get; set; } = new();
    public int TurnImpactScore { get; set; }
    public bool IsTurnRecommended { get; set; }
    public string Reason { get; set; } = string.Empty;
}
