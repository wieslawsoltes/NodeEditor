﻿using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingSelectionBehavior : Behavior<ItemsControl>
{
    public static readonly StyledProperty<Control?> InputSourceProperty = 
        AvaloniaProperty.Register<DrawingSelectionBehavior, Control?>(nameof(InputSource));

    public static readonly StyledProperty<Canvas?> AdornerCanvasProperty = 
        AvaloniaProperty.Register<DrawingSelectionBehavior, Canvas?>(nameof(AdornerCanvas));

    public static readonly StyledProperty<bool> EnableSnapProperty = 
        AvaloniaProperty.Register<DrawingSelectionBehavior, bool>(nameof(EnableSnap));

    public static readonly StyledProperty<double> SnapXProperty = 
        AvaloniaProperty.Register<DrawingSelectionBehavior, double>(nameof(SnapX), 1.0);

    public static readonly StyledProperty<double> SnapYProperty = 
        AvaloniaProperty.Register<DrawingSelectionBehavior, double>(nameof(SnapY), 1.0);

    private IDisposable? _isEditModeDisposable;
    private IDisposable? _dataContextDisposable;
    private INotifyPropertyChanged? _drawingNodePropertyChanged;
    private IDrawingNode? _drawingNode;
    private SelectionAdorner? _selectionAdorner;
    private SelectedAdorner? _selectedAdorner;
    private bool _dragSelectedItems;
    private Point _start;
    private Rect _selectedRect;
    private Control? _inputSource;

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

    public bool EnableSnap
    {
        get => GetValue(EnableSnapProperty);
        set => SetValue(EnableSnapProperty, value);
    }

    public double SnapX
    {
        get => GetValue(SnapXProperty);
        set => SetValue(SnapXProperty, value);
    }

    public double SnapY
    {
        get => GetValue(SnapYProperty);
        set => SetValue(SnapYProperty, value);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == InputSourceProperty)
        {
            DeInitialize();

            if (AssociatedObject is { } && InputSource is { })
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

        _inputSource.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        _inputSource.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
        _inputSource.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Tunnel);
        _inputSource.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);

        _isEditModeDisposable = AssociatedObject.GetObservable(DrawingNode.IsEditModeProperty)
            .Subscribe(x =>
            {
                if (x == false)
                {
                    RemoveSelection();
                    RemoveSelected();
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

                    RemoveSelection();
                    RemoveSelected();

                    _drawingNode = drawingNode;

                    if (_drawingNode is INotifyPropertyChanged notifyPropertyChanged)
                    {
                        _drawingNodePropertyChanged = notifyPropertyChanged;
                        _drawingNodePropertyChanged.PropertyChanged += DrawingNode_PropertyChanged;
                    }
                }
                else
                {
                    RemoveSelection();
                    RemoveSelected();
                }
            });
    }

    private void DeInitialize()
    {
        if (_inputSource is { })
        {
            _inputSource.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
            _inputSource.RemoveHandler(InputElement.PointerReleasedEvent, Released);
            _inputSource.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
            _inputSource.RemoveHandler(InputElement.PointerMovedEvent, Moved);
            _inputSource = null;
        }

        if (_drawingNodePropertyChanged is { })
        {
            _drawingNodePropertyChanged.PropertyChanged -= DrawingNode_PropertyChanged;
            _drawingNodePropertyChanged = null;
        }

        _isEditModeDisposable?.Dispose();
        _isEditModeDisposable = null;

        _dataContextDisposable?.Dispose();
        _dataContextDisposable = null;
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is { } && InputSource is { })
        {
            Initialize();
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        DeInitialize();
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
                    _selectedRect = HitTestHelper.CalculateSelectedRect(AssociatedObject);

                    if (_selectedAdorner is { })
                    {
                        RemoveSelected();
                    }

                    if (!_selectedRect.IsEmpty && _selectedAdorner is null)
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
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        var info = e.GetCurrentPoint(_inputSource);

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

        if (drawingNode.SelectedNodes is { Count: > 0 } || drawingNode.SelectedConnectors is { Count: > 0 })
        {
            if (_selectedRect.Contains(position))
            {
                _dragSelectedItems = true;
                _start = SnapHelper.Snap(position, SnapX, SnapY, EnableSnap);
            }
            else
            {
                // Add another node to the existing selection
                if (e.KeyModifiers == KeyModifiers.Shift)
                {
                    HitTestHelper.FindSelectedNodesAndAddToSelection(AssociatedObject, pointerHitTestRect);
                    
                    var selectedRect = HitTestHelper.CalculateSelectedRect(AssociatedObject);

                    _selectedRect = selectedRect;

                    UpdateSelected(selectedRect);
                }
                // Clear the selection and select another
                else
                {
                    HitTestHelper.FindSelectedNodes(AssociatedObject, pointerHitTestRect);
                }

                if (drawingNode.SelectedNodes is { Count: > 0 } || drawingNode.SelectedConnectors is { Count: > 0 })
                {
                    _dragSelectedItems = true;
                    _start = SnapHelper.Snap(position, SnapX, SnapY, EnableSnap);
                }
                else
                {
                    drawingNode.SelectedNodes = null;
                    drawingNode.SelectedConnectors = null;
                    RemoveSelected();
                }
            }
        }
        else
        {
            HitTestHelper.FindSelectedNodes(AssociatedObject, pointerHitTestRect);

            if (drawingNode.SelectedNodes is { Count: > 0 } || drawingNode.SelectedConnectors is { Count: > 0 })
            {
                _dragSelectedItems = true;
                _start = SnapHelper.Snap(position, SnapX, SnapY, EnableSnap);
            } 
        }

        if (!_dragSelectedItems)
        {
            drawingNode.SelectedNodes = null;
            drawingNode.SelectedConnectors = null;
            RemoveSelected();

            if (e.Source is not Control { DataContext: not IDrawingNode })
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
            if (e.InitialPressMouseButton == MouseButton.Left && AssociatedObject?.DataContext is IDrawingNode)
            {
                _dragSelectedItems = false;

                if (e.Source is not Control { DataContext: not IDrawingNode })
                {
                    if (_selectionAdorner is { })
                    {
                        HitTestHelper.FindSelectedNodes(AssociatedObject, _selectionAdorner.GetRect());
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

        if (Equals(e.Pointer.Captured, _inputSource) && info.Properties.IsLeftButtonPressed && AssociatedObject?.DataContext is IDrawingNode)
        {
            var position = e.GetPosition(AssociatedObject);

            if (_dragSelectedItems)
            {
                if (AssociatedObject?.DataContext is IDrawingNode drawingNode)
                {
                    if (drawingNode.SelectedNodes is { Count: > 0 } && drawingNode.Nodes is { Count: > 0 })
                    {
                        position = SnapHelper.Snap(position, SnapX, SnapY, EnableSnap);

                        var deltaX = position.X - _start.X;
                        var deltaY = position.Y - _start.Y;
                        _start = position;

                        foreach (var node in drawingNode.SelectedNodes)
                        {
                            if (node.CanMove())
                            {
                                node.Move(deltaX, deltaY);
                            }
                        }

                        var selectedRect = HitTestHelper.CalculateSelectedRect(AssociatedObject);

                        _selectedRect = selectedRect;

                        UpdateSelected(selectedRect);

                        e.Handled = true;
                    }
                }
            }
            else
            {
                if (e.Source is not Control { DataContext: not IDrawingNode })
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
