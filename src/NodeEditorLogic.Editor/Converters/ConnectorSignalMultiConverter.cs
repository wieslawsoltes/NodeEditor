using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using NodeEditorLogic.Models;
using NodeEditorLogic.ViewModels;

namespace NodeEditorLogic.Converters;

public sealed class ConnectorSignalMultiConverter : IMultiValueConverter
{
    public static readonly ConnectorSignalMultiConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var startPin = values.Count > 0 ? values[0] as LogicPinViewModel : null;
        var endPin = values.Count > 1 ? values[1] as LogicPinViewModel : null;
        var isInvalid = GetBool(values, 2);
        var isContention = GetBool(values, 3);

        if (isInvalid)
        {
            return LogicValueToBrushConverter.GetBrush("LogicInvalidBrush")
                ?? new SolidColorBrush(Color.Parse("#E03131"));
        }

        if (isContention)
        {
            return LogicValueToBrushConverter.GetBrush("LogicContentionBrush")
                ?? new SolidColorBrush(Color.Parse("#F08C00"));
        }

        var logicValue = LogicValue.Unknown;
        var outputPin = startPin?.Kind == LogicPinKind.Output ? startPin
            : endPin?.Kind == LogicPinKind.Output ? endPin
            : null;

        if (outputPin is not null)
        {
            logicValue = outputPin.BusWidth > 1
                ? LogicSignalHelper.Aggregate(outputPin.BusValue)
                : outputPin.Value;
        }

        return LogicValueToBrushConverter.Instance.Convert(logicValue, targetType, parameter, culture);
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

    private static IBrush? GetBrush(string key) => LogicValueToBrushConverter.GetBrush(key);
}
