using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.Services.Messaging;
using RingGeneral.UI.ViewModels.Dashboard;
using RingGeneral.UI.ViewModels.Booking;
using RingGeneral.UI.ViewModels.Roster;
using RingGeneral.UI.ViewModels.Storylines;
using RingGeneral.UI.ViewModels.Youth;
using RingGeneral.UI.ViewModels.Finance;
using RingGeneral.UI.ViewModels.Calendar;
using RingGeneral.UI.ViewModels.CompanyHub;
using RingGeneral.UI.ViewModels.Crisis;
using RingGeneral.UI.ViewModels.OwnerBooker;
using RingGeneral.UI.ViewModels.Start;
using RingGeneral.UI.ViewModels.Medical;
using RingGeneral.UI.Views.Shell;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Validation;
using RingGeneral.Core.Services;
using RingGeneral.Core.Interfaces;
using RingGeneral.UI.ViewModels;

namespace RingGeneral.UI;

public sealed class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var logger = ApplicationServices.Logger;

        // Configuration DI
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IEventAggregator, EventAggregator>();

        // ═════════════════════════════════════════════════════════════════════════
        // 🗂️ INITIALISATION DE LA WORLD DB
        // ═════════════════════════════════════════════════════════════════════════
        
        try
        {
            // Chercher BAKI1.1.db dans le répertoire de la solution
            var solutionRoot = AppContext.BaseDirectory;
            // Remonter depuis bin/Debug/net8.0 vers la racine du projet
            var upDirs = new DirectoryInfo(solutionRoot).Parent?.Parent?.Parent;
            var bakiDbPath = upDirs != null ? Path.Combine(upDirs.FullName, "data", "BAKI1.1.db") : "";

            // Fallback : chercher directement dans AppData ou chemins connus
            if (!File.Exists(bakiDbPath))
            {
                bakiDbPath = @"C:\Users\popo2\source\repos\Ring-General-Rework.Exe\data\BAKI1.1.db";
            }

            var worldDbDir = Path.Combine(AppContext.BaseDirectory, "data");
            var worldDbPath = Path.Combine(worldDbDir, "ring_world.db");

            Directory.CreateDirectory(worldDbDir);

            if (File.Exists(bakiDbPath))
            {
                var worldDbInit = new WorldDbInitializer(bakiDbPath, worldDbPath);
                worldDbInit.InitializeIfNeeded();
                logger.Info($"✅ World DB initialisée : {worldDbPath}");
            }
            else
            {
                logger.Warning($"⚠️ BAKI1.1.db introuvable à : {bakiDbPath}");
                logger.Info($"Tentative de création avec données par défaut...");
                
                // Créer une DB vide avec juste le schéma et des données par défaut
                using var worldConnection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={worldDbPath}");
                worldConnection.Open();
                using var transaction = worldConnection.BeginTransaction();
                var initializer = new WorldDbInitializer(bakiDbPath, worldDbPath);
                // Forcer la création du schéma et des données par défaut
                initializer.CreateSchemaWithDefaults(worldConnection);
                transaction.Commit();
            }
        }
        catch (Exception ex)
        {
            logger.Warning($"⚠️ Impossible d'initialiser World DB : {ex.Message}");
            // Continuer - la validation se fera lors de l'accès aux données
        }

        // Database & Repositories
        var factory = new SqliteConnectionFactory();
        var repositories = RepositoryFactory.CreateRepositories(factory);

        // Database initialization and validation services
        var dbInitializer = new DbInitializer();
        var dbValidator = new DbValidator();
        services.AddSingleton(dbInitializer);
        services.AddSingleton(dbValidator);

        // SaveGame manager service
        var saveGameManager = new SaveGameManager(factory, dbInitializer, dbValidator);
        services.AddSingleton(saveGameManager);
        services.AddSingleton(factory);

        // Register all repositories
        services.AddSingleton(repositories.GameRepository);
        services.AddSingleton(repositories.ShowRepository);
        services.AddSingleton(repositories.CompanyRepository);
        services.AddSingleton(repositories.WorkerRepository);
        services.AddSingleton(repositories.BackstageRepository);
        services.AddSingleton(repositories.ScoutingRepository);
        services.AddSingleton(repositories.ContractRepository);
        services.AddSingleton(repositories.SettingsRepository);
        services.AddSingleton(repositories.YouthRepository);
        services.AddSingleton(repositories.TitleRepository);
        services.AddSingleton(repositories.MedicalRepository);
        services.AddSingleton(repositories.WorkerAttributesRepository);

        // Company Governance & Identity
        services.AddSingleton(repositories.OwnerRepository);
        services.AddSingleton(repositories.BookerRepository);
        services.AddSingleton(repositories.CatchStyleRepository);
        services.AddSingleton<IRegionRepository>(_ => new RegionRepository(factory));

        // Core Services
        services.AddSingleton<BookingValidator>();
        services.AddSingleton<SegmentTypeCatalog>(ChargerSegmentTypes());

        // Personality System Services (Phase 1)
        services.AddSingleton<IPersonalityEngine, PersonalityEngine>();
        services.AddSingleton<IPersonalityRepository>(sp =>
            new PersonalityRepository(factory, sp.GetRequiredService<IPersonalityEngine>()));

        // Nepotism System Services (Phase 2)
        services.AddSingleton<INepotismRepository>(sp => new NepotismRepository(factory));
        services.AddSingleton<INepotismEngine>(sp =>
            new NepotismEngine(sp.GetRequiredService<INepotismRepository>()));

        // Morale & Rumors System Services (Phase 3)
        services.AddSingleton<IMoraleRepository>(sp => new MoraleRepository(factory));
        services.AddSingleton<IMoraleEngine>(sp =>
            new MoraleEngine(sp.GetRequiredService<IMoraleRepository>()));
        services.AddSingleton<IRumorRepository>(sp => new RumorRepository(factory));
        services.AddSingleton<IRumorEngine>(sp =>
            new RumorEngine(sp.GetRequiredService<IRumorRepository>()));

        // Crisis System Services (Phase 5)
        // Note: CrisisEngine accepte ICrisisRepository optionnel, donc on peut passer null pour l'instant
        // TODO: Créer un adaptateur ou faire implémenter Core.Interfaces.ICrisisRepository par CrisisRepository
        services.AddSingleton<ICrisisEngine>(sp =>
            new CrisisEngine(crisisRepository: null));  // CrisisEngine fonctionne sans repository

        // Daily Time System Services (Phase 7)
        services.AddSingleton<IGameRepository>(repositories.GameRepository);
        services.AddSingleton<IDailyServices>(sp =>
            new DailyFinanceService(sp.GetRequiredService<IGameRepository>()));
        
        // Show Day Orchestrator (doit être enregistré avant TimeOrchestratorService)
        services.AddSingleton<IShowDayOrchestrator>(sp =>
            new ShowDayOrchestrator(
                showScheduler: null,  // TODO: Implémenter IShowSchedulerStore si nécessaire
                titleService: null,   // TODO: Implémenter ITitleService si nécessaire
                random: null,
                bookerAIEngine: null, // TODO: Implémenter IBookerAIEngine si nécessaire
                impactApplier: null,  // TODO: Implémenter IImpactApplier si nécessaire
                moraleEngine: sp.GetRequiredService<IMoraleEngine>(),
                dailyServices: sp.GetRequiredService<IDailyServices>(),
                contextLoader: null,  // Sera fourni par GameRepository
                statusUpdater: null)); // Sera fourni par ShowRepository
        
        services.AddSingleton<ITimeOrchestratorService>(sp =>
            new TimeOrchestratorService(
                sp.GetRequiredService<IGameRepository>(),
                dailyServices: sp.GetRequiredService<IDailyServices>(),
                eventGenerator: null,     // TODO: Implémenter IEventGeneratorService
                showDayOrchestrator: sp.GetRequiredService<IShowDayOrchestrator>()));

        // Legacy Personality Services
        services.AddSingleton<PersonalityDetectorService>();
        services.AddSingleton<AgentReportGeneratorService>();

        // ViewModels - Core
        services.AddSingleton<ViewModels.Core.ShellViewModel>();

        // ViewModels - Start
        services.AddTransient<StartViewModel>();
        services.AddTransient<CompanySelectorViewModel>();
        services.AddTransient<CreateCompanyViewModel>();

        // ViewModels - Game
        services.AddTransient<DashboardViewModel>(sp =>
            new DashboardViewModel(
                repository: sp.GetRequiredService<GameRepository>(),
                showSchedulerStore: null,  // TODO: Implémenter IShowSchedulerStore si nécessaire
                showDayOrchestrator: sp.GetRequiredService<IShowDayOrchestrator>(),
                timeOrchestrator: sp.GetRequiredService<ITimeOrchestratorService>(),
                moraleEngine: sp.GetRequiredService<IMoraleEngine>(),
                crisisEngine: sp.GetRequiredService<ICrisisEngine>()));

        // Booking ViewModels
        services.AddTransient<BookingViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<ShowHistoryPageViewModel>();
        services.AddTransient<BookingSettingsViewModel>();

        // Roster ViewModels
        services.AddTransient<RosterViewModel>();
        services.AddTransient<ViewModels.Roster.WorkerDetailViewModel>();
        services.AddTransient<ViewModels.Roster.TitlesViewModel>();
        services.AddTransient<ViewModels.Roster.InjuriesViewModel>();

        // Other ViewModels
        services.AddTransient<StorylinesViewModel>();
        services.AddTransient<YouthViewModel>();
        services.AddTransient<FinanceViewModel>();
        services.AddTransient<CalendarViewModel>();
        services.AddTransient<CompanyHubViewModel>();
        services.AddTransient<OwnerBookerViewModel>();
        services.AddTransient<CrisisViewModel>();

        // Inbox & Settings ViewModels
        services.AddTransient<ViewModels.Inbox.InboxViewModel>();
        services.AddTransient<ViewModels.Settings.SettingsViewModel>();
        services.AddTransient<MedicalViewModel>();

        var provider = services.BuildServiceProvider();

        // Obtenir le NavigationService
        var navigationService = provider.GetRequiredService<INavigationService>();

        // Vérifier si une partie est déjà en cours
        var hasActiveSave = CheckForActiveSave(repositories.GameRepository);

        if (hasActiveSave)
        {
            // Charger directement le Shell si une partie existe
            logger.Info("Partie active détectée, chargement du Dashboard...");
            // Le ShellViewModel naviguera automatiquement vers DashboardViewModel
        }
        else
        {
            // Sinon, afficher le menu de démarrage
            logger.Info("Aucune partie active, affichage du menu de démarrage...");

            // Initialiser le NavigationService avec StartViewModel AVANT de créer le Shell
            navigationService.NavigateTo<StartViewModel>();
            logger.Debug("Navigation vers StartViewModel effectuée");
        }

        // Créer le ShellViewModel (qui observera le NavigationService)
        var shellViewModel = provider.GetRequiredService<ViewModels.Core.ShellViewModel>();
        logger.Debug($"ShellViewModel créé, CurrentContentViewModel = {shellViewModel.CurrentContentViewModel?.GetType().Name ?? "null"}");

        // Lancer la fenêtre principale avec le ShellViewModel
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(shellViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Vérifie si une partie active existe dans la base de données
    /// </summary>
    private static bool CheckForActiveSave(GameRepository repository)
    {
        try
        {
            using var connection = repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM SaveGames WHERE IsActive = 1";
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
        catch (Exception ex)
        {
            ApplicationServices.Logger.Error($"Erreur lors de la vérification de sauvegarde: {ex.Message}", ex);
            return false;
        }
    }

    private static SegmentTypeCatalog ChargerSegmentTypes()
    {
        // Charger depuis les specs ou créer un catalogue par défaut
        return new SegmentTypeCatalog(
            new Dictionary<string, string>
            {
                ["match"] = "Match",
                ["promo"] = "Promo",
                ["angle"] = "Angle",
                ["interview"] = "Interview"
            },
            new Dictionary<string, IReadOnlyList<string>>(),
            new Dictionary<string, IReadOnlyList<string>>(),
            new Dictionary<string, string>()
        );
    }
}
