namespace RingGeneral.Core.Models;

public sealed record SegmentTemplate(
    string TemplateId,
    string Nom,
    string? Description,
    string TypeSegment,
    int DureeMinutes,
    bool EstMainEvent,
    int Intensite,
    string? MatchTypeId,
    string? SegmentsJson);

public sealed record MatchType(
    string MatchTypeId,
    string Nom,
    string? Description,
    bool EstActif,
    int Ordre);
