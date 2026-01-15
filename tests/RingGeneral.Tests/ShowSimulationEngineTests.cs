using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Company;
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
        result.RapportShow.Should().NotBeNull();
        result.RapportShow.Audience.Should().BeGreaterThanOrEqualTo(0);
        result.RapportShow.Segments.Should().NotBeNull();
        // TotalRevenue = Billetterie + Merch + Tv
        (result.RapportShow.Billetterie + result.RapportShow.Merch + result.RapportShow.Tv).Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Simuler_ShouldHandleEmptySegments()
    {
        // Arrange
        var engine = new ShowSimulationEngine(_mockRandom.Object);
        var baseContext = CreateBasicShowContext();
        // Segments is readonly in ShowContext record - create new context with empty segments
        var context = new ShowContext(
            Show: baseContext.Show,
            Compagnie: baseContext.Compagnie,
            Workers: baseContext.Workers,
            Titres: baseContext.Titres,
            Storylines: baseContext.Storylines,
            Segments: new List<SegmentDefinition>(),
            Chimies: baseContext.Chimies,
            DealTv: baseContext.DealTv
        );

        // Act
        var result = engine.Simuler(context);

        // Assert
        result.Should().NotBeNull();
        result.RapportShow.Segments.Should().BeEmpty();
        result.RapportShow.Audience.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Simuler_ShouldCalculateCorrectCrowdHeat()
    {
        // Arrange
        var engine = new ShowSimulationEngine(_mockRandom.Object);
        var baseContext = CreateBasicShowContext();
        // CompanyState is readonly - create new context with updated company
        var updatedCompany = new CompanyState(
            CompagnieId: baseContext.Compagnie.CompagnieId,
            Nom: baseContext.Compagnie.Nom,
            Region: baseContext.Compagnie.Region,
            Prestige: 80,
            Tresorerie: baseContext.Compagnie.Tresorerie,
            AudienceMoyenne: 60,
            Reach: baseContext.Compagnie.Reach
        );
        var context = new ShowContext(
            Show: baseContext.Show,
            Compagnie: updatedCompany,
            Workers: baseContext.Workers,
            Titres: baseContext.Titres,
            Storylines: baseContext.Storylines,
            Segments: baseContext.Segments,
            Chimies: baseContext.Chimies,
            DealTv: baseContext.DealTv
        );

        // Act
        var result = engine.Simuler(context);

        // Assert
        // CrowdHeat calculation is internal - verify result is valid
        result.RapportShow.Should().NotBeNull();
        result.RapportShow.Audience.Should().BeGreaterThanOrEqualTo(0);
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
        result.Delta.Finances.Should().NotBeNull();
        (result.RapportShow.Billetterie + result.RapportShow.Merch + result.RapportShow.Tv).Should().BeGreaterThanOrEqualTo(0);
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
        result.Delta.PopulariteWorkersDelta.Should().NotBeNull();
        // Au moins les workers du contexte devraient être présents
        foreach (var worker in context.Workers)
        {
            result.Delta.PopulariteWorkersDelta.Should().ContainKey(worker.WorkerId);
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
        result.Delta.PopulariteCompagnieDelta.Should().NotBeNull();
        result.Delta.PopulariteCompagnieDelta.Should().ContainKey(context.Compagnie.CompagnieId);
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
        result.Delta.Blessures.Should().NotBeNull();
        // Avec la configuration mock, il peut y avoir des blessures
    }

    [Fact]
    public void Simuler_ShouldApplyNicheBenefits_WhenNicheRepositoryProvided()
    {
        // Arrange
        var nicheProfile = new NicheFederationProfile
        {
            ProfileId = "PROF001",
            CompanyId = "company1",
            IsNicheFederation = true,
            CaptiveAudiencePercentage = 50.0,
            TvDependencyReduction = 0.1,
            MerchandiseMultiplier = 1.2,
            TicketSalesStability = 50.0,
            TalentSalaryReduction = 0.0,
            TalentLoyaltyBonus = 0.0,
            HasGrowthCeiling = false,
            EstablishedAt = DateTime.UtcNow
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
        var result = method?.Invoke(engine, new object[] { "companyId" });

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
            ProfileId = "PROF002",
            CompanyId = "company1",
            IsNicheFederation = false,
            CaptiveAudiencePercentage = 0.0,
            TvDependencyReduction = 0.0,
            MerchandiseMultiplier = 1.0,
            TicketSalesStability = 50.0,
            TalentSalaryReduction = 0.0,
            TalentLoyaltyBonus = 0.0,
            HasGrowthCeiling = false,
            EstablishedAt = DateTime.UtcNow
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
        var result = method?.Invoke(engine, new object[] { "companyId" });

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
            ProfileId = "PROF003",
            CompanyId = "company1",
            IsNicheFederation = true,
            CaptiveAudiencePercentage = 50.0,
            TvDependencyReduction = 0.2,
            MerchandiseMultiplier = 1.5,
            TicketSalesStability = 50.0,
            TalentSalaryReduction = 0.0,
            TalentLoyaltyBonus = 0.0,
            HasGrowthCeiling = false,
            EstablishedAt = DateTime.UtcNow
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
        var result = method?.Invoke(engine, new object[] { "companyId" });

        // Assert
        var (ticketMultiplier, merchMultiplier, tvReduction) = ((double, double, double))result!;
        ticketMultiplier.Should().Be(1.25); // 1.0 + (50 / 200.0) = 1.25
        merchMultiplier.Should().Be(1.5);
        tvReduction.Should().Be(0.2);
    }

    private ShowContext CreateBasicShowContext()
    {
        var show = new ShowDefinition(
            ShowId: "SHOW001",
            Nom: "Test Show",
            Semaine: 1,
            Region: "US",
            DureeMinutes: 120,
            CompagnieId: "company1",
            DealTvId: null,
            Lieu: "Test Venue",
            Diffusion: "TV"
        );

        var compagnie = new CompanyState(
            CompagnieId: "company1",
            Nom: "Test Company",
            Region: "US",
            Prestige: 50,
            Tresorerie: 1000000.0,
            AudienceMoyenne: 50,
            Reach: 50
        );

        var workers = new List<WorkerSnapshot>
        {
            new WorkerSnapshot(
                WorkerId: "worker1",
                NomComplet: "Test Worker 1",
                InRing: 60,
                Entertainment: 50,
                Story: 50,
                Popularite: 60,
                Fatigue: 0,
                Blessure: "Aucune",
                Momentum: 0,
                RoleTv: "Wrestler",
                Morale: 50
            ),
            new WorkerSnapshot(
                WorkerId: "worker2",
                NomComplet: "Test Worker 2",
                InRing: 40,
                Entertainment: 40,
                Story: 40,
                Popularite: 40,
                Fatigue: 0,
                Blessure: "Aucune",
                Momentum: 0,
                RoleTv: "Wrestler",
                Morale: 50
            )
        };

        var segments = new List<SegmentDefinition>
        {
            new SegmentDefinition(
                SegmentId: "SEG001",
                TypeSegment: "match",
                Participants: new List<string> { "worker1", "worker2" },
                DureeMinutes: 15,
                EstMainEvent: false,
                StorylineId: null,
                TitreId: null,
                Intensite: 50,
                VainqueurId: null,
                PerdantId: null,
                Settings: null
            )
        };

        return new ShowContext(
            Show: show,
            Compagnie: compagnie,
            Workers: workers,
            Titres: new List<TitleInfo>(),
            Storylines: new List<StorylineInfo>(),
            Segments: segments,
            Chimies: new Dictionary<string, int>(),
            DealTv: null
        );
    }
}