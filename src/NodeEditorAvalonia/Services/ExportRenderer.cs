using System;
using System.IO;
#if NET6_0_OR_GREATER
using System.Reflection;
#endif
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
#if NET6_0_OR_GREATER
using Avalonia.Media;
using Avalonia.Skia.Helpers;
using SkiaSharp;
#endif

namespace NodeEditor.Services;

public static class ExportRenderer
{
#if NET6_0_OR_GREATER
    private static MethodInfo? _immediateRenderMethod;
    private static int _immediateRenderParameterCount;

    private static void Render(Control target, SKCanvas canvas, double dpi = 96)
    {
        using var drawingContextImpl = DrawingContextHelper.WrapSkiaCanvas(canvas, new Vector(dpi, dpi));
        var platformDrawingContextType = typeof(DrawingContext).Assembly.GetType("Avalonia.Media.PlatformDrawingContext");
        if (platformDrawingContextType is not null)
        {
            var drawingContext = (DrawingContext?)Activator.CreateInstance(
                platformDrawingContextType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object?[] { drawingContextImpl, true },
                null);
            if (drawingContext is not null)
            {
                if (!TryImmediateRender(target, drawingContext))
                {
                    throw new NotSupportedException("Immediate renderer was not found.");
                }
            }
        }
        else
        {
            throw new NotSupportedException("Platform drawing context type was not found.");
        }
    }

    private static bool TryImmediateRender(Control target, DrawingContext drawingContext)
    {
        _immediateRenderMethod ??= ResolveImmediateRenderMethod();
        if (_immediateRenderMethod is null)
        {
            return false;
        }

        var parameterCount = _immediateRenderParameterCount;
        if (parameterCount <= 2)
        {
            _immediateRenderMethod.Invoke(null, new object?[] { target, drawingContext });
            return true;
        }

        var args = new object?[parameterCount];
        args[0] = target;
        args[1] = drawingContext;
        for (var i = 2; i < parameterCount; i++)
        {
            args[i] = Type.Missing;
        }

        _immediateRenderMethod.Invoke(null, args);
        return true;
    }

    private static MethodInfo? ResolveImmediateRenderMethod()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType("Avalonia.Rendering.ImmediateRenderer", throwOnError: false)
                       ?? FindTypeByName(assembly, "ImmediateRenderer");

            if (type is null)
            {
                continue;
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (!string.Equals(method.Name, "Render", StringComparison.Ordinal))
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length < 2)
                {
                    continue;
                }

                if (!parameters[0].ParameterType.IsAssignableFrom(typeof(Control)))
                {
                    continue;
                }

                if (!parameters[1].ParameterType.IsAssignableFrom(typeof(DrawingContext)))
                {
                    continue;
                }

                var valid = true;
                for (var i = 2; i < parameters.Length; i++)
                {
                    if (!parameters[i].IsOptional)
                    {
                        valid = false;
                        break;
                    }
                }

                if (!valid)
                {
                    continue;
                }

                _immediateRenderParameterCount = parameters.Length;
                return method;
            }
        }

        return null;
    }

    private static Type? FindTypeByName(Assembly assembly, string name)
    {
        try
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name == name)
                {
                    return type;
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            foreach (var type in ex.Types)
            {
                if (type?.Name == name)
                {
                    return type;
                }
            }
        }

        return null;
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
