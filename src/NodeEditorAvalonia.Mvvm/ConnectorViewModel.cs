using System;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;
using ReactiveMarbles.PropertyChanged;

namespace NodeEditor.Mvvm;

[ObservableObject]
public partial class ConnectorViewModel : IConnector
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private IDrawingNode? _parent;
    [ObservableProperty] private ConnectorOrientation _orientation;
    [ObservableProperty] private IPin? _start;
    [ObservableProperty] private IPin? _end;
    [ObservableProperty] private double _offset = 50;

    public event EventHandler<ConnectorCreatedEventArgs>? Created;

    public event EventHandler<ConnectorRemovedEventArgs>? Removed;

    public event EventHandler<ConnectorSelectedEventArgs>? Selected;

    public event EventHandler<ConnectorDeselectedEventArgs>? Deselected;

    public event EventHandler<ConnectorStartChangedEventArgs>? StartChanged;

    public event EventHandler<ConnectorEndChangedEventArgs>? EndChanged;

    public ConnectorViewModel() => ObservePins();

    private void ObservePins()
    {
        this.WhenChanged(x => x.Start)
            .DistinctUntilChanged()
            .Subscribe(start =>
            {
                if (start?.Parent is { })
                {
                    (start.Parent as NodeViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
                    (start.Parent as NodeViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
                }
                else
                {
                    if (start is { })
                    {
                        (start as PinViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
                        (start as PinViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
                    }
                }

                if (start is { })
                {
                    (start as PinViewModel)?.WhenChanged(x => x.Alignment).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
                }
            });

        this.WhenChanged(x => x.End)
            .DistinctUntilChanged()
            .Subscribe(end =>
            {
                if (end?.Parent is { })
                {
                    (end.Parent as NodeViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(End)));
                    (end.Parent as NodeViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(End)));
                }
                else
                {
                    if (end is { })
                    {
                        (end as PinViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(End)));
                        (end as PinViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(End)));
                    }
                }

                if (end is { })
                {
                    (end as PinViewModel)?.WhenChanged(x => x.Alignment).Subscribe(_ => OnPropertyChanged(nameof(End)));
                }
            });
    }

    public virtual bool CanSelect()
    {
        return true;
    }

    public virtual bool CanRemove()
    {
        return true;
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
