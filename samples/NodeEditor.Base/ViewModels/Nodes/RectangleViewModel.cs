using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class RectangleViewModel : ViewModelBase
{
    [ObservableProperty] private object? _label;
}
