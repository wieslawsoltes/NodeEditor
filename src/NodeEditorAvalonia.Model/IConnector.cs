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
