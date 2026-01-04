namespace RingGeneral.Specs.Models;

public sealed class NavigationSpec
{
    public MetaSpec Meta { get; set; } = new();
    public TopbarSpec Topbar { get; set; } = new();
    public SidebarSpec Sidebar { get; set; } = new();
}

public sealed class MetaSpec
{
    public string Langue { get; set; } = "fr";
    public string SourceDeVerite { get; set; } = "specs";
    public string StyleUi { get; set; } = "FM26";
}

public sealed class TopbarSpec
{
    public List<ActionSpec> ActionsGlobales { get; set; } = [];
    public List<IndicatorSpec> Indicateurs { get; set; } = [];
}

public sealed class ActionSpec
{
    public string Id { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
}

public sealed class IndicatorSpec
{
    public string Id { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
}

public sealed class SidebarSpec
{
    public List<NavigationSectionSpec> Sections { get; set; } = [];
}

public sealed class NavigationSectionSpec
{
    public string Id { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Icone { get; set; }
    public string? Route { get; set; }
    public List<NavigationSectionSpec> SousSections { get; set; } = [];
}
