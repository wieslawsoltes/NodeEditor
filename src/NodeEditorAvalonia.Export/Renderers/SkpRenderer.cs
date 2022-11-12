using System.IO;
using Avalonia;
using Avalonia.Controls;
using SkiaSharp;

namespace NodeEditor.Export.Renderers;

public static class SkpRenderer
{
    public static void Render(Control target, Size size, Stream stream, double dpi = 96)
    {
        var bounds = SKRect.Create(new SKSize((float)size.Width, (float)size.Height));
        using var pictureRecorder = new SKPictureRecorder();
        using var canvas = pictureRecorder.BeginRecording(bounds);
        target.Measure(size);
        target.Arrange(new Rect(size));
        CanvasRenderer.Render(target, canvas, dpi);
        using var picture = pictureRecorder.EndRecording();
        picture.Serialize(stream);
    }
}
