using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using NodeEditor.Controls;
#if NET6_0_OR_GREATER
using Avalonia.Skia.Helpers;
using SkiaSharp;
#endif

namespace NodeEditor.Services;

public static class ExportRenderer
{
    internal static readonly AttachedProperty<bool> IsExportPassProperty =
        AvaloniaProperty.RegisterAttached<ExportRoot, Control, bool>("IsExportPass", false, inherits: true);

    private static IDisposable? BeginExport(Control target)
    {
        // A scoped value preserves bindings/local values and restores the live view even if IO fails.
        // Inheritance reaches layers created during Measure/Arrange, including standalone drawings.
        return target.SetValue(IsExportPassProperty, true, BindingPriority.Animation);
    }

#if NET6_0_OR_GREATER
    private static void Render(Control target, SKCanvas canvas, double dpi = 96)
    {
        DrawingContextHelper
            .RenderAsync(canvas, target, new Rect(target.Bounds.Size), new Vector(dpi, dpi))
            .GetAwaiter()
            .GetResult();
    }
#endif

    private static Size NormalizeSize(Size size)
    {
        var width = size.Width;
        var height = size.Height;

        if (double.IsNaN(width) || double.IsInfinity(width) || width <= 0.0)
        {
            width = 1.0;
        }

        if (double.IsNaN(height) || double.IsInfinity(height) || height <= 0.0)
        {
            height = 1.0;
        }

        return new Size(width, height);
    }

    public static void RenderPng(Control target, Size size, Stream stream, double dpi = 96)
    {
        using var export = BeginExport(target);
        var normalized = NormalizeSize(size);
        var pixelSize = new PixelSize((int)normalized.Width, (int)normalized.Height);
        var dpiVector = new Vector(dpi, dpi);
        using var bitmap = new RenderTargetBitmap(pixelSize, dpiVector);
        target.Measure(normalized);
        target.Arrange(new Rect(normalized));
        bitmap.Render(target);
        bitmap.Save(stream);
    }

    public static void RenderSvg(Control target, Size size, Stream stream, double dpi = 96)
    {
#if NET6_0_OR_GREATER
        using var export = BeginExport(target);
        using var managedWStream = new SKManagedWStream(stream);
        var normalized = NormalizeSize(size);
        var bounds = SKRect.Create(new SKSize((float)normalized.Width, (float)normalized.Height));
        using var canvas = SKSvgCanvas.Create(bounds, managedWStream);
        target.Measure(normalized);
        target.Arrange(new Rect(normalized));
        Render(target, canvas, dpi);
#else
        throw new NotSupportedException("SVG export requires Skia.");
#endif
    }

    public static void RenderSkp(Control target, Size size, Stream stream, double dpi = 96)
    {
#if NET6_0_OR_GREATER
        using var export = BeginExport(target);
        var normalized = NormalizeSize(size);
        var bounds = SKRect.Create(new SKSize((float)normalized.Width, (float)normalized.Height));
        using var pictureRecorder = new SKPictureRecorder();
        using var canvas = pictureRecorder.BeginRecording(bounds);
        target.Measure(normalized);
        target.Arrange(new Rect(normalized));
        Render(target, canvas, dpi);
        using var picture = pictureRecorder.EndRecording();
        picture.Serialize(stream);
#else
        throw new NotSupportedException("SKP export requires Skia.");
#endif
    }

    public static void RenderPdf(Control target, Size size, Stream stream, double dpi = 72)
    {
#if NET6_0_OR_GREATER
        using var export = BeginExport(target);
        var normalized = NormalizeSize(size);
        using var document = SKDocument.CreatePdf(stream, (float)dpi);
        using var canvas = document.BeginPage((float)normalized.Width, (float)normalized.Height);
        target.Measure(normalized);
        target.Arrange(new Rect(normalized));
        Render(target, canvas, dpi);
#else
        throw new NotSupportedException("PDF export requires Skia.");
#endif
    }

    public static void RenderXps(Control target, Size size, Stream stream, double dpi = 72)
    {
#if NET6_0_OR_GREATER
        using var export = BeginExport(target);
        var normalized = NormalizeSize(size);
        using var document = SKDocument.CreateXps(stream, (float)dpi);
        using var canvas = document.BeginPage((float)normalized.Width, (float)normalized.Height);
        target.Measure(normalized);
        target.Arrange(new Rect(normalized));
        Render(target, canvas, dpi);
#else
        throw new NotSupportedException("XPS export requires Skia.");
#endif
    }
}
