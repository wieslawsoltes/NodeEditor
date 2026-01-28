using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Data.Converters;

namespace NodeEditorLogic.Converters;

public sealed class ConnectorDashArrayConverter : IMultiValueConverter
{
    public static readonly ConnectorDashArrayConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var isInvalid = GetBool(values, 0);
        var isContention = GetBool(values, 1);

        if (isContention)
        {
            return new AvaloniaList<double> { 4d, 2d };
        }

        if (isInvalid)
        {
            return new AvaloniaList<double> { 2d, 2d };
        }

        return null;
    }

    public object ConvertBack(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static bool GetBool(IList<object?> values, int index)
    {
        if (values.Count > index && values[index] is bool flag)
        {
            return flag;
        }

        return false;
    }
}
