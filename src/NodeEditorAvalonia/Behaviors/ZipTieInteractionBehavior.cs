using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Controls;
using NodeEditor.Model;

namespace NodeEditor.Behaviors;

/// <summary>Edits wire zip ties through pointer and keyboard input in drawing coordinates.</summary>
/// <remarks>
/// Attach before drawing selection behaviors. Drag a collar to move one anchor;
/// Shift-drag a collar or the bundled span to translate the entire bundle.
/// Double-click the span to insert a collar and an intermediate collar to remove it.
/// Ctrl/Cmd+J creates a bundle; Ctrl/Cmd+Shift+J dissolves the selected bundle.
/// Escape rolls back an active drag. A completed drag is one undo operation.
/// </remarks>
public sealed class ZipTieInteractionBehavior : Behavior<ItemsControl>
{
    /// <summary>Identifies the drawing containing the optional zip-tie capability.</summary>
    public static readonly DirectProperty<ZipTieInteractionBehavior, IDrawingNode?> DrawingSourceProperty =
        AvaloniaProperty.RegisterDirect<ZipTieInteractionBehavior, IDrawingNode?>(nameof(DrawingSource),
            behavior => behavior.DrawingSource, (behavior, value) => behavior.DrawingSource = value);

    /// <summary>Identifies the input surface, usually the editor's zoom control.</summary>
    public static readonly DirectProperty<ZipTieInteractionBehavior, Control?> InputSourceProperty =
        AvaloniaProperty.RegisterDirect<ZipTieInteractionBehavior, Control?>(nameof(InputSource),
            behavior => behavior.InputSource, (behavior, value) => behavior.InputSource = value);

    /// <summary>Identifies the collar hit tolerance in screen-independent pixels.</summary>
    public static readonly StyledProperty<double> HitToleranceProperty =
        AvaloniaProperty.Register<ZipTieInteractionBehavior, double>(nameof(HitTolerance), 8);

    private readonly List<AnchorState> _dragAnchors = new();
    private IDrawingNode? _drawingSource;
    private Control? _configuredInput;
    private Control? _inputSource;
    private IDrawingNode? _dragDrawing;
    private IZipTie? _dragTie;
    private IPointer? _pointer;
    private IUndoRedoHost? _undoHost;
    private Point _pressPosition;
    private Point _referencePosition;
    private bool _undoStarted;
    private bool _isVisualTreeDetached;

    /// <summary>Gets or sets the drawing being edited.</summary>
    public IDrawingNode? DrawingSource
    {
        get => _drawingSource;
        set => SetAndRaise(DrawingSourceProperty, ref _drawingSource, value);
    }

    /// <summary>Gets or sets the input surface; defaults to the associated items control.</summary>
    public Control? InputSource
    {
        get => _configuredInput;
        set => SetAndRaise(InputSourceProperty, ref _configuredInput, value);
    }

    /// <summary>Gets or sets the hit tolerance, independent of editor zoom.</summary>
    public double HitTolerance
    {
        get => GetValue(HitToleranceProperty);
        set => SetValue(HitToleranceProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        _isVisualTreeDetached = false;
        AttachInput();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();
        _isVisualTreeDetached = false;
        // A template can attach the behavior before its input-source binding is resolved.
        // Rebind after the control enters the tree so input uses the final zoom surface.
        DetachInput();
        AttachInput();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        _isVisualTreeDetached = true;
        FinishDrag(cancel: true);
        DetachInput();
        base.OnDetachedFromVisualTree();
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        FinishDrag(cancel: true);
        DetachInput();
        base.OnDetaching();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DrawingSourceProperty || change.Property == InputSourceProperty)
        {
            FinishDrag(cancel: true);
        }

        if (change.Property == InputSourceProperty && AssociatedObject is not null && !_isVisualTreeDetached)
        {
            DetachInput();
            AttachInput();
        }
    }

    private void AttachInput()
    {
        _inputSource = InputSource ?? AssociatedObject;
        if (_inputSource is null)
        {
            return;
        }

        _inputSource.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        _inputSource.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
        _inputSource.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
        _inputSource.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        _inputSource.AddHandler(InputElement.KeyDownEvent, KeyDown, RoutingStrategies.Tunnel);
    }

