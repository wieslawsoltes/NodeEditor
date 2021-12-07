using System.Collections.Generic;
using System.Runtime.Serialization;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels;

[DataContract(IsReference = true)]
public class NodeViewModel : ReactiveObject, INode
{
    private string? _name;
    private INode? _parent;
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private object? _content;
    private IList<IPin>? _pins;

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
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
    public object? Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public IList<IPin>? Pins
    {
        get => _pins;
        set => this.RaiseAndSetIfChanged(ref _pins, value);
    }

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
        throw new System.NotImplementedException();
    }
}
