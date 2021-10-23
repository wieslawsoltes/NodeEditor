using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using NodeEditor.Model;
using NodeEditor.ViewModels;

namespace NodeEditor.Controls
{
    public class Connector : Shape
    {
        public static readonly StyledProperty<Point> StartPointProperty =
            AvaloniaProperty.Register<Connector, Point>(nameof(StartPoint));

        public static readonly StyledProperty<Point> EndPointProperty =
            AvaloniaProperty.Register<Connector, Point>(nameof(EndPoint));

        public static readonly StyledProperty<double> OffsetProperty =
            AvaloniaProperty.Register<Connector, double>(nameof(Offset));

        static Connector()
        {
            StrokeThicknessProperty.OverrideDefaultValue<Connector>(1);
            AffectsGeometry<Connector>(StartPointProperty, EndPointProperty);
        }

        public Point StartPoint
        {
            get { return GetValue(StartPointProperty); }
            set { SetValue(StartPointProperty, value); }
        }

        public Point EndPoint
        {
            get { return GetValue(EndPointProperty); }
            set { SetValue(EndPointProperty, value); }
        }

        public double Offset
        {
            get { return GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        protected override Geometry CreateDefiningGeometry()
        {
            var geometry = new StreamGeometry();

            using var context = geometry.Open();

            context.BeginFigure(StartPoint, false);

            if (DataContext is ConnectorViewModel connectorViewModel)
            {
                var p1X = StartPoint.X;
                var p1Y = StartPoint.Y;
                var p2X = EndPoint.X;
                var p2Y = EndPoint.Y;

                connectorViewModel.GetControlPoints(
                    connectorViewModel.Orientation, 
                    Offset, 
                    connectorViewModel.Start?.Alignment ?? PinAlignment.None,
                    connectorViewModel.End?.Alignment ?? PinAlignment.None,
                    ref p1X, ref p1Y, 
                    ref p2X, ref p2Y);

                context.CubicBezierTo(new Point(p1X, p1Y), new Point(p2X, p2Y), EndPoint);
            }
            else
            {
                context.CubicBezierTo(StartPoint, EndPoint, EndPoint);
            }

            context.EndFigure(false);
            
            return geometry;
        }
    }
}
