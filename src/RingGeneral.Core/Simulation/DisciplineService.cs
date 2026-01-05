using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class DisciplineService
{
    private readonly IRandomProvider _random;

    public DisciplineService(IRandomProvider random)
    {
        _random = random;
    }

    public (DisciplinaryAction Action, MoraleHistoryEntry Morale) AppliquerSanction(
        BackstageIncident incident,
        int moraleActuelle,
        int week,
        string? notes = null)
    {
        var actionType = incident.Severity switch
        {
            >= 70 => "SUSPENSION",
            >= 40 => "AMENDE",
            _ => "AVERTISSEMENT"
        };

        var (minDelta, maxDelta) = actionType switch
        {
            "SUSPENSION" => (-20, -12),
            "AMENDE" => (-12, -6),
            _ => (-5, -2)
        };

        var delta = _random.Next(minDelta, maxDelta + 1);
        var nouveau = Math.Clamp(moraleActuelle + delta, 0, 100);
        var actionId = $"DISC-{incident.IncidentId}";

        var action = new DisciplinaryAction(
            actionId,
            incident.IncidentId,
            incident.WorkerId,
            actionType,
            delta,
            week,
            notes);

        var moraleEntry = new MoraleHistoryEntry(
            incident.WorkerId,
            week,
            delta,
            nouveau,
            $"Sanction {actionType} (incident {incident.IncidentType})",
            incident.IncidentId);

        return (action, moraleEntry);
    }
}
