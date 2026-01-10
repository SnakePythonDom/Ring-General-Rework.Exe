using RingGeneral.UI.Views.Start;
using RingGeneral.UI.ViewModels.Start;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour CompanySelectorView utilisant Avalonia Headless
/// </summary>
public class CompanySelectorViewUITests
{
    [AvaloniaFact]
    public async Task CompanySelectorView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);
        var view = new CompanySelectorView { DataContext = viewModel };

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
    public async Task CompanySelectorView_ShouldDisplaySelectorTitle()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);
        var view = new CompanySelectorView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le titre du sélecteur est affiché
        var titleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("SELECT") || tb.Text.Contains("Sélectionner") || tb.Text.Contains("COMPANY"))
            .ToList();

        titleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CompanySelectorView_ShouldDisplayCompanyList()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);

        // Ajouter une compagnie de test
        viewModel.Companies.Add(new CompanyListItem
        {
            CompanyId = "C001",
            Name = "WWE",
            Prestige = 95,
            Region = "US",
            Treasury = 1000000.0
        });

        var view = new CompanySelectorView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la liste des compagnies est affichée
        var companiesList = view.FindControl<ListBox>("CompaniesListBox") ??
            view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();

        companiesList.Should().NotBeNull();
        companiesList?.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CompanySelectorView_ShouldDisplayCompanyDetails()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);

        var company = new CompanyListItem
        {
            CompanyId = "C002",
            Name = "Test Wrestling Company",
            Prestige = 75,
            Region = "US",
            Treasury = 500000.0
        };
        viewModel.Companies.Add(company);
        viewModel.SelectedCompany = company;

        var view = new CompanySelectorView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les détails de la compagnie sont affichés
        var nameTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("Test Wrestling Company"))
            .ToList();

        var prestigeTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("75"))
            .ToList();

        nameTextBlocks.Should().NotBeEmpty();
        prestigeTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CompanySelectorView_ShouldDisplayCompanyStatistics()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);

        var company = new CompanyListItem
        {
            CompanyId = "C003",
            Name = "Sample Company",
            Prestige = 85,
            Region = "US",
            Treasury = 5000000.0
        };
        viewModel.Companies.Add(company);
        viewModel.SelectedCompany = company;

        var view = new CompanySelectorView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les statistiques sont affichées
        var workersTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("25"))
            .ToList();

        var revenueTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("$5,000,000") || tb.Text.Contains("5000000"))
            .ToList();

        workersTextBlocks.Should().NotBeEmpty();
        revenueTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CompanySelectorView_ShouldHaveSelectButton()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);
        var view = new CompanySelectorView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Select
        var selectButton = view.FindControl<Button>("SelectCompanyButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Select") == true ||
                                      btn.Content?.ToString()?.Contains("Sélectionner") == true);

        selectButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task CompanySelectorView_ShouldHaveCreateNewButton()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);
        var view = new CompanySelectorView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Create New
        var createButton = view.FindControl<Button>("CreateNewCompanyButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Create") == true ||
                                      btn.Content?.ToString()?.Contains("Créer") == true);

        createButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task CompanySelectorView_ShouldHandleCompanySelection()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);

        var company1 = new CompanyListItem { Name = "Company A", Prestige = 70 };
        var company2 = new CompanyListItem { Name = "Company B", Prestige = 80 };

        viewModel.Companies.Add(company1);
        viewModel.Companies.Add(company2);
        viewModel.SelectedCompany = company1;

        var view = new CompanySelectorView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la sélection fonctionne
        var companiesList = view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();
        companiesList.Should().NotBeNull();
        companiesList?.SelectedItem.Should().Be(company1);
    }

    [AvaloniaFact]
    public async Task CompanySelectorView_ShouldDisplayCompanyPreview()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);

        var company = new CompanyListItem
        {
            CompanyId = "C004",
            Name = "Preview Company",
            Prestige = 90,
            Region = "US",
            Treasury = 2000000.0
        };
        viewModel.Companies.Add(company);
        viewModel.SelectedCompany = company;

        var view = new CompanySelectorView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que l'aperçu de la compagnie est affiché
        var descriptionTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("This is a preview"))
            .ToList();

        descriptionTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CompanySelectorView_ShouldUpdateWhenSelectionChanges()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);

        var company1 = new CompanyListItem { CompanyId = "C005", Name = "Company 1", Prestige = 60, Region = "US", Treasury = 100000.0 };
        var company2 = new CompanyListItem { CompanyId = "C006", Name = "Company 2", Prestige = 85, Region = "US", Treasury = 200000.0 };

        viewModel.Companies.Add(company1);
        viewModel.Companies.Add(company2);

        var view = new CompanySelectorView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Changer la sélection
        viewModel.SelectedCompany = company2;
        await Task.Delay(50);

        // Assert - Vérifier que l'affichage s'est mis à jour
        var prestigeTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("85"))
            .ToList();

        prestigeTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CompanySelectorView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new CompanySelectorViewModel(null!, null!, null!);
        var view = new CompanySelectorView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainCompanySelectorGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier qu'il y a des sections pour la liste et les détails
        var borders = view.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Background != null)
            .ToList();

        borders.Should().HaveCountGreaterThan(1);

        // Vérifier la présence de contrôles clés
        var listBoxes = view.GetVisualDescendants().OfType<ListBox>().ToList();
        listBoxes.Should().NotBeEmpty();

        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Should().HaveCountGreaterThan(1);

        var textBlocks = view.GetVisualDescendants().OfType<TextBlock>().ToList();
        textBlocks.Should().NotBeEmpty();
    }
}