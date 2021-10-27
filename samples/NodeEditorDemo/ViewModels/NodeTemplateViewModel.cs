using System;
using NodeEditor.ViewModels;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels
{
    public class NodeTemplateViewModel : ViewModelBase
    {
        private string? _title;
        private Func<double, double, NodeViewModel>? _build;

        public string? Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public Func<double, double, NodeViewModel>? Build
        {
            get => _build;
            set => this.RaiseAndSetIfChanged(ref _build, value);
        }   
    }
}
