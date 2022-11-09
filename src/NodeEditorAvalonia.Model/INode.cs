using System;
using System.Collections.Generic;

namespace NodeEditor.Model;

public sealed class NodeCreatedEventArgs : EventArgs
{
    public INode? Node { get; }

    public NodeCreatedEventArgs(INode? node)
    {
        Node = node;
    }
}

public sealed class NodeRemovedEventArgs : EventArgs
{
    public INode? Node { get; }

    public NodeRemovedEventArgs(INode? node)
    {
        Node = node;
    }
}

public sealed class NodeMovedEventArgs : EventArgs
{
    public INode? Node { get; }

    public double X { get; }

    public double Y { get; }

    public NodeMovedEventArgs(INode? node, double x, double y)
    {
        X = x;
        Y = y;
        Node = node;
    }
}

public sealed class NodeSelectedEventArgs : EventArgs
{
    public INode? Node { get; }

    public NodeSelectedEventArgs(INode? node)
    {
        Node = node;
    }
}

public sealed class NodeDeselectedEventArgs : EventArgs
{
    public INode? Node { get; }

    public NodeDeselectedEventArgs(INode? node)
    {
        Node = node;
    }
}

public sealed class NodeResizedEventArgs : EventArgs
{
    public INode? Node { get; }

    public double X { get; }

    public double Y { get; }

    public double Width { get; }

    public double Height { get; }

    public NodeResizedEventArgs(INode? node, double x, double y, double width, double height)
    {
        Node = node;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

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
