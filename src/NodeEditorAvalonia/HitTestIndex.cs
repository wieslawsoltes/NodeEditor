using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using NodeEditor.Model;

namespace NodeEditor;

internal sealed class HitTestIndex
{
    private sealed class ConnectorSegment
    {
        public IConnector Connector { get; }
        public int SegmentIndex { get; }
        public Point Start { get; }
        public Point End { get; }
        public Rect Bounds { get; }

        public ConnectorSegment(IConnector connector, int segmentIndex, Point start, Point end)
        {
            Connector = connector;
            SegmentIndex = segmentIndex;
            Start = start;
            End = end;
            Bounds = BuildBounds(start, end);
        }

        private static Rect BuildBounds(Point start, Point end)
        {
            var left = Math.Min(start.X, end.X);
            var top = Math.Min(start.Y, end.Y);
            var right = Math.Max(start.X, end.X);
            var bottom = Math.Max(start.Y, end.Y);
            return new Rect(new Point(left, top), new Point(right, bottom));
        }
    }

    private sealed class WaypointWatcher : IDisposable
    {
        private readonly HitTestIndex _owner;
        private readonly IConnector _connector;
        private INotifyCollectionChanged? _collection;
        private readonly List<INotifyPropertyChanged> _points = new();

        public WaypointWatcher(HitTestIndex owner, IConnector connector)
        {
            _owner = owner;
            _connector = connector;
            Attach(connector.Waypoints);
        }

        public void Dispose()
        {
            Detach();
        }

        public void Refresh(IList<ConnectorPoint>? waypoints)
        {
            Detach();
            Attach(waypoints);
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
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Refresh(_connector.Waypoints);
                _owner.MarkConnectorDirty(_connector);
                return;
            }

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

