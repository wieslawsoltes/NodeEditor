using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Behaviors;
using NodeEditor.Controls;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorDemo.Services;
using Xunit;

namespace NodeEditor.UnitTests;

public sealed class ZipTieInteractionTests
{
    [AvaloniaFact]
    public void CollarDragUsesDrawingCoordinatesAndCreatesOneUndoEntry()
    {
        using var ui = new InputHarness(scale: 1.5);
        var tie = ui.Tie;
        var originalStart = PointOf(tie.Start);
        var originalEnd = PointOf(tie.End);

        ui.Press(originalStart);
        ui.Move(originalStart + new Vector(20, 10));
        ui.Move(originalStart + new Vector(46, 31));
        ui.Release(originalStart + new Vector(46, 31));

        AssertPoint(originalStart + new Vector(46, 31), tie.Start);
        AssertPoint(originalEnd, tie.End);
        Assert.True(ui.Drawing.CanUndo);
        ui.Drawing.Undo();
        AssertPoint(originalStart, ui.Drawing.ZipTies[0].Start);
        Assert.False(ui.Drawing.CanUndo);
        ui.Drawing.Redo();
        AssertPoint(originalStart + new Vector(46, 31), ui.Drawing.ZipTies[0].Start);
    }

    [AvaloniaFact]
    public void ShiftDragMovesEveryAnchorAndPreservesLaneSpacing()
    {
        using var ui = new InputHarness();
        ui.Tie.Waypoints.Add(new ConnectorPoint(280, 100));
        var originalStart = PointOf(ui.Tie.Start);
        var originalEnd = PointOf(ui.Tie.End);
        var spacing = ui.Tie.Spacing;
        var delta = new Vector(35, 24);

        ui.Press(originalStart, RawInputModifiers.Shift);
        ui.Move(originalStart + delta, RawInputModifiers.Shift);
        ui.Release(originalStart + delta, RawInputModifiers.Shift);

        AssertPoint(originalStart + delta, ui.Tie.Start);
        AssertPoint(originalEnd + delta, ui.Tie.End);
        AssertPoint(new Point(315, 124), ui.Tie.Waypoints[0]);
        Assert.Equal(spacing, ui.Tie.Spacing);
    }

    [AvaloniaFact]
    public void ShiftDraggingSharedSpanMovesBundle()
    {
        using var ui = new InputHarness();
        var point = new Point(280, 140);
        ui.Press(point, RawInputModifiers.Shift);
        ui.Move(point + new Vector(22, 30), RawInputModifiers.Shift);
        ui.Release(point + new Vector(22, 30), RawInputModifiers.Shift);

        AssertPoint(new Point(172, 170), ui.Tie.Start);
        AssertPoint(new Point(442, 170), ui.Tie.End);
    }

    [AvaloniaFact]
    public void EscapeRestoresDragAndDoesNotLeaveAnUndoBatchOpen()
    {
        using var ui = new InputHarness();
        ui.Press(new Point(150, 140));
        ui.Move(new Point(190, 175));
        ui.Key(Key.Escape);
        ui.Release(new Point(190, 175));

        AssertPoint(new Point(150, 140), ui.Tie.Start);
        Assert.False(ui.Drawing.CanUndo);

        // A following operation must get its own snapshot, rather than joining a cancelled drag.
        ui.Drawing.ReverseZipTie(ui.Tie);
        Assert.True(ui.Drawing.CanUndo);
        ui.Drawing.Undo();
        Assert.False(ui.Drawing.CanUndo);
    }

