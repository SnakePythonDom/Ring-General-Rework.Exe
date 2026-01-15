using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RingGeneral.UI.Views.Youth;

public partial class YouthHubView : UserControl
{
    public YouthHubView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
