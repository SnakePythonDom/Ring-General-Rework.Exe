using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;

namespace RingGeneral.UI.Components;

/// <summary>
/// Visual stat bar component with color graduation (red/orange/green)
/// Used for displaying worker attributes, youth potential, coach skills, etc.
/// </summary>
public partial class AttributeBar : UserControl
{
    public static readonly StyledProperty<string> AttributeNameProperty =
        AvaloniaProperty.Register<AttributeBar, string>(
            nameof(AttributeName),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<AttributeBar, int>(
            nameof(Value),
            defaultValue: 0,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<int?> PreviousValueProperty =
        AvaloniaProperty.Register<AttributeBar, int?>(
            nameof(PreviousValue),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<AttributeBar, string>(
            nameof(Description),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<int> MaxValueProperty =
        AvaloniaProperty.Register<AttributeBar, int>(
            nameof(MaxValue),
            defaultValue: 100,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<double> BarWidthProperty =
        AvaloniaProperty.Register<AttributeBar, double>(
            nameof(BarWidth),
            defaultValue: 0.0,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> DeltaTextProperty =
        AvaloniaProperty.Register<AttributeBar, string>(
            nameof(DeltaText),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> HasPositiveDeltaProperty =
        AvaloniaProperty.Register<AttributeBar, bool>(
            nameof(HasPositiveDelta),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> HasNegativeDeltaProperty =
        AvaloniaProperty.Register<AttributeBar, bool>(
            nameof(HasNegativeDelta),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsLowProperty =
        AvaloniaProperty.Register<AttributeBar, bool>(
            nameof(IsLow),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsMediumProperty =
        AvaloniaProperty.Register<AttributeBar, bool>(
            nameof(IsMedium),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsHighProperty =
        AvaloniaProperty.Register<AttributeBar, bool>(
            nameof(IsHigh),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public string AttributeName
    {
        get => GetValue(AttributeNameProperty);
        set => SetValue(AttributeNameProperty, value);
    }

    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int? PreviousValue
    {
        get => GetValue(PreviousValueProperty);
        set => SetValue(PreviousValueProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public int MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public double BarWidth
    {
        get => GetValue(BarWidthProperty);
        private set => SetValue(BarWidthProperty, value);
    }

    public string DeltaText
    {
        get => GetValue(DeltaTextProperty);
        private set => SetValue(DeltaTextProperty, value);
    }

    public bool HasPositiveDelta
    {
        get => GetValue(HasPositiveDeltaProperty);
        private set => SetValue(HasPositiveDeltaProperty, value);
    }

    public bool HasNegativeDelta
    {
        get => GetValue(HasNegativeDeltaProperty);
        private set => SetValue(HasNegativeDeltaProperty, value);
    }

    public bool IsLow
    {
        get => GetValue(IsLowProperty);
        private set => SetValue(IsLowProperty, value);
    }

    public bool IsMedium
    {
        get => GetValue(IsMediumProperty);
        private set => SetValue(IsMediumProperty, value);
    }

    public bool IsHigh
    {
        get => GetValue(IsHighProperty);
        private set => SetValue(IsHighProperty, value);
    }

    public AttributeBar()
    {
        InitializeComponent();
        DataContext = this;

        // Observe property changes
        this.GetObservable(ValueProperty).Subscribe(_ => UpdateBarState());
        this.GetObservable(PreviousValueProperty).Subscribe(_ => UpdateBarState());
        this.GetObservable(MaxValueProperty).Subscribe(_ => UpdateBarState());
        this.GetObservable(BoundsProperty).Subscribe(_ => UpdateBarState());
    }

    private void UpdateBarState()
    {
        // Calculate percentage
        var percentage = MaxValue > 0 ? (double)Value / MaxValue : 0;
        var normalizedValue = Math.Clamp(percentage * 100, 0, 100);

        // Update bar width (relative to container width, max 280px for typical layout)
        var containerWidth = Bounds.Width > 0 ? Bounds.Width - 16 : 280;
        BarWidth = (normalizedValue / 100.0) * containerWidth;

        // Update color thresholds
        IsLow = normalizedValue < 50;
        IsMedium = normalizedValue >= 50 && normalizedValue < 70;
        IsHigh = normalizedValue >= 70;

        // Update delta
        if (PreviousValue.HasValue && PreviousValue.Value != Value)
        {
            var delta = Value - PreviousValue.Value;
            HasPositiveDelta = delta > 0;
            HasNegativeDelta = delta < 0;
            DeltaText = delta > 0 ? $"↑{delta}" : $"↓{Math.Abs(delta)}";
        }
        else
        {
            HasPositiveDelta = false;
            HasNegativeDelta = false;
            DeltaText = string.Empty;
        }
    }
}
