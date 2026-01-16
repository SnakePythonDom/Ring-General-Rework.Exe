using System.Collections.ObjectModel;
using RingGeneral.Core.Interfaces;
using System.Reactive;
using ReactiveUI;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.Services.Messaging;
using RingGeneral.UI.ViewModels.Shared.Navigation;
using RingGeneral.UI.ViewModels.Booking;
using RingGeneral.UI.ViewModels.Dashboard;
using RingGeneral.UI.ViewModels.Roster;
using RingGeneral.UI.ViewModels.Trends;
using RingGeneral.UI.ViewModels.Company;
using RingGeneral.UI.ViewModels.Storylines;
using RingGeneral.UI.ViewModels.Youth;
using RingGeneral.UI.ViewModels.Finance;
using RingGeneral.UI.ViewModels.Calendar;
using RingGeneral.UI.ViewModels.OwnerBooker;
using RingGeneral.UI.ViewModels.Crisis;
using RingGeneral.UI.ViewModels.Inbox;
using RingGeneral.UI.ViewModels.Start;
using RingGeneral.UI.ViewModels.Medical;
using RingGeneral.UI.ViewModels.CompanyHub;
using RingGeneral.UI.ViewModels.Recruitment;
using RingGeneral.UI.ViewModels.Settings;
using RingGeneral.UI.ViewModels; // provide access to GameSessionViewModel
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Core;

