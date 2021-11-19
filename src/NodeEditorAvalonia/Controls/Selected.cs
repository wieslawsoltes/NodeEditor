using Avalonia;
using Avalonia.Controls.Primitives;

namespace NodeEditor.Controls
{
    public class Selected : TemplatedControl
    {
        public static readonly StyledProperty<Rect> RectProperty =
            AvaloniaProperty.Register<Selected, Rect>(nameof(Rect));

        public Rect Rect
        {
            get => GetValue(RectProperty);
            set => SetValue(RectProperty, value);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
#pragma warning disable 8631
            base.OnPropertyChanged(change);
#pragma warning restore 8631

            if (change.Property == RectProperty)
            {
                var rect = Rect;
                Margin = new Thickness(rect.Left, rect.Top, 0, 0);
                Width = rect.Width;
                Height = rect.Height;
            }
        }
    }
}
