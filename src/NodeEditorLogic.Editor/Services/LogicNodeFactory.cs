using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorLogic.Models;
using NodeEditorLogic.ViewModels;
using NodeEditorLogic.ViewModels.Nodes;

namespace NodeEditorLogic.Services;

public sealed class LogicNodeFactory : INodeFactory
{
    private const double DefaultNodeWidth = 180;
    private const double InputNodeWidth = 170;
    private const double OutputNodeWidth = 170;
    private const double ClockNodeWidth = 190;
    private const double FlipFlopNodeWidth = 200;
    private const double BusNodeWidth = 200;
    private const double BusNodeHeight = 120;
    private const double NoteNodeWidth = 220;
    private const double NoteNodeHeight = 140;
    private const double MinNodeHeight = 64;
    private const double PortPaddingTop = 30;
    private const double PortPaddingBottom = 18;
    private const double PortSpacing = 18;
    private const double PinSize = 10;

    private readonly LogicComponentLibrary _library;

    public LogicNodeFactory(LogicComponentLibrary library)
    {
        _library = library;
    }

    public IDrawingNode CreateDrawing(string? name = null)
    {
        var settings = new DrawingNodeSettingsViewModel
        {
            EnableMultiplePinConnections = true,
            EnableSnap = true,
            SnapX = 15.0,
            SnapY = 15.0,
            EnableGrid = true,
            GridCellWidth = 15.0,
            GridCellHeight = 15.0,
            DefaultConnectorStyle = ConnectorStyle.Orthogonal,
            RequireDirectionalConnections = true,
            RequireMatchingBusWidth = true,
            ConnectionValidator = LogicConnectionValidation.TypeCompatibility
        };

        var drawing = new DrawingNodeViewModel(LogicDrawingNodeFactory.Instance)
        {
            Settings = settings,
            Name = name,
            X = 0,
            Y = 0,
            Width = 1100,
            Height = 900,
            Nodes = new ObservableCollection<INode>(),
            Connectors = new ObservableCollection<IConnector>(),
        };

        return drawing;
    }

    public IList<INodeTemplate> CreateTemplates()
    {
        var templates = new ObservableCollection<INodeTemplate>();
        foreach (var category in CreateCategories())
        {
            foreach (var template in category.Templates)
            {
                templates.Add(template);
            }
        }
        return templates;
    }

    public IList<ToolboxCategoryViewModel> CreateCategories()
    {
        var categories = new List<ToolboxCategoryViewModel>
        {
            new ToolboxCategoryViewModel
            {
                Title = "Inputs",
                Templates = new ObservableCollection<INodeTemplate>
                {
                    CreateTemplate("Input", CreateInputNode(0, 0, "A")),
                    CreateTemplate("Clock", CreateClockNode(0, 0)),
                    CreateTemplate("Bus Input", CreateBusInputNode(0, 0))
                }
            },
            new ToolboxCategoryViewModel
            {
                Title = "Outputs",
                Templates = new ObservableCollection<INodeTemplate>
                {
                    CreateTemplate("Output", CreateOutputNode(0, 0, "Q")),
                    CreateTemplate("Bus Output", CreateBusOutputNode(0, 0))
                }
            },
            new ToolboxCategoryViewModel
            {
                Title = "Gates",
                Templates = new ObservableCollection<INodeTemplate>
                {
                    CreateGateTemplate("AND"),
                    CreateGateTemplate("OR"),
                    CreateGateTemplate("NOT"),
                    CreateGateTemplate("NAND"),
                    CreateGateTemplate("NOR"),
                    CreateGateTemplate("XOR"),
                    CreateGateTemplate("XNOR")
                }
            },
            new ToolboxCategoryViewModel
            {
                Title = "Memory",
                Templates = new ObservableCollection<INodeTemplate>
                {
                    CreateTemplate("D Flip-Flop", CreateFlipFlopNode(0, 0))
                }
            },
            new ToolboxCategoryViewModel
            {
                Title = "Components",
                Templates = new ObservableCollection<INodeTemplate>
                {
                    CreateComponentTemplate("Half Adder"),
                    CreateComponentTemplate("Full Adder"),
                    CreateComponentTemplate("2:1 Mux"),
                    CreateComponentTemplate("2:4 Decoder")
                }
            },
            new ToolboxCategoryViewModel
            {
                Title = "Buses",
                Templates = new ObservableCollection<INodeTemplate>
                {
                    CreateTemplate("Bus Split", CreateBusSplitNode(0, 0)),
                    CreateTemplate("Bus Merge", CreateBusMergeNode(0, 0))
                }
            },
            new ToolboxCategoryViewModel
            {
                Title = "IC Library",
                Templates = new ObservableCollection<INodeTemplate>(CreateDefinitionTemplates("IC Library"))
            },
            new ToolboxCategoryViewModel
            {
                Title = "Annotations",
                Templates = new ObservableCollection<INodeTemplate>
                {
                    CreateTemplate("Note", CreateNoteNode(0, 0))
                }
            }
        };

        return categories;
    }

