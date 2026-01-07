using System.Collections.ObjectModel;

namespace RingGeneral.UI.ViewModels;

/// <summary>
/// Legacy navigation item model used by NavigationSpecMapper
/// </summary>
public sealed class LegacyNavigationItemViewModel
{
    public LegacyNavigationItemViewModel(string id, string libelle, string? route, string? icone)
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
    public ObservableCollection<LegacyNavigationItemViewModel> SousSections { get; } = [];
    public bool EstGroupe => SousSections.Count > 0;
}
