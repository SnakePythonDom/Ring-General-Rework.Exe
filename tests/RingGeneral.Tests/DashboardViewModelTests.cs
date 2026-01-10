using RingGeneral.Core.Interfaces;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.ViewModels.Dashboard;
using Xunit;
using FluentAssertions;
using Moq;
using ReactiveUI;

namespace RingGeneral.Tests;

/// <summary>
/// Tests unitaires pour DashboardViewModel
/// </summary>
public class DashboardViewModelTests
{
    private readonly Mock<GameRepository> _mockGameRepository;
    private readonly Mock<IShowSchedulerStore> _mockShowSchedulerStore;
    private readonly Mock<IShowDayOrchestrator> _mockShowDayOrchestrator;
    private readonly Mock<ITimeOrchestratorService> _mockTimeOrchestrator;
    private readonly Mock<IMoraleEngine> _mockMoraleEngine;
    private readonly Mock<ICrisisEngine> _mockCrisisEngine;

    public DashboardViewModelTests()
    {
        _mockGameRepository = new Mock<GameRepository>();
        _mockShowSchedulerStore = new Mock<IShowSchedulerStore>();
        _mockShowDayOrchestrator = new Mock<IShowDayOrchestrator>();
        _mockTimeOrchestrator = new Mock<ITimeOrchestratorService>();
        _mockMoraleEngine = new Mock<IMoraleEngine>();
        _mockCrisisEngine = new Mock<ICrisisEngine>();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var viewModel = new DashboardViewModel();

        // Assert
        viewModel.CompanyName.Should().Be("Ma Compagnie");
        viewModel.CompanyId.Should().BeEmpty();
        viewModel.CurrentDay.Should().Be(1);
        viewModel.TotalWorkers.Should().Be(0);
        viewModel.ActiveStorylines.Should().Be(0);
        viewModel.UpcomingShows.Should().Be(0);
        viewModel.CurrentBudget.Should().Be(0);
        viewModel.LatestNews.Should().Be("Bienvenue dans Ring General !");
        viewModel.MoraleScore.Should().Be(70);
        viewModel.MoraleTrend.Should().Be("Stable");
        viewModel.MoraleLabel.Should().Be("Good");
    }

    [Fact]
    public void Constructor_ShouldAcceptDependencies()
    {
        // Arrange & Act
        var viewModel = new DashboardViewModel(
            _mockGameRepository.Object,
            _mockShowSchedulerStore.Object,
            _mockShowDayOrchestrator.Object,
            _mockTimeOrchestrator.Object,
            _mockMoraleEngine.Object,
            _mockCrisisEngine.Object);

        // Assert
        viewModel.Should().NotBeNull();
    }

