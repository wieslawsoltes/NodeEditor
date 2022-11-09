using System;
using System.Runtime.Serialization;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels;

[DataContract(IsReference = true)]
public class PinViewModel : ReactiveObject, IPin
{
    private string? _name;
    private INode? _parent;
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private PinAlignment _alignment;

    public event EventHandler<PinCreatedEventArgs>? Created;

    public event EventHandler<PinRemovedEventArgs>? Removed;

    public event EventHandler<PinMovedEventArgs>? Moved;

    public event EventHandler<PinSelectedEventArgs>? Selected;

    public event EventHandler<PinDeselectedEventArgs>? Deselected;

    public event EventHandler<PinResizedEventArgs>? Resized;

    public event EventHandler<PinConnectedEventArgs>? Connected;

    public event EventHandler<PinDisconnectedEventArgs>? Disconnected;

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public INode? Parent
    {
        get => _parent;
        set => this.RaiseAndSetIfChanged(ref _parent, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public double X
    {
        get => _x;
        set => this.RaiseAndSetIfChanged(ref _x, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public double Y
    {
        get => _y;
        set => this.RaiseAndSetIfChanged(ref _y, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public double Width
    {
        get => _width;
        set => this.RaiseAndSetIfChanged(ref _width, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public double Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public PinAlignment Alignment
    {
        get => _alignment;
        set => this.RaiseAndSetIfChanged(ref _alignment, value);
    }

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