    private void DetachInput()
    {
        if (_inputSource is null)
        {
            return;
        }

        _inputSource.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
        _inputSource.RemoveHandler(InputElement.PointerMovedEvent, Moved);
        _inputSource.RemoveHandler(InputElement.PointerReleasedEvent, Released);
        _inputSource.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
        _inputSource.RemoveHandler(InputElement.KeyDownEvent, KeyDown);
        _inputSource = null;
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled || AssociatedObject is null || _inputSource is null
            || DrawingSource is not { } drawing || drawing is not IZipTieHost host
            || IsInputBlocked(drawing))
        {
            return;
        }

        if (_dragTie is not null)
        {
            // One pointer owns a drag transaction. Other contacts must not
            // change selection or commit/capture an in-progress collar edit.
            if (ReferenceEquals(e.Pointer, _pointer))
            {
                e.Handled = true;
            }
            return;
        }

        var properties = e.GetCurrentPoint(_inputSource).Properties;
        if (!properties.IsLeftButtonPressed && !properties.IsRightButtonPressed)
        {
            return;
        }

        if (IsInteractiveSource(e.Source, ignoreNodes: false))
        {
            // Node selection must regain normal nudge/delete behavior; embedded editors
            // and controls retain both their input and the active bundle.
            if (!IsInteractiveSource(e.Source, ignoreNodes: true))
            {
                host.SelectedZipTie = null;
            }
            return;
        }

        var position = e.GetPosition(AssociatedObject);
        var wholeBundle = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        var includeSpan = wholeBundle || e.ClickCount == 2 || properties.IsRightButtonPressed;
        if (!TryFindTie(host, position, includeSpan, out var tie, out var anchorIndex))
        {
            // Keep the active bundle while selecting its members or potential new members.
            if (properties.IsLeftButtonPressed && !HitTestHelper.TryFindConnectorAtPoint(
                    drawing, position, DrawingHitTolerance(), out _, out _))
            {
                host.SelectedZipTie = null;
            }
            return;
        }

        if (properties.IsLeftButtonPressed && (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) == 0)
        {
            drawing.NotifyDeselectedNodes();
            drawing.NotifyDeselectedConnectors();
            drawing.SetSelectedNodes(null);
            drawing.SetSelectedConnectors(null);
        }
        host.SelectedZipTie = tie;
        drawing.NotifySelectionChanged();
        _inputSource.Focus(NavigationMethod.Pointer);

        // Keep right-click unhandled so the standard editor menu can open.
        if (properties.IsRightButtonPressed)
        {
            return;
        }

