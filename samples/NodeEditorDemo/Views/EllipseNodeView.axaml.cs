
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NodeEditorDemo.Views
{
    public class EllipseNodeView : UserControl
    {
        public EllipseNodeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

