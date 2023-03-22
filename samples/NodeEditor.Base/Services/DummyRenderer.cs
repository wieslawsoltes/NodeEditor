#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Rendering;

namespace NodeEditorDemo.Services;

internal class DummyRenderer : IRenderer
{
    public void Dispose()
    {
    }

    public void AddDirty(Visual visual)
    {
    }

    public IEnumerable<Visual> HitTest(Point p, Visual root, Func<Visual, bool> filter)
    {
        return Enumerable.Empty<Visual>();
    }

    public Visual HitTestFirst(Point p, Visual root, Func<Visual, bool> filter)
    {
        return null;
    }

    public void RecalculateChildren(Visual visual)
    {
    }

    public void Resized(Size size)
    {
        throw new NotImplementedException();
    }

    public void Paint(Rect rect)
    {
    }

    public void Start()
    {
    }

    public void Stop()
    {
    }

    public ValueTask<object> TryGetRenderInterfaceFeature(Type featureType)
    {
        return default;
    }

    public RendererDiagnostics Diagnostics { get; }
    public event EventHandler<SceneInvalidatedEventArgs> SceneInvalidated;
}
