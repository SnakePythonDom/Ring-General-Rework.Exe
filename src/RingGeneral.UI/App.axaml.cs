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

        // ViewModels
        services.AddSingleton<ViewModels.Core.ShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<BookingViewModel>();
        services.AddTransient<RosterViewModel>();
        services.AddTransient<ViewModels.Roster.WorkerDetailViewModel>();
        services.AddTransient<ViewModels.Roster.TitlesViewModel>();
        services.AddTransient<StorylinesViewModel>();
        services.AddTransient<YouthViewModel>();
        services.AddTransient<FinanceViewModel>();
        services.AddTransient<CalendarViewModel>();

        var provider = services.BuildServiceProvider();

        // Lancer le Shell
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(
                provider.GetRequiredService<ViewModels.Core.ShellViewModel>()
            );
        }

        base.OnFrameworkInitializationCompleted();
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
