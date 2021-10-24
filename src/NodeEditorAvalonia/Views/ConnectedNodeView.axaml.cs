using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NodeEditor.Views
{
    public class ConnectedNodeView : UserControl
    {
        public ConnectedNodeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}