using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using NodeEditorLogic.Models;
using NodeEditorLogic.ViewModels;

namespace NodeEditorLogic.Converters;

public sealed class LogicValueToBrushConverter : IValueConverter
{
    public static readonly LogicValueToBrushConverter Instance = new();

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
            LogicValue.High => GetBrush("LogicHighBrush") ?? new SolidColorBrush(Color.Parse("#2F9E44")),
            LogicValue.Low => GetBrush("LogicLowBrush") ?? new SolidColorBrush(Color.Parse("#D9480F")),
            _ => GetBrush("LogicUnknownBrush") ?? new SolidColorBrush(Color.Parse("#8A94A6"))
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    internal static IBrush? GetBrush(string key)
    {
        if (Application.Current?.Resources is not { } resources)
        {
            return null;
        }

        if (resources.TryGetValue(key, out var resource))
        {
            return resource as IBrush;
        }

        if (Application.Current is not { } app)
        {
            return null;
        }

        if (ResourceNodeExtensions.TryFindResource(app, key, app.ActualThemeVariant, out resource))
        {
            return resource as IBrush;
        }

        if (ResourceNodeExtensions.TryFindResource(app, key, ThemeVariant.Default, out resource))
        {
            return resource as IBrush;
        }

        return null;
    }
}
