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

namespace NodeEditor.Model;

public sealed class PinCreatedEventArgs : EventArgs
{
    public IPin? Pin { get; }

    public PinCreatedEventArgs(IPin? pin)
    {
        Pin = pin;
    }
}

public sealed class PinRemovedEventArgs : EventArgs
{
    public IPin? Pin { get; }

    public PinRemovedEventArgs(IPin? pin)
    {
        Pin = pin;
    }
}

public sealed class PinMovedEventArgs : EventArgs
{
    public IPin? Pin { get; }

    public double X { get; }

    public double Y { get; }

    public PinMovedEventArgs(IPin? pin, double x, double y)
    {
        X = x;
        Y = y;
        Pin = pin;
    }
}

public sealed class PinSelectedEventArgs : EventArgs
{
    public IPin? Pin { get; }

    public PinSelectedEventArgs(IPin? pin)
    {
        Pin = pin;
    }
}

public class PinDeselectedEventArgs : EventArgs
{
    public IPin? Pin { get; }

    public PinDeselectedEventArgs(IPin? pin)
    {
        Pin = pin;
    }
}

public sealed class PinResizedEventArgs : EventArgs
{
    public IPin? Pin { get; }

    public double X { get; }

    public double Y { get; }

    public double Width { get; }

    public double Height { get; }

    public PinResizedEventArgs(IPin? pin, double x, double y, double width, double height)
    {
        Pin = pin;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

public sealed class PinConnectedEventArgs : EventArgs
{
    public IPin? Pin { get; }

    public PinConnectedEventArgs(IPin? pin)
    {
        Pin = pin;
    }
}

public sealed class PinDisconnectedEventArgs : EventArgs
{
    public IPin? Pin { get; }

    public PinDisconnectedEventArgs(IPin? pin)
    {
        Pin = pin;
    }
}

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
