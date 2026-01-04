using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class SegmentResultViewModel
{
    public SegmentResultViewModel(SegmentReport report, string? libelle = null)
    {
        SegmentId = report.SegmentId;
        TypeSegment = libelle ?? report.TypeSegment;
        Note = report.Note;
        Duree = report.Impact.FatigueDelta.Values.Sum();
        Participants = string.Join(", ", report.Impact.FatigueDelta.Keys);
        Evenements = report.Evenements.Count == 0 ? "Aucun" : string.Join(", ", report.Evenements);
        Breakdown = string.Join(" | ", report.Facteurs.Select(facteur => $"{facteur.Libelle} {facteur.Impact:+#;-#;0}"));
        Impacts = ConstruireImpacts(report);
        Icons = ConstruireIcons(report);
    }

    public string SegmentId { get; }
    public string TypeSegment { get; }
    public int Note { get; }
    public int Duree { get; }
    public string Participants { get; }
    public string Evenements { get; }
    public string Breakdown { get; }
    public string Impacts { get; }
    public string Icons { get; }

    private static string ConstruireImpacts(SegmentReport report)
    {
        var elements = new List<string>();
        if (report.Impact.MomentumDelta.Count > 0)
        {
            elements.Add("Momentum: " + string.Join(", ", report.Impact.MomentumDelta.Select(kv => $"{kv.Key} {kv.Value:+#;-#;0}")));
        }

        if (report.Impact.FatigueDelta.Count > 0)
        {
            elements.Add("Fatigue: " + string.Join(", ", report.Impact.FatigueDelta.Select(kv => $"{kv.Key} +{kv.Value}")));
        }

        if (report.Impact.PopulariteDelta.Count > 0)
        {
            elements.Add("Pop: " + string.Join(", ", report.Impact.PopulariteDelta.Select(kv => $"{kv.Key} {kv.Value:+#;-#;0}")));
        }

        if (report.Impact.Blessures.Count > 0)
        {
            elements.Add("Blessures: " + string.Join(", ", report.Impact.Blessures));
        }

        return elements.Count == 0 ? "Aucun impact notable" : string.Join(" | ", elements);
    }

    private static string ConstruireIcons(SegmentReport report)
    {
        var icons = new List<string>();
        if (report.Evenements.Any(e => e.Contains("Botch", StringComparison.OrdinalIgnoreCase)))
        {
            icons.Add("💥");
        }

        if (report.Evenements.Any(e => e.Contains("Blessure", StringComparison.OrdinalIgnoreCase)))
        {
            icons.Add("🩹");
        }

        if (report.Impact.StorylineHeatDelta.Count > 0)
        {
            icons.Add("📖");
        }

        if (report.Impact.TitrePrestigeDelta.Count > 0)
        {
            icons.Add("🏆");
        }

        return icons.Count == 0 ? "—" : string.Join(" ", icons);
    }
}
