using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorLogic.ViewModels.Waveforms;

public partial class LogicWaveformSignal : ObservableObject
{
    public LogicWaveformSignal(string key, string name, bool isBus, int busWidth)
    {
        Key = key;
        _name = name;
        _isBus = isBus;
        _busWidth = busWidth;
    }

    public string Key { get; }

    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isBus;
    [ObservableProperty] private int _busWidth;

    public ObservableCollection<LogicWaveformSample> Samples { get; } = new();

    public void AddSample(LogicWaveformSample sample, int maxSamples)
    {
        Samples.Add(sample);
        while (Samples.Count > maxSamples)
        {
            Samples.RemoveAt(0);
        }
    }
}
