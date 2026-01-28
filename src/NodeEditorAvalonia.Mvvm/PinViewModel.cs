using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

public partial class PinViewModel : ObservableObject, IPin, IConnectablePin
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private INode? _parent;
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private double _width;
    [ObservableProperty] private double _height;
    [ObservableProperty] private PinAlignment _alignment;
    [ObservableProperty] private PinDirection _direction = PinDirection.Bidirectional;
    [ObservableProperty] private int _busWidth = 1;

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

    partial void OnBusWidthChanged(int value)
    {
        if (value < 1)
        {
            BusWidth = 1;
        }
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
        Moved?.Invoke(this, new PinMovedEventArgs(this, X, Y));
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
        Resized?.Invoke(this, new PinResizedEventArgs(this, X, Y, Width, Height));
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
