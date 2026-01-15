using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RingGeneral.UI.ViewModels.Roster;

namespace RingGeneral.UI.Views.Roster;

public partial class StructuralDashboardView : UserControl
{
    public StructuralDashboardView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}