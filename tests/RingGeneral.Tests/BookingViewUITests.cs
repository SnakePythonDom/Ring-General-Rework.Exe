using RingGeneral.UI.Views.Booking;
using RingGeneral.UI.ViewModels.Booking;
using RingGeneral.UI.ViewModels;
using RingGeneral.Core.Validation;
using RingGeneral.Core.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using System.Collections.Generic;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour BookingView utilisant Avalonia Headless
/// </summary>
public class BookingViewUITests
{
    [AvaloniaFact]
    public async Task BookingView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        var view = new BookingView { DataContext = viewModel };

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
    public async Task BookingView_ShouldDisplayShowTitle()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le titre du show est affiché
        var titleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("BOOKING") || tb.Text.Contains("Raw"))
            .ToList();

        titleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldDisplayValidationStatus()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence des indicateurs de validation
        var validationBorders = view.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Child is TextBlock tb &&
                       (tb.Text.Contains("✅") || tb.Text.Contains("⚠️")))
            .ToList();

        // Au moins un indicateur de validation devrait être visible
        validationBorders.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldDisplaySegmentCount()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        // Ajouter quelques segments
        var catalog = new SegmentTypeCatalog();
        viewModel.Segments.Add(new SegmentViewModel("SEG1", "match", 8, false, catalog, new List<ParticipantViewModel>(), null, null, 60, null, null, new Dictionary<string, string>()));
        viewModel.Segments.Add(new SegmentViewModel("SEG2", "match", 8, false, catalog, new List<ParticipantViewModel>(), null, null, 60, null, null, new Dictionary<string, string>()));
        viewModel.Segments.Add(new SegmentViewModel("SEG3", "match", 8, false, catalog, new List<ParticipantViewModel>(), null, null, 60, null, null, new Dictionary<string, string>()));

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le nombre de segments est affiché
        var segmentCountTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("3 segments"))
            .ToList();

        segmentCountTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldDisplaySegmentsList()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        // Ajouter un segment de test
        var catalog = new SegmentTypeCatalog();
        var testSegment = new SegmentViewModel("SEG_TEST", "match", 8, false, catalog, new List<ParticipantViewModel>(), null, null, 60, null, null, new Dictionary<string, string>());
        viewModel.Segments.Add(testSegment);

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la liste des segments est affichée
        var segmentsList = view.FindControl<ListBox>("SegmentsListBox") ??
            view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();

        segmentsList.Should().NotBeNull();
        segmentsList?.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldDisplayAddSegmentButton()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Add Segment
        var addButton = view.FindControl<Button>("AddSegmentButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Add") == true ||
                                      btn.Content?.ToString()?.Contains("Ajouter") == true);

        addButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldDisplayAvailableWorkers()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        // Ajouter un worker disponible
        viewModel.WorkersAvailable.Add(new ParticipantViewModel("W001", "John Cena"));

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les workers disponibles sont affichés
        var workersList = view.FindControl<ListBox>("AvailableWorkersListBox") ??
            view.GetVisualDescendants().OfType<ListBox>()
                .Where(lb => lb.Items?.Count > 0)
                .FirstOrDefault();

        workersList.Should().NotBeNull();
        workersList?.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldDisplaySegmentTypes()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les types de segments sont affichés
        var segmentTypesList = view.FindControl<ComboBox>("SegmentTypesComboBox") ??
            view.GetVisualDescendants().OfType<ComboBox>()
                .Where(cb => cb.Items?.Count > 0)
                .FirstOrDefault();

        segmentTypesList.Should().NotBeNull();
        segmentTypesList?.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldDisplayValidationIssues()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        // Ajouter un problème de validation
        viewModel.ValidationIssues.Add(new BookingIssueViewModel(
            Code: "TEST001",
            Message: "Test validation issue",
            Severity: RingGeneral.Core.Models.ValidationSeverity.Avertissement,
            SegmentId: null,
            ActionLabel: "Fix"
        ));

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les problèmes de validation sont affichés
        var issuesList = view.FindControl<ListBox>("ValidationIssuesListBox") ??
            view.GetVisualDescendants().OfType<ListBox>()
                .Where(lb => lb.Items?.Count > 0)
                .FirstOrDefault();

        issuesList.Should().NotBeNull();
        issuesList?.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldDisplayStorylines()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        // Ajouter une storyline disponible
        viewModel.StorylinesAvailable.Add(new StorylineOptionViewModel("ST001", "Test Storyline"));

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les storylines sont affichées
        var storylinesList = view.FindControl<ComboBox>("StorylinesComboBox") ??
            view.GetVisualDescendants().OfType<ComboBox>()
                .Where(cb => cb.Items?.Count > 0)
                .FirstOrDefault();

        storylinesList.Should().NotBeNull();
        storylinesList?.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldDisplayTitles()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        // Ajouter un titre disponible
        viewModel.TitlesAvailable.Add(new TitleOptionViewModel("T001", "World Heavyweight Championship"));

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les titres sont affichés
        var titlesList = view.FindControl<ComboBox>("TitlesComboBox") ??
            view.GetVisualDescendants().OfType<ComboBox>()
                .Where(cb => cb.Items?.Count > 0)
                .FirstOrDefault();

        titlesList.Should().NotBeNull();
        titlesList?.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldUpdateValidationStatus()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Simuler un changement de validation
        // IsBookingValid is readonly - clear validation issues to make it valid
        viewModel.ValidationIssues.Clear();
        await Task.Delay(50);

        // Assert - Vérifier que le statut de validation s'est mis à jour
        var validBorders = view.GetVisualDescendants().OfType<Border>()
            .Where(b => b.IsVisible == true &&
                       b.Child is TextBlock tb &&
                       tb.Text.Contains("✅"))
            .ToList();

        validBorders.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task BookingView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new BookingViewModel(
            repository: null!,
            validator: null!,
            segmentCatalog: null!,
            eventAggregator: null!
        );

        var view = new BookingView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainBookingGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier qu'il y a au moins 3 sections (header, content, footer)
        mainGrid?.RowDefinitions.Should().HaveCountGreaterThanOrEqualTo(3);

        // Vérifier la présence de contrôles clés
        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Should().NotBeEmpty();

        var lists = view.GetVisualDescendants().OfType<ListBox>().ToList();
        lists.Should().NotBeEmpty();
    }
}