using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class SignalViewModel : NodeView
{
    [ObservableProperty] private object? _label;
    [ObservableProperty] private bool? _state;
}
