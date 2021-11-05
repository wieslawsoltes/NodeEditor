using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors
{
    public class DrawingSelectionBehavior : Behavior<Control>
    {
        private Selection? _selection;
        private Selected? _selected;
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
  
            if (AssociatedObject.DataContext is not IDrawingNode)
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
                    if (e.Source is Control control && control.DataContext is not IDrawingNode)
                    {
                        return;
                    }

                    _selectedControls.Clear();

                    RemoveSelected(AssociatedObject);
                    
                    AddSelection(AssociatedObject, position.X, position.Y);
                }
            }
        }

        private void Released(object? sender, PointerReleasedEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }
  
            if (AssociatedObject.DataContext is not IDrawingNode)
            {
                return;
            }

            _dragSelectedItems = false;

            if (e.Source is Control control && control.DataContext is not IDrawingNode)
            {
                return;
            }

            var selectedRect = new Rect();

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

                                if (selectedRect.IsEmpty)
                                {
                                    selectedRect = bounds;
                                }
                                else
                                {
                                    selectedRect = selectedRect.Union(bounds);
                                }
                            }
                        }
                    }

                }
            }

            RemoveSelection(AssociatedObject);

            if (!selectedRect.IsEmpty)
            {
                AddSelected(AssociatedObject, selectedRect);
            }
        }

        private void Moved(object? sender, PointerEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }
  
            if (AssociatedObject.DataContext is not IDrawingNode)
            {
                return;
            }

            var position = e.GetPosition(AssociatedObject);

            if (_dragSelectedItems)
            {
                if (_selectedControls.Count > 0)
                {
                    var selectedRect = new Rect();

                    var deltaX = position.X - _start.X;
                    var deltaY = position.Y - _start.Y;
                    _start = position;

                    foreach (var selectedControl in _selectedControls)
                    {
                        if (selectedControl.DataContext is INode node)
                        {
                            var bounds = selectedControl.Bounds;

                            var x = node.X;
                            var y = node.Y;
                            node.X = x + deltaX;
                            node.Y = y + deltaY;

                            if (selectedRect.IsEmpty)
                            {
                                selectedRect = bounds;
                            }
                            else
                            {
                                selectedRect = selectedRect.Union(bounds);
                            }
                        }
                    }

                    UpdateSelected(selectedRect);
                }

                e.Handled = true;
            }
            else
            {
                if (e.Source is Control control && control.DataContext is not IDrawingNode)
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

        private void AddSelected(Control control, Rect rect)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null)
            {
                return;
            }

            _selected = new Selected()
            {
                [AdornerLayer.AdornedElementProperty] = control,
                IsHitTestVisible = false,
                Rect = rect
            };

            ((ISetLogicalParent) _selected).SetParent(control);
            layer.Children.Add(_selected);
        }

        private void RemoveSelected(Control control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null || _selected is null)
            {
                return;
            }

            layer.Children.Remove(_selected);
            ((ISetLogicalParent) _selected).SetParent(null);
            _selected = null;
        }

        private void UpdateSelected(Rect rect)
        {
            if (_selected is { } selected)
            {
                selected.Rect = rect;
            }
        }
    }
}
