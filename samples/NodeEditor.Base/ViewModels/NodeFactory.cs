using System.Collections.Generic;
using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditor.ViewModels;
using NodeEditorDemo.ViewModels.Nodes;

namespace NodeEditorDemo.ViewModels;

public class NodeFactory
{
    public INode CreateRectangle(double x, double y, double width, double height, string? label, double pinSize = 8)
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = label }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    public INode CreateEllipse(double x, double y, double width, double height, string? label, double pinSize = 8)
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new EllipseViewModel { Label = label }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");
            
        return node;
    }

    public INode CreateSignal(double x, double y, double width = 120, double height = 30, string? label = null, bool? state = false, double pinSize = 8, string name = "SIGNAL")
    {
        var node = new NodeViewModel
        {
            Name = name,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new SignalViewModel { Label = label, State = state }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "IN");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "OUT");
  
        return node;
    }

    public INode CreateAndGate(double x, double y, double width = 60, double height = 60, double pinSize = 8, string name = "AND")
    {
        var node = new NodeViewModel
        {
            Name = name,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new AndGateViewModel { Label = "&" }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    public INode CreateOrGate(double x, double y, double width = 60, double height = 60, int count = 1, double pinSize = 8, string name = "OR")
    {
        var node = new NodeViewModel
        {
            Name = name,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new OrGateViewModel { Label = "≥", Count = count}
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    public IConnector CreateConnector(IPin? start, IPin? end)
    {
        return new ConnectorViewModel
        { 
            Start = start,
            End = end
        };
    }

    public IDrawingNode CreateDrawing(string? name = null)
    {
        var drawing = new DrawingNodeViewModel
        {
            Name = name,
            X = 0,
            Y = 0,
            Width = 900,
            Height = 600,
            Nodes = new ObservableCollection<INode>(),
            Connectors = new ObservableCollection<IConnector>(),
            EnableMultiplePinConnections = false
        };

        return drawing;
    }

    public IDrawingNode CreateDemoDrawing()
    {
        var drawing = new DrawingNodeViewModel
        {
            X = 0,
            Y = 0,
            Width = 900,
            Height = 600,
            Nodes = new ObservableCollection<INode>(),
            Connectors = new ObservableCollection<IConnector>(),
            EnableMultiplePinConnections = false
        };

        var rectangle0 = CreateRectangle(30, 30, 60, 60, "rect0");
        rectangle0.Parent = drawing;
        drawing.Nodes.Add(rectangle0);

        var rectangle1 = CreateRectangle(240, 30, 60, 60, "rect1");
        rectangle1.Parent = drawing;
        drawing.Nodes.Add(rectangle1);

        if (rectangle0.Pins?[1] is { } && rectangle1.Pins?[0] is { })
        {
            var connector0 = CreateConnector(rectangle0.Pins[1], rectangle1.Pins[0]);
            connector0.Parent = drawing;
            drawing.Connectors.Add(connector0);
        }

        var rectangle2 = CreateRectangle(30, 130, 60, 60, "rect2");
        rectangle2.Parent = drawing;
        drawing.Nodes.Add(rectangle2);

        var ellipse0 = CreateEllipse(240, 130, 60, 60, "ellipse0");
        ellipse0.Parent = drawing;
        drawing.Nodes.Add(ellipse0);

        var signal0 = CreateSignal(x: 30, y: 270, label: "in0", state: true);
        signal0.Parent = drawing;
        drawing.Nodes.Add(signal0);
  
        var signal1 = CreateSignal(x: 30, y: 390, label: "in1", state: false);
        signal1.Parent = drawing;
        drawing.Nodes.Add(signal1);

        var signal2 = CreateSignal(x: 360, y: 375, label: "out0", state: true);
        signal2.Parent = drawing;
        drawing.Nodes.Add(signal2);

        var orGate0 = CreateOrGate(240, 360);
        orGate0.Parent = drawing;
        drawing.Nodes.Add(orGate0);

        if (signal0.Pins?[1] is { } && orGate0.Pins?[2] is { })
        {
            var connector0 = CreateConnector(signal0.Pins[1], orGate0.Pins[2]);
            connector0.Parent = drawing;
            drawing.Connectors.Add(connector0);
        }

        if (signal1.Pins?[1] is { } && orGate0.Pins?[0] is { })
        {
            var connector0 = CreateConnector(signal1.Pins[1], orGate0.Pins[0]);
            connector0.Parent = drawing;
            drawing.Connectors.Add(connector0);
        }

        if (orGate0.Pins?[1] is { } && signal2.Pins?[0] is { })
        {
            var connector1 = CreateConnector(orGate0.Pins[1], signal2.Pins[0]);
            connector1.Parent = drawing;
            drawing.Connectors.Add(connector1);
        }

        return drawing;
    }
        
    public IList<INodeTemplate> CreateTemplates()
    {
        return new ObservableCollection<INodeTemplate>
        {
            new NodeTemplateViewModel
            {
                Title = "Rectangle",
                Build = (x, y) => CreateRectangle(x, y, 60, 60, "rect"),
                Preview = CreateRectangle(0, 0, 60, 60, "rect")
            },
            new NodeTemplateViewModel
            {
                Title = "Ellipse",
                Build = (x, y) => CreateEllipse(x, y, 60, 60, "ellipse"),
                Preview = CreateEllipse(0, 0, 60, 60, "ellipse")
            },
            new NodeTemplateViewModel
            {
                Title = "Signal",
                Build = (x, y) => CreateSignal(x, y, label: "signal", state: false),
                Preview = CreateSignal(0, 0, label: "signal", state: false)
            },
            new NodeTemplateViewModel
            {
                Title = "AND Gate",
                Build = (x, y) => CreateAndGate(x, y, 30, 30),
                Preview = CreateAndGate(0, 0, 30, 30)
            },
            new NodeTemplateViewModel
            {
                Title = "OR Gate",
                Build = (x, y) => CreateOrGate(x, y, 30, 30),
                Preview = CreateOrGate(0, 0, 30, 30)
            }
        };
    }
}
