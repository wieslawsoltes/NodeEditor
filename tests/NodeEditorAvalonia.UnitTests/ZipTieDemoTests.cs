using System;
using System.Reactive.Linq;
using NodeEditor.Model;
using NodeEditorDemo.Services;
using NodeEditorDemo.ViewModels;
using NodeEditorDemo.ViewModels.Nodes;
using Xunit;

namespace NodeEditor.UnitTests;

public class ZipTieDemoTests
{
    [Fact]
    public void DemoBundlesSixColoredWiresWithoutChangingEndpoints()
    {
        var drawing = ZipTieDemo.CreateDrawing();
        Assert.Equal(2, drawing.Nodes!.Count);
        Assert.Equal(6, drawing.Connectors!.Count);
        var tie = Assert.Single(drawing.ZipTies);
        Assert.Equal(6, tie.Connectors.Count);
        Assert.Same(tie, drawing.SelectedZipTie);
        Assert.Equal(7, tie.Spacing);
        for (var i = 0; i < drawing.Connectors.Count; ++i)
        {
            var wire = Assert.IsType<WireConnectorViewModel>(drawing.Connectors[i]);
            Assert.Equal(i, wire.Channel);
            Assert.Same(drawing.Nodes[0].Pins![i], wire.Start);
            Assert.Same(drawing.Nodes[1].Pins![i], wire.End);
            Assert.Same(tie, drawing.FindZipTie(wire));
        }
    }

    [Fact]
    public void SnapshotRestoresLiveMemberReferencesAndWireColors()
    {
        using var vm = new ZipTieDemoViewModel();
        vm.SaveSnapshotCommand.Execute().Subscribe();
        Assert.True(vm.HasSnapshot);
        vm.Drawing.RemoveZipTie(vm.Drawing.ZipTies[0]);
        Assert.Empty(vm.Drawing.ZipTies);
        vm.LoadSnapshotCommand.Execute().Subscribe();
        var tie = Assert.Single(vm.Drawing.ZipTies);
        Assert.Equal(6, tie.Connectors.Count);
        foreach (var member in tie.Connectors)
        {
            Assert.Contains(member, vm.Drawing.Connectors!);
            Assert.Same(vm.Drawing, member.Parent);
            Assert.IsType<WireConnectorViewModel>(member);
        }
        Assert.Contains("Restored 6 wires and 1 bundles", vm.Status);
    }

    [Fact]
    public void LaneGapAndStyleChangesHaveUndoHistory()
    {
        using var vm = new ZipTieDemoViewModel();
        vm.Spacing = 14;
        Assert.Equal(14, vm.Drawing.ZipTies[0].Spacing);
        vm.Drawing.Undo();
        Assert.Equal(7, vm.Drawing.ZipTies[0].Spacing);
        vm.Drawing.Redo();
        Assert.Equal(14, vm.Drawing.ZipTies[0].Spacing);
        vm.SetWireStyleCommand.Execute(ConnectorStyle.Orthogonal).Subscribe();
        Assert.All(vm.Drawing.Connectors!, wire => Assert.Equal(ConnectorStyle.Orthogonal, wire.Style));
        vm.Drawing.Undo();
        Assert.All(vm.Drawing.Connectors!, wire => Assert.Equal(ConnectorStyle.Bezier, wire.Style));
    }

    [Fact]
    public void SocketTitlesAndSubtitlesSurviveUndoRedoAndSnapshotRestore()
    {
        using var vm = new ZipTieDemoViewModel();
        AssertLabels();

        vm.Spacing = 14;
        vm.Drawing.Undo();
        AssertLabels();
        vm.Drawing.Redo();
        AssertLabels();

        vm.SaveSnapshotCommand.Execute().Subscribe();
        Assert.True(vm.HasSnapshot);
        foreach (var node in vm.Drawing.Nodes!)
        {
            var socket = Assert.IsType<WireSocketViewModel>(node.Content);
            socket.Title = "Changed title";
            socket.Subtitle = "Changed subtitle";
        }
        vm.LoadSnapshotCommand.Execute().Subscribe();
        AssertLabels();

        void AssertLabels()
        {
            Assert.Collection(vm.Drawing.Nodes!,
                node =>
                {
                    var socket = Assert.IsType<WireSocketViewModel>(node.Content);
                    Assert.Equal("Source", socket.Title);
                    Assert.Equal("OUTPUT CHANNELS", socket.Subtitle);
                },
                node =>
                {
                    var socket = Assert.IsType<WireSocketViewModel>(node.Content);
                    Assert.Equal("Destination", socket.Title);
                    Assert.Equal("INPUT CHANNELS", socket.Subtitle);
                });
        }
    }

    [Fact]
    public void DemoToolsCanCreateAdditionalTieAndReset()
    {
        using var vm = new ZipTieDemoViewModel();
        vm.AddBendCommand.Execute().Subscribe();
        Assert.Single(vm.Drawing.ZipTies[0].Waypoints);
        vm.SelectWiresCommand.Execute().Subscribe();
        Assert.Equal(6, vm.Drawing.GetSelectedConnectors()!.Count);
        vm.Drawing.RemoveZipTie(vm.Drawing.ZipTies[0]);
        Assert.Empty(vm.Drawing.ZipTies);
        vm.Drawing.CreateZipTieCommand.Execute(null);
        Assert.Single(vm.Drawing.ZipTies);
        vm.ResetCommand.Execute().Subscribe();
        Assert.Equal(6, vm.Drawing.Connectors!.Count);
        Assert.Empty(vm.Drawing.ZipTies[0].Waypoints);
        Assert.False(vm.Drawing.CanUndo);
    }

    [Fact]
    public void InvalidOrLockedSpacingEditsLeaveDrawingUnchanged()
    {
        using var vm = new ZipTieDemoViewModel();
        var tie = vm.Drawing.ZipTies[0];
        vm.Spacing = double.NaN;
        Assert.Equal(7, tie.Spacing);
        tie.IsLocked = true;
        vm.Spacing = 20;
        Assert.Equal(7, tie.Spacing);
        Assert.False(vm.CanEditSelectedTie);
    }
}
