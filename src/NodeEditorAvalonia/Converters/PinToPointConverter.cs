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
            if (Math.Abs(pin.Parent.Rotation) > 0.001)
            {
                var centerX = pin.Parent.Width / 2.0;
                var centerY = pin.Parent.Height / 2.0;
                var radians = pin.Parent.Rotation * Math.PI / 180.0;
                var cos = Math.Cos(radians);
                var sin = Math.Sin(radians);
                var dx = x - centerX;
                var dy = y - centerY;
                var rotatedX = dx * cos - dy * sin + centerX;
                var rotatedY = dx * sin + dy * cos + centerY;
                x = rotatedX;
                y = rotatedY;
            }

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
