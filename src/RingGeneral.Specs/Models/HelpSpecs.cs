namespace RingGeneral.Specs.Models;

public sealed class HelpMeta
{
    public string Langue { get; set; } = "fr";
    public string? Version { get; set; }
    public string? SourceDeVerite { get; set; }
}

public sealed class HelpGlossaireSpec
{
    public HelpMeta Meta { get; set; } = new();
    public List<HelpGlossaireEntree> Entrees { get; set; } = new();
}

public sealed class HelpGlossaireEntree
{
    public string Id { get; set; } = string.Empty;
    public string Terme { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public List<string> Liens { get; set; } = new();
}

public sealed class HelpSystemsSpec
{
    public HelpMeta Meta { get; set; } = new();
    public List<HelpSystemEntry> Systemes { get; set; } = new();
}

public sealed class HelpSystemEntry
{
    public string Id { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;
    public List<string> Points { get; set; } = new();
    public List<string> Liens { get; set; } = new();
}

public sealed class HelpPagesSpec
{
    public HelpMeta Meta { get; set; } = new();
    public List<HelpPageEntry> Pages { get; set; } = new();
}

public sealed class HelpPageEntry
{
    public string Id { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;
    public List<HelpPageSection> Sections { get; set; } = new();
    public List<string> ErreursFrequentes { get; set; } = new();
}

public sealed class HelpPageSection
{
    public string Titre { get; set; } = string.Empty;
    public string Contenu { get; set; } = string.Empty;
}

public sealed class HelpTooltipsSpec
{
    public HelpMeta Meta { get; set; } = new();
    public List<HelpTooltipEntry> Tooltips { get; set; } = new();
}

public sealed class HelpTooltipEntry
{
    public string Id { get; set; } = string.Empty;
    public string Texte { get; set; } = string.Empty;
}

public sealed class HelpTutorialSpec
{
    public HelpMeta Meta { get; set; } = new();
    public List<HelpTutorialStep> Etapes { get; set; } = new();
}

public sealed class HelpTutorialStep
{
    public string Id { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Contenu { get; set; } = string.Empty;
    public string Objectif { get; set; } = string.Empty;
}