/// <summary>
/// ViewModel principal du Shell (Prototype D - Dual-pane FM26 style)
/// Gère la navigation arborescente et le contenu dynamique
/// </summary>
public sealed class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly GameRepository? _repository;
    private NavigationItemViewModel? _selectedNavigationItem;
    private NavigationItemViewModel? _workersNavigationItem;
    private ViewModelBase? _currentContentViewModel;
    private ViewModelBase? _currentContextViewModel;
    private bool _isInGameMode = false;
    private GameSessionViewModel? _gameSession;
    private readonly ITimeOrchestratorService? _timeOrchestrator;

    public GameSessionViewModel? GameSession
    {
        get => _gameSession;
        private set => this.RaiseAndSetIfChanged(ref _gameSession, value);
    }

    private readonly IRecruitmentService _recruitmentService;
    private readonly IEventAggregator _eventAggregator;

    public ShellViewModel(
        INavigationService navigationService,
        IEventAggregator eventAggregator,
        IRecruitmentService recruitmentService,
        ITimeOrchestratorService? timeOrchestrator = null,
        GameRepository? repository = null)
    {
        _navigationService = navigationService;
        _eventAggregator = eventAggregator;
        _recruitmentService = recruitmentService;
        _timeOrchestrator = timeOrchestrator;
        _repository = repository;

        // Ensure game session exists as early as possible so bindings (commands) are available
        GameSession = new GameSessionViewModel();

        // Construction de l'arbre de navigation
        NavigationItems = BuildNavigationTree();

        // Charger le nombre de workers dynamiquement
        _ = LoadWorkersCountAsync();

        // Observer les changements de ViewModel
        _navigationService.CurrentViewModelObservable
            .Subscribe(vm =>
            {
                Logger.Info($"CurrentViewModel changé: {vm?.GetType().Name ?? "null"}");
                CurrentContentViewModel = vm;

                // Initialization contextuelle des ViewModels
                if (vm is ChildCompaniesViewModel childCompaniesVm)
                {
                    // Charger les filiales pour la compagnie courante
                    // TODO: Get actual company ID from session/repository
                    _ = childCompaniesVm.LoadChildCompaniesAsync("PLAYER_COMPANY_ID");
                }

                // Mettre à jour le context panel selon le contenu
                UpdateContextPanel(vm);
            });

        // =========================================================
        // SUBSCRIPTIONS TO EVENT AGGREGATOR
        // =========================================================
        // We need to listen to segment selection to show segment details in the context panel
        // This connects the BookingViewModel selection to the Shell's ContextPanel
        _eventAggregator.GetEvent<SegmentSelectedEvent>()
            .Subscribe(evt =>
            {
                if (evt?.Segment != null)
                {
                    // Update context panel with the selected segment
                    CurrentContextViewModel = evt.Segment;
                }
            });

        _eventAggregator.GetEvent<RecruitAgentEvent>()
            .Subscribe(evt =>
            {
                if (evt?.Agent != null)
                {
                    // Open recruitment dialog in context panel
                    var dialogVm = new RecruitmentDialogViewModel(evt.Agent, "PLAYER_COMPANY_ID", _recruitmentService, _eventAggregator);
                    CurrentContextViewModel = dialogVm;
                }
            });

        // Commandes
        NavigateCommand = ReactiveCommand.Create<NavigationItemViewModel>(NavigateToItem);
        GlobalSearchCommand = ReactiveCommand.Create(OpenGlobalSearch);
        InboxCommand = ReactiveCommand.Create(OpenInbox);
        HelpCommand = ReactiveCommand.Create(OpenHelp);
        SettingsCommand = ReactiveCommand.Create(OpenSettings);
        ContinueCommand = ReactiveCommand.Create(OnContinue);

        // Commandes de navigation rapide pour la top bar
        NavigateToDashboardCommand = ReactiveCommand.Create(() => _navigationService.NavigateTo<DashboardViewModel>());
        NavigateToBookingCommand = ReactiveCommand.Create(() => _navigationService.NavigateTo<BookingViewModel>());
        NavigateToCompanyCommand = ReactiveCommand.Create(() => _navigationService.NavigateTo<ViewModels.CompanyHub.CompanyHubViewModel>());
        NavigateToLibraryCommand = ReactiveCommand.Create(() => _navigationService.NavigateTo<LibraryViewModel>());
        NavigateToReportsCommand = ReactiveCommand.Create(OpenReports);
        NavigateToSettingsCommand = ReactiveCommand.Create(OpenSettings);
        NavigateToCalendarCommand = ReactiveCommand.Create(() => _navigationService.NavigateTo<CalendarViewModel>());
        NavigateToYouthHubCommand = ReactiveCommand.Create(() => _navigationService.NavigateTo<YouthHubViewModel>());

        // Synchroniser le CurrentViewModel du NavigationService s'il existe déjà
        if (_navigationService.CurrentViewModel != null)
        {
            Logger.Info($"ViewModel initial depuis NavigationService: {_navigationService.CurrentViewModel.GetType().Name}");
            CurrentContentViewModel = _navigationService.CurrentViewModel;
        }
        else
        {
            // Sélectionner l'accueil par défaut seulement si pas de ViewModel initial
            var homeItem = NavigationItems.FirstOrDefault();
            if (homeItem != null)
            {
                Logger.Info($"Navigation vers l'accueil par défaut");
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
            if (_selectedNavigationItem == value)
            {
                return;
            }

            NavigationItemViewModel? navigationItemViewModel = this.RaiseAndSetIfChanged(ref _selectedNavigationItem, value);
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
            // Mettre à jour CurrentViewTitle quand le ViewModel change
            this.RaisePropertyChanged(nameof(CurrentViewTitle));
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
    /// Titre de la vue actuelle affiché dans la top bar
    /// </summary>
    public string CurrentViewTitle
    {
        get
        {
            return CurrentContentViewModel switch
            {
                DashboardViewModel => "DASHBOARD",
                BookingViewModel => "BOOKING",
                LibraryViewModel => "LIBRARY",
                ShowHistoryPageViewModel => "SHOW HISTORY",
                BookingSettingsViewModel => "BOOKING SETTINGS",
                RosterHubViewModel => "ROSTER HUB",
                RosterViewModel => "ROSTER",
                ViewModels.Roster.WorkerDetailViewModel => "WORKER DETAILS",
                TitlesViewModel => "TITLES",
                ViewModels.Roster.StructuralDashboardViewModel => "STRUCTURAL ANALYSIS",
                MedicalViewModel => "MEDICAL",
                ViewModels.CompanyHub.CompanyHubViewModel => "COMPANY HUB",
                ViewModels.Trends.TrendsViewModel => "TRENDS",
                ViewModels.Company.NicheManagementViewModel => "NICHE MANAGEMENT",
                ViewModels.Company.ChildCompaniesViewModel => "CHILD COMPANIES",
                ViewModels.Company.ChildCompanyBookingViewModel => "CHILD BOOKING",
                StorylinesViewModel => "STORYLINES",
                YouthViewModel or YouthHubViewModel => "YOUTH",
                FinanceViewModel => "FINANCE",
                OwnerBookerViewModel => "OWNER & BOOKER",
                CrisisViewModel => "CRISES",
                CalendarViewModel => "CALENDAR",
                InboxViewModel => "INBOX",
                SettingsViewModel => "SETTINGS",
                FreeAgentsViewModel => "FREE AGENTS MARKET",
                Start.StartViewModel => "MAIN MENU",
                Start.CompanySelectorViewModel => "COMPANY SELECTION",
                Start.CreateCompanyViewModel => "CREATE COMPANY",
                null => "",
                _ => CurrentContentViewModel?.GetType().Name.Replace("ViewModel", "") ?? ""
            };
        }
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
    public ReactiveCommand<Unit, Unit> ContinueCommand { get; }

    // Commandes de navigation rapide pour la top bar
    public ReactiveCommand<Unit, Unit> NavigateToDashboardCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToBookingCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToCompanyCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToLibraryCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToReportsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToCalendarCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToYouthHubCommand { get; }

    private ObservableCollection<NavigationItemViewModel> BuildNavigationTree()
    {
        var root = new ObservableCollection<NavigationItemViewModel>();

        // 1. DASHBOARD (Overview)
        var dashboardGroup = new NavigationItemViewModel("dashboard_root", "DASHBOARD", "🏠");
        dashboardGroup.IsExpanded = true;

        dashboardGroup.Children.Add(new NavigationItemViewModel(
            "dashboard.home",
            "Overview",
            "  🏠",
            typeof(DashboardViewModel),
            dashboardGroup
        ));
        dashboardGroup.Children.Add(new NavigationItemViewModel(
            "dashboard.calendar",
            "Calendar & Emails",
            "  📆",
            typeof(CalendarViewModel),
            dashboardGroup
        ));
        root.Add(dashboardGroup);

        // 2. CREATIVE CONTROL (The Core Game)
        var creativeGroup = new NavigationItemViewModel("creative_root", "CREATIVE CONTROL", "🎬");
        creativeGroup.IsExpanded = true;

        creativeGroup.Children.Add(new NavigationItemViewModel(
            "creative.booking",
            "Active Booking",
            "  📺",
            typeof(BookingViewModel),
            creativeGroup
        ));
        creativeGroup.Children.Add(new NavigationItemViewModel(
            "creative.storylines",
            "Storylines",
            "  📖",
            typeof(StorylinesViewModel),
            creativeGroup
        ));
        creativeGroup.Children.Add(new NavigationItemViewModel(
            "creative.titles",
            "Titles & Prestige",
            "  🏆",
            typeof(TitlesViewModel),
            creativeGroup
        ));
        creativeGroup.Children.Add(new NavigationItemViewModel(
            "creative.history",
            "Show History",
            "  📚",
            typeof(ShowHistoryPageViewModel),
            creativeGroup
        ));
        creativeGroup.Children.Add(new NavigationItemViewModel(
           "creative.events",
           "Event Schedule",
           "  📅",
           typeof(BookingSettingsViewModel), // Using BookingSettings as proxy for Event Creation
           creativeGroup
       ));
        creativeGroup.Children.Add(new NavigationItemViewModel(
            "creative.staff",
            "Creative Team",
            "  🧠",
            typeof(ViewModels.CompanyHub.CompanyHubViewModel), // Using CompanyHub as proxy
            creativeGroup
        ));
        root.Add(creativeGroup);

        // 3. TALENT RELATIONS (Human Management)
        var talentGroup = new NavigationItemViewModel("talent_root", "TALENT RELATIONS", "👥");
        talentGroup.IsExpanded = true;

        talentGroup.Children.Add(new NavigationItemViewModel(
            "talent.roster",
            "Roster Hub",
            "  🤼",
            typeof(RosterHubViewModel),
            talentGroup
        ));
        talentGroup.Children.Add(new NavigationItemViewModel(
            "talent.factions",
            "Stables & Teams",
            "  🏴",
            typeof(ViewModels.Roster.FactionsViewModel),
            talentGroup
        ));
        talentGroup.Children.Add(new NavigationItemViewModel(
            "talent.backstage",
            "Backstage",
            "  💞",
            typeof(ViewModels.Roster.BackstageViewModel),
            talentGroup
        ));
        talentGroup.Children.Add(new NavigationItemViewModel(
            "talent.medical",
            "Medical & Fatigue",
            "  🚑",
            typeof(MedicalViewModel),
            talentGroup
        ));
        talentGroup.Children.Add(new NavigationItemViewModel(
           "talent.freeagents",
           "Contracts & Free Agents",
           "  📝",
           typeof(FreeAgentsViewModel),
           talentGroup
       ));
        talentGroup.Children.Add(new NavigationItemViewModel(
            "talent.youth",
            "Youth Structure",
            "  🎓",
            typeof(YouthHubViewModel),
            talentGroup
        ));
        talentGroup.Children.Add(new NavigationItemViewModel(
           "talent.childcompanies",
           "Child Companies",
           "  🐣",
           typeof(ViewModels.Company.ChildCompaniesViewModel),
           talentGroup
       ));
        root.Add(talentGroup);

        // 4. OFFICE (Business Management)
        var officeGroup = new NavigationItemViewModel("office_root", "OFFICE", "🏢");

        officeGroup.Children.Add(new NavigationItemViewModel(
            "office.finance",
            "Finance",
            "  💰",
            typeof(FinanceViewModel),
            officeGroup
        ));
        officeGroup.Children.Add(new NavigationItemViewModel(
            "office.staff",
            "Staff & Politics",
            "  👔",
            typeof(OwnerBookerViewModel),
            officeGroup
        ));
        officeGroup.Children.Add(new NavigationItemViewModel(
            "office.analysis",
            "Market Analysis",
            "  📈",
            typeof(ViewModels.Trends.TrendsViewModel),
            officeGroup
        ));
        officeGroup.Children.Add(new NavigationItemViewModel(
            "office.crises",
            "Incidents & Crises",
            "  🔥",
            typeof(CrisisViewModel),
            officeGroup
        ));
        officeGroup.Children.Add(new NavigationItemViewModel(
           "office.rivals",
           "Rival Companies",
           "  ⚔️",
           typeof(ViewModels.Company.NicheManagementViewModel), // Using Niche/Rivals view
           officeGroup
       ));
        root.Add(officeGroup);

        // 5. SYSTEM
        var systemGroup = new NavigationItemViewModel("system_root", "SYSTEM", "⚙️");

        systemGroup.Children.Add(new NavigationItemViewModel(
            "system.options",
            "Options & Database",
            "  🔧",
            typeof(SettingsViewModel),
            systemGroup
        ));

        root.Add(systemGroup);

        return root;
    }

    private void NavigateToItem(NavigationItemViewModel item)
    {
        Logger.Info($"ShellViewModel: NavigateToItem appelé pour: ID={item.Id}, Label={item.Label}, TargetType={item.TargetViewModelType?.FullName ?? "null"}");

        if (item.TargetViewModelType == null)
        {
            // Si pas de ViewModel cible, c'est juste une catégorie
            // Toggle expand
            item.IsExpanded = !item.IsExpanded;
            Logger.Info($"ShellViewModel: Item {item.Id} est une catégorie, IsExpanded={item.IsExpanded}");
            return;
        }

        // Désélectionner tous les autres items
        DeselectAllItems(NavigationItems);

        // Sélectionner cet item
        item.IsSelected = true;
        _selectedNavigationItem = item;

        // Naviguer via le service
        Logger.Info($"ShellViewModel: Navigation vers {item.TargetViewModelType.FullName} depuis l'item {item.Id}");
        NavigateToViewModelType(item.TargetViewModelType);
    }

    private void DeselectAllItems(ObservableCollection<NavigationItemViewModel> items)
    {
        foreach (var item in items)
        {
            if (item.IsSelected)
            {
                item.IsSelected = false;
                Logger.Debug($"ShellViewModel: Désélection de l'item {item.Id}");
            }
            if (item.HasChildren)
            {
                DeselectAllItems(item.Children);
            }
        }
    }

    private void NavigateToViewModelType(Type viewModelType)
    {
        try
        {
            Logger.Info($"ShellViewModel: Tentative de navigation vers {viewModelType.FullName} (nom court: {viewModelType.Name})");

            // Navigation vers un ViewModel spécifique via reflection
            var navigateMethod = typeof(INavigationService)
                .GetMethod(nameof(INavigationService.NavigateTo),
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null,
                    System.Reflection.CallingConventions.HasThis,
                    Type.EmptyTypes,
                    null)
                ?.MakeGenericMethod(viewModelType);

            if (navigateMethod == null)
            {
                Logger.Error($"ShellViewModel: Méthode NavigateTo non trouvée pour {viewModelType.Name}");
                return;
            }

            navigateMethod.Invoke(_navigationService, null);
            Logger.Info($"ShellViewModel: Navigation vers {viewModelType.FullName} effectuée avec succès");
        }
        catch (Exception ex)
        {
            var errorMessage = ex is System.Reflection.TargetInvocationException && ex.InnerException != null
                ? $"ShellViewModel: Erreur lors de la navigation vers {viewModelType.FullName} (Inner): {ex.InnerException.Message}"
                : $"ShellViewModel: Erreur lors de la navigation vers {viewModelType.FullName}: {ex.Message}";

            Logger.Error(errorMessage, ex);
        }
    }

    private void UpdateContextPanel(ViewModelBase? contentViewModel)
    {
        // Mettre à jour le panneau de contexte selon le contenu affiché
        // Le context panel affiche des informations contextuelles selon la vue active

        if (contentViewModel is BookingViewModel)
        {
            // Afficher le panel de validation pour le booking
            CurrentContextViewModel = null; // TODO: Créer ValidationPanelViewModel
            Logger.Debug("ShellViewModel: Context panel set to null for BookingViewModel (TODO: ValidationPanelViewModel)");
        }
        else if (contentViewModel is RosterHubViewModel or RosterViewModel or WorkerDetailViewModel)
        {
            // Afficher les stats du worker sélectionné
            CurrentContextViewModel = null; // TODO: Créer WorkerStatsPanelViewModel
            Logger.Debug("ShellViewModel: Context panel set to null for Roster/WorkerDetailViewModel (TODO: WorkerStatsPanelViewModel)");
        }
        else if (contentViewModel is MedicalViewModel)
        {
            // MedicalViewModel gère maintenant Injuries en interne
            CurrentContextViewModel = null;
            Logger.Debug("ShellViewModel: Context panel set to null for MedicalViewModel");
        }
        else if (contentViewModel is StorylinesViewModel)
        {
            // Afficher les détails de la storyline sélectionnée
            CurrentContextViewModel = null; // TODO: Créer StorylineDetailsPanelViewModel
            Logger.Debug("ShellViewModel: Context panel set to null for StorylinesViewModel (TODO: StorylineDetailsPanelViewModel)");
        }
        else
        {
            // Pas de context panel pour les autres vues
            CurrentContextViewModel = null;
            Logger.Debug($"ShellViewModel: Context panel set to null for {contentViewModel?.GetType().Name ?? "null"} (no specific context panel)");
        }
    }

    private void OpenGlobalSearch()
    {
        // Ouvrir le panneau de recherche globale
        // TODO: Créer GlobalSearchViewModel et l'afficher en overlay ou modal
        System.Diagnostics.Debug.WriteLine("Opening global search...");
        Logger.Info("ShellViewModel: Global search opened (TODO)");
    }

    private void OpenInbox()
    {
        // Ouvrir l'inbox des notifications
        _navigationService.NavigateTo<InboxViewModel>();
        Logger.Info("ShellViewModel: Navigation vers InboxViewModel");
    }

    private void OpenHelp()
    {
        // Ouvrir le panneau d'aide
        // TODO: Créer HelpViewModel ou ouvrir documentation externe
        System.Diagnostics.Debug.WriteLine("Opening help...");
        Logger.Info("ShellViewModel: Help opened (TODO)");
    }

    private void OpenSettings()
    {
        // Ouvrir les paramètres globaux de l'application
        _navigationService.NavigateTo<SettingsViewModel>();
        Logger.Info("ShellViewModel: Navigation vers SettingsViewModel");
    }

    private void OpenReports()
    {
        // Ouvrir les rapports (pour l'instant, rediriger vers Finance)
        // TODO: Créer ReportsViewModel si nécessaire
        _navigationService.NavigateTo<FinanceViewModel>();
        Logger.Info("ShellViewModel: Navigation vers FinanceViewModel (rapports)");
    }

    private void OnContinue()
    {
        if (_timeOrchestrator == null)
        {
            Logger.Warning("ShellViewModel: ITimeOrchestratorService non disponible");
            return;
        }

        try
        {
            // Récupérer le PlayerCompanyId depuis le repository ou le session context
            // Pour l'instant on utilise PLAYER_COMPANY_ID comme dans le reste du shell
            var playerCompanyId = "PLAYER_COMPANY_ID"; // TODO: Obtenir l'ID réel depuis la sauvegarde active

            Logger.Info("ShellViewModel: Avancement du temps (Jour Suivant)...");
            var result = _timeOrchestrator.PasserJourSuivant(playerCompanyId);

            // Lever un événement ou rafraichir le ViewModel actuel si c'est le Dashboard
            if (CurrentContentViewModel is DashboardViewModel dashboardVm)
            {
                dashboardVm.LoadDashboardData();
            }

            // On pourrait aussi déclencher un événement global via IEventAggregator pour que tous les VMs se rafraichissent
            // _eventAggregator.GetEvent<TimeAdvancedEvent>().Publish(result);

            Logger.Info($"ShellViewModel: Jour suivant effectué. Nouveau jour: {result.Day}");
        }
        catch (Exception ex)
        {
            Logger.Error($"ShellViewModel: Erreur lors du passage au jour suivant: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Charge le nombre de workers depuis la base de données et met à jour le badge
    /// </summary>
    private async System.Threading.Tasks.Task LoadWorkersCountAsync()
    {
        if (_repository == null || _workersNavigationItem == null)
        {
            Logger.Warning("ShellViewModel: _repository ou _workersNavigationItem est null, impossible de charger le nombre de workers.");
            return;
        }

        try
        {
            var count = await System.Threading.Tasks.Task.Run(() =>
            {
                using var connection = _repository.CreateConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Workers";
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            });

            // Mettre à jour le badge sur le thread UI
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_workersNavigationItem != null)
                {
                    _workersNavigationItem.Badge = $"({count})";
                    Logger.Info($"Badge Workers mis à jour: ({count})");
                }
            });
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors du chargement du nombre de workers: {ex.Message}", ex);
        }
    }
}
