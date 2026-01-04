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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
