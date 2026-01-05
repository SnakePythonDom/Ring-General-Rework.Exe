using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class BackstageService
{
    private readonly IRandomProvider _random;
    private readonly IReadOnlyList<IncidentDefinition> _definitions;

    public BackstageService(IRandomProvider random, IReadOnlyList<IncidentDefinition> definitions)
    {
        _random = random;
        _definitions = definitions;
    }

    public IReadOnlyList<BackstageIncident> RollIncidents(
        int week,
        IReadOnlyList<WorkerBackstageProfile> roster,
        int maxIncidents = 2)
    {
        if (roster.Count == 0 || _definitions.Count == 0 || maxIncidents <= 0)
        {
            return [];
        }

        var max = Math.Min(maxIncidents, roster.Count);
        var count = _random.Next(0, max + 1);
        if (count == 0)
        {
            return [];
        }

        var incidents = new List<BackstageIncident>(count);
        var indices = new HashSet<int>();
        for (var i = 0; i < count; i++)
        {
            var workerIndex = TirerIndexUnique(roster.Count, indices);
            var worker = roster[workerIndex];
            var definition = _definitions[_random.Next(0, _definitions.Count)];
            var incidentId = $"INC-{week}-{worker.WorkerId}-{i + 1}";

            incidents.Add(new BackstageIncident(
                incidentId,
                worker.WorkerId,
                definition.IncidentType,
                definition.Description,
                definition.Severity,
                week,
                "OPEN"));
        }

        return incidents;
    }

    private int TirerIndexUnique(int max, HashSet<int> dejaUtilises)
    {
        var index = _random.Next(0, max);
        while (!dejaUtilises.Add(index))
        {
            index = _random.Next(0, max);
        }

        return index;
    }
}
