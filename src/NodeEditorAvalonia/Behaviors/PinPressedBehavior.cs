using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors
{
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
            if (AssociatedObject is null)
            {
                return;
            }

            if (AssociatedObject.DataContext is not IPin pin)
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
                    drawingNode.ConnectorPressed(pin);
                    e.Handled = true;
                }
            }
        }
    }
}
