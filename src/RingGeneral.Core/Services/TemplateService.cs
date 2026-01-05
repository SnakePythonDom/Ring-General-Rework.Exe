using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class TemplateService
{
    public IReadOnlyList<SegmentDefinition> AppliquerTemplate(
        SegmentTemplateDefinition template,
        IReadOnlyList<WorkerSnapshot> workers,
        int intensiteParDefaut = 60)
    {
        if (template is null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (workers is null)
        {
            throw new ArgumentNullException(nameof(workers));
        }

        var segments = new List<SegmentDefinition>();
        var workerIds = workers.Select(worker => worker.WorkerId).ToList();
        var index = 0;

        foreach (var segmentTemplate in template.Segments)
        {
            var participants = new List<string>();
            var autoParticipants = segmentTemplate.AutoParticipants.GetValueOrDefault();
            if (autoParticipants > 0 && workerIds.Count > 0)
            {
                for (var i = 0; i < autoParticipants; i++)
                {
                    participants.Add(workerIds[index % workerIds.Count]);
                    index++;
                }
            }

            segments.Add(new SegmentDefinition(
                $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
                segmentTemplate.TypeSegment,
                participants,
                Math.Max(1, segmentTemplate.DureeMinutes),
                segmentTemplate.EstMainEvent,
                null,
                null,
                intensiteParDefaut,
                null,
                null));
        }

        return segments;
    }
}
