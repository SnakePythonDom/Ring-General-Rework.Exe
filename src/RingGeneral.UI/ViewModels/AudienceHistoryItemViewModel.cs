using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class AudienceHistoryItemViewModel
{
    public AudienceHistoryItemViewModel(AudienceHistoryEntry entry, int? audiencePrecedente)
    {
        Semaine = entry.Week;
        Audience = entry.Audience;
        Evolution = audiencePrecedente.HasValue
            ? $"{entry.Audience - audiencePrecedente.Value:+#;-#;0}"
            : "N/A";
    }

    public int Semaine { get; }
    public int Audience { get; }
    public string Evolution { get; }
}
