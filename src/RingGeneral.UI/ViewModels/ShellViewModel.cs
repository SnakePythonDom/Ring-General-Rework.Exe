using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;
using RingGeneral.UI.Services;

namespace RingGeneral.UI.ViewModels;

public sealed class ShellViewModel : ViewModelBase
{
    private readonly SpecsReader _reader = new();
    private readonly NavigationSpecMapper _mapper = new();
    private readonly SaveStorageService _saveStorage = new();
    private readonly UiPageSpecsProvider _pageSpecsProvider = new();
    private readonly Dictionary<string, UiPageSpec> _pagesParId;

    public ShellViewModel()
    {
        var spec = ChargerSpecNavigation();
        Sidebar = new ObservableCollection<LegacyNavigationItemViewModel>(_mapper.ConstruireSidebar(spec));
        TopbarActions = new ObservableCollection<TopbarActionViewModel>(_mapper.ConstruireActions(spec));
        TopbarIndicators = new ObservableCollection<TopbarIndicatorViewModel>(_mapper.ConstruireIndicateurs(spec));
        Saves = new SaveManagerViewModel(_saveStorage);
        Saves.Initialiser();
        Pages = new ObservableCollection<UiPageSpec>(_pageSpecsProvider.ChargerPages());
        _pagesParId = Pages
            .Where(page => !string.IsNullOrWhiteSpace(page.Meta.PageId))
            .ToDictionary(page => page.Meta.PageId!, page => page, StringComparer.OrdinalIgnoreCase);
        var sauvegarde = Saves.SauvegardeCourante ?? Saves.Sauvegardes.FirstOrDefault();
        if (sauvegarde is null)
        {
            // Mode dégradé : créer une session temporaire en cas d'échec d'initialisation
            // L'utilisateur pourra créer/importer une sauvegarde via l'interface
            Session = new GameSessionViewModel(null);
        }
        else
        {
            Session = new GameSessionViewModel(sauvegarde.Chemin);
        }
        PageSelectionnee = TrouverPageParRoute("/dashboard")
            ?? Pages.FirstOrDefault()
            ?? new UiPageSpec { Meta = new UiPageMetaSpec { Titre = "Aucune page disponible" } };
    }

    public ObservableCollection<LegacyNavigationItemViewModel> Sidebar { get; }
    public ObservableCollection<TopbarActionViewModel> TopbarActions { get; }
    public ObservableCollection<TopbarIndicatorViewModel> TopbarIndicators { get; }
    public SaveManagerViewModel Saves { get; }
    public ObservableCollection<UiPageSpec> Pages { get; }

    public GameSessionViewModel Session
    {
        get => _session;
        private set => this.RaiseAndSetIfChanged(ref _session, value);
    }
    private GameSessionViewModel _session;

    public UiPageSpec PageSelectionnee
    {
        get => _pageSelectionnee;
        private set => this.RaiseAndSetIfChanged(ref _pageSelectionnee, value);
    }
    private UiPageSpec _pageSelectionnee = new();

    public void ChargerSauvegarde(SaveSlotViewModel slot)
    {
        if (Saves.DefinirSauvegardeCourante(slot))
        {
            Session = new GameSessionViewModel(slot.Chemin);
        }
    }

    public void OuvrirPage(string? route)
    {
        var page = TrouverPageParRoute(route);
        if (page is not null)
        {
            PageSelectionnee = page;
        }
    }

    private UiPageSpec? TrouverPageParRoute(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return null;
        }

        var alias = RouteAliases.TryGetValue(route, out var mapped) ? mapped : route.Trim('/');
        return _pagesParId.TryGetValue(alias, out var page) ? page : null;
    }

    private static readonly Dictionary<string, string> RouteAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/compagnie"] = "ma-compagnie",
        ["/relations"] = "relations-partenariats",
        ["/prets"] = "prets-excursions",
        ["/aide"] = "aide-codex"
    };

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
