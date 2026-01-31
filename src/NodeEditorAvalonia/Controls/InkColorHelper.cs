using System;
using Avalonia.Media;

namespace NodeEditor.Controls;

internal static class InkColorHelper
{
    public static Color ToColor(uint argb, double opacity)
    {
        var a = (byte)((argb >> 24) & 0xFF);
        var r = (byte)((argb >> 16) & 0xFF);
        var g = (byte)((argb >> 8) & 0xFF);
        var b = (byte)(argb & 0xFF);
        var scaled = (int)Math.Round(a * opacity);
        if (scaled < 0)
        {
            scaled = 0;
        }
        else if (scaled > 255)
        {
            scaled = 255;
        }

        return Color.FromArgb((byte)scaled, r, g, b);
    }
}
