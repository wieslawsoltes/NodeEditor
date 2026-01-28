using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace NodeEditor.Converters;

public sealed class EnumEqualsConverter : IValueConverter
{
    public static readonly EnumEqualsConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Equals(value, parameter);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
        {
            return parameter;
        }

        return BindingOperations.DoNothing;
    }
}
