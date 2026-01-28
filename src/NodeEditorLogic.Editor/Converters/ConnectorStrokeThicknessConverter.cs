using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NodeEditorLogic.Converters;

public sealed class ConnectorStrokeThicknessConverter : IMultiValueConverter
{
    public static readonly ConnectorStrokeThicknessConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var isBus = GetBool(values, 0);
        var isInvalid = GetBool(values, 1);
        var isContention = GetBool(values, 2);

        if (isInvalid || isContention || isBus)
        {
            return 3d;
        }

        return 2d;
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
