using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels
{
    public class PinViewModel : ReactiveObject
    {
        private NodeViewModel? _parent;
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private PinAlignment _alignment;

        public NodeViewModel? Parent
        {
            get => _parent;
            set => this.RaiseAndSetIfChanged(ref _parent, value);
        }

        public double X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        public double Width
        {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }

        public PinAlignment Alignment
        {
            get => _alignment;
            set => this.RaiseAndSetIfChanged(ref _alignment, value);
        }
    }
}