    [AvaloniaFact]
    public void CaptureLossBalancesUndoAndStopsFurtherDragging()
    {
        using var ui = new InputHarness();
        ui.Press(new Point(150, 140));
        ui.Move(new Point(175, 165));
        Assert.NotNull(ui.LastPointer);
        ui.LastPointer!.Capture(null);
        Dispatcher.UIThread.RunJobs();
        var afterCaptureLoss = PointOf(ui.Drawing.ZipTies[0].Start);
        ui.Move(new Point(230, 210));
        ui.Release(new Point(230, 210));
        AssertPoint(afterCaptureLoss, ui.Drawing.ZipTies[0].Start);

        // Whether capture loss commits or cancels, subsequent edits must remain independently undoable.
        ui.Drawing.ReverseZipTie(ui.Drawing.ZipTies[0]);
        Assert.True(ui.Drawing.CanUndo);
        ui.Drawing.Undo();
        AssertPoint(afterCaptureLoss, ui.Drawing.ZipTies[0].Start);
    }

    [AvaloniaFact]
    public void SecondaryMouseButtonCannotRestartOrFinishAnActiveDrag()
    {
        using var ui = new InputHarness();
        ui.Press(new Point(150, 140));
        ui.Move(new Point(175, 160));
        ui.SecondaryClickWhileDragging(new Point(175, 160));
        ui.Move(new Point(195, 180));
        ui.Release(new Point(195, 180));

        AssertPoint(new Point(195, 180), ui.Tie.Start);
        AssertPoint(new Point(420, 140), ui.Tie.End);
        ui.Drawing.Undo();
        AssertPoint(new Point(150, 140), ui.Drawing.ZipTies[0].Start);
        Assert.False(ui.Drawing.CanUndo);
    }

    [AvaloniaFact]
    public void DragSnapsAnchorsToTheDrawingGrid()
    {
        using var ui = new InputHarness();
        ui.Drawing.Settings.EnableSnap = true;
        ui.Drawing.Settings.SnapX = 10;
        ui.Drawing.Settings.SnapY = 10;
        ui.Press(new Point(150, 140));
        ui.Move(new Point(168, 153));
        ui.Release(new Point(168, 153));
        AssertPoint(new Point(170, 150), ui.Tie.Start);
    }

    [AvaloniaFact]
    public void AltTemporarilyBypassesSnapWhileDragging()
    {
        using var ui = new InputHarness();
        ui.Drawing.Settings.EnableSnap = true;
        ui.Drawing.Settings.SnapX = 10;
        ui.Drawing.Settings.SnapY = 10;
        ui.Press(new Point(150, 140));
        ui.Move(new Point(168, 153), RawInputModifiers.Alt);
        ui.Release(new Point(168, 153), RawInputModifiers.Alt);
        AssertPoint(new Point(168, 153), ui.Tie.Start);
    }

    [AvaloniaFact]
    public void ClickingCollarWithoutMovementDoesNotCreateUndoEntry()
    {
        using var ui = new InputHarness();
        ui.Drawing.SelectedZipTie = null;
        ui.Press(new Point(150, 140));
        ui.Release(new Point(150, 140));
        Assert.Same(ui.Tie, ui.Drawing.SelectedZipTie);
        Assert.False(ui.Drawing.CanUndo);
    }

    [AvaloniaFact]
    public void SwitchingDrawingDuringDragRestoresOriginalDrawing()
    {
        using var ui = new InputHarness();
        ui.Press(new Point(150, 140));
        ui.Move(new Point(190, 165));
        var replacement = ZipTieDemo.CreateDrawing();
        ui.Behavior.DrawingSource = replacement;
        ui.Move(new Point(240, 200));
        ui.Release(new Point(240, 200));
        AssertPoint(new Point(150, 140), ui.Tie.Start);
        AssertPoint(new Point(350, 270), replacement.ZipTies[0].Start);
        Assert.False(ui.Drawing.CanUndo);
    }

    [AvaloniaFact]
    public void DoubleClickCreatesAndRemovesIntermediateCollar()
    {
        using var ui = new InputHarness();
        var bend = new Point(280, 140);
        ui.DoubleClick(bend);
        var waypoint = Assert.Single(ui.Tie.Waypoints);
        AssertPoint(bend, waypoint);

        // Moving first makes the next two clicks an unambiguous double click at a distinct location.
        ui.Press(bend);
        ui.Move(new Point(280, 80));
        ui.Release(new Point(280, 80));
        ui.DoubleClick(new Point(280, 80));
        Assert.Empty(ui.Tie.Waypoints);
        Assert.Equal(6, ui.Tie.Connectors.Count);
    }

