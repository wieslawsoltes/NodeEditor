using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NodeEditorDemo.Views.Nodes
{
    public class AndGateView : UserControl
    {
        public AndGateView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

