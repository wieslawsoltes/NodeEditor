using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Behaviors;
using NodeEditor.Controls;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditor.Services;
using NodeEditorDemo.Services;
using NodeEditorDemo.ViewModels;
using ReactiveUI;
using Xunit;

namespace NodeEditor.UnitTests;

public sealed class ZipTieRenderingTests
{
    [AvaloniaFact]
    public void ShowSelectionSuppressesHighlightAndGripsButKeepsCollars()
    {
        var drawing = ZipTieDemo.CreateDrawing();
        var layer = CreateLayer(drawing);
        var selected = Capture(layer);
        Assert.Contains(Flatten(selected), geometry => BrushColor(geometry.Pen?.Brush) == Colors.Blue);
        // DrawingGroup snapshots geometry as opaque PlatformGeometry; count the two grips per collar.
        Assert.Equal(ZipTieGeometry.GetAnchorCount(drawing.ZipTies[0]) * 6, Flatten(selected).Count());

        layer.ShowSelection = false;
        var plain = Capture(layer);
        Assert.NotEmpty(Flatten(plain));
        Assert.All(Flatten(plain), geometry => Assert.Equal(Colors.Red, BrushColor(geometry.Pen?.Brush)));
        Assert.Equal(ZipTieGeometry.GetAnchorCount(drawing.ZipTies[0]) * 4, Flatten(plain).Count());
        Assert.Same(drawing.ZipTies[0], drawing.SelectedZipTie);
    }

    [AvaloniaFact]
    public void LockedBundleHasNoDragGrips()
    {
        var drawing = ZipTieDemo.CreateDrawing();
        drawing.ZipTies[0].IsLocked = true;
        var rendered = Capture(CreateLayer(drawing));
        Assert.NotEmpty(Flatten(rendered));
        Assert.Equal(ZipTieGeometry.GetAnchorCount(drawing.ZipTies[0]) * 4, Flatten(rendered).Count());
    }

    [AvaloniaFact]
    public void ExportSuppressesSelectionAndRestoresItWhenRenderingThrows()
    {
        var drawing = ZipTieDemo.CreateDrawing();
        var layer = CreateLayer(drawing);
        var target = new ThrowingExportRoot { Child = layer };
        using var output = new MemoryStream();
        Assert.Throws<ExportProbeException>(() => ExportRenderer.RenderPng(target, new Size(1120, 680), output));

        Assert.NotNull(target.Captured);
        Assert.NotEmpty(Flatten(target.Captured!));
        Assert.All(Flatten(target.Captured!), geometry => Assert.Equal(Colors.Red, BrushColor(geometry.Pen?.Brush)));
        Assert.Equal(ZipTieGeometry.GetAnchorCount(drawing.ZipTies[0]) * 4, Flatten(target.Captured!).Count());
        Assert.True(layer.ShowSelection);
        Assert.Same(drawing.ZipTies[0], drawing.SelectedZipTie);
        Assert.Contains(Flatten(Capture(layer)), geometry => BrushColor(geometry.Pen?.Brush) == Colors.Blue);
    }

    [AvaloniaFact]
    public void ManualWaypointHandlesTrackBundleVisibilityRemovalAndLocking()
    {
        var drawing = ZipTieDemo.CreateDrawing();
        var tie = drawing.ZipTies[0];
        var wire = tie.Connectors[0];
        wire.RoutingMode = ConnectorRoutingMode.Manual;
        var control = new Connector { ConnectorSource = wire };
        Assert.False(control.HasEditableWaypoints);
        tie.IsVisible = false;
        Assert.True(control.HasEditableWaypoints);
        tie.IsVisible = true;
        Assert.False(control.HasEditableWaypoints);
        drawing.RemoveZipTie(tie);
        Assert.True(control.HasEditableWaypoints);
        wire.IsLocked = true;
        Assert.False(control.HasEditableWaypoints);
        wire.IsLocked = false;
        Assert.True(control.HasEditableWaypoints);
        wire.RoutingMode = ConnectorRoutingMode.Auto;
        Assert.False(control.HasEditableWaypoints);
        control.ConnectorSource = null;
        Assert.False(control.HasEditableWaypoints);
    }

    [AvaloniaFact]
    public void SelectionOverlayFollowsRoundedOrdinaryBezierCorners()
    {
        var drawing = new DrawingNodeViewModel();
        drawing.Settings.EnableConnectorRouting = false;
        drawing.Settings.RoutingCornerRadius = 40;
        var wire = new ConnectorViewModel
        {
            Parent = drawing,
            Start = new PinViewModel { X = 0, Y = 100 },
            End = new PinViewModel { X = 100, Y = 200 },
            Style = ConnectorStyle.Bezier,
            RoutingMode = ConnectorRoutingMode.Manual,
            Waypoints = new ObservableCollection<ConnectorPoint> { new(100, 100) }
        };
        var overlay = new ConnectorSelectedAdorner { Connectors = new[] { wire }, Stroke = Brushes.Blue };
        var group = new DrawingGroup();
        using (var context = group.Open())
        {
            overlay.Render(context);
        }
        // The rounded corner needs multiple flattened segments; the old sharp polyline had only two.
        Assert.InRange(Flatten(group).Count(), 5, 40);
        var renderedWire = new Connector { ConnectorSource = wire };
        Assert.Equal(95, renderedWire.LabelPoint.X, precision: 6);
        Assert.Equal(105, renderedWire.LabelPoint.Y, precision: 6);
    }

