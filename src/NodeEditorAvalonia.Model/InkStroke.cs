using System.Collections.Generic;

namespace NodeEditor.Model;

public sealed class InkStroke
{
    public IList<InkPoint> Points { get; set; } = new List<InkPoint>();
    public uint Color { get; set; } = 0xFF000000;
    public double Thickness { get; set; } = 2.0;
    public double Opacity { get; set; } = 1.0;
    public string? Name { get; set; }
}
