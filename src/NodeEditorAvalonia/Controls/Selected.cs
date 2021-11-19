using Avalonia;
using Avalonia.Controls;
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
                InvalidateMeasure();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var rect = Rect;

            foreach (var visualChild in VisualChildren)
            {
                if (visualChild is Control control)
                {
                    control.Measure(rect.Size);
                }
            }

            return rect.Size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var rect = Rect;

            foreach (var visualChild in VisualChildren)
            {
                if (visualChild is Control control)
                {
                    control.Arrange(rect);
                }
            }

            return rect.Size;
        }
    }
}
