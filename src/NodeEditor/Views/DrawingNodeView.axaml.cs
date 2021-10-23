using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NodeEditor.Views
{
    public class DrawingNodeView : UserControl
    {
        public DrawingNodeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
