using System;

namespace NodeEditor.Model;

public sealed class InkPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Pressure { get; set; } = 1.0;
    public long Timestamp { get; set; }

    public InkPoint()
    {
    }

    public InkPoint(double x, double y, double pressure = 1.0, long timestamp = 0)
    {
        X = x;
        Y = y;
        Pressure = pressure;
        Timestamp = timestamp;
    }
}
