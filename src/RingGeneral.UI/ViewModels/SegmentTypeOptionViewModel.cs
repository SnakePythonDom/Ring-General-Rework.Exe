namespace RingGeneral.UI.ViewModels;

public sealed class SegmentTypeOptionViewModel
{
    public SegmentTypeOptionViewModel(string id, string label)
    {
        Id = id;
        Label = label;
    }

    public string Id { get; }
    public string Label { get; }
}
