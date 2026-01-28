using System.Collections.Generic;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using Xunit;

namespace NodeEditor.UnitTests;

public class ConnectionRulesTests
{
    [Fact]
    public void DisallowsSelfConnectionsWhenDisabled()
    {
        var drawing = new DrawingNodeViewModel();
        drawing.Settings.AllowSelfConnections = false;
        drawing.Settings.EnableConnections = true;
        drawing.Settings.EnableMultiplePinConnections = true;

        var pinA = new PinViewModel { X = 0, Y = 0, Width = 10, Height = 10 };
        var pinB = new PinViewModel { X = 20, Y = 0, Width = 10, Height = 10 };
        var node = CreateNode(0, 0, pinA, pinB);
        node.Parent = drawing;
        drawing.Nodes = new List<INode> { node };

        drawing.ConnectorLeftPressed(pinA, showWhenMoving: false);
        drawing.ConnectorLeftPressed(pinB, showWhenMoving: false);

        Assert.True(drawing.Connectors is null || drawing.Connectors.Count == 0);
    }

    [Fact]
    public void DisallowsDuplicateConnectionsWhenDisabled()
    {
        var drawing = new DrawingNodeViewModel();
        drawing.Settings.AllowDuplicateConnections = false;
        drawing.Settings.EnableConnections = true;
        drawing.Settings.EnableMultiplePinConnections = true;

        var pinA = new PinViewModel { X = 0, Y = 0, Width = 10, Height = 10 };
        var pinB = new PinViewModel { X = 0, Y = 0, Width = 10, Height = 10 };
        var nodeA = CreateNode(0, 0, pinA);
        var nodeB = CreateNode(100, 0, pinB);
        nodeA.Parent = drawing;
        nodeB.Parent = drawing;
        drawing.Nodes = new List<INode> { nodeA, nodeB };

        drawing.ConnectorLeftPressed(pinA, showWhenMoving: false);
        drawing.ConnectorLeftPressed(pinB, showWhenMoving: false);

        Assert.Equal(1, drawing.Connectors?.Count ?? 0);

        drawing.ConnectorLeftPressed(pinA, showWhenMoving: false);
        drawing.ConnectorLeftPressed(pinB, showWhenMoving: false);

        Assert.Equal(1, drawing.Connectors?.Count ?? 0);
    }

    [Fact]
    public void ConnectorStartChangedRaisesEvent()
    {
        var connector = new ConnectorViewModel();
        var pin = new PinViewModel();
        var fired = false;

        connector.StartChanged += (_, _) => fired = true;
        connector.Start = pin;

        Assert.True(fired);
    }

    [Fact]
    public void CustomValidatorCanBlockConnections()
    {
        var drawing = new DrawingNodeViewModel();
        drawing.Settings.EnableConnections = true;
        drawing.Settings.EnableMultiplePinConnections = true;
        drawing.Settings.ConnectionValidator = context => false;

        var pinA = new PinViewModel { X = 0, Y = 0, Width = 10, Height = 10 };
        var pinB = new PinViewModel { X = 0, Y = 0, Width = 10, Height = 10 };
        var nodeA = CreateNode(0, 0, pinA);
        var nodeB = CreateNode(100, 0, pinB);
        nodeA.Parent = drawing;
        nodeB.Parent = drawing;
        drawing.Nodes = new List<INode> { nodeA, nodeB };

        drawing.ConnectorLeftPressed(pinA, showWhenMoving: false);
        drawing.ConnectorLeftPressed(pinB, showWhenMoving: false);

        Assert.True(drawing.Connectors is null || drawing.Connectors.Count == 0);
    }

    [Fact]
    public void PinViewModelDefaultsConnectableProperties()
    {
        var pin = new PinViewModel();
        var connectable = Assert.IsAssignableFrom<IConnectablePin>(pin);

        Assert.Equal(PinDirection.Bidirectional, connectable.Direction);
        Assert.Equal(1, connectable.BusWidth);
    }

    private static NodeViewModel CreateNode(double x, double y, params PinViewModel[] pins)
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 100,
            Height = 100,
            Pins = new List<IPin>(pins)
        };

        foreach (var pin in pins)
        {
            pin.Parent = node;
        }

        return node;
    }
}
