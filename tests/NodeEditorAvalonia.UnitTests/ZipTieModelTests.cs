using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using Xunit;

namespace NodeEditor.UnitTests;

public sealed class ZipTieModelTests
{
    [Fact]
    public void CreatesStableParallelBundleWithoutChangingConnections()
    {
        var drawing = CreateDrawing(3);
        var original = drawing.Connectors!.ToArray();
        var startPins = original.Select(c => c.Start).ToArray();
        var endPins = original.Select(c => c.End).ToArray();
        var tie = drawing.CreateZipTie(new[] { original[2], original[0], original[1] });

        Assert.NotNull(tie);
        Assert.Same(tie, drawing.SelectedZipTie);
        Assert.Equal(original, tie.Connectors);
        Assert.Equal(tie.Start.Y, tie.End.Y);
        Assert.True(tie.Start.X < tie.End.X);
        for (var i = 0; i < original.Length; i++)
        {
            Assert.Same(startPins[i], original[i].Start);
            Assert.Same(endPins[i], original[i].End);
            Assert.Same(tie, drawing.FindZipTie(original[i]));
        }

        original[0].Start!.Parent!.Y = 100;
        Assert.Equal(original, tie.Connectors);
    }

    [Fact]
    public void IgnoresDuplicateForeignIncompleteHiddenAndLockedWires()
    {
        var drawing = CreateDrawing(5);
        var wires = drawing.Connectors!;
        wires[1].IsLocked = true;
        wires[2].IsVisible = false;
        wires[3].End = null;
        var foreign = CreateDrawing(1).Connectors![0];

        Assert.Null(drawing.CreateZipTie(new[] { wires[0], wires[0], wires[1], wires[2], wires[3], foreign }));
        Assert.Empty(drawing.ZipTies);
        Assert.NotNull(drawing.CreateZipTie(new[] { wires[0], wires[4] }));
        Assert.Null(drawing.CreateZipTie(new[] { wires[0], wires[4] }));
        Assert.Single(drawing.ZipTies);
    }

    [Fact]
    public void SupportsVerticalAndReverseDirectionConnections()
    {
        var drawing = CreateDrawing(2);
        foreach (var node in drawing.Nodes!)
        {
            (node.X, node.Y) = (node.Y, node.X);
            foreach (var pin in node.Pins!)
            {
                (pin.X, pin.Y) = (pin.Y, pin.X);
            }
        }

        var wires = drawing.Connectors!;
        (wires[1].Start, wires[1].End) = (wires[1].End, wires[1].Start);
        var tie = drawing.CreateZipTie(wires)!;
        Assert.Equal(tie.Start.X, tie.End.X);
        Assert.True(tie.Start.Y < tie.End.Y);
        Assert.All(tie.Connectors, c => Assert.Same(tie, drawing.FindZipTie(c)));
    }

    [Fact]
    public void AddingAndRemovingMembersRetainsOrderAndDissolvesSingleton()
    {
        var drawing = CreateDrawing(4);
        var wires = drawing.Connectors!;
        var tie = drawing.CreateZipTie(new[] { wires[0], wires[1] })!;
        Assert.True(drawing.AddToZipTie(tie, wires[3]));
        Assert.Equal(new[] { wires[0], wires[1], wires[3] }, tie.Connectors);
        Assert.False(drawing.AddToZipTie(tie, wires[3]));
        Assert.True(drawing.RemoveFromZipTie(tie, wires[1]));
        Assert.Null(drawing.FindZipTie(wires[1]));
        Assert.True(drawing.RemoveFromZipTie(tie, wires[0]));
        Assert.Empty(drawing.ZipTies);
        Assert.Null(drawing.SelectedZipTie);
        Assert.Null(drawing.FindZipTie(wires[3]));
        Assert.Equal(4, drawing.Connectors!.Count);
    }

