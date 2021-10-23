using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NodeEditor.ViewModels;
using NodeEditorDemo.ViewModels;
using NodeEditorDemo.Views;

namespace NodeEditorDemo
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var vm = new MainWindowViewModel();

                var drawing = CreateDrawing();

                vm.Drawing = drawing;
                
                desktop.MainWindow = new MainWindow
                {
                    DataContext = vm
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static DrawingNodeViewModel CreateDrawing()
        {
            var drawing = new DrawingNodeViewModel
            {
                X = 0,
                Y = 0,
                Width = 500,
                Height = 500,
                Nodes = new ObservableCollection<NodeViewModel>(),
                Connectors = new ObservableCollection<ConnectorViewModel>()
            };

            var rectangle0 = new RectangleNodeViewModel
            {
                Parent = drawing,
                X = 30,
                Y = 30,
                Width = 60,
                Height = 60,
                Pins = new ObservableCollection<PinViewModel>()
            };
            drawing.Nodes.Add(rectangle0);

            rectangle0.AddPin(0, 30, 8, 8);
            rectangle0.AddPin(60, 30, 8, 8);
            rectangle0.AddPin(30, 0, 8, 8);
            rectangle0.AddPin(30, 60, 8, 8);

            var rectangle1 = new RectangleNodeViewModel
            {
                Parent = drawing,
                X = 220,
                Y = 30,
                Width = 60,
                Height = 60,
                Pins = new ObservableCollection<PinViewModel>()
            };
            drawing.Nodes.Add(rectangle1);

            rectangle1.AddPin(0, 30, 8, 8);
            rectangle1.AddPin(60, 30, 8, 8);
            rectangle1.AddPin(30, 0, 8, 8);
            rectangle1.AddPin(30, 60, 8, 8);

            var rectangle2 = new RectangleNodeViewModel
            {
                Parent = drawing,
                X = 30,
                Y = 220,
                Width = 60,
                Height = 60,
                Pins = new ObservableCollection<PinViewModel>()
            };
            drawing.Nodes.Add(rectangle2);

            rectangle2.AddPin(0, 30, 8, 8);
            rectangle2.AddPin(60, 30, 8, 8);
            rectangle2.AddPin(30, 0, 8, 8);
            rectangle2.AddPin(30, 60, 8, 8);

            var rectangle3 = new RectangleNodeViewModel
            {
                Parent = drawing,
                X = 220,
                Y = 220,
                Width = 60,
                Height = 60,
                Pins = new ObservableCollection<PinViewModel>()
            };
            drawing.Nodes.Add(rectangle3);

            rectangle3.AddPin(0, 30, 8, 8);
            rectangle3.AddPin(60, 30, 8, 8);
            rectangle3.AddPin(30, 0, 8, 8);
            rectangle3.AddPin(30, 60, 8, 8);

            var connector0 = new ConnectorViewModel
            {
                Parent = drawing,
                Start = rectangle0.Pins[1],
                End = rectangle1.Pins[0]
            };

            drawing.Connectors.Add(connector0);

            return drawing;
        }
    }
}
