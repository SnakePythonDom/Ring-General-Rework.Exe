using System.Collections.ObjectModel;

namespace RingGeneral.UI.ViewModels;

public sealed class NavigationItemViewModel
{
    public NavigationItemViewModel(string id, string libelle, string? route, string? icone)
    {
        Id = id;
        Libelle = libelle;
        Route = route;
        Icone = icone;
    }

    public string Id { get; }
    public string Libelle { get; }
    public string? Route { get; }
    public string? Icone { get; }
    public ObservableCollection<NavigationItemViewModel> SousSections { get; } = [];
    public bool EstGroupe => SousSections.Count > 0;
}