    [Fact]
    public void DissolvingRestoresExistingWireRoutingData()
    {
        var drawing = CreateDrawing(2);
        var wire = drawing.Connectors![0];
        wire.RoutingMode = ConnectorRoutingMode.Manual;
        wire.Style = ConnectorStyle.Orthogonal;
        wire.StartArrow = ConnectorArrowStyle.Circle;
        wire.Waypoints.Add(new ConnectorPoint(200, 50));
        var tie = drawing.CreateZipTie(drawing.Connectors)!;

        drawing.RemoveZipTie(tie);

        Assert.Empty(drawing.ZipTies);
        Assert.Equal(2, drawing.Connectors.Count);
        Assert.Equal(ConnectorRoutingMode.Manual, wire.RoutingMode);
        Assert.Equal(ConnectorStyle.Orthogonal, wire.Style);
        Assert.Equal(ConnectorArrowStyle.Circle, wire.StartArrow);
        Assert.Equal(200, Assert.Single(wire.Waypoints).X);
    }

    [Fact]
    public void LockedBundleRejectsAllStructuralEdits()
    {
        var drawing = CreateDrawing(3);
        var wires = drawing.Connectors!;
        var tie = drawing.CreateZipTie(new[] { wires[0], wires[1] })!;
        tie.Waypoints.Add(new ConnectorPoint(200, 300));
        tie.IsLocked = true;

        Assert.False(drawing.AddToZipTie(tie, wires[2]));
        Assert.False(drawing.RemoveFromZipTie(tie, wires[0]));
        drawing.ReverseZipTie(tie);
        drawing.StraightenZipTie(tie);
        drawing.RemoveZipTie(tie);

        Assert.Same(tie, Assert.Single(drawing.ZipTies));
        Assert.Equal(new[] { wires[0], wires[1] }, tie.Connectors);
        Assert.Single(tie.Waypoints);
    }

    [Fact]
    public void ReverseAndStraightenPreserveEndpoints()
    {
        var drawing = CreateDrawing(3);
        var tie = drawing.CreateZipTie(drawing.Connectors!)!;
        var start = tie.Start;
        var end = tie.End;
        var originalOrder = tie.Connectors.ToArray();
        tie.Waypoints.Add(new ConnectorPoint(300, 100));

        drawing.ReverseZipTie(tie);
        drawing.StraightenZipTie(tie);

        Assert.Equal(originalOrder.Reverse(), tie.Connectors);
        for (var i = 0; i < tie.Connectors.Count; i++)
        {
            Assert.Equal(i, drawing.GetZipTieLane(tie.Connectors[i]));
        }
        Assert.Same(start, tie.Start);
        Assert.Same(end, tie.End);
        Assert.Empty(tie.Waypoints);
        Assert.All(tie.Connectors, c => Assert.Same(tie, drawing.FindZipTie(c)));
    }

