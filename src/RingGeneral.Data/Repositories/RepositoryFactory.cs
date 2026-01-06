using RingGeneral.Core.Interfaces;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Container for all repository instances.
/// </summary>
public sealed class RepositoryContainer
{
    public GameRepository GameRepository { get; }
    public ShowRepository ShowRepository { get; }
    public CompanyRepository CompanyRepository { get; }
    public WorkerRepository WorkerRepository { get; }
    public BackstageRepository BackstageRepository { get; }
    public IScoutingRepository ScoutingRepository { get; }
    public IContractRepository ContractRepository { get; }
    public SettingsRepository SettingsRepository { get; }
    public YouthRepository YouthRepository { get; }

    public RepositoryContainer(
        GameRepository gameRepository,
        ShowRepository showRepository,
        CompanyRepository companyRepository,
        WorkerRepository workerRepository,
        BackstageRepository backstageRepository,
        ScoutingRepository scoutingRepository,
        ContractRepository contractRepository,
        SettingsRepository settingsRepository,
        YouthRepository youthRepository)
    {
        GameRepository = gameRepository;
        ShowRepository = showRepository;
        CompanyRepository = companyRepository;
        WorkerRepository = workerRepository;
        BackstageRepository = backstageRepository;
        ScoutingRepository = scoutingRepository;
        ContractRepository = contractRepository;
        SettingsRepository = settingsRepository;
        YouthRepository = youthRepository;
    }
}

/// <summary>
/// Factory for creating repository instances with proper dependency injection.
/// </summary>
public static class RepositoryFactory
{
    /// <summary>
    /// Creates all repositories with proper dependencies.
    /// </summary>
    public static RepositoryContainer CreateRepositories(SqliteConnectionFactory factory)
    {
        var showRepository = new ShowRepository(factory);
        var companyRepository = new CompanyRepository(factory);
        var workerRepository = new WorkerRepository(factory);
        var backstageRepository = new BackstageRepository(factory);
        var scoutingRepository = new ScoutingRepository(factory);
        var contractRepository = new ContractRepository(factory);
        var settingsRepository = new SettingsRepository(factory);
        var youthRepository = new YouthRepository(factory);

        var gameRepository = new GameRepository(
            factory,
            showRepository,
            companyRepository,
            workerRepository,
            backstageRepository,
            scoutingRepository,
            contractRepository,
            settingsRepository,
            youthRepository);

        return new RepositoryContainer(
            gameRepository,
            showRepository,
            companyRepository,
            workerRepository,
            backstageRepository,
            scoutingRepository,
            contractRepository,
            settingsRepository,
            youthRepository);
    }

    /// <summary>
    /// Creates a fully configured GameRepository with all specialized repository dependencies.
    /// </summary>
    [Obsolete("Use CreateRepositories() instead to access specialized repositories directly")]
    public static GameRepository CreateGameRepository(SqliteConnectionFactory factory)
    {
        return CreateRepositories(factory).GameRepository;
    }
}
