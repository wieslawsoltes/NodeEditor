using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NodeEditor.Converters;

public sealed class PlaceholderVisibilityConverter : IMultiValueConverter
{
    public static readonly PlaceholderVisibilityConverter Instance = new();

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = values.Count > 0 ? values[0] as string : null;
        var isEditing = values.Count > 1 && values[1] is bool flag && flag;
        return !isEditing && string.IsNullOrWhiteSpace(text);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