    [AvaloniaFact]
    public void KeyboardCreationAndDissolveKeepEveryWire()
    {
        using var ui = new InputHarness();
        var wires = new List<IConnector>(ui.Drawing.Connectors!);
        ui.Drawing.RemoveZipTie(ui.Tie);
        ui.Drawing.SetSelectedConnectors(new HashSet<IConnector>(wires));
        ui.Drawing.NotifySelectionChanged();

        ui.Key(Key.J, RawInputModifiers.Control);
        Assert.Single(ui.Drawing.ZipTies);
        Assert.NotNull(ui.Drawing.SelectedZipTie);
        ui.Key(Key.J, RawInputModifiers.Control | RawInputModifiers.Shift);
        Assert.Empty(ui.Drawing.ZipTies);
        Assert.Equal(wires, ui.Drawing.Connectors);
    }

    [AvaloniaFact]
    public void DeleteRemovesOnlyTheSelectedTieAndCanBeUndone()
    {
        using var ui = new InputHarness();
        var wireCount = ui.Drawing.Connectors!.Count;
        ui.Key(Key.Delete);
        Assert.Empty(ui.Drawing.ZipTies);
        Assert.Equal(wireCount, ui.Drawing.Connectors.Count);
        ui.Drawing.Undo();
        Assert.Single(ui.Drawing.ZipTies);
        Assert.Equal(wireCount, ui.Drawing.Connectors!.Count);
    }

    [AvaloniaFact]
    public void ArrowKeysNudgeEntireBundleUsingConfiguredStep()
    {
        using var ui = new InputHarness();
        ui.Drawing.Settings.NudgeStep = 2;
        ui.Drawing.Settings.NudgeMultiplier = 5;
        ui.Key(Key.Right);
        ui.Key(Key.Down, RawInputModifiers.Shift);
        AssertPoint(new Point(152, 150), ui.Tie.Start);
        AssertPoint(new Point(422, 150), ui.Tie.End);
    }

    [AvaloniaFact]
    public void LockedTieIgnoresPointerDeleteAndNudge()
    {
        using var ui = new InputHarness();
        ui.Tie.IsLocked = true;
        ui.Press(new Point(150, 140));
        ui.Move(new Point(200, 200));
        ui.Release(new Point(200, 200));
        ui.Key(Key.Delete);
        ui.Key(Key.Right);
        Assert.Single(ui.Drawing.ZipTies);
        AssertPoint(new Point(150, 140), ui.Tie.Start);
        Assert.False(ui.Drawing.CanUndo);
    }

    [AvaloniaFact]
    public void HiddenTieCannotBeDragged()
    {
        using var ui = new InputHarness();
        ui.Tie.IsVisible = false;
        ui.Press(new Point(150, 140));
        ui.Move(new Point(200, 200));
        ui.Release(new Point(200, 200));
        AssertPoint(new Point(150, 140), ui.Tie.Start);
        Assert.False(ui.Drawing.CanUndo);
    }

    [AvaloniaFact]
    public void RightClickingBundlePreservesWireSelectionForMembershipCommands()
    {
        using var ui = new InputHarness();
        var selection = new HashSet<IConnector> { ui.Drawing.Connectors![0], ui.Drawing.Connectors[1] };
        ui.Drawing.SetSelectedConnectors(selection);
        ui.Drawing.NotifySelectionChanged();
        ui.Drawing.SelectedZipTie = null;

        ui.ContextClick(new Point(280, 140));

        Assert.Same(ui.Tie, ui.Drawing.SelectedZipTie);
        Assert.Equal(selection, ui.Drawing.GetSelectedConnectors());
        Assert.True(((IZipTieCommands)ui.Drawing).RemoveSelectedFromZipTieCommand.CanExecute(null));
        Assert.False(ui.Drawing.CanUndo);
    }

