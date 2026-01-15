namespace RingGeneral.Core.Models;

public sealed record MatchTypeDefinition(
    string MatchTypeId,
    string Libelle,
    string? Description,
    int? Participants,
    int? DureeParDefaut);

public sealed record SegmentTemplateDefinition(
    string TemplateId,
    string Libelle,
    string? Description,
    IReadOnlyList<SegmentTemplateSegmentDefinition> Segments);

public sealed record SegmentTemplateSegmentDefinition(
    string TypeSegment,
    int DureeMinutes,
    bool EstMainEvent,
    int? AutoParticipants,
    string? MatchTypeId);
