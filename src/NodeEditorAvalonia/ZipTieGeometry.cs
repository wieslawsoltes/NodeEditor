using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using NodeEditor.Model;

namespace NodeEditor;

/// <summary>Shared geometry for bundled wire routes, zip-tie collars, and interaction hit testing.</summary>
/// <remarks>
/// Anchors are indexed from the start collar (zero), through the intermediate collars, to the end
/// collar. Stored connector order determines lane order independently of connector direction.
/// </remarks>
public static class ZipTieGeometry
{
    private const double Epsilon = 0.000001;
    private const double FlattenTolerance = 0.35;

    /// <summary>Gets the number of collars, including the two end collars.</summary>
    public static int GetAnchorCount(IZipTie tie) => tie.Waypoints.Count + 2;

    /// <summary>Gets a collar's drawing-space center.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The anchor index is outside the bundle.</exception>
    public static Point GetAnchor(IZipTie tie, int anchorIndex)
    {
        var count = GetAnchorCount(tie);
        if ((uint)anchorIndex >= (uint)count)
        {
            throw new ArgumentOutOfRangeException(nameof(anchorIndex));
        }

        var point = anchorIndex == 0 ? tie.Start : anchorIndex == count - 1 ? tie.End : tie.Waypoints[anchorIndex - 1];
        return new Point(SafeCoordinate(point.X), SafeCoordinate(point.Y));
    }

    /// <summary>Gets the endpoints of a collar crossing every lane, including a small end margin.</summary>
    public static void GetBandSegment(IZipTie tie, int anchorIndex, out Point start, out Point end)
    {
        var center = GetAnchor(tie, anchorIndex);
        var normal = GetMiterNormal(tie, anchorIndex);
        var halfWidth = Math.Max(0, tie.Connectors.Count - 1) * GetSpacing(tie) * 0.5;
        var margin = Normalize(normal, new Vector(0, 1)) * 5;
        var extent = normal * halfWidth + margin;
        start = center - extent;
        end = center + extent;
    }

    /// <summary>Tests collars first, then optionally the bundled span.</summary>
    /// <param name="tie">The zip tie to test.</param>
    /// <param name="point">The pointer in drawing coordinates.</param>
    /// <param name="tolerance">Hit tolerance in drawing units.</param>
    /// <param name="includeSpan">Whether the space occupied by the bundled wires can be hit.</param>
    /// <param name="anchorIndex">The collar index, or minus one for the span.</param>
    /// <returns>Whether a visible bundle was hit.</returns>
    public static bool TryHitTest(IZipTie tie, Point point, double tolerance, bool includeSpan, out int anchorIndex)
    {
        anchorIndex = -1;
        if (!tie.IsVisible || tie.Connectors.Count < 2 || !IsFinite(point))
        {
            return false;
        }

        tolerance = double.IsFinite(tolerance) ? Math.Max(0, tolerance) : 0;
        var bands = GetBands(tie);
        var bestDistance = tolerance;
        for (var i = 0; i < bands.Length; i++)
        {
            var distance = DistanceToSegment(point, bands[i].Start, bands[i].End);
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                anchorIndex = i;
            }
        }

        if (anchorIndex >= 0)
        {
            return true;
        }

        if (!includeSpan)
        {
            return false;
        }

