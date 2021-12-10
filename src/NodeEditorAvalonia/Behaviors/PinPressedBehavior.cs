using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class PinPressedBehavior : Behavior<ContentPresenter>
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
        if (AssociatedObject?.DataContext is not IPin pin)
        {
            return;
        }

        if (!AssociatedObject.GetValue(DrawingNode.IsEditModeProperty))
        {
            return;
        }

        if (pin.Parent is not { } nodeViewModel)
        {
            return;
        }

        if (nodeViewModel.Parent is IDrawingNode drawingNode)
        {
            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                e.Pointer.Capture(AssociatedObject);
                drawingNode.ConnectorLeftPressed(pin);
                e.Handled = true;
            }
        }
    }
}
