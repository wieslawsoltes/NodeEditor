using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using NodeEditor.Model;
using NodeEditor.Services;

namespace NodeEditor.Controls;

/// <summary>Renders zip-tie collars above connectors without intercepting pointer input.</summary>
public sealed class ZipTieLayer : Control
{
    /// <summary>Identifies the drawing supplying the zip ties.</summary>
    public static readonly DirectProperty<ZipTieLayer, IDrawingNode?> DrawingProperty =
        AvaloniaProperty.RegisterDirect<ZipTieLayer, IDrawingNode?>(nameof(Drawing), control => control.Drawing,
            (control, value) => control.Drawing = value);

    /// <summary>Identifies the collar outline brush.</summary>
    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<ZipTieLayer, IBrush?>(nameof(Stroke), Brushes.SlateGray);

    /// <summary>Identifies the selected collar outline brush.</summary>
    public static readonly StyledProperty<IBrush?> SelectedStrokeProperty =
        AvaloniaProperty.Register<ZipTieLayer, IBrush?>(nameof(SelectedStroke), Brushes.DodgerBlue);

    /// <summary>Identifies the collar's translucent inner brush.</summary>
    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<ZipTieLayer, IBrush?>(nameof(Fill), Brushes.White);

    /// <summary>Identifies the width of the collar outline.</summary>
    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<ZipTieLayer, double>(nameof(StrokeThickness), 1.5);

    /// <summary>Identifies whether interactive selection outlines and grips are displayed.</summary>
    public static readonly StyledProperty<bool> ShowSelectionProperty =
        AvaloniaProperty.Register<ZipTieLayer, bool>(nameof(ShowSelection), true);

    private IDrawingNode? _drawing;
    private ZipTieCollectionSubscription? _subscription;
    private readonly Dictionary<IZipTie, ZipTieBand[]> _bands = new();
    private bool _isAttached;

    static ZipTieLayer()
    {
        IsHitTestVisibleProperty.OverrideDefaultValue<ZipTieLayer>(false);
        AffectsRender<ZipTieLayer>(DrawingProperty, StrokeProperty, SelectedStrokeProperty, FillProperty,
            StrokeThicknessProperty, ShowSelectionProperty, ExportRenderer.IsExportPassProperty);
    }

    /// <summary>Gets or sets the drawing whose optional <see cref="IZipTieHost"/> capability is displayed.</summary>
    public IDrawingNode? Drawing
    {
        get => _drawing;
        set => SetAndRaise(DrawingProperty, ref _drawing, value);
    }

    /// <summary>Gets or sets the collar outline brush.</summary>
    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    /// <summary>Gets or sets the selected collar outline brush.</summary>
    public IBrush? SelectedStroke
    {
        get => GetValue(SelectedStrokeProperty);
        set => SetValue(SelectedStrokeProperty, value);
    }

    /// <summary>Gets or sets the collar's inner brush.</summary>
    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>Gets or sets the collar outline width.</summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets whether selection outlines and editing grips are displayed. Export rendering
    /// suppresses these affordances independently without changing document selection or bindings.
    /// </summary>
    public bool ShowSelection
    {
        get => GetValue(ShowSelectionProperty);
        set => SetValue(ShowSelectionProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DrawingProperty)
        {
            AttachDrawing();
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        AttachDrawing();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isAttached = false;
        AttachDrawing();
    }

    private void AttachDrawing()
    {
        _subscription?.Dispose();
        _bands.Clear();
        _subscription = _isAttached && Drawing is IZipTieHost host
            ? new ZipTieCollectionSubscription(host, TieChanged)
            : null;
        InvalidateVisual();
    }

    private void TieChanged(IZipTie? tie)
    {
        if (tie is null)
        {
            _bands.Clear();
        }
        else
        {
            _bands.Remove(tie);
        }
        InvalidateVisual();
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Drawing is not IZipTieHost host)
        {
            return;
        }

        var thickness = double.IsFinite(StrokeThickness) ? System.Math.Max(0.5, StrokeThickness) : 1.5;
        var normalPen = Stroke is { } stroke ? new ImmutablePen(stroke.ToImmutable(), thickness, lineCap: PenLineCap.Round) : null;
        var selectedPen = SelectedStroke is { } selected ? new ImmutablePen(selected.ToImmutable(), thickness + 0.5, lineCap: PenLineCap.Round) : normalPen;
        var innerPen = Fill is { } fill ? new ImmutablePen(fill.ToImmutable(), 3, lineCap: PenLineCap.Round) : null;

        foreach (var tie in host.ZipTies)
        {
            if (!tie.IsVisible || tie.Connectors.Count < 2)
            {
                continue;
            }

            var isSelected = ShowSelection && !GetValue(ExportRenderer.IsExportPassProperty)
                && ReferenceEquals(host.SelectedZipTie, tie);
            var pen = isSelected ? selectedPen : normalPen;
            if (pen is null)
            {
                continue;
            }

            ZipTieBand[] bands;
            if (_subscription is null)
            {
                // Standalone export controls do not receive visual-tree attach notifications.
                bands = ZipTieGeometry.GetBands(tie);
            }
            else if (!_bands.TryGetValue(tie, out bands!))
            {
                bands = ZipTieGeometry.GetBands(tie);
                _bands.Add(tie, bands);
            }

            for (var i = 0; i < bands.Length; i++)
            {
                var start = bands[i].Start;
                var end = bands[i].End;
                Vector delta = end - start;
                var length = delta.Length;
                var side = length > 0.000001 ? new Vector(delta.Y / length, -delta.X / length) * 2.5 : new Vector(2.5, 0);
                if (innerPen is not null)
                {
                    using (context.PushOpacity(0.72))
                    {
                        context.DrawLine(innerPen, start, end);
                    }
                }

                context.DrawLine(pen, start - side, end - side);
                context.DrawLine(pen, start + side, end + side);
                context.DrawLine(pen, start - side, start + side);
                context.DrawLine(pen, end - side, end + side);
                if (isSelected && !tie.IsLocked)
                {
                    // Small end grips remain visible while individual wire colors show through the collar.
                    context.DrawEllipse(Fill, pen, start, 3.25, 3.25);
                    context.DrawEllipse(Fill, pen, end, 3.25, 3.25);
                }
            }
        }
    }
}
