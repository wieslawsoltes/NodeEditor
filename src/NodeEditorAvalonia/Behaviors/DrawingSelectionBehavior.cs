using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors
{
    public class DrawingSelectionBehavior : Behavior<ItemsControl>
    {
        private IDisposable? _dataContextDisposable;
        private INotifyPropertyChanged? _drawingNodePropertyChanged;
        private IDrawingNode? _drawingNode;
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

                _dataContextDisposable = AssociatedObject
                    .GetObservable(StyledElement.DataContextProperty)
                    .Subscribe(x =>
                    {
                        if (x is IDrawingNode drawingNode)
                        {
                            if (_drawingNode == drawingNode)
                            {
                                if (_drawingNodePropertyChanged != null)
                                {
                                    _drawingNodePropertyChanged.PropertyChanged -= DrawingNode_PropertyChanged;
                                }
                            }

                            _drawingNode = drawingNode;

                            if (_drawingNode is INotifyPropertyChanged notifyPropertyChanged)
                            {
                                _drawingNodePropertyChanged = notifyPropertyChanged;
                                _drawingNodePropertyChanged.PropertyChanged += DrawingNode_PropertyChanged;
                            }
                        }
                    });
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

                if (_drawingNodePropertyChanged is { })
                {
                    _drawingNodePropertyChanged.PropertyChanged -= DrawingNode_PropertyChanged;
                }

                _dataContextDisposable?.Dispose();
            }
        }

        private void DrawingNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode)
            {
                return;
            }

            if (e.PropertyName == nameof(IDrawingNode.SelectedNodes))
            {
                if (_drawingNode is { })
                {
                    if (_drawingNode.SelectedNodes is { Count: > 0 })
                    {
                        _selectedRect = CalculateSelectedRect();

                        if (_selected is { })
                        {
                            RemoveSelected(AssociatedObject);
                        }

                        if (!_selectedRect.IsEmpty && _selected is null)
                        {
                            AddSelected(AssociatedObject, _selectedRect);
                        }
                    }
                    else
                    {
                        RemoveSelected(AssociatedObject);
                    }
                }
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
                    else
                    {
                        drawingNode.SelectedNodes = null;
                        RemoveSelected(AssociatedObject);
                    }
                }

                if (!_dragSelectedItems)
                {
                    drawingNode.SelectedNodes = null;
                    RemoveSelected(AssociatedObject);

                    if (e.Source is Control { DataContext: not IDrawingNode })
                    {
                        return;
                    }

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
                FindSelectedNodes(_selection.GetRect());
            }

            RemoveSelection(AssociatedObject);
        }

        private void FindSelectedNodes(Rect rect)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode drawingNode)
            {
                return;
            }

            drawingNode.SelectedNodes = null;

            var selectedNodes = new HashSet<INode>();

            foreach (var container in AssociatedObject.ItemContainerGenerator.Containers)
            {
                if (container.ContainerControl is not { DataContext: INode node } containerControl)
                {
                    continue;
                }

                var bounds = containerControl.Bounds;

                if (!rect.Intersects(bounds))
                {
                    continue;
                }

                selectedNodes.Add(node);
            }

            if (selectedNodes.Count > 0)
            {
                drawingNode.SelectedNodes = selectedNodes;
            }
        }

        private void ExecuteLayoutPass()
        {
            if (AssociatedObject?.GetVisualRoot() is TopLevel topLevel)
            {
                topLevel.LayoutManager.ExecuteLayoutPass();
            }
        }

        private Rect CalculateSelectedRect()
        {
            if (AssociatedObject?.DataContext is not IDrawingNode drawingNode)
            {
                return Rect.Empty;
            }

            if (drawingNode.SelectedNodes is not { Count: > 0 } || drawingNode.Nodes is not { Count: > 0 })
            {
                return Rect.Empty;
            }

            var selectedRect = new Rect();

            ExecuteLayoutPass();

            foreach (var node in drawingNode.SelectedNodes)
            {
                var index = drawingNode.Nodes.IndexOf(node);
                var selectedControl = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(index);
                var bounds = selectedControl.Bounds;
                selectedRect = selectedRect.IsEmpty ? bounds : selectedRect.Union(bounds);
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
                if (e.Source is Control { DataContext: not IDrawingNode })
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

            var deltaX = position.X - _start.X;
            var deltaY = position.Y - _start.Y;
            _start = position;

            var selectedRect = CalculateSelectedRect();

            _selectedRect = selectedRect;

            foreach (var node in drawingNode.SelectedNodes)
            {
                node.X += deltaX;
                node.Y += deltaY;
            }

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
