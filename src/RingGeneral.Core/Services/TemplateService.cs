using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Charge les templates de segments disponibles.
    /// </summary>
    public IReadOnlyList<SegmentTemplate> LoadTemplates()
    {
        // TODO: Charger depuis le repository ou fichier de configuration
        return Array.Empty<SegmentTemplate>();
    }
}
