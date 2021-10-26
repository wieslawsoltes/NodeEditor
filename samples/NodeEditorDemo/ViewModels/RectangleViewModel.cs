using ReactiveUI;

namespace NodeEditorDemo.ViewModels
{
    public class RectangleViewModel : ViewModelBase
    {
        private object? _label;

        public object? Label
        {
            get => _label;
            set => this.RaiseAndSetIfChanged(ref _label, value);
        }
    }
}
