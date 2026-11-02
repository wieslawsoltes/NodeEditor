using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using NodeEditor;
using NodeEditor.Model;

namespace NodeEditor.Controls;

public class ConnectorSelectedAdorner : Control
{
    private readonly Dictionary<IConnector, ZipTieConnectorSubscription> _zipTieSubscriptions = new();
    private readonly List<INotifyPropertyChanged> _connectorSubscriptions = new();
    private INotifyCollectionChanged? _collection;
    private bool _isAttached;

    public static readonly StyledProperty<IReadOnlyList<IConnector>?> ConnectorsProperty =
        AvaloniaProperty.Register<ConnectorSelectedAdorner, IReadOnlyList<IConnector>?>(nameof(Connectors));

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<ConnectorSelectedAdorner, IBrush?>(nameof(Stroke));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<ConnectorSelectedAdorner, double>(nameof(StrokeThickness), 2.0);

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

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
#pragma warning disable 8631
        base.OnPropertyChanged(change);
#pragma warning restore 8631

        if (change.Property == ConnectorsProperty || change.Property == StrokeProperty || change.Property == StrokeThicknessProperty)
        {
            if (change.Property == ConnectorsProperty)
            {
                RefreshSubscriptions();
            }
            InvalidateVisual();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        RefreshSubscriptions();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isAttached = false;
        RefreshSubscriptions();
    }

    private void RefreshSubscriptions()
    {
        if (_collection is not null)
        {
            _collection.CollectionChanged -= CollectionChanged;
            _collection = null;
        }
        foreach (var notifier in _connectorSubscriptions)
        {
            notifier.PropertyChanged -= ConnectorChanged;
        }
        _connectorSubscriptions.Clear();
        foreach (var subscription in _zipTieSubscriptions.Values)
        {
            subscription.Dispose();
        }
        _zipTieSubscriptions.Clear();

        if (!_isAttached || Connectors is not { } connectors)
        {
            return;
        }
        _collection = connectors as INotifyCollectionChanged;
        if (_collection is not null)
        {
            _collection.CollectionChanged += CollectionChanged;
        }
        foreach (var connector in connectors)
        {
            if (_zipTieSubscriptions.ContainsKey(connector))
            {
                continue;
            }
            _zipTieSubscriptions.Add(connector, new ZipTieConnectorSubscription(connector, InvalidateVisual));
            if (connector is INotifyPropertyChanged notifier)
            {
                _connectorSubscriptions.Add(notifier);
                notifier.PropertyChanged += ConnectorChanged;
            }
        }
    }

    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshSubscriptions();
        InvalidateVisual();
    }

    private void ConnectorChanged(object? sender, PropertyChangedEventArgs e) => InvalidateVisual();

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Connectors is not { Count: > 0 } connectors)
        {
            return;
        }

        var brush = Stroke;
        if (brush is null)
        {
            return;
        }

        var pen = new ImmutablePen(brush.ToImmutable(), StrokeThickness);

        foreach (var connector in connectors)
        {
            if (!ConnectorPathHelper.TryGetEndpoints(connector, out var start, out var end))
            {
                continue;
            }

            var points = ConnectorPathHelper.GetFlattenedPath(connector, start, end);
            if (points.Count == 0)
            {
                continue;
            }

            if (points.Count == 1)
            {
                context.DrawLine(pen, points[0], points[0]);
                continue;
            }

            for (var i = 1; i < points.Count; i++)
            {
                context.DrawLine(pen, points[i - 1], points[i]);
            }
        }
    }
}
