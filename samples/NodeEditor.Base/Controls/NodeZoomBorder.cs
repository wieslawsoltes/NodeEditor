using System.Globalization;
using Avalonia.Controls.PanAndZoom;

namespace NodeEditorDemo.Controls;

public class NodeZoomBorder : ZoomBorder
{
    public void ResetZoomCommand()
    {
        ResetMatrix();
    }

    public void ZoomToCommand(string value)
    {
        if (Child == null)
        {
            return;
        }

        ResetMatrix();

        var ratio = double.Parse(value, CultureInfo.InvariantCulture);
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