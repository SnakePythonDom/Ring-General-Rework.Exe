using RingGeneral.UI.Views.Calendar;
using RingGeneral.UI.ViewModels.Calendar;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour CalendarView utilisant Avalonia Headless
/// </summary>
public class CalendarViewUITests
{
    [AvaloniaFact]
    public async Task CalendarView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new CalendarViewModel();
        var view = new CalendarView { DataContext = viewModel };

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
    public async Task CalendarView_ShouldDisplayCalendarTitle()
    {
        // Arrange
        var viewModel = new CalendarViewModel();
        var view = new CalendarView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le titre Calendar est affiché
        var titleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && (tb.Text.Contains("CALENDAR") || tb.Text.Contains("Calendrier")))
            .ToList();

        titleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CalendarView_ShouldDisplayCurrentMonth()
    {
        // Arrange
        var viewModel = new CalendarViewModel();
        // CalendarViewModel utilise CurrentDate (DateOnly) au lieu de CurrentWeek

        var view = new CalendarView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le mois actuel est affiché
        var monthTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("January 2024"))
            .ToList();

        monthTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CalendarView_ShouldDisplayEvents()
    {
        // Arrange
        var viewModel = new CalendarViewModel();

        // Ajouter un événement
        viewModel.CalendarEntries.Add(new CalendarEntryItemViewModel
        {
            EntryId = "CAL_WM",
            Date = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"),
            EntryType = "PPV",
            Title = "WrestleMania",
            Notes = "The Grandest Stage of Them All"
        });

        var view = new CalendarView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les événements sont affichés
        var eventsList = view.FindControl<ListBox>("CalendarEntriesListBox") ??
            view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();

        eventsList.Should().NotBeNull();
        if (eventsList != null)
        {
            eventsList.Items.Should().NotBeEmpty();
        }
    }

    [AvaloniaFact]
    public async Task CalendarView_ShouldDisplayNavigationButtons()
    {
        // Arrange
        var viewModel = new CalendarViewModel();
        var view = new CalendarView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence des boutons de navigation
        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();

        // Devrait avoir des boutons Previous/Next ou similaires
        var navButtons = buttons.Where(btn =>
            btn.Content?.ToString()?.Contains("Previous") == true ||
            btn.Content?.ToString()?.Contains("Next") == true ||
            btn.Content?.ToString()?.Contains("Précédent") == true ||
            btn.Content?.ToString()?.Contains("Suivant") == true).ToList();

        navButtons.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CalendarView_ShouldDisplayEventDetails()
    {
        // Arrange
        var viewModel = new CalendarViewModel();

        var eventItem = new CalendarEntryItemViewModel
        {
            EntryId = "CAL_RR",
            Date = DateTime.Now.AddDays(15).ToString("yyyy-MM-dd"),
            EntryType = "PPV",
            Title = "Royal Rumble",
            Notes = "30-man Royal Rumble match - Madison Square Garden"
        };
        viewModel.CalendarEntries.Add(eventItem);
        viewModel.SelectedEntry = eventItem;

        var view = new CalendarView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les détails de l'événement sont affichés
        var titleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("Royal Rumble"))
            .ToList();

        var venueTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text != null && tb.Text.Contains("Madison Square Garden"))
            .ToList();

        titleTextBlocks.Should().NotBeEmpty();
        venueTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task CalendarView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new CalendarViewModel();
        var view = new CalendarView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainCalendarGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier qu'il y a des sections pour le calendrier et les événements
        var borders = view.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Background != null)
            .ToList();

        borders.Should().HaveCountGreaterThan(1);

        // Vérifier la présence de contrôles de calendrier
        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Should().NotBeEmpty();

        var listBoxes = view.GetVisualDescendants().OfType<ListBox>().ToList();
        listBoxes.Should().NotBeEmpty();
    }
}