    [AvaloniaFact]
    public void RestoringSnapshotInEditorRemovesPreviousDrawingSelectionOverlay()
    {
        using var vm = new ZipTieDemoViewModel();
        using var ui = new EditorSelectionHarness(vm.Drawing);
        using var binding = ui.Editor.Bind(Editor.DrawingSourceProperty,
            vm.WhenAnyValue(viewModel => viewModel.Drawing));

        vm.Drawing.ZipTies[0].Waypoints.Add(new ConnectorPoint(500, 350));
        vm.SaveSnapshotCommand.Execute().Subscribe();
        Assert.True(vm.HasSnapshot);
        vm.SetWireStyleCommand.Execute(ConnectorStyle.Straight).Subscribe();
        vm.Drawing.RemoveZipTie(vm.Drawing.ZipTies[0]);
        vm.SelectWiresCommand.Execute().Subscribe();
        vm.Drawing.CreateZipTieCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var previousDrawing = vm.Drawing;
        var previousOverlay = Assert.Single(ui.Editor.AdornerCanvas!.Children.OfType<ConnectorSelectedAdorner>());
        Assert.Equal(6, previousOverlay.Connectors!.Count);
        Assert.All(previousOverlay.Connectors, wire => Assert.Same(previousDrawing, wire.Parent));

        vm.LoadSnapshotCommand.Execute().Subscribe();
        Dispatcher.UIThread.RunJobs();

        Assert.NotSame(previousDrawing, vm.Drawing);
        Assert.Same(vm.Drawing, ui.Editor.DrawingSource);
        Assert.Null(vm.Drawing.GetSelectedConnectors());
        Assert.Empty(ui.Editor.AdornerCanvas.Children.OfType<ConnectorSelectedAdorner>());
        Assert.Null(previousOverlay.Parent);
        Assert.Null(previousOverlay.Connectors);
        Assert.Single(vm.Drawing.ZipTies[0].Waypoints);

        // Events from the replaced drawing must not recreate its overlay.
        previousDrawing.NotifySelectionChanged();
        previousDrawing.ZipTies[0].End.X += 40;
        Dispatcher.UIThread.RunJobs();
        Assert.Empty(ui.Editor.AdornerCanvas.Children.OfType<ConnectorSelectedAdorner>());

        // The new drawing still receives normal selection rendering.
        vm.SelectWiresCommand.Execute().Subscribe();
        Dispatcher.UIThread.RunJobs();
        var currentOverlay = Assert.Single(ui.Editor.AdornerCanvas.Children.OfType<ConnectorSelectedAdorner>());
        Assert.NotSame(previousOverlay, currentOverlay);
        Assert.All(currentOverlay.Connectors!, wire => Assert.Same(vm.Drawing, wire.Parent));
    }

    [AvaloniaFact]
    public void DetachingEditorSelectionBehaviorRemovesAndReattachingRestoresCurrentAdorners()
    {
        var drawing = ZipTieDemo.CreateDrawing();
        using var ui = new EditorSelectionHarness(drawing);
        drawing.SetSelectedNodes(new HashSet<INode>(drawing.Nodes!));
        drawing.SetSelectedConnectors(new HashSet<IConnector>(drawing.Connectors!));
        drawing.NotifySelectionChanged();
        Dispatcher.UIThread.RunJobs();

        var layer = ui.Editor.AdornerCanvas!;
        var previousOverlay = Assert.Single(layer.Children.OfType<ConnectorSelectedAdorner>());
        Assert.Single(layer.Children.OfType<SelectedAdorner>());
        var surface = Assert.Single(ui.Editor.GetVisualDescendants().OfType<ItemsControl>(),
            control => Interaction.GetBehaviors(control).OfType<DrawingSelectionBehavior>().Any());
        var behaviors = Interaction.GetBehaviors(surface);
        var selection = Assert.Single(behaviors.OfType<DrawingSelectionBehavior>());
        behaviors.Remove(selection);
        Dispatcher.UIThread.RunJobs();

        Assert.Empty(layer.Children.OfType<ConnectorSelectedAdorner>());
        Assert.Empty(layer.Children.OfType<SelectedAdorner>());
        Assert.Null(previousOverlay.Parent);
        Assert.Null(previousOverlay.Connectors);
        drawing.NotifySelectionChanged();
        Assert.Empty(layer.Children.OfType<ConnectorSelectedAdorner>());

        // Existing model selection is restored once when the behavior is attached again.
        behaviors.Add(selection);
        Dispatcher.UIThread.RunJobs();
        var currentOverlay = Assert.Single(layer.Children.OfType<ConnectorSelectedAdorner>());
        Assert.Equal(6, currentOverlay.Connectors!.Count);
        Assert.Single(layer.Children.OfType<SelectedAdorner>());
    }

