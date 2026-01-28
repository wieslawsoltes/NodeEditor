using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

public partial class EditorViewModel : ObservableObject, INodeTemplatesHost, IEditor
{
    [ObservableProperty] private INodeSerializer? _serializer;
    [ObservableProperty] private INodeFactory? _factory;
    [ObservableProperty] private IList<INodeTemplate>? _templates;
    [ObservableProperty] private IDrawingNode? _drawing;
}
