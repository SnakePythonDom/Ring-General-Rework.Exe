namespace RingGeneral.UI.ViewModels;

public sealed class SaveGameEntryViewModel
{
    public SaveGameEntryViewModel(string nom, string chemin, DateTime derniereModification)
    {
        Nom = nom;
        Chemin = chemin;
        DerniereModification = derniereModification;
    }

    public string Nom { get; }
    public string Chemin { get; }
    public DateTime DerniereModification { get; }
}
