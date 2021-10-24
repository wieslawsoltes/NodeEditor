using System.Collections.ObjectModel;
using ReactiveUI;

namespace NodeEditor.ViewModels
{
    public class DrawingNodeViewModel : NodeViewModel
    {
        private ObservableCollection<NodeViewModel>? _nodes;
        private ObservableCollection<ConnectorViewModel>? _connectors;

        public ObservableCollection<NodeViewModel>? Nodes
        {
            get => _nodes;
            set => this.RaiseAndSetIfChanged(ref _nodes, value);
        }

        public ObservableCollection<ConnectorViewModel>? Connectors
        {
            get => _connectors;
            set => this.RaiseAndSetIfChanged(ref _connectors, value);
        }

        private ConnectorViewModel? _connectorViewModel;

        public void DrawingPressed(double d, double d1)
        {
            // TODO:
        }

        public void DrawingCancel()
        {
            if (_connectorViewModel is { })
            {
                if (Connectors is { })
                {
                    Connectors.Remove(_connectorViewModel);
                }

                _connectorViewModel = null;
            }
        }

        public void ConnectorPressed(PinViewModel pin)
        {
            if (_connectors is null)
            {
                return;
            }

            if (_connectorViewModel is null)
            {
                var x = pin.X;
                var y = pin.Y;

                if (pin.Parent is { })
                {
                    x += pin.Parent.X;
                    y += pin.Parent.Y;
                }

                var end = new PinViewModel()
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

                Connectors ??= new ObservableCollection<ConnectorViewModel>();
                Connectors.Add(connector);

                _connectorViewModel = connector;
            }
            else
            {
                _connectorViewModel.End = pin;
                _connectorViewModel = null;
            }
        }

        public void ConnectorMove(double x, double y)
        {
            if (_connectorViewModel is { })
            {
                if (_connectorViewModel.End is { })
                {
                    _connectorViewModel.End.X = x;
                    _connectorViewModel.End.Y = y;
                }
            }
        }
    }
}
