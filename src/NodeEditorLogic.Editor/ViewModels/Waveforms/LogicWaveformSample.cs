using NodeEditorLogic.Models;

namespace NodeEditorLogic.ViewModels.Waveforms;

public sealed class LogicWaveformSample
{
    public LogicWaveformSample(int tick, string display, LogicValue aggregate)
    {
        Tick = tick;
        Display = display;
        Aggregate = aggregate;
    }

    public int Tick { get; }
    public string Display { get; }
    public LogicValue Aggregate { get; }
}
