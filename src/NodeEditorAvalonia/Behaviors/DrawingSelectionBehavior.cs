﻿using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingSelectionBehavior : Behavior<ItemsControl>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<DrawingSelectionBehavior, IDrawingNode?>(nameof(DrawingSource));

    public static readonly StyledProperty<Control?> InputSourceProperty = 
        AvaloniaProperty.Register<DrawingSelectionBehavior, Control?>(nameof(InputSource));

    public static readonly StyledProperty<Canvas?> AdornerCanvasProperty = 
        AvaloniaProperty.Register<DrawingSelectionBehavior, Canvas?>(nameof(AdornerCanvas));

    private IDisposable? _dataContextDisposable;
    private IDrawingNode? _drawingNode;
    private SelectionAdorner? _selectionAdorner;
    private SelectedAdorner? _selectedAdorner;
    private bool _dragSelectedItems;
    private Point _start;
    private Rect _selectedRect;
    private Control? _inputSource;

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    public Control? InputSource
    {
        get => GetValue(InputSourceProperty);
        set => SetValue(InputSourceProperty, value);
    }

    public Canvas? AdornerCanvas
    {
        get => GetValue(AdornerCanvasProperty);
        set => SetValue(AdornerCanvasProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == InputSourceProperty)
        {
            DeInitialize();

            if (AssociatedObject is not null && InputSource is not null)
            {
                Initialize();
            }
        }
    }

    private void Initialize()
    {
        if (AssociatedObject is null || InputSource is null)
        {
            return;
        }

        _inputSource = InputSource;

        _inputSource.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        _inputSource.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        _inputSource.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        _inputSource.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

        _dataContextDisposable = this
            .GetObservable(DrawingSourceProperty)
            .Subscribe(new AnonymousObserver<IDrawingNode?>(
                x =>
                {
                    if (x is { } drawingNode)
                    {
                        if (_drawingNode == drawingNode)
                        {
                            if (_drawingNode == drawingNode)
                            {
                                _drawingNode.SelectionChanged -= DrawingNode_SelectionChanged;
                            }
                        }

                        RemoveSelection();
                        RemoveSelected();

                        _drawingNode = drawingNode;
                        _drawingNode.SelectionChanged += DrawingNode_SelectionChanged;
                    }
                    else
                    {
                        RemoveSelection();
                        RemoveSelected();
                    }
                }));
    }

    private void DeInitialize()
    {
        if (_inputSource is not null)
        {
            _inputSource.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
            _inputSource.RemoveHandler(InputElement.PointerReleasedEvent, Released);
            _inputSource.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
            _inputSource.RemoveHandler(InputElement.PointerMovedEvent, Moved);
            _inputSource = null;
        }

        if (_drawingNode is not null)
        {
            _drawingNode.SelectionChanged -= DrawingNode_SelectionChanged;
        }

        _dataContextDisposable?.Dispose();
        _dataContextDisposable = null;
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null && InputSource is not null)
        {
            Initialize();
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        DeInitialize();
    }

    private void DrawingNode_SelectionChanged(object? sender, EventArgs e)
    {
        if (DrawingSource is null)
        {
            return;
        }
        
        if (_drawingNode is not null)
        {
            var selectedNodes = _drawingNode.GetSelectedNodes();
            var selectedConnectors = _drawingNode.GetSelectedConnectors();

            if (selectedNodes is { Count: > 0 } || selectedConnectors is { Count: > 0 })
            {
                _selectedRect = HitTestHelper.CalculateSelectedRect(_drawingNode, AssociatedObject);

                if (_selectedAdorner is not null)
                {
                    RemoveSelected();
                }

                if (!(_selectedRect == default) && _selectedAdorner is null)
                {
                    AddSelected(_selectedRect);
                }
            }
            else
            {
                RemoveSelected();
            }
        }
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        var info = e.GetCurrentPoint(_inputSource);

        if (DrawingSource is not { } drawingNode)
        {
            return;
        }

        if (e.Source is Control { DataContext: IPin })
        {
            return;
        }

        var position = e.GetPosition(AssociatedObject);

        if (!drawingNode.CanSelectNodes() && !drawingNode.CanSelectConnectors())
        {
            return;
        }

        if (!info.Properties.IsLeftButtonPressed)
        {
            return;
        }

        _dragSelectedItems = false;

        var pointerHitTestRect = new Rect(position.X - 1, position.Y - 1, 3, 3);
        var selectedNodes = drawingNode.GetSelectedNodes();
        var selectedConnectors = drawingNode.GetSelectedConnectors();

        if (selectedNodes is { Count: > 0 } || selectedConnectors is { Count: > 0 })
        {
            if (_selectedRect.Contains(position))
            {
                _dragSelectedItems = true;
                _start = SnapHelper.Snap(position, drawingNode.Settings.SnapX, drawingNode.Settings.SnapY, drawingNode.Settings.EnableSnap);
            }
            else
            {
                HitTestHelper.FindSelectedNodes(drawingNode, AssociatedObject, pointerHitTestRect);

                selectedNodes = drawingNode.GetSelectedNodes();
                selectedConnectors = drawingNode.GetSelectedConnectors();

                if (selectedNodes is { Count: > 0 } || selectedConnectors is { Count: > 0 })
                {
                    _dragSelectedItems = true;
                    _start = SnapHelper.Snap(position, drawingNode.Settings.SnapX, drawingNode.Settings.SnapY, drawingNode.Settings.EnableSnap);
                }
                else
                {
                    drawingNode.NotifyDeselectedNodes();
                    drawingNode.NotifyDeselectedConnectors();
                    drawingNode.SetSelectedNodes(null);
                    drawingNode.SetSelectedConnectors(null);
                    drawingNode.NotifySelectionChanged();

                    RemoveSelected();
                }
            }
        }
        else
        {
            HitTestHelper.FindSelectedNodes(drawingNode, AssociatedObject, pointerHitTestRect);

            selectedNodes = drawingNode.GetSelectedNodes();
            selectedConnectors = drawingNode.GetSelectedConnectors();

            if (selectedNodes is { Count: > 0 } || selectedConnectors is { Count: > 0 })
            {
                _dragSelectedItems = true;
                _start = SnapHelper.Snap(position, drawingNode.Settings.SnapX, drawingNode.Settings.SnapY, drawingNode.Settings.EnableSnap);
            } 
        }

        if (!_dragSelectedItems)
        {
            drawingNode.SetSelectedNodes(null);
            drawingNode.SetSelectedConnectors(null);
            drawingNode.NotifySelectionChanged();

            RemoveSelected();

            // TODO: if (e.Source is not Control { DataContext: not IDrawingNode })
            {
                AddSelection(position.X, position.Y);
            }
        }

        e.Pointer.Capture(_inputSource);
        e.Handled = true;
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (Equals(e.Pointer.Captured, _inputSource))
        {
            if (e.InitialPressMouseButton == MouseButton.Left && DrawingSource is IDrawingNode drawingNode)
            {
                _dragSelectedItems = false;

                // TODO: if (e.Source is not Control { DataContext: not IDrawingNode })
                {
                    if (_selectionAdorner is not null)
                    {
                        HitTestHelper.FindSelectedNodes(drawingNode, AssociatedObject, _selectionAdorner.GetRect());
                    }

                    RemoveSelection();
                }
            }

            e.Pointer.Capture(null);
        }
    }

    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        RemoveSelection();
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        var info = e.GetCurrentPoint(_inputSource);

        if (Equals(e.Pointer.Captured, _inputSource) && info.Properties.IsLeftButtonPressed && DrawingSource is IDrawingNode)
        {
            var position = e.GetPosition(AssociatedObject);

            if (_dragSelectedItems)
            {
                if (DrawingSource is IDrawingNode drawingNode)
                {
                    var selectedNodes = drawingNode.GetSelectedNodes();

                    if (selectedNodes is { Count: > 0 } && drawingNode.Nodes is { Count: > 0 })
                    {
                        position = SnapHelper.Snap(position, drawingNode.Settings.SnapX, drawingNode.Settings.SnapY, drawingNode.Settings.EnableSnap);

                        var deltaX = position.X - _start.X;
                        var deltaY = position.Y - _start.Y;
                        _start = position;

                        foreach (var node in selectedNodes)
                        {
                            if (node.CanMove())
                            {
                                node.Move(deltaX, deltaY);
                                node.OnMoved();
                            }
                        }

                        var selectedRect = HitTestHelper.CalculateSelectedRect(drawingNode, AssociatedObject);

                        _selectedRect = selectedRect;

                        UpdateSelected(selectedRect);

                        e.Handled = true;
                    }
                }
            }
            else
            {
                // TODO: if (e.Source is not Control { DataContext: not IDrawingNode })
                {
                    UpdateSelection(position.X, position.Y);

                    e.Handled = true;
                }
            }
        }
    }

    private void AddSelection(double x, double y)
    {
        var layer = AdornerCanvas;
        if (layer is null)
        {
            return;
        }

        _selectionAdorner = new SelectionAdorner
        {
            IsHitTestVisible = false,
            TopLeft = new Point(x, y),
            BottomRight = new Point(x, y)
        };

        layer.Children.Add(_selectionAdorner);

        InputSource?.InvalidateVisual();
    }

    private void RemoveSelection()
    {
        var layer = AdornerCanvas;
        if (layer is null || _selectionAdorner is null)
        {
            return;
        }

        layer.Children.Remove(_selectionAdorner);
        _selectionAdorner = null;
    }

    private void UpdateSelection(double x, double y)
    {
        var layer = AdornerCanvas;
        if (layer is null)
        {
            return;
        }

        if (_selectionAdorner is { } selection)
        {
            selection.BottomRight = new Point(x, y);
        }

        layer.InvalidateVisual();
        InputSource?.InvalidateVisual();
    }

    private void AddSelected(Rect rect)
    {
        var layer = AdornerCanvas;
        if (layer is null)
        {
            return;
        }

        _selectedAdorner = new SelectedAdorner
        {
            IsHitTestVisible = true,
            Rect = rect
        };

        layer.Children.Add(_selectedAdorner);

        layer.InvalidateVisual();
        InputSource?.InvalidateVisual();
    }

    private void RemoveSelected()
    {
        var layer = AdornerCanvas;
        if (layer is null || _selectedAdorner is null)
        {
            return;
        }

        layer.Children.Remove(_selectedAdorner);
        _selectedAdorner = null;
    }

    private void UpdateSelected(Rect rect)
    {
        var layer = AdornerCanvas;
        if (layer is null)
        {
            return;
        }

        if (_selectedAdorner is { } selected)
        {
            selected.Rect = rect;
        }

        layer.InvalidateVisual();
        InputSource?.InvalidateVisual();
    }
}
