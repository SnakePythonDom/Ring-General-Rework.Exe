using System;
using System.Collections.Generic;
using System.Linq;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class YouthProgressionService
{
    private readonly IRandomProvider _random;
    private readonly YouthSpec _spec;

    public YouthProgressionService(IRandomProvider random, YouthSpec spec)
    {
        _random = random;
        _spec = spec;
    }

    public YouthProgressionReport SimulerSemaine(int semaine, IReadOnlyList<YouthStructureState> structures, IReadOnlyList<YouthTraineeProgressionState> trainees, int seed)
    {
        _random.Reseed(seed);
        var structuresMap = structures.ToDictionary(s => s.YouthId, StringComparer.OrdinalIgnoreCase);
        var updates = new List<YouthProgressionUpdate>();

        foreach (var trainee in trainees)
        {
            if (!structuresMap.TryGetValue(trainee.YouthId, out var structure))
            {
                continue;
            }

            if (!string.Equals(trainee.Statut, "EN_FORMATION", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var gainTotal = CalculerGainHebdo(structure);
            var distribution = ConstruireDistribution(structure);

            var deltaInRing = CalculerDelta(gainTotal, distribution.GetValueOrDefault("in_ring", 0));
            var deltaEntertainment = CalculerDelta(gainTotal, distribution.GetValueOrDefault("entertainment", 0));
            var deltaStory = CalculerDelta(gainTotal, distribution.GetValueOrDefault("story", 0));

            var nouveauInRing = trainee.InRing + deltaInRing;
            var nouveauEntertainment = trainee.Entertainment + deltaEntertainment;
            var nouveauStory = trainee.Story + deltaStory;

            var estGradue = VerifierGraduation(nouveauInRing, nouveauEntertainment, nouveauStory);

            updates.Add(new YouthProgressionUpdate(
                trainee.WorkerId,
                trainee.YouthId,
                nouveauInRing,
                nouveauEntertainment,
                nouveauStory,
                deltaInRing,
                deltaEntertainment,
                deltaStory,
                estGradue));
        }

        return new YouthProgressionReport(semaine, updates);
    }

    private double CalculerGainHebdo(YouthStructureState structure)
    {
        var infraBonus = Math.Max(0, structure.NiveauEquipements - 1) * _spec.Progression.BonusInfrastructureParNiveau;
        var coachingBonus = Math.Max(0, structure.QualiteCoaching - 10) * _spec.Progression.BonusCoachingParPoint;
        var budgetBonus = _spec.Progression.BonusBudget
            .FirstOrDefault(tier => structure.BudgetAnnuel >= tier.Min && structure.BudgetAnnuel <= tier.Max)?.Bonus ?? 0;

        var total = _spec.Progression.GainBase + infraBonus + coachingBonus + budgetBonus;
        return Math.Clamp(total, 0, _spec.Progression.GainMax);
    }

    private Dictionary<string, double> ConstruireDistribution(YouthStructureState structure)
    {
        var distribution = new Dictionary<string, double>(_spec.Progression.Distribution, StringComparer.OrdinalIgnoreCase);
        if (_spec.Progression.Philosophie.TryGetValue(structure.Philosophie, out var bonus))
        {
            distribution["in_ring"] = distribution.GetValueOrDefault("in_ring", 0) + bonus.InRing;
            distribution["entertainment"] = distribution.GetValueOrDefault("entertainment", 0) + bonus.Entertainment;
            distribution["story"] = distribution.GetValueOrDefault("story", 0) + bonus.Story;
        }

        var total = distribution.Values.Sum();
        if (total <= 0)
        {
            return new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["in_ring"] = 1,
                ["entertainment"] = 1,
                ["story"] = 1
            };
        }

        return distribution.ToDictionary(pair => pair.Key, pair => pair.Value / total, StringComparer.OrdinalIgnoreCase);
    }

    private int CalculerDelta(double gainTotal, double poids)
    {
        if (poids <= 0)
        {
            return 0;
        }

        var gain = gainTotal * poids;
        var entier = (int)Math.Floor(gain);
        var fraction = gain - entier;
        return entier + (_random.NextDouble() < fraction ? 1 : 0);
    }

    private bool VerifierGraduation(int inRing, int entertainment, int story)
    {
        var moyenne = (inRing + entertainment + story) / 3.0;
        return moyenne >= _spec.Graduation.SeuilGlobal
               && inRing >= _spec.Graduation.SeuilInRing
               && entertainment >= _spec.Graduation.SeuilEntertainment
               && story >= _spec.Graduation.SeuilStory;
    }
}
