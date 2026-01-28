using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditorLogic.Models;

namespace NodeEditorLogic.ViewModels.Nodes;

public partial class LogicFlipFlopNodeViewModel : LogicNodeContentViewModel
{
    [ObservableProperty] private LogicValue _storedValue = LogicValue.Low;
    [ObservableProperty] private LogicValue _lastClockValue = LogicValue.Low;
}
