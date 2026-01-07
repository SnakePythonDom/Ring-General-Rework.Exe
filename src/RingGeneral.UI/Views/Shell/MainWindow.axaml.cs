using Avalonia.Controls;
using RingGeneral.UI.ViewModels.Core;

namespace RingGeneral.UI.Views.Shell;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor with ViewModel injection (DI)
    /// </summary>
    public MainWindow(ShellViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
