using RingGeneral.UI.Views.Finance;
using RingGeneral.UI.ViewModels.Finance;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour FinanceView utilisant Avalonia Headless
/// </summary>
public class FinanceViewUITests
{
    [AvaloniaFact]
    public async Task FinanceView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new FinanceViewModel
        {
            CurrentBalance = 2500000.50m,
            WeeklyRevenue = 150000.25m,
            WeeklyExpenses = 120000.75m,
            CurrentWeek = 15,
            TotalDebt = 500000m
        };

        var view = new FinanceView { DataContext = viewModel };

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
    public async Task FinanceView_ShouldDisplayCurrentBalance()
    {
        // Arrange
        var expectedBalance = 1750000.75m;
        var viewModel = new FinanceViewModel
        {
            CurrentBalance = expectedBalance
        };

        var view = new FinanceView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche le solde actuel
        var balanceTextBlock = view.FindControl<TextBlock>("CurrentBalanceTextBlock") ??
            view.GetVisualDescendants().OfType<TextBlock>()
                .FirstOrDefault(tb => tb.Text.Contains("$1,750,000.75"));

        balanceTextBlock.Should().NotBeNull();
        balanceTextBlock?.Text.Should().Contain("$1,750,000.75");
    }

    [AvaloniaFact]
    public async Task FinanceView_ShouldDisplayWeeklyRevenue()
    {
        // Arrange
        var expectedRevenue = 98765.43m;
        var viewModel = new FinanceViewModel
        {
            WeeklyRevenue = expectedRevenue
        };

        var view = new FinanceView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche les revenus hebdomadaires
        var revenueTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("$98,765.43"))
            .ToList();

        revenueTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task FinanceView_ShouldDisplayWeeklyExpenses()
    {
        // Arrange
        var expectedExpenses = 54321.67m;
        var viewModel = new FinanceViewModel
        {
            WeeklyExpenses = expectedExpenses
        };

        var view = new FinanceView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche les dépenses hebdomadaires
        var expenseTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("$54,321.67"))
            .ToList();

        expenseTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task FinanceView_ShouldDisplayCurrentWeek()
    {
        // Arrange
        var expectedWeek = 42;
        var viewModel = new FinanceViewModel
        {
            CurrentWeek = expectedWeek
        };

        var view = new FinanceView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche la semaine actuelle
        var weekTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains($"Week {expectedWeek}"))
            .ToList();

        weekTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task FinanceView_ShouldDisplayTotalDebt()
    {
        // Arrange
        var expectedDebt = 750000m;
        var viewModel = new FinanceViewModel
        {
            TotalDebt = expectedDebt
        };

        var view = new FinanceView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Trouver le TextBlock qui affiche la dette totale
        var debtTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("$750,000"))
            .ToList();

        debtTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task FinanceView_ShouldDisplayTransactionsList()
    {
        // Arrange
        var viewModel = new FinanceViewModel();
        viewModel.Transactions.Add(new TransactionItemViewModel
        {
            Category = "Revenue",
            Description = "Test Transaction",
            Amount = 10000.50m,
            Week = 1
        });

        var view = new FinanceView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la liste de transactions est affichée
        var dataGrid = view.FindControl<DataGrid>("TransactionsDataGrid") ??
            view.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();

        dataGrid.Should().NotBeNull();
        dataGrid?.ItemsSource.Should().NotBeNull();
        ((System.Collections.IEnumerable?)dataGrid.ItemsSource)?.Cast<object>().Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task FinanceView_ShouldHaveBrowseNetworksButton()
    {
        // Arrange
        var viewModel = new FinanceViewModel();
        var view = new FinanceView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Browse Networks
        var browseButton = view.FindControl<Button>("BrowseNetworksButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Browse Networks") == true);

        browseButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task FinanceView_ShouldDisplayTvDeals()
    {
        // Arrange
        var viewModel = new FinanceViewModel();
        viewModel.TvDeals.Add(new TvDealViewModel
        {
            Network = "Test Network",
            WeeklyPayment = 10000m,
            MinShows = 48,
            MaxShows = 52
        });

        var view = new FinanceView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les TV deals sont affichés
        var tvDealsList = view.FindControl<ListBox>("TvDealsListBox") ??
            view.GetVisualDescendants().OfType<ListBox>()
                .FirstOrDefault(lb => lb.Items?.Count > 0);

        tvDealsList.Should().NotBeNull();
        tvDealsList?.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task FinanceView_ShouldUpdateWhenViewModelChanges()
    {
        // Arrange
        var viewModel = new FinanceViewModel
        {
            CurrentBalance = 1000000m
        };

        var view = new FinanceView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Changer la valeur dans le ViewModel
        viewModel.CurrentBalance = 2000000m;
        await Task.Delay(50); // Attendre la mise à jour de l'UI

        // Assert - Vérifier que l'UI s'est mise à jour
        var balanceTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("$2,000,000"))
            .ToList();

        balanceTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task FinanceView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new FinanceViewModel();
        var view = new FinanceView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainFinanceGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier qu'il y a des statistiques financières
        var statBorders = view.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Background != null)
            .ToList();

        statBorders.Should().HaveCountGreaterThan(3); // Au moins 4 cartes de stats
    }
}