    public static IConnector CreateConnector(IPin? start, IPin? end)
    {
        var connector = new LogicConnectorViewModel
        {
            Start = start,
            End = end,
            Style = ConnectorStyle.Orthogonal
        };

        if (start is LogicPinViewModel startPin && end is LogicPinViewModel endPin)
        {
            connector.BusWidth = Math.Max(startPin.BusWidth, endPin.BusWidth);
            connector.IsBus = connector.BusWidth > 1;
        }

        return connector;
    }

    private static NodeTemplateViewModel CreateTemplate(string title, NodeViewModel node)
    {
        return new NodeTemplateViewModel
        {
            Title = title,
            Template = node,
            Preview = CloneForPreview(node)
        };
    }

    private NodeTemplateViewModel CreateGateTemplate(string title)
    {
        var definition = GetDefinitionByTitle(title);
        var node = CreateGateNode(0, 0, definition, title);
        return CreateTemplate(title, node);
    }

    private NodeTemplateViewModel CreateComponentTemplate(string title)
    {
        var definition = GetDefinitionByTitle(title);
        var node = CreateGateNode(0, 0, definition, title);
        return CreateTemplate(title, node);
    }

    private IEnumerable<INodeTemplate> CreateDefinitionTemplates(string category)
    {
        foreach (var definition in _library.Definitions)
        {
            if (!string.Equals(definition.Category, category, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var node = CreateGateNode(0, 0, definition, definition.Title);
            yield return CreateTemplate(definition.Title, node);
        }
    }

    private LogicComponentDefinition? GetDefinitionByTitle(string title)
    {
        foreach (var definition in _library.Definitions)
        {
            if (string.Equals(definition.Title, title, StringComparison.OrdinalIgnoreCase))
            {
                return definition;
            }
        }

        return null;
    }

    public LogicComponentDefinition? GetDefinition(string id)
    {
        return _library.Get(id);
    }

    private static NodeViewModel CloneForPreview(NodeViewModel node)
    {
        var clone = new NodeViewModel
        {
            Name = node.Name,
            X = 0,
            Y = 0,
            Width = node.Width,
            Height = node.Height,
            Pins = new ObservableCollection<IPin>(),
            Content = node.Content
        };

        if (node.Pins is null)
        {
            return clone;
        }

        foreach (var pin in node.Pins)
        {
            if (pin is LogicPinViewModel logicPin)
            {
                clone.Pins.Add(new LogicPinViewModel
                {
                    Name = logicPin.Name,
                    Parent = clone,
                    X = logicPin.X,
                    Y = logicPin.Y,
                    Width = logicPin.Width,
                    Height = logicPin.Height,
                    Alignment = logicPin.Alignment,
                    Kind = logicPin.Kind,
                    Value = logicPin.Value,
                    BusWidth = logicPin.BusWidth,
                    BusValue = CopyBusValue(logicPin.BusValue)
                });
            }
        }

        return clone;
    }

    public NodeViewModel CreateInputNode(double x, double y, string label)
    {
        var content = new LogicInputNodeViewModel
        {
            Title = "Input",
            Subtitle = "Source",
            ComponentId = "input",
            PropagationDelay = 0,
            Label = label,
            IsOn = false
        };

        return CreateNode(x, y, InputNodeWidth, content, Array.Empty<string>(), new[] { "Out" });
    }

    public NodeViewModel CreateOutputNode(double x, double y, string label)
    {
        var content = new LogicOutputNodeViewModel
        {
            Title = "Output",
            Subtitle = "Probe",
            ComponentId = "output",
            PropagationDelay = 0,
            Label = label
        };

        return CreateNode(x, y, OutputNodeWidth, content, new[] { "In" }, Array.Empty<string>());
    }

    public NodeViewModel CreateBusInputNode(double x, double y, int busWidth = 4, string label = "BUS")
    {
        var content = new LogicBusInputNodeViewModel
        {
            Title = "Bus Input",
            Subtitle = "Source",
            ComponentId = "bus-input",
            PropagationDelay = 0,
            Label = label,
            BusWidth = busWidth,
            BusValue = 0
        };

        var node = new NodeViewModel
        {
            Name = content.Title,
            X = x,
            Y = y,
            Width = BusNodeWidth,
            Height = BusNodeHeight,
            Pins = new ObservableCollection<IPin>(),
            Content = content
        };

        content.HostNode = node;
        content.InputPins = new ObservableCollection<LogicPinViewModel>();
        content.OutputPins = new ObservableCollection<LogicPinViewModel>();

        var pin = CreatePin(node, node.Width, PortPaddingTop, PinAlignment.Right, "BUS", LogicPinKind.Output, content.BusWidth);
        content.OutputPins.Add(pin);

        return node;
    }

    public NodeViewModel CreateBusOutputNode(double x, double y, int busWidth = 4, string label = "BUS")
    {
        var content = new LogicBusOutputNodeViewModel
        {
            Title = "Bus Output",
            Subtitle = "Probe",
            ComponentId = "bus-output",
            PropagationDelay = 0,
            Label = label,
            BusWidth = busWidth
        };

        var node = new NodeViewModel
        {
            Name = content.Title,
            X = x,
            Y = y,
            Width = BusNodeWidth,
            Height = BusNodeHeight,
            Pins = new ObservableCollection<IPin>(),
            Content = content
        };

        content.HostNode = node;
        content.InputPins = new ObservableCollection<LogicPinViewModel>();
        content.OutputPins = new ObservableCollection<LogicPinViewModel>();

        var pin = CreatePin(node, 0, PortPaddingTop, PinAlignment.Left, "BUS", LogicPinKind.Input, content.BusWidth);
        content.InputPins.Add(pin);

        return node;
    }

    public NodeViewModel CreateBusSplitNode(double x, double y, int bitCount = 4)
    {
        var content = new LogicBusSplitNodeViewModel
        {
            Title = "Bus Split",
            Subtitle = "Bundle",
            ComponentId = "bus-split",
            PropagationDelay = 0,
            BitCount = bitCount
        };

        var node = new NodeViewModel
        {
            Name = content.Title,
            X = x,
            Y = y,
            Width = BusNodeWidth,
            Height = Math.Max(MinNodeHeight, PortPaddingTop + PortPaddingBottom + bitCount * PortSpacing),
            Pins = new ObservableCollection<IPin>(),
            Content = content
        };

        content.HostNode = node;
        content.InputPins = new ObservableCollection<LogicPinViewModel>();
        content.OutputPins = new ObservableCollection<LogicPinViewModel>();

        RefreshBusSplitPins(node, content);

        return node;
    }

    public NodeViewModel CreateBusMergeNode(double x, double y, int bitCount = 4)
    {
        var content = new LogicBusMergeNodeViewModel
        {
            Title = "Bus Merge",
            Subtitle = "Bundle",
            ComponentId = "bus-merge",
            PropagationDelay = 0,
            BitCount = bitCount
        };

        var node = new NodeViewModel
        {
            Name = content.Title,
            X = x,
            Y = y,
            Width = BusNodeWidth,
            Height = Math.Max(MinNodeHeight, PortPaddingTop + PortPaddingBottom + bitCount * PortSpacing),
            Pins = new ObservableCollection<IPin>(),
            Content = content
        };

        content.HostNode = node;
        content.InputPins = new ObservableCollection<LogicPinViewModel>();
        content.OutputPins = new ObservableCollection<LogicPinViewModel>();

        RefreshBusMergePins(node, content);

        return node;
    }

    public NodeViewModel CreateClockNode(double x, double y)
    {
        var content = new LogicClockNodeViewModel
        {
            Title = "Clock",
            Subtitle = "Pulse",
            ComponentId = "clock",
            PropagationDelay = 0,
            IsRunning = false,
            Period = 2,
            Counter = 0,
            State = LogicValue.Low,
            HighTicks = 1
        };

        return CreateNode(x, y, ClockNodeWidth, content, Array.Empty<string>(), new[] { "Clk" });
    }

    public NodeViewModel CreateFlipFlopNode(double x, double y)
    {
        var content = new LogicFlipFlopNodeViewModel
        {
            Title = "D Flip-Flop",
            Subtitle = "Memory",
            ComponentId = "dff",
            PropagationDelay = 1,
            StoredValue = LogicValue.Low,
            LastClockValue = LogicValue.Low
        };

        return CreateNode(x, y, FlipFlopNodeWidth, content, new[] { "D", "Clk" }, new[] { "Q", "Qn" });
    }

    public NodeViewModel CreateNoteNode(double x, double y)
    {
        var content = new LogicNoteNodeViewModel
        {
            Text = "Note"
        };

        return new NodeViewModel
        {
            Name = "Note",
            X = x,
            Y = y,
            Width = NoteNodeWidth,
            Height = NoteNodeHeight,
            Pins = new ObservableCollection<IPin>(),
            Content = content
        };
    }

    public NodeViewModel CreateGateNode(double x, double y, LogicComponentDefinition? definition, string? fallbackTitle = null)
    {
        if (definition is null)
        {
            var title = fallbackTitle ?? "Component";
            var fallback = new LogicGateNodeViewModel
            {
                Title = title,
                Subtitle = "Component",
                ComponentId = title.ToLowerInvariant().Replace(' ', '-'),
                PropagationDelay = 1
            };

            return CreateNode(x, y, DefaultNodeWidth, fallback, new[] { "In" }, new[] { "Out" });
        }

        var content = new LogicGateNodeViewModel
        {
            Title = definition.Title,
            Subtitle = definition.Category,
            ComponentId = definition.Id,
            PropagationDelay = definition.PropagationDelay
        };

        return CreateNode(x, y, DefaultNodeWidth, content, definition.Inputs, definition.Outputs);
    }

    private static NodeViewModel CreateNode(
        double x,
        double y,
        double width,
        LogicNodeContentViewModel content,
        IReadOnlyList<string> inputLabels,
        IReadOnlyList<string> outputLabels)
    {
        var portCount = Math.Max(inputLabels.Count, outputLabels.Count);
        var height = Math.Max(MinNodeHeight, PortPaddingTop + PortPaddingBottom + portCount * PortSpacing);

        var node = new NodeViewModel
        {
            Name = content.Title,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = content
        };

        content.HostNode = node;
        content.InputPins = new ObservableCollection<LogicPinViewModel>();
        content.OutputPins = new ObservableCollection<LogicPinViewModel>();

        for (var i = 0; i < inputLabels.Count; i++)
        {
            var pin = CreatePin(node, 0, PortPaddingTop + i * PortSpacing, PinAlignment.Left, inputLabels[i], LogicPinKind.Input, 1);
            content.InputPins.Add(pin);
        }

        for (var i = 0; i < outputLabels.Count; i++)
        {
            var pin = CreatePin(node, width, PortPaddingTop + i * PortSpacing, PinAlignment.Right, outputLabels[i], LogicPinKind.Output, 1);
            content.OutputPins.Add(pin);
        }

        return node;
    }

    public static void RefreshNodeLayout(NodeViewModel node, LogicNodeContentViewModel content)
    {
        var portCount = Math.Max(content.InputPins.Count, content.OutputPins.Count);
        var height = Math.Max(MinNodeHeight, PortPaddingTop + PortPaddingBottom + portCount * PortSpacing);
        node.Height = height;

        for (var i = 0; i < content.InputPins.Count; i++)
        {
            var pin = content.InputPins[i];
            pin.Parent = node;
            pin.Alignment = PinAlignment.Left;
            pin.X = 0;
            pin.Y = PortPaddingTop + i * PortSpacing;
        }

        for (var i = 0; i < content.OutputPins.Count; i++)
        {
            var pin = content.OutputPins[i];
            pin.Parent = node;
            pin.Alignment = PinAlignment.Right;
            pin.X = node.Width;
            pin.Y = PortPaddingTop + i * PortSpacing;
        }
    }

    public static void RefreshBusInputPins(NodeViewModel node, LogicBusInputNodeViewModel content)
    {
        content.HostNode = node;
        node.Pins ??= new ObservableCollection<IPin>();

        var pin = content.OutputPins.Count > 0 ? content.OutputPins[0] : null;
        if (pin is null)
        {
            pin = CreatePin(node, node.Width, PortPaddingTop, PinAlignment.Right, "BUS", LogicPinKind.Output, content.BusWidth);
            content.OutputPins.Add(pin);
        }
        else
        {
            pin.BusWidth = content.BusWidth;
            pin.Name = "BUS";
            var pinSize = content.BusWidth > 1 ? PinSize + 4 : PinSize;
            pin.Width = pinSize;
            pin.Height = pinSize;
        }

        RefreshNodeLayout(node, content);
    }

    public static void RefreshBusOutputPins(NodeViewModel node, LogicBusOutputNodeViewModel content)
    {
        content.HostNode = node;
        node.Pins ??= new ObservableCollection<IPin>();

        var pin = content.InputPins.Count > 0 ? content.InputPins[0] : null;
        if (pin is null)
        {
            pin = CreatePin(node, 0, PortPaddingTop, PinAlignment.Left, "BUS", LogicPinKind.Input, content.BusWidth);
            content.InputPins.Add(pin);
        }
        else
        {
            pin.BusWidth = content.BusWidth;
            pin.Name = "BUS";
            var pinSize = content.BusWidth > 1 ? PinSize + 4 : PinSize;
            pin.Width = pinSize;
            pin.Height = pinSize;
        }

        RefreshNodeLayout(node, content);
    }

    public static void RefreshBusSplitPins(NodeViewModel node, LogicBusSplitNodeViewModel content)
    {
        content.HostNode = node;
        node.Pins ??= new ObservableCollection<IPin>();

        var busPin = content.InputPins.Count > 0 ? content.InputPins[0] : null;
        if (busPin is null)
        {
            busPin = CreatePin(node, 0, PortPaddingTop, PinAlignment.Left, "BUS", LogicPinKind.Input, content.BitCount);
            content.InputPins.Add(busPin);
        }
        else
        {
            busPin.BusWidth = content.BitCount;
            busPin.Name = "BUS";
            var pinSize = content.BitCount > 1 ? PinSize + 4 : PinSize;
            busPin.Width = pinSize;
            busPin.Height = pinSize;
        }

        SyncPins(node, content.OutputPins, content.BitCount, LogicPinKind.Output, "B");

        RefreshNodeLayout(node, content);
    }

    public static void RefreshBusMergePins(NodeViewModel node, LogicBusMergeNodeViewModel content)
    {
        content.HostNode = node;
        node.Pins ??= new ObservableCollection<IPin>();

        var busPin = content.OutputPins.Count > 0 ? content.OutputPins[0] : null;
        if (busPin is null)
        {
            busPin = CreatePin(node, node.Width, PortPaddingTop, PinAlignment.Right, "BUS", LogicPinKind.Output, content.BitCount);
            content.OutputPins.Add(busPin);
        }
        else
        {
            busPin.BusWidth = content.BitCount;
            busPin.Name = "BUS";
            var pinSize = content.BitCount > 1 ? PinSize + 4 : PinSize;
            busPin.Width = pinSize;
            busPin.Height = pinSize;
        }

        SyncPins(node, content.InputPins, content.BitCount, LogicPinKind.Input, "B");

        RefreshNodeLayout(node, content);
    }

    private static void SyncPins(NodeViewModel node, ObservableCollection<LogicPinViewModel> pins, int count, LogicPinKind kind, string baseName)
    {
        var clamped = Math.Max(1, Math.Min(16, count));

        while (pins.Count > clamped)
        {
            var pin = pins[^1];
            pins.RemoveAt(pins.Count - 1);
            RemovePin(node, pin);
        }

        while (pins.Count < clamped)
        {
            var index = pins.Count;
            var name = $"{baseName}{index}";
            var pin = CreatePin(node, 0, 0, kind == LogicPinKind.Input ? PinAlignment.Left : PinAlignment.Right, name, kind, 1);
            pins.Add(pin);
        }

        for (var i = 0; i < pins.Count; i++)
        {
            pins[i].Name = $"{baseName}{i}";
            pins[i].Kind = kind;
            pins[i].Alignment = kind == LogicPinKind.Input ? PinAlignment.Left : PinAlignment.Right;
            pins[i].Parent = node;
        }
    }

    private static void RemovePin(NodeViewModel node, LogicPinViewModel pin)
    {
        node.Pins?.Remove(pin);
        pin.OnRemoved();

        if (node.Parent is not IDrawingNode drawing || drawing.Connectors is null)
        {
            return;
        }

        for (var i = drawing.Connectors.Count - 1; i >= 0; i--)
        {
            var connector = drawing.Connectors[i];
            if (connector.Start == pin || connector.End == pin)
            {
                connector.Start?.OnDisconnected();
                connector.End?.OnDisconnected();
                connector.OnRemoved();
                drawing.Connectors.RemoveAt(i);
            }
        }
    }

    private static LogicPinViewModel CreatePin(
        NodeViewModel node,
        double x,
        double y,
        PinAlignment alignment,
        string name,
        LogicPinKind kind,
        int busWidth)
    {
        var pinSize = busWidth > 1 ? PinSize + 4 : PinSize;
        var pin = new LogicPinViewModel
        {
            Name = name,
            Parent = node,
            X = x,
            Y = y,
            Width = pinSize,
            Height = pinSize,
            Alignment = alignment,
            Kind = kind,
            Value = LogicValue.Unknown,
            BusWidth = busWidth,
            BusValue = CreateUnknownBus(busWidth)
        };

        node.Pins ??= new ObservableCollection<IPin>();
        node.Pins.Add(pin);

        return pin;
    }

    private static LogicValue[] CreateUnknownBus(int width)
    {
        var clamped = Math.Max(1, width);
        var values = new LogicValue[clamped];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = LogicValue.Unknown;
        }

        return values;
    }

    private static LogicValue[] CopyBusValue(LogicValue[] source)
    {
        if (source.Length == 0)
        {
            return Array.Empty<LogicValue>();
        }

        var copy = new LogicValue[source.Length];
        Array.Copy(source, copy, source.Length);
        return copy;
    }
}
