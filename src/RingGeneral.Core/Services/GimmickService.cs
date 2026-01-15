using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

/// <summary>
/// Business logic service for gimmick management
/// </summary>
public class GimmickService : IGimmickService
{
    private readonly IGimmickRepository _gimmickRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IPersonalityEngine _personalityEngine;

    public GimmickService(
        IGimmickRepository gimmickRepository,
        IWorkerRepository workerRepository,
        IPersonalityEngine personalityEngine)
    {
        _gimmickRepository = gimmickRepository;
        _workerRepository = workerRepository;
        _personalityEngine = personalityEngine;
    }

    // ====================================================================
    // GIMMICK ASSIGNMENT
    // ====================================================================

    public GimmickAcceptanceResult CheckGimmickAcceptance(Worker worker, Gimmick gimmick)
    {
        var result = new GimmickAcceptanceResult();
        int baseChance = 70; // Base 70% acceptance

        // Check mental attributes
        var mental = worker.MentalAttributes;
        if (mental != null)
        {
            // High adaptability increases acceptance
            baseChance += (mental.Adaptability - 10) * 3; // -30 to +30

            // High ego decreases acceptance for "lesser" gimmicks
            if (gimmick.PopularityTierEnum < worker.PushLevel && mental.Ego > 15)
            {
                baseChance -= 20;
                result.Concerns.Add("Worker considers this gimmick beneath their status");
            }

            // Low professionalism makes changes harder
            if (mental.Professionalism < 8)
            {
                baseChance -= 15;
                result.Concerns.Add("Worker's unprofessional attitude may cause issues");
            }
        }

        // Alignment compatibility
        if (!gimmick.IsCompatibleWithAlignment(worker.Alignment))
        {
            baseChance -= 25;
            result.Concerns.Add($"Gimmick prefers {gimmick.PreferredAlignment} alignment, worker is {worker.Alignment}");
        }

        // Category compatibility with specialization
        if (worker.PrimarySpecialization != null)
        {
            string specCategory = MapSpecializationToCategory(worker.PrimarySpecialization.Specialization.ToString());
            if (specCategory == gimmick.Category)
            {
                baseChance += 15;
            }
            else
            {
                baseChance -= 10;
                result.Concerns.Add("Gimmick doesn't match worker's wrestling style");
            }
        }

        // Clamp acceptance chance
        result.AcceptanceChance = Math.Clamp(baseChance, 5, 95);
        result.WillAccept = result.AcceptanceChance >= 50;

        // Calculate morale impact
        if (result.WillAccept)
        {
            result.MoraleImpact = result.AcceptanceChance >= 80 ? 5 : 0;
            result.Message = result.AcceptanceChance >= 80
                ? $"{worker.Name} is excited about this gimmick!"
                : $"{worker.Name} accepts the gimmick change.";
        }
        else
        {
            result.MoraleImpact = -10;
            result.Message = $"{worker.Name} is reluctant about this gimmick change.";
        }

        return result;
    }

    public int CalculateGimmickPotential(Worker worker, Gimmick gimmick)
    {
        int potential = 50; // Base potential

        // Entertainment synergy
        if (worker.EntertainmentAttributes != null)
        {
            int entertainmentScore = worker.EntertainmentAttributes.EntertainmentAvg;
            if (gimmick.Category == "SHOWMAN" && entertainmentScore > 70)
                potential += 20;
            else if (gimmick.Category == "TECHNICAL" && entertainmentScore < 50)
                potential += 10; // Technical gimmicks don't need high entertainment
        }

        // Alignment match
        if (gimmick.IsCompatibleWithAlignment(worker.Alignment))
            potential += 15;

        // Push level compatibility
        if (gimmick.IsCompatibleWithPushLevel(worker.PushLevel))
            potential += 10;

        // Category match
        if (worker.PrimarySpecialization != null)
        {
            string specCategory = MapSpecializationToCategory(worker.PrimarySpecialization.Specialization.ToString());
            if (specCategory == gimmick.Category)
                potential += 20;
        }

        // Crowd reaction modifier adds to potential
        potential += gimmick.CrowdReactionModifier;

        return Math.Clamp(potential, 10, 100);
    }

    public GimmickAssignmentResult AssignGimmick(int workerId, string gimmickId, string reason, bool forceAssign = false)
    {
        var worker = _workerRepository.GetWorker(workerId);
        if (worker == null)
            return GimmickAssignmentResult.Failed("Worker not found");

        var gimmick = _gimmickRepository.GetGimmickById(gimmickId);
        if (gimmick == null)
            return GimmickAssignmentResult.Failed("Gimmick not found");

        // Check acceptance unless forced
        if (!forceAssign)
        {
            var acceptance = CheckGimmickAcceptance(worker, gimmick);
            if (!acceptance.WillAccept)
            {
                var failedResult = GimmickAssignmentResult.Failed(acceptance.Message);
                failedResult.MoraleChange = acceptance.MoraleImpact;
                return failedResult;
            }
        }

        // End current gimmick if exists
        var currentGimmick = _gimmickRepository.GetCurrentGimmick(workerId);
        if (currentGimmick != null)
        {
            _gimmickRepository.EndCurrentGimmick(workerId, 50); // Default rating
        }

        // Assign new gimmick
        _gimmickRepository.AssignGimmickToWorker(workerId, gimmickId, reason);

        var newHistory = _gimmickRepository.GetCurrentGimmick(workerId);

        return GimmickAssignmentResult.Succeeded(
            $"{worker.Name} is now using the '{gimmick.Name}' gimmick!",
            newHistory
        );
    }

