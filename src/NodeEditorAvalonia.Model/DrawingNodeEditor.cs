using System;
using System.Collections.Generic;

namespace NodeEditor.Model;

public sealed class DrawingNodeEditor
{
    private readonly IDrawingNode _node;
    private readonly IDrawingNodeFactory _factory;
    private IConnector? _connector;
    private string? _clipboard;
    private double _pressedX = double.NaN;
    private double _pressedY = double.NaN;

    private class Clipboard
    {
        public ISet<INode>? SelectedNodes { get; set; }
        public ISet<IConnector>? SelectedConnectors { get; set; }
    }

    public DrawingNodeEditor(IDrawingNode node, IDrawingNodeFactory factory)
    {
        _node = node;
        _factory = factory;
    }

    public T? Clone<T>(T source)
    {
        var serialize = _node.GetSerializer();
        if (serialize is null)
        {
            return default;
        }

        var text = serialize.Serialize(source);

        return serialize.Deserialize<T>(text);
    }

    public bool IsPinConnected(IPin pin)
    {
        if (_node.Connectors is not null)
        {
            foreach (var connector in _node.Connectors)
            {
                if (connector.Start == pin || connector.End == pin)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsConnectorMoving()
    {
        if (_connector is not null)
        {
            return true;
        }

        return false;
    }

    public void CancelConnector()
    {
        if (_connector is null)
        {
            return;
        }

        var start = _connector.Start;
        var end = _connector.End;

        if (_node.Connectors is not null)
        {
            _node.Connectors.Remove(_connector);
        }

        start?.OnDisconnected();
        end?.OnDisconnected();
        _connector.OnRemoved();
        _connector = null;
    }

    public bool CanSelectNodes()
    {
        if (_connector is not null)
        {
            return false;
        }

        return true;
    }

    public bool CanSelectConnectors()
    {
        if (_connector is not null)
        {
            return false;
        }

        return true;
    }

    public bool CanConnectPin(IPin pin)
    {
        if (!_node.Settings.EnableConnections)
        {
            return false;
        }

        if (!_node.Settings.EnableMultiplePinConnections)
        {
            if (IsPinConnected(pin))
            {
                return false;
            }
        }

        return true;
    }

    private static PinDirection GetDirection(IPin pin)
    {
        if (pin is IConnectablePin connectable)
        {
            return connectable.Direction;
        }

        return PinDirection.Bidirectional;
    }

    private static int GetBusWidth(IPin pin)
    {
        if (pin is IConnectablePin connectable)
        {
            return Math.Max(1, connectable.BusWidth);
        }

        return 1;
    }

    private bool TryNormalizePins(IPin start, IPin end, out IPin normalizedStart, out IPin normalizedEnd)
    {
        normalizedStart = start;
        normalizedEnd = end;

        if (_node.Settings.RequireDirectionalConnections)
        {
            var startDir = GetDirection(start);
            var endDir = GetDirection(end);

            if (startDir == PinDirection.Output && endDir == PinDirection.Input)
            {
                // Keep order.
            }
            else if (startDir == PinDirection.Input && endDir == PinDirection.Output)
            {
                normalizedStart = end;
                normalizedEnd = start;
            }
            else if (startDir == PinDirection.Bidirectional || endDir == PinDirection.Bidirectional)
            {
                // Allow bidirectional pins without swapping.
            }
            else
            {
                return false;
            }
        }

        if (_node.Settings.RequireMatchingBusWidth)
        {
            var startWidth = GetBusWidth(normalizedStart);
            var endWidth = GetBusWidth(normalizedEnd);

            if (startWidth != endWidth)
            {
                return false;
            }
        }

        return true;
    }

    private bool CanConnectPins(IPin start, IPin end)
    {
        if (!_node.Settings.AllowSelfConnections)
        {
            var startParent = start.Parent;
            var endParent = end.Parent;
            if (startParent is not null && endParent is not null && ReferenceEquals(startParent, endParent))
            {
                return false;
            }
        }

        if (!_node.Settings.AllowDuplicateConnections)
        {
            if (HasConnectorBetweenPins(start, end, _connector))
            {
                return false;
            }
        }

        var validator = _node.Settings.ConnectionValidator;
        if (validator is not null)
        {
            var context = new ConnectionValidationContext(_node, start, end);
            if (!validator(context))
            {
                return false;
            }
        }

        return true;
    }

    private bool HasConnectorBetweenPins(IPin start, IPin end, IConnector? ignore)
    {
        if (_node.Connectors is not { Count: > 0 })
        {
            return false;
        }

        foreach (var connector in _node.Connectors)
        {
            if (ReferenceEquals(connector, ignore))
            {
                continue;
            }

            var existingStart = connector.Start;
            var existingEnd = connector.End;
            if (existingStart is null || existingEnd is null)
            {
                continue;
            }

            if ((ReferenceEquals(existingStart, start) && ReferenceEquals(existingEnd, end))
                || (ReferenceEquals(existingStart, end) && ReferenceEquals(existingEnd, start)))
            {
                return true;
            }
        }

        return false;
    }

    private void NotifyPinsRemoved(INode node)
    {
        if (node.Pins is not null)
        {
            foreach (var pin in node.Pins)
            {
                pin.OnRemoved();
            }
        }
    }

    private static bool TryGetConnectorNodes(IConnector connector, out INode? startNode, out INode? endNode)
    {
        startNode = connector.Start?.Parent;
        endNode = connector.End?.Parent;
        return startNode is not null && endNode is not null;
    }

    private HashSet<IConnector> CollectConnectorsBetweenNodes(ISet<INode> nodes)
    {
        var connectors = new HashSet<IConnector>();

        if (_node.Connectors is not { Count: > 0 })
        {
            return connectors;
        }

        foreach (var connector in _node.Connectors)
        {
            if (TryGetConnectorNodes(connector, out var startNode, out var endNode)
                && nodes.Contains(startNode!)
                && nodes.Contains(endNode!))
            {
                connectors.Add(connector);
            }
        }

        return connectors;
    }

    private HashSet<IConnector> CollectConnectorsAttachedToNodes(ISet<INode> nodes)
    {
        var connectors = new HashSet<IConnector>();

        if (_node.Connectors is not { Count: > 0 })
        {
            return connectors;
        }

        foreach (var connector in _node.Connectors)
        {
            var startNode = connector.Start?.Parent;
            var endNode = connector.End?.Parent;

            if (startNode is not null && nodes.Contains(startNode))
            {
                connectors.Add(connector);
                continue;
            }

            if (endNode is not null && nodes.Contains(endNode))
            {
                connectors.Add(connector);
            }
        }

        return connectors;
    }

    private void RemoveConnector(IConnector connector)
    {
        _node.Connectors?.Remove(connector);
        connector.Start?.OnDisconnected();
        connector.End?.OnDisconnected();
        connector.OnRemoved();
    }

    private static bool TryGetNodesBounds(ISet<INode> nodes, out double left, out double top, out double right, out double bottom)
    {
        left = 0.0;
        top = 0.0;
        right = 0.0;
        bottom = 0.0;
        var hasBounds = false;

        foreach (var node in nodes)
        {
            var nodeLeft = node.X;
            var nodeTop = node.Y;
            var nodeRight = node.X + node.Width;
            var nodeBottom = node.Y + node.Height;

            if (!hasBounds)
            {
                left = nodeLeft;
                top = nodeTop;
                right = nodeRight;
                bottom = nodeBottom;
                hasBounds = true;
                continue;
            }

            left = Math.Min(left, nodeLeft);
            top = Math.Min(top, nodeTop);
            right = Math.Max(right, nodeRight);
            bottom = Math.Max(bottom, nodeBottom);
        }

        return hasBounds;
    }

    public void DrawingLeftPressed(double x, double y)
    {
        if (IsConnectorMoving())
        {
            CancelConnector();
        }
    }

    public void DrawingRightPressed(double x, double y)
    {
        _pressedX = x;
        _pressedY = y;

        if (IsConnectorMoving())
        {
            CancelConnector();
        }
    }

    public void ConnectorLeftPressed(IPin pin, bool showWhenMoving)
    {
        if (!CanConnectPin(pin) || !pin.CanConnect())
        {
            return;
        }

        if (_connector is null)
        {
            if (_node.Settings.RequireDirectionalConnections && GetDirection(pin) == PinDirection.Input)
            {
                // Allow starting from inputs, but validate when the connection is completed.
            }

            var x = pin.X;
            var y = pin.Y;

            if (pin.Parent is not null)
            {
                x += pin.Parent.X;
                y += pin.Parent.Y;
            }

            var end = _factory.CreatePin();
            end.Parent = null;
            end.X = x;
            end.Y = y;
            end.Width = pin.Width;
            end.Height = pin.Height;
            end.OnCreated();

            var connector = _factory.CreateConnector();
            connector.Parent = _node;
            connector.Style = _node.Settings.DefaultConnectorStyle;
            connector.Start = pin;
            connector.End = end;
            pin.OnConnected();
            end.OnConnected();
            connector.OnCreated();

            if (showWhenMoving)
            {
                _node.Connectors ??= _factory.CreateList<IConnector>();
                _node.Connectors.Add(connector);
            }

            _connector = connector;
        }
        else
        {
            if (_connector.Start != pin)
            {
                if (!CanConnectPin(pin) || !pin.CanConnect())
                {
                    var startPin = _connector.Start;
                    if (startPin is not null)
                    {
                        _node.NotifyConnectionRejected(startPin, pin);
                    }
                    CancelConnector();
                    return;
                }

                var previousEnd = _connector.End;
                var start = _connector.Start;
                if (start is null)
                {
                    CancelConnector();
                    return;
                }

                var normalizedStart = start;
                var normalizedEnd = pin;

                if (_node.Settings.RequireDirectionalConnections || _node.Settings.RequireMatchingBusWidth)
                {
                    if (!TryNormalizePins(start, pin, out normalizedStart, out normalizedEnd))
                    {
                        _node.NotifyConnectionRejected(start, pin);
                        CancelConnector();
                        return;
                    }
                }

                if (!CanConnectPins(normalizedStart, normalizedEnd))
                {
                    _node.NotifyConnectionRejected(normalizedStart, normalizedEnd);
                    CancelConnector();
                    return;
                }

                _connector.Start = normalizedStart;
                _connector.End = normalizedEnd;

                previousEnd?.OnDisconnected();
                pin.OnConnected();

                _node.Connectors ??= _factory.CreateList<IConnector>();
                if (!_node.Connectors.Contains(_connector))
                {
                    _node.Connectors.Add(_connector);
                }

                _connector = null;
            }
        }
    }

    public void ConnectorMove(double x, double y)
    {
        if (_connector is { End: not null })
        {
            _connector.End.X = x;
            _connector.End.Y = y;
            _connector.End.OnMoved();
        }
    }

    public void CutNodes()
    {
        var serializer = _node.GetSerializer();
        if (serializer is null)
        {
            return;
        }

        var selectedNodes = _node.GetSelectedNodes();
        var selectedConnectors = _node.GetSelectedConnectors();

        if (selectedNodes is not { Count: > 0 } && selectedConnectors is not { Count: > 0 })
        {
            return;
        }

        var nodesToCopy = selectedNodes is { Count: > 0 }
            ? new HashSet<INode>(selectedNodes)
            : new HashSet<INode>();
        var connectorsToCopy = nodesToCopy.Count > 0
            ? CollectConnectorsBetweenNodes(nodesToCopy)
            : new HashSet<IConnector>();

        if (nodesToCopy.Count > 0 || connectorsToCopy.Count > 0)
        {
            var clipboard = new Clipboard
            {
                SelectedNodes = nodesToCopy.Count > 0 ? nodesToCopy : null,
                SelectedConnectors = connectorsToCopy.Count > 0 ? connectorsToCopy : null
            };

            _clipboard = serializer.Serialize(clipboard);
        }

        var nodesToRemove = new HashSet<INode>();
        if (selectedNodes is { Count: > 0 })
        {
            foreach (var node in selectedNodes)
            {
                if (node.CanRemove())
                {
                    nodesToRemove.Add(node);
                }
            }
        }

        var attachedConnectors = nodesToRemove.Count > 0
            ? CollectConnectorsAttachedToNodes(nodesToRemove)
            : new HashSet<IConnector>();
        var connectorsToRemove = new HashSet<IConnector>(attachedConnectors);

        if (selectedConnectors is { Count: > 0 })
        {
            foreach (var connector in selectedConnectors)
            {
                connectorsToRemove.Add(connector);
            }
        }

        foreach (var connector in connectorsToRemove)
        {
            if (attachedConnectors.Contains(connector) || connector.CanRemove())
            {
                RemoveConnector(connector);
            }
        }

        foreach (var node in nodesToRemove)
        {
            _node.Nodes?.Remove(node);
            node.OnRemoved();
            NotifyPinsRemoved(node);
        }

        _node.NotifyDeselectedNodes();
        _node.NotifyDeselectedConnectors();

        _node.SetSelectedNodes(null);
        _node.SetSelectedConnectors(null);
        _node.NotifySelectionChanged();
    }

    public void CopyNodes()
    {
        var serializer = _node.GetSerializer();
        if (serializer is null)
        {
            return;
        }

        var selectedNodes = _node.GetSelectedNodes();
        var selectedConnectors = _node.GetSelectedConnectors();

        if (selectedNodes is not { Count: > 0 } && selectedConnectors is not { Count: > 0 })
        {
            return;
        }

        var nodesToCopy = selectedNodes is { Count: > 0 }
            ? new HashSet<INode>(selectedNodes)
            : new HashSet<INode>();
        var connectorsToCopy = nodesToCopy.Count > 0
            ? CollectConnectorsBetweenNodes(nodesToCopy)
            : new HashSet<IConnector>();

        if (nodesToCopy.Count == 0 && connectorsToCopy.Count == 0)
        {
            return;
        }

        var clipboard = new Clipboard
        {
            SelectedNodes = nodesToCopy.Count > 0 ? nodesToCopy : null,
            SelectedConnectors = connectorsToCopy.Count > 0 ? connectorsToCopy : null
        };

        _clipboard = serializer.Serialize(clipboard);
    }

    public void PasteNodes()
    {
        var serializer = _node.GetSerializer();
        if (serializer is null)
        {
            return;
        }

        if (_clipboard is null)
        {
            return;
        }

        var pressedX = _pressedX;
        var pressedY = _pressedY;

        var clipboard = serializer.Deserialize<Clipboard?>(_clipboard);
        if (clipboard is null)
        {
            return;
        }

        _node.NotifyDeselectedNodes();
        _node.NotifyDeselectedConnectors();

        _node.SetSelectedNodes(null);
        _node.SetSelectedConnectors(null);

        var selectedNodes = new HashSet<INode>();
        var selectedConnectors = new HashSet<IConnector>();
        var deltaX = 0.0;
        var deltaY = 0.0;

        if (clipboard.SelectedNodes is { Count: > 0 })
        {
            var minX = 0.0;
            var minY = 0.0;
            var i = 0;

            foreach (var node in clipboard.SelectedNodes)
            {
                minX = i == 0 ? node.X : Math.Min(minX, node.X);
                minY = i == 0 ? node.Y : Math.Min(minY, node.Y);
                i++;
            }

            deltaX = double.IsNaN(pressedX) ? 0.0 : pressedX - minX;
            deltaY = double.IsNaN(pressedY) ? 0.0 : pressedY - minY;

            _node.Nodes ??= _factory.CreateList<INode>();

            foreach (var node in clipboard.SelectedNodes)
            {
                if (node.CanMove())
                {
                    node.Move(deltaX, deltaY);
                }

                node.Parent = _node;

                _node.Nodes?.Add(node);
                node.OnCreated();

                if (node.CanSelect())
                {
                    selectedNodes.Add(node);
                    node.OnSelected();
                }
            }
        }

        if (clipboard.SelectedConnectors is { Count: > 0 })
        {
            _node.Connectors ??= _factory.CreateList<IConnector>();

            foreach (var connector in clipboard.SelectedConnectors)
            {
                if (connector.Waypoints is { Count: > 0 })
                {
                    foreach (var waypoint in connector.Waypoints)
                    {
                        waypoint.X += deltaX;
                        waypoint.Y += deltaY;
                    }
                }

                connector.Parent = _node;

                _node.Connectors.Add(connector);
                connector.OnCreated();

                if (connector.CanSelect())
                {
                    selectedConnectors.Add(connector);
                    connector.OnSelected();
                }
            }
        }

        _node.NotifyDeselectedNodes();

        if (selectedNodes.Count > 0)
        {
            _node.SetSelectedNodes(selectedNodes);
        }
        else
        {
            _node.SetSelectedNodes(null);
        }

        _node.NotifyDeselectedConnectors();

        if (selectedConnectors.Count > 0)
        {
            _node.SetSelectedConnectors(selectedConnectors);
        }
        else
        {
            _node.SetSelectedConnectors(null);
        }

        _node.NotifySelectionChanged();

        _pressedX = double.NaN;
        _pressedY = double.NaN;
    }

    public void DuplicateNodes()
    {
        _pressedX = double.NaN;
        _pressedY = double.NaN;

        CopyNodes();
        PasteNodes();
    }

    public void DeleteNodes()
    {
        var selectedNodes = _node.GetSelectedNodes();
        var selectedConnectors = _node.GetSelectedConnectors();
        var notify = false;

        var nodesToRemove = new HashSet<INode>();
        if (selectedNodes is { Count: > 0 })
        {
            foreach (var node in selectedNodes)
            {
                if (node.CanRemove())
                {
                    nodesToRemove.Add(node);
                }
            }
        }

        var attachedConnectors = nodesToRemove.Count > 0
            ? CollectConnectorsAttachedToNodes(nodesToRemove)
            : new HashSet<IConnector>();
        var connectorsToRemove = new HashSet<IConnector>(attachedConnectors);

        if (selectedConnectors is { Count: > 0 })
        {
            foreach (var connector in selectedConnectors)
            {
                connectorsToRemove.Add(connector);
            }
        }

        if (connectorsToRemove.Count > 0)
        {
            foreach (var connector in connectorsToRemove)
            {
                if (attachedConnectors.Contains(connector) || connector.CanRemove())
                {
                    RemoveConnector(connector);
                }
            }

            notify = true;
        }

        if (nodesToRemove.Count > 0)
        {
            foreach (var node in nodesToRemove)
            {
                _node.Nodes?.Remove(node);
                node.OnRemoved();
                NotifyPinsRemoved(node);
            }

            notify = true;
        }

        if (selectedNodes is { Count: > 0 })
        {
            _node.NotifyDeselectedNodes();
            _node.SetSelectedNodes(null);
            notify = true;
        }

        if (selectedConnectors is { Count: > 0 })
        {
            _node.NotifyDeselectedConnectors();
            _node.SetSelectedConnectors(null);
            notify = true;
        }

        if (notify)
        {
            _node.NotifySelectionChanged();
        }
    }

    public void AlignSelectedNodes(NodeAlignment alignment)
    {
        var selectedNodes = _node.GetSelectedNodes();
        if (selectedNodes is not { Count: > 1 })
        {
            return;
        }

        if (!TryGetNodesBounds(selectedNodes, out var left, out var top, out var right, out var bottom))
        {
            return;
        }

        var centerX = (left + right) / 2.0;
        var centerY = (top + bottom) / 2.0;

        foreach (var node in selectedNodes)
        {
            if (!node.CanMove())
            {
                continue;
            }

            var deltaX = 0.0;
            var deltaY = 0.0;

            switch (alignment)
            {
                case NodeAlignment.Left:
                    deltaX = left - node.X;
                    break;
                case NodeAlignment.Center:
                    deltaX = centerX - (node.X + node.Width / 2.0);
                    break;
                case NodeAlignment.Right:
                    deltaX = right - (node.X + node.Width);
                    break;
                case NodeAlignment.Top:
                    deltaY = top - node.Y;
                    break;
                case NodeAlignment.Middle:
                    deltaY = centerY - (node.Y + node.Height / 2.0);
                    break;
                case NodeAlignment.Bottom:
                    deltaY = bottom - (node.Y + node.Height);
                    break;
            }

            if (Math.Abs(deltaX) > 0.001 || Math.Abs(deltaY) > 0.001)
            {
                node.Move(deltaX, deltaY);
                node.OnMoved();
            }
        }

        _node.NotifySelectionChanged();
    }

    public void DistributeSelectedNodes(NodeDistribution distribution)
    {
        var selectedNodes = _node.GetSelectedNodes();
        if (selectedNodes is not { Count: > 2 })
        {
            return;
        }

        var movable = new List<INode>();
        foreach (var node in selectedNodes)
        {
            if (node.CanMove())
            {
                movable.Add(node);
            }
        }

        if (movable.Count < 3)
        {
            return;
        }

        if (distribution == NodeDistribution.Horizontal)
        {
            movable.Sort((a, b) => a.X.CompareTo(b.X));
            var left = movable[0].X;
            var right = movable[movable.Count - 1].X + movable[movable.Count - 1].Width;
            var totalWidth = 0.0;
            foreach (var node in movable)
            {
                totalWidth += node.Width;
            }

            var available = right - left - totalWidth;
            if (available < 0.0)
            {
                return;
            }

            var spacing = available / (movable.Count - 1);
            var cursor = left;

            foreach (var node in movable)
            {
                var deltaX = cursor - node.X;
                if (Math.Abs(deltaX) > 0.001)
                {
                    node.Move(deltaX, 0.0);
                    node.OnMoved();
                }

                cursor += node.Width + spacing;
            }
        }
        else
        {
            movable.Sort((a, b) => a.Y.CompareTo(b.Y));
            var top = movable[0].Y;
            var bottom = movable[movable.Count - 1].Y + movable[movable.Count - 1].Height;
            var totalHeight = 0.0;
            foreach (var node in movable)
            {
                totalHeight += node.Height;
            }

            var available = bottom - top - totalHeight;
            if (available < 0.0)
            {
                return;
            }

            var spacing = available / (movable.Count - 1);
            var cursor = top;

            foreach (var node in movable)
            {
                var deltaY = cursor - node.Y;
                if (Math.Abs(deltaY) > 0.001)
                {
                    node.Move(0.0, deltaY);
                    node.OnMoved();
                }

                cursor += node.Height + spacing;
            }
        }

        _node.NotifySelectionChanged();
    }

    public void OrderSelectedNodes(NodeOrder order)
    {
        if (_node.Nodes is not { Count: > 0 } nodes)
        {
            return;
        }

        var selectedNodes = _node.GetSelectedNodes();
        if (selectedNodes is not { Count: > 0 })
        {
            return;
        }

        switch (order)
        {
            case NodeOrder.BringToFront:
            {
                var moving = new List<INode>();
                foreach (var node in nodes)
                {
                    if (selectedNodes.Contains(node))
                    {
                        moving.Add(node);
                    }
                }

                foreach (var node in moving)
                {
                    nodes.Remove(node);
                }

                foreach (var node in moving)
                {
                    nodes.Add(node);
                }

                break;
            }
            case NodeOrder.SendToBack:
            {
                var moving = new List<INode>();
                foreach (var node in nodes)
                {
                    if (selectedNodes.Contains(node))
                    {
                        moving.Add(node);
                    }
                }

                foreach (var node in moving)
                {
                    nodes.Remove(node);
                }

                var insertIndex = 0;
                foreach (var node in moving)
                {
                    nodes.Insert(insertIndex, node);
                    insertIndex++;
                }

                break;
            }
            case NodeOrder.BringForward:
            {
                for (var i = nodes.Count - 2; i >= 0; i--)
                {
                    if (selectedNodes.Contains(nodes[i]) && !selectedNodes.Contains(nodes[i + 1]))
                    {
                        (nodes[i], nodes[i + 1]) = (nodes[i + 1], nodes[i]);
                    }
                }

                break;
            }
            case NodeOrder.SendBackward:
            {
                for (var i = 1; i < nodes.Count; i++)
                {
                    if (selectedNodes.Contains(nodes[i]) && !selectedNodes.Contains(nodes[i - 1]))
                    {
                        (nodes[i], nodes[i - 1]) = (nodes[i - 1], nodes[i]);
                    }
                }

                break;
            }
        }
    }

    public void SelectAllNodes()
    {
        var notify = false;

        if (_node.Nodes is not null)
        {
            _node.NotifyDeselectedNodes();

            _node.SetSelectedNodes(null);

            var selectedNodes = new HashSet<INode>();
            var nodes = _node.Nodes;

            foreach (var node in nodes)
            {
                if (node.CanSelect())
                {
                    selectedNodes.Add(node);
                    node.OnSelected();
                }
            }

            if (selectedNodes.Count > 0)
            {
                _node.SetSelectedNodes(selectedNodes);
                notify = true;
            }
        }

        if (_node.Connectors is not null)
        {
            _node.NotifyDeselectedConnectors();

            _node.SetSelectedConnectors(null);

            var selectedConnectors = new HashSet<IConnector>();
            var connectors = _node.Connectors;

            foreach (var connector in connectors)
            {
                if (connector.CanSelect())
                {
                    selectedConnectors.Add(connector);
                    connector.OnSelected();
                }
            }

            if (selectedConnectors.Count > 0)
            {
                _node.SetSelectedConnectors(selectedConnectors);
                notify = true;
            }
        }

        if (notify)
        {
            _node.NotifySelectionChanged();
        }
    }

    public void LockSelection()
    {
        var selectedNodes = _node.GetSelectedNodes();
        var selectedConnectors = _node.GetSelectedConnectors();
        var changed = false;

        if (selectedNodes is { Count: > 0 })
        {
            foreach (var node in selectedNodes)
            {
                if (!node.IsLocked)
                {
                    node.IsLocked = true;
                    changed = true;
                }
            }
        }

        if (selectedConnectors is { Count: > 0 })
        {
            foreach (var connector in selectedConnectors)
            {
                if (!connector.IsLocked)
                {
                    connector.IsLocked = true;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            _node.NotifySelectionChanged();
        }
    }

    public void UnlockSelection()
    {
        var selectedNodes = _node.GetSelectedNodes();
        var selectedConnectors = _node.GetSelectedConnectors();
        var changed = false;

        if (selectedNodes is { Count: > 0 })
        {
            foreach (var node in selectedNodes)
            {
                if (node.IsLocked)
                {
                    node.IsLocked = false;
                    changed = true;
                }
            }
        }

        if (selectedConnectors is { Count: > 0 })
        {
            foreach (var connector in selectedConnectors)
            {
                if (connector.IsLocked)
                {
                    connector.IsLocked = false;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            _node.NotifySelectionChanged();
        }
    }

    public void HideSelection()
    {
        var selectedNodes = _node.GetSelectedNodes();
        var selectedConnectors = _node.GetSelectedConnectors();
        var changed = false;

        if (selectedNodes is { Count: > 0 })
        {
            foreach (var node in selectedNodes)
            {
                if (node.IsVisible)
                {
                    node.IsVisible = false;
                    node.OnDeselected();
                    changed = true;
                }
            }

            _node.SetSelectedNodes(null);
        }

        if (selectedConnectors is { Count: > 0 })
        {
            foreach (var connector in selectedConnectors)
            {
                if (connector.IsVisible)
                {
                    connector.IsVisible = false;
                    connector.OnDeselected();
                    changed = true;
                }
            }

            _node.SetSelectedConnectors(null);
        }

        if (changed)
        {
            _node.NotifySelectionChanged();
        }
    }

    public void ShowSelection()
    {
        var selectedNodes = _node.GetSelectedNodes();
        var selectedConnectors = _node.GetSelectedConnectors();
        var changed = false;

        if (selectedNodes is { Count: > 0 })
        {
            foreach (var node in selectedNodes)
            {
                if (!node.IsVisible)
                {
                    node.IsVisible = true;
                    changed = true;
                }
            }
        }

        if (selectedConnectors is { Count: > 0 })
        {
            foreach (var connector in selectedConnectors)
            {
                if (!connector.IsVisible)
                {
                    connector.IsVisible = true;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            _node.NotifySelectionChanged();
        }
    }

    public void ShowAll()
    {
        var changed = false;

        if (_node.Nodes is { Count: > 0 })
        {
            foreach (var node in _node.Nodes)
            {
                if (!node.IsVisible)
                {
                    node.IsVisible = true;
                    changed = true;
                }
            }
        }

        if (_node.Connectors is { Count: > 0 })
        {
            foreach (var connector in _node.Connectors)
            {
                if (!connector.IsVisible)
                {
                    connector.IsVisible = true;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            _node.NotifySelectionChanged();
        }
    }

    public void DeselectAllNodes()
    {
        _node.NotifyDeselectedNodes();
        _node.NotifyDeselectedConnectors();

        _node.SetSelectedNodes(null);
        _node.SetSelectedConnectors(null);
        _node.NotifySelectionChanged();

        if (IsConnectorMoving())
        {
            CancelConnector();
        }
    }
}
