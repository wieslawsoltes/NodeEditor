using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class RectangleViewModel : NodeView
{
    [ObservableProperty] private object? _label;
}
