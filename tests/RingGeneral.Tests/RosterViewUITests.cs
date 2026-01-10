using RingGeneral.UI.Views.Roster;
using RingGeneral.UI.ViewModels.Roster;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour RosterView utilisant Avalonia Headless
/// </summary>
public class RosterViewUITests
{
    [AvaloniaFact]
    public async Task RosterView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new RosterViewModel();
        // TotalWorkers is readonly - add workers to update it
        for (int i = 0; i < 42; i++)
        {
            viewModel.Workers.Add(new WorkerListItemViewModel { WorkerId = $"W{i}", Name = $"Worker {i}" });
        }

        var view = new RosterView { DataContext = viewModel };

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
    public async Task RosterView_ShouldDisplayTotalWorkers()
    {
        // Arrange
        var expectedTotal = 156;
        var viewModel = new RosterViewModel();
        // TotalWorkers is readonly - add workers to update it
        for (int i = 0; i < expectedTotal; i++)
        {
            viewModel.Workers.Add(new WorkerListItemViewModel { WorkerId = $"W{i}", Name = $"Worker {i}" });
        }

        var view = new RosterView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche le nombre total de workers
        var totalTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains($"{expectedTotal} workers au total"))
            .ToList();

        totalTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task RosterView_ShouldDisplaySearchTextBox()
    {
        // Arrange
        var viewModel = new RosterViewModel();
        var view = new RosterView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du TextBox de recherche
        var searchTextBox = view.FindControl<TextBox>("SearchTextBox") ??
            view.GetVisualDescendants().OfType<TextBox>()
                .FirstOrDefault(tb => tb.Watermark?.Contains("Rechercher") == true);

        searchTextBox.Should().NotBeNull();
        searchTextBox?.Watermark.Should().Contain("Rechercher");
    }

    [AvaloniaFact]
    public async Task RosterView_ShouldDisplayWorkersDataGrid()
    {
        // Arrange
        var viewModel = new RosterViewModel();
        viewModel.Workers.Add(new WorkerListItemViewModel
        {
            Name = "Test Worker",
            Role = "Wrestler",
            Popularity = 75,
            // Health property doesn't exist in WorkerListItemViewModel
        });

        var view = new RosterView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du DataGrid
        var dataGrid = view.FindControl<DataGrid>("WorkersDataGrid") ??
            view.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();

        dataGrid.Should().NotBeNull();
        dataGrid?.ItemsSource.Should().NotBeNull();
        ((System.Collections.IEnumerable?)dataGrid.ItemsSource)?.Cast<object>().Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task RosterView_ShouldDisplayFiltersButton()
    {
        // Arrange
        var viewModel = new RosterViewModel();
        var view = new RosterView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Filtres
        var filtersButton = view.FindControl<Button>("FiltersButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString() == "Filtres");

        filtersButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task RosterView_ShouldDisplayWorkerDetails()
    {
        // Arrange
        var viewModel = new RosterViewModel();
        var testWorker = new WorkerListItemViewModel
        {
            Name = "John Cena",
            Role = "Main Event Star",
            Popularity = 95,
            Morale = 80
            // Health and Age properties don't exist in WorkerListItemViewModel
        };
        viewModel.Workers.Add(testWorker);
        viewModel.SelectedWorker = testWorker;

        var view = new RosterView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les détails du worker sont affichés
        var nameTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text == "John Cena")
            .ToList();

        nameTextBlocks.Should().NotBeEmpty();

        // Vérifier que le rôle est affiché
        var roleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text == "Main Event Star")
            .ToList();

        roleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task RosterView_ShouldDisplayWorkerStatistics()
    {
        // Arrange
        var viewModel = new RosterViewModel();
        var testWorker = new WorkerListItemViewModel
        {
            Name = "Test Worker",
            Popularity = 78,
            // Health property doesn't exist
            Morale = 85
        };
        viewModel.Workers.Add(testWorker);

        var view = new RosterView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les statistiques sont affichées
        var popularityTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text == "78")
            .ToList();

        var healthTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text == "92")
            .ToList();

        var moraleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text == "85")
            .ToList();

        popularityTextBlocks.Should().NotBeEmpty();
        healthTextBlocks.Should().NotBeEmpty();
        moraleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task RosterView_ShouldHandleSearchTextBinding()
    {
        // Arrange
        var viewModel = new RosterViewModel();
        var view = new RosterView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Trouver le TextBox de recherche
        var searchTextBox = view.GetVisualDescendants().OfType<TextBox>()
            .FirstOrDefault(tb => tb.Watermark?.Contains("Rechercher") == true);

        // Simuler la saisie de texte
        if (searchTextBox != null)
        {
            searchTextBox.Text = "Cena";
        }
        await Task.Delay(50);

        // Assert - Vérifier que le binding fonctionne
        viewModel.SearchText.Should().Be("Cena");
    }

    [AvaloniaFact]
    public async Task RosterView_ShouldDisplayWorkerSelection()
    {
        // Arrange
        var viewModel = new RosterViewModel();
        var worker1 = new WorkerListItemViewModel { Name = "Worker 1", Role = "Wrestler" };
        var worker2 = new WorkerListItemViewModel { Name = "Worker 2", Role = "Manager" };

        viewModel.Workers.Add(worker1);
        viewModel.Workers.Add(worker2);
        viewModel.SelectedWorker = worker1;

        var view = new RosterView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la sélection fonctionne
        var dataGrid = view.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();
        dataGrid.Should().NotBeNull();
        dataGrid?.SelectedItem.Should().Be(worker1);
    }

    [AvaloniaFact]
    public async Task RosterView_ShouldUpdateWhenWorkersChange()
    {
        // Arrange
        var viewModel = new RosterViewModel();
        var view = new RosterView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Ajouter un worker dynamiquement
        var newWorker = new WorkerListItemViewModel
        {
            Name = "Dynamic Worker",
            Role = "Newcomer",
            Popularity = 50
        };
        viewModel.Workers.Add(newWorker);
        await Task.Delay(50);

        // Assert - Vérifier que l'UI s'est mise à jour
        var dataGrid = view.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();
        dataGrid.Should().NotBeNull();
        ((System.Collections.IEnumerable?)dataGrid.ItemsSource)?.Cast<object>().Should().Contain(newWorker);
    }

    [AvaloniaFact]
    public async Task RosterView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new RosterViewModel();
        var view = new RosterView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainRosterGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier qu'il y a un DataGrid pour les workers
        var dataGrid = view.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();
        dataGrid.Should().NotBeNull();

        // Vérifier qu'il y a un TextBox de recherche
        var searchBox = view.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        searchBox.Should().NotBeNull();
    }
}