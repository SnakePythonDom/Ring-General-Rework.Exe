using System.Collections.ObjectModel;
using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;
using RingGeneral.UI.Services;

namespace RingGeneral.UI.ViewModels;

public sealed class ShellViewModel : ViewModelBase
{
    private readonly SpecsReader _reader = new();
    private readonly NavigationSpecMapper _mapper = new();

    public ShellViewModel()
    {
        var spec = ChargerSpecNavigation();
        Sidebar = new ObservableCollection<NavigationItemViewModel>(_mapper.ConstruireSidebar(spec));
        TopbarActions = new ObservableCollection<TopbarActionViewModel>(_mapper.ConstruireActions(spec));
        TopbarIndicators = new ObservableCollection<TopbarIndicatorViewModel>(_mapper.ConstruireIndicateurs(spec));
    }

    public ObservableCollection<NavigationItemViewModel> Sidebar { get; }
    public ObservableCollection<TopbarActionViewModel> TopbarActions { get; }
    public ObservableCollection<TopbarIndicatorViewModel> TopbarIndicators { get; }

    private NavigationSpec ChargerSpecNavigation()
    {
        var chemins = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "specs", "navigation.fr.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "specs", "navigation.fr.json")
        };

        var chemin = chemins.FirstOrDefault(File.Exists);

        if (chemin is null)
        {
            throw new FileNotFoundException("Impossible de trouver la spec de navigation.");
        }

        return _reader.Charger<NavigationSpec>(chemin);
    }
}
