using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RingGeneral.UI.Converters;

/// <summary>
/// Convertisseur qui retourne true si la valeur n'est pas null
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}