using RingGeneral.Core.Models;
using Xunit;
using FluentAssertions;
using AutoFixture;

namespace RingGeneral.Tests;

/// <summary>
/// Tests de validation pour les modèles de données
/// </summary>
public class ModelValidationTests
{
    private readonly Fixture _fixture;

    public ModelValidationTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void Worker_ShouldHaveValidDefaultValues()
    {
        // Arrange & Act
        var worker = new Worker();

        // Assert
        worker.Id.Should().NotBeNullOrEmpty();
        worker.Name.Should().NotBeNull();
        worker.Health.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(100);
        worker.Popularity.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(100);
        worker.Morale.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void Worker_Health_ShouldBeClamped_Between0And100()
    {
        // Arrange
        var worker = _fixture.Create<Worker>();

        // Act - Valeur trop haute
        worker.Health = 150;

        // Assert
        worker.Health.Should().Be(100);

        // Act - Valeur trop basse
        worker.Health = -50;

        // Assert
        worker.Health.Should().Be(0);
    }

    [Fact]
    public void Worker_Popularity_ShouldBeClamped_Between0And100()
    {
        // Arrange
        var worker = _fixture.Create<Worker>();

        // Act - Valeur trop haute
        worker.Popularity = 150;

        // Assert
        worker.Popularity.Should().Be(100);

        // Act - Valeur trop basse
        worker.Popularity = -50;

        // Assert
        worker.Popularity.Should().Be(0);
    }

    [Fact]
    public void Worker_Morale_ShouldBeClamped_Between0And100()
    {
        // Arrange
        var worker = _fixture.Create<Worker>();

        // Act - Valeur trop haute
        worker.Morale = 150;

        // Assert
        worker.Morale.Should().Be(100);

        // Act - Valeur trop basse
        worker.Morale = -50;

        // Assert
        worker.Morale.Should().Be(0);
    }

    [Fact]
    public void CompanyState_ShouldHaveValidDefaultValues()
    {
        // Arrange & Act
        var company = new CompanyState();

        // Assert
        company.Id.Should().NotBeNullOrEmpty();
        company.Name.Should().NotBeNull();
        company.Prestige.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(100);
        company.Reach.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(100);
        company.AudienceMoyenne.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void CompanyState_Prestige_ShouldBeClamped_Between0And100()
    {
        // Arrange
        var company = _fixture.Create<CompanyState>();

        // Act - Valeur trop haute
        company.Prestige = 150;

        // Assert
        company.Prestige.Should().Be(100);

        // Act - Valeur trop basse
        company.Prestige = -50;

        // Assert
        company.Prestige.Should().Be(0);
    }

    [Fact]
    public void CompanyState_Reach_ShouldBeClamped_Between0And100()
    {
        // Arrange
        var company = _fixture.Create<CompanyState>();

        // Act - Valeur trop haute
        company.Reach = 150;

        // Assert
        company.Reach.Should().Be(100);

        // Act - Valeur trop basse
        company.Reach = -50;

        // Assert
        company.Reach.Should().Be(0);
    }

    [Fact]
    public void CompanyState_AudienceMoyenne_ShouldBeClamped_Between0And100()
    {
        // Arrange
        var company = _fixture.Create<CompanyState>();

        // Act - Valeur trop haute
        company.AudienceMoyenne = 150;

        // Assert
        company.AudienceMoyenne.Should().Be(100);

        // Act - Valeur trop basse
        company.AudienceMoyenne = -50;

        // Assert
        company.AudienceMoyenne.Should().Be(0);
    }

    [Fact]
    public void ShowContext_ShouldRequireValidCompany()
    {
        // Arrange
        var showContext = _fixture.Create<ShowContext>();

        // Assert
        showContext.Compagnie.Should().NotBeNull();
        showContext.Compagnie.Id.Should().NotBeNullOrEmpty();
        showContext.Compagnie.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ShowContext_ShouldHaveWorkersCollection()
    {
        // Arrange
        var showContext = _fixture.Create<ShowContext>();

        // Assert
        showContext.Workers.Should().NotBeNull();
        showContext.Workers.Should().NotBeEmpty();
    }

    [Fact]
    public void ShowContext_ShouldHaveSegmentsCollection()
    {
        // Arrange
        var showContext = _fixture.Create<ShowContext>();

        // Assert
        showContext.Segments.Should().NotBeNull();
    }

    [Fact]
    public void SegmentDefinition_ShouldHaveValidType()
    {
        // Arrange
        var segment = _fixture.Create<SegmentDefinition>();

        // Assert
        segment.Type.Should().NotBeNullOrEmpty();
        segment.Duration.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SegmentDefinition_Participants_ShouldNotBeNull()
    {
        // Arrange
        var segment = _fixture.Create<SegmentDefinition>();

        // Assert
        segment.Participants.Should().NotBeNull();
    }

    [Fact]
    public void FinanceTransaction_ShouldHaveValidProperties()
    {
        // Arrange
        var transaction = _fixture.Create<FinanceTransaction>();

        // Assert
        transaction.Id.Should().NotBeNullOrEmpty();
        transaction.Description.Should().NotBeNullOrEmpty();
        transaction.Amount.Should().BeOfType(typeof(decimal));
    }

    [Fact]
    public void FinanceTransaction_Amount_ShouldAllowPositiveAndNegativeValues()
    {
        // Arrange
        var transaction = _fixture.Create<FinanceTransaction>();

        // Act & Assert - Valeur positive (revenus)
        transaction.Amount = 1000.50m;
        transaction.Amount.Should().Be(1000.50m);

        // Act & Assert - Valeur négative (dépenses)
        transaction.Amount = -500.25m;
        transaction.Amount.Should().Be(-500.25m);

        // Act & Assert - Valeur zéro
        transaction.Amount = 0m;
        transaction.Amount.Should().Be(0m);
    }

    [Fact]
    public void ContractPayroll_ShouldHaveValidSalary()
    {
        // Arrange
        var contract = _fixture.Create<ContractPayroll>();

        // Assert
        contract.Salaire.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void ContractPayroll_Frequency_ShouldBeValidEnumValue()
    {
        // Arrange
        var contract = _fixture.Create<ContractPayroll>();

        // Assert
        Enum.IsDefined(typeof(PayrollFrequency), contract.Frequence).Should().BeTrue();
    }

    [Theory]
    [InlineData(PayrollFrequency.Hebdomadaire)]
    [InlineData(PayrollFrequency.Mensuelle)]
    public void ContractPayroll_ShouldAcceptValidFrequencies(PayrollFrequency frequency)
    {
        // Arrange
        var contract = _fixture.Create<ContractPayroll>();

        // Act
        contract.Frequence = frequency;

        // Assert
        contract.Frequence.Should().Be(frequency);
    }

    [Fact]
    public void WorkerBackstageProfile_ShouldHaveValidIdAndName()
    {
        // Arrange
        var profile = _fixture.Create<WorkerBackstageProfile>();

        // Assert
        profile.Id.Should().NotBeNullOrEmpty();
        profile.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Storyline_ShouldHaveValidProperties()
    {
        // Arrange
        var storyline = _fixture.Create<Storyline>();

        // Assert
        storyline.Id.Should().NotBeNullOrEmpty();
        storyline.Title.Should().NotBeNullOrEmpty();
        storyline.Heat.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void Storyline_Heat_ShouldBeClamped_Between0And100()
    {
        // Arrange
        var storyline = _fixture.Create<Storyline>();

        // Act - Valeur trop haute
        storyline.Heat = 150;

        // Assert
        storyline.Heat.Should().Be(100);

        // Act - Valeur trop basse
        storyline.Heat = -50;

        // Assert
        storyline.Heat.Should().Be(0);
    }

    [Fact]
    public void Trend_ShouldHaveValidProperties()
    {
        // Arrange
        var trend = _fixture.Create<Trend>();

        // Assert
        trend.Id.Should().NotBeNullOrEmpty();
        trend.Name.Should().NotBeNullOrEmpty();
        trend.Popularity.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void Trend_Popularity_ShouldBeClamped_Between0And100()
    {
        // Arrange
        var trend = _fixture.Create<Trend>();

        // Act - Valeur trop haute
        trend.Popularity = 150;

        // Assert
        trend.Popularity.Should().Be(100);

        // Act - Valeur trop basse
        trend.Popularity = -50;

        // Assert
        trend.Popularity.Should().Be(0);
    }
}