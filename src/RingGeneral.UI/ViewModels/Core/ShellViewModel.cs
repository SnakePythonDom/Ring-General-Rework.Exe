using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.ViewModels.Shared.Navigation;
using RingGeneral.UI.ViewModels.Booking;
using RingGeneral.UI.ViewModels.Dashboard;
using RingGeneral.UI.ViewModels.Roster;
using RingGeneral.UI.ViewModels.Storylines;
using RingGeneral.UI.ViewModels.Youth;
using RingGeneral.UI.ViewModels.Finance;
using RingGeneral.UI.ViewModels.Calendar;
using RingGeneral.UI.ViewModels.Start;

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
    private bool _isInGameMode = false;

    public ShellViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        // Construction de l'arbre de navigation
        NavigationItems = BuildNavigationTree();

        // Observer les changements de ViewModel
        _navigationService.CurrentViewModelObservable
            .Subscribe(vm =>
            {
                System.Console.WriteLine($"[ShellViewModel] CurrentViewModel changé: {vm?.GetType().Name ?? "null"}");
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

        // Synchroniser le CurrentViewModel du NavigationService s'il existe déjà
        if (_navigationService.CurrentViewModel != null)
        {
            System.Console.WriteLine($"[ShellViewModel] ViewModel initial depuis NavigationService: {_navigationService.CurrentViewModel.GetType().Name}");
            CurrentContentViewModel = _navigationService.CurrentViewModel;
        }
        else
        {
            // Sélectionner l'accueil par défaut seulement si pas de ViewModel initial
            var homeItem = NavigationItems.FirstOrDefault();
            if (homeItem != null)
            {
                System.Console.WriteLine($"[ShellViewModel] Navigation vers l'accueil par défaut");
                NavigateToItem(homeItem);
            }
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
            this.RaiseAndSetIfChanged(ref _selectedNavigationItem, value);
            if (value != null)
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
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentContentViewModel, value);
            // Mettre à jour IsInGameMode en fonction du ViewModel actuel
            IsInGameMode = value != null && value is not Start.StartViewModel && value is not Start.CompanySelectorViewModel && value is not Start.CreateCompanyViewModel;
        }
    }

    /// <summary>
    /// Indique si l'application est en mode jeu (vs mode menu de démarrage)
    /// Utilisé pour cacher/montrer les éléments du Shell
    /// </summary>
    public bool IsInGameMode
    {
        get => _isInGameMode;
        private set => this.RaiseAndSetIfChanged(ref _isInGameMode, value);
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
            typeof(DashboardViewModel)
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
            typeof(BookingViewModel),
            booking
        ));
        booking.Children.Add(new NavigationItemViewModel(
            "booking.library",
            "Bibliothèque",
            "  📚",
            typeof(LibraryViewModel),
            booking
        ));
        booking.Children.Add(new NavigationItemViewModel(
            "booking.history",
            "Historique",
            "  📊",
            typeof(ShowHistoryPageViewModel),
            booking
        ));
        booking.Children.Add(new NavigationItemViewModel(
            "booking.settings",
            "Paramètres",
            "  ⚙️",
            typeof(BookingSettingsViewModel),
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
            typeof(RosterViewModel),
            roster
        ) { Badge = "(47)" });
        roster.Children.Add(new NavigationItemViewModel(
            "roster.titles",
            "Titres",
            "  🏆",
            typeof(TitlesViewModel),
            roster
        ) { Badge = "(5)" });
        roster.Children.Add(new NavigationItemViewModel(
            "roster.injuries",
            "Blessures",
            "  🏥",
            typeof(InjuriesViewModel),
            roster
        ));
        root.Add(roster);

        // 📖 STORYLINES
        var storylines = new NavigationItemViewModel(
            "storylines",
            "STORYLINES",
            "📖",
            typeof(StorylinesViewModel)
        );
        root.Add(storylines);

        // 🎓 YOUTH
        var youth = new NavigationItemViewModel(
            "youth",
            "YOUTH",
            "🎓",
            typeof(YouthViewModel)
        );
        root.Add(youth);

        // 💼 FINANCE
        var finance = new NavigationItemViewModel(
            "finance",
            "FINANCE",
            "💼",
            typeof(FinanceViewModel)
        );
        root.Add(finance);

        // 📆 CALENDRIER
        var calendar = new NavigationItemViewModel(
            "calendar",
            "CALENDRIER",
            "📆",
            typeof(CalendarViewModel)
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
        // Navigation vers un ViewModel spécifique via reflection
        var navigateMethod = typeof(INavigationService)
            .GetMethod(nameof(INavigationService.NavigateTo))
            ?.MakeGenericMethod(viewModelType);

        navigateMethod?.Invoke(_navigationService, null);
    }

    private void UpdateContextPanel(ViewModelBase? contentViewModel)
    {
        // Mettre à jour le panneau de contexte selon le contenu affiché
        // Le context panel affiche des informations contextuelles selon la vue active

        if (contentViewModel is BookingViewModel)
        {
            // Afficher le panel de validation pour le booking
            CurrentContextViewModel = null; // TODO: Créer ValidationPanelViewModel
        }
        else if (contentViewModel is RosterViewModel or WorkerDetailViewModel or InjuriesViewModel)
        {
            // Afficher les stats du worker sélectionné
            CurrentContextViewModel = null; // TODO: Créer WorkerStatsPanelViewModel
        }
        else if (contentViewModel is StorylinesViewModel)
        {
            // Afficher les détails de la storyline sélectionnée
            CurrentContextViewModel = null; // TODO: Créer StorylineDetailsPanelViewModel
        }
        else
        {
            // Pas de context panel pour les autres vues
            CurrentContextViewModel = null;
        }
    }

    private void OpenGlobalSearch()
    {
        // Ouvrir le panneau de recherche globale
        // TODO: Créer GlobalSearchViewModel et l'afficher en overlay ou modal
        System.Diagnostics.Debug.WriteLine("Opening global search...");
    }

    private void OpenInbox()
    {
        // Ouvrir l'inbox des notifications
        // TODO: Créer InboxViewModel et l'afficher en overlay
        System.Diagnostics.Debug.WriteLine("Opening inbox...");
    }

    private void OpenHelp()
    {
        // Ouvrir le panneau d'aide
        // TODO: Créer HelpViewModel ou ouvrir documentation externe
        System.Diagnostics.Debug.WriteLine("Opening help...");
    }

    private void OpenSettings()
    {
        // Ouvrir les paramètres globaux de l'application
        // TODO: Créer SettingsViewModel et l'afficher en modal
        System.Diagnostics.Debug.WriteLine("Opening settings...");
    }
}
