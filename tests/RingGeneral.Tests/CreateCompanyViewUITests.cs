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
/// Tests UI automatisés pour CreateCompanyView utilisant Avalonia Headless
/// </summary>
public class CreateCompanyViewUITests
{
    [AvaloniaFact]
    public async Task CreateCompanyView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        var view = new CreateCompanyView { DataContext = viewModel };

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
    public async Task CreateCompanyView_ShouldDisplayCreationTitle()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        var view = new CreateCompanyView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le titre de création est affiché
        var titleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("CREATE") || tb.Text.Contains("Créer") || tb.Text.Contains("COMPANY"))
            .ToList();

        titleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CreateCompanyView_ShouldDisplayCompanyNameField()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        var view = new CreateCompanyView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du champ nom de compagnie
        var nameTextBox = view.FindControl<TextBox>("CompanyNameTextBox") ??
            view.GetVisualDescendants().OfType<TextBox>()
                .FirstOrDefault(tb => tb.Watermark?.Contains("Name") == true ||
                                     tb.Watermark?.Contains("Nom") == true);

        nameTextBox.Should().NotBeNull();
        nameTextBox!.Watermark.Should().Contain("Name");
    }

    [AvaloniaFact]
    public async Task CreateCompanyView_ShouldDisplayStartingBudget()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        viewModel.StartingBudget = 1000000m;

        var view = new CreateCompanyView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le budget de départ est affiché
        var budgetTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("$1,000,000") || tb.Text.Contains("1000000"))
            .ToList();

        budgetTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CreateCompanyView_ShouldDisplayCompanyTypeSelection()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        var view = new CreateCompanyView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence de la sélection de type de compagnie
        var typeComboBox = view.FindControl<ComboBox>("CompanyTypeComboBox") ??
            view.GetVisualDescendants().OfType<ComboBox>()
                .Where(cb => cb.Items?.Count > 0)
                .FirstOrDefault();

        typeComboBox.Should().NotBeNull();
        typeComboBox!.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CreateCompanyView_ShouldDisplayDifficultySelection()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        viewModel.SelectedDifficulty = "Normal";

        var view = new CreateCompanyView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence de la sélection de difficulté
        var difficultyComboBox = view.FindControl<ComboBox>("DifficultyComboBox") ??
            view.GetVisualDescendants().OfType<ComboBox>()
                .Where(cb => cb.Items?.Count > 0)
                .FirstOrDefault();

        difficultyComboBox.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task CreateCompanyView_ShouldHaveCreateButton()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        var view = new CreateCompanyView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Create
        var createButton = view.FindControl<Button>("CreateCompanyButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Create") == true ||
                                      btn.Content?.ToString()?.Contains("Créer") == true);

        createButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task CreateCompanyView_ShouldHaveBackButton()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        var view = new CreateCompanyView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Back
        var backButton = view.FindControl<Button>("BackButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Back") == true ||
                                      btn.Content?.ToString()?.Contains("Retour") == true);

        backButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task CreateCompanyView_ShouldDisplayCompanyPreview()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        viewModel.CompanyName = "Test Wrestling";
        viewModel.StartingBudget = 500000m;

        var view = new CreateCompanyView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que l'aperçu de la compagnie est affiché
        var previewTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("Test Wrestling") || tb.Text.Contains("$500,000"))
            .ToList();

        previewTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CreateCompanyView_ShouldHandleNameInput()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        var view = new CreateCompanyView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Trouver le TextBox du nom et saisir du texte
        var nameTextBox = view.GetVisualDescendants().OfType<TextBox>()
            .FirstOrDefault(tb => tb.Watermark?.Contains("Name") == true);

        if (nameTextBox != null)
        {
            nameTextBox.Text = "My Wrestling Company";
            await Task.Delay(50);
        }

        // Assert - Vérifier que le binding fonctionne
        viewModel.CompanyName.Should().Be("My Wrestling Company");
    }

    [AvaloniaFact]
    public async Task CreateCompanyView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new CreateCompanyViewModel(null, null);
        var view = new CreateCompanyView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainCreateCompanyGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier la présence de contrôles d'entrée
        var textBoxes = view.GetVisualDescendants().OfType<TextBox>().ToList();
        textBoxes.Should().NotBeEmpty();

        var comboBoxes = view.GetVisualDescendants().OfType<ComboBox>().ToList();
        comboBoxes.Should().NotBeEmpty();

        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Should().HaveCountGreaterThan(1);

        // Vérifier qu'il y a des sections organisées
        var borders = view.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Background != null)
            .ToList();

        borders.Should().HaveCountGreaterThan(1);
    }
}