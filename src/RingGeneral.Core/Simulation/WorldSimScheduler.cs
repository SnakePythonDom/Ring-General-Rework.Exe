using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class WorldSimScheduler
{
    private const int CoutLod0 = 14;
    private const int CoutLod1 = 6;
    private const int CoutLod2 = 2;

    private readonly IRandomProvider _random;
    private readonly WorldSimSettings _settings;

    public WorldSimScheduler(IRandomProvider random, WorldSimSettings settings)
    {
        _random = random;
        _settings = settings;
    }

    public WorldSimPlan Planifier(int semaine, string compagnieJoueurId, IReadOnlyList<CompanyState> compagnies)
    {
        var lod0 = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            compagnieJoueurId
        };

        foreach (var compagnie in compagnies
                     .Where(compagnie => compagnie.CompagnieId != compagnieJoueurId)
                     .OrderByDescending(compagnie => compagnie.Prestige)
                     .ThenBy(compagnie => compagnie.CompagnieId)
                     .Take(_settings.NbCompagniesLod0))
        {
            lod0.Add(compagnie.CompagnieId);
        }

        var lodNonMajors = semaine % _settings.FrequenceLod1Semaines == 0
            ? WorldSimLod.Resume
            : WorldSimLod.Statistique;

        if (semaine % _settings.FrequenceLod2Semaines == 0)
        {
            lodNonMajors = WorldSimLod.Statistique;
        }

        var plans = new List<WorldSimCompanyPlan>();
        foreach (var compagnie in compagnies.OrderBy(compagnie => compagnie.CompagnieId))
        {
            if (lod0.Contains(compagnie.CompagnieId))
            {
                plans.Add(new WorldSimCompanyPlan(compagnie.CompagnieId, WorldSimLod.Detail));
            }
            else
            {
                plans.Add(new WorldSimCompanyPlan(compagnie.CompagnieId, lodNonMajors));
            }
        }

        AjusterSelonBudget(plans);
        return new WorldSimPlan(semaine, plans);
    }

    private void AjusterSelonBudget(List<WorldSimCompanyPlan> plans)
    {
        var coutTotal = plans.Sum(plan => Cout(plan.NiveauDetail));
        if (coutTotal <= _settings.BudgetMsParTick)
        {
            return;
        }

        var plansLod1 = plans.Where(plan => plan.NiveauDetail == WorldSimLod.Resume).ToList();
        while (coutTotal > _settings.BudgetMsParTick && plansLod1.Count > 0)
        {
            var index = _random.Next(0, plansLod1.Count);
            var plan = plansLod1[index];
            plansLod1.RemoveAt(index);

            var position = plans.FindIndex(item => item.CompanyId == plan.CompanyId);
            plans[position] = plan with { NiveauDetail = WorldSimLod.Statistique };
            coutTotal -= (CoutLod1 - CoutLod2);
        }
    }

    private static int Cout(WorldSimLod lod) => lod switch
    {
        WorldSimLod.Detail => CoutLod0,
        WorldSimLod.Resume => CoutLod1,
        WorldSimLod.Statistique => CoutLod2,
        _ => CoutLod2
    };
}
