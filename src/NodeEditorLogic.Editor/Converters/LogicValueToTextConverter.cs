using System;
using System.Globalization;
using Avalonia.Data.Converters;
using NodeEditorLogic.Models;
using NodeEditorLogic.ViewModels;

namespace NodeEditorLogic.Converters;

public sealed class LogicValueToTextConverter : IValueConverter
{
    public static readonly LogicValueToTextConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var logicValue = value switch
        {
            LogicValue logic => logic,
            LogicPinViewModel pin => pin.Value,
            _ => LogicValue.Unknown
        };

        return logicValue switch
        {
            LogicValue.High => "1",
            LogicValue.Low => "0",
            _ => "X"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
