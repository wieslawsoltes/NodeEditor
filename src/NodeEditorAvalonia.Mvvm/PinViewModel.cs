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
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

[ObservableObject]
public partial class PinViewModel : IPin
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private INode? _parent;
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private double _width;
    [ObservableProperty] private double _height;
    [ObservableProperty] private PinAlignment _alignment;

    public event EventHandler<PinCreatedEventArgs>? Created;

    public event EventHandler<PinRemovedEventArgs>? Removed;

    public event EventHandler<PinMovedEventArgs>? Moved;

    public event EventHandler<PinSelectedEventArgs>? Selected;

    public event EventHandler<PinDeselectedEventArgs>? Deselected;

    public event EventHandler<PinResizedEventArgs>? Resized;

    public event EventHandler<PinConnectedEventArgs>? Connected;

    public event EventHandler<PinDisconnectedEventArgs>? Disconnected;

    public virtual bool CanConnect()
    {
        return true;
    }

    public virtual bool CanDisconnect()
    {
        return true;
    }

    public void OnCreated()
    {
        Created?.Invoke(this, new PinCreatedEventArgs(this));
    }

    public void OnRemoved()
    {
        Removed?.Invoke(this, new PinRemovedEventArgs(this));
    }

    public void OnMoved()
    {
        Moved?.Invoke(this, new PinMovedEventArgs(this, _x, _y));
    }

    public void OnSelected()
    {
        Selected?.Invoke(this, new PinSelectedEventArgs(this));
    }

    public void OnDeselected()
    {
        Deselected?.Invoke(this, new PinDeselectedEventArgs(this));
    }

    public void OnResized()
    {
        Resized?.Invoke(this, new PinResizedEventArgs(this, _x, _y, _width, _height));
    }

    public void OnConnected()
    {
        Connected?.Invoke(this, new PinConnectedEventArgs(this));
    }

    public void OnDisconnected()
    {
        Disconnected?.Invoke(this, new PinDisconnectedEventArgs(this));
    }
}
