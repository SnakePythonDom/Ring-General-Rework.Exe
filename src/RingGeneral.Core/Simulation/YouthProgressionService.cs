using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.ChildCompany;

namespace RingGeneral.Core.Simulation;

public sealed class YouthProgressionService
{
    private readonly IRandomProvider _random;
    private readonly YouthSpec _spec;
    private readonly IChildCompanyStaffService? _staffService;

    public YouthProgressionService(
        IRandomProvider random,
        YouthSpec spec,
        IChildCompanyStaffService? staffService = null)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
        _staffService = staffService; // Optionnel pour compatibilité ascendante
    }

    public YouthProgressionReport AppliquerProgression(int semaine, IReadOnlyList<YouthTraineeProgressionState> trainees)
    {
        var resultats = new List<YouthTraineeProgressionResult>();
        foreach (var trainee in trainees)
        {
            var (deltaInRing, deltaEntertainment, deltaStory) = CalculerGains(trainee);
            var inRing = Clamp(trainee.InRing + deltaInRing);
            var entertainment = Clamp(trainee.Entertainment + deltaEntertainment);
            var story = Clamp(trainee.Story + deltaStory);
            var diplome = EstDiplome(trainee, semaine, inRing, entertainment, story);

            resultats.Add(new YouthTraineeProgressionResult(
                trainee.WorkerId,
                trainee.YouthId,
                trainee.Nom,
                inRing,
                entertainment,
                story,
                inRing - trainee.InRing,
                entertainment - trainee.Entertainment,
                story - trainee.Story,
                diplome));
        }

        return new YouthProgressionReport(semaine, resultats);
    }

    private (int DeltaInRing, int DeltaEntertainment, int DeltaStory) CalculerGains(YouthTraineeProgressionState trainee)
    {
        var chance = _spec.Progression.ChanceBase;
        chance += Math.Max(0, trainee.NiveauEquipements - 1) * _spec.Progression.BonusInfrastructureParNiveau;
        chance += Math.Max(0, trainee.QualiteCoaching - 10) * _spec.Progression.BonusCoachingParPoint;

        var palier = _spec.Structures.Budget.Paliers
            .FirstOrDefault(item => trainee.BudgetAnnuel >= item.Min && trainee.BudgetAnnuel <= item.Max);
        if (palier is not null)
        {
            chance += palier.BonusChance;
        }

        chance = Math.Clamp(chance, 0, 1);
        var focus = _spec.Structures.Philosophie.TryGetValue(trainee.Philosophie, out var philosophie)
            ? philosophie
            : new YouthPhilosophyFocus(1, 1, 1);

        var gains = new List<(string Attr, double Weight)>
        {
            ("inring", focus.InRing),
            ("entertainment", focus.Entertainment),
            ("story", focus.Story)
        };

        var deltas = new Dictionary<string, int>
        {
            ["inring"] = 0,
            ["entertainment"] = 0,
            ["story"] = 0
        };

        foreach (var (attr, weight) in gains)
        {
            if (_random.NextDouble() <= chance * weight)
            {
                deltas[attr] = 1;
            }
        }

        var totalGains = deltas.Values.Sum();
        if (totalGains > _spec.Progression.MaxGainParSemaine)
        {
            var ordered = gains
                .OrderByDescending(item => item.Weight)
                .Select(item => item.Attr)
                .ToList();
            var allowed = new HashSet<string>(ordered.Take(_spec.Progression.MaxGainParSemaine));
            foreach (var key in deltas.Keys.ToList())
            {
                if (!allowed.Contains(key))
                {
                    deltas[key] = 0;
                }
            }
        }

        return (deltas["inring"], deltas["entertainment"], deltas["story"]);
    }

    private bool EstDiplome(YouthTraineeProgressionState trainee, int semaine, int inRing, int entertainment, int story)
    {
        if (string.Equals(trainee.Statut, "GRADUE", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var semaines = Math.Max(0, semaine - trainee.SemaineInscription + 1);
        if (semaines < _spec.Graduation.MinSemaines)
        {
            return false;
        }

        var moyenne = (inRing + entertainment + story) / 3.0;
        return moyenne >= _spec.Graduation.SeuilMoyen
               && inRing >= _spec.Graduation.SeuilInRing
               && entertainment >= _spec.Graduation.SeuilEntertainment
               && story >= _spec.Graduation.SeuilStory;
    }

    private int Clamp(int value) => Math.Clamp(value, 1, _spec.Progression.CapAttribut);
}
