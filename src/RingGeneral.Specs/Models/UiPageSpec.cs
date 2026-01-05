namespace RingGeneral.Specs.Models;

public sealed class UiPageSpec
{
    public UiPageMetaSpec Meta { get; set; } = new();
    public List<UiPageTabSpec> Onglets { get; set; } = [];
    public List<UiPageTableSpec> Tables { get; set; } = [];
    public List<string> Filtres { get; set; } = [];
    public UiPageDetailPanelSpec PanneauDetail { get; set; } = new();
    public List<UiPageActionSpec> Actions { get; set; } = [];
    public List<UiPageValidationSpec> Validations { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public string? EtatVide { get; set; }
    public List<UiPageTooltipSpec> Tooltips { get; set; } = [];
    public List<string> MessagesErreur { get; set; } = [];
}

public sealed class UiPageMetaSpec
{
    public string? Langue { get; set; }
    public string? PageId { get; set; }
    public string? Titre { get; set; }
}

public sealed class UiPageTabSpec
{
    public string? Id { get; set; }
    public string? Libelle { get; set; }
}

public sealed class UiPageTableSpec
{
    public string? Id { get; set; }
    public string? Libelle { get; set; }
    public List<string> Colonnes { get; set; } = [];
}

public sealed class UiPageDetailPanelSpec
{
    public List<string> Sections { get; set; } = [];
}

public sealed class UiPageActionSpec
{
    public string? Id { get; set; }
    public string? Libelle { get; set; }
}

public sealed class UiPageValidationSpec
{
    public string? Id { get; set; }
    public string? Message { get; set; }
}

public sealed class UiPageTooltipSpec
{
    public string? Id { get; set; }
    public string? Libelle { get; set; }
    public string? Texte { get; set; }
}
