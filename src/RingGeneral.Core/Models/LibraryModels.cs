namespace RingGeneral.Core.Models;

public sealed record SegmentTemplate(
    string TemplateId,
    string Nom,
    string TypeSegment,
    int DureeMinutes,
    bool EstMainEvent,
    int Intensite,
    string? MatchTypeId);

public sealed record MatchType(
    string MatchTypeId,
    string Nom,
    string? Description,
    bool EstActif,
    int Ordre);
