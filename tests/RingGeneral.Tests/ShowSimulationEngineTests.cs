using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Simulation;
using Xunit;
using FluentAssertions;
using Moq;
using AutoFixture;

namespace RingGeneral.Tests;

/// <summary>
/// Tests unitaires pour le moteur de simulation de show
/// </summary>
public class ShowSimulationEngineTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IRandomProvider> _mockRandom;
    private readonly Mock<ITrendRepository> _mockTrendRepository;
    private readonly Mock<IRosterAnalysisRepository> _mockRosterAnalysisRepository;
    private readonly Mock<INicheFederationRepository> _mockNicheRepository;

    public ShowSimulationEngineTests()
    {
        _fixture = new Fixture();
        _mockRandom = new Mock<IRandomProvider>();
        _mockTrendRepository = new Mock<ITrendRepository>();
        _mockRosterAnalysisRepository = new Mock<IRosterAnalysisRepository>();
        _mockNicheRepository = new Mock<INicheFederationRepository>();

        // Configuration par défaut du mock random
        _mockRandom.Setup(r => r.NextDouble()).Returns(0.5);
        _mockRandom.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>())).Returns(5);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDependencies()
    {
        // Arrange & Act
        var engine = new ShowSimulationEngine(
            _mockRandom.Object,
            trendRepository: _mockTrendRepository.Object,
            rosterAnalysisRepository: _mockRosterAnalysisRepository.Object,
            nicheRepository: _mockNicheRepository.Object);

        // Assert
        engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldUseDefaultModels_WhenNotProvided()
    {
        // Arrange & Act
        var engine = new ShowSimulationEngine(_mockRandom.Object);

        // Assert
        engine.Should().NotBeNull();
        // Test indirect via simulation
        var context = CreateBasicShowContext();
        var result = engine.Simuler(context);
        result.Should().NotBeNull();
    }

    [Fact]
    public void Simuler_ShouldReturnValidResult_ForBasicShow()
    {
        // Arrange
        var engine = new ShowSimulationEngine(_mockRandom.Object);
        var context = CreateBasicShowContext();

        // Act
        var result = engine.Simuler(context);

        // Assert
        result.Should().NotBeNull();
        result.CrowdHeat.Should().BeGreaterThanOrEqualTo(0);
        result.TotalAttendance.Should().BeGreaterThanOrEqualTo(0);
        result.TotalRevenue.Should().BeGreaterThanOrEqualTo(0);
        result.SegmentsReports.Should().NotBeNull();
    }

    [Fact]
    public void Simuler_ShouldHandleEmptySegments()
    {
        // Arrange
        var engine = new ShowSimulationEngine(_mockRandom.Object);
        var context = CreateBasicShowContext();
        context.Segments = new List<SegmentDefinition>();

        // Act
        var result = engine.Simuler(context);

        // Assert
        result.Should().NotBeNull();
        result.SegmentsReports.Should().BeEmpty();
        result.TotalAttendance.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Simuler_ShouldCalculateCorrectCrowdHeat()
    {
        // Arrange
        var engine = new ShowSimulationEngine(_mockRandom.Object);
        var context = CreateBasicShowContext();
        context.Compagnie.Prestige = 80;
        context.Compagnie.AudienceMoyenne = 60;

        // Act
        var result = engine.Simuler(context);

        // Assert
        // CrowdHeat = (Prestige + AudienceMoyenne) / 2 = (80 + 60) / 2 = 70
        result.CrowdHeat.Should().Be(70);
    }

    [Fact]
    public void Simuler_ShouldIncludeFinancialTransactions()
    {
        // Arrange
        var engine = new ShowSimulationEngine(_mockRandom.Object);
        var context = CreateBasicShowContext();

        // Act
        var result = engine.Simuler(context);

        // Assert
        result.Finances.Should().NotBeNull();
        result.TotalRevenue.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Simuler_ShouldTrackWorkerPopularityChanges()
    {
        // Arrange
        var engine = new ShowSimulationEngine(_mockRandom.Object);
        var context = CreateBasicShowContext();

        // Act
        var result = engine.Simuler(context);

        // Assert
        result.PopulariteWorkers.Should().NotBeNull();
        // Au moins les workers du contexte devraient être présents
        foreach (var worker in context.Workers)
        {
            result.PopulariteWorkers.Should().ContainKey(worker.Id);
        }
    }

    [Fact]
    public void Simuler_ShouldTrackCompanyPopularityChanges()
    {
        // Arrange
        var engine = new ShowSimulationEngine(_mockRandom.Object);
        var context = CreateBasicShowContext();

        // Act
        var result = engine.Simuler(context);

        // Assert
        result.PopulariteCompagnie.Should().NotBeNull();
        result.PopulariteCompagnie.Should().ContainKey(context.Compagnie.Id);
    }

    [Fact]
    public void Simuler_ShouldHandleInjuries()
    {
        // Arrange
        _mockRandom.Setup(r => r.NextDouble()).Returns(0.01); // Haute probabilité de blessure
        var engine = new ShowSimulationEngine(_mockRandom.Object);
        var context = CreateBasicShowContext();

        // Act
        var result = engine.Simuler(context);

        // Assert
        result.Blessures.Should().NotBeNull();
        // Avec la configuration mock, il peut y avoir des blessures
    }

    [Fact]
    public void Simuler_ShouldApplyNicheBenefits_WhenNicheRepositoryProvided()
    {
        // Arrange
        var nicheProfile = new NicheFederationProfile
        {
            IsNicheFederation = true,
            TicketSalesStability = 50,
            MerchandiseMultiplier = 1.2,
            TvDependencyReduction = 0.1
        };

        _mockNicheRepository
            .Setup(r => r.GetNicheFederationProfileByCompanyIdAsync(It.IsAny<string>()))
            .ReturnsAsync(nicheProfile);

        var engine = new ShowSimulationEngine(
            _mockRandom.Object,
            nicheRepository: _mockNicheRepository.Object);

        var context = CreateBasicShowContext();

        // Act
        var result = engine.Simuler(context);

        // Assert
        result.Should().NotBeNull();
        // Les bénéfices de niche devraient être appliqués
    }

    [Fact]
    public void Simuler_ShouldHandleNicheRepositoryException()
    {
        // Arrange
        _mockNicheRepository
            .Setup(r => r.GetNicheFederationProfileByCompanyIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Repository error"));

        var engine = new ShowSimulationEngine(
            _mockRandom.Object,
            nicheRepository: _mockNicheRepository.Object);

        var context = CreateBasicShowContext();

        // Act
        var result = engine.Simuler(context);

        // Assert
        result.Should().NotBeNull();
        // Devrait continuer malgré l'erreur
    }

    [Fact]
    public void CalculateNicheBenefits_ShouldReturnDefaultValues_WhenNoNicheRepository()
    {
        // Arrange
        var engine = new ShowSimulationEngine(_mockRandom.Object);
        var method = typeof(ShowSimulationEngine).GetMethod("CalculateNicheBenefits",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = method!.Invoke(engine, new object[] { "companyId" });

        // Assert
        var (ticketMultiplier, merchMultiplier, tvReduction) = ((double, double, double))result!;
        ticketMultiplier.Should().Be(1.0);
        merchMultiplier.Should().Be(1.0);
        tvReduction.Should().Be(0.0);
    }

    [Fact]
    public void CalculateNicheBenefits_ShouldReturnDefaultValues_WhenNotNicheFederation()
    {
        // Arrange
        var nicheProfile = new NicheFederationProfile
        {
            IsNicheFederation = false,
            TicketSalesStability = 50
        };

        _mockNicheRepository
            .Setup(r => r.GetNicheFederationProfileByCompanyIdAsync(It.IsAny<string>()))
            .ReturnsAsync(nicheProfile);

        var engine = new ShowSimulationEngine(
            _mockRandom.Object,
            nicheRepository: _mockNicheRepository.Object);

        var method = typeof(ShowSimulationEngine).GetMethod("CalculateNicheBenefits",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = method!.Invoke(engine, new object[] { "companyId" });

        // Assert
        var (ticketMultiplier, merchMultiplier, tvReduction) = ((double, double, double))result!;
        ticketMultiplier.Should().Be(1.0);
        merchMultiplier.Should().Be(1.0);
        tvReduction.Should().Be(0.0);
    }

    [Fact]
    public void CalculateNicheBenefits_ShouldApplyNicheMultipliers_WhenNicheFederation()
    {
        // Arrange
        var nicheProfile = new NicheFederationProfile
        {
            IsNicheFederation = true,
            TicketSalesStability = 50,
            MerchandiseMultiplier = 1.5,
            TvDependencyReduction = 0.2
        };

        _mockNicheRepository
            .Setup(r => r.GetNicheFederationProfileByCompanyIdAsync(It.IsAny<string>()))
            .ReturnsAsync(nicheProfile);

        var engine = new ShowSimulationEngine(
            _mockRandom.Object,
            nicheRepository: _mockNicheRepository.Object);

        var method = typeof(ShowSimulationEngine).GetMethod("CalculateNicheBenefits",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = method!.Invoke(engine, new object[] { "companyId" });

        // Assert
        var (ticketMultiplier, merchMultiplier, tvReduction) = ((double, double, double))result!;
        ticketMultiplier.Should().Be(1.25); // 1.0 + (50 / 200.0) = 1.25
        merchMultiplier.Should().Be(1.5);
        tvReduction.Should().Be(0.2);
    }

    private ShowContext CreateBasicShowContext()
    {
        return new ShowContext
        {
            Compagnie = new CompanyState
            {
                Id = "company1",
                Prestige = 50,
                AudienceMoyenne = 50,
                Reach = 50
            },
            Workers = new List<Worker>
            {
                new Worker { Id = "worker1", Name = "Test Worker 1", Popularity = 60 },
                new Worker { Id = "worker2", Name = "Test Worker 2", Popularity = 40 }
            },
            Segments = new List<SegmentDefinition>
            {
                new SegmentDefinition
                {
                    Type = "match",
                    Duration = 15,
                    Participants = new List<string> { "worker1", "worker2" }
                }
            },
            Storylines = new Dictionary<string, Storyline>(),
            CurrentTrends = new List<Trend>(),
            DiffuseTv = false
        };
    }
}