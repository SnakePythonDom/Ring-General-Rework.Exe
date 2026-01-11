using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using RingGeneral.UI.ViewModels.Booking;

namespace RingGeneral.UI.Views.Booking;

public partial class WorkerSelectionView : UserControl
{
    public WorkerSelectionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var listBox = this.FindControl<ListBox>("WorkerList");
        if (listBox != null)
        {
            listBox.SelectionChanged += ListBox_SelectionChanged;
        }
    }

    private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is WorkerSelectionViewModel vm && e.AddedItems.Count > 0)
        {
            if (e.AddedItems[0] is ViewModels.ParticipantViewModel p)
            {
                vm.SelectWorkerCommand.Execute(p).Subscribe();
            }
        }
    }
}
