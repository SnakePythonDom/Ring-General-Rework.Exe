namespace RingGeneral.Core.Models;

public sealed record TitleRecord(
    string TitleId,
    string CompanyId,
    string Name,
    int Prestige,
    string? HolderWorkerId);

public sealed record TitleReignRecord(
    int TitleReignId,
    string TitleId,
    string WorkerId,
    int StartDate,
    int? EndDate,
    bool IsCurrent);

public sealed record TitleMatchRecord(
    int TitleMatchId,
    string TitleId,
    string ChampionWorkerId,
    string ChallengerWorkerId,
    string WinnerWorkerId,
    string LoserWorkerId,
    int Week,
    string? ShowId,
    string? SegmentId,
    bool IsTitleChange);

public sealed record ContenderRankingEntry(
    string TitleId,
    string WorkerId,
    int Rank,
    int Score,
    int Week);

public sealed record TitleDefenseRequest(
    string TitleId,
    string ChampionId,
    string ChallengerId,
    string WinnerId,
    string LoserId,
    int Week,
    string? ShowId,
    string? SegmentId);

public sealed record TitleChangeResult(
    bool ChampionChange,
    int PrestigeDelta,
    int? NewReignId,
    int? ClosedReignId);
