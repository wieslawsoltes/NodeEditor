using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditorLogic.Models;

namespace NodeEditorLogic.ViewModels.Nodes;

public partial class LogicOutputNodeViewModel : LogicNodeContentViewModel
{
    [ObservableProperty] private string? _label;
    [ObservableProperty] private LogicValue _observedValue = LogicValue.Unknown;
}
