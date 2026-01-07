using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;

namespace RingGeneral.UI.Components;

/// <summary>
/// Advanced DataGrid component with sorting, filtering, pagination, and CSV export
/// Used for Worker lists, Title rankings, Youth trainees, Financial reports, etc.
/// </summary>
public partial class SortableDataGrid : UserControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsProperty =
        AvaloniaProperty.Register<SortableDataGrid, IEnumerable?>(
            nameof(Items),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<SortableDataGrid, object?>(
            nameof(SelectedItem),
            defaultValue: null,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> SearchTextProperty =
        AvaloniaProperty.Register<SortableDataGrid, string>(
            nameof(SearchText),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> CanFilterProperty =
        AvaloniaProperty.Register<SortableDataGrid, bool>(
            nameof(CanFilter),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> CanExportProperty =
        AvaloniaProperty.Register<SortableDataGrid, bool>(
            nameof(CanExport),
            defaultValue: true,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> FiltersVisibleProperty =
        AvaloniaProperty.Register<SortableDataGrid, bool>(
            nameof(FiltersVisible),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> FilterContentProperty =
        AvaloniaProperty.Register<SortableDataGrid, object?>(
            nameof(FilterContent),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> StatusTextProperty =
        AvaloniaProperty.Register<SortableDataGrid, string>(
            nameof(StatusText),
            defaultValue: "0 items",
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> HasPaginationProperty =
        AvaloniaProperty.Register<SortableDataGrid, bool>(
            nameof(HasPagination),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> PaginationTextProperty =
        AvaloniaProperty.Register<SortableDataGrid, string>(
            nameof(PaginationText),
            defaultValue: "Page 1 of 1",
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> CanGoPreviousProperty =
        AvaloniaProperty.Register<SortableDataGrid, bool>(
            nameof(CanGoPrevious),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> CanGoNextProperty =
        AvaloniaProperty.Register<SortableDataGrid, bool>(
            nameof(CanGoNext),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> PrimaryActionTextProperty =
        AvaloniaProperty.Register<SortableDataGrid, string>(
            nameof(PrimaryActionText),
            defaultValue: "Action",
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> HasPrimaryActionProperty =
        AvaloniaProperty.Register<SortableDataGrid, bool>(
            nameof(HasPrimaryAction),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public string SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public bool CanFilter
    {
        get => GetValue(CanFilterProperty);
        set => SetValue(CanFilterProperty, value);
    }

    public bool CanExport
    {
        get => GetValue(CanExportProperty);
        set => SetValue(CanExportProperty, value);
    }

    public bool FiltersVisible
    {
        get => GetValue(FiltersVisibleProperty);
        set => SetValue(FiltersVisibleProperty, value);
    }

    public object? FilterContent
    {
        get => GetValue(FilterContentProperty);
        set => SetValue(FilterContentProperty, value);
    }

    public string StatusText
    {
        get => GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public bool HasPagination
    {
        get => GetValue(HasPaginationProperty);
        set => SetValue(HasPaginationProperty, value);
    }

    public string PaginationText
    {
        get => GetValue(PaginationTextProperty);
        set => SetValue(PaginationTextProperty, value);
    }

    public bool CanGoPrevious
    {
        get => GetValue(CanGoPreviousProperty);
        set => SetValue(CanGoPreviousProperty, value);
    }

    public bool CanGoNext
    {
        get => GetValue(CanGoNextProperty);
        set => SetValue(CanGoNextProperty, value);
    }

    public string PrimaryActionText
    {
        get => GetValue(PrimaryActionTextProperty);
        set => SetValue(PrimaryActionTextProperty, value);
    }

    public bool HasPrimaryAction
    {
        get => GetValue(HasPrimaryActionProperty);
        set => SetValue(HasPrimaryActionProperty, value);
    }

    public ReactiveCommand<Unit, Unit> ToggleFiltersCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportCsvCommand { get; }
    public ReactiveCommand<Unit, Unit> PrimaryActionCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; }

    public SortableDataGrid()
    {
        InitializeComponent();
        DataContext = this;

        ToggleFiltersCommand = ReactiveCommand.Create(ToggleFilters);
        ExportCsvCommand = ReactiveCommand.Create(ExportCsv);
        PrimaryActionCommand = ReactiveCommand.Create(() => { }); // Override in usage
        PreviousPageCommand = ReactiveCommand.Create(PreviousPage);
        NextPageCommand = ReactiveCommand.Create(NextPage);

        // Update status text when items change
        this.GetObservable(ItemsProperty).Subscribe(items =>
        {
            UpdateStatusText(items);
        });
    }

    private void ToggleFilters()
    {
        FiltersVisible = !FiltersVisible;
    }

    private void ExportCsv()
    {
        if (Items == null)
            return;

        // Basic CSV export implementation
        var csv = new StringBuilder();

        // For now, just notify that export was requested
        // Full implementation would use SaveFileDialog and write to file
        System.Diagnostics.Debug.WriteLine("[SortableDataGrid] CSV export requested");

        // TODO: Implement full CSV export with:
        // 1. SaveFileDialog to select location
        // 2. Column headers from DataGrid.Columns
        // 3. Row data from Items collection
        // 4. Proper CSV escaping (quotes, commas)
    }

    private void PreviousPage()
    {
        // Pagination logic - to be implemented by consumer
        System.Diagnostics.Debug.WriteLine("[SortableDataGrid] Previous page requested");
    }

    private void NextPage()
    {
        // Pagination logic - to be implemented by consumer
        System.Diagnostics.Debug.WriteLine("[SortableDataGrid] Next page requested");
    }

    private void UpdateStatusText(IEnumerable? items)
    {
        if (items == null)
        {
            StatusText = "0 items";
            return;
        }

        var count = 0;
        foreach (var _ in items)
            count++;

        StatusText = count == 1 ? "1 item" : $"{count} items";
    }
}
