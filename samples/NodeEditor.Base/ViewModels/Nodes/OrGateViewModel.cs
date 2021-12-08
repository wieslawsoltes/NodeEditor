using System.Runtime.Serialization;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels.Nodes;

[DataContract(IsReference = true)]
public class OrGateViewModel : ViewModelBase
{
    private object? _label;
    private int _count;

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public object? Label
    {
        get => _label;
        set => this.RaiseAndSetIfChanged(ref _label, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public int Count
    {
        get => _count;
        set => this.RaiseAndSetIfChanged(ref _count, value);
    }
}