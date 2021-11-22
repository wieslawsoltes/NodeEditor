﻿using System;
using System.ComponentModel;
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
        private IDisposable? _isEditModeDisposable;
        private IDisposable? _dataContextDisposable;
        private INotifyPropertyChanged? _drawingNodePropertyChanged;
        private IDrawingNode? _drawingNode;
        private SelectionAdorner? _selectionAdorner;
        private SelectedAdorner? _selectedAdorner;
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
                
                _isEditModeDisposable = AssociatedObject.GetObservable(DrawingNode.IsEditModeProperty)
                    .Subscribe(x =>
                    {
                        if (x == false)
                        {
                            RemoveSelection(AssociatedObject);
                            RemoveSelected(AssociatedObject);
                        }
                    });

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

                            RemoveSelection(AssociatedObject);
                            RemoveSelected(AssociatedObject);

                            _drawingNode = drawingNode;

                            if (_drawingNode is INotifyPropertyChanged notifyPropertyChanged)
                            {
                                _drawingNodePropertyChanged = notifyPropertyChanged;
                                _drawingNodePropertyChanged.PropertyChanged += DrawingNode_PropertyChanged;
                            }
                        }
                        else
                        {
                            RemoveSelection(AssociatedObject);
                            RemoveSelected(AssociatedObject);
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

                _isEditModeDisposable?.Dispose();
                _dataContextDisposable?.Dispose();
            }
        }

        private void DrawingNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (AssociatedObject?.DataContext is not IDrawingNode)
            {
                return;
            }

            if (e.PropertyName == nameof(IDrawingNode.SelectedNodes) || e.PropertyName == nameof(IDrawingNode.SelectedConnectors))
            {
                if (_drawingNode is { })
                {
                    if (_drawingNode.SelectedNodes is { Count: > 0 } || _drawingNode.SelectedConnectors is { Count: > 0 })
                    {
                        _selectedRect = HitTest.CalculateSelectedRect(AssociatedObject);

                        if (_selectedAdorner is { })
                        {
                            RemoveSelected(AssociatedObject);
                        }

                        if (!_selectedRect.IsEmpty && _selectedAdorner is null)
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

            if (e.Source is Control { DataContext: IPin })
            {
                return;
            }

            if (!AssociatedObject.GetValue(DrawingNode.IsEditModeProperty))
            {
                return;
            }

            var position = e.GetPosition(AssociatedObject);

            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                _dragSelectedItems = false;

                var pointerHitTestRect = new Rect(position.X - 1, position.Y - 1, 3, 3);

                if (drawingNode.SelectedNodes is { Count: > 0 } || drawingNode.SelectedConnectors is { Count: > 0 })
                {
                    if (_selectedRect.Contains(position))
                    {
                        _dragSelectedItems = true;
                        _start = position;
                        e.Handled = true;
                    }
                    else
                    {
                        HitTest.FindSelectedNodes(AssociatedObject, pointerHitTestRect);

                        if (drawingNode.SelectedNodes is { Count: > 0 } || drawingNode.SelectedConnectors is { Count: > 0 })
                        {
                            _dragSelectedItems = true;
                            _start = position;
                            e.Handled = true;
                        }
                        else
                        {
                            drawingNode.SelectedNodes = null;
                            drawingNode.SelectedConnectors = null;
                            RemoveSelected(AssociatedObject);
                        }
                    }
                }
                else
                {
                    HitTest.FindSelectedNodes(AssociatedObject, pointerHitTestRect);

                    if (drawingNode.SelectedNodes is { Count: > 0 } || drawingNode.SelectedConnectors is { Count: > 0 })
                    {
                        _dragSelectedItems = true;
                        _start = position;
                        e.Handled = true;
                    } 
                }

                if (!_dragSelectedItems)
                {
                    drawingNode.SelectedNodes = null;
                    drawingNode.SelectedConnectors = null;
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

            if (_selectionAdorner is { })
            {
                HitTest.FindSelectedNodes(AssociatedObject, _selectionAdorner.GetRect());
            }

            RemoveSelection(AssociatedObject);
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

            var selectedRect = HitTest.CalculateSelectedRect(AssociatedObject);

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

            _selectionAdorner = new SelectionAdorner
            {
                [AdornerLayer.AdornedElementProperty] = control,
                IsHitTestVisible = false,
                TopLeft = new Point(x, y),
                BottomRight = new Point(x, y)
            };

            ((ISetLogicalParent) _selectionAdorner).SetParent(control);
            layer.Children.Add(_selectionAdorner);
        }

        private void RemoveSelection(Control control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null || _selectionAdorner is null)
            {
                return;
            }

            layer.Children.Remove(_selectionAdorner);
            ((ISetLogicalParent) _selectionAdorner).SetParent(null);
            _selectionAdorner = null;
        }

        private void UpdateSelection(double x, double y)
        {
            if (_selectionAdorner is { } selection)
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

            _selectedAdorner = new SelectedAdorner
            {
                [AdornerLayer.AdornedElementProperty] = control,
                IsHitTestVisible = false,
                Rect = rect
            };

            ((ISetLogicalParent) _selectedAdorner).SetParent(control);
            layer.Children.Add(_selectedAdorner);
        }

        private void RemoveSelected(Control control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer is null || _selectedAdorner is null)
            {
                return;
            }

            layer.Children.Remove(_selectedAdorner);
            ((ISetLogicalParent) _selectedAdorner).SetParent(null);
            _selectedAdorner = null;
        }

        private void UpdateSelected(Rect rect)
        {
            if (_selectedAdorner is { } selected)
            {
                selected.Rect = rect;
            }
        }
    }
}
