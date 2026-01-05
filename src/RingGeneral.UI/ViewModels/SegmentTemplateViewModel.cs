using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class SegmentTemplateViewModel
{
    public SegmentTemplateViewModel(
        SegmentTemplateDefinition definition,
        IReadOnlyDictionary<string, string> segmentLabels,
        IReadOnlyDictionary<string, string> matchTypeLabels)
    {
        Definition = definition;
        TemplateId = definition.TemplateId;
        Libelle = definition.Libelle;
        Description = definition.Description ?? string.Empty;
        SegmentsResume = ConstruireResume(definition, segmentLabels, matchTypeLabels);
        SegmentCount = definition.Segments.Count;
    }

    public SegmentTemplateDefinition Definition { get; }
    public string TemplateId { get; }
    public string Libelle { get; }
    public string Description { get; }
    public string SegmentsResume { get; }
    public int SegmentCount { get; }

    private static string ConstruireResume(
        SegmentTemplateDefinition definition,
        IReadOnlyDictionary<string, string> segmentLabels,
        IReadOnlyDictionary<string, string> matchTypeLabels)
    {
        var segments = definition.Segments.Select(segment =>
        {
            var typeLabel = segmentLabels.TryGetValue(segment.TypeSegment, out var label)
                ? label
                : segment.TypeSegment;
            var matchLabel = segment.MatchTypeId is not null && matchTypeLabels.TryGetValue(segment.MatchTypeId, out var match)
                ? $" ({match})"
                : string.Empty;
            var mainEvent = segment.EstMainEvent ? " • Main event" : string.Empty;
            return $"{typeLabel}{matchLabel} {segment.DureeMinutes}min{mainEvent}";
        });

        return string.Join(" | ", segments);
    }
}
