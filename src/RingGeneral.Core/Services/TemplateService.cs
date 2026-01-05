using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class TemplateService
{
    public SegmentDefinition AppliquerTemplate(SegmentTemplate template)
    {
        var segmentId = $"SEG-{Guid.NewGuid():N}".ToUpperInvariant();
        return new SegmentDefinition(
            segmentId,
            template.TypeSegment,
            Array.Empty<string>(),
            Math.Max(1, template.DureeMinutes),
            template.EstMainEvent,
            null,
            null,
            template.Intensite,
            null,
            null);
    }
}