    [Fact]
    public void GeometryChangesRelayOnlyToTheirOwnerProperty()
    {
        var tie = new ZipTieViewModel();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)tie).PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        tie.Start.X += 10;
        Assert.Equal(new[] { nameof(IZipTie.Start) }, changed);
        changed.Clear();
        var point = new ConnectorPoint(20, 30);
        tie.Waypoints.Add(point);
        point.Y += 10;
        Assert.Equal(new[] { nameof(IZipTie.Waypoints), nameof(IZipTie.Waypoints) }, changed);
        changed.Clear();
        var oldStart = tie.Start;
        tie.Start = new ConnectorPoint(10, 20);
        changed.Clear();
        oldStart.X += 10;
        Assert.Empty(changed);
        tie.Waypoints.Clear();
        changed.Clear();
        point.X += 20;
        Assert.Empty(changed);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1025)]
    public void RejectsInvalidWireSpacing(double spacing)
    {
        var tie = new ZipTieViewModel();
        Assert.Throws<ArgumentOutOfRangeException>(() => tie.Spacing = spacing);
        Assert.Equal(6, tie.Spacing);
    }

    [Fact]
    public void KeepsAnchorCoordinatesFinite()
    {
        var tie = new ZipTieViewModel { Start = new ConnectorPoint(12, 34) };
        Assert.Throws<ArgumentOutOfRangeException>(() => tie.End = new ConnectorPoint(double.NaN, 0));
        tie.Start.X = double.PositiveInfinity;
        tie.Start.Y = double.NaN;
        Assert.Equal(12, tie.Start.X);
        Assert.Equal(34, tie.Start.Y);
    }

    [Fact]
    public void ReplacingMemberCollectionsRebindsIndexAndObservers()
    {
        var drawing = CreateDrawing(4);
        var wires = drawing.Connectors!;
        var tie = drawing.CreateZipTie(new[] { wires[0], wires[1] })!;
        var oldMembers = tie.Connectors;
        tie.Connectors = new ObservableCollection<IConnector> { wires[2], wires[3] };
        Assert.Null(drawing.FindZipTie(wires[0]));
        Assert.Same(tie, drawing.FindZipTie(wires[2]));
        oldMembers.Add(wires[2]);
        Assert.Null(drawing.FindZipTie(wires[0]));
        drawing.ZipTies = new ObservableCollection<IZipTie>();
        Assert.Null(drawing.SelectedZipTie);
        Assert.Null(drawing.FindZipTie(wires[2]));
    }

    [Fact]
    public void EditingCommandsSupportNonObservableHostAndMemberLists()
    {
        var drawing = CreateDrawing(3);
        drawing.ZipTies = new List<IZipTie>();
        var wires = drawing.Connectors!;
        var tie = drawing.CreateZipTie(new[] { wires[0], wires[1] })!;
        Assert.Same(tie, drawing.FindZipTie(wires[0]));
        tie.Connectors = new List<IConnector>(tie.Connectors);
        Assert.True(drawing.AddToZipTie(tie, wires[2]));
        Assert.Same(tie, drawing.FindZipTie(wires[2]));
        Assert.True(drawing.RemoveFromZipTie(tie, wires[2]));
        Assert.Null(drawing.FindZipTie(wires[2]));
        drawing.RemoveZipTie(tie);
        Assert.Empty(drawing.ZipTies);
        Assert.Null(drawing.FindZipTie(wires[0]));
    }

    [Fact]
    public void NormalizesMalformedCollectionsWithoutReentrantMutation()
    {
        var drawing = CreateDrawing(5);
        var wires = drawing.Connectors!;
        var first = drawing.CreateZipTie(wires.Take(3))!;
        var second = new ZipTieViewModel
        {
            Connectors = new List<IConnector> { wires[0], wires[3], wires[3], null!, wires[4] }
        };
        drawing.ZipTies.Add(second);
        Assert.Equal(new[] { wires[3], wires[4] }, second.Connectors);
        Assert.Same(first, drawing.FindZipTie(wires[0]));
        Assert.Same(second, drawing.FindZipTie(wires[3]));

        var list = (INotifyCollectionChanged)drawing.ZipTies;
        list.CollectionChanged += (_, _) => { };
        drawing.ZipTies.Add(null!);
        drawing.ZipTies.Add(first);
        Assert.Equal(new[] { first, second }, drawing.ZipTies);
        second.Waypoints.Add(null!);
        Assert.Empty(second.Waypoints);
        second.Connectors.Add(wires[3]);
        Assert.Equal(new[] { wires[3], wires[4] }, second.Connectors);
        drawing.Connectors!.Remove(wires[4]);
        Assert.Same(first, Assert.Single(drawing.ZipTies));
        Assert.Equal(-1, drawing.GetZipTieLane(wires[3]));
    }

    [Fact]
    public void ReadOnlyCollectionInputsRemainEditable()
    {
        var drawing = CreateDrawing(2);
        var tie = new ZipTieViewModel
        {
            Connectors = drawing.Connectors!.ToArray(),
            Waypoints = new[] { new ConnectorPoint(250, 10) }
        };
        drawing.ZipTies = new[] { tie };
        tie.Waypoints.Add(new ConnectorPoint(300, 10));
        drawing.RemoveZipTie(tie);
        Assert.Empty(drawing.ZipTies);
    }

    [Fact]
    public void RemovingWiresPrunesLockedBundlesAndUnsubscribesIndex()
    {
        var drawing = CreateDrawing(3);
        var tie = drawing.CreateZipTie(drawing.Connectors!)!;
        tie.IsLocked = true;
        drawing.Connectors!.RemoveAt(0);
        Assert.Equal(2, tie.Connectors.Count);
        Assert.Single(drawing.ZipTies);
        drawing.Connectors.RemoveAt(0);
        Assert.Empty(drawing.ZipTies);
        Assert.Null(drawing.SelectedZipTie);
        Assert.Null(drawing.FindZipTie(drawing.Connectors[0]));
    }

    [Fact]
    public void DeletingNodePrunesBundlesWithNonObservableConnectorList()
    {
        var drawing = CreateDrawing(3);
        drawing.Connectors = new List<IConnector>(drawing.Connectors!);
        var tie = drawing.CreateZipTie(drawing.Connectors)!;
        drawing.SelectedZipTie = null;
        drawing.SetSelectedNodes(new HashSet<INode> { drawing.Nodes![0] });

        drawing.DeleteNodes();

        Assert.Equal(2, tie.Connectors.Count);
        Assert.Equal(2, drawing.Connectors.Count);
        Assert.Single(drawing.ZipTies);
    }

    [Fact]
    public void SerializationPreservesIdentitySettingsAndIntermediateTies()
    {
        var drawing = CreateDrawing(3);
        var serializer = new NodeSerializer(typeof(ObservableCollection<>));
        var tie = drawing.CreateZipTie(drawing.Connectors!)!;
        tie.Name = "Signal trunk";
        tie.Spacing = 9;
        tie.Waypoints.Add(new ConnectorPoint(300, 180));
        tie.IsLocked = true;
        var json = serializer.Serialize(drawing);
        var restored = serializer.Deserialize<DrawingNodeViewModel>(json)!;

        Assert.DoesNotContain("SelectedZipTie", json);
        Assert.DoesNotContain("ZipTieCommand", json);
        var restoredTie = Assert.Single(restored.ZipTies);
        Assert.Equal("Signal trunk", restoredTie.Name);
        Assert.Equal(9, restoredTie.Spacing);
        Assert.True(restoredTie.IsLocked);
        Assert.Equal(180, Assert.Single(restoredTie.Waypoints).Y);
        Assert.Null(restored.SelectedZipTie);
        Assert.Equal(restored.Connectors, restoredTie.Connectors);
        foreach (var wire in restored.Connectors!)
        {
            Assert.Same(restored, wire.Parent);
            Assert.Same(restoredTie, restored.FindZipTie(wire));
        }
    }

    [Fact]
    public void UndoRedoRestoresBundleMembershipAndBatchedGeometry()
    {
        var drawing = CreateDrawing(3);
        drawing.SetSerializer(new NodeSerializer(typeof(ObservableCollection<>)));
        drawing.CreateZipTie(drawing.Connectors!);
        Assert.True(drawing.CanUndo);
        drawing.Undo();
        Assert.Empty(drawing.ZipTies);
        drawing.Redo();
        var tie = Assert.Single(drawing.ZipTies);
        Assert.All(drawing.Connectors!, c => Assert.Same(tie, drawing.FindZipTie(c)));

        var initialX = tie.Start.X;
        drawing.BeginUndoBatch();
        tie.Start.X += 20;
        tie.Start.X += 30;
        tie.Waypoints.Add(new ConnectorPoint(250, 70));
        drawing.EndUndoBatch();
        drawing.Undo();
        Assert.Equal(initialX, Assert.Single(drawing.ZipTies).Start.X);
        Assert.Empty(drawing.ZipTies[0].Waypoints);
        drawing.Redo();
        Assert.Equal(initialX + 50, drawing.ZipTies[0].Start.X);
        Assert.Single(drawing.ZipTies[0].Waypoints);
    }

    [Fact]
    public void CopyPasteTranslatesBundleAndRemapsOnlyPastedMembers()
    {
        var drawing = CreateDrawing(3);
        drawing.SetSerializer(new NodeSerializer(typeof(ObservableCollection<>)));
        var tie = drawing.CreateZipTie(drawing.Connectors!)!;
        tie.Waypoints.Add(new ConnectorPoint(250, 70));
        // Copy only two of three complete endpoint pairs.
        drawing.SetSelectedNodes(new HashSet<INode>(drawing.Nodes!.Take(4)));
        drawing.CopyNodes();
        drawing.DrawingRightPressed(1000, 800);
        drawing.PasteNodes();

        Assert.Equal(2, drawing.ZipTies.Count);
        Assert.Null(drawing.SelectedZipTie);
        var pasted = drawing.ZipTies[1];
        Assert.Equal(2, pasted.Connectors.Count);
        Assert.DoesNotContain(pasted.Connectors[0], tie.Connectors);
        Assert.Equal(tie.Start.X + 1000, pasted.Start.X);
        Assert.Equal(tie.Start.Y + 800, pasted.Start.Y);
        Assert.Equal(tie.Waypoints[0].X + 1000, pasted.Waypoints[0].X);
        Assert.Equal(tie.Waypoints[0].Y + 800, pasted.Waypoints[0].Y);
        Assert.All(pasted.Connectors, c =>
        {
            Assert.Contains(c, drawing.Connectors!);
            Assert.Same(drawing, c.Parent);
            Assert.Same(pasted, drawing.FindZipTie(c));
        });
    }

    [Fact]
    public void CloneDrawingRemapsBundleMembersToClonedWires()
    {
        var drawing = CreateDrawing(2);
        drawing.SetSerializer(new NodeSerializer(typeof(ObservableCollection<>)));
        var tie = drawing.CreateZipTie(drawing.Connectors!)!;
        var clone = drawing.Clone(drawing)!;
        var clonedTie = Assert.Single(clone.ZipTies);
        Assert.NotSame(tie, clonedTie);
        Assert.Equal(clone.Connectors, clonedTie.Connectors);
        Assert.NotSame(drawing.Connectors![0], clonedTie.Connectors[0]);
    }

    [Fact]
    public void CommandsFollowSelectionAndExistingLockAndVisibilityTools()
    {
        var drawing = CreateDrawing(3);
        drawing.SetSelectedConnectors(new HashSet<IConnector>(drawing.Connectors!.Take(2)));
        drawing.NotifySelectionChanged();
        Assert.True(drawing.CreateZipTieCommand.CanExecute(null));
        drawing.CreateZipTieCommand.Execute(null);
        var tie = Assert.Single(drawing.ZipTies);
        Assert.False(drawing.CreateZipTieCommand.CanExecute(null));
        Assert.True(drawing.RemoveZipTieCommand.CanExecute(null));
        drawing.LockSelection();
        Assert.True(tie.IsLocked);
        Assert.False(drawing.RemoveZipTieCommand.CanExecute(null));
        drawing.UnlockSelection();
        Assert.False(tie.IsLocked);
        drawing.HideSelection();
        Assert.False(tie.IsVisible);
        Assert.Null(drawing.SelectedZipTie);
        drawing.ShowAll();
        Assert.True(tie.IsVisible);
    }

    [Fact]
    public void BulkMembershipCommandsPublishOneAtomicChangePerBundle()
    {
        var drawing = CreateDrawing(8);
        var tie = drawing.CreateZipTie(drawing.Connectors!.Take(2))!;
        var updates = 0;
        tie.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(IZipTie.Connectors))
            {
                updates++;
            }
        };
        drawing.SetSelectedConnectors(new HashSet<IConnector>(drawing.Connectors!.Skip(2)));
        drawing.NotifySelectionChanged();
        drawing.AddSelectedToZipTieCommand.Execute(null);
        Assert.Equal(8, tie.Connectors.Count);
        Assert.Equal(1, updates);
        Assert.All(tie.Connectors, c => Assert.Same(tie, drawing.FindZipTie(c)));

        updates = 0;
        drawing.RemoveSelectedFromZipTieCommand.Execute(null);
        Assert.Equal(2, tie.Connectors.Count);
        Assert.Equal(1, updates);
        Assert.All(drawing.Connectors!.Skip(2), c => Assert.Null(drawing.FindZipTie(c)));
    }

    [Fact]
    public void DeleteSelectedBundleRetainsUnselectedMemberWires()
    {
        var drawing = CreateDrawing(2);
        drawing.CreateZipTie(drawing.Connectors!);
        drawing.DeleteNodes();
        Assert.Empty(drawing.ZipTies);
        Assert.Equal(2, drawing.Connectors!.Count);
        Assert.Equal(4, drawing.Nodes!.Count);

        drawing.CreateZipTie(drawing.Connectors!);
        drawing.SetSelectedConnectors(new HashSet<IConnector> { drawing.Connectors[0] });
        drawing.DeleteNodes();
        Assert.Empty(drawing.ZipTies);
        Assert.Single(drawing.Connectors);
    }

    [Fact]
    public void SnapshotRestorationDetachesTemporaryParentSubscriptions()
    {
        var drawing = CreateDrawing(3);
        var oldWire = drawing.Connectors![0];
        drawing.Connectors[0] = new ParentTrackingConnectorViewModel
        {
            Parent = drawing, Start = oldWire.Start, End = oldWire.End
        };
        drawing.SetSerializer(new NodeSerializer(typeof(ObservableCollection<>)));
        drawing.CreateZipTie(drawing.Connectors);
        drawing.Undo();
        drawing.Redo();

        var wire = Assert.IsType<ParentTrackingConnectorViewModel>(drawing.Connectors![0]);
        var temporaryParent = Assert.IsType<DrawingNodeViewModel>(wire.PreviousParent);
        Assert.NotSame(drawing, temporaryParent);
        var retiredNotifications = 0;
        temporaryParent.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(IZipTieHost.ZipTies))
            {
                retiredNotifications++;
            }
        };

        drawing.ReverseZipTie(Assert.Single(drawing.ZipTies));
        Assert.Equal(0, retiredNotifications);
        Assert.All(drawing.Connectors!, c => Assert.Same(drawing, c.Parent));
    }

    public sealed class ParentTrackingConnectorViewModel : ConnectorViewModel
    {
        private IDrawingNode? _lastParent;

        public ParentTrackingConnectorViewModel()
        {
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(Parent))
                {
                    if (_lastParent is not null && !ReferenceEquals(_lastParent, Parent))
                    {
                        PreviousParent = _lastParent;
                    }

                    _lastParent = Parent;
                }
            };
        }

        [JsonIgnore]
        public IDrawingNode? PreviousParent { get; private set; }
    }

    private static DrawingNodeViewModel CreateDrawing(int wireCount)
    {
        var drawing = new DrawingNodeViewModel
        {
            Nodes = new ObservableCollection<INode>(),
            Connectors = new ObservableCollection<IConnector>()
        };
        for (var i = 0; i < wireCount; i++)
        {
            var left = new NodeViewModel { X = 0, Y = i * 50, Width = 100, Height = 30, Parent = drawing };
            var right = new NodeViewModel { X = 500, Y = i * 50, Width = 100, Height = 30, Parent = drawing };
            var output = new PinViewModel { X = 100, Y = 15, Alignment = PinAlignment.Right, Parent = left };
            var input = new PinViewModel { X = 0, Y = 15, Alignment = PinAlignment.Left, Parent = right };
            left.Pins = new ObservableCollection<IPin> { output };
            right.Pins = new ObservableCollection<IPin> { input };
            drawing.Nodes.Add(left);
            drawing.Nodes.Add(right);
            drawing.Connectors.Add(new ConnectorViewModel { Start = output, End = input, Parent = drawing });
        }

        return drawing;
    }
}
