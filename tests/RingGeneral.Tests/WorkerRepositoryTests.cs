using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;
using Xunit;
using FluentAssertions;

namespace RingGeneral.Tests;

/// <summary>
/// Tests d'intégration pour WorkerRepository
/// </summary>
public class WorkerRepositoryTests : RepositoryTestBase
{
    private readonly WorkerRepository _repository;

    public WorkerRepositoryTests()
    {
        _repository = new WorkerRepository(ConnectionFactory);
    }

    [Fact]
    public void ChargerBackstageRoster_ShouldReturnEmptyList_WhenNoWorkers()
    {
        // Arrange
        var companyId = "company1";

        // Act
        var result = _repository.ChargerBackstageRoster(companyId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ChargerBackstageRoster_ShouldReturnWorkers_ForCompany()
    {
        // Arrange
        var companyId = "company1";
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = "worker1",
            ["company_id"] = companyId,
            ["nom"] = "Doe",
            ["prenom"] = "John"
        });
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = "worker2",
            ["company_id"] = companyId,
            ["nom"] = "Smith",
            ["prenom"] = "Jane"
        });
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = "worker3",
            ["company_id"] = "otherCompany",
            ["nom"] = "Brown",
            ["prenom"] = "Bob"
        });

        // Act
        var result = _repository.ChargerBackstageRoster(companyId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Id == "worker1" && r.Name == "Doe John");
        result.Should().Contain(r => r.Id == "worker2" && r.Name == "Smith Jane");
        result.Should().NotContain(r => r.Id == "worker3");
    }

    [Fact]
    public void ChargerMorales_ShouldReturnEmptyDictionary_WhenNoWorkers()
    {
        // Arrange
        var companyId = "company1";

        // Act
        var result = _repository.ChargerMorales(companyId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ChargerMorales_ShouldReturnMorales_ForCompany()
    {
        // Arrange
        var companyId = "company1";
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = "worker1",
            ["company_id"] = companyId,
            ["nom"] = "Doe",
            ["prenom"] = "John",
            ["morale"] = 75
        });
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = "worker2",
            ["company_id"] = companyId,
            ["nom"] = "Smith",
            ["prenom"] = "Jane",
            ["morale"] = 60
        });
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = "worker3",
            ["company_id"] = "otherCompany",
            ["nom"] = "Brown",
            ["prenom"] = "Bob",
            ["morale"] = 80
        });

        // Act
        var result = _repository.ChargerMorales(companyId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainKey("worker1").WhoseValue.Should().Be(75);
        result.Should().ContainKey("worker2").WhoseValue.Should().Be(60);
        result.Should().NotContainKey("worker3");
    }

    [Fact]
    public void ChargerMorale_ShouldReturnMorale_ForWorker()
    {
        // Arrange
        var workerId = "worker1";
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = workerId,
            ["company_id"] = "company1",
            ["nom"] = "Doe",
            ["prenom"] = "John",
            ["morale"] = 85
        });

        // Act
        var result = _repository.ChargerMorale(workerId);

        // Assert
        result.Should().Be(85);
    }

    [Fact]
    public void ChargerMorale_ShouldReturnDefaultMorale_WhenWorkerNotFound()
    {
        // Arrange
        var workerId = "nonexistent";

        // Act
        var result = _repository.ChargerMorale(workerId);

        // Assert
        result.Should().Be(50); // Valeur par défaut
    }

    [Fact]
    public void ModifierMorale_ShouldUpdateMorale_ForWorker()
    {
        // Arrange
        var workerId = "worker1";
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = workerId,
            ["company_id"] = "company1",
            ["nom"] = "Doe",
            ["prenom"] = "John",
            ["morale"] = 50
        });

        // Act
        _repository.ModifierMorale(workerId, 80);

        // Assert
        var morale = _repository.ChargerMorale(workerId);
        morale.Should().Be(80);
    }

    [Fact]
    public void ModifierMorale_ShouldClampValue_Between0And100()
    {
        // Arrange
        var workerId = "worker1";
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = workerId,
            ["company_id"] = "company1",
            ["nom"] = "Doe",
            ["prenom"] = "John",
            ["morale"] = 50
        });

        // Act - Valeur trop haute
        _repository.ModifierMorale(workerId, 150);

        // Assert
        var morale = _repository.ChargerMorale(workerId);
        morale.Should().Be(100);

        // Act - Valeur trop basse
        _repository.ModifierMorale(workerId, -50);

        // Assert
        morale = _repository.ChargerMorale(workerId);
        morale.Should().Be(0);
    }

    [Fact]
    public void ModifierMorales_ShouldUpdateMultipleMorales()
    {
        // Arrange
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = "worker1",
            ["company_id"] = "company1",
            ["nom"] = "Doe",
            ["prenom"] = "John",
            ["morale"] = 50
        });
        InsertTestData("workers", new Dictionary<string, object>
        {
            ["worker_id"] = "worker2",
            ["company_id"] = "company1",
            ["nom"] = "Smith",
            ["prenom"] = "Jane",
            ["morale"] = 40
        });

        var updates = new Dictionary<string, int>
        {
            ["worker1"] = 70,
            ["worker2"] = 60
        };

        // Act
        _repository.ModifierMorales(updates);

        // Assert
        _repository.ChargerMorale("worker1").Should().Be(70);
        _repository.ChargerMorale("worker2").Should().Be(60);
    }
}