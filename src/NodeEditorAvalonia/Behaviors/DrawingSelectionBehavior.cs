using System.Collections.Generic;
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
        private readonly HashSet<IControl> _selectedControls = new();
        private bool _dragSelectedItems;
        private Point _start;
 
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
  
            if (AssociatedObject.DataContext is not DrawingNodeViewModel)
            {
                return;
            }

            var position = e.GetPosition(AssociatedObject);

            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                _dragSelectedItems = false;

                if (_selectedControls.Count > 0)
                {
                    foreach (var selectedControl in _selectedControls)
                    {
                        var bounds = selectedControl.Bounds;
                        if (bounds.Contains(position))
                        {
                            _dragSelectedItems = true;
                            e.Handled = true;
                            _start = position;
                            break;
                        }
                    }
                }

                if (!_dragSelectedItems)
                {
                    if (e.Source is Control control && control.DataContext is not DrawingNodeViewModel)
                    {
                        return;
                    }

                    _selectedControls.Clear();
                    AddSelection(AssociatedObject, position.X, position.Y);
                }
            }
        }

        private void Released(object sender, PointerReleasedEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }
  
            if (AssociatedObject.DataContext is not DrawingNodeViewModel)
            {
                return;
            }

            _dragSelectedItems = false;

            if (e.Source is Control control && control.DataContext is not DrawingNodeViewModel)
            {
                return;
            }

            if (_selection is { })
            {
                var rect = _selection.GetRect();

                if (AssociatedObject is ItemsControl itemsControl)
                {
                    var itemContainerGenerator = itemsControl.ItemContainerGenerator;

                    _selectedControls.Clear();
                    
                    foreach (var container in itemContainerGenerator.Containers)
                    {
                        if (container.ContainerControl is { } containerControl)
                        {
                            var bounds = containerControl.Bounds;
                            if (rect.Intersects(bounds))
                            {
                                _selectedControls.Add(containerControl);
                            }
                        }
                    }
                }
            }

            RemoveSelection(AssociatedObject);
        }

        private void Moved(object? sender, PointerEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }
  
            if (AssociatedObject.DataContext is not DrawingNodeViewModel drawingNodeViewModel)
            {
                return;
            }

            var position = e.GetPosition(AssociatedObject);

            if (_dragSelectedItems)
            {
                if (_selectedControls.Count > 0)
                {
                    var deltaX = position.X - _start.X;
                    var deltaY = position.Y - _start.Y;
                    _start = position;

                    foreach (var selectedControl in _selectedControls)
                    {
                        if (selectedControl.DataContext is NodeViewModel nodeViewModel)
                        {
                            var x = nodeViewModel.X;
                            var y = nodeViewModel.Y;
                            nodeViewModel.X = x + deltaX;
                            nodeViewModel.Y = y + deltaY;
                        }
                    }
                }

                e.Handled = true;
            }
            else
            {
                if (e.Source is Control control && control.DataContext is not DrawingNodeViewModel)
                {
                    return;
                }

                UpdateSelection(position.X, position.Y);
            }
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
