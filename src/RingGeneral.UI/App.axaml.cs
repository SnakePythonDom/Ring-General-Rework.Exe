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
using CoreInterfaces = RingGeneral.Core.Interfaces;
using DataRepositories = RingGeneral.Data.Repositories;

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
        // 🗂️ INITIALISATION DE LA GENERAL DB (ring_general.db)
        // ═════════════════════════════════════════════════════════════════════════

        try
        {
            var generalDbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RingGeneral",
                "ring_general.db");

            // Chercher le template statique dans plusieurs emplacements possibles
            var possibleTemplatePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "data", "ring_general_static.db"),
                Path.Combine(Directory.GetCurrentDirectory(), "data", "ring_general_static.db"),
                // Remonter depuis bin/Debug/net8.0 vers la racine du projet
                Path.Combine(new DirectoryInfo(AppContext.BaseDirectory).Parent?.Parent?.Parent?.FullName ?? "", "data", "ring_general_static.db")
            };

            var templatePath = possibleTemplatePaths.FirstOrDefault(File.Exists);

            // Si la base n'existe pas, l'initialiser depuis le template ou créer avec données génériques
            if (!File.Exists(generalDbPath))
            {
                if (templatePath != null && File.Exists(templatePath))
                {
                    var tempInitializer = new DbInitializer();
                    tempInitializer.InitializeFromStaticTemplate(generalDbPath, templatePath);
                    logger.Info($"✅ General DB initialisée depuis template : {generalDbPath}");
                }
                else
                {
                    logger.Warning($"⚠️ Template statique introuvable. Chemins testés :");
                    foreach (var path in possibleTemplatePaths)
                    {
                        logger.Warning($"  - {path}");
                    }
                    logger.Info($"Création de la base avec schéma et données génériques...");

                    // Créer la base vide et appliquer le schéma + seed
                    var tempInitializer = new DbInitializer();
                    tempInitializer.CreateDatabaseIfMissing(generalDbPath);

                    // Remplir avec des données génériques
                    using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={generalDbPath}");
                    connection.Open();

                    // S'assurer que toutes les tables essentielles existent avant de seed
                    SqliteConnectionFactory.EnsureEssentialTablesExist(connection);

                    // Configurer le logger pour DbSeeder
                    DbSeeder.SetLogger(logger);

                    DbSeeder.SeedIfEmpty(connection);

                    logger.Info($"✅ General DB créée avec données génériques : {generalDbPath}");
                }
            }
            else
            {
                logger.Info($"✅ General DB existe déjà : {generalDbPath}");

                // Vérifier que la base contient les tables nécessaires
                try
                {
                    using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={generalDbPath}");
                    connection.Open();

                    // Vérifier si les tables existent
                    using var checkCmd = connection.CreateCommand();
                    checkCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND (name='Companies' OR name='companies')";
                    var hasCompaniesTable = Convert.ToInt64(checkCmd.ExecuteScalar()) > 0;

                    checkCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND (name='Workers' OR name='workers')";
                    var hasWorkersTable = Convert.ToInt64(checkCmd.ExecuteScalar()) > 0;

                    if (!hasCompaniesTable || !hasWorkersTable)
                    {
                        logger.Warning($"⚠️ Base de données existe mais schéma manquant. Création du schéma...");
                        var tempInitializer = new DbInitializer();
                        tempInitializer.CreateDatabaseIfMissing(generalDbPath);

                        // Vérifier à nouveau et remplir avec des données génériques
                        checkCmd.CommandText = "SELECT COUNT(*) FROM Companies";
                        var companiesCount = Convert.ToInt64(checkCmd.ExecuteScalar());

                        checkCmd.CommandText = "SELECT COUNT(*) FROM Workers";
                        var workersCount = Convert.ToInt64(checkCmd.ExecuteScalar());

                        if (companiesCount == 0 || workersCount == 0)
                        {
                            logger.Info($"Remplissage avec données génériques...");
                            DbSeeder.SetLogger(logger);
                            DbSeeder.SeedIfEmpty(connection);
                            logger.Info($"✅ Base de données initialisée avec données génériques");
                        }
                        else
                        {
                            logger.Info($"✅ Schéma créé, base contient déjà des données : {companiesCount} companies, {workersCount} workers");
                        }
                    }
                    else
                    {
                        // Tables existent, vérifier le contenu
                        using var cmd = connection.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM Companies";
                        var companiesCount = Convert.ToInt64(cmd.ExecuteScalar());

                        cmd.CommandText = "SELECT COUNT(*) FROM Workers";
                        var workersCount = Convert.ToInt64(cmd.ExecuteScalar());

                        if (companiesCount == 0 || workersCount == 0)
                        {
                            logger.Warning($"⚠️ Base de données vide. Remplissage avec données génériques...");
                            DbSeeder.SetLogger(logger);
                            DbSeeder.SeedIfEmpty(connection);
                            logger.Info($"✅ Données génériques ajoutées");
                        }
                        else
                        {
                            logger.Info($"✅ Base de données valide : {companiesCount} companies, {workersCount} workers");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warning($"⚠️ Erreur lors de la validation de la base : {ex.Message}");
                    // Essayer de créer le schéma même en cas d'erreur
                    try
                    {
                        var tempInitializer = new DbInitializer();
                        tempInitializer.CreateDatabaseIfMissing(generalDbPath);
                        logger.Info($"✅ Schéma créé après erreur de validation");
                    }
                    catch (Exception initEx)
                    {
                        logger.Error($"❌ Impossible de créer le schéma : {initEx.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Warning($"⚠️ Impossible d'initialiser General DB : {ex.Message}");
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
        services.AddSingleton<IGameRepository>(sp => repositories.GameRepository);
        services.AddSingleton(repositories.ShowRepository);
        services.AddSingleton(repositories.CompanyRepository);

        // Show Scheduling System (Daily Show System)
        // Note: Ces services sont enregistrés ici mais DailyShowSchedulerService sera créé après IShowDayOrchestrator
        services.AddSingleton<RingGeneral.Core.Interfaces.IShowSchedulerStore>(sp =>
            new RingGeneral.Data.Repositories.ShowSchedulerStore(
                sp.GetRequiredService<ShowRepository>(),
                sp.GetRequiredService<SqliteConnectionFactory>()));
        services.AddSingleton<RingGeneral.Core.Services.ShowSchedulerService>(sp =>
            new RingGeneral.Core.Services.ShowSchedulerService(
                sp.GetRequiredService<RingGeneral.Core.Interfaces.IShowSchedulerStore>()));

        // Child Company Booking System
        services.AddSingleton<RingGeneral.Core.Interfaces.IChildCompanyBookingRepository>(sp =>
            new RingGeneral.Data.Repositories.ChildCompanyBookingRepository(
                sp.GetRequiredService<SqliteConnectionFactory>()));
        services.AddSingleton(repositories.WorkerRepository);
        services.AddSingleton(repositories.BackstageRepository);
        services.AddSingleton(repositories.ScoutingRepository);
        services.AddSingleton(repositories.ContractRepository);
        services.AddSingleton(repositories.SettingsRepository);
        services.AddSingleton(repositories.YouthRepository);
        services.AddSingleton(repositories.TitleRepository);
        services.AddSingleton<CoreInterfaces.ITitleRepository>(sp => repositories.TitleRepository);
        services.AddSingleton<CoreInterfaces.IContenderRepository>(sp => repositories.TitleRepository);
        services.AddSingleton<CoreInterfaces.ISettingsRepository>(sp => repositories.SettingsRepository);
        services.AddSingleton<CoreInterfaces.IYouthRepository>(sp => repositories.YouthRepository);
        services.AddSingleton<CoreInterfaces.IBackstageRepository>(sp => repositories.BackstageRepository);
        services.AddSingleton<IMedicalRepository>(sp => repositories.MedicalRepository);
        services.AddSingleton<CoreInterfaces.IWorkerAttributesRepository>(sp => (CoreInterfaces.IWorkerAttributesRepository)repositories.WorkerAttributesRepository);
        services.AddSingleton<IRelationsRepository>(sp => repositories.RelationsRepository);
        services.AddSingleton<INotesRepository>(sp => repositories.NotesRepository);
        services.AddSingleton<IContractRepository>(sp => repositories.ContractRepository);

        // Company Governance & Identity
        services.AddSingleton(repositories.OwnerRepository);
        services.AddSingleton(repositories.BookerRepository);
        // Enregistrer aussi avec l'interface Core pour compatibilité
        services.AddSingleton<CoreInterfaces.IBookerRepository>(sp =>
            (CoreInterfaces.IBookerRepository)repositories.BookerRepository);
        services.AddSingleton<CoreInterfaces.IOwnerRepository>(sp => (CoreInterfaces.IOwnerRepository)repositories.OwnerRepository);
        services.AddSingleton(repositories.CatchStyleRepository);
        services.AddSingleton<RingGeneral.Core.Interfaces.ICatchStyleRepository>(sp => repositories.CatchStyleRepository);
        services.AddSingleton(repositories.EraRepository);
        services.AddSingleton<RingGeneral.Core.Interfaces.IEraRepository>(sp => repositories.EraRepository);
        services.AddSingleton<IRegionRepository>(_ => new RegionRepository(factory));

        // Structural Analysis & Niche Strategies Repositories (Phase 6)
        services.AddSingleton<IRosterAnalysisRepository>(repositories.RosterAnalysisRepository);
        services.AddSingleton<ITrendRepository>(repositories.TrendRepository);
        services.AddSingleton<INicheFederationRepository>(repositories.NicheFederationRepository);
        services.AddSingleton<IChildCompanyExtendedRepository>(repositories.ChildCompanyExtendedRepository);
        services.AddSingleton<IDNATransitionRepository>(repositories.DNATransitionRepository);
        services.AddSingleton<IChildCompanyStaffRepository>(repositories.ChildCompanyStaffRepository);

        // Structural Analysis & Niche Strategies Services (Phase 6)
        services.AddSingleton<RosterAnalysisService>(sp =>
            new RosterAnalysisService(
                sp.GetRequiredService<IRosterAnalysisRepository>()));
        services.AddSingleton<TrendEngine>(sp =>
            new TrendEngine(sp.GetRequiredService<ITrendRepository>()));
        services.AddSingleton<CompatibilityCalculator>(sp =>
            new CompatibilityCalculator(sp.GetRequiredService<ITrendRepository>()));
        services.AddSingleton<NicheFederationService>(sp =>
            new NicheFederationService(
                sp.GetRequiredService<INicheFederationRepository>(),
                sp.GetRequiredService<IRosterAnalysisRepository>()));
        services.AddSingleton<RosterInertiaService>(sp =>
            new RosterInertiaService(
                sp.GetRequiredService<IRosterAnalysisRepository>(),
                sp.GetRequiredService<IDNATransitionRepository>()));
        services.AddSingleton<ChildCompanyService>(sp =>
            new ChildCompanyService(
                sp.GetRequiredService<IChildCompanyExtendedRepository>(),
                sp.GetRequiredService<ITrendRepository>(),
                sp.GetRequiredService<YouthRepository>())); // Phase 2.3

        // Staff Repository
        services.AddSingleton<IStaffRepository>(sp =>
        {
            var connectionString = sp.GetRequiredService<SqliteConnectionFactory>().GetConnectionString();
            return new StaffRepository(connectionString);
        });

        // Crisis Repository
        // Crisis Repository
        services.AddSingleton<CoreInterfaces.ICrisisRepository>(sp =>
            new CrisisRepository(sp.GetRequiredService<SqliteConnectionFactory>()));

        // Child Company Staff Service
        services.AddSingleton<IChildCompanyStaffService>(sp =>
        {
            var connectionString = sp.GetRequiredService<SqliteConnectionFactory>().GetConnectionString();
            var childCompanyRepository = new ChildCompanyRepository(connectionString);
            return new ChildCompanyStaffService(
                sp.GetRequiredService<IChildCompanyStaffRepository>(),
                sp.GetRequiredService<IStaffRepository>(),
                childCompanyRepository);
        });

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
        services.AddSingleton<ICrisisEngine>(sp =>
            new CrisisEngine(sp.GetRequiredService<ICrisisRepository>()));

        // Owner & Booker Decision Engines
        services.AddSingleton<IOwnerDecisionEngine>(sp =>
            new OwnerDecisionEngine(null)); // Ces services fonctionnent sans repository
        // Phase 3.3 - BookerAIEngine avec intégration personnalités
        services.AddSingleton<IBookerAIEngine>(sp =>
            new BookerAIEngine(
                sp.GetRequiredService<IBookerRepository>(),
                sp.GetRequiredService<IEraRepository>(),
                sp.GetRequiredService<PersonalityDetectorService>(),
                sp.GetRequiredService<RingGeneral.Core.Interfaces.IWorkerAttributesRepository>()));

        // Legacy registration (removed)
        // services.AddSingleton<IBookerAIEngine>(sp =>
        //     new BookerAIEngine(null)); // Ces services fonctionnent sans repository

        // Phase 2.1 - TV Deal Negotiation Service
        services.AddSingleton<ICompanyRepository>(repositories.CompanyRepository);
        services.AddSingleton<ITvDealRepository>(repositories.CompanyRepository);
        services.AddSingleton<CoreInterfaces.IBrandRepository>(sp => new BrandRepository(factory));
        services.AddSingleton<CoreInterfaces.IEraRepository>(sp => new EraRepository(factory));
        services.AddSingleton<CoreInterfaces.INicheFederationRepository>(sp => new NicheFederationRepository(factory));

        services.AddSingleton<ITvDealNegotiationService>(sp =>
            new TvDealNegotiationService(
                sp.GetRequiredService<CoreInterfaces.ICompanyRepository>(),
                sp.GetRequiredService<ITvDealRepository>()));

        // Phase 2.2 - Revenue Projection & Budget Allocation Services
        services.AddSingleton<IRevenueProjectionService>(sp =>
            new RevenueProjectionService(
                sp.GetRequiredService<ICompanyRepository>(),
                sp.GetRequiredService<ITvDealRepository>()));
        services.AddSingleton<IBudgetAllocationService>(sp =>
            new BudgetAllocationService());

        // Phase 2.2 - Debt Management Service
        services.AddSingleton<IDebtManagementService>(sp =>
            new DebtManagementService(
                sp.GetRequiredService<ICompanyRepository>()));

        // Phase 1.2 - Booking Control Service
        services.AddSingleton<IBookingControlService>(sp =>
            new BookingControlService(
                sp.GetRequiredService<IBookerAIEngine>()));

        // Booking Builder & Template Services
        services.AddSingleton<RingGeneral.Core.Services.BookingBuilderService>();
        services.AddSingleton<RingGeneral.Core.Services.TemplateService>();
        services.AddSingleton<RingGeneral.Core.Contracts.ContractNegotiationService>();

        // Daily Time System Services (Phase 7)
        services.AddSingleton<IGameRepository>(repositories.GameRepository);
        services.AddSingleton<IDailyServices>(sp =>
            new DailyFinanceService(sp.GetRequiredService<IGameRepository>()));

        // Show Day Orchestrator (doit être enregistré avant TimeOrchestratorService)
        services.AddSingleton<IShowDayOrchestrator>(sp =>
            new ShowDayOrchestrator(
                showScheduler: sp.GetRequiredService<RingGeneral.Core.Interfaces.IShowSchedulerStore>(),
                titleService: null,   // TODO: Implémenter ITitleService si nécessaire
                random: null,
                bookerAIEngine: null, // TODO: Implémenter IBookerAIEngine si nécessaire
                impactApplier: null,  // TODO: Implémenter IImpactApplier si nécessaire
                moraleEngine: sp.GetRequiredService<IMoraleEngine>(),
                dailyServices: sp.GetRequiredService<IDailyServices>(),
                contextLoader: null,  // Sera fourni par GameRepository
                statusUpdater: null,  // Sera fourni par ShowRepository
                inboxItemAdder: item => repositories.GameRepository.AjouterInboxItem(item))); // Phase 3.2

        // DailyShowSchedulerService (doit être créé après IShowDayOrchestrator)
        services.AddSingleton<RingGeneral.Core.Services.DailyShowSchedulerService>(sp =>
            new RingGeneral.Core.Services.DailyShowSchedulerService(
                sp.GetRequiredService<RingGeneral.Core.Services.ShowSchedulerService>(),
                sp.GetService<RingGeneral.Core.Interfaces.IOwnerDecisionEngine>(),
                sp.GetRequiredService<RingGeneral.Core.Interfaces.IGameRepository>(),
                sp.GetService<RingGeneral.Core.Interfaces.IShowDayOrchestrator>(),
                sp.GetService<RingGeneral.Core.Interfaces.IBookerAIEngine>()));

        services.AddSingleton<ITimeOrchestratorService>(sp =>
            new TimeOrchestratorService(
                sp.GetRequiredService<IGameRepository>(),
                dailyServices: sp.GetRequiredService<IDailyServices>(),
                eventGenerator: null,     // TODO: Implémenter IEventGeneratorService
                showDayOrchestrator: sp.GetRequiredService<IShowDayOrchestrator>(),
                dailyShowScheduler: sp.GetRequiredService<RingGeneral.Core.Services.DailyShowSchedulerService>()));

        // Additional Core Services
        services.AddSingleton<RingGeneral.Core.Services.TitleService>(sp =>
            new RingGeneral.Core.Services.TitleService(
                sp.GetRequiredService<ITitleRepository>()));
        services.AddSingleton<RingGeneral.Core.Services.ContenderService>(sp =>
            new RingGeneral.Core.Services.ContenderService(
                sp.GetRequiredService<IContenderRepository>()));
        services.AddSingleton<RingGeneral.Core.Services.BrandManagementService>();
        services.AddSingleton<RingGeneral.Core.Services.EraTransitionService>();
        services.AddSingleton<RingGeneral.Core.Services.StorylineService>();
        services.AddSingleton<ICommunicationEngine>(sp =>
            new RingGeneral.Core.Services.CommunicationEngine(
                crisisRepository: null)); // CommunicationEngine fonctionne sans repository
        services.AddSingleton<RingGeneral.Core.Services.StaffCompatibilityCalculator>(sp =>
        {
            var connectionString = sp.GetRequiredService<SqliteConnectionFactory>().GetConnectionString();
            var childCompanyRepository = new ChildCompanyRepository(connectionString);
            return new RingGeneral.Core.Services.StaffCompatibilityCalculator(childCompanyRepository);
        });
        services.AddSingleton<RingGeneral.Core.Services.StaffProposalService>(sp =>
            new RingGeneral.Core.Services.StaffProposalService(
                sp.GetRequiredService<RingGeneral.Core.Services.StaffCompatibilityCalculator>()));
        services.AddSingleton<RingGeneral.Core.Services.StaffSharingEngine>(sp =>
        {
            var connectionString = sp.GetRequiredService<SqliteConnectionFactory>().GetConnectionString();
            var childCompanyRepository = new ChildCompanyRepository(connectionString);
            return new RingGeneral.Core.Services.StaffSharingEngine(
                sp.GetRequiredService<IChildCompanyStaffService>(),
                sp.GetRequiredService<IStaffRepository>(),
                childCompanyRepository);
        });

        // ChildCompanyBookingService (doit être créé après DailyShowSchedulerService)
        services.AddSingleton<RingGeneral.Core.Services.ChildCompanyBookingService>(sp =>
            new RingGeneral.Core.Services.ChildCompanyBookingService(
                sp.GetRequiredService<RingGeneral.Core.Interfaces.IChildCompanyBookingRepository>(),
                sp.GetService<RingGeneral.Core.Services.DailyShowSchedulerService>(),
                sp.GetService<RingGeneral.Core.Services.ShowSchedulerService>(),
                sp.GetService<RingGeneral.Core.Interfaces.IBookerAIEngine>()));

        // Legacy Personality Services
        services.AddSingleton<PersonalityDetectorService>();
        services.AddSingleton<AgentReportGeneratorService>();

        // ViewModels - Core
        services.AddSingleton<ViewModels.Core.ShellViewModel>(sp =>
            new ViewModels.Core.ShellViewModel(
                navigationService: sp.GetRequiredService<INavigationService>(),
                eventAggregator: sp.GetRequiredService<RingGeneral.UI.Services.Messaging.IEventAggregator>(),
                repository: sp.GetService<GameRepository>()));

        // ViewModels - Start
        services.AddTransient<StartViewModel>();
        services.AddTransient<CompanySelectorViewModel>();
        services.AddTransient<CreateCompanyViewModel>();

        // ViewModels - Game
        services.AddTransient<DashboardViewModel>(sp =>
            new DashboardViewModel(
                repository: sp.GetRequiredService<GameRepository>(),
                showSchedulerStore: sp.GetRequiredService<RingGeneral.Core.Interfaces.IShowSchedulerStore>(),
                showDayOrchestrator: sp.GetRequiredService<IShowDayOrchestrator>(),
                timeOrchestrator: sp.GetRequiredService<ITimeOrchestratorService>(),
                moraleEngine: sp.GetRequiredService<IMoraleEngine>(),
                crisisEngine: sp.GetRequiredService<ICrisisEngine>()));

        // Booking ViewModels
        services.AddTransient<BookingViewModel>(sp =>
            new BookingViewModel(
                sp.GetRequiredService<GameRepository>(),
                sp.GetRequiredService<BookingValidator>(),
                sp.GetRequiredService<SegmentTypeCatalog>(),
                sp.GetRequiredService<IEventAggregator>(),
                sp.GetRequiredService<RingGeneral.Core.Services.BookingBuilderService>(),
                sp.GetRequiredService<RingGeneral.Core.Services.TemplateService>(),
                sp.GetRequiredService<RingGeneral.Core.Interfaces.IBookingControlService>(),
                sp.GetRequiredService<RingGeneral.Core.Interfaces.IShowDayOrchestrator>()));
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<ShowHistoryPageViewModel>();
        services.AddTransient<BookingSettingsViewModel>();
        services.AddTransient<ShowBookingViewModel>(sp =>
            new ShowBookingViewModel(
                sp.GetRequiredService<GameRepository>(),
                sp.GetRequiredService<SegmentTypeCatalog>(),
                sp.GetRequiredService<BookingValidator>(),
                sp.GetRequiredService<RingGeneral.Core.Services.BookingBuilderService>(),
                sp.GetRequiredService<RingGeneral.Core.Services.TemplateService>(),
                sp.GetRequiredService<IBookerAIEngine>(),
                sp.GetRequiredService<IBookingControlService>(),
                sp.GetRequiredService<SettingsRepository>()));

        // Roster ViewModels
        services.AddTransient<RosterViewModel>(sp =>
            new RosterViewModel(
                repository: sp.GetRequiredService<GameRepository>(),
                navigationService: sp.GetRequiredService<INavigationService>()));
        services.AddTransient<ViewModels.Roster.WorkerDetailViewModel>(sp =>
            new ViewModels.Roster.WorkerDetailViewModel(
                repository: sp.GetRequiredService<GameRepository>(),
                profileViewModel: sp.GetRequiredService<ViewModels.Workers.Profile.ProfileViewModel>()));
        services.AddTransient<ViewModels.Roster.TitlesViewModel>(sp =>
            new ViewModels.Roster.TitlesViewModel(
                sp.GetRequiredService<ITitleRepository>(),
                sp.GetRequiredService<RingGeneral.Core.Services.ContenderService>(),
                sp.GetRequiredService<GameRepository>()));
        services.AddTransient<ViewModels.Medical.InjuriesViewModel>(sp =>
            new ViewModels.Medical.InjuriesViewModel(
                sp.GetRequiredService<IGameRepository>(),
                sp.GetRequiredService<IMedicalRepository>(),
                sp.GetRequiredService<INavigationService>()));
        services.AddTransient<ViewModels.Roster.StructuralDashboardViewModel>(sp =>
            new ViewModels.Roster.StructuralDashboardViewModel(
                sp.GetRequiredService<IRosterAnalysisRepository>(),
                sp.GetRequiredService<RosterAnalysisService>()));

        // Trends ViewModels (Phase 6)
        services.AddTransient<ViewModels.Trends.TrendsViewModel>(sp =>
            new ViewModels.Trends.TrendsViewModel(
                sp.GetRequiredService<ITrendRepository>(),
                sp.GetRequiredService<IRosterAnalysisRepository>(),
                sp.GetRequiredService<CompatibilityCalculator>()));

        // Company ViewModels (Phase 6)
        services.AddTransient<ViewModels.Company.NicheManagementViewModel>(sp =>
            new ViewModels.Company.NicheManagementViewModel(
                sp.GetRequiredService<INicheFederationRepository>(),
                sp.GetRequiredService<NicheFederationService>(),
                sp.GetRequiredService<IOwnerDecisionEngine>()));
        services.AddTransient<ViewModels.Company.ChildCompaniesViewModel>(sp =>
            new ViewModels.Company.ChildCompaniesViewModel(
                sp.GetRequiredService<IChildCompanyExtendedRepository>(),
                sp.GetRequiredService<ChildCompanyService>(),
                sp.GetService<IRegionRepository>(),
                sp.GetRequiredService<INavigationService>()));
        services.AddTransient<ViewModels.Company.ChildCompanyDetailViewModel>(sp =>
            new ViewModels.Company.ChildCompanyDetailViewModel(
                sp.GetRequiredService<IGameRepository>(),
                sp.GetRequiredService<IStaffRepository>(),
                sp.GetRequiredService<IChildCompanyExtendedRepository>(),
                sp.GetRequiredService<WorkerRepository>(),
                sp.GetRequiredService<INavigationService>()));
        services.AddTransient<ViewModels.Company.ChildCompanyBookingViewModel>(sp =>
            new ViewModels.Company.ChildCompanyBookingViewModel(
                sp.GetRequiredService<RingGeneral.Core.Services.ChildCompanyBookingService>(),
                sp.GetRequiredService<ShowRepository>(),
                sp.GetRequiredService<GameRepository>()));

        // Other ViewModels
        services.AddTransient<StorylinesViewModel>(sp =>
            new StorylinesViewModel(
                sp.GetRequiredService<GameRepository>(),
                sp.GetRequiredService<RingGeneral.Core.Services.StorylineService>()));
        services.AddTransient<YouthViewModel>(sp =>
            new YouthViewModel(
                sp.GetRequiredService<GameRepository>(),
                sp.GetRequiredService<YouthRepository>()));
        services.AddTransient<YouthHubViewModel>(sp =>
            new YouthHubViewModel(
                sp.GetRequiredService<GameRepository>(),
                sp.GetRequiredService<YouthRepository>()));
        services.AddTransient<FinanceViewModel>(sp =>
            new FinanceViewModel(
                sp.GetRequiredService<GameRepository>(),
                sp.GetRequiredService<IDebtManagementService>(),
                sp.GetRequiredService<IRevenueProjectionService>(),
                sp.GetRequiredService<IBudgetAllocationService>(),
                sp.GetRequiredService<ITvDealNegotiationService>()));
        services.AddTransient<CalendarViewModel>(sp =>
            new CalendarViewModel(
                sp.GetRequiredService<GameRepository>(),
                sp.GetRequiredService<RingGeneral.Core.Services.ShowSchedulerService>(),
                sp.GetRequiredService<ShowRepository>()));
        services.AddTransient<CompanyHubViewModel>(sp =>
            new CompanyHubViewModel(
                sp.GetRequiredService<GameRepository>(),
                sp.GetRequiredService<IOwnerRepository>(),
                sp.GetRequiredService<IBookerRepository>(),
                sp.GetRequiredService<ICatchStyleRepository>(),
                sp.GetRequiredService<IChildCompanyExtendedRepository>(),
                sp.GetRequiredService<IChildCompanyStaffService>(),
                sp.GetRequiredService<IChildCompanyStaffRepository>(),
                sp.GetRequiredService<IStaffRepository>(),
                sp.GetRequiredService<RingGeneral.Core.Services.StaffCompatibilityCalculator>(),
                sp.GetRequiredService<RingGeneral.Core.Services.StaffProposalService>(),
                sp.GetRequiredService<RingGeneral.Core.Services.StaffSharingEngine>(),
                sp.GetRequiredService<IBrandRepository>(),
                sp.GetRequiredService<IEraRepository>(),
                sp.GetRequiredService<INicheFederationRepository>()));
        services.AddTransient<OwnerBookerViewModel>(sp =>
            new OwnerBookerViewModel(
                sp.GetRequiredService<OwnerRepository>(),
                sp.GetRequiredService<BookerRepository>()));
        services.AddTransient<CrisisViewModel>(sp =>
            new CrisisViewModel(
                sp.GetRequiredService<ICrisisRepository>(),
                sp.GetRequiredService<ICrisisEngine>(),
                sp.GetRequiredService<ICommunicationEngine>()));

        // Modal ViewModels (Phase 10)
        services.AddTransient<ViewModels.Workers.Profile.ProfileViewModel>(sp =>
            new ViewModels.Workers.Profile.ProfileViewModel(
                sp.GetRequiredService<IWorkerAttributesRepository>(),
                sp.GetRequiredService<IRelationsRepository>(),
                sp.GetRequiredService<INotesRepository>(),
                sp.GetRequiredService<PersonalityDetectorService>(),
                sp.GetRequiredService<AgentReportGeneratorService>()));

        services.AddSingleton<Func<string, string, int, ViewModels.Contracts.ContractNegotiationViewModel>>(sp =>
            (workerId, companyId, currentWeek) => new ViewModels.Contracts.ContractNegotiationViewModel(
                sp.GetRequiredService<RingGeneral.Core.Contracts.ContractNegotiationService>(),
                sp.GetRequiredService<RingGeneral.Core.Services.TemplateService>(),
                sp.GetRequiredService<GameRepository>(),
                workerId,
                companyId,
                currentWeek));

        services.AddSingleton<Func<string, ViewModels.Finance.TvDealNegotiationViewModel>>(sp =>
            companyId => new ViewModels.Finance.TvDealNegotiationViewModel(
                sp.GetRequiredService<ITvDealNegotiationService>(),
                companyId));
        // Inbox & Settings ViewModels
        services.AddTransient<ViewModels.Inbox.InboxViewModel>(sp =>
            new ViewModels.Inbox.InboxViewModel(
                sp.GetRequiredService<GameRepository>()));
        services.AddTransient<ViewModels.Settings.SettingsViewModel>();
        services.AddTransient<MedicalViewModel>(sp =>
            new MedicalViewModel(
                gameRepository: sp.GetRequiredService<GameRepository>(),
                medicalRepository: sp.GetRequiredService<MedicalRepository>(),
                navigationService: sp.GetRequiredService<INavigationService>()));

        var provider = services.BuildServiceProvider();

        // Obtenir le NavigationService
        var navigationService = provider.GetRequiredService<INavigationService>();

        // Vérifier si une partie est déjà en cours
        var hasActiveSave = CheckForActiveSave(saveGameManager);

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
    private static bool CheckForActiveSave(SaveGameManager saveGameManager)
    {
        try
        {
            // #region agent log
            var logPath = Path.Combine(AppContext.BaseDirectory, ".cursor", "debug.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"App.axaml.cs:402\",\"message\":\"CheckForActiveSave entry\",\"data\":{{\"saveGameManagerType\":\"{saveGameManager.GetType().Name}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
            // #endregion

            // Utiliser SaveGameManager qui accède à la Save DB
            var activeSave = saveGameManager.ChargerSauvegardeActive();

            // #region agent log
            File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"App.axaml.cs:408\",\"message\":\"After ChargerSauvegardeActive\",\"data\":{{\"hasActiveSave\":{activeSave != null}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
            // #endregion

            return activeSave != null;
        }
        catch (Exception ex)
        {
            // #region agent log
            var logPath = Path.Combine(AppContext.BaseDirectory, ".cursor", "debug.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"App.axaml.cs:412\",\"message\":\"Exception caught\",\"data\":{{\"exceptionType\":\"{ex.GetType().Name}\",\"message\":\"{ex.Message.Replace("\"", "\\\"")}\",\"stackTrace\":\"{ex.StackTrace?.Replace("\"", "\\\"").Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
            // #endregion

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
