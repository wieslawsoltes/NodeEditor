using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class DrawingSelectionBehavior : Behavior<ItemsControl>
{
    private const double NudgeStep = 1.0;
    private const double FastNudgeMultiplier = 10.0;

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
    private ConnectorSelectedAdorner? _connectorSelectedAdorner;
    private GuidesAdorner? _guidesAdorner;
    private bool _dragSelectedItems;
    private bool _undoDragActive;
    private Point _start;
    private Rect _selectedRect;
    private Control? _inputSource;
    private HitTestHelper.SelectionMode _selectionMode = HitTestHelper.SelectionMode.Replace;

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
        _inputSource.AddHandler(InputElement.KeyDownEvent, KeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

        _dataContextDisposable = this
            .GetObservable(DrawingSourceProperty)
            .Subscribe(new AnonymousObserver<IDrawingNode?>(
                drawingNode =>
                {
                    if (_drawingNode is not null)
                    {
                        _drawingNode.SelectionChanged -= DrawingNode_SelectionChanged;
                    }

                    RemoveSelection();
                    RemoveSelected();

                    _drawingNode = drawingNode;

                    if (_drawingNode is not null)
                    {
                        _drawingNode.SelectionChanged += DrawingNode_SelectionChanged;
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
            _inputSource.RemoveHandler(InputElement.KeyDownEvent, KeyDown);
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

            if (selectedNodes is { Count: > 0 })
            {
                _selectedRect = HitTestHelper.CalculateSelectedRect(_drawingNode, AssociatedObject);

                if (_selectedAdorner is not null)
                {
                    UpdateSelected(_selectedRect);
                }
                else if (!(_selectedRect == default))
                {
                    AddSelected(_selectedRect);
                }
            }
            else
            {
                RemoveSelected();
            }

            UpdateSelectedConnectors(selectedConnectors);
        }
    }

    private void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (DrawingSource is not IDrawingNode drawingNode)
        {
            return;
        }

        if (drawingNode.IsConnectorMoving() || ShouldIgnoreSelection(drawingNode))
        {
            return;
        }

        if (IsInteractiveSource(e.Source))
        {
            return;
        }

        if (!TryGetNudgeDelta(drawingNode.Settings, e, out var delta))
        {
            return;
        }

        var selectedNodes = drawingNode.GetSelectedNodes();
        if (selectedNodes is not { Count: > 0 })
        {
            return;
        }

        if (drawingNode is IUndoRedoHost undoHost)
        {
            undoHost.BeginUndoBatch();
            try
            {
                ApplyMoveDelta(drawingNode, selectedNodes, delta.X, delta.Y);
            }
            finally
            {
                undoHost.EndUndoBatch();
            }
        }
        else
        {
            ApplyMoveDelta(drawingNode, selectedNodes, delta.X, delta.Y);
        }

        e.Handled = true;
    }

    private static bool ShouldIgnoreSelection(IDrawingNode drawingNode)
    {
        return drawingNode.Settings.EnableInk && drawingNode.Settings.IsInkMode;
    }

    private void ApplyMoveDelta(IDrawingNode drawingNode, ISet<INode> selectedNodes, double deltaX, double deltaY)
    {
        if (Math.Abs(deltaX) < 0.001 && Math.Abs(deltaY) < 0.001)
        {
            return;
        }

        foreach (var node in selectedNodes)
        {
            if (node.CanMove())
            {
                node.Move(deltaX, deltaY);
                node.OnMoved();
            }
        }

        if (drawingNode.Connectors is { Count: > 0 })
        {
            foreach (var connector in drawingNode.Connectors)
            {
                var startNode = connector.Start?.Parent;
                var endNode = connector.End?.Parent;

                if (startNode is null || endNode is null)
                {
                    continue;
                }

                if (!selectedNodes.Contains(startNode) || !selectedNodes.Contains(endNode))
                {
                    continue;
                }

                if (connector.Waypoints is { Count: > 0 })
                {
                    foreach (var waypoint in connector.Waypoints)
                    {
                        waypoint.X += deltaX;
                        waypoint.Y += deltaY;
                    }
                }
            }
        }

        if (AssociatedObject is not null)
        {
            _selectedRect = HitTestHelper.CalculateSelectedRect(drawingNode, AssociatedObject);
            UpdateSelected(_selectedRect);
        }

        UpdateSelectedConnectors(drawingNode.GetSelectedConnectors());
    }

    private static bool TryGetNudgeDelta(IDrawingNodeSettings settings, KeyEventArgs e, out Vector delta)
    {
        delta = default;

        var isFast = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var baseStep = settings.NudgeStep;
        if (baseStep <= 0.0)
        {
            baseStep = NudgeStep;
        }

        var multiplier = settings.NudgeMultiplier;
        if (multiplier <= 0.0)
        {
            multiplier = FastNudgeMultiplier;
        }

        var stepX = GetNudgeStep(settings.EnableSnap ? settings.SnapX : 0.0, baseStep, multiplier, isFast);
        var stepY = GetNudgeStep(settings.EnableSnap ? settings.SnapY : 0.0, baseStep, multiplier, isFast);

        switch (e.Key)
        {
            case Key.Left:
                delta = new Vector(-stepX, 0.0);
                return true;
            case Key.Right:
                delta = new Vector(stepX, 0.0);
                return true;
            case Key.Up:
                delta = new Vector(0.0, -stepY);
                return true;
            case Key.Down:
                delta = new Vector(0.0, stepY);
                return true;
        }

        return false;
    }

    private static double GetNudgeStep(double snapValue, double baseStep, double multiplier, bool isFast)
    {
        var step = snapValue > 0.0 ? snapValue : baseStep;
        if (isFast)
        {
            step *= multiplier;
        }

        return step;
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

        if (ShouldIgnoreSelection(drawingNode))
        {
            return;
        }

        if (IsPinSource(e.Source) || IsConnectorSource(e.Source))
        {
            return;
        }

        if (e.ClickCount > 1 && IsEditableTextBlockSource(e.Source))
        {
            return;
        }

        if (IsInteractiveSource(e.Source))
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
        _undoDragActive = false;

        var pointerHitTestRect = new Rect(position.X - 1, position.Y - 1, 3, 3);
        var selectionMode = GetSelectionMode(e.KeyModifiers);
        var canDrag = selectionMode == HitTestHelper.SelectionMode.Replace;
        var selectedNodes = drawingNode.GetSelectedNodes();
        var selectedConnectors = drawingNode.GetSelectedConnectors();
        HitTestHelper.GetHitElements(drawingNode, AssociatedObject, pointerHitTestRect, out var hitNodes, out var hitConnectors);
        var hasHits = hitNodes.Count > 0 || hitConnectors.Count > 0;
        var clickedOnSelectedNode = selectedNodes is { Count: > 0 } && hitNodes.Overlaps(selectedNodes);

        if (clickedOnSelectedNode && canDrag)
        {
            _dragSelectedItems = true;
            _start = SnapHelper.Snap(position, drawingNode.Settings.SnapX, drawingNode.Settings.SnapY, drawingNode.Settings.EnableSnap);
            e.Pointer.Capture(_inputSource);
            e.Handled = true;
            return;
        }

        if (hasHits)
        {
            HitTestHelper.ApplySelection(drawingNode, hitNodes, hitConnectors, selectionMode);

            if (canDrag && hitNodes.Count > 0)
            {
                _dragSelectedItems = true;
                _start = SnapHelper.Snap(position, drawingNode.Settings.SnapX, drawingNode.Settings.SnapY, drawingNode.Settings.EnableSnap);
                e.Pointer.Capture(_inputSource);
                e.Handled = true;
                return;
            }

            e.Handled = true;
            return;
        }

        _selectionMode = selectionMode;

        if (selectionMode == HitTestHelper.SelectionMode.Replace)
        {
            HitTestHelper.ApplySelection(drawingNode, new HashSet<INode>(), new HashSet<IConnector>(), selectionMode);
        }

        {
            AddSelection(position.X, position.Y);
        }

        e.Pointer.Capture(_inputSource);
        e.Handled = true;
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (DrawingSource is not IDrawingNode drawingNode)
        {
            return;
        }

        if (ShouldIgnoreSelection(drawingNode))
        {
            return;
        }

        if (Equals(e.Pointer.Captured, _inputSource))
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                _dragSelectedItems = false;
                EndDragUndo(drawingNode);

                if (_selectionAdorner is not null)
                {
                    HitTestHelper.GetHitElements(drawingNode, AssociatedObject, _selectionAdorner.GetRect(), out var hitNodes, out var hitConnectors);
                    HitTestHelper.ApplySelection(drawingNode, hitNodes, hitConnectors, _selectionMode);
                }

                RemoveSelection();
                RemoveGuides();
                _selectionMode = HitTestHelper.SelectionMode.Replace;
            }

            e.Pointer.Capture(null);
        }
    }

    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        RemoveSelection();
        RemoveGuides();
        _selectionMode = HitTestHelper.SelectionMode.Replace;
        if (_drawingNode is not null)
        {
            EndDragUndo(_drawingNode);
        }
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        var info = e.GetCurrentPoint(_inputSource);

        if (DrawingSource is not IDrawingNode drawingNode)
        {
            return;
        }

        if (ShouldIgnoreSelection(drawingNode))
        {
            return;
        }

        if (Equals(e.Pointer.Captured, _inputSource) && info.Properties.IsLeftButtonPressed)
        {
            var position = e.GetPosition(AssociatedObject);

            if (_dragSelectedItems)
            {
                var selectedNodes = drawingNode.GetSelectedNodes();

                if (selectedNodes is { Count: > 0 } && drawingNode.Nodes is { Count: > 0 })
                {
                    position = SnapHelper.Snap(position, drawingNode.Settings.SnapX, drawingNode.Settings.SnapY, drawingNode.Settings.EnableSnap);

                    var deltaX = position.X - _start.X;
                    var deltaY = position.Y - _start.Y;
                    _start = position;

                    if (!_undoDragActive && (Math.Abs(deltaX) > 0.001 || Math.Abs(deltaY) > 0.001))
                    {
                        BeginDragUndo(drawingNode);
                    }

                    if (drawingNode.Settings.EnableGuides
                        && drawingNode.Settings.GuideSnapTolerance > 0.0
                        && TryGetSelectedNodesBounds(selectedNodes, out var selectionBounds))
                    {
                        var tentativeBounds = new Rect(
                            selectionBounds.X + deltaX,
                            selectionBounds.Y + deltaY,
                            selectionBounds.Width,
                            selectionBounds.Height);
                        var guideSnap = CalculateGuideSnap(drawingNode, selectedNodes, tentativeBounds, out var guides);

                        if (Math.Abs(guideSnap.X) > 0.001 || Math.Abs(guideSnap.Y) > 0.001)
                        {
                            deltaX += guideSnap.X;
                            deltaY += guideSnap.Y;
                        }

                        UpdateGuides(guides);
                    }
                    else
                    {
                        RemoveGuides();
                    }

                    foreach (var node in selectedNodes)
                    {
                        if (node.CanMove())
                        {
                            node.Move(deltaX, deltaY);
                            node.OnMoved();
                        }
                    }

                    if (drawingNode.Connectors is { Count: > 0 } && (Math.Abs(deltaX) > 0.001 || Math.Abs(deltaY) > 0.001))
                    {
                        foreach (var connector in drawingNode.Connectors)
                        {
                            var startNode = connector.Start?.Parent;
                            var endNode = connector.End?.Parent;

                            if (startNode is null || endNode is null)
                            {
                                continue;
                            }

                            if (!selectedNodes.Contains(startNode) || !selectedNodes.Contains(endNode))
                            {
                                continue;
                            }

                            if (connector.Waypoints is { Count: > 0 })
                            {
                                foreach (var waypoint in connector.Waypoints)
                                {
                                    waypoint.X += deltaX;
                                    waypoint.Y += deltaY;
                                }
                            }
                        }
                    }

                    var selectedRect = HitTestHelper.CalculateSelectedRect(drawingNode, AssociatedObject);

                    _selectedRect = selectedRect;

                    UpdateSelected(selectedRect);
                    UpdateSelectedConnectors(drawingNode.GetSelectedConnectors());

                    e.Handled = true;
                }
            }
            else
            {
                UpdateSelection(position.X, position.Y);
                e.Handled = true;
            }
        }
    }

    private static bool TryGetSelectedNodesBounds(ISet<INode> selectedNodes, out Rect bounds)
    {
        bounds = default;
        var hasBounds = false;

        foreach (var node in selectedNodes)
        {
            var rect = HitTestHelper.GetNodeBounds(node);
            if (!hasBounds)
            {
                bounds = rect;
                hasBounds = true;
                continue;
            }

            bounds = bounds.Union(rect);
        }

        return hasBounds;
    }

    private static Vector CalculateGuideSnap(
        IDrawingNode drawingNode,
        ISet<INode> selectedNodes,
        Rect selectionBounds,
        out List<GuideLine> guides)
    {
        guides = new List<GuideLine>();

        if (drawingNode.Nodes is not { Count: > 0 })
        {
            return default;
        }

        var tolerance = drawingNode.Settings.GuideSnapTolerance;
        if (tolerance <= 0.0)
        {
            return default;
        }

        var selectionCandidatesX = new[]
        {
            selectionBounds.Left,
            selectionBounds.Center.X,
            selectionBounds.Right
        };

        var selectionCandidatesY = new[]
        {
            selectionBounds.Top,
            selectionBounds.Center.Y,
            selectionBounds.Bottom
        };

        var bestXDiff = double.PositiveInfinity;
        var bestYDiff = double.PositiveInfinity;
        GuideLine? bestXGuide = null;
        GuideLine? bestYGuide = null;

        foreach (var node in drawingNode.Nodes)
        {
            if (selectedNodes.Contains(node))
            {
                continue;
            }

            if (!node.CanSelect())
            {
                continue;
            }

            var otherBounds = new Rect(node.X, node.Y, node.Width, node.Height);
            var otherCandidatesX = new[]
            {
                otherBounds.Left,
                otherBounds.Center.X,
                otherBounds.Right
            };
            var otherCandidatesY = new[]
            {
                otherBounds.Top,
                otherBounds.Center.Y,
                otherBounds.Bottom
            };

            foreach (var candidateX in otherCandidatesX)
            {
                foreach (var selectedX in selectionCandidatesX)
                {
                    var diff = candidateX - selectedX;
                    var abs = Math.Abs(diff);
                    if (abs > tolerance || abs >= bestXDiff)
                    {
                        continue;
                    }

                    var top = Math.Min(selectionBounds.Top, otherBounds.Top);
                    var bottom = Math.Max(selectionBounds.Bottom, otherBounds.Bottom);
                    bestXDiff = abs;
                    bestXGuide = new GuideLine(new Point(candidateX, top), new Point(candidateX, bottom));
                }
            }

            foreach (var candidateY in otherCandidatesY)
            {
                foreach (var selectedY in selectionCandidatesY)
                {
                    var diff = candidateY - selectedY;
                    var abs = Math.Abs(diff);
                    if (abs > tolerance || abs >= bestYDiff)
                    {
                        continue;
                    }

                    var left = Math.Min(selectionBounds.Left, otherBounds.Left);
                    var right = Math.Max(selectionBounds.Right, otherBounds.Right);
                    bestYDiff = abs;
                    bestYGuide = new GuideLine(new Point(left, candidateY), new Point(right, candidateY));
                }
            }
        }

        var snapX = 0.0;
        var snapY = 0.0;

        if (bestXGuide.HasValue)
        {
            var selectedX = selectionCandidatesX[0];
            var candidateX = bestXGuide.Value.Start.X;

            foreach (var candidate in selectionCandidatesX)
            {
                if (Math.Abs(candidateX - candidate) < Math.Abs(candidateX - selectedX))
                {
                    selectedX = candidate;
                }
            }

            snapX = candidateX - selectedX;
            guides.Add(bestXGuide.Value);
        }

        if (bestYGuide.HasValue)
        {
            var selectedY = selectionCandidatesY[0];
            var candidateY = bestYGuide.Value.Start.Y;

            foreach (var candidate in selectionCandidatesY)
            {
                if (Math.Abs(candidateY - candidate) < Math.Abs(candidateY - selectedY))
                {
                    selectedY = candidate;
                }
            }

            snapY = candidateY - selectedY;
            guides.Add(bestYGuide.Value);
        }

        return new Vector(snapX, snapY);
    }

    private void BeginDragUndo(IDrawingNode drawingNode)
    {
        if (_undoDragActive)
        {
            return;
        }

        if (drawingNode is IUndoRedoHost host)
        {
            host.BeginUndoBatch();
            _undoDragActive = true;
        }
    }

    private void EndDragUndo(IDrawingNode drawingNode)
    {
        if (!_undoDragActive)
        {
            return;
        }

        if (drawingNode is IUndoRedoHost host)
        {
            host.EndUndoBatch();
        }

        _undoDragActive = false;
    }

    private void UpdateGuides(IReadOnlyList<GuideLine> guides)
    {
        var layer = AdornerCanvas;
        if (layer is null)
        {
            return;
        }

        if (guides.Count == 0)
        {
            RemoveGuides();
            return;
        }

        if (_guidesAdorner is null)
        {
            _guidesAdorner = new GuidesAdorner
            {
                IsHitTestVisible = false
            };
            layer.Children.Add(_guidesAdorner);
        }

        _guidesAdorner.Guides = guides;
        layer.InvalidateVisual();
    }

    private void RemoveGuides()
    {
        var layer = AdornerCanvas;
        if (layer is null || _guidesAdorner is null)
        {
            return;
        }

        layer.Children.Remove(_guidesAdorner);
        _guidesAdorner = null;
    }

    private static HitTestHelper.SelectionMode GetSelectionMode(KeyModifiers modifiers)
    {
        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            return HitTestHelper.SelectionMode.Add;
        }

        if (modifiers.HasFlag(KeyModifiers.Control) || modifiers.HasFlag(KeyModifiers.Meta))
        {
            return HitTestHelper.SelectionMode.Toggle;
        }

        return HitTestHelper.SelectionMode.Replace;
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

    private void UpdateSelectedConnectors(ISet<IConnector>? connectors)
    {
        if (connectors is not { Count: > 0 })
        {
            RemoveConnectorSelected();
            return;
        }

        var layer = AdornerCanvas;
        if (layer is null)
        {
            return;
        }

        if (_connectorSelectedAdorner is null)
        {
            _connectorSelectedAdorner = new ConnectorSelectedAdorner
            {
                IsHitTestVisible = false
            };
            layer.Children.Add(_connectorSelectedAdorner);
        }

        _connectorSelectedAdorner.Connectors = new List<IConnector>(connectors);
        layer.InvalidateVisual();
        InputSource?.InvalidateVisual();
    }

    private void RemoveConnectorSelected()
    {
        var layer = AdornerCanvas;
        if (layer is null || _connectorSelectedAdorner is null)
        {
            return;
        }

        layer.Children.Remove(_connectorSelectedAdorner);
        _connectorSelectedAdorner = null;
    }

    private static bool IsPinSource(object? source)
    {
        if (source is not Visual visual)
        {
            return false;
        }

        if (visual is Pin)
        {
            return true;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ancestor is Pin)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsConnectorSource(object? source)
    {
        if (source is not Visual visual)
        {
            return false;
        }

        if (visual is Connector)
        {
            return true;
        }

        if (visual is Control { DataContext: IConnector or ConnectorPoint })
        {
            return true;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ancestor is Connector)
            {
                return true;
            }

            if (ancestor is Control { DataContext: IConnector or ConnectorPoint })
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInteractiveSource(object? source)
    {
        if (source is not Visual visual)
        {
            return false;
        }

        if (visual is Control control && IsInteractiveControl(control, includeFocusable: true))
        {
            return true;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ancestor is Control ancestorControl && IsInteractiveControl(ancestorControl, includeFocusable: false))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInteractiveControl(Control control, bool includeFocusable)
    {
        if (control is Node or DrawingNode or Pin or Connector)
        {
            return false;
        }

        if (control is EditableTextBlock editableText)
        {
            return editableText.IsEditing;
        }

        if (control is Thumb
            || control is TextBox
            || control is ToggleButton
            || control is Button
            || control is ComboBox
            || control is Slider
            || control is SelectingItemsControl)
        {
            return true;
        }

        return includeFocusable && control.Focusable;
    }

    private static bool IsEditableTextBlockSource(object? source)
    {
        if (source is not Visual visual)
        {
            return false;
        }

        if (visual is EditableTextBlock)
        {
            return true;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ancestor is EditableTextBlock)
            {
                return true;
            }
        }

        return false;
    }
}
