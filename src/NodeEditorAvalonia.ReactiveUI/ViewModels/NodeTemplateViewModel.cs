using System;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels;

public class NodeTemplateViewModel : ReactiveObject, INodeTemplate
{
    private string? _title;
    private INode? _template;
    private INode? _preview;

    public string? Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public INode? Template
    {
        get => _template;
        set => this.RaiseAndSetIfChanged(ref _template, value);
    }

    public INode? Preview
    {
        get => _preview;
        set => this.RaiseAndSetIfChanged(ref _preview, value);
    }
}
