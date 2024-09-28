using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingMovedBehavior : Behavior<ItemsControl>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<DrawingMovedBehavior, IDrawingNode?>(nameof(DrawingSource));

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is not null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
        }
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (DrawingSource is not IDrawingNode drawingNode)
        {
            return;
        }

        var (x, y) = e.GetPosition(AssociatedObject);

        var info = e.GetCurrentPoint(AssociatedObject);

        if (info.Pointer.Type == PointerType.Mouse)
        {
            drawingNode.ConnectorMove(x, y);
        }
    }
}