    [Fact]
    public void CompanyName_ShouldNotifyPropertyChanged()
    {
        // Arrange
        var viewModel = new DashboardViewModel();
        var propertyChangedCalled = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.CompanyName))
                propertyChangedCalled = true;
        };

        // Act
        viewModel.CompanyName = "Nouvelle Compagnie";

        // Assert
        propertyChangedCalled.Should().BeTrue();
        viewModel.CompanyName.Should().Be("Nouvelle Compagnie");
    }

    [Fact]
    public void CurrentDay_ShouldNotifyPropertyChanged()
    {
        // Arrange
        var viewModel = new DashboardViewModel();
        var propertyChangedCalled = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.CurrentDay))
                propertyChangedCalled = true;
        };

        // Act
        viewModel.CurrentDay = 5;

        // Assert
        propertyChangedCalled.Should().BeTrue();
        viewModel.CurrentDay.Should().Be(5);
    }

    [Fact]
    public void MoraleScore_ShouldUpdateMoraleLabel_Automatically()
    {
        // Arrange
        var viewModel = new DashboardViewModel();

        // Act & Assert - Excellent (90-100)
        viewModel.MoraleScore = 95;
        viewModel.MoraleLabel.Should().Be("Excellent");

        // Act & Assert - Good (70-89)
        viewModel.MoraleScore = 80;
        viewModel.MoraleLabel.Should().Be("Good");

        // Act & Assert - Average (50-69)
        viewModel.MoraleScore = 60;
        viewModel.MoraleLabel.Should().Be("Average");

        // Act & Assert - Poor (30-49)
        viewModel.MoraleScore = 40;
        viewModel.MoraleLabel.Should().Be("Poor");

        // Act & Assert - Terrible (0-29)
        viewModel.MoraleScore = 20;
        viewModel.MoraleLabel.Should().Be("Terrible");
    }

    [Fact]
    public void MoraleScore_ShouldClampValue_Between0And100()
    {
        // Arrange
        var viewModel = new DashboardViewModel();

        // Act - Valeur trop haute
        viewModel.MoraleScore = 150;

        // Assert
        viewModel.MoraleScore.Should().Be(100);

        // Act - Valeur trop basse
        viewModel.MoraleScore = -50;

        // Assert
        viewModel.MoraleScore.Should().Be(0);
    }

    [Fact]
    public void TotalWorkers_ShouldNotifyPropertyChanged()
    {
        // Arrange
        var viewModel = new DashboardViewModel();
        var propertyChangedCalled = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.TotalWorkers))
                propertyChangedCalled = true;
        };

        // Act
        viewModel.TotalWorkers = 25;

        // Assert
        propertyChangedCalled.Should().BeTrue();
        viewModel.TotalWorkers.Should().Be(25);
    }

    [Fact]
    public void CurrentBudget_ShouldNotifyPropertyChanged()
    {
        // Arrange
        var viewModel = new DashboardViewModel();
        var propertyChangedCalled = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.CurrentBudget))
                propertyChangedCalled = true;
        };

        // Act
        viewModel.CurrentBudget = 150000.50m;

        // Assert
        propertyChangedCalled.Should().BeTrue();
        viewModel.CurrentBudget.Should().Be(150000.50m);
    }

    [Fact]
    public void ActiveStorylines_ShouldNotifyPropertyChanged()
    {
        // Arrange
        var viewModel = new DashboardViewModel();
        var propertyChangedCalled = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.ActiveStorylines))
                propertyChangedCalled = true;
        };

        // Act
        viewModel.ActiveStorylines = 3;

        // Assert
        propertyChangedCalled.Should().BeTrue();
        viewModel.ActiveStorylines.Should().Be(3);
    }

    [Fact]
    public void UpcomingShows_ShouldNotifyPropertyChanged()
    {
        // Arrange
        var viewModel = new DashboardViewModel();
        var propertyChangedCalled = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.UpcomingShows))
                propertyChangedCalled = true;
        };

        // Act
        viewModel.UpcomingShows = 2;

        // Assert
        propertyChangedCalled.Should().BeTrue();
        viewModel.UpcomingShows.Should().Be(2);
    }

    [Fact]
    public void LatestNews_ShouldNotifyPropertyChanged()
    {
        // Arrange
        var viewModel = new DashboardViewModel();
        var propertyChangedCalled = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.LatestNews))
                propertyChangedCalled = true;
        };

        // Act
        viewModel.LatestNews = "Nouvelles importantes !";

        // Assert
        propertyChangedCalled.Should().BeTrue();
        viewModel.LatestNews.Should().Be("Nouvelles importantes !");
    }

    [Fact]
    public void ActiveCrisesCount_ShouldUpdateHasCriticalCrises()
    {
        // Arrange
        var viewModel = new DashboardViewModel();

        // Act & Assert - Pas de crises critiques
        viewModel.CriticalCrisesCount = 0;
        viewModel.HasCriticalCrises.Should().BeFalse();

        // Act & Assert - Crises critiques présentes
        viewModel.CriticalCrisesCount = 2;
        viewModel.HasCriticalCrises.Should().BeTrue();
    }

    [Fact]
    public void HasUpcomingShow_ShouldBeTrue_WhenUpcomingShowNameIsSet()
    {
        // Arrange
        var viewModel = new DashboardViewModel();

        // Act
        viewModel.UpcomingShowName = "Show Principal";

        // Assert
        viewModel.HasUpcomingShow.Should().BeTrue();
    }

    [Fact]
    public void HasUpcomingShow_ShouldBeFalse_WhenUpcomingShowNameIsEmpty()
    {
        // Arrange
        var viewModel = new DashboardViewModel();

        // Act
        viewModel.UpcomingShowName = string.Empty;

        // Assert
        viewModel.HasUpcomingShow.Should().BeFalse();
    }

    [Fact]
    public void TrendIcon_ShouldDefaultToRightArrow()
    {
        // Arrange
        var viewModel = new DashboardViewModel();

        // Assert
        viewModel.TrendIcon.Should().Be("➡️");
    }

    [Fact]
    public void CurrentDate_ShouldNotifyPropertyChanged()
    {
        // Arrange
        var viewModel = new DashboardViewModel();
        var propertyChangedCalled = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.CurrentDate))
                propertyChangedCalled = true;
        };
        var newDate = new DateTime(2024, 6, 15);

        // Act
        viewModel.CurrentDate = newDate;

        // Assert
        propertyChangedCalled.Should().BeTrue();
        viewModel.CurrentDate.Should().Be(newDate);
    }
}