    [AvaloniaFact]
    public void RemovingSurfaceFromVisualTreeCancelsDragAndReattachesCleanly()
    {
        using var ui = new InputHarness();
        ui.Press(new Point(150, 140));
        ui.Move(new Point(190, 160));
        ui.Input.Children.Remove(ui.Surface);
        Dispatcher.UIThread.RunJobs();
        AssertPoint(new Point(150, 140), ui.Tie.Start);
        Assert.False(ui.Drawing.CanUndo);
        ui.Window.MouseUp(new Point(230, 190), MouseButton.Left);

        ui.Input.Children.Add(ui.Surface);
        Dispatcher.UIThread.RunJobs();
        ui.Press(new Point(420, 140));
        ui.Move(new Point(440, 155));
        ui.Release(new Point(440, 155));
        AssertPoint(new Point(440, 155), ui.Tie.End);
        Assert.True(ui.Drawing.CanUndo);
    }

    [AvaloniaFact]
    public void TinySnapIntervalsKeepInsertedCollarsFinite()
    {
        using var ui = new InputHarness();
        ui.Drawing.Settings.EnableSnap = true;
        ui.Drawing.Settings.SnapX = double.Epsilon;
        ui.Drawing.Settings.SnapY = double.Epsilon;

        ui.DoubleClick(new Point(280, 140));

        AssertPoint(new Point(280, 140), Assert.Single(ui.Tie.Waypoints));
    }

    [AvaloniaFact]
    public void OverflowingNudgeCannotPartiallyMoveAnchorsOrCreateHistory()
    {
        using var ui = new InputHarness();
        ui.Drawing.Settings.NudgeStep = double.MaxValue;
        ui.Drawing.Settings.NudgeMultiplier = double.MaxValue;
        ui.Key(Key.Right, RawInputModifiers.Shift);
        AssertPoint(new Point(150, 140), ui.Tie.Start);
        AssertPoint(new Point(420, 140), ui.Tie.End);
        Assert.False(ui.Drawing.CanUndo);

        ui.Tie.End.X = double.MaxValue;
        ui.Drawing.Settings.NudgeMultiplier = 1;
        ui.Key(Key.Right);
        AssertPoint(new Point(150, 140), ui.Tie.Start);
        Assert.Equal(double.MaxValue, ui.Tie.End.X);
        Assert.False(ui.Drawing.CanUndo);
    }

