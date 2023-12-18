using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class OrGateViewModel : ViewModelBase
{
    [ObservableProperty] private object? _label;
    [ObservableProperty] private int _count;
}
