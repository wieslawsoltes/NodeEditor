using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace NodeEditor.Converters;

public sealed class ColumnWidthConverter : IValueConverter
{
    public static readonly ColumnWidthConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isVisible && parameter is double width)
        {
            return isVisible ? new GridLength(width, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
        }

        return new GridLength(0, GridUnitType.Pixel);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
