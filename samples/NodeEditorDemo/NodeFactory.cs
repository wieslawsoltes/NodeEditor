using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditor.ViewModels;
using NodeEditorDemo.ViewModels;

namespace NodeEditorDemo
{
    public class NodeFactory
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
    }
}
