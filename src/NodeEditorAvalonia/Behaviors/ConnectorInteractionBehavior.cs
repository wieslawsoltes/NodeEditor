using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

public class ConnectorInteractionBehavior : Behavior<ItemsControl>
{
    public static readonly StyledProperty<IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.Register<ConnectorInteractionBehavior, IDrawingNode?>(nameof(DrawingSource));

    private const double HitTolerance = 8.0;
    private IConnector? _dragConnector;
    private int _dragWaypointIndex = -1;
    private bool _undoDragActive;

    public IDrawingNode? DrawingSource
    {
        get => GetValue(DrawingSourceProperty);
        set => SetValue(DrawingSourceProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Bubble);
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Bubble);
            AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Bubble);
            AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Bubble);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is not null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
            AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
            AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
        }
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject is null || DrawingSource is not IDrawingNode drawingNode)
        {
            return;
        }

        if (drawingNode.IsConnectorMoving())
        {
            return;
        }

        var info = e.GetCurrentPoint(AssociatedObject);
        if (!info.Properties.IsLeftButtonPressed)
        {
            return;
        }

        var position = e.GetPosition(AssociatedObject);

        if (!HitTestHelper.TryFindConnectorAtPoint(drawingNode, position, HitTolerance, out var connector, out var segmentIndex))
        {
            return;
        }

        if (connector is null || !connector.CanSelect())
        {
            return;
        }

        var isManual = connector.RoutingMode == ConnectorRoutingMode.Manual;

        if (connector.IsLocked)
        {
            SelectConnector(drawingNode, connector, e.KeyModifiers);
            e.Handled = true;
            return;
        }

        var waypointIndex = TryFindWaypointIndex(connector, position, HitTolerance);

        if (e.ClickCount == 2)
        {
            if (!isManual)
            {
                SelectConnector(drawingNode, connector, e.KeyModifiers);
                e.Handled = true;
                return;
            }

            var existingWaypoints = connector.Waypoints;
            if (existingWaypoints is not null && existingWaypoints.IsReadOnly)
            {
                SelectConnector(drawingNode, connector, e.KeyModifiers);
                e.Handled = true;
                return;
            }

            if (drawingNode is IUndoRedoHost host)
            {
                host.BeginUndoBatch();
            }

            try
            {
                var waypoints = EnsureWaypoints(connector);
                if (waypoints.IsReadOnly)
                {
                    SelectConnector(drawingNode, connector, e.KeyModifiers);
                    e.Handled = true;
                    return;
                }

                if (waypointIndex >= 0)
                {
                    if (waypointIndex < waypoints.Count)
                    {
                        waypoints.RemoveAt(waypointIndex);
                    }
                }
                else
                {
                    var snapped = SnapHelper.Snap(position, drawingNode.Settings.SnapX, drawingNode.Settings.SnapY, drawingNode.Settings.EnableSnap);
                    var insertIndex = waypoints.Count == 0 ? 0 : Clamp(segmentIndex, 0, waypoints.Count);
                    waypoints.Insert(insertIndex, new ConnectorPoint(snapped.X, snapped.Y));
                }
            }
            finally
            {
                if (drawingNode is IUndoRedoHost endHost)
                {
                    endHost.EndUndoBatch();
                }
            }

            if (IsConnectorSelected(drawingNode, connector))
            {
                drawingNode.NotifySelectionChanged();
            }

            e.Handled = true;
            return;
        }

        if (waypointIndex >= 0)
        {
            if (!isManual)
            {
                SelectConnector(drawingNode, connector, e.KeyModifiers);
                e.Handled = true;
                return;
            }

            StartWaypointDrag(drawingNode, connector, waypointIndex, e);
            return;
        }

        SelectConnector(drawingNode, connector, e.KeyModifiers);
        e.Handled = true;
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (_dragConnector is null || _dragWaypointIndex < 0 || DrawingSource is not IDrawingNode drawingNode)
        {
            return;
        }

        if (_dragConnector.IsLocked)
        {
            return;
        }

        var dragWaypoints = _dragConnector.Waypoints;
        if (dragWaypoints is null || _dragWaypointIndex >= dragWaypoints.Count)
        {
            return;
        }

        var position = e.GetPosition(AssociatedObject);
        var snapped = SnapHelper.Snap(position, drawingNode.Settings.SnapX, drawingNode.Settings.SnapY, drawingNode.Settings.EnableSnap);
        var waypoint = dragWaypoints[_dragWaypointIndex];
        waypoint.X = snapped.X;
        waypoint.Y = snapped.Y;

        if (IsConnectorSelected(drawingNode, _dragConnector))
        {
            drawingNode.NotifySelectionChanged();
        }

        e.Handled = true;
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (AssociatedObject is null)
        {
            return;
        }

        if (_dragConnector is not null)
        {
            _dragConnector = null;
            _dragWaypointIndex = -1;
            EndDragUndo();
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_dragConnector is null)
        {
            return;
        }

        _dragConnector = null;
        _dragWaypointIndex = -1;
        EndDragUndo();
    }

    private void StartWaypointDrag(IDrawingNode drawingNode, IConnector connector, int index, PointerPressedEventArgs e)
    {
        if (AssociatedObject is null)
        {
            return;
        }

        BeginDragUndo(drawingNode);
        _dragConnector = connector;
        _dragWaypointIndex = index;
        e.Pointer.Capture(AssociatedObject);
        e.Handled = true;
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

    private void EndDragUndo()
    {
        if (!_undoDragActive)
        {
            return;
        }

        if (DrawingSource is IUndoRedoHost host)
        {
            host.EndUndoBatch();
        }

        _undoDragActive = false;
    }

    private static int TryFindWaypointIndex(IConnector connector, Point position, double tolerance)
    {
        var waypoints = connector.Waypoints;
        if (waypoints is null || waypoints.Count == 0)
        {
            return -1;
        }

        var best = -1;
        var bestDistance = tolerance;

        for (var i = 0; i < waypoints.Count; i++)
        {
            var waypoint = waypoints[i];
            var distance = HitTestHelper.Length(new Point(waypoint.X, waypoint.Y), position);
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }

        return best;
    }

    private static IList<ConnectorPoint> EnsureWaypoints(IConnector connector)
    {
        if (connector.Waypoints is not null)
        {
            return connector.Waypoints;
        }

        var waypoints = new ObservableCollection<ConnectorPoint>();
        connector.Waypoints = waypoints;
        return waypoints;
    }

    private static void SelectConnector(IDrawingNode drawingNode, IConnector connector, KeyModifiers modifiers)
    {
        if (!drawingNode.CanSelectConnectors())
        {
            return;
        }

        var append = modifiers.HasFlag(KeyModifiers.Control)
            || modifiers.HasFlag(KeyModifiers.Meta)
            || modifiers.HasFlag(KeyModifiers.Shift);
        var selectedConnectors = drawingNode.GetSelectedConnectors();

        if (!append)
        {
            drawingNode.NotifyDeselectedNodes();
            drawingNode.NotifyDeselectedConnectors();
            drawingNode.SetSelectedNodes(null);
            selectedConnectors = new System.Collections.Generic.HashSet<IConnector>();
        }
        else if (selectedConnectors is null)
        {
            selectedConnectors = new System.Collections.Generic.HashSet<IConnector>();
        }

        if (selectedConnectors.Contains(connector))
        {
            if (append)
            {
                selectedConnectors.Remove(connector);
                connector.OnDeselected();
            }
        }
        else
        {
            selectedConnectors.Add(connector);
            connector.OnSelected();
        }

        drawingNode.SetSelectedConnectors(selectedConnectors.Count > 0 ? selectedConnectors : null);
        drawingNode.NotifySelectionChanged();
    }

    private static bool IsConnectorSelected(IDrawingNode drawingNode, IConnector connector)
    {
        var selected = drawingNode.GetSelectedConnectors();
        return selected is not null && selected.Contains(connector);
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
