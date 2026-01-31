using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using NodeEditorLogic.ViewModels;
using NodeEditorLogic.ViewModels.Nodes;
using NodeEditorLogic.Views.Nodes;

namespace NodeEditorLogic;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is LogicGateNodeViewModel)
        {
            return new GateNodeView();
        }

        if (data is LogicInputNodeViewModel)
        {
            return new InputNodeView();
        }

        if (data is LogicOutputNodeViewModel)
        {
            return new OutputNodeView();
        }

        if (data is LogicClockNodeViewModel)
        {
            return new ClockNodeView();
        }

        if (data is LogicFlipFlopNodeViewModel)
        {
            return new FlipFlopNodeView();
        }

        if (data is LogicBusInputNodeViewModel)
        {
            return new BusInputNodeView();
        }

        if (data is LogicBusOutputNodeViewModel)
        {
            return new BusOutputNodeView();
        }

        if (data is LogicBusSplitNodeViewModel)
        {
            return new BusSplitNodeView();
        }

        if (data is LogicBusMergeNodeViewModel)
        {
            return new BusMergeNodeView();
        }

        if (data is LogicNoteNodeViewModel)
        {
            return new NoteNodeView();
        }

        var name = data?.GetType().FullName?.Replace("ViewModel", "View");
        var type = name is null ? null : Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
