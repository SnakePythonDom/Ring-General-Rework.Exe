using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RingGeneral.UI.Views.Roster;

public sealed partial class WorkerDetailView : UserControl
{
    public WorkerDetailView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
