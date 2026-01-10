using RingGeneral.Core.Models;
using RingGeneral.Core.Simulation;
using Xunit;
using FluentAssertions;
using AutoFixture;
using Moq;

namespace RingGeneral.Tests;

/// <summary>
/// Tests unitaires pour le moteur de calcul financier
/// </summary>
public class FinanceEngineTests
{
    private readonly Fixture _fixture;
    private readonly FinanceSettings _defaultSettings;

    public FinanceEngineTests()
    {
        _fixture = new Fixture();
        _defaultSettings = CreateDefaultFinanceSettings();
    }

    private FinanceSettings CreateDefaultFinanceSettings()
    {
        return new FinanceSettings
        {
            Venue = new VenueSettings
            {
                CapaciteBase = 1000,
                CapaciteParReach = 10,
                CapaciteParPrestige = 5,
                CapaciteMin = 100,
                CapaciteMax = 10000
            },
            Billetterie = new BilletterieSettings
            {
                TauxRemplissageBase = 0.5,
                TauxRemplissageParPoint = 0.01,
                TauxRemplissageMin = 0.1,
                TauxRemplissageMax = 1.0,
                PrixBase = 20.0,
                PrixParAudience = 0.1,
                PrixParPrestige = 0.5,
                PrixMin = 5.0,
                PrixMax = 200.0
            },
            Merch = new MerchSettings
            {
                DepenseParFan = 5.0,
                MultiplicateurStars = 2.0,
                StarsPrisesEnCompte = 3
            },
            Tv = new TvSettings
            {
                RevenuBase = 1000.0,
                RevenuParAudience = 10.0
            },
            Production = new ProductionSettings
            {
                CoutBase = 500.0,
                CoutParMinute = 5.0,
                CoutParSpectateur = 0.1
            },
            Paie = new PaieSettings
            {
                SemainesParMois = 4
            }
        };
    }

