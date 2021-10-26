using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels
{
    [DataContract(IsReference = true)]
    public class NodeViewModel : ReactiveObject
    {
        private NodeViewModel? _parent;
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private object? _content;
        private ObservableCollection<PinViewModel>? _pins;

        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public NodeViewModel? Parent
        {
            get => _parent;
            set => this.RaiseAndSetIfChanged(ref _parent, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public double X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public double Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public double Width
        {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public double Height
        {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public object? Content
        {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ObservableCollection<PinViewModel>? Pins
        {
            get => _pins;
            set => this.RaiseAndSetIfChanged(ref _pins, value);
        }

        public PinViewModel AddPin(double x, double y, double width, double height, PinAlignment alignment = PinAlignment.None)
        {
            var pin = new PinViewModel()
            {
                Parent = this,
                X = x,
                Y = y, 
                Width = width, 
                Height = height,
                Alignment = alignment
            };

            Pins ??= new ObservableCollection<PinViewModel>();
            Pins.Add(pin);

            return pin;
        }
    }
}
