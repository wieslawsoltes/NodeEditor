
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NodeEditorDemo.Views
{
    public class RectangleNodeView : UserControl
    {
        public RectangleNodeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

