using FluentAssertions;
using Microsoft.Data.Sqlite;
using Moq;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests.Data.Repositories;

public class AlumniRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SqliteConnectionFactory _factory;
    private readonly AlumniRepository _repository;
    private readonly WorkerRepository _workerRepository;

    public AlumniRepositoryTests()
    {
        // Setup in-memory DB with shared cache to allow multiple connections to see same data
        var connectionString = "DataSource=file:memdb1?mode=memory&cache=shared";
        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        // Setup schema
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE Workers (
                WorkerId TEXT PRIMARY KEY,
                Name TEXT,
                RingName TEXT,
                CompanyId TEXT,
                DepartureDate TEXT,
                DepartureReason TEXT,
                IsHallOfFame INTEGER,
                LegacyScore INTEGER,
                Nationality TEXT,
                Gender TEXT,
                BirthDate TEXT,
                RoleTv TEXT,
                InjuryStatus TEXT,
                Morale INTEGER
            );
            CREATE TABLE WorkerInRingAttributes (WorkerId TEXT, Striking INTEGER, Grappling INTEGER, HighFlying INTEGER, Powerhouse INTEGER, Timing INTEGER, Selling INTEGER, Psychology INTEGER, Stamina INTEGER, Safety INTEGER, HardcoreBrawl INTEGER);
            CREATE TABLE WorkerEntertainmentAttributes (WorkerId TEXT, Charisma INTEGER, MicWork INTEGER, Acting INTEGER, CrowdConnection INTEGER, StarPower INTEGER, Improvisation INTEGER, Entrance INTEGER, SexAppeal INTEGER, MerchandiseAppeal INTEGER, CrossoverPotential INTEGER);
            CREATE TABLE WorkerStoryAttributes (WorkerId TEXT, CharacterDepth INTEGER, Consistency INTEGER, HeelPerformance INTEGER, BabyfacePerformance INTEGER, StorytellingLongTerm INTEGER, EmotionalRange INTEGER, Adaptability INTEGER, RivalryChemistry INTEGER, CreativeInput INTEGER, MoralAlignment INTEGER);
            CREATE TABLE WorkerMentalAttributes (WorkerId TEXT, Ambition INTEGER, Loyauté INTEGER, Professionnalisme INTEGER, Pression INTEGER, Tempérament INTEGER, Égoïsme INTEGER, Détermination INTEGER, Adaptabilité INTEGER, Influence INTEGER, Sportivité INTEGER);
            CREATE TABLE WorkerSpecializations (WorkerId TEXT, Specialization TEXT, Level INTEGER);
            CREATE TABLE WorkerRelations (Id INTEGER PRIMARY KEY, WorkerId1 TEXT, WorkerId2 TEXT, RelationType TEXT, RelationStrength INTEGER, Notes TEXT, IsPublic INTEGER);
            CREATE TABLE ContractHistory (WorkerId TEXT, StartDate TEXT, EndDate TEXT, WeeklySalary REAL, Status TEXT, ContractType TEXT);
            CREATE TABLE MatchHistory (WorkerId TEXT, MatchDate TEXT, MatchType TEXT, Result TEXT, Rating INTEGER);
            CREATE TABLE WorkerNotes (WorkerId TEXT, Text TEXT, Category TEXT, CreatedDate TEXT);
            CREATE TABLE TitleReigns (TitleId TEXT, WorkerId TEXT, DateWon TEXT, DateLost TEXT);
        ";
        command.ExecuteNonQuery();

        _factory = new SqliteConnectionFactory(connectionString);
        _workerRepository = new WorkerRepository(_factory);
        _repository = new AlumniRepository(_factory, _workerRepository);
    }

    [Fact]
    public async Task GetCompanyAlumniAsync_ShouldReturnWorkersWithDepartureDate()
    {
        // Arrange
        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO Workers (WorkerId, Name, CompanyId, DepartureDate, LegacyScore) VALUES 
                ('W1', 'Alumni One', 'C1', '2023-01-01', 80),
                ('W2', 'Active Worker', 'C1', NULL, 50),
                ('W3', 'Other Alumni', 'C2', '2023-01-01', 70);
            ";
            cmd.ExecuteNonQuery();
        }

        // Act
        var alumni = await _repository.GetCompanyAlumniAsync("C1");

        // Assert
        alumni.Should().HaveCount(1);
        alumni.First().Name.Should().Be("Alumni One");
        alumni.First().LegacyScore.Should().Be(80);
    }

    [Fact]
    public async Task GetHallOfFameInducteesAsync_ShouldReturnInductees()
    {
        // Arrange
        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO Workers (WorkerId, Name, IsHallOfFame, CompanyId) VALUES 
                ('H1', 'Legend One', 1, 'C1'),
                ('H2', 'Legend Two', 1, 'GLOBAL'),
                ('H3', 'Regular Joe', 0, 'C1');
            ";
            cmd.ExecuteNonQuery();
        }

        // Act
        var globalHof = await _repository.GetHallOfFameInducteesAsync("GLOBAL");

        // Assert
        globalHof.Should().HaveCount(2); // H1 and H2
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
