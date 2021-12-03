using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingPressedBehavior : Behavior<ItemsControl>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is { })
        {
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is { })
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
        }
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject?.DataContext is not IDrawingNode drawingNode)
        {
            return;
        }

        var (x, y) = e.GetPosition(AssociatedObject);

        if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
        {
            drawingNode.DrawingLeftPressed(x, y);
        }
        else if (e.GetCurrentPoint(AssociatedObject).Properties.IsRightButtonPressed)
        {
            drawingNode.DrawingRightPressed(x, y);
        }
    }
}