            _owner.MarkConnectorDirty(_connector);
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
            _owner.MarkConnectorDirty(_connector);
        }
    }

    private readonly IDrawingNode _drawingNode;
    private readonly RTree<INode> _nodeTree = new();
    private readonly RTree<ConnectorSegment> _connectorTree = new();
    private readonly Dictionary<INode, RTree<INode>.Entry> _nodeEntries = new();
    private readonly Dictionary<IConnector, List<RTree<ConnectorSegment>.Entry>> _connectorEntries = new();
    private readonly HashSet<INode> _dirtyNodes = new();
    private readonly HashSet<IConnector> _dirtyConnectors = new();
    private readonly HashSet<INode> _attachedNodes = new();
    private readonly HashSet<IConnector> _attachedConnectors = new();
    private readonly Dictionary<IConnector, WaypointWatcher> _waypointWatchers = new();
    private bool _rebuildAll = true;

    private IList<INode>? _nodes;
    private IList<IConnector>? _connectors;
    private INotifyCollectionChanged? _nodesCollection;
    private INotifyCollectionChanged? _connectorsCollection;
    private INotifyPropertyChanged? _drawingNotifier;
    private IDrawingNodeSettings? _settings;
    private INotifyPropertyChanged? _settingsNotifier;

    public HitTestIndex(IDrawingNode drawingNode)
    {
        _drawingNode = drawingNode;
        AttachDrawingNode();
    }

    public IEnumerable<INode> QueryNodes(Rect rect)
    {
        EnsureUpToDate();

        foreach (var entry in _nodeTree.Search(rect))
        {
            yield return entry.Item;
        }
    }

    public IEnumerable<(IConnector Connector, int SegmentIndex, Point Start, Point End)> QueryConnectorSegments(Rect rect)
    {
        EnsureUpToDate();

        foreach (var entry in _connectorTree.Search(rect))
        {
            var segment = entry.Item;
            yield return (segment.Connector, segment.SegmentIndex, segment.Start, segment.End);
        }
    }

    public void EnsureUpToDate()
    {
        if (!ReferenceEquals(_nodes, _drawingNode.Nodes))
        {
            AttachNodes(_drawingNode.Nodes);
        }

        if (!ReferenceEquals(_connectors, _drawingNode.Connectors))
        {
            AttachConnectors(_drawingNode.Connectors);
        }

        if (!ReferenceEquals(_drawingNode.Settings, _settings))
        {
            AttachSettings(_drawingNode.Settings);
        }

        if (_rebuildAll)
        {
            RebuildAll();
            return;
        }

        if (_dirtyNodes.Count > 0)
        {
            foreach (var node in _dirtyNodes)
            {
                UpdateNode(node);
            }

            _dirtyNodes.Clear();
        }

        if (_dirtyConnectors.Count > 0)
        {
            foreach (var connector in _dirtyConnectors)
            {
                UpdateConnector(connector);
            }

            _dirtyConnectors.Clear();
        }
    }

    private void AttachDrawingNode()
    {
        if (_drawingNode is INotifyPropertyChanged notifier)
        {
            _drawingNotifier = notifier;
            _drawingNotifier.PropertyChanged += DrawingNodePropertyChanged;
        }

        AttachNodes(_drawingNode.Nodes);
        AttachConnectors(_drawingNode.Connectors);
        AttachSettings(_drawingNode.Settings);
    }

    private void AttachSettings(IDrawingNodeSettings? settings)
    {
        if (_settingsNotifier is not null)
        {
            _settingsNotifier.PropertyChanged -= SettingsPropertyChanged;
            _settingsNotifier = null;
        }

        _settings = settings;

        if (settings is INotifyPropertyChanged notifier)
        {
            _settingsNotifier = notifier;
            _settingsNotifier.PropertyChanged += SettingsPropertyChanged;
        }

        MarkAllConnectorsDirty();
    }

    private void DrawingNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.Equals(e.PropertyName, nameof(IDrawingNode.Nodes), StringComparison.Ordinal))
        {
            AttachNodes(_drawingNode.Nodes);
            return;
        }

        if (string.Equals(e.PropertyName, nameof(IDrawingNode.Connectors), StringComparison.Ordinal))
        {
            AttachConnectors(_drawingNode.Connectors);
            return;
        }

        if (string.Equals(e.PropertyName, nameof(IDrawingNode.Settings), StringComparison.Ordinal))
        {
            AttachSettings(_drawingNode.Settings);
        }
    }

    private void AttachNodes(IList<INode>? nodes)
    {
        if (ReferenceEquals(_nodes, nodes))
        {
            return;
        }

        DetachNodes();
        _nodes = nodes;

        if (_nodes is INotifyCollectionChanged collection)
        {
            _nodesCollection = collection;
            _nodesCollection.CollectionChanged += NodesCollectionChanged;
        }

        if (_nodes is not null)
        {
            foreach (var node in _nodes)
            {
                AttachNode(node);
            }
        }

        _rebuildAll = true;
    }

    private void DetachNodes()
    {
        if (_nodesCollection is not null)
        {
            _nodesCollection.CollectionChanged -= NodesCollectionChanged;
            _nodesCollection = null;
        }

        foreach (var node in _attachedNodes)
        {
            DetachNode(node);
        }

        _attachedNodes.Clear();
        _nodes = null;
    }

    private void NodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            ResetNodes();
            _rebuildAll = true;
            return;
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is INode node)
                {
                    DetachNode(node);
                    RemoveNode(node);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is INode node)
                {
                    AttachNode(node);
                }
            }
        }
    }

    private void AttachNode(INode node)
    {
        if (!_attachedNodes.Add(node))
        {
            return;
        }

        if (node is INotifyPropertyChanged notifier)
        {
            notifier.PropertyChanged += NodePropertyChanged;
        }

        _dirtyNodes.Add(node);
    }

    private void DetachNode(INode node)
    {
        if (node is INotifyPropertyChanged notifier)
        {
            notifier.PropertyChanged -= NodePropertyChanged;
        }

        _dirtyNodes.Remove(node);
    }

    private void NodePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not INode node)
        {
            return;
        }

        _dirtyNodes.Add(node);
    }

    private void AttachConnectors(IList<IConnector>? connectors)
    {
        if (ReferenceEquals(_connectors, connectors))
        {
            return;
        }

        DetachConnectors();
        _connectors = connectors;

        if (_connectors is INotifyCollectionChanged collection)
        {
            _connectorsCollection = collection;
            _connectorsCollection.CollectionChanged += ConnectorsCollectionChanged;
        }

        if (_connectors is not null)
        {
            foreach (var connector in _connectors)
            {
                AttachConnector(connector);
            }
        }

        _rebuildAll = true;
    }

    private void DetachConnectors()
    {
        if (_connectorsCollection is not null)
        {
            _connectorsCollection.CollectionChanged -= ConnectorsCollectionChanged;
            _connectorsCollection = null;
        }

        foreach (var connector in _attachedConnectors)
        {
            DetachConnector(connector);
            RemoveConnector(connector);
        }

        _attachedConnectors.Clear();
        _connectors = null;
    }

    private void ConnectorsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            ResetConnectors();
            _rebuildAll = true;
            return;
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is IConnector connector)
                {
                    DetachConnector(connector);
                    RemoveConnector(connector);
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
    }

    private void AttachConnector(IConnector connector)
    {
        if (!_attachedConnectors.Add(connector))
        {
            return;
        }

        if (connector is INotifyPropertyChanged notifier)
        {
            notifier.PropertyChanged += ConnectorPropertyChanged;
        }

        if (!_waypointWatchers.ContainsKey(connector))
        {
            _waypointWatchers[connector] = new WaypointWatcher(this, connector);
        }

        _dirtyConnectors.Add(connector);
    }

    private void ResetNodes()
    {
        foreach (var node in _attachedNodes)
        {
            DetachNode(node);
            RemoveNode(node);
        }

        _attachedNodes.Clear();
        _dirtyNodes.Clear();

        if (_nodes is not null)
        {
            foreach (var node in _nodes)
            {
                AttachNode(node);
            }
        }
    }

    private void ResetConnectors()
    {
        foreach (var connector in _attachedConnectors)
        {
            DetachConnector(connector);
            RemoveConnector(connector);
        }

        _attachedConnectors.Clear();
        _dirtyConnectors.Clear();

        if (_connectors is not null)
        {
            foreach (var connector in _connectors)
            {
                AttachConnector(connector);
            }
        }
    }

    private void DetachConnector(IConnector connector)
    {
        if (connector is INotifyPropertyChanged notifier)
        {
            notifier.PropertyChanged -= ConnectorPropertyChanged;
        }

        if (_waypointWatchers.TryGetValue(connector, out var watcher))
        {
            watcher.Dispose();
            _waypointWatchers.Remove(connector);
        }

        _dirtyConnectors.Remove(connector);
    }

    private void ConnectorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not IConnector connector)
        {
            return;
        }

        if (string.Equals(e.PropertyName, nameof(IConnector.Waypoints), StringComparison.Ordinal))
        {
            if (_waypointWatchers.TryGetValue(connector, out var watcher))
            {
                watcher.Refresh(connector.Waypoints);
            }
        }

        _dirtyConnectors.Add(connector);
    }

    private void SettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName))
        {
            MarkAllConnectorsDirty();
            return;
        }

        if (e.PropertyName.StartsWith("Routing", StringComparison.Ordinal)
            || string.Equals(e.PropertyName, nameof(IDrawingNodeSettings.EnableConnectorRouting), StringComparison.Ordinal))
        {
            MarkAllConnectorsDirty();
        }
    }

    private void MarkAllConnectorsDirty()
    {
        if (_connectors is null)
        {
            _rebuildAll = true;
            return;
        }

        foreach (var connector in _connectors)
        {
            _dirtyConnectors.Add(connector);
        }
    }

    internal void MarkConnectorDirty(IConnector connector)
    {
        _dirtyConnectors.Add(connector);
    }

    private void RebuildAll()
    {
        _nodeTree.Clear();
        _connectorTree.Clear();
        _nodeEntries.Clear();
        _connectorEntries.Clear();

        if (_nodes is not null)
        {
            foreach (var node in _nodes)
            {
                UpdateNode(node);
            }
        }

        if (_connectors is not null)
        {
            foreach (var connector in _connectors)
            {
                UpdateConnector(connector);
            }
        }

        _dirtyNodes.Clear();
        _dirtyConnectors.Clear();
        _rebuildAll = false;
    }

    private void UpdateNode(INode node)
    {
        var bounds = HitTestHelper.GetNodeBounds(node);

        if (_nodeEntries.TryGetValue(node, out var entry))
        {
            _nodeTree.Update(entry, bounds);
            return;
        }

        _nodeEntries[node] = _nodeTree.Insert(node, bounds);
    }

    private void RemoveNode(INode node)
    {
        if (_nodeEntries.TryGetValue(node, out var entry))
        {
            _nodeTree.Remove(entry);
            _nodeEntries.Remove(node);
        }
    }

    private void UpdateConnector(IConnector connector)
    {
        RemoveConnector(connector);

        if (!ConnectorPathHelper.TryGetEndpoints(connector, out var start, out var end))
        {
            return;
        }

        var points = ConnectorPathHelper.GetFlattenedPath(connector, start, end);
        if (points.Count < 2)
        {
            return;
        }

        var entries = new List<RTree<ConnectorSegment>.Entry>(points.Count - 1);

        for (var i = 1; i < points.Count; i++)
        {
            var segment = new ConnectorSegment(connector, i - 1, points[i - 1], points[i]);
            var entry = _connectorTree.Insert(segment, segment.Bounds);
            entries.Add(entry);
        }

        _connectorEntries[connector] = entries;
    }

    private void RemoveConnector(IConnector connector)
    {
        if (_connectorEntries.TryGetValue(connector, out var entries))
        {
            foreach (var entry in entries)
            {
                _connectorTree.Remove(entry);
            }

            _connectorEntries.Remove(connector);
        }
    }
}
