using System;

namespace NodeEditor.Model;

public interface IConnector
{
    string? Name { get; set; }
    IDrawingNode? Parent { get; set; }
    ConnectorOrientation Orientation { get; set; }
    IPin? Start { get; set; }
    IPin? End { get; set; }
    double Offset { get; set; }
    bool CanSelect();
    bool CanRemove();
    event EventHandler<ConnectorCreatedEventArgs>? Created;
    event EventHandler<ConnectorRemovedEventArgs>? Removed;
    event EventHandler<ConnectorMovedEventArgs>? Moved;
    event EventHandler<ConnectorSelectedEventArgs>? Selected;
    event EventHandler<ConnectorDeselectedEventArgs>? Deselected;
    event EventHandler<ConnectorResizedEventArgs>? Resized;
    event EventHandler<ConnectorStartChangedEventArgs>? Connected;
    event EventHandler<ConnectorEndChangedEventArgs>? Disconnected;
    void OnCreated();
    void OnRemoved();
    void OnMoved();
    void OnSelected();
    void OnDeselected();
    void OnResized();
    void OnStartChanged();
    void OnEndChanged();
}
