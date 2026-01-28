using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.VisualTree;
using NodeEditor.Model;

namespace NodeEditor.Controls;

[PseudoClasses(":selected")]
public class Connector : Shape
{
    private sealed class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute();

        public void Execute(object? parameter) => _execute();

        public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public static readonly StyledProperty<IConnector?> ConnectorSourceProperty =
        AvaloniaProperty.Register<Connector, IConnector?>(nameof(ConnectorSource));

    public static readonly StyledProperty<Point> StartPointProperty =
        AvaloniaProperty.Register<Connector, Point>(nameof(StartPoint));

    public static readonly StyledProperty<Point> EndPointProperty =
        AvaloniaProperty.Register<Connector, Point>(nameof(EndPoint));

    public static readonly StyledProperty<double> OffsetProperty =
        AvaloniaProperty.Register<Connector, double>(nameof(Offset));

    public static readonly StyledProperty<ConnectorStyle> ConnectorStyleProperty =
        AvaloniaProperty.Register<Connector, ConnectorStyle>(nameof(ConnectorStyle));

    public static readonly StyledProperty<ConnectorOrientation> OrientationProperty =
        AvaloniaProperty.Register<Connector, ConnectorOrientation>(nameof(Orientation));

    public static readonly StyledProperty<Point> LabelPointProperty =
        AvaloniaProperty.Register<Connector, Point>(nameof(LabelPoint));

    private INotifyCollectionChanged? _waypointsCollection;
    private readonly List<INotifyPropertyChanged> _waypointSubscriptions = new();
    private INotifyPropertyChanged? _connectorSubscription;
    private INotifyPropertyChanged? _settingsSubscription;
    private IConnector? _attachedConnector;
    private IDrawingNodeSettings? _attachedSettings;
    private readonly DelegateCommand _swapDirectionCommand;

    static Connector()
    {
        StrokeThicknessProperty.OverrideDefaultValue<Connector>(1);
        AffectsGeometry<Connector>(
            StartPointProperty, 
            EndPointProperty, 
            OffsetProperty, 
            ConnectorStyleProperty,
            OrientationProperty);
    }

    public Connector()
    {
        _swapDirectionCommand = new DelegateCommand(SwapDirection, CanSwapDirection);
    }

    public IConnector? ConnectorSource
    {
        get => GetValue(ConnectorSourceProperty);
        set => SetValue(ConnectorSourceProperty, value);
    }

    public Point StartPoint
    {
        get => GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }

    public Point EndPoint
    {
        get => GetValue(EndPointProperty);
        set => SetValue(EndPointProperty, value);
    }

    public double Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public ConnectorStyle ConnectorStyle
    {
        get => GetValue(ConnectorStyleProperty);
        set => SetValue(ConnectorStyleProperty, value);
    }

    public ConnectorOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public Point LabelPoint
    {
        get => GetValue(LabelPointProperty);
        set => SetValue(LabelPointProperty, value);
    }

