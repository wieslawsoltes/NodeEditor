using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

public partial class NodeTemplateViewModel : ObservableObject, INodeTemplate
{
    [ObservableProperty] private string? _title;
    [ObservableProperty] private INode? _template;
    [ObservableProperty] private INode? _preview;
}
