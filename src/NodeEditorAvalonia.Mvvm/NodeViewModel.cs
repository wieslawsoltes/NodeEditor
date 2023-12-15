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
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

[ObservableObject]
public partial class NodeViewModel : INode
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private INode? _parent;
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private double _width;
    [ObservableProperty] private double _height;
    [ObservableProperty] private object? _content;
    [ObservableProperty] private IList<IPin>? _pins;

    public event EventHandler<NodeCreatedEventArgs>? Created;

    public event EventHandler<NodeRemovedEventArgs>? Removed;

    public event EventHandler<NodeMovedEventArgs>? Moved;

    public event EventHandler<NodeSelectedEventArgs>? Selected;

    public event EventHandler<NodeDeselectedEventArgs>? Deselected;

    public event EventHandler<NodeResizedEventArgs>? Resized;

    public virtual bool CanSelect()
    {
        return true;
    }

    public virtual bool CanRemove()
    {
        return true;
    }

    public virtual bool CanMove()
    {
        return true;
    }

    public virtual bool CanResize()
    {
        return true;
    }

    public virtual void Move(double deltaX, double deltaY)
    {
        X += deltaX;
        Y += deltaY;
    }

    public virtual void Resize(double deltaX, double deltaY, NodeResizeDirection direction)
    {
        // TODO: Resize
    }

    public virtual void OnCreated()
    {
        Created?.Invoke(this, new NodeCreatedEventArgs(this));
    }

    public virtual void OnRemoved()
    {
        Removed?.Invoke(this, new NodeRemovedEventArgs(this));
    }

    public virtual void OnMoved()
    {
        Moved?.Invoke(this, new NodeMovedEventArgs(this, _x, _y));
    }

    public virtual void OnSelected()
    {
        Selected?.Invoke(this, new NodeSelectedEventArgs(this));
    }

    public virtual void OnDeselected()
    {
        Deselected?.Invoke(this, new NodeDeselectedEventArgs(this));
    }

    public virtual void OnResized()
    {
        Resized?.Invoke(this, new NodeResizedEventArgs(this, _x, _y, _width, _height));
    }
}
