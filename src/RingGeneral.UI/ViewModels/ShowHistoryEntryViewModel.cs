using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class ShowHistoryEntryViewModel
{
    public ShowHistoryEntryViewModel(ShowHistoryEntry entry)
    {
        ShowId = entry.ShowId;
        Week = entry.Week;
        Note = entry.Note;
        Audience = entry.Audience;
        Summary = entry.Summary;
        CreatedAt = entry.CreatedAt;
    }

    public string ShowId { get; }
    public int Week { get; }
    public int Note { get; }
    public int Audience { get; }
    public string Summary { get; }
    public DateTime CreatedAt { get; }
}