    [AvaloniaFact]
    public void DeleteAndArrowKeysInTextEditorDoNotEditBundle()
    {
        using var ui = new InputHarness();
        var textBox = new TextBox { Text = "Edit this label", Width = 170, Height = 34 };
        Canvas.SetLeft(textBox, 20);
        Canvas.SetTop(textBox, 350);
        ui.Input.Children.Add(textBox);
        Dispatcher.UIThread.RunJobs();
        textBox.Focus();
        ui.Window.KeyPress(Key.Delete, RawInputModifiers.None, PhysicalKey.Delete, null);
        ui.Window.KeyRelease(Key.Delete, RawInputModifiers.None, PhysicalKey.Delete, null);
        ui.Window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
        ui.Window.KeyRelease(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
        Assert.Single(ui.Drawing.ZipTies);
        AssertPoint(new Point(150, 140), ui.Tie.Start);
    }

    [AvaloniaFact]
    public void DetachingBehaviorDuringDragStopsInputAndBalancesUndo()
    {
        using var ui = new InputHarness();
        ui.Press(new Point(150, 140));
        ui.Move(new Point(190, 160));
        Interaction.GetBehaviors(ui.Surface).Remove(ui.Behavior);
        var afterDetach = PointOf(ui.Drawing.ZipTies[0].Start);
        ui.Move(new Point(240, 230));
        ui.Release(new Point(240, 230));
        AssertPoint(afterDetach, ui.Drawing.ZipTies[0].Start);
        ui.Drawing.ReverseZipTie(ui.Drawing.ZipTies[0]);
        ui.Drawing.Undo();
        AssertPoint(afterDetach, ui.Drawing.ZipTies[0].Start);
    }

    [AvaloniaFact]
    public void DefaultEditorThemeConnectsZipTieRenderingAndInput()
    {
        var drawing = ZipTieDemo.CreateDrawing();
        drawing.SetSerializer(new NodeSerializer(typeof(ObservableCollection<>)));
        var editor = new Editor { DrawingSource = drawing };
        var window = new Window { Width = 1120, Height = 680, Content = editor };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var layer = Assert.Single(editor.GetVisualDescendants().OfType<ZipTieLayer>());
            Assert.Same(drawing, layer.Drawing);
            Assert.False(layer.IsHitTestVisible);
            var surface = editor.GetVisualDescendants().OfType<ItemsControl>()
                .Single(control => control.Name == "PART_NodesItemsControl");
            var behavior = Assert.Single(Interaction.GetBehaviors(surface).OfType<ZipTieInteractionBehavior>());
            Assert.Same(drawing, behavior.DrawingSource);
            Assert.NotNull(behavior.InputSource);

            var tie = drawing.ZipTies[0];
            var origin = surface.TranslatePoint(PointOf(tie.Start), window)!.Value;
            window.MouseDown(origin, MouseButton.Left);
            window.MouseMove(origin + new Vector(40, -25), RawInputModifiers.LeftMouseButton);
            window.MouseUp(origin + new Vector(40, -25), MouseButton.Left);
            Assert.Equal(390, tie.Start.X, precision: 6);
            Assert.Equal(245, tie.Start.Y, precision: 6);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ClickingBundledWireKeepsTheActiveTieForMembershipCommands()
    {
        using var ui = new EditorHarness();
        var tie = ui.Tie;
        ui.Drawing.SetSelectedConnectors(null);
        ui.Drawing.NotifySelectionChanged();

        ui.Click(new Point(510, 252.5));

        Assert.Same(tie, ui.Drawing.SelectedZipTie);
        Assert.Same(ui.Drawing.Connectors![0], Assert.Single(ui.Drawing.GetSelectedConnectors()!));
        AssertPoint(new Point(350, 270), tie.Start);
    }

    [AvaloniaFact]
    public void ShiftDraggingSpanInDefaultEditorMovesTheBundle()
    {
        using var ui = new EditorHarness();
        ui.Drag(new Point(510, 270), new Vector(32, -25), RawInputModifiers.Shift);
        AssertPoint(new Point(382, 245), ui.Tie.Start);
        AssertPoint(new Point(702, 245), ui.Tie.End);
    }

    [AvaloniaFact]
    public void EditorContextMenuBindsZipTieCommandsAndPreservesSelectedWires()
    {
        using var ui = new EditorHarness();
        var selected = new HashSet<IConnector> { ui.Drawing.Connectors![0] };
        ui.Drawing.SetSelectedConnectors(selected);
        ui.Drawing.NotifySelectionChanged();
        ui.ContextClick(new Point(510, 270));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(selected, ui.Drawing.GetSelectedConnectors());
        var drawingControl = Assert.Single(ui.Editor.GetVisualDescendants().OfType<DrawingNode>());
        var menu = Assert.IsType<MenuFlyout>(drawingControl.ContextFlyout);
        Assert.True(menu.IsOpen);
        var zipTies = menu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "Wire zip _ties"));
        var detach = zipTies.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "Detach selected wires"));
        Assert.Same(((IZipTieCommands)ui.Drawing).RemoveSelectedFromZipTieCommand, detach.Command);
        Assert.True(detach.IsEnabled);
        menu.Hide();
    }

    [AvaloniaFact]
    public void DraggingBothEndpointNodesMovesTheWholeBundle()
    {
        using var ui = new EditorHarness();
        var source = ui.Drawing.Nodes![0];
        var destination = ui.Drawing.Nodes[1];
        ui.Tie.Waypoints.Add(new ConnectorPoint(510, 230));
        ui.Drawing.SetSelectedNodes(new HashSet<INode> { source, destination });
        ui.Drawing.NotifySelectionChanged();

        ui.Drag(new Point(source.X + 15, source.Y + 15), new Vector(45, 25));

        Assert.Equal(105, source.X, precision: 6);
        Assert.Equal(125, source.Y, precision: 6);
        Assert.Equal(835, destination.X, precision: 6);
        Assert.Equal(165, destination.Y, precision: 6);
        AssertPoint(new Point(395, 295), ui.Tie.Start);
        AssertPoint(new Point(715, 295), ui.Tie.End);
        AssertPoint(new Point(555, 255), Assert.Single(ui.Tie.Waypoints));
        Assert.Null(ui.Drawing.SelectedZipTie);
    }

    [AvaloniaFact]
    public void DraggingOneEndpointNodeKeepsTheBundleAnchored()
    {
        using var ui = new EditorHarness();
        var source = ui.Drawing.Nodes![0];
        ui.Drawing.SetSelectedNodes(new HashSet<INode> { source });
        ui.Drawing.NotifySelectionChanged();

        ui.Drag(new Point(source.X + 15, source.Y + 15), new Vector(45, 25));

        Assert.Equal(105, source.X, precision: 6);
        Assert.Equal(125, source.Y, precision: 6);
        Assert.Equal(790, ui.Drawing.Nodes[1].X, precision: 6);
        AssertPoint(new Point(350, 270), ui.Tie.Start);
        AssertPoint(new Point(670, 270), ui.Tie.End);
        Assert.Null(ui.Drawing.SelectedZipTie);
    }

    [AvaloniaFact]
    public void SelectingNodeHandsArrowKeysToTheNodeInsteadOfTheActiveTie()
    {
        using var ui = new EditorHarness();
        var source = ui.Drawing.Nodes![0];
        ui.Drawing.Settings.NudgeStep = 2;
        ui.Click(new Point(source.X + 15, source.Y + 15));
        Assert.Null(ui.Drawing.SelectedZipTie);
        Assert.Contains(source, ui.Drawing.GetSelectedNodes()!);

        ui.Editor.ZoomControl!.Focus();
        ui.Window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
        ui.Window.KeyRelease(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);

        Assert.Equal(62, source.X, precision: 6);
        AssertPoint(new Point(350, 270), ui.Tie.Start);
    }

    private static Point PointOf(ConnectorPoint point) => new(point.X, point.Y);

    private static void AssertPoint(Point expected, ConnectorPoint actual)
    {
        Assert.Equal(expected.X, actual.X, precision: 6);
        Assert.Equal(expected.Y, actual.Y, precision: 6);
    }

    private sealed class EditorHarness : IDisposable
    {
        public DrawingNodeViewModel Drawing { get; } = ZipTieDemo.CreateDrawing();
        public IZipTie Tie => Drawing.ZipTies[0];
        public Editor Editor { get; }
        public Window Window { get; }
        public ItemsControl Surface { get; }

        public EditorHarness()
        {
            Drawing.Settings.EnableGuides = false;
            Drawing.SetSerializer(new NodeSerializer(typeof(ObservableCollection<>)));
            Editor = new Editor { DrawingSource = Drawing };
            Window = new Window { Width = 1120, Height = 680, Content = Editor };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Surface = Editor.GetVisualDescendants().OfType<ItemsControl>()
                .Single(control => control.Name == "PART_NodesItemsControl");
        }

        public void Click(Point point)
        {
            var position = ToWindow(point);
            Window.MouseDown(position, MouseButton.Left);
            Window.MouseUp(position, MouseButton.Left);
        }

        public void ContextClick(Point point)
        {
            var position = ToWindow(point);
            Window.MouseDown(position, MouseButton.Right);
            Window.MouseUp(position, MouseButton.Right);
        }

        public void Drag(Point point, Vector delta, RawInputModifiers modifiers = RawInputModifiers.None)
        {
            var start = ToWindow(point);
            var end = ToWindow(point + delta);
            Window.MouseDown(start, MouseButton.Left, modifiers);
            Window.MouseMove(end, modifiers | RawInputModifiers.LeftMouseButton);
            Window.MouseUp(end, MouseButton.Left, modifiers);
        }

        private Point ToWindow(Point point) => Surface.TranslatePoint(point, Window)
            ?? throw new InvalidOperationException("The editor must be attached to its window.");

        public void Dispose() => Window.Close();
    }

    private sealed class InputHarness : IDisposable
    {
        public DrawingNodeViewModel Drawing { get; }
        public IZipTie Tie => Drawing.ZipTies[0];
        public Canvas Input { get; }
        public ItemsControl Surface { get; }
        public Window Window { get; }
        public ZipTieInteractionBehavior Behavior { get; }
        public IPointer? LastPointer { get; private set; }

        public InputHarness(double scale = 1)
        {
            Drawing = ZipTieDemo.CreateDrawing();
            Drawing.ZipTies[0].Start = new ConnectorPoint(150, 140);
            Drawing.ZipTies[0].End = new ConnectorPoint(420, 140);
            Drawing.SetSerializer(new NodeSerializer(typeof(ObservableCollection<>)));
            Surface = new ItemsControl
            {
                Width = 560,
                Height = 340,
                Background = Brushes.Transparent,
                RenderTransformOrigin = RelativePoint.TopLeft,
                RenderTransform = new ScaleTransform(scale, scale)
            };
            Canvas.SetLeft(Surface, 40);
            Canvas.SetTop(Surface, 30);
            Input = new Canvas { Background = Brushes.Transparent, Focusable = true };
            Input.Children.Add(Surface);
            Input.AddHandler(InputElement.PointerPressedEvent, (_, e) => LastPointer = e.Pointer,
                RoutingStrategies.Tunnel, handledEventsToo: true);
            Behavior = new ZipTieInteractionBehavior { DrawingSource = Drawing, InputSource = Input };
            Interaction.GetBehaviors(Surface).Add(Behavior);
            Window = new Window { Width = 920, Height = 620, Content = Input };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Input.Focus();
        }

        private Point ToWindow(Point point) => Surface.TranslatePoint(point, Window)
            ?? throw new InvalidOperationException("The test surface must be attached to its window.");

        public void Press(Point point, RawInputModifiers modifiers = RawInputModifiers.None)
            => Window.MouseDown(ToWindow(point), MouseButton.Left, modifiers);

        public void Move(Point point, RawInputModifiers modifiers = RawInputModifiers.None)
            => Window.MouseMove(ToWindow(point), modifiers | RawInputModifiers.LeftMouseButton);

        public void Release(Point point, RawInputModifiers modifiers = RawInputModifiers.None)
            => Window.MouseUp(ToWindow(point), MouseButton.Left, modifiers);

        public void DoubleClick(Point point)
        {
            Press(point);
            Release(point);
            Press(point);
            Release(point);
        }

        public void ContextClick(Point point)
        {
            Window.MouseDown(ToWindow(point), MouseButton.Right);
            Window.MouseUp(ToWindow(point), MouseButton.Right);
        }

        public void SecondaryClickWhileDragging(Point point)
        {
            Window.MouseDown(ToWindow(point), MouseButton.Right, RawInputModifiers.LeftMouseButton);
            Window.MouseUp(ToWindow(point), MouseButton.Right, RawInputModifiers.LeftMouseButton);
        }

        public void Key(Key key, RawInputModifiers modifiers = RawInputModifiers.None)
        {
            Input.Focus();
            Window.KeyPress(key, modifiers, PhysicalKey.None, null);
            Window.KeyRelease(key, modifiers, PhysicalKey.None, null);
        }

        public void Dispose()
        {
            Interaction.GetBehaviors(Surface).Clear();
            Window.Close();
        }
    }
}