    public GimmickAssignmentResult AssignCustomGimmick(int workerId, string gimmickName, string reason)
    {
        var worker = _workerRepository.GetWorker(workerId);
        if (worker == null)
            return GimmickAssignmentResult.Failed("Worker not found");

        // End current gimmick if exists
        var currentGimmick = _gimmickRepository.GetCurrentGimmick(workerId);
        if (currentGimmick != null)
        {
            _gimmickRepository.EndCurrentGimmick(workerId, 50);
        }

        // Assign custom gimmick
        _gimmickRepository.AssignCustomGimmick(workerId, gimmickName, reason);

        var newHistory = _gimmickRepository.GetCurrentGimmick(workerId);

        return GimmickAssignmentResult.Succeeded(
            $"{worker.Name} is now using the '{gimmickName}' gimmick!",
            newHistory
        );
    }

    // ====================================================================
    // RECOMMENDATIONS
    // ====================================================================

    public List<GimmickRecommendation> GetRecommendations(int workerId, int limit = 10)
    {
        var worker = _workerRepository.GetWorker(workerId);
        if (worker == null)
            return new List<GimmickRecommendation>();

        // Get base recommendations from repository
        var gimmicks = _gimmickRepository.GetRecommendedGimmicks(workerId, limit * 2);

        // Score and rank recommendations
        var recommendations = gimmicks.Select(g => new GimmickRecommendation
        {
            Gimmick = g,
            Score = CalculateGimmickPotential(worker, g),
            IsAlignmentMatch = g.IsCompatibleWithAlignment(worker.Alignment),
            IsCategoryMatch = worker.PrimarySpecialization != null &&
                MapSpecializationToCategory(worker.PrimarySpecialization.Specialization.ToString()) == g.Category,
            Reason = GenerateRecommendationReason(worker, g)
        })
        .OrderByDescending(r => r.Score)
        .Take(limit)
        .ToList();

        return recommendations;
    }

    public List<Gimmick> GetCompatibleGimmicks(Worker worker)
    {
        var alignmentGimmicks = _gimmickRepository.GetGimmicksByAlignment(worker.Alignment.ToString());

        // Further filter by push level compatibility
        return alignmentGimmicks
            .Where(g => g.IsCompatibleWithPushLevel(worker.PushLevel))
            .ToList();
    }

    // ====================================================================
    // PERFORMANCE EFFECTS
    // ====================================================================

    public int CalculateGimmickEntertainmentBonus(Worker worker)
    {
        var currentGimmick = _gimmickRepository.GetCurrentGimmick(worker.Id);
        if (currentGimmick?.GimmickId == null)
            return 0;

        var gimmick = _gimmickRepository.GetGimmickById(currentGimmick.GimmickId);
        if (gimmick == null)
            return 0;

        // Base modifier from gimmick
        int bonus = gimmick.EntertainmentModifier;

        // Tenure bonus: +1 for every 4 weeks with the gimmick (max +5)
        int tenureBonus = Math.Min(currentGimmick.DurationInWeeks / 4, 5);
        bonus += tenureBonus;

        // Alignment match bonus
        if (gimmick.IsCompatibleWithAlignment(worker.Alignment))
            bonus += 3;

        return bonus;
    }

    public int CalculateGimmickCrowdBonus(Worker worker, bool isHeelTerritory)
    {
        var currentGimmick = _gimmickRepository.GetCurrentGimmick(worker.Id);
        if (currentGimmick?.GimmickId == null)
            return 0;

        var gimmick = _gimmickRepository.GetGimmickById(currentGimmick.GimmickId);
        if (gimmick == null)
            return 0;

        int bonus = gimmick.CrowdReactionModifier;

        // Territory alignment effects
        if (worker.Alignment == Alignment.Heel && isHeelTerritory)
            bonus += 5; // Heels get cheered in heel territory
        else if (worker.Alignment == Alignment.Face && !isHeelTerritory)
            bonus += 5; // Faces get cheered in face territory

        // Mismatch penalty
        if ((worker.Alignment == Alignment.Heel && !isHeelTerritory) ||
            (worker.Alignment == Alignment.Face && isHeelTerritory))
            bonus -= 3;

        return bonus;
    }

