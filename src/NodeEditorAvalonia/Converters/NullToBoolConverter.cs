using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NodeEditor.Converters;

public sealed class NullToBoolConverter : IValueConverter
{
    public static readonly NullToBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var invert = parameter is string text && string.Equals(text, "invert", StringComparison.OrdinalIgnoreCase);
        var hasValue = value is not null;
        return invert ? !hasValue : hasValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
