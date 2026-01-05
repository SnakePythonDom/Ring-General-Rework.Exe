namespace RingGeneral.Core.Models;

public sealed record ShowHistoryEntry(
    string ShowId,
    int Week,
    int Note,
    int Audience,
    string Summary,
    DateTime CreatedAt);
