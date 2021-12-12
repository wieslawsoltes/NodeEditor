using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace NodeEditor.Controls;

public class GridDecorator : Decorator
{
    public static readonly StyledProperty<bool> EnableGridProperty = 
        AvaloniaProperty.Register<GridDecorator, bool>(nameof(EnableGrid));

    public static readonly StyledProperty<double> CellWidthProperty = 
        AvaloniaProperty.Register<GridDecorator, double>(nameof(CellWidth));

    public static readonly StyledProperty<double> CellHeightProperty = 
        AvaloniaProperty.Register<GridDecorator, double>(nameof(CellHeight));

    public bool EnableGrid
    {
        get => GetValue(EnableGridProperty);
        set => SetValue(EnableGridProperty, value);
    }

    public double CellWidth
    {
        get => GetValue(CellWidthProperty);
        set => SetValue(CellWidthProperty, value);
    }

    public double CellHeight
    {
        get => GetValue(CellHeightProperty);
        set => SetValue(CellHeightProperty, value);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EnableGridProperty
            || change.Property == CellWidthProperty
            || change.Property == CellHeightProperty)
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!EnableGrid)
        {
            return;
        }

        var cw = CellWidth;
        var ch = CellHeight;
        if (cw <= 0 || ch <= 0.0)
        {
            return;
        }
        
        var rect = Bounds;
        var thickness = 1.0;
        // rect = rect.Deflate(thickness * 0.5);

        var brush = new ImmutableSolidColorBrush(Color.FromArgb(255, 222, 222, 222));
        var pen = new ImmutablePen(brush, thickness);

        var ox = rect.X;
        var ex = rect.X + rect.Width;
        var oy = rect.Y;
        var ey = rect.Y + rect.Height;
        for (var x = ox + cw; x < ex; x += cw)
        {
            var p0 = new Point(x, oy);
            var p1 = new Point(x, ey);
            context.DrawLine(pen, p0, p1);
        }

        for (var y = oy + ch; y < ey; y += ch)
        {
            var p0 = new Point(ox, y);
            var p1 = new Point(ex, y);
            context.DrawLine(pen, p0, p1);
        }

        context.DrawRectangle(null, pen, rect);
    }
}
