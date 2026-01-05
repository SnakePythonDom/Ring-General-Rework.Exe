using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class ShowHistoryViewModel
{
    public ShowHistoryViewModel(ShowHistoryEntry entry)
    {
        ShowId = entry.ShowId;
        Semaine = entry.Week;
        Note = entry.Note;
        Audience = entry.Audience;
        Resume = entry.Summary;
        Date = ConvertirDate(entry.CreatedAt);
        Ligne = string.IsNullOrWhiteSpace(Date)
            ? $"Semaine {Semaine} • Note {Note} • Audience {Audience} • {Resume}"
            : $"Semaine {Semaine} • Note {Note} • Audience {Audience} • {Resume} • {Date}";
    }

    public string ShowId { get; }
    public int Semaine { get; }
    public int Note { get; }
    public int Audience { get; }
    public string Resume { get; }
    public string? Date { get; }
    public string Ligne { get; }

    private static string? ConvertirDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return null;
        }

        return DateTimeOffset.TryParse(date, out var parsed)
            ? parsed.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
            : date;
    }
}
