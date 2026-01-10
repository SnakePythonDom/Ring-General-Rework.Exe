using RingGeneral.UI.Views.Youth;
using RingGeneral.UI.ViewModels.Youth;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour YouthView utilisant Avalonia Headless
/// </summary>
public class YouthViewUITests
{
    [AvaloniaFact]
    public async Task YouthView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new YouthViewModel();
        var view = new YouthView { DataContext = viewModel };

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
    public async Task YouthView_ShouldDisplayYouthTitle()
    {
        // Arrange
        var viewModel = new YouthViewModel();
        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le titre Youth est affiché
        var titleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && (tb.Text.Contains("YOUTH") || tb.Text.Contains("Jeunesse")))
            .ToList();

        titleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task YouthView_ShouldDisplayTraineesList()
    {
        // Arrange
        var viewModel = new YouthViewModel();

        // Ajouter un trainee de test
        viewModel.Trainees.Add(new TraineeItemViewModel
        {
            Name = "Young Wrestler",
            Age = 18,
            InRing = 25,
            Entertainment = 30,
            Progress = 40
        });

        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la liste des trainees est affichée
        var traineesList = view.FindControl<DataGrid>("TraineesDataGrid") ??
            view.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();

        traineesList.Should().NotBeNull();
        if (traineesList != null)
        {
            traineesList.ItemsSource.Should().NotBeNull();
            ((System.Collections.IEnumerable?)traineesList.ItemsSource)?.Cast<object>().Should().NotBeEmpty();
        }
    }

    [AvaloniaFact]
    public async Task YouthView_ShouldDisplayTraineeDetails()
    {
        // Arrange
        var viewModel = new YouthViewModel();

        var trainee = new TraineeItemViewModel
        {
            Name = "Alex Johnson",
            Age = 19,
            InRing = 35,
            Entertainment = 28,
            Story = 22,
            Potential = 85,
            Progress = 50
        };
        viewModel.Trainees.Add(trainee);
        // SelectedTrainee property doesn't exist, skipping assignment

        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les détails du trainee sont affichés
        var nameTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("Alex Johnson"))
            .ToList();

        var skillTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && (tb.Text.Contains("35") || tb.Text.Contains("28") || tb.Text.Contains("22")))
            .ToList();

        nameTextBlocks.Should().NotBeEmpty();
        skillTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task YouthView_ShouldDisplayYouthStatistics()
    {
        // Arrange
        var viewModel = new YouthViewModel();
        // TotalTrainees, ActiveTrainees, GraduatedThisMonth are readonly - add trainees to update
        for (int i = 0; i < 12; i++)
        {
            viewModel.Trainees.Add(new TraineeItemViewModel { WorkerId = $"T{i}", Name = $"Trainee {i}", Age = 18 + i % 5 });
        }

        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les statistiques sont affichées
        var totalTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("12"))
            .ToList();

        var activeTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("8"))
            .ToList();

        var graduatedTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("2"))
            .ToList();

        totalTextBlocks.Should().NotBeEmpty();
        activeTextBlocks.Should().NotBeEmpty();
        graduatedTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task YouthView_ShouldDisplayTrainingFacilities()
    {
        // Arrange
        var viewModel = new YouthViewModel();
        // FacilityLevel, TrainingCapacity, and CoachingQuality properties don't exist
        // These would be part of YouthStructureViewModel if needed

        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les infos des facilities sont affichées
        var levelTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("3"))
            .ToList();

        var capacityTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("15"))
            .ToList();

        levelTextBlocks.Should().NotBeEmpty();
        capacityTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task YouthView_ShouldHaveRecruitButton()
    {
        // Arrange
        var viewModel = new YouthViewModel();
        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Recruit
        var recruitButton = view.FindControl<Button>("RecruitTraineeButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Recruit") == true ||
                                      btn.Content?.ToString()?.Contains("Recruter") == true);

        recruitButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task YouthView_ShouldHaveUpgradeFacilityButton()
    {
        // Arrange
        var viewModel = new YouthViewModel();
        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Upgrade Facility
        var upgradeButton = view.FindControl<Button>("UpgradeFacilityButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Upgrade") == true ||
                                      btn.Content?.ToString()?.Contains("Améliorer") == true);

        upgradeButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task YouthView_ShouldDisplayProgressionCharts()
    {
        // Arrange
        var viewModel = new YouthViewModel();

        var trainee = new TraineeItemViewModel
        {
            Name = "Test Trainee",
            InRing = 40,
            Entertainment = 35,
            Story = 30,
            Progress = 60
        };
        viewModel.Trainees.Add(trainee);

        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les graphiques de progression sont présents
        // (Les graphiques peuvent être représentés par des ProgressBar ou des éléments visuels)
        var progressBars = view.GetVisualDescendants().OfType<ProgressBar>().ToList();
        var borders = view.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Background != null)
            .ToList();

        // Au moins quelques éléments visuels pour représenter la progression
        (progressBars.Count + borders.Count).Should().BeGreaterThan(2);
    }

    [AvaloniaFact]
    public async Task YouthView_ShouldHandleTraineeSelection()
    {
        // Arrange
        var viewModel = new YouthViewModel();

        var trainee1 = new TraineeItemViewModel { Name = "Trainee 1", Age = 18 };
        var trainee2 = new TraineeItemViewModel { Name = "Trainee 2", Age = 19 };

        viewModel.Trainees.Add(trainee1);
        viewModel.Trainees.Add(trainee2);
        // SelectedTrainee property doesn't exist in YouthViewModel

        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la sélection fonctionne
        var dataGrid = view.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();
        dataGrid.Should().NotBeNull();
        dataGrid?.SelectedItem.Should().Be(trainee1);
    }

    [AvaloniaFact]
    public async Task YouthView_ShouldUpdateWhenTraineesChange()
    {
        // Arrange
        var viewModel = new YouthViewModel();
        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Ajouter un trainee dynamiquement
        var newTrainee = new TraineeItemViewModel
        {
            Name = "New Trainee",
            Age = 17,
            InRing = 20
        };
        viewModel.Trainees.Add(newTrainee);
        await Task.Delay(50);

        // Assert - Vérifier que l'UI s'est mise à jour
        var dataGrid = view.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();
        dataGrid.Should().NotBeNull();
        ((System.Collections.IEnumerable?)dataGrid.ItemsSource)?.Cast<object>().Should().Contain(newTrainee);
    }

    [AvaloniaFact]
    public async Task YouthView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new YouthViewModel();
        var view = new YouthView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainYouthGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier qu'il y a plusieurs sections (stats, liste, détails)
        var borders = view.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Background != null)
            .ToList();

        borders.Should().HaveCountGreaterThan(2);

        // Vérifier la présence d'un DataGrid pour les trainees
        var dataGrid = view.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();
        dataGrid.Should().NotBeNull();

        // Vérifier la présence de boutons d'actions
        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Should().HaveCountGreaterThan(2);
    }
}