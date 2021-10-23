using System.Collections.ObjectModel;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels
{
    public abstract class ConnectedNodeViewModel : NodeViewModel
    {
        private ObservableCollection<PinViewModel>? _pins;
 
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
