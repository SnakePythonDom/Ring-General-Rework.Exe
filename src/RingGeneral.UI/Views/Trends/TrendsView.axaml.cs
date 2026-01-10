using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RingGeneral.UI.ViewModels.Trends;

namespace RingGeneral.UI.Views.Trends;

public partial class TrendsView : UserControl
{
    public TrendsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}