    public ICommand SwapDirectionCommand => _swapDirectionCommand;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ConnectorSourceProperty)
        {
            DetachConnector();
            AttachConnector(change.NewValue as IConnector);
            InvalidateGeometry();
            UpdateLabelPoint();
            _swapDirectionCommand.NotifyCanExecuteChanged();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AttachConnector(ConnectorSource);
        UpdateLabelPoint();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        DetachConnector();
        UpdateLabelPoint();
    }

    protected override Geometry CreateDefiningGeometry()
    {
        if (ConnectorSource is IConnector connector && ConnectorPathHelper.TryGetEndpoints(connector, out var start, out var end))
        {
            var lineGeometry = BuildLineGeometry(connector, start, end);
            if (connector.StartArrow == ConnectorArrowStyle.None && connector.EndArrow == ConnectorArrowStyle.None)
            {
                return lineGeometry;
            }

            var group = new GeometryGroup();
            group.Children.Add(lineGeometry);
            AppendArrowGeometries(group, connector, start, end);
            return group;
        }

        var fallback = new StreamGeometry();
        using (var context = fallback.Open())
        {
            context.BeginFigure(StartPoint, false);
            context.CubicBezierTo(StartPoint, EndPoint, EndPoint);
            context.EndFigure(false);
        }

        return fallback;
    }

    private static StreamGeometry BuildLineGeometry(IConnector connector, Point start, Point end)
    {
        if (connector.Style == ConnectorStyle.Bezier)
        {
            var points = ConnectorPathHelper.GetPolylinePoints(connector, start, end);
            if (points.Count <= 2 && connector.Waypoints.Count == 0)
            {
                return BuildSingleBezierGeometry(connector, start, end);
            }

            var radius = ResolveCornerRadius(connector);
            return BuildRoundedGeometry(points, radius);
        }

        var polylinePoints = ConnectorPathHelper.GetPolylinePoints(connector, start, end);
        return BuildPolylineGeometry(polylinePoints);
    }

    private static StreamGeometry BuildSingleBezierGeometry(IConnector connector, Point start, Point end)
    {
        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            var p1X = start.X;
            var p1Y = start.Y;
            var p2X = end.X;
            var p2Y = end.Y;

            connector.GetControlPoints(
                connector.Orientation,
                connector.Offset,
                connector.Start?.Alignment ?? PinAlignment.None,
                connector.End?.Alignment ?? PinAlignment.None,
                ref p1X,
                ref p1Y,
                ref p2X,
                ref p2Y);

            context.BeginFigure(start, false);
            context.CubicBezierTo(new Point(p1X, p1Y), new Point(p2X, p2Y), end);
            context.EndFigure(false);
        }

        return geometry;
    }

    private static StreamGeometry BuildPolylineGeometry(IReadOnlyList<Point> points)
    {
        var geometry = new StreamGeometry();
        if (points.Count == 0)
        {
            return geometry;
        }

        using (var context = geometry.Open())
        {
            context.BeginFigure(points[0], false);
            for (var i = 1; i < points.Count; i++)
            {
                context.LineTo(points[i]);
            }
            context.EndFigure(false);
        }

        return geometry;
    }

    private static StreamGeometry BuildRoundedGeometry(IReadOnlyList<Point> points, double radius)
    {
        if (points.Count < 2 || radius <= 0.001)
        {
            return BuildPolylineGeometry(points);
        }

        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            context.BeginFigure(points[0], false);

            for (var i = 1; i < points.Count - 1; i++)
            {
                var prev = points[i - 1];
                var current = points[i];
                var next = points[i + 1];

                var inDx = prev.X - current.X;
                var inDy = prev.Y - current.Y;
                var outDx = next.X - current.X;
                var outDy = next.Y - current.Y;

                var inLen = Math.Sqrt(inDx * inDx + inDy * inDy);
                var outLen = Math.Sqrt(outDx * outDx + outDy * outDy);

                if (inLen < 0.001 || outLen < 0.001 || IsColinear(prev, current, next))
                {
                    context.LineTo(current);
                    continue;
                }

                var cornerRadius = Math.Min(radius, Math.Min(inLen, outLen) * 0.5);
                var inScale = cornerRadius / inLen;
                var outScale = cornerRadius / outLen;

                var startCurve = new Point(current.X + inDx * inScale, current.Y + inDy * inScale);
                var endCurve = new Point(current.X + outDx * outScale, current.Y + outDy * outScale);

                context.LineTo(startCurve);
                context.CubicBezierTo(current, current, endCurve);
            }

            context.LineTo(points[points.Count - 1]);
            context.EndFigure(false);
        }

        return geometry;
    }

    private static bool IsColinear(Point a, Point b, Point c)
    {
        var dx1 = b.X - a.X;
        var dy1 = b.Y - a.Y;
        var dx2 = c.X - b.X;
        var dy2 = c.Y - b.Y;
        return Math.Abs(dx1 * dy2 - dy1 * dx2) < 0.001;
    }

    private static double ResolveCornerRadius(IConnector connector)
    {
        if (connector.Parent?.Settings is { } settings)
        {
            return Math.Max(0.0, settings.RoutingCornerRadius);
        }

        return 0.0;
    }

    private void AppendArrowGeometries(GeometryGroup group, IConnector connector, Point start, Point end)
    {
        var points = ConnectorPathHelper.GetFlattenedPath(connector, start, end);
        if (points.Count < 2)
        {
            return;
        }

        if (connector.StartArrow != ConnectorArrowStyle.None)
        {
            var geometry = CreateArrowGeometry(points[1], points[0], connector.StartArrow);
            if (geometry is not null)
            {
                group.Children.Add(geometry);
            }
        }

        if (connector.EndArrow != ConnectorArrowStyle.None)
        {
            var geometry = CreateArrowGeometry(
                points[points.Count - 2],
                points[points.Count - 1],
                connector.EndArrow);
            if (geometry is not null)
            {
                group.Children.Add(geometry);
            }
        }
    }

    private Geometry? CreateArrowGeometry(Point from, Point to, ConnectorArrowStyle style)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var length = Math.Sqrt(dx * dx + dy * dy);
        if (length < 0.001)
        {
            return null;
        }

        var unit = new Vector(dx / length, dy / length);
        var perpendicular = new Vector(-unit.Y, unit.X);
        var size = Math.Max(6.0, StrokeThickness * 2.5);
        var half = size * 0.5;

        switch (style)
        {
            case ConnectorArrowStyle.Arrow:
                var basePoint = to - unit * size;
                var p1 = basePoint + perpendicular * half;
                var p2 = basePoint - perpendicular * half;
                return CreatePolygon(to, p1, p2);
            case ConnectorArrowStyle.Circle:
                return new EllipseGeometry(new Rect(to.X - half, to.Y - half, half + half, half + half));
            case ConnectorArrowStyle.Diamond:
                var forward = unit * half;
                var side = perpendicular * half;
                var d1 = to + forward;
                var d2 = to + side;
                var d3 = to - forward;
                var d4 = to - side;
                return CreatePolygon(d1, d2, d3, d4);
            default:
                return null;
        }
    }

    private static StreamGeometry CreatePolygon(params Point[] points)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            if (points.Length > 0)
            {
                context.BeginFigure(points[0], true);
                for (var i = 1; i < points.Length; i++)
                {
                    context.LineTo(points[i]);
                }
                context.EndFigure(true);
            }
        }

        return geometry;
    }

    private void AttachConnector(IConnector? connector)
    {
        if (connector is null)
        {
            return;
        }

        if (ReferenceEquals(_attachedConnector, connector))
        {
            return;
        }

        DetachConnector();

        _attachedConnector = connector;

        if (connector is INotifyPropertyChanged notify)
        {
            _connectorSubscription = notify;
            notify.PropertyChanged += OnConnectorPropertyChanged;
        }

        AttachWaypoints(connector.Waypoints);
        AttachSettings(connector.Parent?.Settings);
        UpdateLabelPoint();
    }

    private void DetachConnector()
    {
        if (_attachedConnector is INotifyPropertyChanged notify && _connectorSubscription == notify)
        {
            notify.PropertyChanged -= OnConnectorPropertyChanged;
            _connectorSubscription = null;
        }

        DetachWaypoints();
        DetachSettings();
        _attachedConnector = null;
    }

    private void AttachWaypoints(IList<ConnectorPoint> waypoints)
    {
        if (waypoints is INotifyCollectionChanged notifyCollection)
        {
            _waypointsCollection = notifyCollection;
            notifyCollection.CollectionChanged += OnWaypointsCollectionChanged;
        }

        foreach (var waypoint in waypoints)
        {
            if (waypoint is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged += OnWaypointPropertyChanged;
                _waypointSubscriptions.Add(notify);
            }
        }
    }

    private void DetachWaypoints()
    {
        if (_waypointsCollection is not null)
        {
            _waypointsCollection.CollectionChanged -= OnWaypointsCollectionChanged;
            _waypointsCollection = null;
        }

        foreach (var waypoint in _waypointSubscriptions)
        {
            waypoint.PropertyChanged -= OnWaypointPropertyChanged;
        }

        _waypointSubscriptions.Clear();
    }

    private void AttachSettings(IDrawingNodeSettings? settings)
    {
        if (ReferenceEquals(_attachedSettings, settings))
        {
            return;
        }

        DetachSettings();

        _attachedSettings = settings;
        if (settings is INotifyPropertyChanged notify)
        {
            _settingsSubscription = notify;
            notify.PropertyChanged += OnSettingsPropertyChanged;
        }
    }

    private void DetachSettings()
    {
        if (_settingsSubscription is not null)
        {
            _settingsSubscription.PropertyChanged -= OnSettingsPropertyChanged;
            _settingsSubscription = null;
        }

        _attachedSettings = null;
    }

    private void OnConnectorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (ConnectorSource is not IConnector connector)
        {
            return;
        }

        if (e.PropertyName == nameof(IConnector.Waypoints))
        {
            DetachWaypoints();
            AttachWaypoints(connector.Waypoints);
        }

        if (e.PropertyName == nameof(IConnector.Parent))
        {
            AttachSettings(connector.Parent?.Settings);
        }

        if (e.PropertyName == nameof(IConnector.Style)
            || e.PropertyName == nameof(IConnector.Orientation)
            || e.PropertyName == nameof(IConnector.Offset)
            || e.PropertyName == nameof(IConnector.RoutingMode)
            || e.PropertyName == nameof(IConnector.Start)
            || e.PropertyName == nameof(IConnector.End)
            || e.PropertyName == nameof(IConnector.Parent)
            || e.PropertyName == nameof(IConnector.Waypoints))
        {
            InvalidateGeometry();
            UpdateLabelPoint();
        }

        if (e.PropertyName == nameof(IConnector.Start)
            || e.PropertyName == nameof(IConnector.End)
            || e.PropertyName == nameof(IConnector.IsLocked))
        {
            _swapDirectionCommand.NotifyCanExecuteChanged();
        }

        InvalidateVisual();
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateGeometry();
        UpdateLabelPoint();
        InvalidateVisual();
        _swapDirectionCommand.NotifyCanExecuteChanged();
    }

    private void OnWaypointsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (ConnectorSource is not IConnector connector)
        {
            return;
        }

        DetachWaypoints();
        AttachWaypoints(connector.Waypoints);
        InvalidateGeometry();
        UpdateLabelPoint();
    }

    private void OnWaypointPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateGeometry();
        UpdateLabelPoint();
    }

    private void UpdateLabelPoint()
    {
        if (ConnectorSource is IConnector connector
            && ConnectorPathHelper.TryGetEndpoints(connector, out var start, out var end))
        {
            LabelPoint = ConnectorPathHelper.GetLabelPoint(connector, start, end);
            return;
        }

        LabelPoint = default;
    }

    private bool CanSwapDirection()
    {
        if (ConnectorSource is not IConnector connector)
        {
            return false;
        }

        if (connector.IsLocked)
        {
            return false;
        }

        if (connector.Start is null || connector.End is null)
        {
            return false;
        }

        if (connector.Parent is not IDrawingNode drawingNode)
        {
            return true;
        }

        var settings = drawingNode.Settings;
        var start = connector.Start;
        var end = connector.End;

        if (settings.RequireDirectionalConnections)
        {
            var startDir = GetDirection(start);
            var endDir = GetDirection(end);
            if (!IsDirectionalConnectionAllowed(endDir, startDir))
            {
                return false;
            }
        }

        if (settings.RequireMatchingBusWidth)
        {
            if (GetBusWidth(start) != GetBusWidth(end))
            {
                return false;
            }
        }

        var validator = settings.ConnectionValidator;
        if (validator is not null)
        {
            var context = new ConnectionValidationContext(drawingNode, end, start);
            if (!validator(context))
            {
                return false;
            }
        }

        return true;
    }

    private void SwapDirection()
    {
        if (ConnectorSource is not IConnector connector)
        {
            return;
        }

        if (connector.IsLocked || connector.Start is null || connector.End is null)
        {
            return;
        }

        if (!CanSwapDirection())
        {
            return;
        }

        if (connector.Parent is IUndoRedoHost undoHost)
        {
            undoHost.BeginUndoBatch();
            try
            {
                SwapPinsAndWaypoints(connector);
            }
            finally
            {
                undoHost.EndUndoBatch();
            }
        }
        else
        {
            SwapPinsAndWaypoints(connector);
        }
    }

    private static void SwapPinsAndWaypoints(IConnector connector)
    {
        var start = connector.Start;
        var end = connector.End;
        if (start is null || end is null)
        {
            return;
        }

        connector.Start = end;
        connector.End = start;

        var waypoints = connector.Waypoints;
        if (waypoints is { Count: > 1 } && !waypoints.IsReadOnly)
        {
            var i = 0;
            var j = waypoints.Count - 1;
            while (i < j)
            {
                (waypoints[i], waypoints[j]) = (waypoints[j], waypoints[i]);
                i++;
                j--;
            }
        }
    }

    private static PinDirection GetDirection(IPin pin)
    {
        if (pin is IConnectablePin connectable)
        {
            return connectable.Direction;
        }

        return PinDirection.Bidirectional;
    }

    private static int GetBusWidth(IPin pin)
    {
        if (pin is IConnectablePin connectable)
        {
            return Math.Max(1, connectable.BusWidth);
        }

        return 1;
    }

    private static bool IsDirectionalConnectionAllowed(PinDirection start, PinDirection end)
    {
        if (start == PinDirection.Bidirectional || end == PinDirection.Bidirectional)
        {
            return true;
        }

        return start == PinDirection.Output && end == PinDirection.Input;
    }
}
