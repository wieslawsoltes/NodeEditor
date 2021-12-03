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
}