        var halfWidth = Math.Max(0, tie.Connectors.Count - 1) * GetSpacing(tie) * 0.5 + tolerance;
        var testedStyles = 0;
        foreach (var connector in tie.Connectors)
        {
            var style = connector.Style;
            var styleMask = style == ConnectorStyle.Bezier ? 1 : style == ConnectorStyle.Orthogonal ? 2 : 4;
            if (!connector.IsVisible || (testedStyles & styleMask) != 0)
            {
                continue;
            }
            testedStyles |= styleMask;
            var spine = BuildSpinePath(tie, style).Flatten(FlattenTolerance);
            for (var i = 1; i < spine.Count; i++)
            {
                if (DistanceToSegment(point, spine[i - 1], spine[i]) <= halfWidth)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the intermediate-anchor insertion index of the nearest rendered spine interval.
    /// Curved and orthogonal routes are measured using the same segments as wire rendering,
    /// considering each distinct style among visible member wires.
    /// </summary>
    /// <param name="tie">The bundle whose spine is inspected.</param>
    /// <param name="point">The proposed anchor position in drawing coordinates.</param>
    /// <returns>An insertion index from zero through <c>tie.Waypoints.Count</c>, inclusive.</returns>
    /// <remarks>
    /// Equal-distance intervals resolve to the earliest interval. When no member wire is visible,
    /// the default Bézier spine supplies a deterministic insertion position.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The point contains a non-finite coordinate.</exception>
    public static int GetInsertionIndex(IZipTie tie, Point point)
    {
        if (!IsFinite(point))
        {
            throw new ArgumentOutOfRangeException(nameof(point), "The anchor position must contain finite drawing coordinates.");
        }

        var count = GetAnchorCount(tie);
        Span<SpineFrame> frames = count <= 64 ? stackalloc SpineFrame[count] : new SpineFrame[count];
        BuildFrames(tie, frames);
        var styles = 0;
        foreach (var connector in tie.Connectors)
        {
            if (connector.IsVisible)
            {
                styles |= connector.Style == ConnectorStyle.Bezier ? 1 : connector.Style == ConnectorStyle.Orthogonal ? 2 : 4;
            }
        }
        if (styles == 0)
        {
            styles = 1;
        }

        var bestDistance = double.PositiveInfinity;
        var bestIndex = 0;
        for (var mask = 1; mask <= 4; mask <<= 1)
        {
            if ((styles & mask) == 0)
            {
                continue;
            }
            var style = mask == 1 ? ConnectorStyle.Bezier : mask == 2 ? ConnectorStyle.Orthogonal : ConnectorStyle.Straight;
            for (var index = 0; index < count - 1; index++)
            {
                var interval = new ConnectorPath(frames[index].Center);
                AppendSpineInterval(interval, frames[index], frames[index + 1], style);
                var points = interval.Flatten(FlattenTolerance);
                var distance = Distance(point, points[0]);
                for (var i = 1; i < points.Count; i++)
                {
                    distance = Math.Min(distance, DistanceToSegment(point, points[i - 1], points[i]));
                }

                if (distance < bestDistance - Epsilon
                    || (Math.Abs(distance - bestDistance) <= Epsilon && index < bestIndex))
                {
                    bestDistance = distance;
                    bestIndex = index;
                }
            }
        }
        return bestIndex;
    }

    /// <summary>
    /// Gets the flattened, drawing-space route of a member wire, including both fan-outs.
    /// The same cubic segments are used by the renderer and by selection, arrows, labels, and crossings.
    /// </summary>
    /// <exception cref="ArgumentException">The connector is not a member of the zip tie.</exception>
    public static IReadOnlyList<Point> GetPathPoints(IZipTie tie, IConnector connector, Point start, Point end)
    {
        if (!IsFinite(start) || !IsFinite(end))
        {
            throw new ArgumentException("Wire endpoints must contain finite drawing coordinates.");
        }

        var lane = FindLane(tie, connector);
        if (lane < 0)
        {
            throw new ArgumentException("The connector must belong to the zip tie.", nameof(connector));
        }

        return BuildPath(tie, connector, lane, start, end).Flatten(FlattenTolerance);
    }

    internal static bool TryCreatePath(IConnector connector, Point start, Point end, out ConnectorPath path)
    {
        path = null!;
        if (connector.Parent is not IZipTieHost host
            || host.FindZipTie(connector) is not { IsVisible: true } tie
            || tie.Connectors.Count < 2
            || !IsFinite(start)
            || !IsFinite(end))
        {
            return false;
        }

        var lane = FindLane(tie, connector);
        if (lane < 0)
        {
            return false;
        }

        path = BuildPath(tie, connector, lane, start, end);
        return true;
    }

    internal static ZipTieBand[] GetBands(IZipTie tie)
    {
        var count = GetAnchorCount(tie);
        Span<SpineFrame> frames = count <= 64 ? stackalloc SpineFrame[count] : new SpineFrame[count];
        BuildFrames(tie, frames);
        var bands = new ZipTieBand[count];
        var halfWidth = Math.Max(0, tie.Connectors.Count - 1) * GetSpacing(tie) * 0.5;
        for (var i = 0; i < count; i++)
        {
            var extent = frames[i].Normal * halfWidth + Normalize(frames[i].Normal, new Vector(0, 1)) * 5;
            bands[i] = new ZipTieBand(frames[i].Center - extent, frames[i].Center + extent);
        }
        return bands;
    }

    internal static ConnectorPath BuildSpinePath(IZipTie tie, ConnectorStyle style = ConnectorStyle.Bezier)
    {
        var count = GetAnchorCount(tie);
        Span<SpineFrame> frames = count <= 64 ? stackalloc SpineFrame[count] : new SpineFrame[count];
        BuildFrames(tie, frames);
        var path = new ConnectorPath(frames[0].Center);
        for (var i = 1; i < count; i++)
        {
            AppendSpineInterval(path, frames[i - 1], frames[i], style);
        }
        return path;
    }

    private static void AppendSpineInterval(ConnectorPath path, SpineFrame start, SpineFrame end, ConnectorStyle style)
    {
        if (style == ConnectorStyle.Bezier)
        {
            AppendSpineSegment(path, start.Center, end.Center, start.Tangent, end.Tangent);
        }
        else if (style == ConnectorStyle.Orthogonal)
        {
            AppendOrthogonal(path, start.Center, end.Center, start.Tangent);
        }
        else
        {
            path.LineTo(end.Center);
        }
    }

    private static ConnectorPath BuildPath(IZipTie tie, IConnector connector, int lane, Point start, Point end)
    {
        var count = GetAnchorCount(tie);
        Span<SpineFrame> frames = count <= 64 ? stackalloc SpineFrame[count] : new SpineFrame[count];
        BuildFrames(tie, frames);
        var firstCenter = frames[0].Center;
        var lastCenter = frames[count - 1].Center;
        // Choose traversal per wire. Reversing a connector must not exchange physical lanes.
        var forward = Distance(start, firstCenter) + Distance(end, lastCenter)
                      <= Distance(start, lastCenter) + Distance(end, firstCenter);
        var direction = forward ? 1 : -1;
        var first = forward ? 0 : count - 1;
        var last = forward ? count - 1 : 0;
        var offset = (lane - (tie.Connectors.Count - 1) * 0.5) * GetSpacing(tie);
        var firstPoint = frames[first].GetLanePoint(offset);
        var firstTangent = frames[first].Tangent * direction;
        var path = new ConnectorPath(start);

        if (connector.Style == ConnectorStyle.Bezier)
        {
            var handle = GetFanHandle(start, firstPoint, connector.Offset);
            path.CubicTo(start + GetPinDirection(connector.Start, firstPoint - start) * handle,
                firstPoint - firstTangent * handle, firstPoint);
        }
        else if (connector.Style == ConnectorStyle.Orthogonal)
        {
            AppendOrthogonal(path, start, firstPoint, GetPinDirection(connector.Start, firstPoint - start));
        }
        else
        {
            path.LineTo(firstPoint);
        }

        for (var index = first; index != last; index += direction)
        {
            var next = index + direction;
            var a = frames[index].GetLanePoint(offset);
            var b = frames[next].GetLanePoint(offset);
            if (connector.Style == ConnectorStyle.Bezier)
            {
                AppendSpineSegment(path, a, b, frames[index].Tangent * direction, frames[next].Tangent * direction);
            }
            else if (connector.Style == ConnectorStyle.Orthogonal)
            {
                // Every member follows the same physical dogleg, including opposite-direction wires.
                AppendOrthogonal(path, a, b, frames[Math.Min(index, next)].Tangent);
            }
            else
            {
                path.LineTo(b);
            }
        }

        var lastPoint = frames[last].GetLanePoint(offset);
        if (connector.Style == ConnectorStyle.Bezier)
        {
            var handle = GetFanHandle(lastPoint, end, connector.Offset);
            path.CubicTo(lastPoint + frames[last].Tangent * (direction * handle),
                end + GetPinDirection(connector.End, lastPoint - end) * handle, end);
        }
        else if (connector.Style == ConnectorStyle.Orthogonal)
        {
            AppendOrthogonal(path, lastPoint, end, frames[last].Tangent * direction);
        }
        else
        {
            path.LineTo(end);
        }

        return path;
    }

    private static void AppendSpineSegment(ConnectorPath path, Point start, Point end, Vector startTangent, Vector endTangent)
    {
        Vector delta = end - start;
        var length = delta.Length;
        if (length < Epsilon)
        {
            path.LineTo(end);
            return;
        }

        var unit = delta / length;
        if ((unit - startTangent).Length < Epsilon && (unit - endTangent).Length < Epsilon)
        {
            path.LineTo(end);
            return;
        }

        // A bounded Hermite span keeps cusp and short-span edits from producing runaway handles.
        var handle = length / 3;
        path.CubicTo(start + startTangent * handle, end - endTangent * handle, end);
    }

    private static void AppendOrthogonal(ConnectorPath path, Point start, Point end, Vector tangent)
    {
        if (Math.Abs(start.X - end.X) <= Epsilon || Math.Abs(start.Y - end.Y) <= Epsilon)
        {
            path.LineTo(end);
            return;
        }

        if (Math.Abs(tangent.X) >= Math.Abs(tangent.Y))
        {
            var x = (start.X + end.X) * 0.5;
            path.LineTo(new Point(x, start.Y));
            path.LineTo(new Point(x, end.Y));
        }
        else
        {
            var y = (start.Y + end.Y) * 0.5;
            path.LineTo(new Point(start.X, y));
            path.LineTo(new Point(end.X, y));
        }

        path.LineTo(end);
    }

    private static void BuildFrames(IZipTie tie, Span<SpineFrame> frames)
    {
        var before = default(Vector);
        for (var i = 0; i < frames.Length; i++)
        {
            var center = GetAnchor(tie, i);
            if (i > 0)
            {
                Vector delta = center - frames[i - 1].Center;
                if (delta.Length > Epsilon)
                {
                    before = Normalize(delta, new Vector(1, 0));
                }
            }
            // The first pass temporarily stores the incoming direction in Tangent.
            frames[i] = new SpineFrame(center, before, default);
        }

        var after = default(Vector);
        for (var i = frames.Length - 1; i >= 0; i--)
        {
            if (i + 1 < frames.Length)
            {
                Vector delta = frames[i + 1].Center - frames[i].Center;
                if (delta.Length > Epsilon)
                {
                    after = Normalize(delta, new Vector(1, 0));
                }
            }
            before = frames[i].Tangent;
            var outgoing = after.Length > Epsilon ? after : before;
            if (before.Length < Epsilon)
            {
                before = outgoing.Length > Epsilon ? outgoing : new Vector(1, 0);
            }
            if (outgoing.Length < Epsilon)
            {
                outgoing = before;
            }
            var tangent = Normalize(before + outgoing, outgoing);
            var normal = ComputeMiterNormal(before, outgoing);
            frames[i] = new SpineFrame(frames[i].Center, tangent, normal);
        }
    }

    private readonly record struct SpineFrame(Point Center, Vector Tangent, Vector Normal)
    {
        internal Point GetLanePoint(double offset) => Center + Normal * offset;
    }

    private static Vector GetMiterNormal(IZipTie tie, int anchorIndex)
    {
        GetDirections(tie, anchorIndex, out var before, out var after);
        return ComputeMiterNormal(before, after);
    }

    private static Vector ComputeMiterNormal(Vector before, Vector after)
    {
        var beforeNormal = new Vector(-before.Y, before.X);
        var afterNormal = new Vector(-after.Y, after.X);
        var normal = Normalize(beforeNormal + afterNormal, afterNormal);
        var denominator = Math.Abs(normal.X * afterNormal.X + normal.Y * afterNormal.Y);
        // Limit miters to four lane offsets, including a 180-degree reversal/collapsed segment.
        return normal / Math.Max(0.25, denominator);
    }

    private static void GetDirections(IZipTie tie, int anchorIndex, out Vector before, out Vector after)
    {
        var point = GetAnchor(tie, anchorIndex);
        before = default;
        after = default;
        for (var i = anchorIndex - 1; i >= 0; i--)
        {
            Vector delta = point - GetAnchor(tie, i);
            if (delta.Length > Epsilon)
            {
                before = Normalize(delta, new Vector(1, 0));
                break;
            }
        }

        for (var i = anchorIndex + 1; i < GetAnchorCount(tie); i++)
        {
            Vector delta = GetAnchor(tie, i) - point;
            if (delta.Length > Epsilon)
            {
                after = Normalize(delta, new Vector(1, 0));
                break;
            }
        }

        if (before.Length < Epsilon)
        {
            before = after.Length > Epsilon ? after : new Vector(1, 0);
        }

        if (after.Length < Epsilon)
        {
            after = before;
        }
    }

    private static Vector GetPinDirection(IPin? pin, Vector fallback)
    {
        var direction = pin?.Alignment switch
        {
            PinAlignment.Left => new Vector(-1, 0),
            PinAlignment.Right => new Vector(1, 0),
            PinAlignment.Top => new Vector(0, -1),
            PinAlignment.Bottom => new Vector(0, 1),
            _ => Normalize(fallback, new Vector(1, 0))
        };

        if (pin?.Alignment is PinAlignment.None || pin is null)
        {
            return direction;
        }

        var degrees = pin.Parent?.Rotation ?? 0;
        if (!double.IsFinite(degrees))
        {
            return direction;
        }

        var radians = degrees * Math.PI / 180;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return new Vector(direction.X * cos - direction.Y * sin, direction.X * sin + direction.Y * cos);
    }

    private static double GetFanHandle(Point start, Point end, double offset)
        => Math.Min(Distance(start, end) * 0.5, Math.Max(24, double.IsFinite(offset) ? Math.Abs(offset) : 50));

    private static int FindLane(IZipTie tie, IConnector connector)
    {
        if (connector.Parent is IZipTieLaneProvider lanes)
        {
            var lane = lanes.GetZipTieLane(connector);
            if ((uint)lane < (uint)tie.Connectors.Count && ReferenceEquals(tie.Connectors[lane], connector))
            {
                return lane;
            }
        }

        for (var i = 0; i < tie.Connectors.Count; i++)
        {
            if (ReferenceEquals(tie.Connectors[i], connector))
            {
                return i;
            }
        }

        return -1;
    }

    private static double GetSpacing(IZipTie tie)
        => double.IsFinite(tie.Spacing) && tie.Spacing > 0 ? Math.Min(tie.Spacing, 1024) : 6;

    private static double SafeCoordinate(double coordinate)
        => double.IsFinite(coordinate) ? Math.Clamp(coordinate, -100000000, 100000000) : 0;

    private static bool IsFinite(Point point) => double.IsFinite(point.X) && double.IsFinite(point.Y);

    private static double Distance(Point first, Point second) => ((Vector)(second - first)).Length;

    private static Vector Normalize(Vector vector, Vector fallback)
    {
        var length = vector.Length;
        return length > Epsilon ? vector / length : fallback;
    }

    private static double DistanceToSegment(Point point, Point start, Point end)
    {
        var delta = end - start;
        var lengthSquared = delta.X * delta.X + delta.Y * delta.Y;
        if (lengthSquared <= Epsilon)
        {
            return Distance(point, start);
        }

        var relative = point - start;
        var fraction = Math.Clamp((relative.X * delta.X + relative.Y * delta.Y) / lengthSquared, 0, 1);
        return Distance(point, start + delta * fraction);
    }
}

internal readonly record struct ZipTieBand(Point Start, Point End);
