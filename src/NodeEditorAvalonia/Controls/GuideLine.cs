using Avalonia;

namespace NodeEditor.Controls;

public readonly struct GuideLine
{
    public GuideLine(Point start, Point end)
    {
        Start = start;
        End = end;
    }

    public Point Start { get; }

    public Point End { get; }
}
