using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using NodeEditor;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class ConnectorCrossingsAdorner : Control
{
    public static readonly StyledProperty<IReadOnlyList<IConnector>?> ConnectorsProperty =
        AvaloniaProperty.Register<ConnectorCrossingsAdorner, IReadOnlyList<IConnector>?>(nameof(Connectors));

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<ConnectorCrossingsAdorner, IBrush?>(nameof(Stroke));

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<ConnectorCrossingsAdorner, IBrush?>(nameof(Background));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<ConnectorCrossingsAdorner, double>(nameof(StrokeThickness), 2.0);

    public static readonly StyledProperty<double> ArcRadiusProperty =
        AvaloniaProperty.Register<ConnectorCrossingsAdorner, double>(nameof(ArcRadius), 6.0);

    private const double AlignmentTolerance = 0.01;
    private const double MinSegmentLength = 0.1;
    private const double IntersectionEpsilon = 1e-6;

    private INotifyCollectionChanged? _connectorsCollection;
    private readonly Dictionary<IConnector, INotifyPropertyChanged> _connectorSubscriptions = new();
    private readonly Dictionary<IConnector, WaypointWatcher> _waypointWatchers = new();
    private bool _isAttached;
    private bool _crossingsDirty = true;
    private readonly List<Crossing> _crossings = new();

    public IReadOnlyList<IConnector>? Connectors
    {
        get => GetValue(ConnectorsProperty);
        set => SetValue(ConnectorsProperty, value);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public double ArcRadius
    {
        get => GetValue(ArcRadiusProperty);
        set => SetValue(ArcRadiusProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ConnectorsProperty)
        {
            if (_isAttached)
            {
                DetachConnectors();
                AttachConnectors(change.NewValue as IReadOnlyList<IConnector>);
            }

            MarkDirty();
            return;
        }

        if (change.Property == StrokeProperty
            || change.Property == StrokeThicknessProperty
            || change.Property == BackgroundProperty
            || change.Property == ArcRadiusProperty)
        {
            MarkDirty();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        AttachConnectors(Connectors);
        MarkDirty();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachConnectors();
        _isAttached = false;
        _crossings.Clear();
        _crossingsDirty = true;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Connectors is not { Count: > 0 } connectors)
        {
            return;
        }

        var stroke = Stroke;
        if (stroke is null)
        {
            return;
        }

        var strokeThickness = StrokeThickness;
        if (strokeThickness <= 0.0)
        {
            return;
        }

        var radius = ArcRadius;
        if (radius <= 0.1)
        {
            return;
        }

        var background = ResolveBackgroundBrush();
        var pen = new ImmutablePen(stroke.ToImmutable(), strokeThickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        ImmutablePen? backgroundPen = null;
        if (background is not null)
        {
            backgroundPen = new ImmutablePen(background.ToImmutable(), strokeThickness + 2.0, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        }

        if (_crossingsDirty)
        {
            RebuildCrossings(connectors, radius, strokeThickness);
        }

        if (_crossings.Count == 0)
        {
            return;
        }

        foreach (var crossing in _crossings)
        {
            DrawCrossing(context, crossing.Center, radius, crossing.Direction, pen, backgroundPen);
        }
    }

    private void MarkDirty()
    {
        _crossingsDirty = true;
        InvalidateVisual();
    }

    private IBrush? ResolveBackgroundBrush()
    {
        var background = Background;
        if (background is not null && !IsTransparentBrush(background))
        {
            return background;
        }

        if (TryGetDrawingBackground(out var drawingBackground))
        {
            return drawingBackground;
        }

        return background;
    }

    private bool TryGetDrawingBackground(out IBrush? brush)
    {
        brush = null;

        if (this is not Visual visual)
        {
            return false;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ancestor is DrawingNode drawingNode && drawingNode.Background is not null)
            {
                brush = drawingNode.Background;
                return true;
            }
        }

        return false;
    }

    private static bool IsTransparentBrush(IBrush brush)
    {
        if (brush is ISolidColorBrush solid)
        {
            return solid.Color.A == 0;
        }

        return false;
    }

    private void AttachConnectors(IReadOnlyList<IConnector>? connectors)
    {
        if (connectors is INotifyCollectionChanged notify)
        {
            _connectorsCollection = notify;
            _connectorsCollection.CollectionChanged += OnConnectorsCollectionChanged;
        }

        if (connectors is null)
        {
            return;
        }

        foreach (var connector in connectors)
        {
            AttachConnector(connector);
        }
    }

    private void DetachConnectors()
    {
        if (_connectorsCollection is not null)
        {
            _connectorsCollection.CollectionChanged -= OnConnectorsCollectionChanged;
            _connectorsCollection = null;
        }

        foreach (var entry in _connectorSubscriptions)
        {
            entry.Value.PropertyChanged -= OnConnectorPropertyChanged;
        }

        _connectorSubscriptions.Clear();

        foreach (var watcher in _waypointWatchers.Values)
        {
            watcher.Dispose();
        }

        _waypointWatchers.Clear();
    }

    private void AttachConnector(IConnector connector)
    {
        if (connector is INotifyPropertyChanged notify && !_connectorSubscriptions.ContainsKey(connector))
        {
            _connectorSubscriptions[connector] = notify;
            notify.PropertyChanged += OnConnectorPropertyChanged;
        }

        if (!_waypointWatchers.ContainsKey(connector))
        {
            _waypointWatchers[connector] = new WaypointWatcher(connector.Waypoints, MarkDirty);
        }
    }

    private void DetachConnector(IConnector connector)
    {
        if (_connectorSubscriptions.TryGetValue(connector, out var notify))
        {
            _connectorSubscriptions.Remove(connector);
            notify.PropertyChanged -= OnConnectorPropertyChanged;
        }

        if (_waypointWatchers.TryGetValue(connector, out var watcher))
        {
            _waypointWatchers.Remove(connector);
            watcher.Dispose();
        }
    }

    private void OnConnectorsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is IConnector connector)
                {
                    DetachConnector(connector);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is IConnector connector)
                {
                    AttachConnector(connector);
                }
            }
        }

        MarkDirty();
    }

    private void OnConnectorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not IConnector connector)
        {
            return;
        }

        if (e.PropertyName == nameof(IConnector.Waypoints))
        {
            if (_waypointWatchers.TryGetValue(connector, out var watcher))
            {
                watcher.Update(connector.Waypoints);
            }
            else
            {
                _waypointWatchers[connector] = new WaypointWatcher(connector.Waypoints, MarkDirty);
            }
        }

        MarkDirty();
    }

    private void RebuildCrossings(IReadOnlyList<IConnector> connectors, double radius, double strokeThickness)
    {
        _crossingsDirty = false;
        _crossings.Clear();

        var segments = BuildSegments(connectors);
        if (segments.Count < 2)
        {
            return;
        }

        var clearance = radius + strokeThickness;
        var tree = new RTree<Segment>();
        foreach (var segment in segments)
        {
            var bounds = ExpandBounds(segment.Bounds, clearance);
            tree.Insert(segment, bounds);
        }

        var seen = new HashSet<long>();
        foreach (var segment in segments)
        {
            var bounds = ExpandBounds(segment.Bounds, clearance);
            foreach (var entry in tree.Search(bounds))
            {
                var other = entry.Item;
                if (other.Index <= segment.Index)
                {
                    continue;
                }

                if (ReferenceEquals(segment.Connector, other.Connector))
                {
                    continue;
                }

                if (!TryGetIntersection(segment, other, clearance, out var intersection, out var direction))
                {
                    continue;
                }

                var key = BuildCrossingKey(intersection);
                if (!seen.Add(key))
                {
                    continue;
                }

                _crossings.Add(new Crossing(intersection, direction));
            }
        }
    }

    private static List<Segment> BuildSegments(IReadOnlyList<IConnector> connectors)
    {
        var segments = new List<Segment>();
        var index = 0;

        foreach (var connector in connectors)
        {
            if (connector is null || !connector.IsVisible)
            {
                continue;
            }

            if (!ConnectorPathHelper.TryGetEndpoints(connector, out var start, out var end))
            {
                continue;
            }

            var points = ConnectorPathHelper.GetFlattenedPath(connector, start, end);
            if (points.Count < 2)
            {
                continue;
            }

            for (var i = 1; i < points.Count; i++)
            {
                if (TryCreateSegment(connector, points[i - 1], points[i], index++, out var segment))
                {
                    segments.Add(segment);
                }
            }
        }

        return segments;
    }

    private static bool TryCreateSegment(IConnector connector, Point start, Point end, int index, out Segment segment)
    {
        segment = default;

        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var length = Math.Sqrt(dx * dx + dy * dy);
        if (length < MinSegmentLength)
        {
            return false;
        }

        var direction = new Vector(dx / length, dy / length);
        var bounds = BuildBounds(start, end);
        var alignment = GetAlignment(direction);
        segment = new Segment(index, connector, start, end, direction, length, bounds, alignment);
        return true;
    }

    private static Rect BuildBounds(Point start, Point end)
    {
        var left = Math.Min(start.X, end.X);
        var top = Math.Min(start.Y, end.Y);
        var right = Math.Max(start.X, end.X);
        var bottom = Math.Max(start.Y, end.Y);
        return new Rect(new Point(left, top), new Point(right, bottom));
    }

    private static Rect ExpandBounds(Rect rect, double amount)
    {
        if (amount <= 0.0)
        {
            return rect;
        }

        return new Rect(rect.X - amount, rect.Y - amount, rect.Width + amount * 2.0, rect.Height + amount * 2.0);
    }

    private static SegmentAlignment GetAlignment(Vector direction)
    {
        if (Math.Abs(direction.Y) <= AlignmentTolerance)
        {
            return SegmentAlignment.Horizontal;
        }

        if (Math.Abs(direction.X) <= AlignmentTolerance)
        {
            return SegmentAlignment.Vertical;
        }

        return SegmentAlignment.Diagonal;
    }

    private static void DrawCrossing(
        DrawingContext context,
        Point center,
        double radius,
        Vector direction,
        ImmutablePen pen,
        ImmutablePen? backgroundPen)
    {
        if (Math.Abs(direction.X) + Math.Abs(direction.Y) <= 0.001)
        {
            return;
        }

        var start = center - direction * radius;
        var end = center + direction * radius;

        if (backgroundPen is not null)
        {
            context.DrawLine(backgroundPen, start, end);
        }

        var arc = CreateArcGeometry(start, end, radius, direction);
        if (backgroundPen is not null)
        {
            context.DrawGeometry(null, backgroundPen, arc);
        }

        context.DrawGeometry(null, pen, arc);
    }

    private static StreamGeometry CreateArcGeometry(Point start, Point end, double radius, Vector direction)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            var angle = Math.Atan2(direction.Y, direction.X) * 180.0 / Math.PI;
            ctx.BeginFigure(start, false);
            ctx.ArcTo(end, new Size(radius, radius), angle, false, SweepDirection.Clockwise);
            ctx.EndFigure(false);
        }

        return geometry;
    }

    private static bool TryGetIntersection(Segment first, Segment second, double clearance, out Point intersection, out Vector bridgeDirection)
    {
        intersection = default;
        bridgeDirection = default;

        if (first.Length <= clearance * 2.0 || second.Length <= clearance * 2.0)
        {
            return false;
        }

        if (!TryGetIntersectionPoint(first.Start, first.End, second.Start, second.End, out var point, out var t, out var u))
        {
            return false;
        }

        if (!IsInteriorIntersection(first.Length, t, clearance) || !IsInteriorIntersection(second.Length, u, clearance))
        {
            return false;
        }

        intersection = point;
        bridgeDirection = ChooseBridgeDirection(first, second);
        return true;
    }

    private static bool TryGetIntersectionPoint(
        Point aStart,
        Point aEnd,
        Point bStart,
        Point bEnd,
        out Point intersection,
        out double t,
        out double u)
    {
        intersection = default;
        t = 0.0;
        u = 0.0;

        var r = aEnd - aStart;
        var s = bEnd - bStart;
        var rxs = Cross(r, s);
        if (Math.Abs(rxs) < IntersectionEpsilon)
        {
            return false;
        }

        var qp = bStart - aStart;
        t = Cross(qp, s) / rxs;
        u = Cross(qp, r) / rxs;

        if (t <= IntersectionEpsilon || t >= 1.0 - IntersectionEpsilon)
        {
            return false;
        }

        if (u <= IntersectionEpsilon || u >= 1.0 - IntersectionEpsilon)
        {
            return false;
        }

        intersection = aStart + r * t;
        return true;
    }

    private static double Cross(Vector a, Vector b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    private static bool IsInteriorIntersection(double length, double t, double clearance)
    {
        var distance = length * Math.Min(t, 1.0 - t);
        return distance > clearance;
    }

    private static Vector ChooseBridgeDirection(Segment first, Segment second)
    {
        if (first.Alignment == SegmentAlignment.Horizontal && second.Alignment == SegmentAlignment.Vertical)
        {
            return first.Direction;
        }

        if (first.Alignment == SegmentAlignment.Vertical && second.Alignment == SegmentAlignment.Horizontal)
        {
            return second.Direction;
        }

        var firstScore = Math.Abs(first.Direction.Y);
        var secondScore = Math.Abs(second.Direction.Y);
        if (Math.Abs(firstScore - secondScore) < AlignmentTolerance)
        {
            return first.Index <= second.Index ? first.Direction : second.Direction;
        }

        return firstScore <= secondScore ? first.Direction : second.Direction;
    }

    private static long BuildCrossingKey(Point point)
    {
        var x = (long)Math.Round(point.X * 10.0);
        var y = (long)Math.Round(point.Y * 10.0);
        return (x << 32) ^ (uint)y;
    }

    private readonly struct Segment
    {
        public Segment(
            int index,
            IConnector connector,
            Point start,
            Point end,
            Vector direction,
            double length,
            Rect bounds,
            SegmentAlignment alignment)
        {
            Index = index;
            Connector = connector;
            Start = start;
            End = end;
            Direction = direction;
            Length = length;
            Bounds = bounds;
            Alignment = alignment;
        }

        public int Index { get; }
        public IConnector Connector { get; }
        public Point Start { get; }
        public Point End { get; }
        public Vector Direction { get; }
        public double Length { get; }
        public Rect Bounds { get; }
        public SegmentAlignment Alignment { get; }
    }

    private readonly struct Crossing
    {
        public Crossing(Point center, Vector direction)
        {
            Center = center;
            Direction = direction;
        }

        public Point Center { get; }
        public Vector Direction { get; }
    }

    private enum SegmentAlignment
    {
        Horizontal,
        Vertical,
        Diagonal
    }

    private sealed class WaypointWatcher : IDisposable
    {
        private readonly Action _changed;
        private INotifyCollectionChanged? _collection;
        private readonly List<INotifyPropertyChanged> _points = new();

        public WaypointWatcher(IList<ConnectorPoint>? waypoints, Action changed)
        {
            _changed = changed;
            Attach(waypoints);
        }

        public void Update(IList<ConnectorPoint>? waypoints)
        {
            Detach();
            Attach(waypoints);
            _changed();
        }

        public void Dispose()
        {
            Detach();
        }

        private void Attach(IList<ConnectorPoint>? waypoints)
        {
            if (waypoints is INotifyCollectionChanged collection)
            {
                _collection = collection;
                _collection.CollectionChanged += OnCollectionChanged;
            }

            if (waypoints is null)
            {
                return;
            }

            foreach (var point in waypoints)
            {
                AttachPoint(point);
            }
        }

        private void Detach()
        {
            if (_collection is not null)
            {
                _collection.CollectionChanged -= OnCollectionChanged;
                _collection = null;
            }

            foreach (var point in _points)
            {
                point.PropertyChanged -= OnPointChanged;
            }

            _points.Clear();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is INotifyPropertyChanged point)
                    {
                        DetachPoint(point);
                    }
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is INotifyPropertyChanged point)
                    {
                        AttachPoint(point);
                    }
                }
            }

            _changed();
        }

        private void AttachPoint(INotifyPropertyChanged point)
        {
            if (_points.Contains(point))
            {
                return;
            }

            _points.Add(point);
            point.PropertyChanged += OnPointChanged;
        }

        private void DetachPoint(INotifyPropertyChanged point)
        {
            if (!_points.Remove(point))
            {
                return;
            }

            point.PropertyChanged -= OnPointChanged;
        }

        private void OnPointChanged(object? sender, PropertyChangedEventArgs e)
        {
            _changed();
        }
    }
}
