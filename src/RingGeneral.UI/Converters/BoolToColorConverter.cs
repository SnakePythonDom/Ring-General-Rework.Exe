using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace RingGeneral.UI.Converters;

/// <summary>
/// Convertisseur qui retourne une couleur selon une valeur booléenne
/// Format du parameter: "couleurSiTrue:couleurSiFalse" (ex: "#f59e0b:#3b82f6")
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string paramString)
            return null;

        var colors = paramString.Split(':');
        if (colors.Length != 2)
            return null;

        var colorString = boolValue ? colors[0].Trim() : colors[1].Trim();
        
        // Essayer de parser la couleur
        if (Color.TryParse(colorString, out var color))
        {
            return new SolidColorBrush(color);
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
