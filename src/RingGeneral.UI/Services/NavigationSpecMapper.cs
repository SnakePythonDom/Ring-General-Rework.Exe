using RingGeneral.Specs.Models;
using RingGeneral.UI.ViewModels;

namespace RingGeneral.UI.Services;

public sealed class NavigationSpecMapper
{
    public IReadOnlyList<NavigationItemViewModel> ConstruireSidebar(NavigationSpec spec)
    {
        return spec.Sidebar.Sections.Select(ConstruireSection).ToList();
    }

    public IReadOnlyList<TopbarActionViewModel> ConstruireActions(NavigationSpec spec)
    {
        return spec.Topbar.ActionsGlobales
            .Select(action => new TopbarActionViewModel(action.Id, action.Libelle, action.Tooltip))
            .ToList();
    }

    public IReadOnlyList<TopbarIndicatorViewModel> ConstruireIndicateurs(NavigationSpec spec)
    {
        return spec.Topbar.Indicateurs
            .Select(indicateur => new TopbarIndicatorViewModel(indicateur.Id, indicateur.Libelle, "--"))
            .ToList();
    }

    private static NavigationItemViewModel ConstruireSection(NavigationSectionSpec section)
    {
        var item = new NavigationItemViewModel(section.Id, section.Libelle, section.Route, section.Icone);

        foreach (var sousSection in section.SousSections)
        {
            item.SousSections.Add(ConstruireSection(sousSection));
        }

        return item;
    }
}
