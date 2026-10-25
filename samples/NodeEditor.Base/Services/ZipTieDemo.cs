using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorDemo.ViewModels;
using NodeEditorDemo.ViewModels.Nodes;

namespace NodeEditorDemo.Services;

/// <summary>Creates an editable six-wire bundle using the same public APIs as a host application.</summary>
public static class ZipTieDemo
{
    /// <summary>Creates two socket nodes and six independent wires in a selected zip-tie bundle.</summary>
    public static DrawingNodeViewModel CreateDrawing()
    {
        var drawing = new DrawingNodeViewModel
        {
            Name = "Wire zip ties",
            Width = 1020,
            Height = 540,
            Nodes = new ObservableCollection<INode>(),
            Connectors = new ObservableCollection<IConnector>(),
            Settings = new DrawingNodeSettingsViewModel
            {
                EnableGrid = false,
                EnableSnap = false,
                SnapX = 10,
                SnapY = 10,
                EnableGuides = true,
                EnableConnectorRouting = false,
                EnableMultiplePinConnections = true,
                RequireDirectionalConnections = true,
                AllowDuplicateConnections = false,
                DefaultConnectorStyle = ConnectorStyle.Bezier
            }
        };

        var source = CreateSocket(drawing, 60, 100, "Source", "OUTPUT CHANNELS", PinAlignment.Right);
        var destination = CreateSocket(drawing, 790, 140, "Destination", "INPUT CHANNELS", PinAlignment.Left);
        drawing.Nodes.Add(source);
        drawing.Nodes.Add(destination);

        for (var i = 0; i < 6; ++i)
        {
            drawing.Connectors.Add(new WireConnectorViewModel
            {
                Parent = drawing,
                Channel = i,
                Start = source.Pins![i],
                End = destination.Pins![i],
                Style = ConnectorStyle.Bezier,
                RoutingMode = ConnectorRoutingMode.Manual,
                Offset = 85
            });
        }

        var tie = drawing.CreateZipTie(drawing.Connectors);
        if (tie is not null)
        {
            tie.Name = "Six-channel harness";
            tie.Start = new ConnectorPoint(350, 270);
            tie.End = new ConnectorPoint(670, 270);
            tie.Spacing = 7;
        }

        return drawing;
    }

    private static NodeViewModel CreateSocket(IDrawingNode parent, double x, double y,
        string title, string subtitle, PinAlignment alignment)
    {
        var node = new NodeViewModel
        {
            Parent = parent,
            X = x,
            Y = y,
            Width = 170,
            Height = 292,
            Content = new WireSocketViewModel { Title = title, Subtitle = subtitle }
        };

        string[] labels = { "Grayscale", "Color", "SDF 2D", "SDF 3D", "RGBA", "Texture 3D" };
        for (var i = 0; i < labels.Length; ++i)
        {
            var pin = node.AddPin(alignment == PinAlignment.Right ? node.Width : 0,
                92 + i * 32, 10, 10, alignment, labels[i]);
            if (pin is PinViewModel vm)
            {
                vm.Direction = alignment == PinAlignment.Right ? PinDirection.Output : PinDirection.Input;
            }
        }

        return node;
    }
}
