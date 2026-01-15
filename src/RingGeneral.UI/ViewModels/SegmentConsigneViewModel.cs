using ReactiveUI;

namespace RingGeneral.UI.ViewModels;

public sealed class SegmentConsigneViewModel : ReactiveObject
{
    public SegmentConsigneViewModel(string id, string libelle, IReadOnlyList<string> options, string? selection)
    {
        Id = id;
        Libelle = libelle;
        Options = options;
        _selection = selection;
    }

    public string Id { get; }
    public string Libelle { get; }
    public IReadOnlyList<string> Options { get; }

    public string? Selection
    {
        get => _selection;
        set => this.RaiseAndSetIfChanged(ref _selection, value);
    }
    private string? _selection;
}
