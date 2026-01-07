using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RingGeneral.UI.ViewModels.Start;

namespace RingGeneral.UI.Views.Start;

public sealed partial class CompanySelectorView : UserControl
{
    public CompanySelectorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnCompanySelected(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is CompanyListItem company)
        {
            // Mettre à jour la sélection dans le ViewModel
            if (DataContext is CompanySelectorViewModel viewModel)
            {
                viewModel.SelectedCompany = company;
                viewModel.SelectCompanyCommand.Execute().Subscribe();
            }
        }
    }
}
