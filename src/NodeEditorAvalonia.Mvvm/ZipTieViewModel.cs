using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Newtonsoft.Json;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.Mvvm;

/// <summary>
/// Observable wire-bundle data. Geometry and membership changes are relayed through
/// the corresponding owner property so renderers can invalidate only affected wires.
/// </summary>
[JsonObject(MemberSerialization.OptOut)]
public sealed class ZipTieViewModel : ReactiveObject, IZipTie
{
    private string? _name;
    private ConnectorPoint _start = new();
    private ConnectorPoint _end = new(240, 0);
    private IList<ConnectorPoint> _waypoints = new ObservableCollection<ConnectorPoint>();
    private IList<IConnector> _connectors = new ObservableCollection<IConnector>();
    private double _spacing = 6.0;
    private bool _isVisible = true;
    private bool _isLocked;
    private readonly Dictionary<ConnectorPoint, (double X, double Y)> _observedPoints = new();
    private bool _repairingPoint;

    /// <summary>Creates an empty bundle with observable member and anchor collections.</summary>
    public ZipTieViewModel()
    {
        ObservePoints();
        ((INotifyCollectionChanged)_waypoints).CollectionChanged += OnWaypointsChanged;
        ((INotifyCollectionChanged)_connectors).CollectionChanged += OnConnectorsChanged;
    }

    /// <inheritdoc/>
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <inheritdoc/>
    public ConnectorPoint Start
    {
        get => _start;
        set
        {
            ValidatePoint(value, nameof(Start));
            if (ReferenceEquals(_start, value))
            {
                return;
            }

            this.RaisePropertyChanging();
            _start = value;
            ObservePoints();
            this.RaisePropertyChanged();
        }
    }

    /// <inheritdoc/>
    public ConnectorPoint End
    {
        get => _end;
        set
        {
            ValidatePoint(value, nameof(End));
            if (ReferenceEquals(_end, value))
            {
                return;
            }

            this.RaisePropertyChanging();
            _end = value;
            ObservePoints();
            this.RaisePropertyChanged();
        }
    }

    /// <inheritdoc/>
    public IList<ConnectorPoint> Waypoints
    {
        get => _waypoints;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (ReferenceEquals(_waypoints, value))
            {
                return;
            }

            foreach (var point in value)
            {
                ValidatePoint(point, nameof(Waypoints));
            }

            this.RaisePropertyChanging();
            if (_waypoints is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnWaypointsChanged;
            }

            _waypoints = value.IsReadOnly ? new ObservableCollection<ConnectorPoint>(value) : value;
            if (_waypoints is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnWaypointsChanged;
            }

            ObservePoints();
            this.RaisePropertyChanged();
        }
    }

    /// <inheritdoc/>
    public IList<IConnector> Connectors
    {
        get => _connectors;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (ReferenceEquals(_connectors, value))
            {
                return;
            }

            this.RaisePropertyChanging();
            if (_connectors is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnConnectorsChanged;
            }

            _connectors = NormalizeMembers(value);
            if (_connectors is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnConnectorsChanged;
            }

            this.RaisePropertyChanged();
        }
    }

    /// <inheritdoc/>
    public double Spacing
    {
        get => _spacing;
        set
        {
            if (!IsFinite(value) || value <= 0 || value > 1024)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Wire spacing must be finite and greater than zero, up to 1024 drawing units.");
            }

            this.RaiseAndSetIfChanged(ref _spacing, value);
        }
    }

    /// <inheritdoc/>
    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    /// <inheritdoc/>
    public bool IsLocked
    {
        get => _isLocked;
        set => this.RaiseAndSetIfChanged(ref _isLocked, value);
    }

    private void OnWaypointsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        for (var i = 0; i < _waypoints.Count; i++)
        {
            if (_waypoints[i] is null)
            {
                var valid = new ObservableCollection<ConnectorPoint>();
                foreach (var point in _waypoints)
                {
                    if (point is not null)
                    {
                        valid.Add(point);
                    }
                }

                Waypoints = valid;
                return;
            }
        }

        ObservePoints();
        this.RaisePropertyChanged(nameof(Waypoints));
    }

    private void OnConnectorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var normalized = NormalizeMembers(_connectors);
        if (!ReferenceEquals(normalized, _connectors))
        {
            Connectors = normalized;
            return;
        }

        this.RaisePropertyChanged(nameof(Connectors));
    }

    private static IList<IConnector> NormalizeMembers(IList<IConnector> members)
    {
        var seen = new HashSet<IConnector>();
        var valid = new List<IConnector>(members.Count);
        foreach (var connector in members)
        {
            if (connector is not null && seen.Add(connector))
            {
                valid.Add(connector);
            }
        }

        return members.IsReadOnly || valid.Count != members.Count
            ? new ObservableCollection<IConnector>(valid)
            : members;
    }

    private void ObservePoints()
    {
        foreach (var point in _observedPoints.Keys)
        {
            point.PropertyChanged -= OnPointChanged;
        }

        _observedPoints.Clear();
        ObservePoint(_start);
        ObservePoint(_end);
        foreach (var point in _waypoints)
        {
            if (point is not null)
            {
                ObservePoint(point);
            }
        }
    }

    private void ObservePoint(ConnectorPoint point)
    {
        if (_observedPoints.ContainsKey(point))
        {
            return;
        }

        // A caller may mutate an externally supplied list. Repair invalid
        // coordinates at the boundary before they reach geometry/rendering code.
        if (!IsFinite(point.X))
        {
            point.X = 0;
        }

        if (!IsFinite(point.Y))
        {
            point.Y = 0;
        }

        _observedPoints.Add(point, (point.X, point.Y));
        point.PropertyChanged += OnPointChanged;
    }

    private void OnPointChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ConnectorPoint point || _repairingPoint)
        {
            return;
        }

        if (_observedPoints.TryGetValue(point, out var previous))
        {
            _repairingPoint = true;
            try
            {
                if (!IsFinite(point.X))
                {
                    point.X = previous.X;
                }

                if (!IsFinite(point.Y))
                {
                    point.Y = previous.Y;
                }
            }
            finally
            {
                _repairingPoint = false;
            }
        }

        _observedPoints[point] = (point.X, point.Y);
        if (ReferenceEquals(point, _start))
        {
            this.RaisePropertyChanged(nameof(Start));
        }

        if (ReferenceEquals(point, _end))
        {
            this.RaisePropertyChanged(nameof(End));
        }

        if (!ReferenceEquals(point, _start) && !ReferenceEquals(point, _end) || _waypoints.Contains(point))
        {
            this.RaisePropertyChanged(nameof(Waypoints));
        }
    }

    private static void ValidatePoint(ConnectorPoint? point, string name)
    {
        if (point is null)
        {
            throw new ArgumentNullException(name);
        }

        if (!IsFinite(point.X) || !IsFinite(point.Y))
        {
            throw new ArgumentOutOfRangeException(name, "Tie coordinates must be finite.");
        }
    }

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
