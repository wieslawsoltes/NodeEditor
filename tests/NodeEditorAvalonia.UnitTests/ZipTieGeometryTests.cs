using System;
using System.Collections.ObjectModel;
using Avalonia;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using Xunit;

namespace NodeEditor.UnitTests;

public class ZipTieGeometryTests
{
    [Theory]
    [InlineData(ConnectorStyle.Bezier)]
    [InlineData(ConnectorStyle.Straight)]
    [InlineData(ConnectorStyle.Orthogonal)]
    public void HorizontalBundleRetainsEndpointsAndEquallySpacedLanes(ConnectorStyle style)
    {
        var tie = CreateTie(3, style);
        for (var lane = 0; lane < tie.Connectors.Count; lane++)
        {
            var start = new Point(0, 150 + lane * 40);
            var end = new Point(400, 170 + lane * 20);
            var route = ZipTieGeometry.GetPathPoints(tie, tie.Connectors[lane], start, end);
            Assert.Equal(start, route[0]);
            Assert.Equal(end, route[^1]);
            Assert.Contains(new Point(100, 200 + (lane - 1) * 6), route);
            Assert.Contains(new Point(300, 200 + (lane - 1) * 6), route);
        }
    }

    [Fact]
    public void VerticalBundleOffsetsLanesAlongItsNormal()
    {
        var tie = CreateTie();
        tie.Start = new ConnectorPoint(200, 100);
        tie.End = new ConnectorPoint(200, 300);
        var route = ZipTieGeometry.GetPathPoints(tie, tie.Connectors[0], new Point(200, 0), new Point(200, 400));
        Assert.Contains(new Point(206, 100), route);
        Assert.Contains(new Point(206, 300), route);
        ZipTieGeometry.GetBandSegment(tie, 0, out var bandStart, out var bandEnd);
        Assert.Equal(100, bandStart.Y, 7);
        Assert.Equal(100, bandEnd.Y, 7);
        Assert.Equal(22, Math.Abs(bandEnd.X - bandStart.X), 7);
    }

    [Fact]
    public void ReversingConnectorDirectionPreservesPhysicalLane()
    {
        var tie = CreateTie();
        var connector = tie.Connectors[0];
        var start = new Point(0, 100);
        var end = new Point(400, 250);
        var forward = ZipTieGeometry.GetPathPoints(tie, connector, start, end);
        (connector.Start, connector.End) = (connector.End, connector.Start);
        var reversed = ZipTieGeometry.GetPathPoints(tie, connector, end, start);
        Assert.Equal(forward.Count, reversed.Count);
        for (var i = 0; i < forward.Count; i++)
        {
            Assert.Equal(forward[i].X, reversed[reversed.Count - 1 - i].X, 7);
            Assert.Equal(forward[i].Y, reversed[reversed.Count - 1 - i].Y, 7);
        }
    }

    [Fact]
    public void ReverseLaneOrderChangesSlotsWithoutChangingEndpoints()
    {
        var tie = CreateTie();
        var connector = tie.Connectors[0];
        var start = new Point(0, 100);
        var end = new Point(400, 250);
        var before = ZipTieGeometry.GetPathPoints(tie, connector, start, end);
        tie.Connectors = new ObservableCollection<IConnector> { tie.Connectors[2], tie.Connectors[1], connector };
        var after = ZipTieGeometry.GetPathPoints(tie, connector, start, end);
        Assert.Equal(before[0], after[0]);
        Assert.Equal(before[^1], after[^1]);
        Assert.Contains(new Point(100, 194), before);
        Assert.Contains(new Point(100, 206), after);
    }

    [Fact]
    public void IntermediateCollarCrossesEveryMiteredLane()
    {
        var tie = CreateTie(5);
        tie.Waypoints.Add(new ConnectorPoint(200, 300));
        ZipTieGeometry.GetBandSegment(tie, 1, out var bandStart, out var bandEnd);
        for (var i = 0; i < tie.Connectors.Count; i++)
        {
            var route = ZipTieGeometry.GetPathPoints(tie, tie.Connectors[i], new Point(0, 100), new Point(400, 100));
            Assert.Contains(route, p => DistanceToSegment(p, bandStart, bandEnd) < 0.000001);
        }
        Assert.Equal(3, ZipTieGeometry.GetAnchorCount(tie));
        Assert.Equal(new Point(200, 300), ZipTieGeometry.GetAnchor(tie, 1));
    }

    [Fact]
    public void OrthogonalWiresUseOnlyAxisAlignedSegmentsThroughArbitraryAnchors()
    {
        var tie = CreateTie(3, ConnectorStyle.Orthogonal);
        tie.End = new ConnectorPoint(350, 380);
        tie.Waypoints.Add(new ConnectorPoint(175, 270));
        foreach (var connector in tie.Connectors)
        {
            var route = ZipTieGeometry.GetPathPoints(tie, connector, new Point(-50, 70), new Point(450, 430));
            for (var i = 1; i < route.Count; i++)
            {
                Assert.True(Math.Abs(route[i].X - route[i - 1].X) < 0.000001
                            || Math.Abs(route[i].Y - route[i - 1].Y) < 0.000001);
            }
        }
    }

