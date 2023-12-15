/*
 * NodeEditor A node editor control for Avalonia.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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
