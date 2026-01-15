namespace RingGeneral.UI.ViewModels;

public sealed class TableFilterOptionViewModel
{
    public TableFilterOptionViewModel(string id, string libelle)
    {
        Id = id;
        Libelle = libelle;
    }

    public string Id { get; }
    public string Libelle { get; }
}
