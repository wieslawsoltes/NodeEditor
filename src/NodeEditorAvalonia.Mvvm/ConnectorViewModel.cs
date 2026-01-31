using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;
using ReactiveMarbles.PropertyChanged;

namespace NodeEditor.Mvvm;

public partial class ConnectorViewModel : ObservableObject, IConnector
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private IDrawingNode? _parent;
    [ObservableProperty] private ConnectorOrientation _orientation;
    [ObservableProperty] private ConnectorStyle _style = ConnectorStyle.Bezier;
    [ObservableProperty] private ConnectorRoutingMode _routingMode = ConnectorRoutingMode.Auto;
    [ObservableProperty] private ConnectorArrowStyle _startArrow;
    [ObservableProperty] private ConnectorArrowStyle _endArrow;
    [ObservableProperty] private IPin? _start;
    [ObservableProperty] private IPin? _end;
    [ObservableProperty] private double _offset = 50;
    [ObservableProperty] private IList<ConnectorPoint> _waypoints = new ObservableCollection<ConnectorPoint>();
    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private bool _isLocked;

    public event EventHandler<ConnectorCreatedEventArgs>? Created;

    public event EventHandler<ConnectorRemovedEventArgs>? Removed;

    public event EventHandler<ConnectorSelectedEventArgs>? Selected;

    public event EventHandler<ConnectorDeselectedEventArgs>? Deselected;

    public event EventHandler<ConnectorStartChangedEventArgs>? StartChanged;

    public event EventHandler<ConnectorEndChangedEventArgs>? EndChanged;

    private readonly SerialDisposable _startSubscriptions = new();
    private readonly SerialDisposable _endSubscriptions = new();

    public ConnectorViewModel() => ObservePins();

    partial void OnStartChanged(IPin? value)
    {
        OnStartChanged();
    }

    partial void OnEndChanged(IPin? value)
    {
        OnEndChanged();
    }

    private void ObservePins()
    {
        this.WhenChanged(x => x.Start)
            .DistinctUntilChanged()
            .Subscribe(start =>
            {
                _startSubscriptions.Disposable = ObservePin(start, nameof(Start));
            });

        this.WhenChanged(x => x.End)
            .DistinctUntilChanged()
            .Subscribe(end =>
            {
                _endSubscriptions.Disposable = ObservePin(end, nameof(End));
            });
    }

    private IDisposable ObservePin(IPin? pin, string propertyName)
    {
        var disposables = new CompositeDisposable();

        if (pin is null)
        {
            return disposables;
        }

        var parentSubscriptions = new SerialDisposable();
        disposables.Add(parentSubscriptions);

        IDisposable CreateParentSubscriptions(INode? parent)
        {
            if (parent is NodeViewModel parentViewModel)
            {
                var parentDisposables = new CompositeDisposable
                {
                    parentViewModel.WhenChanged(x => x.X)
                        .DistinctUntilChanged()
                        .Subscribe(_ => OnPropertyChanged(propertyName)),
                    parentViewModel.WhenChanged(x => x.Y)
                        .DistinctUntilChanged()
                        .Subscribe(_ => OnPropertyChanged(propertyName)),
                    parentViewModel.WhenChanged(x => x.Rotation)
                        .DistinctUntilChanged()
                        .Subscribe(_ => OnPropertyChanged(propertyName))
                };
                return parentDisposables;
            }

            if (parent is INotifyPropertyChanged parentNotify)
            {
                PropertyChangedEventHandler handler = (_, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName)
                        || e.PropertyName == nameof(INode.X)
                        || e.PropertyName == nameof(INode.Y)
                        || e.PropertyName == nameof(INode.Rotation))
                    {
                        OnPropertyChanged(propertyName);
                    }
                };

                parentNotify.PropertyChanged += handler;
                return Disposable.Create(() => parentNotify.PropertyChanged -= handler);
            }

            return Disposable.Empty;
        }

        void AttachParent(INode? parent)
        {
            parentSubscriptions.Disposable = CreateParentSubscriptions(parent);
        }

        AttachParent(pin.Parent);

        if (pin is PinViewModel pinViewModel)
        {
            disposables.Add(pinViewModel.WhenChanged(x => x.X)
                .DistinctUntilChanged()
                .Subscribe(_ => OnPropertyChanged(propertyName)));
            disposables.Add(pinViewModel.WhenChanged(x => x.Y)
                .DistinctUntilChanged()
                .Subscribe(_ => OnPropertyChanged(propertyName)));
            disposables.Add(pinViewModel.WhenChanged(x => x.Alignment)
                .DistinctUntilChanged()
                .Subscribe(_ => OnPropertyChanged(propertyName)));
            disposables.Add(pinViewModel.WhenChanged(x => x.Parent)
                .DistinctUntilChanged()
                .Subscribe(parent =>
                {
                    AttachParent(parent);
                    OnPropertyChanged(propertyName);
                }));
        }
        else if (pin is INotifyPropertyChanged pinNotify)
        {
            PropertyChangedEventHandler handler = (_, e) =>
            {
                if (string.IsNullOrEmpty(e.PropertyName)
                    || e.PropertyName == nameof(IPin.X)
                    || e.PropertyName == nameof(IPin.Y)
                    || e.PropertyName == nameof(IPin.Alignment))
                {
                    OnPropertyChanged(propertyName);
                }

                if (string.IsNullOrEmpty(e.PropertyName)
                    || e.PropertyName == nameof(IPin.Parent))
                {
                    AttachParent(pin.Parent);
                    OnPropertyChanged(propertyName);
                }
            };

            pinNotify.PropertyChanged += handler;
            disposables.Add(Disposable.Create(() => pinNotify.PropertyChanged -= handler));
        }

        return disposables;
    }

    public virtual bool CanSelect()
    {
        return IsVisible;
    }

    public virtual bool CanRemove()
    {
        return IsVisible && !IsLocked;
    }

    public void OnCreated()
    {
        Created?.Invoke(this, new ConnectorCreatedEventArgs(this));
    }

    public void OnRemoved()
    {
        Removed?.Invoke(this, new ConnectorRemovedEventArgs(this));
    }

    public void OnSelected()
    {
        Selected?.Invoke(this, new ConnectorSelectedEventArgs(this));
    }

    public void OnDeselected()
    {
        Deselected?.Invoke(this, new ConnectorDeselectedEventArgs(this));
    }

    public void OnStartChanged()
    {
        StartChanged?.Invoke(this, new ConnectorStartChangedEventArgs(this));
    }

    public void OnEndChanged()
    {
        EndChanged?.Invoke(this, new ConnectorEndChangedEventArgs(this));
    }
}
