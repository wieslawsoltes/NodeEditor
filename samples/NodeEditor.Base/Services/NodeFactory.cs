using System.Collections.Generic;
using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorDemo.ViewModels.Nodes;

namespace NodeEditorDemo.Services;

public class NodeFactory : INodeFactory
{
    private const string ShapePinType = "shape";
    private const string SignalPinType = "sig";
    internal static INode CreateRectangle(double x, double y, double width, double height, string? label, double pinSize = 10)
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

        AddTypedPin(node, 0, height / 2, pinSize, pinSize, PinAlignment.Left, "L", ShapePinType);
        AddTypedPin(node, width, height / 2, pinSize, pinSize, PinAlignment.Right, "R", ShapePinType);
        AddTypedPin(node, width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T", ShapePinType);
        AddTypedPin(node, width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B", ShapePinType);

        return node;
    }

    internal static INode CreateEllipse(double x, double y, double width, double height, string? label, double pinSize = 10)
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

        AddTypedPin(node, 0, height / 2, pinSize, pinSize, PinAlignment.Left, "L", ShapePinType);
        AddTypedPin(node, width, height / 2, pinSize, pinSize, PinAlignment.Right, "R", ShapePinType);
        AddTypedPin(node, width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T", ShapePinType);
        AddTypedPin(node, width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B", ShapePinType);
            
        return node;
    }

    internal static INode CreateSignal(double x, double y, double width = 180, double height = 30, string? label = null, bool? state = false, double pinSize = 10, string name = "SIGNAL")
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

        AddTypedPin(node, 0, height / 2, pinSize, pinSize, PinAlignment.Left, "IN", SignalPinType, PinDirection.Input);
        AddTypedPin(node, width, height / 2, pinSize, pinSize, PinAlignment.Right, "OUT", SignalPinType, PinDirection.Output);
  
        return node;
    }

    internal static INode CreateAndGate(double x, double y, double width = 60, double height = 60, double pinSize = 10, string name = "AND")
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

        AddTypedPin(node, 0, height / 2, pinSize, pinSize, PinAlignment.Left, "L", SignalPinType, PinDirection.Input);
        AddTypedPin(node, width, height / 2, pinSize, pinSize, PinAlignment.Right, "R", SignalPinType, PinDirection.Output);
        AddTypedPin(node, width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T", SignalPinType, PinDirection.Input);
        AddTypedPin(node, width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B", SignalPinType, PinDirection.Input);

        return node;
    }

    internal static INode CreateOrGate(double x, double y, double width = 60, double height = 60, int count = 1, double pinSize = 10, string name = "OR")
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

        AddTypedPin(node, 0, height / 2, pinSize, pinSize, PinAlignment.Left, "L", SignalPinType, PinDirection.Input);
        AddTypedPin(node, width, height / 2, pinSize, pinSize, PinAlignment.Right, "R", SignalPinType, PinDirection.Output);
        AddTypedPin(node, width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T", SignalPinType, PinDirection.Input);
        AddTypedPin(node, width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B", SignalPinType, PinDirection.Input);

        return node;
    }

    internal static IConnector CreateConnector(IPin? start, IPin? end)
    {
        return new ConnectorViewModel
        { 
            Start = start,
            End = end
        };
    }

    public IDrawingNode CreateDrawing(string? name = null)
    {
        var settings = new DrawingNodeSettingsViewModel()
        {
            EnableMultiplePinConnections = true,
            EnableSnap = true,
            SnapX = 15.0,
            SnapY = 15.0,
            EnableGrid = true,
            GridCellWidth = 15.0,
            GridCellHeight = 15.0,
            ConnectionValidator = BaseConnectionValidation.TypeCompatibility
        };

        var drawing = new DrawingNodeViewModel
        {
            Settings = settings,
            Name = name,
            X = 0,
            Y = 0,
            Width = 900,
            Height = 600,
            Nodes = new ObservableCollection<INode>(),
            Connectors = new ObservableCollection<IConnector>(),
        };

        return drawing;
    }

    private static IPin AddTypedPin(
        NodeViewModel node,
        double x,
        double y,
        double width,
        double height,
        PinAlignment alignment,
        string name,
        string? typeTag,
        PinDirection? direction = null)
    {
        var pin = node.AddPin(x, y, width, height, alignment, FormatPinName(typeTag, name));
        if (direction.HasValue && pin is PinViewModel viewModel)
        {
            viewModel.Direction = direction.Value;
        }

        return pin;
    }

    private static string FormatPinName(string? typeTag, string name)
    {
        if (string.IsNullOrWhiteSpace(typeTag))
        {
            return name;
        }

        return $"{typeTag}:{name}";
    }

    public IList<INodeTemplate> CreateTemplates()
    {
        return new ObservableCollection<INodeTemplate>
        {
            new NodeTemplateViewModel
            {
                Title = "Rectangle",
                Template = CreateRectangle(0, 0, 60, 60, "rect"),
                Preview = CreateRectangle(0, 0, 60, 60, "rect")
            },
            new NodeTemplateViewModel
            {
                Title = "Ellipse",
                Template = CreateEllipse(0, 0, 60, 60, "ellipse"),
                Preview = CreateEllipse(0, 0, 60, 60, "ellipse")
            },
            new NodeTemplateViewModel
            {
                Title = "Signal",
                Template = CreateSignal(0, 0, label: "signal", state: false),
                Preview = CreateSignal(0, 0, label: "signal", state: false)
            },
            new NodeTemplateViewModel
            {
                Title = "AND Gate",
                Template = CreateAndGate(0, 0, 60, 60),
                Preview = CreateAndGate(0, 0, 60, 60)
            },
            new NodeTemplateViewModel
            {
                Title = "OR Gate",
                Template = CreateOrGate(0, 0, 60, 60),
                Preview = CreateOrGate(0, 0, 60, 60)
            }
        };
    }
}
