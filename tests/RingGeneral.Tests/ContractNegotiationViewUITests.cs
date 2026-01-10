using RingGeneral.UI.Views.Contracts;
using RingGeneral.UI.ViewModels.Contracts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour ContractNegotiationView utilisant Avalonia Headless
/// </summary>
public class ContractNegotiationViewUITests
{
    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);
        var view = new ContractNegotiationView { DataContext = viewModel };

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
    public async Task ContractNegotiationView_ShouldDisplayNegotiationTitle()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);
        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le titre de négociation est affiché
        var titleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("CONTRACT") || tb.Text.Contains("Contrat"))
            .ToList();

        titleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldDisplayWorkerInfo()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);

        // Simuler un worker en négociation
        viewModel.WorkerName = "John Cena";
        viewModel.WorkerRole = "Main Event Star";
        viewModel.WorkerPopularity = 95;

        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les infos du worker sont affichées
        var nameTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("John Cena"))
            .ToList();

        var roleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("Main Event Star"))
            .ToList();

        nameTextBlocks.Should().NotBeEmpty();
        roleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldDisplayContractTemplates()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);

        // Ajouter des templates
        viewModel.ContractTemplates.Add(new ContractTemplateViewModel
        {
            Name = "Main Event Star",
            Description = "High salary, appearance-based contract"
        });

        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les templates sont affichés
        var templatesList = view.FindControl<ComboBox>("TemplatesComboBox") ??
            view.GetVisualDescendants().OfType<ComboBox>()
                .Where(cb => cb.Items?.Count > 0)
                .FirstOrDefault();

        templatesList.Should().NotBeNull();
        templatesList!.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldDisplaySalaryFields()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);
        viewModel.MonthlySalary = 50000m;
        viewModel.AppearanceFee = 10000m;

        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les champs de salaire sont affichés
        var salaryTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("$50,000") || tb.Text.Contains("50000"))
            .ToList();

        var appearanceTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("$10,000") || tb.Text.Contains("10000"))
            .ToList();

        salaryTextBlocks.Should().NotBeEmpty();
        appearanceTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldDisplayContractDuration()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);
        viewModel.ContractLength = 24; // 24 mois

        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la durée du contrat est affichée
        var durationTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("24") && tb.Text.Contains("month"))
            .ToList();

        durationTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldDisplayNegotiationButtons()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);
        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence des boutons de négociation
        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();

        buttons.Should().NotBeEmpty();

        // Chercher des boutons spécifiques
        var acceptButton = buttons.FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Accept") == true ||
                                                        btn.Content?.ToString()?.Contains("Accepter") == true);
        var counterButton = buttons.FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Counter") == true ||
                                                         btn.Content?.ToString()?.Contains("Contre") == true);

        acceptButton.Should().NotBeNull();
        counterButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldDisplayWorkerDemands()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);
        viewModel.MinimumSalary = 45000m;
        viewModel.WorkerSatisfaction = 75;

        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les exigences du worker sont affichées
        var minSalaryTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("$45,000") || tb.Text.Contains("45000"))
            .ToList();

        var satisfactionTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("75"))
            .ToList();

        minSalaryTextBlocks.Should().NotBeEmpty();
        satisfactionTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldHandleTemplateSelection()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);

        var template = new ContractTemplateViewModel
        {
            Name = "Mid-Card Regular",
            MonthlySalary = 25000m,
            AppearanceFee = 5000m,
            ContractLength = 12
        };
        viewModel.ContractTemplates.Add(template);

        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Trouver et sélectionner le template
        var templateComboBox = view.GetVisualDescendants().OfType<ComboBox>().FirstOrDefault();
        if (templateComboBox != null)
        {
            templateComboBox.SelectedItem = template;
            await Task.Delay(50);
        }

        // Assert - Vérifier que les valeurs se sont mises à jour
        viewModel.MonthlySalary.Should().Be(25000m);
        viewModel.AppearanceFee.Should().Be(5000m);
        viewModel.ContractLength.Should().Be(12);
    }

    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldDisplayNegotiationHistory()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);

        // Ajouter des offres dans l'historique
        viewModel.NegotiationHistory.Add("Initial offer: $40,000/month");
        viewModel.NegotiationHistory.Add("Counter offer: $45,000/month");

        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que l'historique est affiché
        var historyList = view.FindControl<ListBox>("NegotiationHistoryListBox") ??
            view.GetVisualDescendants().OfType<ListBox>()
                .Where(lb => lb.Items?.Count > 0)
                .FirstOrDefault();

        historyList.Should().NotBeNull();
        historyList!.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldUpdateSalaryDisplay()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);
        viewModel.MonthlySalary = 40000m;

        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Changer le salaire
        viewModel.MonthlySalary = 50000m;
        await Task.Delay(50);

        // Assert - Vérifier que l'affichage s'est mis à jour
        var updatedSalaryTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("$50,000") || tb.Text.Contains("50000"))
            .ToList();

        updatedSalaryTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task ContractNegotiationView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new ContractNegotiationViewModel(null, null);
        var view = new ContractNegotiationView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainNegotiationGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier la présence de contrôles clés
        var textBoxes = view.GetVisualDescendants().OfType<TextBox>().ToList();
        textBoxes.Should().NotBeEmpty(); // Champs de saisie pour salaire

        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Should().NotBeEmpty(); // Boutons d'actions

        var comboBoxes = view.GetVisualDescendants().OfType<ComboBox>().ToList();
        comboBoxes.Should().NotBeEmpty(); // Sélection de templates
    }
}