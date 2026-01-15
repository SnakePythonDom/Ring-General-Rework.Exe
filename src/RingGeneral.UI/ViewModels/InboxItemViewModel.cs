using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class InboxItemViewModel
{
    public InboxItemViewModel(InboxItem item)
    {
        Type = item.Type;
        Titre = item.Titre;
        Contenu = item.Contenu;
        Semaine = item.Semaine;
    }

    public string Type { get; }
    public string Titre { get; }
    public string Contenu { get; }
    public int Semaine { get; }
}
