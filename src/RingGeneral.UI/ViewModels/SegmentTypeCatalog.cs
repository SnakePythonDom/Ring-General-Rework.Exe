namespace RingGeneral.UI.ViewModels;

public sealed class SegmentTypeCatalog
{
    public SegmentTypeCatalog(
        IReadOnlyDictionary<string, string> labels,
        IReadOnlyDictionary<string, IReadOnlyList<string>> consignesParType,
        IReadOnlyDictionary<string, IReadOnlyList<string>> consigneOptions,
        IReadOnlyDictionary<string, string> consigneLabels)
    {
        Labels = labels;
        ConsignesParType = consignesParType;
        ConsigneOptions = consigneOptions;
        ConsigneLabels = consigneLabels;
    }

    public IReadOnlyDictionary<string, string> Labels { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ConsignesParType { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ConsigneOptions { get; }
    public IReadOnlyDictionary<string, string> ConsigneLabels { get; }

    public IReadOnlyList<string> ObtenirConsignesPourType(string typeSegment)
        => ConsignesParType.TryGetValue(typeSegment, out var consignes) ? consignes : Array.Empty<string>();

    public IReadOnlyList<string> ObtenirOptionsConsigne(string consigneId)
        => ConsigneOptions.TryGetValue(consigneId, out var options) ? options : Array.Empty<string>();

    public string ObtenirLibelleConsigne(string consigneId)
        => ConsigneLabels.TryGetValue(consigneId, out var libelle) ? libelle : consigneId;
}
