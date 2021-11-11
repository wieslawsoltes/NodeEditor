using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NodeEditorDemo.Views.Nodes
{
    public class EllipseView : UserControl
    {
        public EllipseView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

