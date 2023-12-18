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
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
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
        if (e.Handled)
        {
            return;
        }

        if (AssociatedObject?.DataContext is not IPin pin)
        {
            return;
        }

        if (pin.Parent is not { } nodeViewModel)
        {
            return;
        }

        if (nodeViewModel.Parent is IDrawingNode drawingNode)
        {
            var info = e.GetCurrentPoint(AssociatedObject);

            if (info.Properties.IsLeftButtonPressed)
            {
                var showWhenMoving = info.Pointer.Type == PointerType.Mouse;

                drawingNode.ConnectorLeftPressed(pin, showWhenMoving);

                e.Handled = true;
            }
        }
    }
}
