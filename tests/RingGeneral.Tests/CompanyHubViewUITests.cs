using RingGeneral.UI.Views.CompanyHub;
using RingGeneral.UI.ViewModels.CompanyHub;
using RingGeneral.Core.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour CompanyHubView utilisant Avalonia Headless
/// </summary>
public class CompanyHubViewUITests
{
    [AvaloniaFact]
    public async Task CompanyHubView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new CompanyHubViewModel(null, null, null);
        var view = new CompanyHubView { DataContext = viewModel };

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
    public async Task CompanyHubView_ShouldDisplayCompanyTreasury()
    {
        // Arrange
        var viewModel = new CompanyHubViewModel(null, null, null);
        // Simuler une compagnie avec une trésorerie
        var mockCompany = new CompanyState(
            CompagnieId: "C001",
            Nom: "Test Company",
            Region: "US",
            Prestige: 50,
            Tresorerie: 2500000.0,
            AudienceMoyenne: 50,
            Reach: 50
        );
        viewModel.CurrentCompany = mockCompany;

        var view = new CompanyHubView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la trésorerie est affichée
        var treasuryTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("$2,500,000"))
            .ToList();

        treasuryTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CompanyHubView_ShouldDisplayCurrentDate()
    {
        // Arrange
        var viewModel = new CompanyHubViewModel(null, null, null);
        viewModel.CurrentDateLabel = "Week 15";

        var view = new CompanyHubView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la date est affichée
        var dateTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("Week 15"))
            .ToList();

        dateTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CompanyHubView_ShouldDisplayContinueButton()
    {
        // Arrange
        var viewModel = new CompanyHubViewModel(null, null, null);
        var view = new CompanyHubView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton CONTINUER
        var continueButton = view.FindControl<Button>("ContinueButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString() == "CONTINUER");

        continueButton.Should().NotBeNull();
        if (continueButton != null)
        {
            // Button found
        }
    }

    [AvaloniaFact]
    public async Task CompanyHubView_ShouldDisplayMainActionButtons()
    {
        // Arrange
        var viewModel = new CompanyHubViewModel(null, null, null);
        var view = new CompanyHubView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence des boutons principaux
        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();

        // Devrait avoir au moins les boutons principaux (Finance, Roster, Booking, etc.)
        buttons.Should().HaveCountGreaterThan(3);

        // Vérifier quelques boutons spécifiques
        var financeButton = buttons.FirstOrDefault(btn => btn.Content?.ToString()?.Contains("FINANCE") == true);
        var rosterButton = buttons.FirstOrDefault(btn => btn.Content?.ToString()?.Contains("ROSTER") == true);
        var bookingButton = buttons.FirstOrDefault(btn => btn.Content?.ToString()?.Contains("BOOKING") == true);

        financeButton.Should().NotBeNull();
        rosterButton.Should().NotBeNull();
        bookingButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task CompanyHubView_ShouldDisplayCompanyStats()
    {
        // Arrange
        var viewModel = new CompanyHubViewModel(null, null, null);
        var view = new CompanyHubView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier l'affichage des statistiques de compagnie
        var statTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("RING GENERAL") ||
                        tb.Text.Contains("Trésorerie") ||
                        tb.Text.Contains("Date"))
            .ToList();

        statTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CompanyHubView_ShouldHaveCreateSubsidiaryButton()
    {
        // Arrange
        var viewModel = new CompanyHubViewModel(null, null, null);
        var view = new CompanyHubView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Create Subsidiary
        var subsidiaryButton = view.FindControl<Button>("CreateSubsidiaryButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Subsidiary") == true ||
                                      btn.Content?.ToString()?.Contains("Filiale") == true);

        subsidiaryButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task CompanyHubView_ShouldDisplayNavigationSections()
    {
        // Arrange
        var viewModel = new CompanyHubViewModel(null, null, null);
        var view = new CompanyHubView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence des sections de navigation
        var borders = view.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Background != null)
            .ToList();

        borders.Should().HaveCountGreaterThan(2); // Au moins header, navigation, content
    }

    [AvaloniaFact]
    public async Task CompanyHubView_ShouldUpdateTreasuryDisplay()
    {
        // Arrange
        var viewModel = new CompanyHubViewModel(null, null, null);
        var mockCompany = new CompanyState(
            CompagnieId: "C001",
            Nom: "Test Company",
            Region: "US",
            Prestige: 50,
            Tresorerie: 1000000.0,
            AudienceMoyenne: 50,
            Reach: 50
        );
        viewModel.CurrentCompany = mockCompany;

        var view = new CompanyHubView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Changer la trésorerie
        mockCompany = new CompanyState(
            CompagnieId: "C001",
            Nom: "Test Company",
            Region: "US",
            Prestige: 50,
            Tresorerie: 2000000.0,
            AudienceMoyenne: 50,
            Reach: 50
        );
        viewModel.CurrentCompany = mockCompany;
        await Task.Delay(50);

        // Assert - Vérifier que l'affichage s'est mis à jour
        var updatedTreasuryTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("$2,000,000"))
            .ToList();

        updatedTreasuryTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CompanyHubView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new CompanyHubViewModel(null, null, null);
        var view = new CompanyHubView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainHubGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier qu'il y a au moins 3 lignes (header, subheader, content)
        mainGrid?.RowDefinitions.Should().HaveCountGreaterThanOrEqualTo(3);

        // Vérifier la présence d'éléments visuels clés
        var textBlocks = view.GetVisualDescendants().OfType<TextBlock>().ToList();
        textBlocks.Should().NotBeEmpty();

        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Should().HaveCountGreaterThan(5); // Plusieurs boutons d'actions
    }
}