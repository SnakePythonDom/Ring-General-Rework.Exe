using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RingGeneral.UI.Views.Start;

public sealed partial class StartView : UserControl
{
    public StartView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
