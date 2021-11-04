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
        private Control? _adorner;
 
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

            if (AssociatedObject.DataContext is not DrawingNodeViewModel drawingNodeViewModel)
            {
                return;
            }

            var (x, y) = e.GetPosition(AssociatedObject);

            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                AddAdorner(AssociatedObject, x, y);
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

            if (AssociatedObject.DataContext is not DrawingNodeViewModel drawingNodeViewModel)
            {
                return;
            }

            RemoveAdorner(AssociatedObject);
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

            UpdateAdorner(x, y);
        }

        private void AddAdorner(Control control, double x, double y)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null)
            {
                return;
            }

            _adorner = new Selection()
            {
                [AdornerLayer.AdornedElementProperty] = control,
                TopLeft = new Point(x, y),
                BottomRight = new Point(x, y)
            };

            ((ISetLogicalParent) _adorner).SetParent(control);
            layer.Children.Add(_adorner);
        }

        private void RemoveAdorner(Control control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null || _adorner is null)
            {
                return;
            }

            layer.Children.Remove(_adorner);
            ((ISetLogicalParent) _adorner).SetParent(null);
            _adorner = null;
        }

        private void UpdateAdorner(double x, double y)
        {
            if (_adorner is Selection selection)
            {
                selection.BottomRight = new Point(x, y);
                selection.InvalidateVisual();
            }
        }
    }
}
