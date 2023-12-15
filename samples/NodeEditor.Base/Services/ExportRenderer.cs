/*
 * NodeEditor A node editor control for Avalonia.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia.Helpers;
using SkiaSharp;

namespace NodeEditorDemo.Services;

internal static class ExportRenderer
{
    private static void Render(Control target, SKCanvas canvas, double dpi = 96)
    {
        using var drawingContextImpl = DrawingContextHelper.WrapSkiaCanvas(canvas, new Vector(dpi, dpi));
        var platformDrawingContextType = typeof(DrawingContext).Assembly.GetType("Avalonia.Media.PlatformDrawingContext");
        if (platformDrawingContextType is { })
        {
            var drawingContext = (DrawingContext?)Activator.CreateInstance(
                platformDrawingContextType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
                null,
                new object?[] { drawingContextImpl, true }, 
                null);
            if (drawingContext is { })
            {
                // TODO: ImmediateRenderer.Render(target, drawingContext);
            }
        }
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
