using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RingGeneral.UI.ViewModels;

namespace RingGeneral.UI.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ShellViewModel();
    }

    private void OnSimulerShow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.SimulerShow();
        }
    }

    private void OnSemaineSuivante(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.PasserSemaineSuivante();
        }
    }

    private void OnOuvrirAide(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is string pageId)
        {
            shell.Session.OuvrirAide(pageId);
        }
    }

    private void OnFermerAide(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.FermerAide();
        }
    }

    private void OnOuvrirImpact(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is string pageId)
        {
            shell.Session.SelectionnerImpact(pageId);
        }
    }

    private void OnOuvrirArticle(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is string articleId)
        {
            shell.Session.Codex.OuvrirArticle(articleId);
        }
    }

    private void OnToggleDetailsSimulation(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.DetailsSimulationVisible = !shell.Session.DetailsSimulationVisible;
        }
    }

    private void OnCreerShow(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.CreerShow();
        }
    }

    private void OnAjouterSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.AjouterSegment();
        }
    }

    private void OnEnregistrerSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.DataContext is SegmentViewModel segment)
        {
            shell.Session.EnregistrerSegment(segment);
        }
    }

    private void OnCopierSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.DataContext is SegmentViewModel segment)
        {
            shell.Session.CopierSegment(segment);
        }
    }

    private void OnDeplacerSegmentHaut(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.DataContext is SegmentViewModel segment)
        {
            shell.Session.DeplacerSegment(segment, -1);
        }
    }

    private void OnDeplacerSegmentBas(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.DataContext is SegmentViewModel segment)
        {
            shell.Session.DeplacerSegment(segment, 1);
        }
    }

    private void OnAjouterParticipant(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.DataContext is SegmentViewModel segment)
        {
            shell.Session.AjouterParticipant(segment);
        }
    }

    private void OnRetirerParticipant(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell &&
            sender is Control control &&
            control.DataContext is ParticipantViewModel participant &&
            control.Tag is SegmentViewModel segment)
        {
            shell.Session.RetirerParticipant(segment, participant);
        }
    }

    private void OnAjouterParticipantNouveauSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.AjouterParticipantNouveauSegment();
        }
    }

    private void OnRetirerParticipantNouveauSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.DataContext is ParticipantViewModel participant)
        {
            shell.Session.RetirerParticipantNouveauSegment(participant);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
