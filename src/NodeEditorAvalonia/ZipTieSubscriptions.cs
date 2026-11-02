using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor;

/// <summary>Observes only the current bundle of a wire, avoiding a subscription to every bundle.</summary>
internal sealed class ZipTieConnectorSubscription : IDisposable
{
    private readonly IConnector _connector;
    private readonly Action _changed;
    private INotifyPropertyChanged? _connectorNotifier;
    private IDrawingNode? _drawing;
    private INotifyPropertyChanged? _drawingNotifier;
    private INotifyCollectionChanged? _collection;
    private IZipTie? _tie;

    internal ZipTieConnectorSubscription(IConnector connector, Action changed)
    {
        _connector = connector;
        _changed = changed;
        _connectorNotifier = connector as INotifyPropertyChanged;
        if (_connectorNotifier is not null)
        {
            _connectorNotifier.PropertyChanged += ConnectorChanged;
        }
        RefreshDrawing();
    }

    public void Dispose()
    {
        if (_connectorNotifier is not null)
        {
            _connectorNotifier.PropertyChanged -= ConnectorChanged;
            _connectorNotifier = null;
        }
        DetachDrawing();
    }

    private void ConnectorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IConnector.Parent))
        {
            RefreshDrawing();
            _changed();
        }
    }

    private void RefreshDrawing()
    {
        if (!ReferenceEquals(_drawing, _connector.Parent))
        {
            DetachDrawing();
            _drawing = _connector.Parent;
            _drawingNotifier = _drawing as INotifyPropertyChanged;
            if (_drawingNotifier is not null)
            {
                _drawingNotifier.PropertyChanged += DrawingChanged;
            }
        }
        RefreshMembership();
    }

    private void RefreshMembership()
    {
        var host = _drawing as IZipTieHost;
        var collection = host?.ZipTies as INotifyCollectionChanged;
        if (!ReferenceEquals(collection, _collection))
        {
            if (_collection is not null)
            {
                _collection.CollectionChanged -= CollectionChanged;
            }
            _collection = collection;
            if (_collection is not null)
            {
                _collection.CollectionChanged += CollectionChanged;
            }
        }

        var tie = host?.FindZipTie(_connector);
        if (ReferenceEquals(tie, _tie))
        {
            return;
        }
        if (_tie is not null)
        {
            _tie.PropertyChanged -= TieChanged;
        }
        _tie = tie;
        if (_tie is not null)
        {
            _tie.PropertyChanged += TieChanged;
        }
    }

    private void DetachDrawing()
    {
        if (_drawingNotifier is not null)
        {
            _drawingNotifier.PropertyChanged -= DrawingChanged;
            _drawingNotifier = null;
        }
        if (_collection is not null)
        {
            _collection.CollectionChanged -= CollectionChanged;
            _collection = null;
        }
        if (_tie is not null)
        {
            _tie.PropertyChanged -= TieChanged;
            _tie = null;
        }
        _drawing = null;
    }

    private void DrawingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IZipTieHost.ZipTies))
        {
            RefreshMembership();
            _changed();
        }
    }

    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshMembership();
        _changed();
    }

    private void TieChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IZipTie.Connectors))
        {
            RefreshMembership();
        }
        _changed();
    }
}

/// <summary>One subscription per drawing for layers and spatial indexes, with complete teardown.</summary>
internal sealed class ZipTieCollectionSubscription : IDisposable
{
    private readonly IZipTieHost _host;
    private readonly Action<IZipTie?> _changed;
    private readonly HashSet<IZipTie> _ties = new();
    private INotifyPropertyChanged? _notifier;
    private INotifyCollectionChanged? _collection;
    private IList<IZipTie>? _source;

    internal ZipTieCollectionSubscription(IZipTieHost host, Action<IZipTie?> changed)
    {
        _host = host;
        _changed = changed;
        _notifier = host as INotifyPropertyChanged;
        if (_notifier is not null)
        {
            _notifier.PropertyChanged += HostChanged;
        }
        Refresh();
    }

    internal void Refresh()
    {
        if (ReferenceEquals(_source, _host.ZipTies))
        {
            return;
        }
        DetachCollection();
        _source = _host.ZipTies;
        _collection = _source as INotifyCollectionChanged;
        if (_collection is not null)
        {
            _collection.CollectionChanged += CollectionChanged;
        }
        AttachTies();
    }

    public void Dispose()
    {
        if (_notifier is not null)
        {
            _notifier.PropertyChanged -= HostChanged;
            _notifier = null;
        }
        DetachCollection();
    }

    private void DetachCollection()
    {
        if (_collection is not null)
        {
            _collection.CollectionChanged -= CollectionChanged;
            _collection = null;
        }
        DetachTies();
        _source = null;
    }

    private void AttachTies()
    {
        if (_source is null)
        {
            return;
        }
        foreach (var tie in _source)
        {
            if (_ties.Add(tie))
            {
                tie.PropertyChanged += TieChanged;
            }
        }
    }

    private void DetachTies()
    {
        foreach (var tie in _ties)
        {
            tie.PropertyChanged -= TieChanged;
        }
        _ties.Clear();
    }

    private void HostChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IZipTieHost.ZipTies))
        {
            Refresh();
            if (_collection is null)
            {
                // Hosts may expose an ordinary IList and announce membership edits themselves.
                DetachTies();
                AttachTies();
            }
            _changed(null);
        }
        else if (e.PropertyName == nameof(IZipTieHost.SelectedZipTie))
        {
            _changed(_host.SelectedZipTie);
        }
    }

    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DetachTies();
        AttachTies();
        _changed(null);
    }

    private void TieChanged(object? sender, PropertyChangedEventArgs e)
        => _changed(e.PropertyName == nameof(IZipTie.Connectors) ? null : sender as IZipTie);
}
