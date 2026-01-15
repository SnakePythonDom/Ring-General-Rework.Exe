namespace RingGeneral.Core.Models;

public sealed record StorylineDefinition(
    string StorylineId,
    string CompagnieId,
    string Nom,
    int Heat,
    StorylinePhase Phase,
    StorylineStatus Status,
    IReadOnlyList<string> Participants,
    string? LeadCreativeId = null,
    string? CreativeIdea = null,
    string? BookerIdea = null,
    int StartWeek = 0,
    int? PauseWeek = null,
    string? ReasonForPause = null);

public sealed record WorkerMemory(
    string MemoryId,
    string WorkerAId,
    string WorkerBId,
    string Description,
    int Intensity,
    int WeekGenerated);

public sealed record StorylineEvent(
    string StorylineEventId,
    string StorylineId,
    int Semaine,
    string Type,
    string? SegmentId,
    int HeatDelta,
    int MomentumDelta,
    string? Description);
