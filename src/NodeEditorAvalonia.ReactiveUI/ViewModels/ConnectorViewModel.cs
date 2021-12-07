using System;
using System.Runtime.Serialization;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels;

[DataContract(IsReference = true)]
public class ConnectorViewModel : ReactiveObject, IConnector
{
    private string? _name;
    private IDrawingNode? _parent;
    private ConnectorOrientation _orientation;
    private IPin? _start;
    private IPin? _end;
    private double _offset = 50;

    public ConnectorViewModel() => ObservePins();

    private void ObservePins()
    {
        this.WhenAnyValue(x => x.Start)
            .Subscribe(start =>
            {
                if (start?.Parent is { })
                {
                    start.Parent.WhenAnyValue(x => x.X).Subscribe(_ => this.RaisePropertyChanged(nameof(Start)));
                    start.Parent.WhenAnyValue(x => x.Y).Subscribe(_ => this.RaisePropertyChanged(nameof(Start)));
                }
                else
                {
                    if (start is { })
                    {
                        start.WhenAnyValue(x => x.X).Subscribe(_ => this.RaisePropertyChanged(nameof(Start)));
                        start.WhenAnyValue(x => x.Y).Subscribe(_ => this.RaisePropertyChanged(nameof(Start)));
                    }
                }

                if (start is { })
                {
                    start.WhenAnyValue(x => x.Alignment).Subscribe(_ => this.RaisePropertyChanged(nameof(Start)));
                }
            });

        this.WhenAnyValue(x => x.End)
            .Subscribe(end =>
            {
                if (end?.Parent is { })
                {
                    end.Parent.WhenAnyValue(x => x.X).Subscribe(_ => this.RaisePropertyChanged(nameof(End)));
                    end.Parent.WhenAnyValue(x => x.Y).Subscribe(_ => this.RaisePropertyChanged(nameof(End)));
                }
                else
                {
                    if (end is { })
                    {
                        end.WhenAnyValue(x => x.X).Subscribe(_ => this.RaisePropertyChanged(nameof(End)));
                        end.WhenAnyValue(x => x.Y).Subscribe(_ => this.RaisePropertyChanged(nameof(End)));
                    }
                }

                if (end is { })
                {
                    end.WhenAnyValue(x => x.Alignment).Subscribe(_ => this.RaisePropertyChanged(nameof(End)));
                }
            });
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public IDrawingNode? Parent
    {
        get => _parent;
        set => this.RaiseAndSetIfChanged(ref _parent, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public ConnectorOrientation Orientation
    {
        get => _orientation;
        set => this.RaiseAndSetIfChanged(ref _orientation, value);
    }

    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public IPin? Start
    {
        get => _start;
        set => this.RaiseAndSetIfChanged(ref _start, value);
    }

    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public IPin? End
    {
        get => _end;
        set => this.RaiseAndSetIfChanged(ref _end, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public double Offset
    {
        get => _offset;
        set => this.RaiseAndSetIfChanged(ref _offset, value);
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
