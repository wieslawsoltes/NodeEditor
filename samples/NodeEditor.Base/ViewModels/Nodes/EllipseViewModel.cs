using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class EllipseViewModel : NodeView
{
    [ObservableProperty] private object? _label;
}
