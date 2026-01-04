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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
