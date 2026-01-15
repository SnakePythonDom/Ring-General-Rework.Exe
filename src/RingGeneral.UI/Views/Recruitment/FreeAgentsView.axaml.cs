using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RingGeneral.UI.Views.Recruitment;

public partial class FreeAgentsView : UserControl
{
    public FreeAgentsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
