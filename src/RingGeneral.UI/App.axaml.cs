using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.Services.Messaging;
using RingGeneral.UI.ViewModels.Core;
using RingGeneral.UI.ViewModels.Dashboard;
using RingGeneral.UI.ViewModels.Booking;
using RingGeneral.UI.ViewModels.Roster;
using RingGeneral.UI.ViewModels.Storylines;
using RingGeneral.UI.ViewModels.Youth;
using RingGeneral.UI.ViewModels.Finance;
using RingGeneral.UI.ViewModels.Calendar;
using RingGeneral.UI.ViewModels.Start;
using RingGeneral.UI.Views.Shell;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Validation;
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
        // Configuration DI
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IEventAggregator, EventAggregator>();

        // Database & Repositories
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "ringgeneral.db");
        var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
        var repositories = RepositoryFactory.CreateRepositories(factory);
        services.AddSingleton(repositories.GameRepository);
        services.AddSingleton(repositories.ScoutingRepository);

        // Core Services
        services.AddSingleton<BookingValidator>();
        services.AddSingleton<SegmentTypeCatalog>(ChargerSegmentTypes());

        // ViewModels - Core
        services.AddSingleton<ViewModels.Core.ShellViewModel>();

        // ViewModels - Start
        services.AddTransient<StartViewModel>();
        services.AddTransient<CompanySelectorViewModel>();
        services.AddTransient<CreateCompanyViewModel>();

        // ViewModels - Game
        services.AddTransient<DashboardViewModel>();

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

        var provider = services.BuildServiceProvider();

        // Obtenir le NavigationService
        var navigationService = provider.GetRequiredService<INavigationService>();

        // Vérifier si une partie est déjà en cours
        var hasActiveSave = CheckForActiveSave(repositories.GameRepository);

        if (hasActiveSave)
        {
            // Charger directement le Shell si une partie existe
            System.Console.WriteLine("[App] Partie active détectée, chargement du Dashboard...");
            // Le ShellViewModel naviguera automatiquement vers DashboardViewModel
        }
        else
        {
            // Sinon, afficher le menu de démarrage
            System.Console.WriteLine("[App] Aucune partie active, affichage du menu de démarrage...");

            // Initialiser le NavigationService avec StartViewModel AVANT de créer le Shell
            navigationService.NavigateTo<StartViewModel>();
            System.Console.WriteLine($"[App] Navigation vers StartViewModel effectuée");
        }

        // Créer le ShellViewModel (qui observera le NavigationService)
        var shellViewModel = provider.GetRequiredService<ViewModels.Core.ShellViewModel>();
        System.Console.WriteLine($"[App] ShellViewModel créé, CurrentContentViewModel = {shellViewModel.CurrentContentViewModel?.GetType().Name ?? "null"}");

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
            System.Console.Error.WriteLine($"[App] Erreur lors de la vérification de sauvegarde: {ex.Message}");
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
