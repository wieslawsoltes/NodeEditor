using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditorLogic.ViewModels;

public partial class ToolboxCategoryViewModel : ViewModelBase
{
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private ObservableCollection<INodeTemplate> _templates = new();
    [ObservableProperty] private bool _isExpanded = true;
}
