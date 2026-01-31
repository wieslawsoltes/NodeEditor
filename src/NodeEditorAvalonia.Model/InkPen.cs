using System;

namespace NodeEditor.Model;

public sealed class InkPen
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Pen";
    public uint Color { get; set; } = 0xFF000000;
    public double Thickness { get; set; } = 2.0;
    public double Opacity { get; set; } = 1.0;
}
