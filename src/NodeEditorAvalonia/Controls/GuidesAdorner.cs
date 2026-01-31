using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace NodeEditor.Controls;

public class GuidesAdorner : Control
{
    public static readonly StyledProperty<IReadOnlyList<GuideLine>?> GuidesProperty =
        AvaloniaProperty.Register<GuidesAdorner, IReadOnlyList<GuideLine>?>(nameof(Guides));

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<GuidesAdorner, IBrush?>(
            nameof(Stroke),
            new ImmutableSolidColorBrush(Color.FromArgb(0xFF, 0x17, 0x9D, 0xE3)));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<GuidesAdorner, double>(nameof(StrokeThickness), 1.0);

    public IReadOnlyList<GuideLine>? Guides
    {
        get => GetValue(GuidesProperty);
        set => SetValue(GuidesProperty, value);
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

        if (change.Property == GuidesProperty || change.Property == StrokeProperty || change.Property == StrokeThicknessProperty)
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Guides is not { Count: > 0 } guides)
        {
            return;
        }

        var brush = Stroke;
        if (brush is null)
        {
            return;
        }

        var thickness = StrokeThickness;
        var pen = new ImmutablePen(brush.ToImmutable(), thickness);

        foreach (var guide in guides)
        {
            context.DrawLine(pen, guide.Start, guide.End);
        }
    }
}
