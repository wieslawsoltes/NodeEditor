using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NodeEditor.Views
{
    public class PinView : UserControl
    {
        public PinView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

