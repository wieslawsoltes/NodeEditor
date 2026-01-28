using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class InkStrokePresenter : Control
{
    public static readonly StyledProperty<InkStroke?> StrokeProperty =
        AvaloniaProperty.Register<InkStrokePresenter, InkStroke?>(nameof(Stroke));

    public InkStroke? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == StrokeProperty)
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Stroke is null)
        {
            return;
        }

        InkLayer.RenderStroke(context, Stroke);
    }
}
