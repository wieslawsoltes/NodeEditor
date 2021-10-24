using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.ViewModels;

namespace NodeEditor.Behaviors
{
    public class DrawingPressedBehavior : Behavior<Control>
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

            if (AssociatedObject.DataContext is not DrawingNodeViewModel drawingNodeViewModel)
            {
                return;
            }

            var (x, y) = e.GetPosition(AssociatedObject);

            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                drawingNodeViewModel.DrawingPressed(x, y);
            }
            else if (e.GetCurrentPoint(AssociatedObject).Properties.IsRightButtonPressed)
            {
                drawingNodeViewModel.DrawingCancel();
            }
        }
    }
}
