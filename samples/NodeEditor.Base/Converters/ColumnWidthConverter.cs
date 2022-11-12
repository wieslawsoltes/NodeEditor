using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace NodeEditorDemo.Converters;

public class ColumnWidthConverter : IValueConverter
{
    public static ColumnWidthConverter Instance = new ();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool flag && parameter is double width)
        {
            if (flag)
            {
                return new GridLength(width);
            }
            else
            {
                return new GridLength(0);
            }
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
