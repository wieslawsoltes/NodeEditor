# Export and Storage

The editor includes helpers for exporting visual output and using the Avalonia storage APIs.

## ExportRenderer

`ExportRenderer` renders a control to several formats:

- PNG
- SVG (Skia required)
- SKP (Skia required)
- PDF (Skia required)
- XPS (Skia required)

Basic usage:

```csharp
using var stream = File.Create("drawing.png");
ExportRenderer.RenderPng(editorControl, new Size(1280, 720), stream);
```

For vector formats (SVG/PDF/XPS), the control is rendered through a Skia-backed drawing context.

## StorageService

`StorageService` wraps Avalonia storage providers and predefined file types:

- `StorageService.Json`
- `StorageService.ImagePng`
- `StorageService.ImageSvg`
- `StorageService.ImageSkp`
- `StorageService.Pdf`
- `StorageService.Xps`

Use `GetStorageProvider()` to resolve the provider in desktop or single-view scenarios.
