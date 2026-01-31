using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using NodeEditor;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class ConnectorSelectedAdorner : Control
{
    public static readonly StyledProperty<IReadOnlyList<IConnector>?> ConnectorsProperty =
        AvaloniaProperty.Register<ConnectorSelectedAdorner, IReadOnlyList<IConnector>?>(nameof(Connectors));

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<ConnectorSelectedAdorner, IBrush?>(nameof(Stroke));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<ConnectorSelectedAdorner, double>(nameof(StrokeThickness), 2.0);

    public IReadOnlyList<IConnector>? Connectors
    {
        get => GetValue(ConnectorsProperty);
        set => SetValue(ConnectorsProperty, value);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
#pragma warning disable 8631
        base.OnPropertyChanged(change);
#pragma warning restore 8631

        if (change.Property == ConnectorsProperty || change.Property == StrokeProperty || change.Property == StrokeThicknessProperty)
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Connectors is not { Count: > 0 } connectors)
        {
            return;
        }

        var brush = Stroke;
        if (brush is null)
        {
            return;
        }

        var pen = new ImmutablePen(brush.ToImmutable(), StrokeThickness);

        foreach (var connector in connectors)
        {
            if (!ConnectorPathHelper.TryGetEndpoints(connector, out var start, out var end))
            {
                continue;
            }

            var points = ConnectorPathHelper.GetFlattenedPath(connector, start, end);
            if (points.Count == 0)
            {
                continue;
            }

            if (points.Count == 1)
            {
                context.DrawLine(pen, points[0], points[0]);
                continue;
            }

            for (var i = 1; i < points.Count; i++)
            {
                context.DrawLine(pen, points[i - 1], points[i]);
            }
        }
    }
}
