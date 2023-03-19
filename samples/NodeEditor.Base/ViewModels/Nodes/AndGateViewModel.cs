using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class AndGateViewModel : NodeView
{
    [ObservableProperty] private object? _label;
}
