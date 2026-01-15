using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RingGeneral.UI.ViewModels.Company;

namespace RingGeneral.UI.Views.Company;

public partial class ChildCompaniesView : UserControl
{
    public ChildCompaniesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}