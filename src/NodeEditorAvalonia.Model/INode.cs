using System;
using System.Collections.Generic;

namespace NodeEditor.Model;

public interface INode
{
    string? Name { get; set; }
    INode? Parent { get; set; }
    double X { get; set; }
    double Y { get; set; }
    double Width { get; set; }
    double Height { get; set; }
    object? Content { get; set; }
    IList<IPin>? Pins { get; set; }
    bool CanSelect();
    bool CanRemove();
    bool CanMove();
    bool CanResize();
    void Move(double deltaX, double deltaY);
    void Resize(double deltaX, double deltaY, NodeResizeDirection direction);
    event EventHandler<NodeCreatedEventArgs>? Created;
    event EventHandler<NodeRemovedEventArgs>? Removed;
    event EventHandler<NodeMovedEventArgs>? Moved;
    event EventHandler<NodeSelectedEventArgs>? Selected;
    event EventHandler<NodeDeselectedEventArgs>? Deselected;
    event EventHandler<NodeResizedEventArgs>? Resized;
    void OnCreated();
    void OnRemoved();
    void OnMoved();
    void OnSelected();
    void OnDeselected();
    void OnResized();
}
