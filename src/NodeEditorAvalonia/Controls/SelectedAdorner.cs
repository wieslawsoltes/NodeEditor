using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace NodeEditor.Controls;

public class SelectedAdorner : Control
{
    public static readonly StyledProperty<Rect> RectProperty =
        AvaloniaProperty.Register<SelectedAdorner, Rect>(nameof(Rect));

    public Rect Rect
    {
        get => GetValue(RectProperty);
        set => SetValue(RectProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
#pragma warning disable 8631
        base.OnPropertyChanged(change);
#pragma warning restore 8631

        if (change.Property == RectProperty)
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var thickness = 2.0;
        var pen = new ImmutablePen(
            new ImmutableSolidColorBrush(new Color(0xFF, 0x17, 0x9D, 0xE3)), 
            thickness);
        var bounds = Rect;
        var rect = bounds.Deflate(thickness * 0.5);
        context.DrawRectangle(null, pen, rect);
    }
}
