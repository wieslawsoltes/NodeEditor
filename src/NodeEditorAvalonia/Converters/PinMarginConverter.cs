using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using NodeEditor.Model;

namespace NodeEditor.Converters;

public class PinMarginConverter : IValueConverter
{
    public static PinMarginConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IPin pin)
        {
            return new Thickness(-pin.Width / 2, -pin.Height / 2, 0, 0);
        }

        return new Thickness(0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
