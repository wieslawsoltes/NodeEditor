using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Input;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels;

[DataContract(IsReference = true)]
public class DrawingNodeViewModel : NodeViewModel, IDrawingNode
{
    private IList<INode>? _nodes;
    private IList<IConnector>? _connectors;
    private ISet<INode>? _selectedNodes;
    private ISet<IConnector>? _selectedConnectors;
    private INodeSerializer? _serializer;
    private IConnector? _connector;
    private string? _clipboard;
    private double _pressedX = double.NaN;
    private double _pressedY = double.NaN;

    private class Clipboard
    {
        public ISet<INode>? SelectedNodes { get; set; }
        public ISet<IConnector>? SelectedConnectors { get; set; }
    }

    public DrawingNodeViewModel()
    {
        CutCommand = ReactiveCommand.Create(CutNodes);

        CopyCommand = ReactiveCommand.Create(CopyNodes);

        PasteCommand = ReactiveCommand.Create(PasteNodes);

        DuplicateCommand = ReactiveCommand.Create(DuplicateNodes);

        SelectAllCommand = ReactiveCommand.Create(SelectAllNodes);

        DeselectAllCommand = ReactiveCommand.Create(DeselectAllNodes);

        DeleteCommand = ReactiveCommand.Create(DeleteNodes);

    }

    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public IList<INode>? Nodes
    {
        get => _nodes;
        set => this.RaiseAndSetIfChanged(ref _nodes, value);
    }

    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public IList<IConnector>? Connectors
    {
        get => _connectors;
        set => this.RaiseAndSetIfChanged(ref _connectors, value);
    }

    [IgnoreDataMember]
    public ISet<INode>? SelectedNodes
    {
        get => _selectedNodes;
        set => this.RaiseAndSetIfChanged(ref _selectedNodes, value);
    }

    [IgnoreDataMember]
    public ISet<IConnector>? SelectedConnectors
    {
        get => _selectedConnectors;
        set => this.RaiseAndSetIfChanged(ref _selectedConnectors, value);
    }

    [IgnoreDataMember]
    public INodeSerializer? Serializer
    {
        get => _serializer;
        set => this.RaiseAndSetIfChanged(ref _serializer, value);
    }

    public ICommand CutCommand { get; }

    public ICommand CopyCommand { get; }

    public ICommand PasteCommand { get; }

    public ICommand DuplicateCommand { get; }

    public ICommand SelectAllCommand { get; }

    public ICommand DeselectAllCommand { get; }

    public ICommand DeleteCommand { get; }

    public virtual bool CanSelectNodes()
    {
        if (_connector is { })
        {
            return false;
        }

        return true;
    }

    public virtual bool CanSelectConnectors()
    {
        if (_connector is { })
        {
            return false;
        }

        return true;
    }

    public virtual bool CanConnectPin(IPin pin)
    {
        return true;
    }

    public virtual void DrawingLeftPressed(double x, double y)
    {
    }

    public virtual void DrawingRightPressed(double x, double y)
    {
        _pressedX = x;
        _pressedY = y;

        if (_connector is { })
        {
            if (Connectors is { })
            {
                Connectors.Remove(_connector);
            }

            _connector = null;
        }
    }

