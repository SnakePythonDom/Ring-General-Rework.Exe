using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class BackstageService
{
    private readonly IRandomProvider _random;

    public BackstageService(IRandomProvider random)
    {
        _random = random;
    }

    public BackstageRollResult LancerIncidents(
        int week,
        string companyId,
        IReadOnlyList<BackstageWorker> roster,
        IReadOnlyDictionary<string, int> morales,
        IReadOnlyList<BackstageIncidentDefinition> definitions)
    {
        var incidents = new List<BackstageIncident>();
        var impacts = new List<BackstageMoraleImpact>();
        var inbox = new List<InboxItem>();

        if (roster.Count == 0 || definitions.Count == 0)
        {
            return new BackstageRollResult(incidents, impacts, inbox);
        }

        var facteurMorale = morales.Count == 0
            ? 1.0
            : Math.Clamp(1.2 - (morales.Values.Average() / 100.0), 0.6, 1.4);

        foreach (var definition in definitions)
        {
            if (_random.NextDouble() > definition.Chance * facteurMorale)
            {
                continue;
            }

            var participants = TirerParticipants(roster, definition.ParticipantsMin, definition.ParticipantsMax);
            if (participants.Count == 0)
            {
                continue;
            }

            var incidentId = $"INC-{Guid.NewGuid():N}";
            var gravite = _random.Next(definition.GraviteMin, definition.GraviteMax + 1);
            var moraleDelta = _random.Next(definition.MoraleImpactMin, definition.MoraleImpactMax + 1);
            var noms = string.Join(", ", participants.Select(worker => worker.NomComplet));
            var description = definition.DescriptionTemplate
                .Replace("{worker}", participants[0].NomComplet, StringComparison.OrdinalIgnoreCase)
                .Replace("{workers}", noms, StringComparison.OrdinalIgnoreCase);

            incidents.Add(new BackstageIncident(
                incidentId,
                companyId,
                week,
                definition.TypeId,
                definition.Titre,
                description,
                gravite,
                participants.Select(worker => worker.WorkerId).ToList()));

            foreach (var participant in participants)
            {
                impacts.Add(new BackstageMoraleImpact(
                    participant.WorkerId,
                    moraleDelta,
                    $"Incident: {definition.Titre}",
                    incidentId,
                    null));
            }

            inbox.Add(new InboxItem("incident", definition.Titre, description, week));
        }

        return new BackstageRollResult(incidents, impacts, inbox);
    }

    private List<BackstageWorker> TirerParticipants(
        IReadOnlyList<BackstageWorker> roster,
        int participantsMin,
        int participantsMax)
    {
        var total = Math.Min(roster.Count, _random.Next(participantsMin, participantsMax + 1));
        var disponibles = roster.ToList();
        var selection = new List<BackstageWorker>();

        for (var i = 0; i < total; i++)
        {
            var index = _random.Next(0, disponibles.Count);
            selection.Add(disponibles[index]);
            disponibles.RemoveAt(index);
        }

        return selection;
    }
}
