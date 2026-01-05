namespace RingGeneral.Core.Models;

public sealed record TitleDetail(
    string TitleId,
    string CompanyId,
    int Prestige,
    string? HolderWorkerId);

public sealed record TitleReignDetail(
    int TitleReignId,
    string TitleId,
    string WorkerId,
    int StartDate,
    int? EndDate,
    bool IsCurrent);

public sealed record TitleMatchRecord(
    string TitleId,
    string? ShowId,
    int Week,
    string? ChampionId,
    string ChallengerId,
    string WinnerId,
    bool IsTitleChange,
    int PrestigeDelta);

public sealed record ContenderRanking(
    string TitleId,
    string WorkerId,
    int Rank,
    double Score,
    string Reason);

public sealed record TitleMatchInput(
    string TitleId,
    string ChallengerId,
    string WinnerId,
    int Week,
    string? ChampionId = null,
    string? ShowId = null);

public sealed record TitleMatchOutcome(
    bool TitleChanged,
    int PrestigeDelta,
    int? NewReignId);
