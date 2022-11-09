using System;

namespace NodeEditor.Model;

public interface IPin
{
    string? Name { get; set; }
    INode? Parent { get; set; }
    double X { get; set; }
    double Y { get; set; }
    double Width { get; set; }
    double Height { get; set; }
    PinAlignment Alignment { get; set; }
    bool CanConnect();
    bool CanDisconnect();
    event EventHandler<PinCreatedEventArgs>? Created;
    event EventHandler<PinRemovedEventArgs>? Removed;
    event EventHandler<PinMovedEventArgs>? Moved;
    event EventHandler<PinSelectedEventArgs>? Selected;
    event EventHandler<PinDeselectedEventArgs>? Deselected;
    event EventHandler<PinResizedEventArgs>? Resized;
    event EventHandler<PinConnectedEventArgs>? Connected;
    event EventHandler<PinDisconnectedEventArgs>? Disconnected;
    void OnCreated();
    void OnRemoved();
    void OnMoved();
    void OnSelected();
    void OnDeselected();
    void OnResized();
    void OnConnected();
    void OnDisconnected();
}
