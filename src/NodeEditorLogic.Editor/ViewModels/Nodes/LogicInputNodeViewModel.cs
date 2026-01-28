using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorLogic.ViewModels.Nodes;

public partial class LogicInputNodeViewModel : LogicNodeContentViewModel
{
    [ObservableProperty] private bool _isOn;
    [ObservableProperty] private string? _label;
}
