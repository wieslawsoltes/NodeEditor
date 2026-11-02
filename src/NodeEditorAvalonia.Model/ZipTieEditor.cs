using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NodeEditor.Model;

/// <summary>
/// Framework-independent editing rules for spatial wire bundles. The caller owns
/// history transactions and supplies its own bundle implementation.
/// </summary>
public sealed class ZipTieEditor
{
    private readonly struct LaneMember
    {
        public LaneMember(IConnector connector, double position, int index)
        {
            Connector = connector;
            Position = position;
            Index = index;
        }

        public IConnector Connector { get; }
        public double Position { get; }
        public int Index { get; }
    }

    private readonly IDrawingNode _drawing;
    private readonly IZipTieHost _host;
    private readonly Func<IZipTie> _factory;

    /// <summary>Creates an editor for a drawing and its optional zip-tie capability.</summary>
    public ZipTieEditor(IDrawingNode drawing, IZipTieHost host, Func<IZipTie> factory)
    {
        _drawing = drawing ?? throw new ArgumentNullException(nameof(drawing));
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Creates a bundle from eligible wires. Ineligible, duplicate, or already
    /// bundled wires are ignored; at least two eligible wires are required.
    /// </summary>
    public IZipTie? Create(IEnumerable<IConnector> connectors)
    {
        if (connectors is null)
        {
            throw new ArgumentNullException(nameof(connectors));
        }

        var members = new List<IConnector>();
        var seen = new HashSet<IConnector>();
        var available = _drawing.Connectors is not null
            ? new HashSet<IConnector>(_drawing.Connectors)
            : new HashSet<IConnector>();
        foreach (var connector in connectors)
        {
            if (connector is not null && seen.Add(connector) && CanAdd(connector, available))
            {
                members.Add(connector);
            }
        }

        if (members.Count < 2)
        {
            return null;
        }

        GetDefaultSpan(members, out var start, out var end, out var horizontal);
        // Cache projection keys once and preserve original order on ties. Sorting
        // is O(n log n); pin transforms are never repeated inside the comparer.
        var ordered = new LaneMember[members.Count];
        for (var i = 0; i < members.Count; i++)
        {
            ordered[i] = new LaneMember(members[i], GetLaneKey(members[i], horizontal), i);
        }

        Array.Sort(ordered, (left, right) =>
        {
            var result = left.Position.CompareTo(right.Position);
            return result != 0 ? result : left.Index.CompareTo(right.Index);
        });

        var tie = _factory();
        tie.Start = start;
        tie.End = end;
        tie.Name = "Wire bundle";
        var sortedMembers = new IConnector[ordered.Length];
        for (var i = 0; i < ordered.Length; i++)
        {
            sortedMembers[i] = ordered[i].Connector;
        }

        tie.Connectors = new ObservableCollection<IConnector>(sortedMembers);

        _host.ZipTies.Add(tie);
        _host.SelectedZipTie = tie;
        return tie;
    }

    /// <summary>Removes an editable bundle while preserving every wire.</summary>
    public void Remove(IZipTie tie)
    {
        if (!CanEdit(tie))
        {
            return;
        }

        _host.ZipTies.Remove(tie);
        if (ReferenceEquals(_host.SelectedZipTie, tie))
        {
            _host.SelectedZipTie = null;
        }
    }

    /// <summary>Appends an eligible wire to a bundle without reordering its existing lanes.</summary>
    public bool Add(IZipTie tie, IConnector connector)
    {
        if (!CanEdit(tie) || !CanAdd(connector))
        {
            return false;
        }

        tie.Connectors.Add(connector);
        return true;
    }

    /// <summary>Adds eligible wires in input order and publishes a single membership change.</summary>
    public int AddRange(IZipTie tie, IEnumerable<IConnector> connectors)
    {
        if (connectors is null)
        {
            throw new ArgumentNullException(nameof(connectors));
        }

        if (!CanEdit(tie))
        {
            return 0;
        }

        var available = _drawing.Connectors is not null
            ? new HashSet<IConnector>(_drawing.Connectors)
            : new HashSet<IConnector>();
        var members = new List<IConnector>(tie.Connectors);
        var seen = new HashSet<IConnector>(members);
        foreach (var connector in connectors)
        {
            if (connector is not null && seen.Add(connector) && CanAdd(connector, available))
            {
                members.Add(connector);
            }
        }

        var added = members.Count - tie.Connectors.Count;
        if (added > 0)
        {
            tie.Connectors = new ObservableCollection<IConnector>(members);
        }

        return added;
    }

    /// <summary>Detaches a wire, automatically dissolving a bundle with fewer than two members.</summary>
    public bool Detach(IZipTie tie, IConnector connector)
    {
        if (!CanEdit(tie) || connector is null || connector.IsLocked || !tie.Connectors.Remove(connector))
        {
            return false;
        }

        if (tie.Connectors.Count < 2)
        {
            Remove(tie);
        }

        return true;
    }

    /// <summary>Detaches eligible wires atomically, dissolving a bundle with fewer than two remaining members.</summary>
    public int DetachRange(IZipTie tie, IEnumerable<IConnector> connectors)
    {
        if (connectors is null)
        {
            throw new ArgumentNullException(nameof(connectors));
        }

        if (!CanEdit(tie))
        {
            return 0;
        }

        var selected = new HashSet<IConnector>(connectors);
        var retained = new List<IConnector>(tie.Connectors.Count);
        foreach (var connector in tie.Connectors)
        {
            if (connector.IsLocked || !selected.Contains(connector))
            {
                retained.Add(connector);
            }
        }

        var removed = tie.Connectors.Count - retained.Count;
        if (removed > 0)
        {
            tie.Connectors = new ObservableCollection<IConnector>(retained);
            if (retained.Count < 2)
            {
                Remove(tie);
            }
        }

        return removed;
    }

    /// <summary>Reverses lane order without changing direction, pins, or individual routing settings.</summary>
    public void Reverse(IZipTie tie)
    {
        if (!CanEdit(tie))
        {
            return;
        }

        var reversed = new ObservableCollection<IConnector>();
        for (var i = tie.Connectors.Count - 1; i >= 0; i--)
        {
            reversed.Add(tie.Connectors[i]);
        }

        // Publish one atomic membership update, avoiding intermediate duplicate
        // lanes and repeated render/index invalidations during a reversal.
        tie.Connectors = reversed;
    }

    /// <summary>Clears intermediate anchors while preserving the end anchors.</summary>
    public void Straighten(IZipTie tie)
    {
        if (CanEdit(tie))
        {
            tie.Waypoints = new ObservableCollection<ConnectorPoint>();
        }
    }

    private bool CanEdit(IZipTie? tie)
    {
        return tie is not null && tie.IsVisible && !tie.IsLocked && _host.ZipTies.Contains(tie);
    }

    private bool CanAdd(IConnector? connector, ISet<IConnector>? available = null)
    {
        return connector is not null
            && connector.IsVisible && !connector.IsLocked
            && (available?.Contains(connector)
                ?? (_drawing.Connectors is not null && _drawing.Connectors.Contains(connector)))
            && connector.Start?.Parent is { IsVisible: true } && connector.End?.Parent is { IsVisible: true }
            && TryGetPoint(connector.Start, out _, out _) && TryGetPoint(connector.End, out _, out _)
            && _host.FindZipTie(connector) is null;
    }

    private static double GetLaneKey(IConnector connector, bool horizontal)
    {
        TryGetPoint(connector.Start!, out var sx, out var sy);
        TryGetPoint(connector.End!, out var ex, out var ey);
        return horizontal ? sy / 2 + ey / 2 : sx / 2 + ex / 2;
    }

    private static void GetDefaultSpan(IList<IConnector> members, out ConnectorPoint start, out ConnectorPoint end, out bool horizontal)
    {
        // The first wire determines the bundle's forward direction. Reverse
        // connections are normalized for layout only; their pins are untouched.
        TryGetPoint(members[0].Start!, out var firstStartX, out var firstStartY);
        TryGetPoint(members[0].End!, out var firstEndX, out var firstEndY);
        horizontal = Math.Abs(firstEndX - firstStartX) >= Math.Abs(firstEndY - firstStartY);
        var direction = horizontal ? firstEndX - firstStartX : firstEndY - firstStartY;
        var sx = 0.0;
        var sy = 0.0;
        var ex = 0.0;
        var ey = 0.0;
        for (var i = 0; i < members.Count; i++)
        {
            TryGetPoint(members[i].Start!, out var ax, out var ay);
            TryGetPoint(members[i].End!, out var bx, out var by);
            var memberDirection = horizontal ? bx - ax : by - ay;
            if (Math.Sign(memberDirection) != Math.Sign(direction))
            {
                (ax, bx) = (bx, ax);
                (ay, by) = (by, ay);
            }

            // Incremental mean avoids overflowing a sum for large drawings.
            var count = i + 1.0;
            var previousWeight = i / count;
            sx = sx * previousWeight + ax / count;
            sy = sy * previousWeight + ay / count;
            ex = ex * previousWeight + bx / count;
            ey = ey * previousWeight + by / count;
        }

        if (horizontal)
        {
            var y = sy / 2 + ey / 2;
            start = new ConnectorPoint(sx * 0.75 + ex * 0.25, y);
            end = new ConnectorPoint(sx * 0.25 + ex * 0.75, y);
        }
        else
        {
            var x = sx / 2 + ex / 2;
            start = new ConnectorPoint(x, sy * 0.75 + ey * 0.25);
            end = new ConnectorPoint(x, sy * 0.25 + ey * 0.75);
        }

        // Coincident pins still get a usable, draggable span.
        if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 1.0)
        {
            if (horizontal)
            {
                start.X -= 24;
                end.X += 24;
            }
            else
            {
                start.Y -= 24;
                end.Y += 24;
            }
        }
    }

    private static bool TryGetPoint(IPin pin, out double x, out double y)
    {
        x = pin.X;
        y = pin.Y;
        if (pin.Parent is { } node)
        {
            var angle = node.Rotation * Math.PI / 180.0;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            var centerX = node.Width / 2;
            var centerY = node.Height / 2;
            var px = x - centerX;
            var py = y - centerY;
            x = px * cos - py * sin + centerX + node.X;
            y = px * sin + py * cos + centerY + node.Y;
        }

        return IsFinite(x) && IsFinite(y);
    }

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
