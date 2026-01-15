using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace RingGeneral.UI.Components;

/// <summary>
/// Collapsible section component for use inside DetailPanel
/// Supports badges (success/warning/error/info) and custom content
/// </summary>
public partial class DetailSection : UserControl
{
    public static readonly StyledProperty<string> SectionTitleProperty =
        AvaloniaProperty.Register<DetailSection, string>(
            nameof(SectionTitle),
            defaultValue: "Section",
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<DetailSection, bool>(
            nameof(IsExpanded),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> SectionContentProperty =
        AvaloniaProperty.Register<DetailSection, object?>(
            nameof(SectionContent),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> BadgeTextProperty =
        AvaloniaProperty.Register<DetailSection, string>(
            nameof(BadgeText),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> HasBadgeProperty =
        AvaloniaProperty.Register<DetailSection, bool>(
            nameof(HasBadge),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<BadgeType> BadgeTypeProperty =
        AvaloniaProperty.Register<DetailSection, BadgeType>(
            nameof(BadgeType),
            defaultValue: BadgeType.Info,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsSuccessBadgeProperty =
        AvaloniaProperty.Register<DetailSection, bool>(
            nameof(IsSuccessBadge),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsWarningBadgeProperty =
        AvaloniaProperty.Register<DetailSection, bool>(
            nameof(IsWarningBadge),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsErrorBadgeProperty =
        AvaloniaProperty.Register<DetailSection, bool>(
            nameof(IsErrorBadge),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsInfoBadgeProperty =
        AvaloniaProperty.Register<DetailSection, bool>(
            nameof(IsInfoBadge),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public string SectionTitle
    {
        get => GetValue(SectionTitleProperty);
        set => SetValue(SectionTitleProperty, value);
    }

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public object? SectionContent
    {
        get => GetValue(SectionContentProperty);
        set => SetValue(SectionContentProperty, value);
    }

    public string BadgeText
    {
        get => GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    public bool HasBadge
    {
        get => GetValue(HasBadgeProperty);
        private set => SetValue(HasBadgeProperty, value);
    }

    public BadgeType BadgeType
    {
        get => GetValue(BadgeTypeProperty);
        set => SetValue(BadgeTypeProperty, value);
    }

    public bool IsSuccessBadge
    {
        get => GetValue(IsSuccessBadgeProperty);
        private set => SetValue(IsSuccessBadgeProperty, value);
    }

    public bool IsWarningBadge
    {
        get => GetValue(IsWarningBadgeProperty);
        private set => SetValue(IsWarningBadgeProperty, value);
    }

    public bool IsErrorBadge
    {
        get => GetValue(IsErrorBadgeProperty);
        private set => SetValue(IsErrorBadgeProperty, value);
    }

    public bool IsInfoBadge
    {
        get => GetValue(IsInfoBadgeProperty);
        private set => SetValue(IsInfoBadgeProperty, value);
    }

    public DetailSection()
    {
        InitializeComponent();
        DataContext = this;

        // Update badge visibility
        this.GetObservable(BadgeTextProperty).Subscribe(text =>
        {
            HasBadge = !string.IsNullOrWhiteSpace(text);
        });

        // Update badge type flags
        this.GetObservable(BadgeTypeProperty).Subscribe(type =>
        {
            IsSuccessBadge = type == BadgeType.Success;
            IsWarningBadge = type == BadgeType.Warning;
            IsErrorBadge = type == BadgeType.Error;
            IsInfoBadge = type == BadgeType.Info;
        });
    }
}

public enum BadgeType
{
    Info,
    Success,
    Warning,
    Error
}