    public virtual void ConnectorLeftPressed(IPin pin)
    {
        if (_connectors is null)
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

            var end = new PinViewModel
            {
                Parent = null,
                X = x,
                Y = y, 
                Width = pin.Width, 
                Height = pin.Height
            };

            var connector = new ConnectorViewModel
            {
                Parent = this,
                Start = pin,
                End = end
            };

            Connectors ??= new ObservableCollection<IConnector>();
            Connectors.Add(connector);

            _connector = connector;
        }
        else
        {
            if (_connector.Start != pin)
            {
                _connector.End = pin;
                _connector = null;
            }
        }
    }

    public virtual void ConnectorMove(double x, double y)
    {
        if (_connector is { End: { } })
        {
            _connector.End.X = x;
            _connector.End.Y = y;
        }
    }

    public virtual void CutNodes()
    {
        if (Serializer is null)
        {
            return;
        }

        if (SelectedNodes is not { Count: > 0 } && SelectedConnectors is not { Count: > 0 })
        {
            return;
        }

        var clipboard = new Clipboard
        {
            SelectedNodes = SelectedNodes,
            SelectedConnectors = SelectedConnectors
        };

        _clipboard = Serializer.Serialize(clipboard);

        if (clipboard.SelectedNodes is { })
        {
            foreach (var node in clipboard.SelectedNodes)
            {
                if (node.CanRemove())
                {
                    Nodes?.Remove(node);
                }
            }
        }

        if (clipboard.SelectedConnectors is { })
        {
            foreach (var connector in clipboard.SelectedConnectors)
            {
                if (connector.CanRemove())
                {
                    Connectors?.Remove(connector);
                }
            }
        }

        SelectedNodes = null;
        SelectedConnectors = null;
    }

    public virtual void CopyNodes()
    {
        if (Serializer is null)
        {
            return;
        }

        if (SelectedNodes is not { Count: > 0 } && SelectedConnectors is not { Count: > 0 })
        {
            return;
        }

        var clipboard = new Clipboard
        {
            SelectedNodes = SelectedNodes,
            SelectedConnectors = SelectedConnectors
        };

        _clipboard = Serializer.Serialize(clipboard);
    }

    public virtual void PasteNodes()
    {
        if (Serializer is null)
        {
            return;
        }

        if (_clipboard is null)
        {
            return;
        }

        var pressedX = _pressedX;
        var pressedY = _pressedY;

        var clipboard = Serializer.Deserialize<Clipboard?>(_clipboard);
        if (clipboard is null)
        {
            return;
        }

        SelectedNodes = null;
        SelectedConnectors = null;

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

                node.Parent = this;

                Nodes?.Add(node);

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
                connector.Parent = this;

                Connectors?.Add(connector);

                if (connector.CanSelect())
                {
                    selectedConnectors.Add(connector);
                }
            }
        }

        if (selectedNodes.Count > 0)
        {
            SelectedNodes = selectedNodes;
        }

        if (selectedConnectors.Count > 0)
        {
            SelectedConnectors = selectedConnectors;
        }

        _pressedX = double.NaN;
        _pressedY = double.NaN;
    }

    public virtual void DuplicateNodes()
    {
        _pressedX = double.NaN;
        _pressedY = double.NaN;

        CopyNodes();
        PasteNodes();
    }

    public virtual void DeleteNodes()
    {
        if (SelectedNodes is { Count: > 0 })
        {
            var selectedNodes = SelectedNodes;

            foreach (var node in selectedNodes)
            {
                if (node.CanRemove())
                {
                    Nodes?.Remove(node);
                }
            }

            SelectedNodes = null;
        }

        if (SelectedConnectors is { Count: > 0 })
        {
            var selectedConnectors = SelectedConnectors;

            foreach (var connector in selectedConnectors)
            {
                if (connector.CanRemove())
                {
                    Connectors?.Remove(connector);
                }
            }

            SelectedConnectors = null;
        }
    }

    public virtual void SelectAllNodes()
    {
        if (Nodes is not null)
        {
            SelectedNodes = null;

            var selectedNodes = new HashSet<INode>();
            var nodes = Nodes;

            foreach (var node in nodes)
            {
                if (node.CanSelect())
                {
                    selectedNodes.Add(node);
                }
            }

            if (selectedNodes.Count > 0)
            {
                SelectedNodes = selectedNodes;
            }
        }

        if (Connectors is not null)
        {
            SelectedConnectors = null;

            var selectedConnectors = new HashSet<IConnector>();
            var connectors = Connectors;

            foreach (var connector in connectors)
            {
                if (connector.CanSelect())
                {
                    selectedConnectors.Add(connector);
                }
            }

            if (selectedConnectors.Count > 0)
            {
                SelectedConnectors = selectedConnectors;
            }
        }
    }

    public virtual void DeselectAllNodes()
    {
        SelectedNodes = null;
        SelectedConnectors = null;
    }
}
