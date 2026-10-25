using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using NodeEditorDemo.ViewModels;

namespace NodeEditorDemo.Converters;

/// <summary>Maps sample wire channels to immutable brushes without changing the connector model.</summary>
public sealed class WireColorConverter : IValueConverter
{
    private readonly IBrush[] _brushes =
    {
        new ImmutableSolidColorBrush(0xFFADB8D1),
        new ImmutableSolidColorBrush(0xFF54D6C2),
        new ImmutableSolidColorBrush(0xFFFFCE66),
        new ImmutableSolidColorBrush(0xFFFF916E),
        new ImmutableSolidColorBrush(0xFF7C9CFF),
        new ImmutableSolidColorBrush(0xFFD486F6)
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is WireConnectorViewModel wire
            ? _brushes[Math.Clamp(wire.Channel, 0, _brushes.Length - 1)]
            : _brushes[0];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
