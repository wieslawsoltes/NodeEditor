using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace NodeEditor.Controls;

public class SelectionAdorner : Control
{
    public static readonly StyledProperty<Point> TopLeftProperty =
        AvaloniaProperty.Register<SelectionAdorner, Point>(nameof(TopLeft));

    public static readonly StyledProperty<Point> BottomRightProperty =
        AvaloniaProperty.Register<SelectionAdorner, Point>(nameof(BottomRight));

    public Point TopLeft
    {
        get => GetValue(TopLeftProperty);
        set => SetValue(TopLeftProperty, value);
    }

    public Point BottomRight
    {
        get => GetValue(BottomRightProperty);
        set => SetValue(BottomRightProperty, value);
    }

    public Rect GetRect()
    {
        var topLeftX = Math.Min(TopLeft.X, BottomRight.X);
        var topLeftY = Math.Min(TopLeft.Y, BottomRight.Y);
        var bottomRightX = Math.Max(TopLeft.X, BottomRight.X);
        var bottomRightY = Math.Max(TopLeft.Y, BottomRight.Y);
        return new Rect(
            new Point(topLeftX, topLeftY),
            new Point(bottomRightX, bottomRightY));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
#pragma warning disable 8631
        base.OnPropertyChanged(change);
#pragma warning restore 8631

        if (change.Property == TopLeftProperty || change.Property == BottomRightProperty)
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var brush = new ImmutableSolidColorBrush(new Color(0xFF, 0x00, 0x00, 0xFF), 0.3);
        var thickness = 2.0;
        var pen = new ImmutablePen(
            new ImmutableSolidColorBrush(new Color(0xFF, 0x00, 0x00, 0xFF)),
            thickness);
        var bounds = GetRect();
        var rect = bounds.Deflate(thickness * 0.5);
        context.DrawRectangle(brush, pen, rect);
    }
}
