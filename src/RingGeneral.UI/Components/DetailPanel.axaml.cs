using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace RingGeneral.UI.Components;

/// <summary>
/// Reusable detail panel component for the Context Panel (right column)
/// Supports collapsible sections, custom content, and empty states
/// Used in Booking validation, Worker details, Segment details, etc.
/// </summary>
public partial class DetailPanel : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<DetailPanel, string>(
            nameof(Title),
            defaultValue: "Details",
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> SubtitleProperty =
        AvaloniaProperty.Register<DetailPanel, string>(
            nameof(Subtitle),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> HasSubtitleProperty =
        AvaloniaProperty.Register<DetailPanel, bool>(
            nameof(HasSubtitle),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<DetailPanel, object?>(
            nameof(Content),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsEmptyProperty =
        AvaloniaProperty.Register<DetailPanel, bool>(
            nameof(IsEmpty),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> EmptyMessageProperty =
        AvaloniaProperty.Register<DetailPanel, string>(
            nameof(EmptyMessage),
            defaultValue: "Aucune information à afficher",
            defaultBindingMode: BindingMode.OneWay);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public bool HasSubtitle
    {
        get => GetValue(HasSubtitleProperty);
        private set => SetValue(HasSubtitleProperty, value);
    }

    public new object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public bool IsEmpty
    {
        get => GetValue(IsEmptyProperty);
        set => SetValue(IsEmptyProperty, value);
    }

    public string EmptyMessage
    {
        get => GetValue(EmptyMessageProperty);
        set => SetValue(EmptyMessageProperty, value);
    }

    public DetailPanel()
    {
        InitializeComponent();
        DataContext = this;

        // Update HasSubtitle when Subtitle changes
        this.GetObservable(SubtitleProperty).Subscribe(subtitle =>
        {
            HasSubtitle = !string.IsNullOrWhiteSpace(subtitle);
        });

        // Update IsEmpty when Content changes
        this.GetObservable(ContentProperty).Subscribe(content =>
        {
            if (!GetValue(IsEmptyProperty))
            {
                IsEmpty = content == null;
            }
        });
    }
}
