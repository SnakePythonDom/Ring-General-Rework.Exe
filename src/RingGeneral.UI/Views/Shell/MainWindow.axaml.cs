using Avalonia.Controls;
using RingGeneral.UI.ViewModels;

namespace RingGeneral.UI.Views.Shell;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor with ViewModel injection (DI)
    /// Accepte n'importe quel ViewModelBase (StartViewModel, ShellViewModel, etc.)
    /// </summary>
    public MainWindow(ViewModelBase viewModel) : this()
    {
        DataContext = viewModel;
    }

    /// <summary>
    /// Legacy constructor for backward compatibility
    /// </summary>
    public MainWindow(ViewModels.Core.ShellViewModel viewModel) : this((ViewModelBase)viewModel)
    {
    }
}
