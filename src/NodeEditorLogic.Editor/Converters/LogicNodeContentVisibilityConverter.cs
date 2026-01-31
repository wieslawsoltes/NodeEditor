using System;
using System.Globalization;
using Avalonia.Data.Converters;
using NodeEditorLogic.ViewModels.Nodes;

namespace NodeEditorLogic.Converters;

public sealed class LogicNodeContentVisibilityConverter : IValueConverter
{
    public static readonly LogicNodeContentVisibilityConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isLogicNode = value is LogicNodeContentViewModel;

        if (parameter is string mode && mode.Equals("invert", StringComparison.OrdinalIgnoreCase))
        {
            return !isLogicNode;
        }

        return isLogicNode;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
