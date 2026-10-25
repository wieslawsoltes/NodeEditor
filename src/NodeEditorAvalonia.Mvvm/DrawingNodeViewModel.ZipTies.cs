using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Newtonsoft.Json;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.Mvvm;

public partial class DrawingNodeViewModel : IZipTieHost, IZipTieCommands, IZipTieLaneProvider, IDrawingGraphTransferObserver
{
    private IList<IZipTie> _zipTies = new ObservableCollection<IZipTie>();
    private IZipTie? _selectedZipTie;
    private ZipTieEditor _zipTieEditor = null!;
    private readonly Dictionary<IConnector, (IZipTie Tie, int Lane)> _zipTieMembership = new();
    private readonly HashSet<IZipTie> _observedZipTies = new();
    private readonly BehaviorSubject<bool> _canCreateZipTie = new(false);
    private readonly BehaviorSubject<bool> _canEditZipTie = new(false);
    private readonly BehaviorSubject<bool> _canAddToZipTie = new(false);
    private readonly BehaviorSubject<bool> _canDetachFromZipTie = new(false);
    private readonly BehaviorSubject<bool> _canStraightenZipTie = new(false);
    private bool _updatingZipTies;

    /// <inheritdoc/>
    public IList<IZipTie> ZipTies
    {
        get => _zipTies;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (ReferenceEquals(value, _zipTies))
            {
                return;
            }

            if (_zipTies is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnZipTiesChanged;
            }

            _zipTies = value.IsReadOnly ? new ObservableCollection<IZipTie>(value) : value;
            if (_zipTies is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnZipTiesChanged;
            }

            RebuildZipTieMembership();
            OnPropertyChanged();
        }
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IZipTie? SelectedZipTie
    {
        get => _selectedZipTie;
        set
        {
            if (value is not null && !_zipTies.Contains(value))
            {
                throw new ArgumentException("The selected zip tie must belong to this drawing.", nameof(value));
            }

            if (SetProperty(ref _selectedZipTie, value))
            {
                UpdateZipTieCommandState();
            }
        }
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public ICommand CreateZipTieCommand { get; private set; } = null!;

    /// <inheritdoc/>
    [JsonIgnore]
    public ICommand RemoveZipTieCommand { get; private set; } = null!;

    /// <inheritdoc/>
    [JsonIgnore]
    public ICommand AddSelectedToZipTieCommand { get; private set; } = null!;

    /// <inheritdoc/>
    [JsonIgnore]
    public ICommand RemoveSelectedFromZipTieCommand { get; private set; } = null!;

    /// <inheritdoc/>
    [JsonIgnore]
    public ICommand ReverseZipTieCommand { get; private set; } = null!;

    /// <inheritdoc/>
    [JsonIgnore]
    public ICommand StraightenZipTieCommand { get; private set; } = null!;

    private void InitializeZipTies()
    {
        _zipTieEditor = new ZipTieEditor(this, this, () => new ZipTieViewModel());
        ((INotifyCollectionChanged)_zipTies).CollectionChanged += OnZipTiesChanged;
        CreateZipTieCommand = ReactiveCommand.Create(() =>
        {
            var selected = GetSelectedConnectors();
            if (selected is not null)
            {
                CreateZipTie(selected);
            }
        }, _canCreateZipTie.DistinctUntilChanged());
        RemoveZipTieCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedZipTie is { } tie)
            {
                RemoveZipTie(tie);
            }
        }, _canEditZipTie.DistinctUntilChanged());
        AddSelectedToZipTieCommand = ReactiveCommand.Create(AddSelectedToZipTie, _canAddToZipTie.DistinctUntilChanged());
        RemoveSelectedFromZipTieCommand = ReactiveCommand.Create(RemoveSelectedFromZipTie, _canDetachFromZipTie.DistinctUntilChanged());
        ReverseZipTieCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedZipTie is { } tie)
            {
                ReverseZipTie(tie);
            }
        }, _canEditZipTie.DistinctUntilChanged());
        StraightenZipTieCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedZipTie is { } tie)
            {
                StraightenZipTie(tie);
            }
        }, _canStraightenZipTie.DistinctUntilChanged());
    }

    /// <inheritdoc/>
    public IZipTie? FindZipTie(IConnector connector)
    {
        return connector is not null && _zipTieMembership.TryGetValue(connector, out var member) ? member.Tie : null;
    }

    /// <inheritdoc/>
    public int GetZipTieLane(IConnector connector)
    {
        return connector is not null && _zipTieMembership.TryGetValue(connector, out var member) ? member.Lane : -1;
    }

    /// <inheritdoc/>
    public IZipTie? CreateZipTie(IEnumerable<IConnector> connectors)
    {
        IZipTie? created = null;
        ExecuteWithUndo(() =>
        {
            created = _zipTieEditor.Create(connectors);
            RefreshNonObservableZipTies(created);
        });
        return created;
    }

    /// <inheritdoc/>
    public void RemoveZipTie(IZipTie tie) => ExecuteWithUndo(() =>
    {
        _zipTieEditor.Remove(tie);
        RefreshNonObservableZipTies(tie);
    });

    /// <inheritdoc/>
    public bool AddToZipTie(IZipTie tie, IConnector connector)
    {
        var changed = false;
        ExecuteWithUndo(() =>
        {
            changed = _zipTieEditor.Add(tie, connector);
            RefreshNonObservableZipTies(tie);
        });
        return changed;
    }

    /// <inheritdoc/>
    public bool RemoveFromZipTie(IZipTie tie, IConnector connector)
    {
        var changed = false;
        ExecuteWithUndo(() =>
        {
            changed = _zipTieEditor.Detach(tie, connector);
            RefreshNonObservableZipTies(tie);
        });
        return changed;
    }

    /// <inheritdoc/>
    public void ReverseZipTie(IZipTie tie) => ExecuteWithUndo(() =>
    {
        _zipTieEditor.Reverse(tie);
        RefreshNonObservableZipTies(tie);
    });

    /// <inheritdoc/>
    public void StraightenZipTie(IZipTie tie) => ExecuteWithUndo(() => _zipTieEditor.Straighten(tie));

    private void AddSelectedToZipTie()
    {
        if (SelectedZipTie is not { } tie || GetSelectedConnectors() is not { } selected)
        {
            return;
        }

        ExecuteWithUndo(() =>
        {
            _zipTieEditor.AddRange(tie, selected);
            RefreshNonObservableZipTies(tie);
        });
    }

    private void RemoveSelectedFromZipTie()
    {
        if (GetSelectedConnectors() is not { } selected)
        {
            return;
        }

        ExecuteWithUndo(() =>
        {
            var groups = new Dictionary<IZipTie, List<IConnector>>();
            foreach (var connector in selected)
            {
                if (FindZipTie(connector) is { } tie)
                {
                    if (!groups.TryGetValue(tie, out var members))
                    {
                        members = new List<IConnector>();
                        groups.Add(tie, members);
                    }

                    members.Add(connector);
                }
            }

            foreach (var group in groups)
            {
                _zipTieEditor.DetachRange(group.Key, group.Value);
                RefreshNonObservableZipTies(group.Key);
            }
        });
    }

    private void RefreshNonObservableZipTies(IZipTie? tie)
    {
        if (_zipTies is not INotifyCollectionChanged || tie?.Connectors is not INotifyCollectionChanged)
        {
            RebuildZipTieMembership();
            OnPropertyChanged(nameof(ZipTies));
        }
    }

    private void OnZipTiesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_updatingZipTies)
        {
            return;
        }

        RebuildZipTieMembership();
        OnPropertyChanged(nameof(ZipTies));
    }

    private void OnZipTiePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_updatingZipTies)
        {
            return;
        }

        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IZipTie.Connectors))
        {
            RebuildZipTieMembership();
            OnPropertyChanged(nameof(ZipTies));
        }
        else if (ReferenceEquals(sender, SelectedZipTie)
                 && (e.PropertyName == nameof(IZipTie.IsLocked)
                     || e.PropertyName == nameof(IZipTie.IsVisible)
                     || e.PropertyName == nameof(IZipTie.Waypoints)))
        {
            UpdateZipTieCommandState();
        }
    }

    private void RebuildZipTieMembership()
    {
        if (_updatingZipTies)
        {
            return;
        }

        _updatingZipTies = true;
        try
        {
            foreach (var oldTie in _observedZipTies)
            {
                oldTie.PropertyChanged -= OnZipTiePropertyChanged;
            }

            _observedZipTies.Clear();
            _zipTieMembership.Clear();
            List<IZipTie>? normalizedTies = null;
            var tieIndex = 0;
            foreach (var tie in _zipTies)
            {
                if (tie is null || tie.Start is null || tie.End is null || !_observedZipTies.Add(tie))
                {
                    normalizedTies ??= CopyZipTiePrefix(tieIndex);
                    tieIndex++;
                    continue;
                }

                // Custom IList implementations and imported documents may
                // contain duplicate or null references. First ownership wins;
                // publish a repaired collection atomically rather than mutating
                // an ObservableCollection during its CollectionChanged event.
                var members = new List<IConnector>(tie.Connectors?.Count ?? 0);
                var seen = new HashSet<IConnector>();
                if (tie.Connectors is not null)
                {
                    foreach (var connector in tie.Connectors)
                    {
                        if (connector is not null && seen.Add(connector) && !_zipTieMembership.ContainsKey(connector))
                        {
                            members.Add(connector);
                        }
                    }
                }

                if (members.Count < 2)
                {
                    _observedZipTies.Remove(tie);
                    normalizedTies ??= CopyZipTiePrefix(tieIndex);
                    tieIndex++;
                    continue;
                }

                if (tie.Connectors is null || members.Count != tie.Connectors.Count)
                {
                    tie.Connectors = new ObservableCollection<IConnector>(members);
                }

                if (tie.Waypoints is null)
                {
                    tie.Waypoints = new ObservableCollection<ConnectorPoint>();
                }

                tie.PropertyChanged += OnZipTiePropertyChanged;
                for (var lane = 0; lane < members.Count; lane++)
                {
                    _zipTieMembership.Add(members[lane], (tie, lane));
                }

                normalizedTies?.Add(tie);
                tieIndex++;
            }

            if (normalizedTies is not null)
            {
                if (_zipTies is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= OnZipTiesChanged;
                }

                _zipTies = new ObservableCollection<IZipTie>(normalizedTies);
                ((INotifyCollectionChanged)_zipTies).CollectionChanged += OnZipTiesChanged;
            }

            if (_selectedZipTie is not null && !_zipTies.Contains(_selectedZipTie))
            {
                SelectedZipTie = null;
            }

            UpdateZipTieCommandState();
        }
        finally
        {
            _updatingZipTies = false;
        }
    }

    private List<IZipTie> CopyZipTiePrefix(int count)
    {
        var result = new List<IZipTie>(_zipTies.Count);
        for (var i = 0; i < count; i++)
        {
            result.Add(_zipTies[i]);
        }

        return result;
    }

    private void UpdateZipTieCommandState()
    {
        var available = 0;
        var canDetach = false;
        if (GetSelectedConnectors() is { } selected)
        {
            foreach (var connector in selected)
            {
                if (connector is null || !connector.IsVisible || connector.IsLocked || connector.Start?.Parent is null || connector.End?.Parent is null)
                {
                    continue;
                }

                var tie = FindZipTie(connector);
                if (tie is null)
                {
                    available++;
                }
                else if (tie.IsVisible && !tie.IsLocked)
                {
                    canDetach = true;
                }
            }
        }

        var canEdit = SelectedZipTie is { IsVisible: true, IsLocked: false };
        _canCreateZipTie.OnNext(available >= 2);
        _canEditZipTie.OnNext(canEdit);
        _canAddToZipTie.OnNext(canEdit && available > 0);
        _canDetachFromZipTie.OnNext(canDetach);
        _canStraightenZipTie.OnNext(canEdit && SelectedZipTie!.Waypoints.Count > 0);
    }

    partial void OnConnectorsChanged(IList<IConnector>? oldValue, IList<IConnector>? newValue)
    {
        if (oldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= OnDrawingConnectorsChanged;
        }

        if (newValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += OnDrawingConnectorsChanged;
        }

        if (!_isRestoring && oldValue is not null)
        {
            PruneZipTies();
        }

        UpdateZipTieCommandState();
    }

    private void OnDrawingConnectorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_isRestoring && (e.Action == NotifyCollectionChangedAction.Remove
                              || e.Action == NotifyCollectionChangedAction.Replace
                              || e.Action == NotifyCollectionChangedAction.Reset))
        {
            PruneZipTies();
        }
    }

    private void PruneZipTies()
    {
        RebuildZipTieMembership();
        var available = Connectors is not null ? new HashSet<IConnector>(Connectors) : new HashSet<IConnector>();
        _updatingZipTies = true;
        try
        {
            for (var i = _zipTies.Count - 1; i >= 0; i--)
            {
                var tie = _zipTies[i];
                for (var memberIndex = tie.Connectors.Count - 1; memberIndex >= 0; memberIndex--)
                {
                    if (!available.Contains(tie.Connectors[memberIndex]))
                    {
                        tie.Connectors.RemoveAt(memberIndex);
                    }
                }

                if (tie.Connectors.Count < 2)
                {
                    _zipTies.RemoveAt(i);
                }
            }
        }
        finally
        {
            _updatingZipTies = false;
        }

        RebuildZipTieMembership();
        OnPropertyChanged(nameof(ZipTies));
    }

    /// <inheritdoc/>
    public void PrepareGraphTransfer(IEnumerable<INode>? nodes, IEnumerable<IConnector>? connectors)
    {
        // Reference-preserving snapshots include parent drawings through the
        // nodes/wires. Once their children are reparented to this live document,
        // those temporary parents must stop observing the shared restored graph.
        var parents = new HashSet<DrawingNodeViewModel>();
        if (nodes is not null)
        {
            foreach (var node in nodes)
            {
                if (node?.Parent is DrawingNodeViewModel parent && !ReferenceEquals(parent, this))
                {
                    parents.Add(parent);
                }
            }
        }

        if (connectors is not null)
        {
            foreach (var connector in connectors)
            {
                if (connector?.Parent is DrawingNodeViewModel parent && !ReferenceEquals(parent, this))
                {
                    parents.Add(parent);
                }
            }
        }

        foreach (var parent in parents)
        {
            parent.DetachZipTieGraphSubscriptions();
        }
    }

    private void DetachZipTieGraphSubscriptions()
    {
        if (_zipTies is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= OnZipTiesChanged;
        }

        if (Connectors is INotifyCollectionChanged connectors)
        {
            connectors.CollectionChanged -= OnDrawingConnectorsChanged;
        }

        foreach (var tie in _observedZipTies)
        {
            tie.PropertyChanged -= OnZipTiePropertyChanged;
        }

        _observedZipTies.Clear();
        _zipTieMembership.Clear();
    }
}
