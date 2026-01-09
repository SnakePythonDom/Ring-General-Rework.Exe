using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace RingGeneral.UI.Components;

/// <summary>
/// Panel component that groups multiple AttributeBars by category
/// Used for organizing worker attributes, coach skills, etc. by logical groups
/// </summary>
public partial class AttributeCategoryPanel : UserControl
{
    public static readonly StyledProperty<string> CategoryTitleProperty =
        AvaloniaProperty.Register<AttributeCategoryPanel, string>(
            nameof(CategoryTitle),
            defaultValue: "Category",
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> CategoryIconProperty =
        AvaloniaProperty.Register<AttributeCategoryPanel, string>(
            nameof(CategoryIcon),
            defaultValue: "📊",
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> CategoryDescriptionProperty =
        AvaloniaProperty.Register<AttributeCategoryPanel, string>(
            nameof(CategoryDescription),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<IEnumerable<object>> AttributesProperty =
        AvaloniaProperty.Register<AttributeCategoryPanel, IEnumerable<object>>(
            nameof(Attributes),
            defaultValue: Enumerable.Empty<object>(),
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> HasAttributesProperty =
        AvaloniaProperty.Register<AttributeCategoryPanel, bool>(
            nameof(HasAttributes),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public string CategoryTitle
    {
        get => GetValue(CategoryTitleProperty);
        set => SetValue(CategoryTitleProperty, value);
    }

    public string CategoryIcon
    {
        get => GetValue(CategoryIconProperty);
        set => SetValue(CategoryIconProperty, value);
    }

    public string CategoryDescription
    {
        get => GetValue(CategoryDescriptionProperty);
        set => SetValue(CategoryDescriptionProperty, value);
    }

    public IEnumerable<object> Attributes
    {
        get => GetValue(AttributesProperty);
        set => SetValue(AttributesProperty, value);
    }

    public bool HasAttributes
    {
        get => GetValue(HasAttributesProperty);
        private set => SetValue(HasAttributesProperty, value);
    }

    public AttributeCategoryPanel()
    {
        InitializeComponent();
        DataContext = this;

        // Observe changes to Attributes collection
        this.GetObservable(AttributesProperty).Subscribe(_ => UpdateHasAttributes());
    }

    private void UpdateHasAttributes()
    {
        HasAttributes = Attributes?.Any() ?? false;
    }
}
