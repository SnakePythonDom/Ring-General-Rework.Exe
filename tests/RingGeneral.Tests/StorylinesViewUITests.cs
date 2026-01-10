using RingGeneral.UI.Views.Storylines;
using RingGeneral.UI.ViewModels.Storylines;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour StorylinesView utilisant Avalonia Headless
/// </summary>
public class StorylinesViewUITests
{
    [AvaloniaFact]
    public async Task StorylinesView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);
        var view = new StorylinesView { DataContext = viewModel };

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
    public async Task StorylinesView_ShouldDisplayStorylinesTitle()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);
        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le titre Storylines est affiché
        var titleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("STORYLINES") || tb.Text.Contains("Intrigues"))
            .ToList();

        titleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task StorylinesView_ShouldDisplayActiveStorylines()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);

        // Ajouter une storyline active
        viewModel.ActiveStorylines.Add(new StorylineViewModel
        {
            Title = "Championship Rivalry",
            Description = "John Cena vs The Rock for the WWE Championship",
            Participants = "John Cena, The Rock",
            Heat = 85,
            WeeksActive = 3
        });

        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les storylines actives sont affichées
        var storylinesList = view.FindControl<ListBox>("ActiveStorylinesListBox") ??
            view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();

        storylinesList.Should().NotBeNull();
        storylinesList!.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task StorylinesView_ShouldDisplayStorylineDetails()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);

        var storyline = new StorylineViewModel
        {
            Title = "Betrayal Angle",
            Description = "A shocking betrayal that will change everything",
            Participants = "Triple H, Shawn Michaels, Kevin Nash",
            Heat = 92,
            WeeksActive = 5,
            Status = "Building"
        };
        viewModel.ActiveStorylines.Add(storyline);
        viewModel.SelectedStoryline = storyline;

        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les détails de la storyline sont affichés
        var titleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("Betrayal Angle"))
            .ToList();

        var descriptionTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("shocking betrayal"))
            .ToList();

        var heatTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("92"))
            .ToList();

        titleTextBlocks.Should().NotBeEmpty();
        descriptionTextBlocks.Should().NotBeEmpty();
        heatTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task StorylinesView_ShouldDisplayStorylineStatistics()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);
        viewModel.TotalActiveStorylines = 8;
        viewModel.AverageHeat = 76;
        viewModel.CompletedThisMonth = 3;

        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les statistiques sont affichées
        var totalTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("8"))
            .ToList();

        var averageTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("76"))
            .ToList();

        var completedTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("3"))
            .ToList();

        totalTextBlocks.Should().NotBeEmpty();
        averageTextBlocks.Should().NotBeEmpty();
        completedTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task StorylinesView_ShouldHaveCreateStorylineButton()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);
        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Create Storyline
        var createButton = view.FindControl<Button>("CreateStorylineButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Create") == true ||
                                      btn.Content?.ToString()?.Contains("Créer") == true);

        createButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task StorylinesView_ShouldHaveAdvanceStorylineButton()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);
        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Advance Storyline
        var advanceButton = view.FindControl<Button>("AdvanceStorylineButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Advance") == true ||
                                      btn.Content?.ToString()?.Contains("Avancer") == true);

        advanceButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task StorylinesView_ShouldDisplayStorylineHeat()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);

        var highHeatStoryline = new StorylineViewModel
        {
            Title = "Main Event Feud",
            Heat = 95,
            Status = "Red Hot"
        };
        viewModel.ActiveStorylines.Add(highHeatStoryline);

        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la chaleur de la storyline est affichée
        var heatTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("95") || tb.Text.Contains("Red Hot"))
            .ToList();

        heatTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task StorylinesView_ShouldDisplayParticipants()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);

        var storyline = new StorylineViewModel
        {
            Title = "Tag Team Saga",
            Participants = "The Hardy Boyz, Edge & Christian, Dudley Boyz",
            InvolvedWorkers = 6
        };
        viewModel.ActiveStorylines.Add(storyline);
        viewModel.SelectedStoryline = storyline;

        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les participants sont affichés
        var participantsTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("Hardy Boyz") || tb.Text.Contains("Edge"))
            .ToList();

        var countTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("6"))
            .ToList();

        participantsTextBlocks.Should().NotBeEmpty();
        countTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task StorylinesView_ShouldHandleStorylineSelection()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);

        var storyline1 = new StorylineViewModel { Title = "Storyline A", Heat = 70 };
        var storyline2 = new StorylineViewModel { Title = "Storyline B", Heat = 80 };

        viewModel.ActiveStorylines.Add(storyline1);
        viewModel.ActiveStorylines.Add(storyline2);
        viewModel.SelectedStoryline = storyline1;

        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la sélection fonctionne
        var listBox = view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();
        listBox.Should().NotBeNull();
        listBox!.SelectedItem.Should().Be(storyline1);
    }

    [AvaloniaFact]
    public async Task StorylinesView_ShouldUpdateWhenStorylinesChange()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);
        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Ajouter une storyline dynamiquement
        var newStoryline = new StorylineViewModel
        {
            Title = "New Dramatic Storyline",
            Heat = 60,
            Status = "Starting"
        };
        viewModel.ActiveStorylines.Add(newStoryline);
        await Task.Delay(50);

        // Assert - Vérifier que l'UI s'est mise à jour
        var listBox = view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();
        listBox.Should().NotBeNull();
        listBox!.Items.Should().Contain(newStoryline);
    }

    [AvaloniaFact]
    public async Task StorylinesView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new StorylinesViewModel(null);
        var view = new StorylinesView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainStorylinesGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier qu'il y a des sections pour les stats et la liste
        var borders = view.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Background != null)
            .ToList();

        borders.Should().HaveCountGreaterThan(2);

        // Vérifier la présence d'un ListBox pour les storylines
        var listBox = view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();
        listBox.Should().NotBeNull();

        // Vérifier la présence de boutons d'actions
        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Should().HaveCountGreaterThan(2);
    }
}