    [AvaloniaFact]
    public void RemovingAndReaddingSameWireRestoresSpatialSubscriptions()
    {
        using var harness = new WireHitHarness();
        var wire = harness.Wire;
        harness.Click(new Point(180, 100));
        Assert.Contains(wire, harness.Drawing.GetSelectedConnectors()!);

        harness.Drawing.Connectors!.Remove(wire);
        harness.ClearSelection();
        harness.Click(new Point(180, 100));
        Assert.Null(harness.Drawing.GetSelectedConnectors());

        harness.Drawing.Connectors.Add(wire);
        wire.Start!.Y = 180;
        wire.End!.Y = 180;
        harness.Click(new Point(180, 180));
        Assert.Contains(wire, harness.Drawing.GetSelectedConnectors()!);
        harness.ClearSelection();
        harness.Click(new Point(180, 100));
        Assert.Null(harness.Drawing.GetSelectedConnectors());
    }

    [AvaloniaFact]
    public void HiddenWireDoesNotMaskNearbyVisibleWireInSpatialIndex()
    {
        using var harness = new WireHitHarness();
        harness.Wire.IsVisible = false;
        var visible = new ConnectorViewModel
        {
            Parent = harness.Drawing,
            Start = new PinViewModel { X = 20, Y = 103 },
            End = new PinViewModel { X = 360, Y = 103 },
            Style = ConnectorStyle.Straight,
            RoutingMode = ConnectorRoutingMode.Manual
        };
        harness.Drawing.Connectors!.Add(visible);
        harness.Click(new Point(180, 100));
        Assert.Same(visible, Assert.Single(harness.Drawing.GetSelectedConnectors()!));
    }

    private static ZipTieLayer CreateLayer(DrawingNodeViewModel drawing) => new()
    {
        Drawing = drawing,
        Stroke = Brushes.Red,
        SelectedStroke = Brushes.Blue,
        Fill = null
    };

    private static DrawingGroup Capture(ZipTieLayer layer)
    {
        var group = new DrawingGroup();
        using var context = group.Open();
        layer.Render(context);
        return group;
    }

    private static IEnumerable<GeometryDrawing> Flatten(Drawing drawing)
    {
        if (drawing is GeometryDrawing geometry)
        {
            yield return geometry;
        }
        else if (drawing is DrawingGroup group)
        {
            foreach (var child in group.Children)
            {
                foreach (var nested in Flatten(child))
                {
                    yield return nested;
                }
            }
        }
    }

    private static Color? BrushColor(IBrush? brush) => (brush as ISolidColorBrush)?.Color;

    private sealed class ExportProbeException : Exception
    {
    }

    private sealed class ThrowingExportRoot : Decorator
    {
        public DrawingGroup? Captured { get; private set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            Captured = Capture((ZipTieLayer)Child!);
            throw new ExportProbeException();
        }
    }

    private sealed class WireHitHarness : IDisposable
    {
        public DrawingNodeViewModel Drawing { get; } = new();
        public ConnectorViewModel Wire { get; }
        private readonly ItemsControl _surface;
        private readonly Window _window;

        public WireHitHarness()
        {
            Drawing.Settings.EnableConnectorRouting = false;
            Wire = new ConnectorViewModel
            {
                Parent = Drawing,
                Start = new PinViewModel { X = 20, Y = 100 },
                End = new PinViewModel { X = 360, Y = 100 },
                Style = ConnectorStyle.Straight,
                RoutingMode = ConnectorRoutingMode.Manual
            };
            Drawing.Connectors = new ObservableCollection<IConnector> { Wire };
            _surface = new ItemsControl { Width = 400, Height = 300, Background = Brushes.Transparent };
            Interaction.GetBehaviors(_surface).Add(new ConnectorInteractionBehavior { DrawingSource = Drawing });
            _window = new Window { Width = 400, Height = 300, Content = _surface };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        public void ClearSelection()
        {
            Drawing.NotifyDeselectedConnectors();
            Drawing.SetSelectedConnectors(null);
            Drawing.NotifySelectionChanged();
        }

        public void Click(Point point)
        {
            _window.MouseDown(point, MouseButton.Left);
            _window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            Interaction.GetBehaviors(_surface).Clear();
            _window.Close();
        }
    }

    private sealed class EditorSelectionHarness : IDisposable
    {
        private readonly Window _window;
        public Editor Editor { get; }

        public EditorSelectionHarness(DrawingNodeViewModel drawing)
        {
            Editor = new Editor { DrawingSource = drawing };
            _window = new Window { Width = 1120, Height = 680, Content = Editor };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose() => _window.Close();
    }
}
