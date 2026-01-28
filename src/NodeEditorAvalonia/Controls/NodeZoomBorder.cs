using System.Globalization;
using Avalonia.Controls.PanAndZoom;

namespace NodeEditor.Controls;

public class NodeZoomBorder : ZoomBorder
{
    public void ResetZoomCommand()
    {
        ResetMatrix();
    }

    public void ZoomToCommand(object? value)
    {
        if (Child == null || value is not string s)
        {
            return;
        }

        if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var ratio) || ratio <= 0.0)
        {
            return;
        }
        ResetMatrix();
        var x = Child.Bounds.Width / 2.0;
        var y = Child.Bounds.Height / 2.0;

        ZoomTo(ratio, x, y);
    }

    public void ZoomInCommand()
    {
        ZoomIn();
    }

    public void ZoomOutCommand()
    {
        ZoomOut();
    }

    public void FitCanvasCommand()
    {
        Uniform();
    }

    public void FitToFillCommand()
    {
        UniformToFill();
    }

    public void FillCanvasCommand()
    {
        Fill();
    }
}
