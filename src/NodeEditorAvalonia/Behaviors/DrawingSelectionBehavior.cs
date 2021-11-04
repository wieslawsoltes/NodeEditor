using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.ViewModels;

namespace NodeEditor.Behaviors
{
    public class DrawingSelectionBehavior : Behavior<Control>
    {
        private Selection? _selection;
 
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is { })
            {
                AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
                AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
                AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject is { })
            {
                AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
                AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
                AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
            }
        }

        private void Pressed(object? sender, PointerPressedEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }
  
            if (e.Source is Control control && control.DataContext is not DrawingNodeViewModel)
            {
                return;
            }

            if (AssociatedObject.DataContext is not DrawingNodeViewModel)
            {
                return;
            }

            var (x, y) = e.GetPosition(AssociatedObject);

            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                AddSelection(AssociatedObject, x, y);
            }
        }
        
        private void Released(object sender, PointerReleasedEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }
  
            if (e.Source is Control control && control.DataContext is not DrawingNodeViewModel)
            {
                return;
            }

            if (AssociatedObject.DataContext is not DrawingNodeViewModel)
            {
                return;
            }

            RemoveSelection(AssociatedObject);
        }

        private void Moved(object? sender, PointerEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }
  
            if (e.Source is Control control && control.DataContext is not DrawingNodeViewModel)
            {
                return;
            }

            if (AssociatedObject.DataContext is not DrawingNodeViewModel drawingNodeViewModel)
            {
                return;
            }

            var (x, y) = e.GetPosition(AssociatedObject);

            UpdateSelection(x, y);
        }

        private void AddSelection(Control control, double x, double y)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null)
            {
                return;
            }

            _selection = new Selection()
            {
                [AdornerLayer.AdornedElementProperty] = control,
                TopLeft = new Point(x, y),
                BottomRight = new Point(x, y)
            };

            ((ISetLogicalParent) _selection).SetParent(control);
            layer.Children.Add(_selection);
        }

        private void RemoveSelection(Control control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null || _selection is null)
            {
                return;
            }

            layer.Children.Remove(_selection);
            ((ISetLogicalParent) _selection).SetParent(null);
            _selection = null;
        }

        private void UpdateSelection(double x, double y)
        {
            if (_selection is { } selection)
            {
                selection.BottomRight = new Point(x, y);
            }
        }
    }
}
