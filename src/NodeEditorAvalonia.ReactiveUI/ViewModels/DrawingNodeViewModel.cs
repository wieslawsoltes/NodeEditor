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
        private IList<IConnector>? _connectors;
        private IConnector? _connector;

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
    }
}
