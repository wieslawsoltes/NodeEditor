using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

[ObservableObject]
public partial class NodeTemplateViewModel : INodeTemplate
{
    [ObservableProperty] private string? _title;
    [ObservableProperty] private INode? _template;
    [ObservableProperty] private INode? _preview;
}
