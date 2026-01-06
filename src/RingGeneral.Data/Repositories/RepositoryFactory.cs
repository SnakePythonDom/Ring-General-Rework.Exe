using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Factory for creating repository instances with proper dependency injection.
/// </summary>
public static class RepositoryFactory
{
    /// <summary>
    /// Creates a fully configured GameRepository with all specialized repository dependencies.
    /// </summary>
    public static GameRepository CreateGameRepository(SqliteConnectionFactory factory)
    {
        var showRepository = new ShowRepository(factory);
        var companyRepository = new CompanyRepository(factory);
        var workerRepository = new WorkerRepository(factory);
        var backstageRepository = new BackstageRepository(factory);
        var scoutingRepository = new ScoutingRepository(factory);
        var contractRepository = new ContractRepository(factory);
        var settingsRepository = new SettingsRepository(factory);
        var youthRepository = new YouthRepository(factory);

        return new GameRepository(
            factory,
            showRepository,
            companyRepository,
            workerRepository,
            backstageRepository,
            scoutingRepository,
            contractRepository,
            settingsRepository,
            youthRepository);
    }
}
