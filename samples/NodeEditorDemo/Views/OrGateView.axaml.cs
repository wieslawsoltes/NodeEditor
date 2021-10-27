using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NodeEditorDemo.Views
{
    public class OrGateView : UserControl
    {
        public OrGateView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

