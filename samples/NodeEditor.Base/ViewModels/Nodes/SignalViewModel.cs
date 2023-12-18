using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class SignalViewModel : ViewModelBase
{
    [ObservableProperty] private object? _label;
    [ObservableProperty] private bool? _state;
}
