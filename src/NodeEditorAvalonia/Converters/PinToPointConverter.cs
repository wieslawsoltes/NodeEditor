using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using NodeEditor.Model;

namespace NodeEditor.Converters;

public class PinToPointConverter : IValueConverter
{
    public static PinToPointConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IPin pin)
        {
            return new Point();
        }

        var x = pin.X;
        var y = pin.Y;

        if (pin.Parent is not null)
        {
            x += pin.Parent.X;
            y += pin.Parent.Y;
        }

        return new Point(x, y);

    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
