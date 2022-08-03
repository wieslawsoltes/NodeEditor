using System;
using System.Collections.Generic;

namespace NodeEditor.Model;

public sealed class DrawingNodeEditor
{
    private readonly IDrawingNode _node;
    private readonly DrawingNodeFactory _factory;
    private IConnector? _connector;
    private string? _clipboard;
    private double _pressedX = double.NaN;
    private double _pressedY = double.NaN;

    private class Clipboard
    {
        public ISet<INode>? SelectedNodes { get; set; }
        public ISet<IConnector>? SelectedConnectors { get; set; }
    }

    public DrawingNodeEditor(IDrawingNode node, DrawingNodeFactory factory)
    {
        _node = node;
        _factory = factory;
    }

    public T? Clone<T>(T source)
    {
        if (_node.Serializer is null)
        {
            return default;
        }

        var text = _node.Serializer.Serialize(source);

        return _node.Serializer.Deserialize<T>(text);
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

            var connector = _factory.CreateConnector();
            connector.Parent = _node;
            connector.Start = pin;
            connector.End = end;

            if (showWhenMoving)
            {
                _node.Connectors ??= _factory.CreateConnectorList();
                _node.Connectors.Add(connector);            
            }

            _connector = connector;
        }
        else
        {
            if (_connector.Start != pin)
            {
                _connector.End = pin;

                if (!showWhenMoving)
                {
                    _node.Connectors ??= _factory.CreateConnectorList();
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
        }
    }

    public void CutNodes()
    {
        if (_node.Serializer is null)
        {
            return;
        }

        if (_node.SelectedNodes is not { Count: > 0 } && _node.SelectedConnectors is not { Count: > 0 })
        {
            return;
        }

        var clipboard = new Clipboard
        {
            SelectedNodes = _node.SelectedNodes,
            SelectedConnectors = _node.SelectedConnectors
        };

        _clipboard = _node.Serializer.Serialize(clipboard);

        if (clipboard.SelectedNodes is { })
        {
            foreach (var node in clipboard.SelectedNodes)
            {
                if (node.CanRemove())
                {
                    _node.Nodes?.Remove(node);
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
                }
            }
        }

        _node.SelectedNodes = null;
        _node.SelectedConnectors = null;
    }

    public void CopyNodes()
    {
        if (_node.Serializer is null)
        {
            return;
        }

        if (_node.SelectedNodes is not { Count: > 0 } && _node.SelectedConnectors is not { Count: > 0 })
        {
            return;
        }

        var clipboard = new Clipboard
        {
            SelectedNodes = _node.SelectedNodes,
            SelectedConnectors = _node.SelectedConnectors
        };

        _clipboard = _node.Serializer.Serialize(clipboard);
    }

    public void PasteNodes()
    {
        if (_node.Serializer is null)
        {
            return;
        }

        if (_clipboard is null)
        {
            return;
        }

        var pressedX = _pressedX;
        var pressedY = _pressedY;

        var clipboard = _node.Serializer.Deserialize<Clipboard?>(_clipboard);
        if (clipboard is null)
        {
            return;
        }

        _node.SelectedNodes = null;
        _node.SelectedConnectors = null;

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

                if (node.CanSelect())
                {
                    selectedNodes.Add(node);
                }
            }
        }

        if (clipboard.SelectedConnectors is { Count: > 0 })
        {
            foreach (var connector in clipboard.SelectedConnectors)
            {
                connector.Parent = _node;

                _node.Connectors?.Add(connector);

                if (connector.CanSelect())
                {
                    selectedConnectors.Add(connector);
                }
            }
        }

        if (selectedNodes.Count > 0)
        {
            _node.SelectedNodes = selectedNodes;
        }

        if (selectedConnectors.Count > 0)
        {
            _node.SelectedConnectors = selectedConnectors;
        }

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
        if (_node.SelectedNodes is { Count: > 0 })
        {
            var selectedNodes = _node.SelectedNodes;

            foreach (var node in selectedNodes)
            {
                if (node.CanRemove())
                {
                    _node.Nodes?.Remove(node);
                }
            }

            _node.SelectedNodes = null;
        }

        if (_node.SelectedConnectors is { Count: > 0 })
        {
            var selectedConnectors = _node.SelectedConnectors;

            foreach (var connector in selectedConnectors)
            {
                if (connector.CanRemove())
                {
                    _node.Connectors?.Remove(connector);
                }
            }

            _node.SelectedConnectors = null;
        }
    }

    public void SelectAllNodes()
    {
        if (_node.Nodes is not null)
        {
            _node.SelectedNodes = null;

            var selectedNodes = new HashSet<INode>();
            var nodes = _node.Nodes;

            foreach (var node in nodes)
            {
                if (node.CanSelect())
                {
                    selectedNodes.Add(node);
                }
            }

            if (selectedNodes.Count > 0)
            {
                _node.SelectedNodes = selectedNodes;
            }
        }

        if (_node.Connectors is not null)
        {
            _node.SelectedConnectors = null;

            var selectedConnectors = new HashSet<IConnector>();
            var connectors = _node.Connectors;

            foreach (var connector in connectors)
            {
                if (connector.CanSelect())
                {
                    selectedConnectors.Add(connector);
                }
            }

            if (selectedConnectors.Count > 0)
            {
                _node.SelectedConnectors = selectedConnectors;
            }
        }
    }

    public void DeselectAllNodes()
    {
        _node.SelectedNodes = null;
        _node.SelectedConnectors = null;

        if (IsConnectorMoving())
        {
            CancelConnector();
        }
    }
}
