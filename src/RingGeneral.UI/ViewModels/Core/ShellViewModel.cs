using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.ViewModels.Shared.Navigation;
using RingGeneral.UI.ViewModels.Booking;
using RingGeneral.UI.ViewModels.Roster;
using RingGeneral.UI.ViewModels.Storyline;
using RingGeneral.UI.ViewModels.Youth;
using RingGeneral.UI.ViewModels.Finance;
using RingGeneral.UI.ViewModels.Schedule;

namespace RingGeneral.UI.ViewModels.Core;

/// <summary>
/// ViewModel principal du Shell (Prototype D - Dual-pane FM26 style)
/// Gère la navigation arborescente et le contenu dynamique
/// </summary>
public sealed class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private NavigationItemViewModel? _selectedNavigationItem;
    private ViewModelBase? _currentContentViewModel;
    private ViewModelBase? _currentContextViewModel;

    public ShellViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        // Construction de l'arbre de navigation
        NavigationItems = BuildNavigationTree();

        // Observer les changements de ViewModel
        _navigationService.CurrentViewModelObservable
            .Subscribe(vm =>
            {
                CurrentContentViewModel = vm;
                // Mettre à jour le context panel selon le contenu
                UpdateContextPanel(vm);
            });

        // Commandes
        NavigateCommand = ReactiveCommand.Create<NavigationItemViewModel>(NavigateToItem);
        GlobalSearchCommand = ReactiveCommand.Create(OpenGlobalSearch);
        InboxCommand = ReactiveCommand.Create(OpenInbox);
        HelpCommand = ReactiveCommand.Create(OpenHelp);
        SettingsCommand = ReactiveCommand.Create(OpenSettings);

        // Sélectionner l'accueil par défaut
        var homeItem = NavigationItems.FirstOrDefault();
        if (homeItem != null)
        {
            NavigateToItem(homeItem);
        }
    }

    /// <summary>
    /// Items de navigation (TreeView)
    /// </summary>
    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    /// <summary>
    /// Item de navigation sélectionné
    /// </summary>
    public NavigationItemViewModel? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _selectedNavigationItem, value) && value != null)
            {
                NavigateToItem(value);
            }
        }
    }

    /// <summary>
    /// ViewModel du contenu central (zone principale)
    /// </summary>
    public ViewModelBase? CurrentContentViewModel
    {
        get => _currentContentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _currentContentViewModel, value);
    }

    /// <summary>
    /// ViewModel du panneau de contexte (droite)
    /// </summary>
    public ViewModelBase? CurrentContextViewModel
    {
        get => _currentContextViewModel;
        private set => this.RaiseAndSetIfChanged(ref _currentContextViewModel, value);
    }

    /// <summary>
    /// Informations contextuelles (Topbar)
    /// </summary>
    public string CurrentShowName => "Monday Night Raw";
    public int CurrentWeek => 24;
    public int TotalWeeks => 52;
    public string CurrentBudget => "$2.4M";

    /// <summary>
    /// Badges de notifications
    /// </summary>
    public int InboxCount
    {
        get => _inboxCount;
        set => this.RaiseAndSetIfChanged(ref _inboxCount, value);
    }
    private int _inboxCount = 3;

    // Commandes
    public ReactiveCommand<NavigationItemViewModel, Unit> NavigateCommand { get; }
    public ReactiveCommand<Unit, Unit> GlobalSearchCommand { get; }
    public ReactiveCommand<Unit, Unit> InboxCommand { get; }
    public ReactiveCommand<Unit, Unit> HelpCommand { get; }
    public ReactiveCommand<Unit, Unit> SettingsCommand { get; }

    private ObservableCollection<NavigationItemViewModel> BuildNavigationTree()
    {
        var root = new ObservableCollection<NavigationItemViewModel>();

        // 🏠 Accueil / Dashboard
        var home = new NavigationItemViewModel(
            "home",
            "ACCUEIL",
            "🏠",
            null // TODO: Créer DashboardViewModel
        );
        root.Add(home);

        // 📋 BOOKING
        var booking = new NavigationItemViewModel(
            "booking",
            "BOOKING",
            "📋"
        );
        booking.IsExpanded = true; // Expanded par défaut
        booking.Children.Add(new NavigationItemViewModel(
            "booking.shows",
            "Shows actifs",
            "  📺",
            typeof(BookingViewModel), // TODO: À créer
            booking
        ));
        booking.Children.Add(new NavigationItemViewModel(
            "booking.library",
            "Bibliothèque",
            "  📚",
            null, // TODO: LibraryViewModel
            booking
        ));
        booking.Children.Add(new NavigationItemViewModel(
            "booking.history",
            "Historique",
            "  📊",
            null, // TODO: ShowHistoryViewModel
            booking
        ));
        booking.Children.Add(new NavigationItemViewModel(
            "booking.settings",
            "Paramètres",
            "  ⚙️",
            null,
            booking
        ));
        root.Add(booking);

        // 👤 ROSTER
        var roster = new NavigationItemViewModel(
            "roster",
            "ROSTER",
            "👤"
        );
        roster.Children.Add(new NavigationItemViewModel(
            "roster.workers",
            "Workers",
            "  🤼",
            typeof(RosterViewModel), // TODO: À créer
            roster
        ) { Badge = "(47)" });
        roster.Children.Add(new NavigationItemViewModel(
            "roster.titles",
            "Titres",
            "  🏆",
            null, // TODO: TitlesViewModel
            roster
        ) { Badge = "(5)" });
        roster.Children.Add(new NavigationItemViewModel(
            "roster.injuries",
            "Blessures",
            "  🏥",
            null,
            roster
        ));
        root.Add(roster);

        // 📖 STORYLINES
        var storylines = new NavigationItemViewModel(
            "storylines",
            "STORYLINES",
            "📖"
        );
        storylines.Children.Add(new NavigationItemViewModel(
            "storylines.active",
            "Actives",
            "  🔥",
            null, // TODO: ActiveStorylinesViewModel
            storylines
        ) { Badge = "(2)" });
        storylines.Children.Add(new NavigationItemViewModel(
            "storylines.suspended",
            "Suspendues",
            "  ⏸️",
            null,
            storylines
        ) { Badge = "(1)" });
        storylines.Children.Add(new NavigationItemViewModel(
            "storylines.completed",
            "Terminées",
            "  ✅",
            null,
            storylines
        ));
        root.Add(storylines);

        // 🎓 YOUTH
        var youth = new NavigationItemViewModel(
            "youth",
            "YOUTH",
            "🎓",
            typeof(YouthDashboardViewModel) // TODO: À créer
        );
        root.Add(youth);

        // 💼 FINANCE
        var finance = new NavigationItemViewModel(
            "finance",
            "FINANCE",
            "💼",
            typeof(FinanceDashboardViewModel) // TODO: À créer
        );
        root.Add(finance);

        // 📆 CALENDRIER
        var calendar = new NavigationItemViewModel(
            "calendar",
            "CALENDRIER",
            "📆",
            typeof(CalendarViewModel) // TODO: À créer
        );
        root.Add(calendar);

        return root;
    }

    private void NavigateToItem(NavigationItemViewModel item)
    {
        if (item.TargetViewModelType == null)
        {
            // Si pas de ViewModel cible, c'est juste une catégorie
            // Toggle expand
            item.IsExpanded = !item.IsExpanded;
            return;
        }

        // Désélectionner tous les autres items
        DeselectAllItems(NavigationItems);

        // Sélectionner cet item
        item.IsSelected = true;
        _selectedNavigationItem = item;

        // Naviguer via le service
        // Note: Pour l'instant, on ne peut pas utiliser la réflexion ici car les ViewModels n'existent pas encore
        // On va créer une méthode temporaire
        NavigateToViewModelType(item.TargetViewModelType);
    }

    private void DeselectAllItems(ObservableCollection<NavigationItemViewModel> items)
    {
        foreach (var item in items)
        {
            item.IsSelected = false;
            if (item.HasChildren)
            {
                DeselectAllItems(item.Children);
            }
        }
    }

    private void NavigateToViewModelType(Type viewModelType)
    {
        // Cette méthode sera implémentée une fois qu'on aura tous les ViewModels
        // Pour l'instant, placeholder
        System.Diagnostics.Debug.WriteLine($"Navigation vers {viewModelType.Name}");
    }

    private void UpdateContextPanel(ViewModelBase? contentViewModel)
    {
        // Mettre à jour le panneau de contexte selon le contenu affiché
        // Par exemple : si on affiche BookingViewModel, on affiche ValidationPanelViewModel
        // TODO: Implémenter la logique de contexte
    }

    private void OpenGlobalSearch()
    {
        // TODO: Ouvrir le panneau de recherche globale
    }

    private void OpenInbox()
    {
        // TODO: Ouvrir l'inbox
    }

    private void OpenHelp()
    {
        // TODO: Ouvrir l'aide
    }

    private void OpenSettings()
    {
        // TODO: Ouvrir les paramètres
    }
}
