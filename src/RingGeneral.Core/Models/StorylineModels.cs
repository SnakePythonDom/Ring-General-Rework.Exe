namespace RingGeneral.Core.Models;

public sealed record StorylineDefinition(
    string StorylineId,
    string CompagnieId,
    string Nom,
    int Heat,
    StorylinePhase Phase,
    StorylineStatus Status,
    IReadOnlyList<string> Participants);

public sealed record StorylineEvent(
    string StorylineEventId,
    string StorylineId,
    int Semaine,
    string Type,
    string? SegmentId,
    int HeatDelta,
    int MomentumDelta,
    string? Description);
