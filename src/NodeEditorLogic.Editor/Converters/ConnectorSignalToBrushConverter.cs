using System;
using System.Globalization;
using Avalonia.Data.Converters;
using NodeEditor.Model;
using NodeEditorLogic.Models;
using NodeEditorLogic.ViewModels;

namespace NodeEditorLogic.Converters;

public sealed class ConnectorSignalToBrushConverter : IValueConverter
{
    public static readonly ConnectorSignalToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IConnector connector)
        {
            return LogicValueToBrushConverter.Instance.Convert(LogicValue.Unknown, targetType, parameter, culture);
        }

        var logicValue = ExtractValue(connector.Start, connector.End);
        return LogicValueToBrushConverter.Instance.Convert(logicValue, targetType, parameter, culture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static LogicValue ExtractValue(IPin? start, IPin? end)
    {
        var startPin = start as LogicPinViewModel;
        var endPin = end as LogicPinViewModel;

        if (startPin?.Kind == LogicPinKind.Output)
        {
            return startPin.Value;
        }

        if (endPin?.Kind == LogicPinKind.Output)
        {
            return endPin.Value;
        }

        return LogicValue.Unknown;
    }
}
