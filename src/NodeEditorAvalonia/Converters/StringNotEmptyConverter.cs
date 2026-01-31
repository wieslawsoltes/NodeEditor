using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NodeEditor.Converters;

public sealed class StringNotEmptyConverter : IValueConverter
{
    public static readonly StringNotEmptyConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string;
        var isNotEmpty = !string.IsNullOrWhiteSpace(text);

        if (parameter is string mode && mode.Equals("invert", StringComparison.OrdinalIgnoreCase))
        {
            return !isNotEmpty;
        }

        return isNotEmpty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
