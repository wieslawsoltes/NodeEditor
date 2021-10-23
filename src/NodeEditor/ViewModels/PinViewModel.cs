using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels
{
    public class PinViewModel : NodeViewModel
    {
        private PinAlignment _alignment;
        
        public PinAlignment Alignment
        {
            get => _alignment;
            set => this.RaiseAndSetIfChanged(ref _alignment, value);
        }
    }
}
