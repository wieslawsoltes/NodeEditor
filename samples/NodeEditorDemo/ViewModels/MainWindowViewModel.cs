using NodeEditor.ViewModels;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private DrawingNodeViewModel? _drawing;

        public DrawingNodeViewModel? Drawing
        {
            get => _drawing;
            set => this.RaiseAndSetIfChanged(ref _drawing, value);
        }
    }
}