    public int CalculateGimmickSynergy(Worker worker1, Worker worker2)
    {
        var g1 = _gimmickRepository.GetCurrentGimmick(worker1.Id);
        var g2 = _gimmickRepository.GetCurrentGimmick(worker2.Id);

        if (g1?.GimmickId == null || g2?.GimmickId == null)
            return 0;

        var gimmick1 = _gimmickRepository.GetGimmickById(g1.GimmickId);
        var gimmick2 = _gimmickRepository.GetGimmickById(g2.GimmickId);

        if (gimmick1 == null || gimmick2 == null)
            return 0;

        int synergy = 0;

        // Opposite alignments create good drama
        if ((worker1.Alignment == Alignment.Face && worker2.Alignment == Alignment.Heel) ||
            (worker1.Alignment == Alignment.Heel && worker2.Alignment == Alignment.Face))
        {
            synergy += 15;
        }

        // Same category gimmicks work well together
        if (gimmick1.Category == gimmick2.Category)
            synergy += 10;

        // Monster vs High-Flyer is classic
        if ((gimmick1.Category == "POWER" && gimmick2.Category == "HIGHFLYER") ||
            (gimmick1.Category == "HIGHFLYER" && gimmick2.Category == "POWER"))
            synergy += 12;

        // Technical vs Brawler is classic
        if ((gimmick1.Category == "TECHNICAL" && gimmick2.Category == "BRAWLER") ||
            (gimmick1.Category == "BRAWLER" && gimmick2.Category == "TECHNICAL"))
            synergy += 10;

        return synergy;
    }

    // ====================================================================
    // HISTORY & EVOLUTION
    // ====================================================================

    public void UpdateGimmickSuccess(int workerId, int performanceScore)
    {
        var currentGimmick = _gimmickRepository.GetCurrentGimmick(workerId);
        if (currentGimmick == null)
            return;

        // Calculate new success rating (moving average)
        int newRating = (currentGimmick.SuccessRating * 3 + performanceScore) / 4;
        _gimmickRepository.UpdateGimmickSuccessRating(currentGimmick.HistoryId, newRating);
    }

    public GimmickEvolutionSuggestion? CheckEvolutionOpportunity(int workerId)
    {
        var currentGimmick = _gimmickRepository.GetCurrentGimmick(workerId);
        if (currentGimmick == null)
            return null;

        // Only suggest evolution after 12+ weeks
        if (currentGimmick.DurationInWeeks < 12)
            return null;

        // Only suggest if success rating is high or very low
        if (currentGimmick.SuccessRating >= 80)
        {
            // Suggest evolution to higher tier
            var upgrades = _gimmickRepository.GetGimmicksByPopularityTier("UpperMid")
                .Take(5)
                .ToList();

            return new GimmickEvolutionSuggestion
            {
                CurrentGimmick = currentGimmick.GimmickName,
                SuggestedEvolutions = upgrades,
                Reason = "Strong performance warrants a gimmick upgrade",
                IsNaturalProgression = true
            };
        }
        else if (currentGimmick.SuccessRating <= 30)
        {
            // Suggest fresh start
            var alternatives = _gimmickRepository.GetRecommendedGimmicks(workerId, 5);

            return new GimmickEvolutionSuggestion
            {
                CurrentGimmick = currentGimmick.GimmickName,
                SuggestedEvolutions = alternatives,
                Reason = "Current gimmick isn't connecting; consider a change",
                IsNaturalProgression = false
            };
        }

        return null;
    }

    public GimmickPerformanceStats GetGimmickPerformanceStats(int workerId)
    {
        var history = _gimmickRepository.GetWorkerGimmickHistory(workerId);
        var currentGimmick = history.FirstOrDefault(h => h.IsCurrent);

        return new GimmickPerformanceStats
        {
            CurrentGimmick = currentGimmick?.GimmickName ?? "None",
            DaysWithCurrentGimmick = currentGimmick?.DurationInDays ?? 0,
            MatchesWithCurrentGimmick = 0, // Would need match history integration
            AverageMatchRating = 0,
            CrowdReactionTrend = 0,
            TotalGimmicksUsed = history.Count,
            MostSuccessfulGimmick = history.OrderByDescending(h => h.SuccessRating).FirstOrDefault()
        };
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private string MapSpecializationToCategory(string specialization)
    {
        return specialization switch
        {
            "Power" => "POWER",
            "Technical" => "TECHNICAL",
            "HighFlyer" => "HIGHFLYER",
            "Brawler" => "BRAWLER",
            "Showman" => "SHOWMAN",
            "Hardcore" => "HARDCORE",
            "AllRounder" => "ALLROUNDER",
            _ => "ALLROUNDER"
        };
    }

    private string GenerateRecommendationReason(Worker worker, Gimmick gimmick)
    {
        var reasons = new List<string>();

        if (gimmick.IsCompatibleWithAlignment(worker.Alignment))
            reasons.Add($"Matches {worker.Alignment} alignment");

        if (worker.PrimarySpecialization != null &&
            MapSpecializationToCategory(worker.PrimarySpecialization.Specialization.ToString()) == gimmick.Category)
            reasons.Add("Fits wrestling style");

        if (gimmick.EntertainmentModifier > 10)
            reasons.Add("High entertainment value");

        if (gimmick.CrowdReactionModifier > 10)
            reasons.Add("Great crowd reaction");

        return reasons.Count > 0
            ? string.Join(", ", reasons)
            : "Good potential fit";
    }
}
