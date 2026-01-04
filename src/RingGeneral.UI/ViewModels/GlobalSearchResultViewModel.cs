namespace RingGeneral.UI.ViewModels;

public sealed class GlobalSearchResultViewModel
{
    public GlobalSearchResultViewModel(string type, string titre, string sousTitre, string statut)
    {
        Type = type;
        Titre = titre;
        SousTitre = sousTitre;
        Statut = statut;
    }

    public string Type { get; }
    public string Titre { get; }
    public string SousTitre { get; }
    public string Statut { get; }
}
