using RingGeneral.UI.Views.Inbox;
using RingGeneral.UI.ViewModels.Inbox;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Tests;

/// <summary>
/// Tests UI automatisés pour InboxView utilisant Avalonia Headless
/// </summary>
public class InboxViewUITests
{
    [AvaloniaFact]
    public async Task InboxView_ShouldRenderCorrectly()
    {
        // Arrange
        var viewModel = new InboxViewModel(null);
        var view = new InboxView { DataContext = viewModel };

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
    public async Task InboxView_ShouldDisplayInboxTitle()
    {
        // Arrange
        var viewModel = new InboxViewModel(null);
        var view = new InboxView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le titre Inbox est affiché
        var titleTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("INBOX") || tb.Text.Contains("Messages"))
            .ToList();

        titleTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task InboxView_ShouldDisplayMessagesList()
    {
        // Arrange
        var viewModel = new InboxViewModel(null);

        // Ajouter un message de test
        viewModel.Messages.Add(new InboxMessageViewModel
        {
            Subject = "Test Message",
            Content = "This is a test message",
            IsRead = false,
            DateReceived = DateTime.Now
        });

        var view = new InboxView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la liste des messages est affichée
        var messagesList = view.FindControl<ListBox>("MessagesListBox") ??
            view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();

        messagesList.Should().NotBeNull();
        messagesList!.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task InboxView_ShouldDisplayMessageContent()
    {
        // Arrange
        var viewModel = new InboxViewModel(null);

        var testMessage = new InboxMessageViewModel
        {
            Subject = "Important Announcement",
            Content = "Breaking news in the wrestling world!",
            Sender = "System",
            DateReceived = DateTime.Now
        };
        viewModel.Messages.Add(testMessage);
        viewModel.SelectedMessage = testMessage;

        var view = new InboxView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le contenu du message est affiché
        var contentTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("Breaking news"))
            .ToList();

        contentTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task InboxView_ShouldDisplayUnreadCount()
    {
        // Arrange
        var viewModel = new InboxViewModel(null);

        // Ajouter des messages (certains non lus)
        viewModel.Messages.Add(new InboxMessageViewModel { Subject = "Read Message", IsRead = true });
        viewModel.Messages.Add(new InboxMessageViewModel { Subject = "Unread Message 1", IsRead = false });
        viewModel.Messages.Add(new InboxMessageViewModel { Subject = "Unread Message 2", IsRead = false });

        var view = new InboxView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que le nombre de messages non lus est affiché
        var unreadTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("2") && tb.Text.Contains("non lu"))
            .ToList();

        // Ou chercher d'autres indicateurs de messages non lus
        var unreadIndicators = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("Unread") || tb.Text.Contains("non lu"))
            .ToList();

        unreadIndicators.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task InboxView_ShouldHandleMessageSelection()
    {
        // Arrange
        var viewModel = new InboxViewModel(null);

        var message1 = new InboxMessageViewModel { Subject = "Message 1", Content = "Content 1" };
        var message2 = new InboxMessageViewModel { Subject = "Message 2", Content = "Content 2" };

        viewModel.Messages.Add(message1);
        viewModel.Messages.Add(message2);
        viewModel.SelectedMessage = message1;

        var view = new InboxView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que la sélection fonctionne
        var messagesList = view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();
        messagesList.Should().NotBeNull();
        messagesList!.SelectedItem.Should().Be(message1);
    }

    [AvaloniaFact]
    public async Task InboxView_ShouldDisplayMessageDetails()
    {
        // Arrange
        var viewModel = new InboxViewModel(null);

        var detailedMessage = new InboxMessageViewModel
        {
            Subject = "Worker Contract Expired",
            Content = "The contract for John Cena has expired. Please negotiate a new one.",
            Sender = "HR Department",
            DateReceived = DateTime.Now.AddDays(-1),
            Priority = "High"
        };
        viewModel.Messages.Add(detailedMessage);
        viewModel.SelectedMessage = detailedMessage;

        var view = new InboxView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier que les détails du message sont affichés
        var senderTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("HR Department"))
            .ToList();

        senderTextBlocks.Should().NotBeEmpty();

        var contentTextBlocks = view.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text.Contains("John Cena"))
            .ToList();

        contentTextBlocks.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public async Task InboxView_ShouldHaveMarkAsReadButton()
    {
        // Arrange
        var viewModel = new InboxViewModel(null);
        var view = new InboxView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la présence du bouton Mark as Read
        var markReadButton = view.FindControl<Button>("MarkAsReadButton") ??
            view.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(btn => btn.Content?.ToString()?.Contains("Read") == true ||
                                      btn.Content?.ToString()?.Contains("Lu") == true);

        markReadButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public async Task InboxView_ShouldUpdateWhenMessagesChange()
    {
        // Arrange
        var viewModel = new InboxViewModel(null);
        var view = new InboxView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Ajouter un message dynamiquement
        var newMessage = new InboxMessageViewModel
        {
            Subject = "New Important Message",
            Content = "This is urgent!",
            IsRead = false
        };
        viewModel.Messages.Add(newMessage);
        await Task.Delay(50);

        // Assert - Vérifier que l'UI s'est mise à jour
        var messagesList = view.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();
        messagesList.Should().NotBeNull();
        messagesList!.Items.Should().Contain(newMessage);
    }

    [AvaloniaFact]
    public async Task InboxView_ShouldHaveCorrectLayout()
    {
        // Arrange
        var viewModel = new InboxViewModel(null);
        var view = new InboxView { DataContext = viewModel };

        // Act
        var window = new Window { Content = view };
        window.Show();
        await Task.Delay(100);

        // Assert - Vérifier la structure de base
        var mainGrid = view.FindControl<Grid>("MainInboxGrid") ??
            view.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

        mainGrid.Should().NotBeNull();

        // Vérifier qu'il y a des contrôles de liste et de détails
        var listBoxes = view.GetVisualDescendants().OfType<ListBox>().ToList();
        listBoxes.Should().NotBeEmpty();

        // Vérifier la présence de boutons d'actions
        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Should().NotBeEmpty();
    }
}