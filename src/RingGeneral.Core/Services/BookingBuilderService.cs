using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class BookingBuilderService
{
    public IReadOnlyList<SegmentDefinition> AjouterSegment(
        IReadOnlyList<SegmentDefinition> segments,
        SegmentDefinition segment,
        int? index = null)
    {
        var liste = segments.ToList();
        var insertion = index.HasValue ? Math.Clamp(index.Value, 0, liste.Count) : liste.Count;
        liste.Insert(insertion, segment);
        return liste;
    }

    public IReadOnlyList<SegmentDefinition> MettreAJourSegment(
        IReadOnlyList<SegmentDefinition> segments,
        SegmentDefinition segment)
    {
        var liste = segments.ToList();
        var index = liste.FindIndex(item => item.SegmentId == segment.SegmentId);
        if (index >= 0)
        {
            liste[index] = segment;
        }

        return liste;
    }

    public IReadOnlyList<SegmentDefinition> SupprimerSegment(
        IReadOnlyList<SegmentDefinition> segments,
        string segmentId)
    {
        return segments.Where(segment => segment.SegmentId != segmentId).ToList();
    }

    public IReadOnlyList<SegmentDefinition> DeplacerSegment(
        IReadOnlyList<SegmentDefinition> segments,
        string segmentId,
        int delta)
    {
        var liste = segments.ToList();
        var index = liste.FindIndex(segment => segment.SegmentId == segmentId);
        if (index < 0)
        {
            return liste;
        }

        var target = Math.Clamp(index + delta, 0, liste.Count - 1);
        if (target == index)
        {
            return liste;
        }

        var element = liste[index];
        liste.RemoveAt(index);
        liste.Insert(target, element);
        return liste;
    }

    public SegmentDefinition DupliquerSegment(SegmentDefinition segment, string? nouveauId = null, bool conserverMainEvent = false)
    {
        var id = string.IsNullOrWhiteSpace(nouveauId) ? $"SEG-{Guid.NewGuid():N}".ToUpperInvariant() : nouveauId;
        return segment with { SegmentId = id, EstMainEvent = conserverMainEvent && segment.EstMainEvent };
    }

    public SegmentDefinition DupliquerMatch(SegmentDefinition segment, string? nouveauId = null)
    {
        if (segment.TypeSegment != "match")
        {
            return DupliquerSegment(segment, nouveauId, false);
        }

        return DupliquerSegment(segment, nouveauId, false);
    }
}
