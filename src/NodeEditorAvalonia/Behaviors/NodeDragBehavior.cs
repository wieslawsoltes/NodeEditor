using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.ViewModels;

namespace NodeEditor.Behaviors
{
    public class NodeDragBehavior : Behavior<Control>
    {
        private bool _enableDrag;
        private bool _moved;
        private Point _start;
        private IControl? _parent;
        private Control? _draggedContainer;

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is { })
            {
                AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
                AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
                AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
                AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Tunnel);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject is { })
            {
                AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
                AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
                AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
                AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
            }
        }

        private void Pressed(object? sender, PointerPressedEventArgs e)
        {
            if (AssociatedObject?.Parent is not { } parent)
            {
                return;
            }
  
            if (e.Source is Control control && control.DataContext is PinViewModel)
            {
                return;
            }

            if (!e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                return;
            }

            _enableDrag = true;
            _moved = false;
            _start = e.GetPosition(AssociatedObject.Parent);
            _parent = parent;
            _draggedContainer = AssociatedObject;
        }

        private void Released(object? sender, PointerReleasedEventArgs e)
        {
            e.Handled = _moved && _enableDrag;
            Released();
        }

        private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            Released();
        }

        private void Moved(object? sender, PointerEventArgs e)
        {
            if (_parent is null || _draggedContainer is null || !_enableDrag)
            {
                return;
            }

            if (e.Source is Control control && control.DataContext is PinViewModel)
            {
                return;
            }
            
            if (_draggedContainer.DataContext is not NodeViewModel nodeViewModel)
            {
                return;
            }

            var position = e.GetPosition(_parent);
            var deltaX = position.X - _start.X;
            var deltaY = position.Y - _start.Y;
            _moved = true;
            _start = position;
            var x = nodeViewModel.X;
            var y = nodeViewModel.Y;
            nodeViewModel.X = x + deltaX;
            nodeViewModel.Y = y + deltaY;
        }

        private void Released()
        {
            if (_enableDrag)
            {
                _enableDrag = false;
                _parent = null;
                _draggedContainer = null;
            }
        }
    }
}
