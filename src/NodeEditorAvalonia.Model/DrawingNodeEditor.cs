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
        if (_node.Connectors is { })
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
        if (_connector is { })
        {
            return true;
        }

        return false;
    }

    public void CancelConnector()
    {
        if (_connector is { })
        {
            if (_node.Connectors is { })
            {
                _node.Connectors.Remove(_connector);
            }

            _connector = null;
        }
    }

    public bool CanSelectNodes()
    {
        if (_connector is { })
        {
            return false;
        }

        return true;
    }

    public bool CanSelectConnectors()
    {
        if (_connector is { })
        {
            return false;
        }

        return true;
    }

    public bool CanConnectPin(IPin pin)
    {
        if (!_node.EnableMultiplePinConnections)
        {
            if (IsPinConnected(pin))
            {
                return false;
            }
        }

        return true;
    }

    private void NotifyPinsRemoved(INode node)
    {
        if (node.Pins is { })
        {
            foreach (var pin in node.Pins)
            {
                pin.OnRemoved();
            }
        }
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
        if (_node.Connectors is null)
        {
            return;
        }

        if (!CanConnectPin(pin) || !pin.CanConnect())
        {
            return;
        }

        if (_connector is null)
        {
            var x = pin.X;
            var y = pin.Y;

            if (pin.Parent is { })
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
                var end = _connector.End;
                _connector.End = pin;
                end?.OnDisconnected();
                pin.OnConnected();

                if (!showWhenMoving)
                {
                    _node.Connectors ??= _factory.CreateList<IConnector>();
                    _node.Connectors.Add(_connector);            
                }

                _connector = null;
            }
        }
    }

    public void ConnectorMove(double x, double y)
    {
        if (_connector is { End: { } })
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

        var clipboard = new Clipboard
        {
            SelectedNodes = selectedNodes,
            SelectedConnectors = selectedConnectors
        };

        _clipboard = serializer.Serialize(clipboard);

        if (clipboard.SelectedNodes is { })
        {
            foreach (var node in clipboard.SelectedNodes)
            {
                if (node.CanRemove())
                {
                    _node.Nodes?.Remove(node);
                    node.OnRemoved();
                    NotifyPinsRemoved(node);
                }
            }
        }

        if (clipboard.SelectedConnectors is { })
        {
            foreach (var connector in clipboard.SelectedConnectors)
            {
                if (connector.CanRemove())
                {
                    _node.Connectors?.Remove(connector);
                    connector.OnRemoved();
                }
            }
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

        var clipboard = new Clipboard
        {
            SelectedNodes = selectedNodes,
            SelectedConnectors = selectedConnectors
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

            var deltaX = double.IsNaN(pressedX) ? 0.0 : pressedX - minX;
            var deltaY = double.IsNaN(pressedY) ? 0.0 : pressedY - minY;

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
            foreach (var connector in clipboard.SelectedConnectors)
            {
                connector.Parent = _node;

                _node.Connectors?.Add(connector);
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

        if (selectedNodes is { Count: > 0 })
        {
            foreach (var node in selectedNodes)
            {
                if (node.CanRemove())
                {
                    _node.Nodes?.Remove(node);
                    node.OnRemoved();
                    NotifyPinsRemoved(node);
                }
            }

            _node.NotifyDeselectedNodes();

            _node.SetSelectedNodes(null);
            notify = true;
        }

        if (selectedConnectors is { Count: > 0 })
        {
            foreach (var connector in selectedConnectors)
            {
                if (connector.CanRemove())
                {
                    _node.Connectors?.Remove(connector);
                    connector.OnRemoved();
                }
            }

            _node.NotifyDeselectedConnectors();

            _node.SetSelectedConnectors(null);
            notify = true;
        }

        if (notify)
        {
            _node.NotifySelectionChanged();
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
