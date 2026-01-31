using System;
using System.Collections.Generic;
using Avalonia;
using NodeEditor.Model;

namespace NodeEditor;

internal static class OrthogonalRouter
{
    private const int DefaultMaxCells = 200;

    private readonly struct GridCell : IEquatable<GridCell>
    {
        public int X { get; }
        public int Y { get; }

        public GridCell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridCell other) => X == other.X && Y == other.Y;

        public override bool Equals(object? obj) => obj is GridCell other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }
    }

    private readonly struct SearchState : IEquatable<SearchState>
    {
        public GridCell Cell { get; }
        public sbyte Direction { get; }

        public SearchState(GridCell cell, sbyte direction)
        {
            Cell = cell;
            Direction = direction;
        }

        public bool Equals(SearchState other) => Cell.Equals(other.Cell) && Direction == other.Direction;

        public override bool Equals(object? obj) => obj is SearchState other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Cell.GetHashCode() * 397) ^ Direction;
            }
        }
    }

    private readonly struct RouteDirection
    {
        public int Dx { get; }
        public int Dy { get; }
        public double Cost { get; }

        public RouteDirection(int dx, int dy, double cost)
        {
            Dx = dx;
            Dy = dy;
            Cost = cost;
        }

        public bool IsDiagonal => Dx != 0 && Dy != 0;
    }

    private readonly struct OpenNode
    {
        public SearchState State { get; }
        public double G { get; }
        public double F { get; }

        public OpenNode(SearchState state, double g, double f)
        {
            State = state;
            G = g;
            F = f;
        }
    }

    private sealed class MinHeap
    {
        private readonly List<OpenNode> _items = new();

        public int Count => _items.Count;

        public void Enqueue(OpenNode node)
        {
            _items.Add(node);
            SiftUp(_items.Count - 1);
        }

        public OpenNode Dequeue()
        {
            var root = _items[0];
            var last = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);
            if (_items.Count > 0)
            {
                _items[0] = last;
                SiftDown(0);
            }

            return root;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                var parent = (index - 1) / 2;
                if (!IsHigherPriority(_items[index], _items[parent]))
                {
                    break;
                }

                (_items[index], _items[parent]) = (_items[parent], _items[index]);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            var count = _items.Count;
            while (true)
            {
                var left = index * 2 + 1;
                var right = left + 1;
                var smallest = index;

                if (left < count && IsHigherPriority(_items[left], _items[smallest]))
                {
                    smallest = left;
                }

                if (right < count && IsHigherPriority(_items[right], _items[smallest]))
                {
                    smallest = right;
                }

                if (smallest == index)
                {
                    break;
                }

                (_items[index], _items[smallest]) = (_items[smallest], _items[index]);
                index = smallest;
            }
        }

        private static bool IsHigherPriority(OpenNode candidate, OpenNode current)
        {
            if (candidate.F < current.F)
            {
                return true;
            }

            if (Math.Abs(candidate.F - current.F) < 0.0001 && candidate.G < current.G)
            {
                return true;
            }

            return false;
        }
    }

    public static bool TryRoute(IConnector connector, Point start, Point end, out List<Point> points)
    {
        return TryRoute(connector, start, end, allowDiagonal: false, preferDirect: false, out points);
    }

    public static bool TryRoute(
        IConnector connector,
        Point start,
        Point end,
        bool allowDiagonal,
        bool preferDirect,
        out List<Point> points)
    {
        points = new List<Point>();

        if (connector.Parent?.Nodes is not { Count: > 0 } nodes)
        {
            return false;
        }

        var settings = connector.Parent.Settings;
        if (!settings.EnableConnectorRouting)
        {
            return false;
        }

        var grid = ResolveGridSize(settings);
        if (grid <= 0.0)
        {
            return false;
        }

        var padding = Math.Max(0.0, settings.RoutingObstaclePadding);
        var obstacles = BuildObstacles(nodes, connector, padding);

        if (preferDirect && IsLineClear(start, end, obstacles))
        {
            points.Add(start);
            points.Add(end);
            return true;
        }

        var bounds = ComputeBounds(start, end, obstacles, grid);

        var originX = Math.Floor(bounds.Left / grid) * grid;
        var originY = Math.Floor(bounds.Top / grid) * grid;
        var cols = (int)Math.Ceiling((bounds.Right - originX) / grid) + 1;
        var rows = (int)Math.Ceiling((bounds.Bottom - originY) / grid) + 1;
        var maxCells = settings.RoutingMaxCells > 0 ? settings.RoutingMaxCells : DefaultMaxCells;

        if (cols <= 1 || rows <= 1 || cols > maxCells || rows > maxCells)
        {
            return false;
        }

        var startCell = ToCell(start, originX, originY, grid);
        var endCell = ToCell(end, originX, originY, grid);

        var blocked = BuildBlockedCells(obstacles, originX, originY, cols, rows, grid);
        blocked.Remove(startCell);
        blocked.Remove(endCell);

        var bendPenalty = Math.Max(0.0, settings.RoutingBendPenalty);
        var diagonalCost = Math.Max(1.0, settings.RoutingDiagonalCost);

        if (!TryFindPath(startCell, endCell, blocked, cols, rows, allowDiagonal, diagonalCost, bendPenalty, out var cellPath))
        {
            return false;
        }

        points = new List<Point>(cellPath.Count);
        foreach (var cell in cellPath)
        {
            points.Add(new Point(originX + cell.X * grid, originY + cell.Y * grid));
        }

        if (points.Count == 0)
        {
            return false;
        }

        points[0] = start;
        points[points.Count - 1] = end;
        Simplify(points);

        return points.Count > 1;
    }

    private static double ResolveGridSize(IDrawingNodeSettings settings)
    {
        if (settings.RoutingGridSize > 0.001)
        {
            return settings.RoutingGridSize;
        }

        if (settings.EnableSnap)
        {
            var snap = Math.Max(1.0, Math.Min(settings.SnapX, settings.SnapY));
            return snap;
        }

        return 10.0;
    }

    private static List<Rect> BuildObstacles(IList<INode> nodes, IConnector connector, double padding)
    {
        var obstacles = new List<Rect>();
        var startNode = connector.Start?.Parent;
        var endNode = connector.End?.Parent;

        foreach (var node in nodes)
        {
            if (!node.IsVisible)
            {
                continue;
            }

            if (ReferenceEquals(node, startNode) || ReferenceEquals(node, endNode))
            {
                continue;
            }

            var rect = new Rect(
                node.X - padding,
                node.Y - padding,
                node.Width + padding + padding,
                node.Height + padding + padding);
            obstacles.Add(rect);
        }

        return obstacles;
    }

    private static Rect ComputeBounds(Point start, Point end, IReadOnlyList<Rect> obstacles, double grid)
    {
        var minX = Math.Min(start.X, end.X);
        var minY = Math.Min(start.Y, end.Y);
        var maxX = Math.Max(start.X, end.X);
        var maxY = Math.Max(start.Y, end.Y);

        foreach (var rect in obstacles)
        {
            minX = Math.Min(minX, rect.Left);
            minY = Math.Min(minY, rect.Top);
            maxX = Math.Max(maxX, rect.Right);
            maxY = Math.Max(maxY, rect.Bottom);
        }

        var margin = Math.Max(grid * 4.0, 20.0);
        minX -= margin;
        minY -= margin;
        maxX += margin;
        maxY += margin;

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private static GridCell ToCell(Point point, double originX, double originY, double grid)
    {
        var x = (int)Math.Round((point.X - originX) / grid);
        var y = (int)Math.Round((point.Y - originY) / grid);
        return new GridCell(x, y);
    }

    private static HashSet<GridCell> BuildBlockedCells(
        IReadOnlyList<Rect> obstacles,
        double originX,
        double originY,
        int cols,
        int rows,
        double grid)
    {
        var blocked = new HashSet<GridCell>();

        foreach (var rect in obstacles)
        {
            var minX = (int)Math.Floor((rect.Left - originX) / grid);
            var maxX = (int)Math.Ceiling((rect.Right - originX) / grid);
            var minY = (int)Math.Floor((rect.Top - originY) / grid);
            var maxY = (int)Math.Ceiling((rect.Bottom - originY) / grid);

            minX = Clamp(minX, 0, cols - 1);
            maxX = Clamp(maxX, 0, cols - 1);
            minY = Clamp(minY, 0, rows - 1);
            maxY = Clamp(maxY, 0, rows - 1);

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    blocked.Add(new GridCell(x, y));
                }
            }
        }

        return blocked;
    }

    private static bool TryFindPath(
        GridCell start,
        GridCell goal,
        HashSet<GridCell> blocked,
        int cols,
        int rows,
        bool allowDiagonal,
        double diagonalCost,
        double bendPenalty,
        out List<GridCell> path)
    {
        path = new List<GridCell>();

        var directions = GetDirections(allowDiagonal, diagonalCost);
        var open = new MinHeap();
        var cameFrom = new Dictionary<SearchState, SearchState>();
        var gScore = new Dictionary<SearchState, double>();

        var startState = new SearchState(start, -1);
        gScore[startState] = 0.0;
        open.Enqueue(new OpenNode(startState, 0.0, Heuristic(start, goal, allowDiagonal, diagonalCost)));

        SearchState? found = null;

        while (open.Count > 0)
        {
            var current = open.Dequeue();

            if (!gScore.TryGetValue(current.State, out var currentG) || currentG < current.G - 0.0001)
            {
                continue;
            }

            if (current.State.Cell.Equals(goal))
            {
                found = current.State;
                break;
            }

            for (var i = 0; i < directions.Length; i++)
            {
                var direction = directions[i];
                var nextCell = new GridCell(current.State.Cell.X + direction.Dx, current.State.Cell.Y + direction.Dy);

                if (nextCell.X < 0 || nextCell.X >= cols || nextCell.Y < 0 || nextCell.Y >= rows)
                {
                    continue;
                }

                if (blocked.Contains(nextCell))
                {
                    continue;
                }

                if (direction.IsDiagonal && IsDiagonalBlocked(current.State.Cell, direction, blocked))
                {
                    continue;
                }

                var nextState = new SearchState(nextCell, (sbyte)i);
                var nextG = currentG + direction.Cost;

                if (current.State.Direction >= 0 && current.State.Direction != i)
                {
                    nextG += bendPenalty;
                }

                if (gScore.TryGetValue(nextState, out var knownG) && nextG >= knownG)
                {
                    continue;
                }

                cameFrom[nextState] = current.State;
                gScore[nextState] = nextG;

                var f = nextG + Heuristic(nextCell, goal, allowDiagonal, diagonalCost);
                open.Enqueue(new OpenNode(nextState, nextG, f));
            }
        }

        if (found is null)
        {
            return false;
        }

        var cursor = found.Value;
        path.Add(cursor.Cell);
        while (cameFrom.TryGetValue(cursor, out var previous))
        {
            cursor = previous;
            path.Add(cursor.Cell);
        }

        path.Reverse();
        return true;
    }

    private static bool IsDiagonalBlocked(GridCell current, RouteDirection direction, HashSet<GridCell> blocked)
    {
        var neighborX = new GridCell(current.X + direction.Dx, current.Y);
        var neighborY = new GridCell(current.X, current.Y + direction.Dy);
        return blocked.Contains(neighborX) || blocked.Contains(neighborY);
    }

    private static RouteDirection[] GetDirections(bool allowDiagonal, double diagonalCost)
    {
        var orthogonal = new[]
        {
            new RouteDirection(-1, 0, 1.0),
            new RouteDirection(1, 0, 1.0),
            new RouteDirection(0, -1, 1.0),
            new RouteDirection(0, 1, 1.0)
        };

        if (!allowDiagonal)
        {
            return orthogonal;
        }

        return new[]
        {
            new RouteDirection(-1, 0, 1.0),
            new RouteDirection(1, 0, 1.0),
            new RouteDirection(0, -1, 1.0),
            new RouteDirection(0, 1, 1.0),
            new RouteDirection(-1, -1, diagonalCost),
            new RouteDirection(1, -1, diagonalCost),
            new RouteDirection(-1, 1, diagonalCost),
            new RouteDirection(1, 1, diagonalCost)
        };
    }

    private static double Heuristic(GridCell cell, GridCell goal, bool allowDiagonal, double diagonalCost)
    {
        var dx = Math.Abs(cell.X - goal.X);
        var dy = Math.Abs(cell.Y - goal.Y);

        if (!allowDiagonal)
        {
            return dx + dy;
        }

        var min = Math.Min(dx, dy);
        var max = Math.Max(dx, dy);
        return diagonalCost * min + (max - min);
    }

    private static void Simplify(List<Point> points)
    {
        if (points.Count < 3)
        {
            return;
        }

        var simplified = new List<Point>(points.Count) { points[0] };

        for (var i = 1; i < points.Count - 1; i++)
        {
            var prev = simplified[simplified.Count - 1];
            var current = points[i];
            var next = points[i + 1];

            if (IsColinear(prev, current, next))
            {
                continue;
            }

            if (Distance(prev, current) < 0.001)
            {
                continue;
            }

            simplified.Add(current);
        }

        simplified.Add(points[points.Count - 1]);
        points.Clear();
        points.AddRange(simplified);
    }

    private static bool IsColinear(Point a, Point b, Point c)
    {
        var dx1 = b.X - a.X;
        var dy1 = b.Y - a.Y;
        var dx2 = c.X - b.X;
        var dy2 = c.Y - b.Y;
        return Math.Abs(dx1 * dy2 - dy1 * dx2) < 0.001;
    }

    private static double Distance(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static bool IsLineClear(Point start, Point end, IReadOnlyList<Rect> obstacles)
    {
        foreach (var rect in obstacles)
        {
            if (SegmentIntersectsRect(start, end, rect))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SegmentIntersectsRect(Point a, Point b, Rect rect)
    {
        if (rect.Contains(a) || rect.Contains(b))
        {
            return true;
        }

        var minX = Math.Min(a.X, b.X);
        var maxX = Math.Max(a.X, b.X);
        var minY = Math.Min(a.Y, b.Y);
        var maxY = Math.Max(a.Y, b.Y);
        var segmentBounds = new Rect(new Point(minX, minY), new Point(maxX, maxY));

        if (!rect.Intersects(segmentBounds))
        {
            return false;
        }

        var topLeft = new Point(rect.Left, rect.Top);
        var topRight = new Point(rect.Right, rect.Top);
        var bottomLeft = new Point(rect.Left, rect.Bottom);
        var bottomRight = new Point(rect.Right, rect.Bottom);

        return SegmentsIntersect(a, b, topLeft, topRight)
               || SegmentsIntersect(a, b, topRight, bottomRight)
               || SegmentsIntersect(a, b, bottomRight, bottomLeft)
               || SegmentsIntersect(a, b, bottomLeft, topLeft);
    }

    private static bool SegmentsIntersect(Point a1, Point a2, Point b1, Point b2)
    {
        var d1 = Cross(a1, a2, b1);
        var d2 = Cross(a1, a2, b2);
        var d3 = Cross(b1, b2, a1);
        var d4 = Cross(b1, b2, a2);

        if (HasOppositeSigns(d1, d2) && HasOppositeSigns(d3, d4))
        {
            return true;
        }

        if (Math.Abs(d1) < 0.0001 && OnSegment(a1, a2, b1))
        {
            return true;
        }

        if (Math.Abs(d2) < 0.0001 && OnSegment(a1, a2, b2))
        {
            return true;
        }

        if (Math.Abs(d3) < 0.0001 && OnSegment(b1, b2, a1))
        {
            return true;
        }

        return Math.Abs(d4) < 0.0001 && OnSegment(b1, b2, a2);
    }

    private static bool HasOppositeSigns(double a, double b)
    {
        return (a > 0 && b < 0) || (a < 0 && b > 0);
    }

    private static double Cross(Point a, Point b, Point c)
    {
        return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
    }

    private static bool OnSegment(Point a, Point b, Point p)
    {
        return p.X >= Math.Min(a.X, b.X) - 0.0001
               && p.X <= Math.Max(a.X, b.X) + 0.0001
               && p.Y >= Math.Min(a.Y, b.Y) - 0.0001
               && p.Y <= Math.Max(a.Y, b.Y) + 0.0001;
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
