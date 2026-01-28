using System.Collections.Generic;
using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditorLogic.ViewModels;

namespace NodeEditorLogic.Services;

public sealed class LogicDrawingNodeFactory : IDrawingNodeFactory
{
    public static readonly LogicDrawingNodeFactory Instance = new();

    public IPin CreatePin() => new LogicPinViewModel();

    public IConnector CreateConnector() => new LogicConnectorViewModel();

    public IList<T> CreateList<T>() => new ObservableCollection<T>();
}
