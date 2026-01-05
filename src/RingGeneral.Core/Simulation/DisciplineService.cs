using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class DisciplineService
{
    public DisciplineResult AppliquerAction(
        int week,
        string companyId,
        BackstageIncident incident,
        BackstageWorker worker,
        DisciplinaryActionDefinition action,
        string notes)
    {
        var actionId = $"DISC-{Guid.NewGuid():N}";
        var record = new DisciplinaryAction(
            actionId,
            companyId,
            worker.WorkerId,
            week,
            action.TypeId,
            action.Gravite,
            action.MoraleDelta,
            notes,
            incident.IncidentId);

        var impact = new BackstageMoraleImpact(
            worker.WorkerId,
            action.MoraleDelta,
            $"Discipline: {action.Libelle}",
            incident.IncidentId,
            actionId);

        return new DisciplineResult(record, impact);
    }
}