    [Fact]
    public void CoincidentAndRepeatedAnchorsStayFiniteAndBounded()
    {
        var tie = CreateTie();
        tie.Start = new ConnectorPoint(200, 200);
        tie.End = new ConnectorPoint(200, 200);
        tie.Waypoints.Add(new ConnectorPoint(200, 200));
        tie.Waypoints.Add(new ConnectorPoint(200, 200));
        foreach (var connector in tie.Connectors)
        {
            var route = ZipTieGeometry.GetPathPoints(tie, connector, new Point(0, 100), new Point(400, 300));
            Assert.InRange(route.Count, 2, 200);
            Assert.All(route, point =>
            {
                Assert.True(double.IsFinite(point.X));
                Assert.True(double.IsFinite(point.Y));
                Assert.InRange(point.X, -1, 401);
                Assert.InRange(point.Y, 50, 350);
            });
        }
    }

    [Fact]
    public void SharpCuspUsesBoundedMiterAndAdaptiveCurveFlattening()
    {
        var tie = CreateTie(7);
        tie.Waypoints.Add(new ConnectorPoint(500, 200));
        tie.End = new ConnectorPoint(100, 200.01);
        var route = ZipTieGeometry.GetPathPoints(tie, tie.Connectors[0], new Point(0, 100), new Point(50, 300));
        Assert.InRange(route.Count, 2, 400);
        Assert.All(route, point =>
        {
            Assert.True(double.IsFinite(point.X));
            Assert.True(double.IsFinite(point.Y));
            Assert.InRange(point.X, -100, 700);
            Assert.InRange(point.Y, 0, 500);
        });
    }

    [Fact]
    public void CollarsAreHitBeforeSpanAndHiddenTiesCannotBeHit()
    {
        var tie = CreateTie();
        Assert.True(ZipTieGeometry.TryHitTest(tie, new Point(100, 211), 1, true, out var first));
        Assert.Equal(0, first);
        Assert.True(ZipTieGeometry.TryHitTest(tie, new Point(300, 200), 3, true, out var last));
        Assert.Equal(1, last);
        Assert.False(ZipTieGeometry.TryHitTest(tie, new Point(200, 200), 3, false, out _));
        Assert.True(ZipTieGeometry.TryHitTest(tie, new Point(200, 200), 3, true, out var span));
        Assert.Equal(-1, span);
        Assert.False(ZipTieGeometry.TryHitTest(tie, new Point(200, 250), 3, true, out _));
        tie.IsVisible = false;
        Assert.False(ZipTieGeometry.TryHitTest(tie, new Point(100, 200), 3, true, out _));
    }

    [Fact]
    public void MovingAnchorsUpdatesRouteAndBandWithoutStaleGeometry()
    {
        var tie = CreateTie();
        var connector = tie.Connectors[1];
        var start = new Point(0, 100);
        var end = new Point(400, 250);
        var before = ZipTieGeometry.GetPathPoints(tie, connector, start, end);
        tie.Start.Y = 320;
        tie.End.Y = 320;
        var after = ZipTieGeometry.GetPathPoints(tie, connector, start, end);
        Assert.Contains(new Point(100, 200), before);
        Assert.Contains(new Point(100, 320), after);
        Assert.DoesNotContain(new Point(100, 200), after);
        Assert.True(ZipTieGeometry.TryHitTest(tie, new Point(100, 320), 3, false, out _));
        Assert.False(ZipTieGeometry.TryHitTest(tie, new Point(100, 200), 3, false, out _));
    }

