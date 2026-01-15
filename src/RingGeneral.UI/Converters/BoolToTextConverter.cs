using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RingGeneral.UI.Converters;

/// <summary>
/// Convertisseur booléen vers texte avec paramètres personnalisables
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var parts = param.Split('|');
            return boolValue ? parts[0] : parts.Length > 1 ? parts[1] : parts[0];
        }

        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}