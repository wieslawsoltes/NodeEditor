using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class OrGateViewModel : NodeView
{
    [ObservableProperty] private object? _label;
    [ObservableProperty] private int _count;
}
