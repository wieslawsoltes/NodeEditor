using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace NodeEditor.Controls
{
    public class Selected : Control
    {
        public static readonly StyledProperty<IBrush?> BrushProperty =
            AvaloniaProperty.Register<Selected, IBrush?>(nameof(Brush), Brushes.Transparent);

        public static readonly StyledProperty<IPen?> PenProperty =
            AvaloniaProperty.Register<Selected, IPen?>(nameof(Pen), new Pen(Brushes.Cyan, 2));

        public static readonly StyledProperty<Rect> RectProperty =
            AvaloniaProperty.Register<Selected, Rect>(nameof(Rect));

        static Selected()
        {
            AffectsRender<Selected>(BrushProperty, PenProperty, RectProperty);
        }

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

        public Rect Rect
        {
            get => GetValue(RectProperty);
            set => SetValue(RectProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            var adornedElement = GetValue(AdornerLayer.AdornedElementProperty);
            if (adornedElement is null)
            {
                return;
            }

            context.DrawRectangle(Brush, Pen, Rect);
        }
    }
}
