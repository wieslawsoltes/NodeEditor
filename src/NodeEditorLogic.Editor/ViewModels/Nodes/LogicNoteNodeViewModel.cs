using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorLogic.ViewModels.Nodes;

public partial class LogicNoteNodeViewModel : ViewModelBase
{
    [ObservableProperty] private string? _text = "Note";
}
