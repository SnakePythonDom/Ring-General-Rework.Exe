namespace RingGeneral.UI.ViewModels;

public sealed class ReachMapItemViewModel
{
    public ReachMapItemViewModel(string zone, int reach, string? details = null)
    {
        Zone = zone;
        Reach = reach;
        Details = details ?? string.Empty;
    }

    public string Zone { get; }
    public int Reach { get; }
    public string Details { get; }
}