    [Fact]
    public void InvalidAnchorIndexAndNonMemberAreRejected()
    {
        var tie = CreateTie();
        Assert.Throws<ArgumentOutOfRangeException>(() => ZipTieGeometry.GetAnchor(tie, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ZipTieGeometry.GetAnchor(tie, 2));
        Assert.Throws<ArgumentException>(() => ZipTieGeometry.GetPathPoints(tie, new ConnectorViewModel(), default, default));
        Assert.Throws<ArgumentException>(() => ZipTieGeometry.GetPathPoints(tie, tie.Connectors[0], new Point(double.NaN, 0), default));
        Assert.Throws<ArgumentException>(() => ZipTieGeometry.GetPathPoints(tie, tie.Connectors[0], default, new Point(0, double.PositiveInfinity)));
    }

    [Theory]
    [InlineData(0.125)]
    [InlineData(1024)]
    public void ValidSpacingIsHonoredWithoutChangingLaneOrder(double spacing)
    {
        var tie = CreateTie();
        tie.Spacing = spacing;
        var points = ZipTieGeometry.GetPathPoints(tie, tie.Connectors[0], new Point(0, 100), new Point(400, 300));
        Assert.Contains(new Point(100, 200 - spacing), points);
        Assert.Contains(new Point(300, 200 - spacing), points);
    }

    [Fact]
    public void LongStraightBundleDoesNotTessellatePerPixel()
    {
        var tie = CreateTie();
        tie.Start = new ConnectorPoint(100, 200);
        tie.End = new ConnectorPoint(100000, 200);
        var points = ZipTieGeometry.GetPathPoints(tie, tie.Connectors[1], new Point(0, 200), new Point(100100, 200));
        Assert.InRange(points.Count, 4, 8);
    }

    [Fact]
    public void OrthogonalInsertionUsesRenderedDoglegInsteadOfAnchorChord()
    {
        var tie = CreateTie(3, ConnectorStyle.Orthogonal);
        tie.Start = new ConnectorPoint(100, 100);
        tie.End = new ConnectorPoint(100, 500);
        tie.Waypoints.Add(new ConnectorPoint(500, 500));
        var point = new Point(300, 450);

        // The visible vertical dogleg belongs to interval zero, although the other chord is closer.
        Assert.True(DistanceToSegment(point, new Point(100, 100), new Point(500, 500))
                    > DistanceToSegment(point, new Point(500, 500), new Point(100, 500)));
        Assert.Equal(0, ZipTieGeometry.GetInsertionIndex(tie, point));
    }

    [Fact]
    public void BezierInsertionUsesCurvedIntervalInsteadOfNearbyCrossingChord()
    {
        var tie = CreateTie();
        tie.Start = new ConnectorPoint(100, 100);
        tie.End = new ConnectorPoint(100, 500);
        tie.Waypoints.Add(new ConnectorPoint(500, 500));
        tie.Waypoints.Add(new ConnectorPoint(500, 100));
        // Exact midpoint of the first cubic interval; the last anchor chord passes closer to it.
        var point = new Point(284.6718517561812, 377.0598050073099);
        var rendered = ZipTieGeometry.GetPathPoints(tie, tie.Connectors[1], new Point(100, 100), new Point(100, 500));
        Assert.Contains(rendered, p => ((Vector)(p - point)).Length < 0.000001);
        Assert.True(DistanceToSegment(point, new Point(100, 100), new Point(500, 500))
                    > DistanceToSegment(point, new Point(500, 100), new Point(100, 500)));
        Assert.Equal(0, ZipTieGeometry.GetInsertionIndex(tie, point));
    }

    [Fact]
    public void InsertionConsidersAllVisibleMemberStyles()
    {
        var tie = CreateTie();
        tie.Start = new ConnectorPoint(100, 100);
        tie.End = new ConnectorPoint(100, 500);
        tie.Waypoints.Add(new ConnectorPoint(500, 500));
        tie.Connectors[1].Style = ConnectorStyle.Orthogonal;
        tie.Connectors[2].Style = ConnectorStyle.Straight;
        Assert.Equal(0, ZipTieGeometry.GetInsertionIndex(tie, new Point(300, 450)));
    }

    [Fact]
    public void InsertionHandlesCollapsedSpinesAndRejectsNonFinitePointerCoordinates()
    {
        var tie = CreateTie();
        tie.Start = new ConnectorPoint(100, 100);
        tie.End = new ConnectorPoint(100, 100);
        tie.Waypoints.Add(new ConnectorPoint(100, 100));
        Assert.Equal(0, ZipTieGeometry.GetInsertionIndex(tie, new Point(130, 120)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ZipTieGeometry.GetInsertionIndex(tie, new Point(double.NaN, 0)));
    }

    private static ZipTieViewModel CreateTie(int count = 3, ConnectorStyle style = ConnectorStyle.Bezier)
    {
        var tie = new ZipTieViewModel
        {
            Start = new ConnectorPoint(100, 200),
            End = new ConnectorPoint(300, 200),
            Spacing = 6
        };
        for (var i = 0; i < count; i++)
        {
            tie.Connectors.Add(new ConnectorViewModel
            {
                Start = new PinViewModel { Alignment = PinAlignment.Right },
                End = new PinViewModel { Alignment = PinAlignment.Left },
                Style = style
            });
        }
        return tie;
    }

    private static double DistanceToSegment(Point point, Point start, Point end)
    {
        Vector delta = end - start;
        var lengthSquared = delta.X * delta.X + delta.Y * delta.Y;
        Vector relative = point - start;
        var fraction = lengthSquared > 0 ? Math.Clamp((relative.X * delta.X + relative.Y * delta.Y) / lengthSquared, 0, 1) : 0;
        return ((Vector)(point - (start + delta * fraction))).Length;
    }
}
