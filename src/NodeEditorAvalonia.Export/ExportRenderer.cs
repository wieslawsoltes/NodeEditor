using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Skia.Helpers;
using SkiaSharp;

namespace NodeEditor.Export;

public static class ExportRenderer
{
    private class CanvasRenderTarget : IRenderTarget
    {
        private readonly SKCanvas _canvas;
        private readonly double _dpi;

        public CanvasRenderTarget(SKCanvas canvas, double dpi)
        {
            _canvas = canvas;
            _dpi = dpi;
        }

        public IDrawingContextImpl CreateDrawingContext(IVisualBrushRenderer? visualBrushRenderer)
        {
            return DrawingContextHelper.WrapSkiaCanvas(_canvas, new Vector(_dpi, _dpi), visualBrushRenderer);
        }

        public void Dispose()
        {
        }
    }

    private static void Render(Control target, SKCanvas canvas, double dpi = 96)
    {
        var renderTarget = new CanvasRenderTarget(canvas, dpi);
        ImmediateRenderer.Render(target, renderTarget);
    }

    public static void RenderPng(Control target, Size size, Stream stream, double dpi = 96)
    {
        var pixelSize = new PixelSize((int)size.Width, (int)size.Height);
        var dpiVector = new Vector(dpi, dpi);
        using var bitmap = new RenderTargetBitmap(pixelSize, dpiVector);
        target.Measure(size);
        target.Arrange(new Rect(size));
        bitmap.Render(target);
        bitmap.Save(stream);
    }

    public static void RenderSvg(Control target, Size size, Stream stream, double dpi = 96)
    {
        using var managedWStream = new SKManagedWStream(stream);
        var bounds = SKRect.Create(new SKSize((float)size.Width, (float)size.Height));
        using var canvas = SKSvgCanvas.Create(bounds, managedWStream);
        target.Measure(size);
        target.Arrange(new Rect(size));
        Render(target, canvas, dpi);
    }

    public static void RenderSkp(Control target, Size size, Stream stream, double dpi = 96)
    {
        var bounds = SKRect.Create(new SKSize((float)size.Width, (float)size.Height));
        using var pictureRecorder = new SKPictureRecorder();
        using var canvas = pictureRecorder.BeginRecording(bounds);
        target.Measure(size);
        target.Arrange(new Rect(size));
        Render(target, canvas, dpi);
        using var picture = pictureRecorder.EndRecording();
        picture.Serialize(stream);
    }

    public static void RenderPdf(Control target, Size size, Stream stream, double dpi = 72)
    {
        using var managedWStream = new SKManagedWStream(stream);
        using var document = SKDocument.CreatePdf(stream, (float)dpi);
        using var canvas = document.BeginPage((float)size.Width, (float)size.Height);
        target.Measure(size);
        target.Arrange(new Rect(size));
        Render(target, canvas, dpi);
    }
    
    public static void RenderXps(Control target, Size size, Stream stream, double dpi = 72)
    {
        using var managedWStream = new SKManagedWStream(stream);
        using var document = SKDocument.CreateXps(stream, (float)dpi);
        using var canvas = document.BeginPage((float)size.Width, (float)size.Height);
        target.Measure(size);
        target.Arrange(new Rect(size));
        Render(target, canvas, dpi);
    }
}
