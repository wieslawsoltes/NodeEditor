using Avalonia.Controls;
using Avalonia.Rendering;
using SkiaSharp;

namespace NodeEditor.Export;

public static class CanvasRenderer
{
    public static void Render(Control target, SKCanvas canvas, double dpi = 96)
    {
        var renderTarget = new CanvasRenderTarget(canvas, dpi);
        ImmediateRenderer.Render(target, renderTarget);
    }
}
