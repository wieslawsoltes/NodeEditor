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
    public class DrawingSelectionBehavior : Behavior<ItemsControl>
    {
        private Selection? _selection;
        private Selected? _selected;
        private bool _dragSelectedItems;
        private Point _start;
        private Rect _selectedRect;

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
            if (AssociatedObject?.DataContext is not IDrawingNode drawingNode)
            {
                return;
            }

            var position = e.GetPosition(AssociatedObject);

            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                _dragSelectedItems = false;

                if (drawingNode.SelectedNodes is { Count: > 0 })
                {
                    if (_selectedRect.Contains(position))
                    {
                        _dragSelectedItems = true;
                        _start = position;
                        e.Handled = true;
                    }
                }

                if (!_dragSelectedItems)
                {
                    if (e.Source is Control { DataContext: not IDrawingNode })
                    {
                        return;
                    }

                    drawingNode.SelectedNodes = null;

                    RemoveSelected(AssociatedObject);
                    
                    AddSelection(AssociatedObject, position.X, position.Y);
                }
            }
        }

        private void Released(object? sender, PointerReleasedEventArgs e)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode)
            {
                return;
            }

            _dragSelectedItems = false;

            if (e.Source is Control { DataContext: not IDrawingNode })
            {
                return;
            }

            if (_selection is { })
            {
                _selectedRect = GetSelectedRect(_selection.GetRect());
            }

            RemoveSelection(AssociatedObject);

            if (!_selectedRect.IsEmpty)
            {
                AddSelected(AssociatedObject, _selectedRect);
            }
        }

        private Rect GetSelectedRect(Rect rect)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode drawingNode)
            {
                return Rect.Empty;
            }

            var selectedRect = new Rect();

            drawingNode.SelectedNodes = null;
            drawingNode.SelectedNodes = new HashSet<INode>();

            foreach (var container in AssociatedObject.ItemContainerGenerator.Containers)
            {
                if (container.ContainerControl is { DataContext: INode node } containerControl)
                {
                    var bounds = containerControl.Bounds;
                    if (!rect.Intersects(bounds))
                    {
                        continue;
                    }

                    drawingNode.SelectedNodes.Add(node);
                    selectedRect = selectedRect.IsEmpty ? bounds : selectedRect.Union(bounds);
                }
            }

            return selectedRect;
        }

        private void Moved(object? sender, PointerEventArgs e)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode)
            {
                return;
            }

            var position = e.GetPosition(AssociatedObject);

            if (_dragSelectedItems)
            {
                Move(position);

                e.Handled = true;
            }
            else
            {
                if (e.Source is Control { DataContext: not IDrawingNode } control)
                {
                    return;
                }

                UpdateSelection(position.X, position.Y);
            }
        }

        private void Move(Point position)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode drawingNode)
            {
                return;
            }

            if (drawingNode.SelectedNodes is not { Count: > 0 } || drawingNode.Nodes is not { Count: > 0 })
            {
                return;
            }

            var selectedRect = new Rect();
            var deltaX = position.X - _start.X;
            var deltaY = position.Y - _start.Y;
            _start = position;

            foreach (var node in drawingNode.SelectedNodes)
            {
                var index = drawingNode.Nodes.IndexOf(node);
                var selectedControl = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(index);
                var bounds = selectedControl.Bounds;

                node.X += deltaX;
                node.Y += deltaY;
                selectedRect = selectedRect.IsEmpty ? bounds : selectedRect.Union(bounds);
            }

            _selectedRect = selectedRect;

            UpdateSelected(selectedRect);
        }

        private void AddSelection(Control control, double x, double y)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null)
            {
                return;
            }

            _selection = new Selection
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

            _selected = new Selected
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
