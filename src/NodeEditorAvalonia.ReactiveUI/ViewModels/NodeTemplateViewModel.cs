using System;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels
{
    public class NodeTemplateViewModel : ReactiveObject, INodeTemplate
    {
        private string? _title;
        private Func<double, double, INode>? _build;

        public string? Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public Func<double, double, INode>? Build
        {
            get => _build;
            set => this.RaiseAndSetIfChanged(ref _build, value);
        }   
    }
}
