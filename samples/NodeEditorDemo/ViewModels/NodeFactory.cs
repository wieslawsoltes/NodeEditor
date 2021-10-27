using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditor.ViewModels;
using NodeEditorDemo.ViewModels.Nodes;

namespace NodeEditorDemo.ViewModels
{
    public static class NodeFactory
    {
        public static NodeViewModel CreateRectangle(double x, double y, double width, double height, string? label, double pinSize = 8)
        {
            var node = new NodeViewModel
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Pins = new ObservableCollection<PinViewModel>(),
                Content = new RectangleViewModel { Label = label }
            };

            node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left);
            node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right);
            node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top);
            node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom);

            return node;
        }

        public static NodeViewModel CreateEllipse(double x, double y, double width, double height, string? label, double pinSize = 8)
        {
            var node = new NodeViewModel
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Pins = new ObservableCollection<PinViewModel>(),
                Content = new EllipseViewModel { Label = label }
            };

            node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left);
            node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right);
            node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top);
            node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom);
            
            return node;
        }

        public static NodeViewModel CreateSignal(double x, double y, double width = 120, double height = 30, string? label = null, bool? state = false, double pinSize = 8)
        {
            var node = new NodeViewModel
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Pins = new ObservableCollection<PinViewModel>(),
                Content = new SignalViewModel { Label = label, State = state }
            };

            node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left);
            node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right);
  
            return node;
        }

        public static NodeViewModel CreateAndGate(double x, double y, double width = 60, double height = 60, double pinSize = 8)
        {
            var node = new NodeViewModel
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Pins = new ObservableCollection<PinViewModel>(),
                Content = new AndGateViewModel() { Label = "&" }
            };

            node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left);
            node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right);
            node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top);
            node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom);

            return node;
        }

        public static NodeViewModel CreateOrGate(double x, double y, double width = 60, double height = 60, int count = 1, double pinSize = 8)
        {
            var node = new NodeViewModel
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Pins = new ObservableCollection<PinViewModel>(),
                Content = new OrGateViewModel() { Label = "≥", Count = count}
            };

            node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left);
            node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right);
            node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top);
            node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom);

            return node;
        }

        public static ConnectorViewModel CreateConnector(PinViewModel? start, PinViewModel? end)
        {
            return new ConnectorViewModel
            { 
                Start = start,
                End = end
            };
        }

        public static DrawingNodeViewModel CreateDrawing()
        {
            var drawing = new DrawingNodeViewModel
            {
                X = 0,
                Y = 0,
                Width = 750,
                Height = 480,
                Nodes = new ObservableCollection<NodeViewModel>(),
                Connectors = new ObservableCollection<ConnectorViewModel>()
            };

            return drawing;
        }

        public static DrawingNodeViewModel CreateDemoDrawing()
        {
            var drawing = new DrawingNodeViewModel
            {
                X = 0,
                Y = 0,
                Width = 750,
                Height = 480,
                Nodes = new ObservableCollection<NodeViewModel>(),
                Connectors = new ObservableCollection<ConnectorViewModel>()
            };

            var rectangle0 = NodeFactory.CreateRectangle(30, 30, 60, 60, "rect0");
            rectangle0.Parent = drawing;
            drawing.Nodes.Add(rectangle0);

            var rectangle1 = NodeFactory.CreateRectangle(220, 30, 60, 60, "rect1");
            rectangle1.Parent = drawing;
            drawing.Nodes.Add(rectangle1);

            if (rectangle0.Pins?[1] is { } && rectangle1.Pins?[0] is { })
            {
                var connector0 = NodeFactory.CreateConnector(rectangle0.Pins[1], rectangle1.Pins[0]);
                connector0.Parent = drawing;
                drawing.Connectors.Add(connector0);
            }

            var rectangle2 = NodeFactory.CreateRectangle(30, 130, 60, 60, "rect2");
            rectangle2.Parent = drawing;
            drawing.Nodes.Add(rectangle2);

            var ellipse0 = NodeFactory.CreateEllipse(220, 130, 60, 60, "ellipse0");
            ellipse0.Parent = drawing;
            drawing.Nodes.Add(ellipse0);

            var signal0 = NodeFactory.CreateSignal(x: 30, y: 270, label: "in0", state: true);
            signal0.Parent = drawing;
            drawing.Nodes.Add(signal0);
  
            var signal1 = NodeFactory.CreateSignal(x: 30, y: 390, label: "in1", state: false);
            signal1.Parent = drawing;
            drawing.Nodes.Add(signal1);

            var signal2 = NodeFactory.CreateSignal(x: 360, y: 375, label: "out0", state: true);
            signal2.Parent = drawing;
            drawing.Nodes.Add(signal2);

            var orGate0 = NodeFactory.CreateOrGate(240, 360);
            orGate0.Parent = drawing;
            drawing.Nodes.Add(orGate0);
            
            if (signal0.Pins?[1] is { } && orGate0.Pins?[2] is { })
            {
                var connector0 = NodeFactory.CreateConnector(signal0.Pins[1], orGate0.Pins[2]);
                connector0.Parent = drawing;
                drawing.Connectors.Add(connector0);
            }
    
            if (signal1.Pins?[1] is { } && orGate0.Pins?[0] is { })
            {
                var connector0 = NodeFactory.CreateConnector(signal1.Pins[1], orGate0.Pins[0]);
                connector0.Parent = drawing;
                drawing.Connectors.Add(connector0);
            }

            if (orGate0.Pins?[1] is { } && signal2.Pins?[0] is { })
            {
                var connector1 = NodeFactory.CreateConnector(orGate0.Pins[1], signal2.Pins[0]);
                connector1.Parent = drawing;
                drawing.Connectors.Add(connector1);
            }

            return drawing;
        }
    }
}
