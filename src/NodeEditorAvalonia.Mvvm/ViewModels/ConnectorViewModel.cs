using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;

namespace NodeEditor.ViewModels;

[ObservableObject]
public partial class ConnectorViewModel : IConnector
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private IDrawingNode? _parent;
    [ObservableProperty] private ConnectorOrientation _orientation;
    [ObservableProperty] private IPin? _start;
    [ObservableProperty] private IPin? _end;
    [ObservableProperty] private double _offset = 50;

    public ConnectorViewModel() => ObservePins();

    private void ObservePins()
    {
        this.WhenChanged(x => x.Start)
            .Subscribe(start =>
            {
                if (start?.Parent is { })
                {
                    (start.Parent as NodeViewModel)?.WhenChanged(x => x.X).Subscribe(_ => OnPropertyChanged(nameof(Start)));
                    (start.Parent as NodeViewModel)?.WhenChanged(x => x.Y).Subscribe(_ => OnPropertyChanged(nameof(Start)));
                }
                else
                {
                    if (start is { })
                    {
                        (start as PinViewModel)?.WhenChanged(x => x.X).Subscribe(_ => OnPropertyChanged(nameof(Start)));
                        (start as PinViewModel)?.WhenChanged(x => x.Y).Subscribe(_ => OnPropertyChanged(nameof(Start)));
                    }
                }

                if (start is { })
                {
                    (start as PinViewModel)?.WhenChanged(x => x.Alignment).Subscribe(_ => OnPropertyChanged(nameof(Start)));
                }
            });

        this.WhenChanged(x => x.End)
            .Subscribe(end =>
            {
                if (end?.Parent is { })
                {
                    (end.Parent as NodeViewModel)?.WhenChanged(x => x.X).Subscribe(_ => OnPropertyChanged(nameof(End)));
                    (end.Parent as NodeViewModel)?.WhenChanged(x => x.Y).Subscribe(_ => OnPropertyChanged(nameof(End)));
                }
                else
                {
                    if (end is { })
                    {
                        (end as PinViewModel)?.WhenChanged(x => x.X).Subscribe(_ => OnPropertyChanged(nameof(End)));
                        (end as PinViewModel)?.WhenChanged(x => x.Y).Subscribe(_ => OnPropertyChanged(nameof(End)));
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
}
