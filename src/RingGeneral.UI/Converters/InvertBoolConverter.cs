using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RingGeneral.UI.Converters;

/// <summary>
/// Convertisseur qui inverse une valeur booléenne
/// </summary>
public class InvertBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return false;
    }
}