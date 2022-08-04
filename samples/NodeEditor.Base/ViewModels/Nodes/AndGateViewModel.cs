using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class AndGateViewModel : ViewModelBase
{
    [ObservableProperty] private object? _label;
}
