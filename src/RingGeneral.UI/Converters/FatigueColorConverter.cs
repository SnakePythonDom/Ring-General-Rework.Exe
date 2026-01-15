using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace RingGeneral.UI.Converters;

public class FatigueColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int fatigue)
        {
            if (fatigue >= 80) return Brushes.Red;
            if (fatigue >= 50) return Brushes.Orange;
            if (fatigue >= 30) return Brushes.Goldenrod;
            return Brushes.Green;
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
