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

public sealed class ConnectorCreatedEventArgs : EventArgs
{
    public IConnector? Connector { get; }

    public ConnectorCreatedEventArgs(IConnector? connector)
    {
        Connector = connector;
    }
}

public sealed class ConnectorRemovedEventArgs : EventArgs
{
    public IConnector? Connector { get; }

    public ConnectorRemovedEventArgs(IConnector? connector)
    {
        Connector = connector;
    }
}

public sealed class ConnectorSelectedEventArgs : EventArgs
{
    public IConnector? Connector { get; }

    public ConnectorSelectedEventArgs(IConnector? connector)
    {
        Connector = connector;
    }
}

public sealed class ConnectorDeselectedEventArgs : EventArgs
{
    public IConnector? Connector { get; }

    public ConnectorDeselectedEventArgs(IConnector? connector)
    {
        Connector = connector;
    }
}

public sealed class ConnectorStartChangedEventArgs : EventArgs
{
    public IConnector? Connector { get; }

    public ConnectorStartChangedEventArgs(IConnector? connector)
    {
        Connector = connector;
    }
}

public sealed class ConnectorEndChangedEventArgs : EventArgs
{
    public IConnector? Connector { get; }

    public ConnectorEndChangedEventArgs(IConnector? connector)
    {
        Connector = connector;
    }
}

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
    event EventHandler<ConnectorSelectedEventArgs>? Selected;
    event EventHandler<ConnectorDeselectedEventArgs>? Deselected;
    event EventHandler<ConnectorStartChangedEventArgs>? StartChanged;
    event EventHandler<ConnectorEndChangedEventArgs>? EndChanged;
    void OnCreated();
    void OnRemoved();
    void OnSelected();
    void OnDeselected();
    void OnStartChanged();
    void OnEndChanged();
}
