using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels
{
    [DataContract(IsReference = true)]
    public class DrawingNodeViewModel : NodeViewModel, IDrawingNode
    {
        private IList<INode>? _nodes;
        private ISet<INode>? _selectedNodes;
        private IList<IConnector>? _connectors;
        private INodeSerializer? _serializer;
        private IConnector? _connector;
        private string? _clipboard;

        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public IList<INode>? Nodes
        {
            get => _nodes;
            set => this.RaiseAndSetIfChanged(ref _nodes, value);
        }

        [IgnoreDataMember]
        public ISet<INode>? SelectedNodes
        {
            get => _selectedNodes;
            set => this.RaiseAndSetIfChanged(ref _selectedNodes, value);
        }

        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public IList<IConnector>? Connectors
        {
            get => _connectors;
            set => this.RaiseAndSetIfChanged(ref _connectors, value);
        }

        [IgnoreDataMember]
        public INodeSerializer? Serializer
        {
            get => _serializer;
            set => this.RaiseAndSetIfChanged(ref _serializer, value);
        }

        public void DrawingPressed(double x, double y)
        {
            // TODO: Handle pressed event.
        }

        public void DrawingCancel()
        {
            if (_connector is { })
            {
                if (Connectors is { })
                {
                    Connectors.Remove(_connector);
                }

                _connector = null;
            }
        }

        public void ConnectorPressed(IPin pin)
        {
            if (_connectors is null)
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
                _connector.End = pin;
                _connector = null;
            }
        }

        public void ConnectorMove(double x, double y)
        {
            if (_connector is { })
            {
                if (_connector.End is { })
                {
                    _connector.End.X = x;
                    _connector.End.Y = y;
                }
            }
        }

        public void CutNodes()
        {
            if (Serializer is null)
            {
                return;
            }
 
            if (SelectedNodes is { Count: > 0 })
            {
                _clipboard = Serializer.Serialize(SelectedNodes);

                foreach (var node in SelectedNodes)
                {
                    Nodes?.Remove(node);
                }
            }
        }

        public void CopyNodes()
        {
            if (Serializer is null)
            {
                return;
            }
 
            if (SelectedNodes is { Count: > 0 })
            {
                _clipboard = Serializer.Serialize(SelectedNodes);
            }
        }

        public void PasteNodes(double x = 0.0, double y = 0.0)
        {
            if (Serializer is null)
            {
                return;
            }

            if (_clipboard is null)
            {
                return;
            }

            var nodes = Serializer.Deserialize<ISet<INode>>(_clipboard);

            if (nodes is { Count: > 0 })
            {
                foreach (var node in nodes)
                {
                    node.X += x;
                    node.Y += y;
                    node.Parent = this;

                    Nodes?.Add(node);
                }
            }
        }
    }
}