    [Fact]
    public void Constructor_ShouldStoreSettings()
    {
        // Arrange & Act
        var engine = new FinanceEngine(_defaultSettings);

        // Assert - Test indirectement via une méthode publique
        var context = CreateBasicShowFinanceContext();
        var result = engine.CalculerFinancesShow(context);

        result.Should().NotBeNull();
        result.Billetterie.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(50, 0.5)] // Audience moyenne = taux de base
    [InlineData(60, 0.6)] // Audience supérieure = taux supérieur
    [InlineData(40, 0.4)] // Audience inférieure = taux inférieur
    [InlineData(100, 1.0)] // Audience max = taux max
    [InlineData(0, 0.1)]  // Audience min = taux min
    public void CalculerTauxRemplissage_ShouldReturnCorrectRate(int audience, double expectedRate)
    {
        // Arrange
        var engine = new FinanceEngine(_defaultSettings);
        var method = typeof(FinanceEngine).GetMethod("CalculerTauxRemplissage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (double)method!.Invoke(engine, new object[] { audience })!;

        // Assert
        result.Should().Be(expectedRate);
    }

    [Theory]
    [InlineData(1000, 50, 1000)] // Capacité de base + reach/prestige = capacité normale
    [InlineData(100, 10, 100)]   // Capacité minimum
    [InlineData(10000, 100, 10000)] // Capacité maximum
    public void CalculerCapacite_ShouldReturnCorrectCapacity(int expectedCapacity, int reach, int prestige)
    {
        // Arrange
        var engine = new FinanceEngine(_defaultSettings);
        var company = new CompanyState(
            CompagnieId: "C001",
            Nom: "Test Company",
            Region: "US",
            Prestige: prestige,
            Tresorerie: 1000000.0,
            AudienceMoyenne: 50,
            Reach: reach
        );
        var method = typeof(FinanceEngine).GetMethod("CalculerCapacite",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (int)method!.Invoke(engine, new object[] { company })!;

        // Assert
        result.Should().Be(expectedCapacity);
    }

    [Theory]
    [InlineData(50, 20.0)]  // Audience moyenne = prix de base
    [InlineData(60, 21.0)]  // Audience supérieure = prix supérieur
    [InlineData(40, 19.0)]  // Audience inférieure = prix inférieur
    [InlineData(0, 5.0)]    // Prix minimum
    [InlineData(1000, 200.0)] // Prix maximum (avec prestige)
    public void CalculerPrixBillet_ShouldReturnCorrectPrice(int audience, double expectedPrice)
    {
        // Arrange
        var engine = new FinanceEngine(_defaultSettings);
        var company = new CompanyState(
            CompagnieId: "C002",
            Nom: "Test Company",
            Region: "US",
            Prestige: audience >= 1000 ? 200 : 0,
            Tresorerie: 1000000.0,
            AudienceMoyenne: audience,
            Reach: 50
        );
        var method = typeof(FinanceEngine).GetMethod("CalculerPrixBillet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (double)method!.Invoke(engine, new object[] { company, audience })!;

        // Assert
        result.Should().Be(expectedPrice);
    }

    [Theory]
    [InlineData(100, new[] { 80, 70, 60 }, 750.0)] // 3 stars avec moyenne 70
    [InlineData(100, new[] { 100, 90 }, 900.0)]   // 2 stars avec moyenne 95
    [InlineData(100, new int[] { }, 500.0)]       // Aucune star
    [InlineData(0, new[] { 100, 90, 80 }, 0.0)]  // Aucun spectateur
    public void CalculerMerch_ShouldReturnCorrectAmount(int attendance, int[] popularities, double expectedAmount)
    {
        // Arrange
        var engine = new FinanceEngine(_defaultSettings);
        var method = typeof(FinanceEngine).GetMethod("CalculerMerch",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (double)method!.Invoke(engine, new object[] { attendance, popularities })!;

        // Assert
        result.Should().Be(expectedAmount);
    }

    [Fact]
    public void CalculerFinancesShow_ShouldCalculateCorrectly_WithBasicShow()
    {
        // Arrange
        var engine = new FinanceEngine(_defaultSettings);
        var context = CreateBasicShowFinanceContext();

        // Act
        var result = engine.CalculerFinancesShow(context);

        // Assert
        result.Should().NotBeNull();
        result.Billetterie.Should().BeGreaterThan(0);
        result.Merch.Should().BeGreaterThan(0);
        result.CoutsProduction.Should().BeGreaterThan(0);
        result.Transactions.Should().NotBeEmpty();
        result.Transactions.Should().Contain(t => t.Description == "Billetterie");
        result.Transactions.Should().Contain(t => t.Description == "Merchandising");
        result.Transactions.Should().Contain(t => t.Description == "Coûts de production");
    }

    [Fact]
    public void CalculerFinancesShow_ShouldIncludeTvRevenue_WhenTvBroadcast()
    {
        // Arrange
        var engine = new FinanceEngine(_defaultSettings);
        var context = CreateBasicShowFinanceContext(diffuseTv: true);

        // Act
        var result = engine.CalculerFinancesShow(context);

        // Assert
        result.Tv.Should().BeGreaterThan(0);
        result.Transactions.Should().Contain(t => t.Description == "Droits TV");
    }

    [Fact]
    public void CalculerFinancesShow_ShouldNotIncludeTvRevenue_WhenNoTvBroadcast()
    {
        // Arrange
        var engine = new FinanceEngine(_defaultSettings);
        var context = CreateBasicShowFinanceContext(diffuseTv: false);

        // Act
        var result = engine.CalculerFinancesShow(context);

        // Assert
        result.Tv.Should().Be(0);
        result.Transactions.Should().NotContain(t => t.Description == "Droits TV");
    }

    [Theory]
    [InlineData(PayrollFrequency.Hebdomadaire, 1, 100.0)]     // Paiement hebdomadaire
    [InlineData(PayrollFrequency.Mensuelle, 4, 100.0)]        // Paiement mensuel à la 4ème semaine
    [InlineData(PayrollFrequency.Mensuelle, 1, 0.0)]          // Pas de paiement mensuel à la 1ère semaine
    public void CalculerPaie_ShouldCalculateCorrectPayroll(PayrollFrequency frequency, int week, double expectedPay)
    {
        // Arrange
        var engine = new FinanceEngine(_defaultSettings);
        var contracts = new[]
        {
            new ContractPayroll { Frequence = frequency, Salaire = 100.0m }
        };
        var method = typeof(FinanceEngine).GetMethod("CalculerPaie",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (double)method!.Invoke(engine, new object[] { week, contracts })!;

        // Assert
        result.Should().Be(expectedPay);
    }

    [Fact]
    public void CalculerFinancesHebdo_ShouldIncludePayroll_WhenContractsExist()
    {
        // Arrange
        var engine = new FinanceEngine(_defaultSettings);
        var contracts = new[]
        {
            new ContractPayroll { Frequence = PayrollFrequency.Hebdomadaire, Salaire = 1000.0m },
            new ContractPayroll { Frequence = PayrollFrequency.Mensuelle, Salaire = 500.0m }
        };
        var context = new WeeklyFinanceContext(1, contracts);

        // Act
        var result = engine.CalculerFinancesHebdo(context);

        // Assert
        result.Depenses.Should().Be(1000.0m); // Seulement le contrat hebdomadaire
        result.Transactions.Should().Contain(t => t.Description == "Paie des contrats");
    }

    [Fact]
    public void CalculerFinancesHebdo_ShouldHandleNoContracts()
    {
        // Arrange
        var engine = new FinanceEngine(_defaultSettings);
        var context = new WeeklyFinanceContext(1, Array.Empty<ContractPayroll>());

        // Act
        var result = engine.CalculerFinancesHebdo(context);

        // Assert
        result.Depenses.Should().Be(0m);
        result.Transactions.Should().BeEmpty();
    }

    private ShowFinanceContext CreateBasicShowFinanceContext(bool diffuseTv = false)
    {
        return new ShowFinanceContext
        {
            Compagnie = new CompanyState(
                CompagnieId: "C003",
                Nom: "Test Company",
                Region: "US",
                Prestige: 50,
                Tresorerie: 1000000.0,
                AudienceMoyenne: 50,
                Reach: 50
            ),
            Audience = 50,
            DureeMinutes = 120,
            PopularitesWorkers = new[] { 80, 70, 60 },
            DiffuseTv = diffuseTv
        };
    }
}