using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Owner;
using System;
using System.Collections.Generic;

namespace RingGeneral.Core.Services;

public class OwnerGoalGenerator
{
    private readonly System.Random _random = new();

    public OwnerGoal GenerateGoal(Owner snapshot, DateTime currentDate)
    {
        var metric = SelectMetricBasedOnVision(snapshot.VisionType);
        var targetValue = CalculateTargetValue(metric, snapshot);
        var durationWeeks = _random.Next(4, 13); // 1 to 3 months

        return new OwnerGoal
        {
            OwnerId = snapshot.OwnerId,
            Description = GenerateDescription(metric, targetValue),
            Metric = metric,
            TargetValue = targetValue,
            CurrentValue = 0,
            Deadline = currentDate.AddDays(durationWeeks * 7),
            Importance = _random.Next(30, 91)
        };
    }

    private GoalMetric SelectMetricBasedOnVision(string visionType)
    {
        return visionType switch
        {
            "Profit" => GoalMetric.Revenue,
            "Growth" => GoalMetric.FanSatisfaction,
            "Prestige" => GoalMetric.AverageMatchRating,
            "Product" => GoalMetric.AverageShowRating,
            "Balanced" => (GoalMetric)_random.Next(0, 6),
            _ => GoalMetric.Revenue
        };
    }

    private double CalculateTargetValue(GoalMetric metric, Owner snapshot)
    {
        return metric switch
        {
            GoalMetric.Revenue => _random.Next(50000, 200001),
            GoalMetric.AverageMatchRating => _random.Next(65, 86),
            GoalMetric.AverageShowRating => _random.Next(60, 81),
            GoalMetric.FanSatisfaction => _random.Next(5, 16), // Growth %
            GoalMetric.RosterMorale => _random.Next(60, 81),
            _ => 100
        };
    }

    private string GenerateDescription(GoalMetric metric, double value)
    {
        return metric switch
        {
            GoalMetric.Revenue => $"Générer au moins {value:C0} de revenus.",
            GoalMetric.AverageMatchRating => $"Atteindre une note moyenne de match de {value}.",
            GoalMetric.AverageShowRating => $"Atteindre une note moyenne de show de {value}.",
            GoalMetric.FanSatisfaction => $"Augmenter la satisfaction des fans de {value}%.",
            GoalMetric.RosterMorale => $"Maintenir un moral moyen du roster de {value}.",
            _ => "Atteindre les objectifs de la direction."
        };
    }
}
