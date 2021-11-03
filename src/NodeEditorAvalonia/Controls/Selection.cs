using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace NodeEditor.Controls
{
    public class Selection : Control
    {
        public static readonly StyledProperty<IBrush?> BrushProperty =
            AvaloniaProperty.Register<Selection, IBrush?>(nameof(Brush), new SolidColorBrush(Colors.Blue) { Opacity = 0.5 });

        public static readonly StyledProperty<IPen?> PenProperty =
            AvaloniaProperty.Register<Selection, IPen?>(nameof(Pen), new Pen(new SolidColorBrush(Colors.Blue), 2));

        public static readonly StyledProperty<Point> TopLeftProperty =
            AvaloniaProperty.Register<Selection, Point>(nameof(TopLeft));

        public static readonly StyledProperty<Point> BottomRightProperty =
            AvaloniaProperty.Register<Selection, Point>(nameof(BottomRight));

        public IBrush? Brush
        {
            get => GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        public IPen? Pen
        {
            get => GetValue(PenProperty);
            set => SetValue(PenProperty, value);
        }

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

        public override void Render(DrawingContext context)
        {
            var adornedElement = GetValue(AdornerLayer.AdornedElementProperty);
            if (adornedElement is null)
            {
                return;
            }

            var topLeftX = Math.Min(TopLeft.X, BottomRight.X);
            var topLeftY = Math.Min(TopLeft.Y, BottomRight.Y);
            var bottomRightX = Math.Max(TopLeft.X, BottomRight.X);
            var bottomRightY = Math.Max(TopLeft.Y, BottomRight.Y);

            context.DrawRectangle(
                Brush, 
                Pen, 
                new Rect(
                    new Point(topLeftX, topLeftY), 
                    new Point(bottomRightX, bottomRightY)));
        }
    }
}
