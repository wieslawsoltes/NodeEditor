using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors
{
    public class NodeDragBehavior : Behavior<ContentPresenter>
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
  
            if (e.Source is Control { DataContext: IPin })
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

            if (e.Source is Control { DataContext: IPin })
            {
                return;
            }
            
            if (_draggedContainer.DataContext is not INode node)
            {
                return;
            }

            var position = e.GetPosition(_parent);
            var deltaX = position.X - _start.X;
            var deltaY = position.Y - _start.Y;
            node.X += deltaX;
            node.Y += deltaY;
            _moved = true;
            _start = position;
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
