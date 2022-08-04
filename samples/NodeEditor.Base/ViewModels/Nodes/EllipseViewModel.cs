using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class EllipseViewModel : ViewModelBase
{
    [ObservableProperty] private object? _label;
}
