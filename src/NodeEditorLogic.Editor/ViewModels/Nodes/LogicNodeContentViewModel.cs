using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorLogic.Models;
using NodeEditorLogic.Services;

namespace NodeEditorLogic.ViewModels.Nodes;

public partial class LogicNodeContentViewModel : ViewModelBase
{
    [JsonIgnore] public NodeViewModel? HostNode { get; set; }
    [ObservableProperty] private string? _title;
    [ObservableProperty] private string? _subtitle;
    [ObservableProperty] private string? _componentId;
    [ObservableProperty] private int _propagationDelay = 0;
    [ObservableProperty] private ObservableCollection<LogicPinViewModel> _inputPins = new();
    [ObservableProperty] private ObservableCollection<LogicPinViewModel> _outputPins = new();

    partial void OnPropagationDelayChanged(int value)
    {
        if (value < 0)
        {
            PropagationDelay = 0;
        }
    }

    [RelayCommand]
    private void AddInputPin()
    {
        AddPin(LogicPinKind.Input, "In");
    }

    [RelayCommand]
    private void AddOutputPin()
    {
        AddPin(LogicPinKind.Output, "Out");
    }

    [RelayCommand]
    private void RemoveInputPin(LogicPinViewModel? pin)
    {
        RemovePin(pin, InputPins);
    }

    [RelayCommand]
    private void RemoveOutputPin(LogicPinViewModel? pin)
    {
        RemovePin(pin, OutputPins);
    }

    private void AddPin(LogicPinKind kind, string baseName)
    {
        if (HostNode is null)
        {
            return;
        }

        var name = GetNextPinName(kind, baseName);
        var pin = new LogicPinViewModel
        {
            Name = name,
            Parent = HostNode,
            Width = 10,
            Height = 10,
            Kind = kind,
            Alignment = kind == LogicPinKind.Input ? PinAlignment.Left : PinAlignment.Right,
            Value = LogicValue.Unknown
        };

        HostNode.Pins ??= new ObservableCollection<NodeEditor.Model.IPin>();
        HostNode.Pins.Add(pin);
        pin.OnCreated();

        if (kind == LogicPinKind.Input)
        {
            InputPins.Add(pin);
        }
        else
        {
            OutputPins.Add(pin);
        }

        LogicNodeFactory.RefreshNodeLayout(HostNode, this);
    }

    private void RemovePin(LogicPinViewModel? pin, ObservableCollection<LogicPinViewModel> collection)
    {
        if (HostNode is null || pin is null)
        {
            return;
        }

        collection.Remove(pin);
        HostNode.Pins?.Remove(pin);

        if (HostNode.Parent is IDrawingNode drawing && drawing.Connectors is not null)
        {
            for (var i = drawing.Connectors.Count - 1; i >= 0; i--)
            {
                var connector = drawing.Connectors[i];
                if (connector.Start == pin || connector.End == pin)
                {
                    connector.Start?.OnDisconnected();
                    connector.End?.OnDisconnected();
                    connector.OnRemoved();
                    drawing.Connectors.RemoveAt(i);
                }
            }
        }

        LogicNodeFactory.RefreshNodeLayout(HostNode, this);
        pin.OnRemoved();
    }

    private string GetNextPinName(LogicPinKind kind, string baseName)
    {
        var index = 1;
        var list = kind == LogicPinKind.Input ? InputPins : OutputPins;
        var candidate = $"{baseName}{index}";

        while (list.Any(pin => string.Equals(pin.Name, candidate, System.StringComparison.OrdinalIgnoreCase)))
        {
            index++;
            candidate = $"{baseName}{index}";
        }

        return candidate;
    }
}
