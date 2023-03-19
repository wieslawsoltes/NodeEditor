#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Styling;

namespace NodeEditorDemo.Services;

internal class ExportRoot : Decorator, IFocusScope, ILayoutRoot, IInputRoot, IRenderRoot, IStyleHost, ILogicalRoot
{
    public ExportRoot()
    {
        Renderer = null;
        LayoutManager = new LayoutManager(this);
        IsVisible = true;
        KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Cycle);
    }

    public ExportRoot(Control child)
        : this(false, child)
    {
        Child = child;
    }

    public ExportRoot(bool useGlobalStyles, Control child)
        : this()
    {
        if (useGlobalStyles)
        {
            StylingParent = Application.Current;
        }

        Child = child;
    }

    public Size ClientSize { get; set; } = new Size(100, 100);

    public Size MaxClientSize { get; set; } = Size.Infinity;

    public double LayoutScaling { get; set; } = 1;

    public ILayoutManager LayoutManager { get; set; }

    public double RenderScaling => 1;

    public IRenderer Renderer { get; set; }

    public IAccessKeyHandler AccessKeyHandler => null;

    public IKeyboardNavigationHandler KeyboardNavigationHandler => null;

    public IInputElement PointerOverElement { get; set; }

    public IMouseDevice MouseDevice { get; set; }

    public bool ShowAccessKeys { get; set; }

    public IStyleHost StylingParent { get; set; }

    IStyleHost IStyleHost.StylingParent => StylingParent;

    public IRenderTarget CreateRenderTarget()
    {
        return null;
    }

    public void Invalidate(Rect rect)
    {
    }

    public Point PointToClient(PixelPoint p) => p.ToPoint(1);

    public PixelPoint PointToScreen(Point p) => PixelPoint.FromPoint(p, 1);
}
