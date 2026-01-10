using RingGeneral.UI.Views.Dashboard;
using RingGeneral.UI.ViewModels.Dashboard;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour DashboardView utilisant Avalonia Headless
/// </summary>
public class DashboardViewUITests
{
    [AvaloniaFact]
    public async Task DashboardView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new DashboardViewModel
        {
            CompanyName = "Test Company",
            CurrentDay = 15,
            TotalWorkers = 25,
            ActiveStorylines = 3,
            UpcomingShows = 2,
            CurrentBudget = 150000.50m,
            LatestNews = "Test News"
        };

        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();

        // Attendre que l'UI se rende
        await Task.Delay(100);

        // Assert
        view.Should().NotBeNull();
        view.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public async Task DashboardView_ShouldDisplayCompanyName()
    {
        // Arrange
        var expectedCompanyName = "Test Wrestling Company";
        var viewModel = new DashboardViewModel
        {
            CompanyName = expectedCompanyName
        };

        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche le nom de la compagnie
        var companyNameTextBlock = view.FindControl<TextBlock>("CompanyNameTextBlock") ??
            view.GetVisualDescendants().OfType<TextBlock>()
                .FirstOrDefault(tb => tb.Text == expectedCompanyName);

        companyNameTextBlock.Should().NotBeNull();
        companyNameTextBlock!.Text.Should().Be(expectedCompanyName);
    }

    [AvaloniaFact]
    public async Task DashboardView_ShouldDisplayTotalWorkers()
    {
        // Arrange
        var expectedWorkers = 42;
        var viewModel = new DashboardViewModel
        {
            TotalWorkers = expectedWorkers
        };

        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche le nombre de workers
        var workersTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text == expectedWorkers.ToString())
            .ToList();

        workersTextBlocks.Should().NotBeEmpty();
        workersTextBlocks.First().Text.Should().Be(expectedWorkers.ToString());
    }

    [AvaloniaFact]
    public async Task DashboardView_ShouldDisplayActiveStorylines()
    {
        // Arrange
        var expectedStorylines = 7;
        var viewModel = new DashboardViewModel
        {
            ActiveStorylines = expectedStorylines
        };

        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche le nombre de storylines
        var storylinesTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text == expectedStorylines.ToString())
            .ToList();

        storylinesTextBlocks.Should().NotBeEmpty();
        storylinesTextBlocks.First().Text.Should().Be(expectedStorylines.ToString());
    }

    [AvaloniaFact]
    public async Task DashboardView_ShouldDisplayUpcomingShows()
    {
        // Arrange
        var expectedShows = 3;
        var viewModel = new DashboardViewModel
        {
            UpcomingShows = expectedShows
        };

        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche le nombre de shows
        var showsTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text == expectedShows.ToString())
            .ToList();

        showsTextBlocks.Should().NotBeEmpty();
        showsTextBlocks.First().Text.Should().Be(expectedShows.ToString());
    }

    [AvaloniaFact]
    public async Task DashboardView_ShouldDisplayCurrentBudget()
    {
        // Arrange
        var expectedBudget = 250000.75m;
        var viewModel = new DashboardViewModel
        {
            CurrentBudget = expectedBudget
        };

        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche le budget
        var budgetTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains(expectedBudget.ToString("C")))
            .ToList();

        budgetTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task DashboardView_ShouldDisplayLatestNews()
    {
        // Arrange
        var expectedNews = "Breaking News: Major storyline development!";
        var viewModel = new DashboardViewModel
        {
            LatestNews = expectedNews
        };

        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche les news
        var newsTextBlock = view.GetVisualDescendants().OfType<TextBlock>()
            .FirstOrDefault(tb => tb.Text == expectedNews);

        newsTextBlock.Should().NotBeNull();
        newsTextBlock!.Text.Should().Be(expectedNews);
    }

    [AvaloniaFact]
    public async Task DashboardView_ShouldDisplayMoraleInformation()
    {
        // Arrange
        var expectedMoraleScore = 85;
        var expectedMoraleLabel = "Good";
        var viewModel = new DashboardViewModel
        {
            MoraleScore = expectedMoraleScore
        };

        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le score et le label sont affichés
        var moraleScoreTextBlock = view.GetVisualDescendants().OfType<TextBlock>()
            .FirstOrDefault(tb => tb.Text == expectedMoraleScore.ToString());

        var moraleLabelTextBlock = view.GetVisualDescendants().OfType<TextBlock>()
            .FirstOrDefault(tb => tb.Text == expectedMoraleLabel);

        moraleScoreTextBlock.Should().NotBeNull();
        moraleLabelTextBlock.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task DashboardView_ShouldDisplayCurrentDay()
    {
        // Arrange
        var expectedDay = 42;
        var viewModel = new DashboardViewModel
        {
            CurrentDay = expectedDay
        };

        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche le jour
        var dayTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains($"Jour {expectedDay}"))
            .ToList();

        dayTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task DashboardView_ShouldUpdateWhenViewModelChanges()
    {
        // Arrange
        var viewModel = new DashboardViewModel
        {
            TotalWorkers = 10
        };

        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Changer la valeur dans le ViewModel
        viewModel.TotalWorkers = 25;
        await Task.Delay(50); // Attendre la mise à jour de l'UI

        // Assert - Vérifier que l'UI s'est mise à jour
        var workersTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text == "25")
            .ToList();

        workersTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task DashboardView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new DashboardViewModel();
        var view = new DashboardView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var scrollViewer = view.FindControl<ScrollViewer>("MainScrollViewer") ??
            view.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();

        scrollViewer.Should().NotBeNull();

        var mainStackPanel = scrollViewer!.Content as StackPanel;
        mainStackPanel.Should().NotBeNull();
        mainStackPanel!.Children.Should().NotBeEmpty();

        // Vérifier qu'il y a une grille avec les cartes statistiques
        var grid = mainStackPanel.Children.OfType<Grid>().FirstOrDefault();
        grid.Should().NotBeNull();
        grid!.ColumnDefinitions.Should().NotBeEmpty();
    }
}