using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RingGeneral.UI.Views.Company;

public sealed partial class ChildCompanyBookingView : UserControl
{
    public ChildCompanyBookingView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
