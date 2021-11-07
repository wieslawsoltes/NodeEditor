using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Input;
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
        private double _pressedX;
        private double _pressedY;

        public DrawingNodeViewModel()
        {
            CutCommand = ReactiveCommand.Create(CutNodes);

            CopyCommand = ReactiveCommand.Create(CopyNodes);

            PasteCommand = ReactiveCommand.Create(PasteNodes);

            DeleteCommand = ReactiveCommand.Create(DeleteNodes);
        }

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

        public ICommand CutCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand PasteCommand { get; }

        public ICommand DeleteCommand { get; }

        public void DrawingLeftPressed(double x, double y)
        {
        }

        public void DrawingRightPressed(double x, double y)
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

        public void ConnectorLeftPressed(IPin pin)
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
            if (_connector is { End: { } })
            {
                _connector.End.X = x;
                _connector.End.Y = y;
            }
        }

        public void CutNodes()
        {
            if (Serializer is null)
            {
                return;
            }

            if (SelectedNodes is not { Count: > 0 })
            {
                return;
            }

            var selectedNodes = SelectedNodes;
                
            _clipboard = Serializer.Serialize(selectedNodes);

            foreach (var node in selectedNodes)
            {
                Nodes?.Remove(node);
            }

            SelectedNodes = null;
        }

        public void CopyNodes()
        {
            if (Serializer is null)
            {
                return;
            }

            if (SelectedNodes is not { Count: > 0 })
            {
                return;
            }

            _clipboard = Serializer.Serialize(SelectedNodes);
        }

        public void PasteNodes()
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

            var nodes = Serializer.Deserialize<ISet<INode>>(_clipboard);

            SelectedNodes = null;

            var selectedNodes = new HashSet<INode>();

            if (nodes is { Count: > 0 })
            {
                var minX = 0.0;
                var minY = 0.0;
                var i = 0;

                foreach (var node in nodes)
                {
                    minX = i == 0 ? node.X : Math.Min(minX, node.X);
                    minY = i == 0 ? node.Y : Math.Min(minY, node.Y);
                    i++;
                }

                var deltaX = pressedX - minX;
                var deltaY = pressedY - minY;

                foreach (var node in nodes)
                {
                    node.X += deltaX;
                    node.Y += deltaY;
                    node.Parent = this;

                    Nodes?.Add(node);
                    selectedNodes.Add(node);
                }
            }

            SelectedNodes = selectedNodes;

            _pressedX = 0;
            _pressedY = 0;
        }

        public void DeleteNodes()
        {
            if (SelectedNodes is not { Count: > 0 })
            {
                return;
            }

            var selectedNodes = SelectedNodes;

            foreach (var node in selectedNodes)
            {
                Nodes?.Remove(node);
            }

            SelectedNodes = null;
        }
    }
}
