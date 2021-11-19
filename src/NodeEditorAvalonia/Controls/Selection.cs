using System;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace NodeEditor.Controls
{
    public class Selection : TemplatedControl
    {
        public static readonly StyledProperty<Point> TopLeftProperty =
            AvaloniaProperty.Register<Selection, Point>(nameof(TopLeft));

        public static readonly StyledProperty<Point> BottomRightProperty =
            AvaloniaProperty.Register<Selection, Point>(nameof(BottomRight));

        public Point TopLeft
        {
            get => GetValue(TopLeftProperty);
            set => SetValue(TopLeftProperty, value);
        }

        public Point BottomRight
        {
            get => GetValue(BottomRightProperty);
            set => SetValue(BottomRightProperty, value);
        }

        public Rect GetRect()
        {
            var topLeftX = Math.Min(TopLeft.X, BottomRight.X);
            var topLeftY = Math.Min(TopLeft.Y, BottomRight.Y);
            var bottomRightX = Math.Max(TopLeft.X, BottomRight.X);
            var bottomRightY = Math.Max(TopLeft.Y, BottomRight.Y);
            return new Rect(
                new Point(topLeftX, topLeftY),
                new Point(bottomRightX, bottomRightY));
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
#pragma warning disable 8631
            base.OnPropertyChanged(change);
#pragma warning restore 8631

            if (change.Property == TopLeftProperty || change.Property == BottomRightProperty)
            {
                var rect = GetRect();
                Margin = new Thickness(rect.Left, rect.Top, 0, 0);
                Width = rect.Width;
                Height = rect.Height;
            }
        }
    }
}