        e.Handled = true;
        if (!CanEdit(drawing, tie))
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            FinishDrag(cancel: false);
            EditCollar(drawing, tie, anchorIndex, position);
            return;
        }

        BeginDrag(drawing, tie, wholeBundle || anchorIndex < 0 ? -1 : anchorIndex, position, e.Pointer);
    }

    private void BeginDrag(IDrawingNode drawing, IZipTie tie, int anchorIndex, Point position, IPointer pointer)
    {
        FinishDrag(cancel: false);
        _dragDrawing = drawing;
        _dragTie = tie;
        _pressPosition = position;
        _referencePosition = ZipTieGeometry.GetAnchor(tie, Math.Max(0, anchorIndex));
        _undoHost = drawing as IUndoRedoHost;
        var seen = new HashSet<ConnectorPoint>();
        var first = anchorIndex < 0 ? 0 : anchorIndex;
        var end = anchorIndex < 0 ? ZipTieGeometry.GetAnchorCount(tie) : anchorIndex + 1;
        for (var i = first; i < end; i++)
        {
            var anchor = GetAnchorModel(tie, i);
            if (seen.Add(anchor))
            {
                _dragAnchors.Add(new AnchorState(anchor));
            }
        }

        _pointer = pointer;
        pointer.Capture(_inputSource);
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (_dragTie is not { } tie || _dragDrawing is not { } drawing
            || AssociatedObject is null || !ReferenceEquals(e.Pointer, _pointer))
        {
            return;
        }

        e.Handled = true;
        if (!CanEdit(drawing, tie) || IsInputBlocked(drawing))
        {
            FinishDrag(cancel: true);
            return;
        }

        var target = _referencePosition + (e.GetPosition(AssociatedObject) - _pressPosition);
        if (!TrySnap(target, drawing.Settings, (e.KeyModifiers & KeyModifiers.Alt) == 0, out target))
        {
            return;
        }

        Vector delta = target - _referencePosition;
        if (!double.IsFinite(delta.X) || !double.IsFinite(delta.Y))
        {
            return;
        }

        // Validate every destination before changing any anchor. A rejected
        // move keeps the last valid geometry and cannot create partial motion.
        foreach (var anchor in _dragAnchors)
        {
            if (!double.IsFinite(anchor.Position.X + delta.X) || !double.IsFinite(anchor.Position.Y + delta.Y))
            {
                return;
            }
        }

        if (!_undoStarted && delta.Length > 0.001)
        {
            _undoHost?.BeginUndoBatch();
            _undoStarted = true;
        }
        if (!_undoStarted)
        {
            return;
        }

        foreach (var anchor in _dragAnchors)
        {
            anchor.Point.X = anchor.Position.X + delta.X;
            anchor.Point.Y = anchor.Position.Y + delta.Y;
        }
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragTie is not null && ReferenceEquals(e.Pointer, _pointer)
            && !e.GetCurrentPoint(_inputSource).Properties.IsLeftButtonPressed
            && (e.Pointer.Type == PointerType.Touch || e.InitialPressMouseButton == MouseButton.Left))
        {
            e.Handled = true;
            FinishDrag(cancel: false);
        }
    }

    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_dragTie is not null && ReferenceEquals(e.Pointer, _pointer))
        {
            FinishDrag(cancel: false);
        }
    }

    private void FinishDrag(bool cancel)
    {
        var pointer = _pointer;
        _pointer = null;
        _dragTie = null;
        _dragDrawing = null;
        try
        {
            if (cancel)
            {
                foreach (var anchor in _dragAnchors)
                {
                    anchor.Point.X = anchor.Position.X;
                    anchor.Point.Y = anchor.Position.Y;
                }
            }
        }
        finally
        {
            _dragAnchors.Clear();
            if (_undoStarted)
            {
                _undoStarted = false;
                _undoHost?.EndUndoBatch();
            }
            _undoHost = null;
            if (pointer is not null && ReferenceEquals(pointer.Captured, _inputSource))
            {
                pointer.Capture(null);
            }
        }
    }

    private void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || DrawingSource is not { } drawing || drawing is not IZipTieHost host
            || IsInteractiveSource(e.Source, ignoreNodes: true))
        {
            return;
        }

        if (_dragTie is not null)
        {
            if (e.Key == Key.Escape)
            {
                FinishDrag(cancel: true);
            }
            // A topology/history change cannot run in the middle of a pointer transaction.
            e.Handled = true;
            return;
        }

        if (IsInputBlocked(drawing))
        {
            return;
        }

        var commandModifier = (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0;
        if (commandModifier && e.Key == Key.J)
        {
            if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
            {
                if (host.SelectedZipTie is { } selected)
                {
                    host.RemoveZipTie(selected);
                }
            }
            else if (drawing.GetSelectedConnectors() is { Count: >= 2 } connectors)
            {
                host.CreateZipTie(connectors);
            }
            e.Handled = true;
            return;
        }

        if (host.SelectedZipTie is not { } tie)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            host.SelectedZipTie = null;
            e.Handled = true;
        }
        else if (!commandModifier && e.Key is Key.Delete or Key.Back)
        {
            host.RemoveZipTie(tie);
            e.Handled = true;
        }
        else if (!commandModifier && (e.KeyModifiers & KeyModifiers.Alt) == 0
                 && e.Key is Key.Left or Key.Right or Key.Up or Key.Down)
        {
            if (CanEdit(drawing, tie) && TryGetNudge(drawing.Settings, e, out var delta))
            {
                ExecuteWithUndo(drawing, () => Translate(tie, delta));
            }
            e.Handled = true;
        }
    }

    private void EditCollar(IDrawingNode drawing, IZipTie tie, int anchorIndex, Point position)
    {
        if (anchorIndex == 0 || anchorIndex == ZipTieGeometry.GetAnchorCount(tie) - 1)
        {
            return;
        }

        ExecuteWithUndo(drawing, () =>
        {
            // Replacing a read-only collection is supported by the model contract.
            var points = new ObservableCollection<ConnectorPoint>(tie.Waypoints);
            if (anchorIndex > 0)
            {
                points.RemoveAt(anchorIndex - 1);
            }
            else
            {
                var index = ZipTieGeometry.GetInsertionIndex(tie, position);
                if (!TrySnap(position, drawing.Settings, true, out position))
                {
                    return;
                }

                points.Insert(index, new ConnectorPoint(position.X, position.Y));
            }
            tie.Waypoints = points;
        });
    }

    private bool TryFindTie(IZipTieHost host, Point position, bool includeSpan, out IZipTie tie, out int anchorIndex)
    {
        var tolerance = DrawingHitTolerance();
        // All collars take precedence over any overlapping span.
        for (var pass = 0; pass < (includeSpan ? 2 : 1); pass++)
        {
            if (host.SelectedZipTie is { } selected
                && ZipTieGeometry.TryHitTest(selected, position, tolerance, pass == 1, out anchorIndex))
            {
                tie = selected;
                return true;
            }
            for (var i = host.ZipTies.Count - 1; i >= 0; i--)
            {
                var candidate = host.ZipTies[i];
                if (ReferenceEquals(candidate, host.SelectedZipTie))
                {
                    continue;
                }
                if (ZipTieGeometry.TryHitTest(candidate, position, tolerance, pass == 1, out anchorIndex))
                {
                    tie = candidate;
                    return true;
                }
            }
        }
        tie = null!;
        anchorIndex = -1;
        return false;
    }

    private double DrawingHitTolerance()
    {
        var tolerance = double.IsFinite(HitTolerance) ? Math.Max(0, HitTolerance) : 8;
        if (AssociatedObject?.TransformToVisual(_inputSource!) is { } transform)
        {
            var scale = Math.Min(Math.Sqrt(transform.M11 * transform.M11 + transform.M12 * transform.M12),
                Math.Sqrt(transform.M21 * transform.M21 + transform.M22 * transform.M22));
            if (scale > 0.0001 && double.IsFinite(scale))
            {
                tolerance /= scale;
            }
        }
        return tolerance;
    }

    private bool IsInteractiveSource(object? source, bool ignoreNodes)
    {
        for (var visual = source as Visual; visual is not null && !ReferenceEquals(visual, _inputSource);
             visual = visual.GetVisualParent())
        {
            if ((!ignoreNodes && visual is Node or Pin)
                || visual is TextBox or Button or Thumb or SelectingItemsControl or Slider
                || visual is EditableTextBlock { IsEditing: true })
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsInputBlocked(IDrawingNode drawing) => drawing.IsConnectorMoving()
        || (drawing.Settings.EnableInk && drawing.Settings.IsInkMode);

    private static bool CanEdit(IDrawingNode drawing, IZipTie tie) => tie.IsVisible && !tie.IsLocked
        && drawing is IZipTieHost host && host.ZipTies.Contains(tie);

    private static ConnectorPoint GetAnchorModel(IZipTie tie, int index) => index == 0 ? tie.Start
        : index == ZipTieGeometry.GetAnchorCount(tie) - 1 ? tie.End : tie.Waypoints[index - 1];

    private static bool TrySnap(Point point, IDrawingNodeSettings settings, bool enabled, out Point snapped)
    {
        snapped = point;
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y))
        {
            return false;
        }

        if (!enabled || !settings.EnableSnap)
        {
            return true;
        }

        if (!TrySnapCoordinate(point.X, settings.SnapX, out var x)
            || !TrySnapCoordinate(point.Y, settings.SnapY, out var y))
        {
            return false;
        }

        snapped = new Point(x, y);
        return true;
    }

    private static bool TrySnapCoordinate(double value, double interval, out double snapped)
    {
        snapped = value;
        var step = Math.Abs(interval);
        if (!double.IsFinite(step) || step == 0)
        {
            return true;
        }

        var quotient = value / step;
        if (!double.IsFinite(quotient))
        {
            // This quantum is far below the coordinate's representable
            // precision. Rounding to it cannot change the finite double value.
            return true;
        }

        var candidate = Math.Round(quotient, MidpointRounding.AwayFromZero) * step;
        if (!double.IsFinite(candidate))
        {
            return false;
        }

        snapped = candidate;
        return true;
    }

    private static bool TryGetNudge(IDrawingNodeSettings settings, KeyEventArgs e, out Vector delta)
    {
        var step = double.IsFinite(settings.NudgeStep) && settings.NudgeStep > 0 ? settings.NudgeStep : 1;
        var multiplier = (e.KeyModifiers & KeyModifiers.Shift) != 0
            ? (double.IsFinite(settings.NudgeMultiplier) && settings.NudgeMultiplier > 0 ? settings.NudgeMultiplier : 10) : 1;
        var x = (settings.EnableSnap && double.IsFinite(settings.SnapX) && settings.SnapX > 0 ? settings.SnapX : step) * multiplier;
        var y = (settings.EnableSnap && double.IsFinite(settings.SnapY) && settings.SnapY > 0 ? settings.SnapY : step) * multiplier;
        delta = e.Key switch
        {
            Key.Left => new Vector(-x, 0),
            Key.Right => new Vector(x, 0),
            Key.Up => new Vector(0, -y),
            Key.Down => new Vector(0, y),
            _ => default
        };
        return delta != default && double.IsFinite(delta.X) && double.IsFinite(delta.Y);
    }

    internal static void MoveWithNodes(IDrawingNode drawing, ISet<INode> nodes, double deltaX, double deltaY)
    {
        if (drawing is not IZipTieHost host || (Math.Abs(deltaX) < 0.001 && Math.Abs(deltaY) < 0.001))
        {
            return;
        }

        foreach (var tie in host.ZipTies)
        {
            if (tie.IsLocked || !tie.IsVisible || tie.Connectors.Count < 2)
            {
                continue;
            }
            var move = true;
            foreach (var connector in tie.Connectors)
            {
                if (connector.Start?.Parent is not { } start || connector.End?.Parent is not { } end
                    || !nodes.Contains(start) || !nodes.Contains(end) || !start.CanMove() || !end.CanMove())
                {
                    move = false;
                    break;
                }
            }
            if (move)
            {
                Translate(tie, new Vector(deltaX, deltaY));
            }
        }
    }

    private static void Translate(IZipTie tie, Vector delta)
    {
        if (!double.IsFinite(delta.X) || !double.IsFinite(delta.Y))
        {
            return;
        }

        var seen = new HashSet<ConnectorPoint>();
        for (var i = 0; i < ZipTieGeometry.GetAnchorCount(tie); i++)
        {
            var point = GetAnchorModel(tie, i);
            if (seen.Add(point) && (!double.IsFinite(point.X + delta.X) || !double.IsFinite(point.Y + delta.Y)))
            {
                return;
            }
        }

        foreach (var point in seen)
        {
            point.X += delta.X;
            point.Y += delta.Y;
        }
    }

    private static void ExecuteWithUndo(IDrawingNode drawing, System.Action edit)
    {
        var host = drawing as IUndoRedoHost;
        host?.BeginUndoBatch();
        try
        {
            edit();
        }
        finally
        {
            host?.EndUndoBatch();
        }
    }

    private readonly struct AnchorState
    {
        public AnchorState(ConnectorPoint point)
        {
            Point = point;
            Position = new Point(point.X, point.Y);
        }

        public ConnectorPoint Point { get; }
        public Point Position { get; }
    }
}
